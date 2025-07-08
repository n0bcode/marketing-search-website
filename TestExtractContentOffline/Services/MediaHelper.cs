using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VideoSubtitleExtractor.Services
{
    public static class MediaHelper
    {
        // Sinh tên file ổn định dựa trên hash của URL (dùng cho media/audio và transcript)
        // Sinh tên file ổn định dựa trên hash của URL đã chuẩn hóa
        public static string GetStableFileNameFromUrl(string url, string ext)
        {
            string normUrl = NormalizeUrl(url);
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var hash = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(normUrl));
                string hashStr = BitConverter.ToString(hash).Replace("-", "").ToLower();
                return $"media_{hashStr}{ext}";
            }
        }

        public static string GetFileNameFromUrl(string url)
        {
            Uri uri = new Uri(url);
            string name = Path.GetFileName(uri.LocalPath);
            if (string.IsNullOrWhiteSpace(name)) name = "downloaded_input";
            string ext = Path.GetExtension(name);
            if (string.IsNullOrWhiteSpace(ext)) ext = ".mp4";
            name = Path.GetFileNameWithoutExtension(name);
            foreach (char c in Path.GetInvalidFileNameChars()) name = name.Replace(c, '_');
            name += $"_{DateTime.Now:yyyyMMdd_HHmmss}";
            return name + ext;
        }
        public static bool IsSupportedMediaUrl(string url) =>
            Uri.TryCreate(url, UriKind.Absolute, out var uriResult) &&
            (IsYouTubeUrl(url) || IsFacebookUrl(url) || IsTikTokUrl(url) ||
            url.EndsWith(".mp4") || url.EndsWith(".mp3") || url.EndsWith(".m4a"));

        public static bool IsYouTubeUrl(string url) => url.Contains("youtube.com", StringComparison.OrdinalIgnoreCase) || url.Contains("youtu.be", StringComparison.OrdinalIgnoreCase);

        public static bool IsFacebookUrl(string url) => url.Contains("facebook.com", StringComparison.OrdinalIgnoreCase);

        public static bool IsTikTokUrl(string url) => url.Contains("tiktok.com", StringComparison.OrdinalIgnoreCase);

        #region [Mã chuẩn hóa url]
        // Chuẩn hóa URL để cùng 1 video luôn ra cùng 1 tên file (loại bỏ query không cần thiết, chuyển về chữ thường, bỏ dấu / cuối)
        public static string NormalizeUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return url;
            try
            {
                var uri = new Uri(url);
                string baseUrl = uri.GetLeftPart(UriPartial.Path).TrimEnd('/').ToLowerInvariant();
                // Đối với YouTube, chỉ lấy id video
                if (IsYouTubeUrl(url))
                {
                    var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
                    string vid = query["v"];
                    if (!string.IsNullOrEmpty(vid))
                        return $"youtube.com/watch?v={vid}";
                    // youtu.be short link
                    if (uri.Host.Contains("youtu.be"))
                        return $"youtu.be{uri.AbsolutePath}";
                }
                // Đối với TikTok, Facebook: chỉ lấy path chính
                if (IsTikTokUrl(url) || IsFacebookUrl(url))
                {
                    return baseUrl;
                }
                // Mặc định: bỏ query, fragment, về chữ thường
                return baseUrl;
            }
            catch { return url.Trim().ToLowerInvariant(); }
        }

        #endregion

    }
}