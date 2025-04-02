using SupportMe.Error;

namespace SupportMe.Helpers
{
    public static class VideoHelper
    {
        public static bool ValidateVideoFormat(this string data)
        {
            List<string> VideoFormat = new List<string>() { "mp4" };
            var VideoFormatToUpload = data.Split('/')[1];
            VideoFormatToUpload = VideoFormatToUpload.Split(";")[0];
            if (!VideoFormat.Contains(VideoFormatToUpload))
            {
                throw new AppException("INVALID_VIDEO_FORMAT");
            }
            return true;
        }

        public static string GetFileExtension(this string data)
        {
            var fileFormatToUpload = data.Split('/')[1];
            fileFormatToUpload = fileFormatToUpload.Split(";")[0];
            return fileFormatToUpload;
        }
    }
}
