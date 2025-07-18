using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using System.Net.Http;

namespace Api.Services.VideoServices
{
    public static class TiktokService
    {
        public static async Task<(string? videoUrl, string? cookies)> ResolveTikTokDownloadLinkViaHttp(string tiktokUrl)
        {
            string html = string.Empty;
            string? finalVideoUrl = null;
            string? cookieString = null;

            using var handler = new HttpClientHandler { AllowAutoRedirect = true, UseCookies = false };
            using var client = new HttpClient(handler);
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) Chrome/91.0.4472.124");
            client.DefaultRequestHeaders.Referrer = new Uri("https://www.tiktok.com/");

            var request = new HttpRequestMessage(HttpMethod.Get, tiktokUrl);
            var response = await client.SendAsync(request);
            html = await response.Content.ReadAsStringAsync();

            if (response.Headers.TryGetValues("Set-Cookie", out var cookieHeaders))
            {
                cookieString = string.Join("; ", cookieHeaders.Select(c => c.Split(';')[0]));
            }

            var match = Regex.Match(html, "<script id=\"__UNIVERSAL_DATA_FOR_REHYDRATION__\" type=\"application/json\">([\\s\\S]*?)</script>");
            if (match.Success)
            {
                string jsonStr = match.Groups[1].Value;
                var data = JObject.Parse(jsonStr);
                finalVideoUrl = data?["__DEFAULT_SCOPE__"]?["webapp.video-detail"]?["itemInfo"]?["itemStruct"]?["video"]?["playAddr"]?.ToString();
            }

            return (finalVideoUrl, cookieString);
        }

    }
}
