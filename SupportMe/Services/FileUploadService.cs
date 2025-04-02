using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using System.Net;
using Amazon.S3.Transfer;
using System.Text.RegularExpressions;
using SupportMe.DTOs.FileUploadDTOs;
using SupportMe.Helpers;

namespace SupportMe.Services
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
        private async Task<string> Upload(string bucketName, string cdnUrl, string data, CancellationToken token = default, bool returnUrl = true, bool optimize = true)
        {
            string fileName = Guid.NewGuid().ToString() + "." + data.GetFileExtension();
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

                await transferUtility.UploadAsync(ms, bucketName, fileName, token);
                if (returnUrl)
                    return $"{fullPath}";
                else
                    return fileName;
            }
            return fileName;
        }

        private async Task<string> Upload(string id, string bucketName, string cdnUrl, string data, CancellationToken token = default, bool returnUrl = true, bool optimize = true)
        {
            string fileName = id + "." + data.GetFileExtension();
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

                await transferUtility.UploadAsync(ms, bucketName, fileName, token);
                if (returnUrl)
                    return $"{fullPath}";
                else
                    return fileName;
            }
            return fileName;
        }

        /// <summary>
        /// Use the ProcessImageUrl to upload an image.
        /// resize: true will resize the image into multiple formats(see ImageHelper for the sizes).
        /// The thumbnail flag remains false unless the image is specifically a thumbnail.
        /// </summary>
        public async Task<string> ProcessImageUrl(string bucketName, string cdnUrl, string imageUrl, bool isThumbnail = false, bool resizeToMultipleSizes = false)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                return null;
            }
            else if (imageUrl.IsUrl())
            {
                return imageUrl;
            }
            else if (imageUrl.ValidateImageFormat())
            {
                string response = null;
                if (isThumbnail)
                {
                    response = await ProcessImageUrl(bucketName, cdnUrl, imageUrl.ConverToThumbnail());
                }
                else if (!resizeToMultipleSizes)
                {
                    response = await Upload(bucketName, cdnUrl, imageUrl);
                }
                else if (resizeToMultipleSizes)
                {
                    response = await SaveResizeToMultipleSizes(bucketName, cdnUrl, imageUrl);
                }

                return response;
            }
            return imageUrl;
        }

        private async Task<string> SaveResizeToMultipleSizes(string bucketName, string cdnUrl, string image)
        {
            string response = string.Empty;
            var listOfImages = ImageHelper.ResizeToMultipleSizes(image);

            var originalImage = listOfImages.FirstOrDefault(resizedImage => resizedImage.Item2 == ImageSize.ORIGINAL);
            if (originalImage != null)
            {
                listOfImages.Remove(originalImage);
                response = await Upload(bucketName, cdnUrl, originalImage.Item1);
            }
            else
            {
                throw new Exception("Error in url parse of image");
            }

            var match = Regex.Match(response, @"\/([a-fA-F0-9\-]+)(\.(jpg|png|jpeg|gif|bmp|ico|webp))$");

            if (!match.Success)
            {
                throw new Exception("Error in url parse of image");
            }

            var id = match.Groups[1].Value;

            foreach (var (imageBase64, size) in listOfImages)
            {
                var idFormat = id + "_" + size;
                await Upload(idFormat, bucketName, cdnUrl, imageBase64);
            }

            return response;
        }

        public async Task<string> UploadVideo(string bucketName, string cdnUrl, string data, CancellationToken token = default, bool returnUrl = true, bool optimize = true)
        {
            string fileName = Guid.NewGuid().ToString() + "." + data.GetFileExtension();
            if (IsUrl(data))
                return data;

            var bucketCreatedResponse = await CreateBucketIfNeeded(bucketName);

            if (bucketCreatedResponse.Contains(bucketName))
            {
                var cleanData = Regex.Replace(data, @"^data:video\/([\w]+|mp4);base64,", string.Empty);

                var bytes = Convert.FromBase64String(cleanData);
                var fullPath = $"{cdnUrl}/{fileName}";

                using var transferUtility = new TransferUtility(_s3Client);

                await using var ms = new MemoryStream(bytes);

                await transferUtility.UploadAsync(ms, bucketName, fileName, token);

                if (returnUrl)
                    return $"{fullPath}";
                else
                    return fileName;
            }
            return fileName;
        }

        public async Task<string> UploadMedia(string bucketName, string cdnUrl, string data, CancellationToken token = default, bool returnUrl = true, bool optimize = true)
        {
            var extension = data.GetFileExtension();
            string fileName = Guid.NewGuid().ToString() + "." + extension;
            if (IsUrl(data))
                return data;

            var bucketCreatedResponse = await CreateBucketIfNeeded(bucketName);

            if (bucketCreatedResponse.Contains(bucketName))
            {
                var cleanData = string.Empty;

                if (extension == "mp4")
                {
                    cleanData = Regex.Replace(data, @"^data:video\/([\w]+|mp4);base64,", string.Empty);
                }
                else
                {
                    cleanData = Regex.Replace(data, @"^data:image\/[a-zA-Z]+;base64,", string.Empty);
                }

                var bytes = Convert.FromBase64String(cleanData);
                var fullPath = $"{cdnUrl}/{fileName}";

                using var transferUtility = new TransferUtility(_s3Client);

                await using var ms = new MemoryStream(bytes);

                await transferUtility.UploadAsync(ms, bucketName, fileName, token);

                if (returnUrl)
                    return $"{fullPath}";
                else
                    return fileName;
            }
            return fileName;
        }

        public async Task<DeleteObjectResponse> DeleteFile<T>(string bucketName, string filename, ILogger<T> _logger, string message)
        {
            var deleteObjectRequest = new DeleteObjectRequest
            {
                BucketName = bucketName,
                Key = filename
            };
            var result = await _s3Client.DeleteObjectAsync(deleteObjectRequest);
            _logger.LogError(message);
            return result;
        }
        public static bool IsUrl(string data)
        {
            return Uri.TryCreate(data, UriKind.Absolute, out var uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }

    }
}
