using System.Text.RegularExpressions;
using SupportMe.Error;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Webp;

namespace SupportMe.Helpers
{
    public static class ImageHelper
    {
        private static readonly List<(int size, ImageSize image)> Sizes = new()
        {
            (1920, ImageSize.ORIGINAL),
            (800 , ImageSize.MEDIUM),
            (300 , ImageSize.SMALL),
            (160 , ImageSize.THUMBNAIL)
        };

        public static List<Tuple<string, ImageSize>> ResizeToMultipleSizes(this string sourceBase64)
        {
            var match = Regex.Match(sourceBase64, @"^data:image\/(?<ext>[a-zA-Z]+);base64,");
            string extension = match.Success ? match.Groups["ext"].Value.ToLower() : "jpeg";

            var resizedImages = new List<Tuple<string, ImageSize>>();

            byte[] imgBytes = Convert.FromBase64String(
                Regex.Replace(sourceBase64, @"^data:image\/[a-zA-Z]+;base64,", string.Empty)
            );

            foreach (var (targetSize, imageSize) in Sizes)
            {
                using var image = Image.Load(imgBytes);
                var imageLargerSize = Math.Max(image.Width, image.Height);

                if (imageLargerSize <= targetSize)
                {
                    resizedImages.Add(new Tuple<string, ImageSize>(sourceBase64, imageSize));
                    continue;
                }

                int imgWidth, imgHeight;
                if (targetSize < imageLargerSize)
                {
                    double ratio = (double)targetSize / imageLargerSize;
                    imgWidth = (int)(image.Width * ratio);
                    imgHeight = (int)(image.Height * ratio);

                    image.Mutate(x => x.Resize(new ResizeOptions
                    {
                        Mode = ResizeMode.Crop,
                        Size = new Size(imgWidth, imgHeight),
                        Sampler = KnownResamplers.Lanczos3
                    }));
                }

                using var memoryStream = new MemoryStream();
                if (extension == "webp")
                {
                    var webpEncoder = new WebpEncoder { Quality = 80 };
                    image.Save(memoryStream, webpEncoder);
                }
                else
                {
                    var jpegEncoder = new JpegEncoder { Quality = 75, SkipMetadata = true };
                    image.Save(memoryStream, jpegEncoder);
                }

                var resizedImageBase64 = $"data:image/{extension};base64,{Convert.ToBase64String(memoryStream.ToArray())}";
                resizedImages.Add(new Tuple<string, ImageSize>(resizedImageBase64, imageSize));
            }

            return resizedImages;
        }

        public static string ConverToThumbnail(this string sourceBase64)
        {

            using var memoryStream = new MemoryStream();
            byte[] result;
            var img = Convert.FromBase64String(Regex.Replace(sourceBase64, @"^data:image\/[a-zA-Z]+;base64,", string.Empty));
            using (Image image = Image.Load(img))
            {
                var encoder = new JpegEncoder()
                {
                    Quality = 100
                };
                image.Mutate(x => x.Resize(70, 70));

                image.Save(memoryStream, encoder);
                result = memoryStream.ToArray();
            }

            return result.ToBase64JPG();

        }

        public static string ConverToThumbnailCore(this string sourceBase64, int size)
        {
            int imgWidth = 0;
            int imgHeight = 0;
            using var memoryStream = new MemoryStream();
            byte[] result;
            var img = Convert.FromBase64String(Regex.Replace(sourceBase64, @"^data:image\/[a-zA-Z]+;base64,", string.Empty));
            using (Image image = Image.Load(img))
            {
                if (image.Width > image.Height)
                {
                    imgWidth = size;
                    imgHeight = (int)((double)size / ((double)image.Width / (double)image.Height));
                }
                else
                {
                    imgWidth = (int)((double)size / ((double)image.Height / (double)image.Width));
                    imgHeight = size;
                }

                var encoder = new JpegEncoder()
                {
                    Quality = 100
                };
                image.Mutate(x => x.Resize(imgWidth, imgHeight));

                image.Save(memoryStream, encoder);
                result = memoryStream.ToArray();
            }

            return result.ToBase64JPG();

        }
        public static bool ValidateImageFormat(this string data)
        {
            List<string> ImageFormat = new List<string>() { "jpg", "jpeg", "png", "gif", "bmp", "ico", "webp" };
            var imageFormatToUpload = data.Split('/')[1];
            imageFormatToUpload = imageFormatToUpload.Split(";")[0];
            if (!ImageFormat.Contains(imageFormatToUpload))
            {
                throw new AppException("INVALID_IMAGE_FORMAT");
            }
            return true;
        }

        public static bool ValidateMediaFormat(this string data)
        {
            List<string> ImageFormat = new List<string>() { "jpg", "jpeg", "png", "gif", "bmp", "mp4", "webp" };
            var imageFormatToUpload = data.Split('/')[1];
            imageFormatToUpload = imageFormatToUpload.Split(";")[0];
            if (!ImageFormat.Contains(imageFormatToUpload))
            {
                throw new AppException("INVALID_MEDIA_FORMAT");
            }
            return true;
        }
        public static bool IsBase64(this string Base64) => Base64.IndexOf("base64") > 0;
        public static string ToBase64JPG(this byte[] bytes) => $"data:image/jpeg;base64,{Convert.ToBase64String(bytes)}";
        public static bool IsUrl(this string data) =>
            Uri.TryCreate(data, UriKind.Absolute, out var uriResult)
                && (
                    uriResult.Scheme == Uri.UriSchemeHttp
                    || uriResult.Scheme == Uri.UriSchemeHttps
                );

    }

    public enum ImageSize
    {
        MEDIUM, SMALL, THUMBNAIL, ORIGINAL
    }
}
