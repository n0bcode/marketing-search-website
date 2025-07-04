using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Api.Models;
using Api.Services.SearchServices.Google;

namespace Api.Services.SearchServices
{
    public class BingSearchService
    {
        public async Task<ResponseAPI<List<BingSearchResult>>> SearchBing(SearchRequest request)
        {
            var results = new List<BingSearchResult>();
            string nextUrl = BuildBingSearchUrl(request);
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
                        }
                    }

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

        private string BuildBingSearchUrl(SearchRequest request)
        {
            var urlParams = new List<string>();
            urlParams.Add(Uri.EscapeDataString(request.q));

            string searchUrl = $"https://www.bing.com/search?q={string.Join(" ", urlParams)}";

            if (!string.IsNullOrWhiteSpace(request.as_sitesearch)) searchUrl += $" site:{request.as_sitesearch}";
            if (!string.IsNullOrWhiteSpace(request.type)) searchUrl += $" filetype:{request.type}";
            if (!string.IsNullOrWhiteSpace(request.hl)) searchUrl += $"&lr={request.hl}";
            if (!string.IsNullOrWhiteSpace(request.gl)) searchUrl += $"&cr={request.gl}";
            if (!string.IsNullOrWhiteSpace(request.tbs))
            {
                if (request.tbs.ToLower().Contains("year")) searchUrl += "&tbs=qdr:y";
                else if (request.tbs.ToLower().Contains("month")) searchUrl += "&tbs=qdr:m";
                else if (request.tbs.ToLower().Contains("24")) searchUrl += "&tbs=qdr:d";
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