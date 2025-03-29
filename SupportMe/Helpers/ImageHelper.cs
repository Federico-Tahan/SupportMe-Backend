using System.Text.RegularExpressions;
using SupportMe.Error;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace SupportMe.Helpers
{
    public static class ImageHelper
    {
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
        public static bool ValidateImageFormat(this string data)
        {
            List<string> ImageFormat = new List<string>() { "jpg", "jpeg", "png", "gif", "bmp" };
            var imageFormatToUpload = data.Split('/')[1];
            imageFormatToUpload = imageFormatToUpload.Split(";")[0];
            if (!ImageFormat.Contains(imageFormatToUpload))
            {
                throw new AppException("INVALID_IMAGE_FORMAT");
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
}
