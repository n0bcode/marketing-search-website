using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VideoSubtitleExtractor.Services
{
    public static class MediaHelper
    {
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
    }
}