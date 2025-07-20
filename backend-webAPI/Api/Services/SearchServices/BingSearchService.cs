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
            string? nextUrl = BuildBingSearchUrl(request);

            if (string.IsNullOrEmpty(nextUrl))
            {
                return new ResponseAPI<List<BingSearchResult>> { Success = false, Message = $"Truy vấn tìm kiếm quá dài. Vui lòng rút gọn truy vấn của bạn." };
            }
            string lastHtml = string.Empty;
            bool htmlContentReceived = false;

            while (!string.IsNullOrEmpty(nextUrl))
            {
                try
                {
                    using var httpClient = new HttpClient();
                    httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) Chrome/122.0.0.0 Safari/537.36");
                    var html = await httpClient.GetStringAsync(nextUrl);
                    if (string.IsNullOrEmpty(html))
                    {
                        nextUrl = null; // Stop trying to get more pages
                        continue; // Skip to the next iteration of the while loop
                    }
                    lastHtml = html;
                    htmlContentReceived = true;

                    var doc = new HtmlAgilityPack.HtmlDocument();
                    doc.LoadHtml(html);

                    var nodes = doc.DocumentNode.SelectNodes("//ol[@id='b_results']/li[contains(@class,'b_algo')]");
                    if (nodes != null)
                    {
                        foreach (var node in nodes)
                        {
                            var aNode = node.SelectSingleNode(".//h2/a");
                            var title = aNode?.InnerText?.Trim() ?? string.Empty;
                            var url = aNode?.GetAttributeValue("href", string.Empty) ?? string.Empty;
                            var snippetNode = node.SelectSingleNode(".//div[contains(@class,'b_caption')]/p") ??
                                              node.SelectSingleNode(".//div[contains(@class,'b_caption')]");
                            var snippet = snippetNode?.InnerText?.Trim() ?? string.Empty;

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
                    return new ResponseAPI<List<BingSearchResult>> { Success = true, Data = results, Message = $"Lỗi khi tìm kiếm Bing: {ex.Message}" };
                }
            }

            if (results.Count == 0)
            {
                if (!htmlContentReceived)
                {
                    return new ResponseAPI<List<BingSearchResult>> { Success = false, Message = "Không thể kết nối đến Bing hoặc không nhận được nội dung HTML. Vui lòng thử lại sau." };
                }
                else
                {
                    return new ResponseAPI<List<BingSearchResult>> { Success = false, Message = "Không lấy được kết quả nào từ Bing. Có thể Bing đã chặn bot hoặc thay đổi cấu trúc HTML." };
                }
            }

            return new ResponseAPI<List<BingSearchResult>> { Success = true, Data = results, Message = "Tìm kiếm Bing thành công." };
        }

        private const int MaxUrlLength = 2000; // A common practical limit for URLs

        private string? BuildBingSearchUrl(SearchRequest request)
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

            // Check if the URL length exceeds the limit
            if (searchUrl.Length > MaxUrlLength)
            {
                return null;
            }

            return searchUrl;
        }
    }

    public class BingSearchResult
    {
        public string Title { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string Snippet { get; set; } = string.Empty;
    }
}