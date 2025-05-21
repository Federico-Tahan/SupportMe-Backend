using Minio;
using Minio.Exceptions;
using Minio.DataModel.Args;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Minio.ApiEndpoints;
using SupportMe.DTOs.MercadoPagoDTOs;

namespace SupportMe.Services
{
    public class MinioFileUploadService
    {
        private readonly IMinioClient _minioClient;
        private readonly string _endpoint;
        private readonly string _bucketName;
        private readonly IConfiguration _configuration;

        public MinioFileUploadService(string endpoint, string accessKey, string secretKey, IConfiguration configuration)
        {
            _endpoint = endpoint;
            _minioClient = new MinioClient()
                .WithEndpoint(endpoint)
                .WithCredentials(accessKey, secretKey)
                .Build();
            _configuration = configuration;
            _bucketName = _configuration.GetValue<string>("MINIO_BUCKET");
        }

        public async Task<string> CreateBucketIfNeeded(string bucketName)
        {
            bool exists = await _minioClient.BucketExistsAsync(new BucketExistsArgs().WithBucket(bucketName));
            if (exists)
                return $"Bucket {bucketName} already exists.";

            await _minioClient.MakeBucketAsync(new MakeBucketArgs().WithBucket(bucketName));
            return $"Bucket {bucketName} created.";
        }

        private static bool IsUrl(string data)
        {
            return Uri.TryCreate(data, UriKind.Absolute, out var uriResult) &&
                   (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }

        private static string GetFileExtensionFromBase64(string base64Data)
        {
            var match = Regex.Match(base64Data, @"^data:(image|video)\/(\w+);base64,");
            if (match.Success)
            {
                return match.Groups[2].Value;
            }
            return null;
        }

        private static string CleanBase64Data(string base64Data)
        {
            return Regex.Replace(base64Data, @"^data:(image|video)\/[\w\+]+;base64,", string.Empty);
        }

        public async Task<string> Upload(string bucketName, string cdnUrl, string data, CancellationToken token = default, bool returnUrl = true)
        {
            if (IsUrl(data))
                return data;

            var bucketResponse = await CreateBucketIfNeeded(bucketName);
            if (!bucketResponse.Contains(bucketName))
                throw new Exception($"Bucket creation failed or not accessible: {bucketResponse}");

            string extension = GetFileExtensionFromBase64(data) ?? "bin";
            string fileName = Guid.NewGuid().ToString() + "." + extension;

            var cleanedData = CleanBase64Data(data);
            var bytes = Convert.FromBase64String(cleanedData);

            using var ms = new MemoryStream(bytes);

            await _minioClient.PutObjectAsync(new PutObjectArgs()
                .WithBucket(bucketName)
                .WithObject(fileName)
                .WithStreamData(ms)
                .WithObjectSize(ms.Length), token);

            if (returnUrl)
                return $"{cdnUrl}/{fileName}";
            else
                return fileName;
        }

        public async Task<string> Upload(string id, string bucketName, string cdnUrl, string data, CancellationToken token = default, bool returnUrl = true)
        {
            if (IsUrl(data))
                return data;

            var bucketResponse = await CreateBucketIfNeeded(bucketName);
            if (!bucketResponse.Contains(bucketName))
                throw new Exception($"Bucket creation failed or not accessible: {bucketResponse}");

            string extension = GetFileExtensionFromBase64(data) ?? "bin";
            string fileName = id + "." + extension;

            var cleanedData = CleanBase64Data(data);
            var bytes = Convert.FromBase64String(cleanedData);

            using var ms = new MemoryStream(bytes);

            await _minioClient.PutObjectAsync(new PutObjectArgs()
                .WithBucket(bucketName)
                .WithObject(fileName)
                .WithStreamData(ms)
                .WithObjectSize(ms.Length), token);

            if (returnUrl)
                return $"{cdnUrl}/{fileName}";
            else
                return fileName;
        }

        public async Task RemoveFileAsync(string bucketName, string fileName, ILogger logger = null, string message = null)
        {
            try
            {
                await _minioClient.RemoveObjectAsync(new RemoveObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(fileName));
                if (logger != null && !string.IsNullOrEmpty(message))
                    logger.LogInformation(message);
            }
            catch (Exception ex)
            {
                if (logger != null)
                    logger.LogError(ex, $"Error deleting file {fileName} in bucket {bucketName}");
                throw;
            }
        }

        public async Task<List<string>> ListObjectsAsync(string bucketName, bool recursive = true)
        {
            var objects = new List<string>();

            var args = new ListObjectsArgs()
                .WithBucket(bucketName)
                .WithRecursive(recursive);

            var observable = _minioClient.ListObjectsAsync(args);
            var tcs = new TaskCompletionSource<bool>();

            var subscription = observable.Subscribe(
                item => objects.Add(item.Key),
                ex => tcs.TrySetException(ex),
                () => tcs.TrySetResult(true));

            await tcs.Task;

            return objects;
        }

        public async Task<string> UploadImageToMinioAsync(string base64Image, string fileName)
        {
            var cleanData = string.Empty;


            cleanData = Regex.Replace(base64Image, @"^data:image\/[a-zA-Z]+;base64,", string.Empty);
            
            byte[] imageBytes = Convert.FromBase64String(cleanData);
            using var stream = new MemoryStream(imageBytes);

            bool found = await _minioClient.BucketExistsAsync(new BucketExistsArgs().WithBucket(_bucketName));
            if (!found)
            {
                await _minioClient.MakeBucketAsync(new MakeBucketArgs().WithBucket(_bucketName));
            }

            await _minioClient.PutObjectAsync(new PutObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(fileName)
                .WithStreamData(stream)
                .WithObjectSize(stream.Length)
                .WithContentType("image/jpeg"));
            string publicUrl = $"http://supportme.site:9000/{_bucketName}/{fileName}";
            return publicUrl;
        }

    }
}
