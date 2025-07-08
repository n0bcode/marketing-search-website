using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VideoSubtitleExtractor.Services
{
    public static class Downloader
    {
        public static async Task DownloadFileWithHttpClientAsync(string downloadUrl, string outputFile, string? cookieString = null)
        {
            Console.WriteLine("Downloading file with HttpClient...");
            using var handler = new HttpClientHandler { AllowAutoRedirect = true };
            using var client = new HttpClient(handler);
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 Chrome/114 Safari/537.36");
            client.DefaultRequestHeaders.Referrer = new Uri("https://www.tiktok.com/");
            client.DefaultRequestHeaders.Accept.ParseAdd("video/mp4,video/*;q=0.9,application/octet-stream;q=0.8");
            client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("en-US,en;q=0.5");
            if (!string.IsNullOrEmpty(cookieString)) client.DefaultRequestHeaders.Add("Cookie", cookieString);
            using var response = await client.GetAsync(downloadUrl);
            response.EnsureSuccessStatusCode();
            await using var fs = new FileStream(outputFile, FileMode.Create, FileAccess.Write);
            await response.Content.CopyToAsync(fs);
        }
    }
}