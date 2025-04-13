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
            var campaignQuery = _context.Campaigns.AsQueryable();

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
                                        GoalDate = x.GoalDate,
                                        MainImage = x.MainImage,
                                        Name = x.Name,
                                        Raised = _context.PaymentDetail.Where(c => c.Status == Status.OK && c.CampaignId == x.Id).Select(x => x.NetReceivedAmount).Sum(),
                                        Tags = _context.CampaignTags.Where(c => c.CampaignId == x.Id).Select(x => x.Tag).ToList()
                                    }).ToListAsync();
            response.TotalRegisters = totalRegisters;
            return response;
        }
        public async Task<string> CreateCampaign(CampaignWriteDTO request, string userId) 
        {
            Campaign campaign = new Campaign();
            campaign.CreationDate = DateTime.Now;
            campaign.Name = request.Name;
            campaign.Description = request.Description;
            campaign.GoalDate = request.GoalDate;
            campaign.GoalAmount = request.GoalAmount;
            campaign.UserId = userId;

            var url = !string.IsNullOrWhiteSpace(request.MainImage) && !request.MainImage.IsUrl() &&
                                    ImageHelper.ValidateImageFormat(request.MainImage) ? 
                                    await _fileUploadService.ProcessImageUrl(_S3BucketConfig.Bucket, _S3BucketConfig.CdnUrl, request.MainImage, resizeToMultipleSizes: false) :
                                    request.MainImage;
            campaign.MainImage = url;


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
                    assetsItem.Asset = imageUrl;
                    assets.Add(assetsItem);
                }
                campaign.Assets = assets;
            }

            if (!request.Tags.IsNullOrEmpty())
            {
                List<CampaignTags> campaignTags = new List<CampaignTags>();
                campaignTags.AddRange(request.Tags.Select(x => new CampaignTags { Tag = x.Tag }).ToList());
                campaign.Tags = campaignTags;
            }

            _context.Add(campaign);
            await _context.SaveChangesAsync();
            return url;
        }
    }
}
