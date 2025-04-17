using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SupportMe.Data;
using SupportMe.DTOs;
using SupportMe.DTOs.CampaignDTOs;
using SupportMe.DTOs.FileUploadDTOs;
using SupportMe.Helpers;
using SupportMe.Models;

namespace SupportMe.Services
{
    public class CampaignService
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        private readonly FileUploadService _fileUploadService;
        private const string Colacion = "SQL_Latin1_General_CP1_CI_AI";
        private readonly S3BucketConfig _S3BucketConfig;
        public CampaignService(DataContext context, IMapper mapper, FileUploadService fileUpload, S3BucketConfig s3)
        {
            _context = context;
            _mapper = mapper;
            _fileUploadService = fileUpload;
            _S3BucketConfig = s3;
        }

        public async Task<PaginationDTO<CampaignReadDTO>> GetCampaigns(CampaignFilter filter, string? userId) 
        {
            var campaignQuery = _context.Campaigns
                                        .Where(x => !x.GoalDate.HasValue || x.GoalDate.Value >= DateTime.UtcNow)
                                        .AsQueryable();

            if (!userId.IsNullOrEmpty()) 
            {
                campaignQuery = campaignQuery.Where(x => x.UserId == userId);
            }

            if (filter.CategoryId != null) 
            {
                campaignQuery = campaignQuery.Where(x => x.CategoryId == filter.CategoryId);
            }
            if (!filter.TextFilter.IsNullOrEmpty())
            {
                campaignQuery = campaignQuery.Where(x => EF.Functions.Collate(x.Name, Colacion).Contains(filter.TextFilter));
            }

            var totalRegisters = await campaignQuery.CountAsync();
            campaignQuery = SortingHelper.ApplyMultipleSortingAndPagination(campaignQuery, filter, true);

            var response = new PaginationDTO<CampaignReadDTO>();
            response.Items = await campaignQuery
                                    .Include(x => x.Category)
                                    .Select(x => new CampaignReadDTO 
                                    {
                                        Id = x.Id,
                                        Category = x.Category != null ? x.Category.Name : null,
                                        CreationDate = x.CreationDate,
                                        Description = x.Description,
                                        GoalAmount = x.GoalAmount,
                                        GoalDate = x.GoalDate.HasValue ? DateHelper.GetDateInZoneTime(x.GoalDate.Value, "ARG", 180) : null,
                                        MainImage = x.MainImage,
                                        Name = x.Name,
                                        Raised = _context.PaymentDetail.Where(c => c.Status == Status.OK && c.CampaignId == x.Id).Select(x => x.TotalPaidAmount).Sum(),
                                        Tags = _context.CampaignTags.Where(c => c.CampaignId == x.Id).Select(x => x.Tag).ToList()
                                    }).ToListAsync();
            response.TotalRegisters = totalRegisters;
            return response;
        }

        public async Task<CampaignReadDTO> GetCampaignDonationById(int id)
        {
           
             var campaign = await _context.Campaigns
                                    .Where(x => x.Id == id)
                                    .Include(x => x.Category)
                                    .Select(x => new CampaignReadDTO
                                    {
                                        Id = x.Id,
                                        Category = x.Category != null ? x.Category.Name : null,
                                        CreationDate = x.CreationDate,
                                        Description = x.Description,
                                        GoalAmount = x.GoalAmount,
                                        GoalDate = x.GoalDate,
                                        MainImage = x.MainImage,
                                        Name = x.Name,
                                        CategoryId = x.CategoryId,
                                        Raised = _context.PaymentDetail.Where(c => c.Status == Status.OK && c.CampaignId == x.Id).Select(x => x.Amount).Sum(),
                                        DonationsCount = _context.PaymentDetail.Where(c => c.Status == Status.OK && c.CampaignId == x.Id).Count(),
                                        Tags = _context.CampaignTags.Where(c => c.CampaignId == x.Id).Select(x => x.Tag).ToList(),
                                        Assets = _context.GaleryAssets.Where(c => c.AssetSoruceId == x.Id.ToString() && c.AssetSource == "CAMPAIGN").Select(x => x.Asset).ToList()
                                    }).FirstOrDefaultAsync();
            return campaign;
        }
        public async Task<string> CreateCampaign(CampaignWriteDTO request, string userId) 
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                Campaign campaign = new Campaign();
                campaign.CreationDate = DateTime.Now;
                campaign.Name = request.Name;
                campaign.Description = request.Description;
                if (request.GoalDate.HasValue)
                {
                    campaign.GoalDate = DateHelper.GetUTCDateFromLocalDate(request.GoalDate.Value, "ARG", 180);
                }
                campaign.GoalAmount = request.GoalAmount;
                campaign.UserId = userId;
                campaign.CategoryId = request.CategoryId;
                var url = !string.IsNullOrWhiteSpace(request.MainImage) && !request.MainImage.IsUrl() &&
                                        ImageHelper.ValidateImageFormat(request.MainImage) ?
                                        await _fileUploadService.ProcessImageUrl(_S3BucketConfig.Bucket, _S3BucketConfig.CdnUrl, request.MainImage, resizeToMultipleSizes: false) :
                                        request.MainImage;
                campaign.MainImage = url;

                if (!request.Tags.IsNullOrEmpty())
                {
                    List<CampaignTags> campaignTags = new List<CampaignTags>();
                    campaignTags.AddRange(request.Tags.Select(x => new CampaignTags { Tag = x.Tag }).ToList());
                    campaign.Tags = campaignTags;
                }

                _context.Add(campaign);
                await _context.SaveChangesAsync();


                if (request.Assets?.Count > 0)
                {
                    List<GaleryAssets> assets = new List<GaleryAssets>();
                    foreach (var item in request.Assets)
                    {
                        GaleryAssets assetsItem = new GaleryAssets();
                        var imageUrl = !string.IsNullOrWhiteSpace(item.Base64) && !item.Base64.IsUrl() &&
                                        ImageHelper.ValidateImageFormat(item.Base64) ?
                                        await _fileUploadService.ProcessImageUrl(_S3BucketConfig.Bucket, _S3BucketConfig.CdnUrl, item.Base64, resizeToMultipleSizes: false) :
                                        item.Base64;
                        assetsItem.AssetSoruceId = campaign.Id.ToString();
                        assetsItem.AssetSource = "CAMPAIGN";
                        assetsItem.Asset = imageUrl;

                        assets.Add(assetsItem);
                    }
                    _context.AddRange(assets);
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();

                return url;
            }
            catch (Exception ex) 
            {
                await transaction.RollbackAsync();

                throw ex;
            }
        }


        public async Task<string> UpdateCampaign(CampaignWriteDTO request, string userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var campaignDB = await _context.Campaigns.FirstOrDefaultAsync(x => x.Id == request.Id.Value && x.UserId == userId);
                campaignDB.Name = request.Name;
                campaignDB.Description = request.Description;
                if (request.GoalDate.HasValue)
                {
                    campaignDB.GoalDate = DateHelper.GetUTCDateFromLocalDate(request.GoalDate.Value, "ARG", 180);
                }
                campaignDB.GoalAmount = request.GoalAmount;
                campaignDB.CategoryId = request.CategoryId;
                var url = !string.IsNullOrWhiteSpace(request.MainImage) && !request.MainImage.IsUrl() &&
                                        ImageHelper.ValidateImageFormat(request.MainImage) ?
                                        await _fileUploadService.ProcessImageUrl(_S3BucketConfig.Bucket, _S3BucketConfig.CdnUrl, request.MainImage, resizeToMultipleSizes: false) :
                                        request.MainImage;
                campaignDB.MainImage = url;
                await _context.CampaignTags.Where(x => x.CampaignId == campaignDB.Id).ExecuteDeleteAsync();

                if (!request.Tags.IsNullOrEmpty())
                {
                    List<CampaignTags> campaignTags = new List<CampaignTags>();
                    campaignTags.AddRange(request.Tags.Select(x => new CampaignTags { Tag = x.Tag }).ToList());
                    campaignDB.Tags = campaignTags;
                }

                _context.Update(campaignDB);
                await _context.SaveChangesAsync();

                await _context.GaleryAssets.Where(x => x.AssetSource == "CAMPAIGN" && x.AssetSoruceId == campaignDB.Id.ToString()).ExecuteDeleteAsync();

                if (request.Assets?.Count > 0)
                {
                    List<GaleryAssets> assets = new List<GaleryAssets>();
                    foreach (var item in request.Assets)
                    {
                        GaleryAssets assetsItem = new GaleryAssets();
                        var imageUrl = !string.IsNullOrWhiteSpace(item.Base64) && !item.Base64.IsUrl() &&
                                        ImageHelper.ValidateImageFormat(item.Base64) ?
                                        await _fileUploadService.ProcessImageUrl(_S3BucketConfig.Bucket, _S3BucketConfig.CdnUrl, item.Base64, resizeToMultipleSizes: false) :
                                        item.Base64;
                        assetsItem.AssetSoruceId = campaignDB.Id.ToString();
                        assetsItem.AssetSource = "CAMPAIGN";
                        assetsItem.Asset = imageUrl;

                        assets.Add(assetsItem);
                    }
                    _context.AddRange(assets);
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();

                return url;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                throw ex;
            }


        }
    }
}
