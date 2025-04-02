using AutoMapper;
using SupportMe.Data;
using SupportMe.DTOs.CampaignDTOs;
using SupportMe.DTOs.FileUploadDTOs;

namespace SupportMe.Services
{
    public class CampaignService
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        private readonly FileUploadService _fileUploadService;
        private readonly S3BucketConfig _S3BucketConfig;
        public CampaignService(DataContext context, IMapper mapper, FileUploadService fileUpload, S3BucketConfig s3)
        {
            _context = context;
            _mapper = mapper;
            _fileUploadService = fileUpload;
            _S3BucketConfig = s3;
        }


        public async Task<string> CreateCampaign(CampaignWriteDTO request) 
        {
            var url = await _fileUploadService.ProcessImageUrl(_S3BucketConfig.Bucket, _S3BucketConfig.CdnUrl, request.MainImage, resizeToMultipleSizes: false);
            return url;
        }
    }
}
