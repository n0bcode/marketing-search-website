using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Api.Models;

namespace Api.Services.SearchServices
{
    public class BingSearchService
    {
        public async Task<ResponseAPI<List<BingSearchResult>>> SearchBing(string query, int maxResults = 0, string site = null, string timeRange = null, string filetype = null, string language = null, string region = null)
        {
            var results = new List<BingSearchResult>();
            string nextUrl = BuildBingSearchUrl(query, site, timeRange, filetype, language, region);
            string lastHtml = null;

            while (!string.IsNullOrEmpty(nextUrl))
            {
                try
                {
                    using var httpClient = new HttpClient();
                    httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) Chrome/122.0.0.0 Safari/537.36");
                    var html = await httpClient.GetStringAsync(nextUrl);
                    lastHtml = html;

                    var doc = new HtmlAgilityPack.HtmlDocument();
                    doc.LoadHtml(html);

                    var nodes = doc.DocumentNode.SelectNodes("//ol[@id='b_results']/li[contains(@class,'b_algo')]");
                    if (nodes != null)
                    {
                        foreach (var node in nodes)
                        {
                            var aNode = node.SelectSingleNode(".//h2/a");
                            var title = aNode?.InnerText?.Trim() ?? "";
                            var url = aNode?.GetAttributeValue("href", "") ?? "";
                            var snippetNode = node.SelectSingleNode(".//div[contains(@class,'b_caption')]/p") ??
                                              node.SelectSingleNode(".//div[contains(@class,'b_caption')]");
                            var snippet = snippetNode?.InnerText?.Trim() ?? "";

                            if (!string.IsNullOrWhiteSpace(title) && !string.IsNullOrWhiteSpace(url))
                            {
                                results.Add(new BingSearchResult { Title = title, Url = url, Snippet = snippet });
                            }

                            if (maxResults > 0 && results.Count >= maxResults) break;
                        }
                    }

                    if (maxResults > 0 && results.Count >= maxResults) break;

                    var nextPageNode = doc.DocumentNode.SelectSingleNode("//a[contains(@class,'sb_pagN') and not(contains(@class,'sb_inactN'))]");
                    if (nextPageNode != null)
                    {
                        string href = nextPageNode.GetAttributeValue("href", null);
                        if (!string.IsNullOrEmpty(href))
                            nextUrl = href.StartsWith("http") ? href : "https://www.bing.com" + href;
                        else nextUrl = null;
                    }
                    else nextUrl = null;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lỗi khi request Bing: {ex.Message}");
                    return new ResponseAPI<List<BingSearchResult>> { Success = false, Message = $"Lỗi khi tìm kiếm Bing: {ex.Message}" };
                }
            }

            if (results.Count == 0)
            {
                return new ResponseAPI<List<BingSearchResult>> { Success = false, Message = "Không lấy được kết quả nào từ Bing. Có thể Bing đã chặn bot hoặc thay đổi cấu trúc HTML." };
            }

            return new ResponseAPI<List<BingSearchResult>> { Success = true, Data = results, Message = "Tìm kiếm Bing thành công." };
        }

        private string BuildBingSearchUrl(string query, string site, string timeRange, string filetype, string language, string region)
        {
            var urlParams = new List<string>();
            urlParams.Add(Uri.EscapeDataString(query));

            string searchUrl = $"https://www.bing.com/search?q={string.Join(" ", urlParams)}";

            if (!string.IsNullOrWhiteSpace(site)) searchUrl += $" site:{site}";
            if (!string.IsNullOrWhiteSpace(filetype)) searchUrl += $" filetype:{filetype}";
            if (!string.IsNullOrWhiteSpace(language)) searchUrl += $"&lr={language}";
            if (!string.IsNullOrWhiteSpace(region)) searchUrl += $"&cr={region}";
            if (!string.IsNullOrWhiteSpace(timeRange))
            {
                if (timeRange.ToLower().Contains("year")) searchUrl += "&tbs=qdr:y";
                else if (timeRange.ToLower().Contains("month")) searchUrl += "&tbs=qdr:m";
                else if (timeRange.ToLower().Contains("24")) searchUrl += "&tbs=qdr:d";
            }
            return searchUrl;
        }
    }

    public class BingSearchResult
    {
        public string Title { get; set; }
        public string Url { get; set; }
        public string Snippet { get; set; }
    }
}