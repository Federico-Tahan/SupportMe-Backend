using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using System.Net;
using Amazon.S3.Transfer;
using System.Text.RegularExpressions;
using BaseProject.DTOs.FileUploadDTOs;

namespace BaseProject.Services
{
    public class FileUploadService
    {
        private readonly AmazonS3Client _s3Client;
        public FileUploadService(AmazonS3Client s3Client) => _s3Client = s3Client;
        private readonly S3BucketConfig _bucketConfig;
        public FileUploadService(S3BucketConfig bucketConfig)
        {
            _bucketConfig = bucketConfig;
        }
        public async Task<string> CreateBucketIfNeeded(string bucketName)
        {

            if (await AmazonS3Util.DoesS3BucketExistV2Async(_s3Client, bucketName))
                return $"Bucket {bucketName} already exists.";

                var putBucketRequest = new PutBucketRequest
                {
                    BucketName = bucketName,
                    UseClientRegion = true
                };

                var putBucketResponse = await _s3Client.PutBucketAsync(putBucketRequest);

                return putBucketResponse.HttpStatusCode == HttpStatusCode.OK
                ? $"Bucket {bucketName} created."
                : $"Failed to create bucket, {putBucketResponse.HttpStatusCode} code returned";
        }
        public async Task<string> Upload(string bucketName, string cdnUrl, string data, CancellationToken token = default, bool returnUrl = true, bool optimize = true)
        {
                string fileName = Guid.NewGuid().ToString();
                if (IsUrl(data))
                    return data;

                var bucketCreatedResponse = await CreateBucketIfNeeded(bucketName);

                if (bucketCreatedResponse.Contains(bucketName))
                {
                    var cleanData = Regex.Replace(data, @"^data:image\/[a-zA-Z]+;base64,", string.Empty);

                    var bytes = Convert.FromBase64String(cleanData);
                    var fullPath = $"{cdnUrl}/{fileName}";

                    using var transferUtility = new TransferUtility(_s3Client);

                    await using var ms = new MemoryStream(bytes);

                    await transferUtility.UploadAsync(ms,bucketName, fileName, token);
                    if (returnUrl)
                        return $"{fullPath}";
                    else
                        return fileName;
                }
                return fileName;
        }
        public static bool IsUrl(string data)
        {
            return Uri.TryCreate(data, UriKind.Absolute, out var uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }
            
    }
}
