using System;
using System.IO;
using System.Net.Http;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using HtmlAgilityPack;
using VideoSubtitleExtractor.Services;

namespace VideoSubtitleExtractor
{
    class Program
    {
        static async Task Main(string[] args)
        {
            bool loop = true;
            while (loop)
            {
                Console.WriteLine("\n=== MENU CHÍNH ===");
                Console.WriteLine("1. Đăng nhập/lưu phiên truy cập mạng xã hội");
                Console.WriteLine("2. Đưa link chứa video và xử lý");
                Console.WriteLine("3. Tìm kiếm thông tin trên mạng xã hội (Facebook, TikTok, ...)");
                Console.WriteLine("4. Thoát");
                Console.Write("Chọn option (1/2/3/4): ");
                string mainChoice = Console.ReadLine()?.Trim();
                switch (mainChoice)
                {
                    case "1":
                        while (true)
                        {
                            Console.WriteLine("Chọn nền tảng để đăng nhập: 1. Facebook  2. TikTok  3. Khác");
                            string platformChoice = Console.ReadLine()?.Trim();
                            string loginUrl = "";
                            string cookieFile = "";
                            string profileDir = "";
                            switch (platformChoice)
                            {
                                case "1":
                                    loginUrl = "https://facebook.com/login";
                                    cookieFile = "facebook_cookies.txt";
                                    profileDir = VideoSubtitleExtractor.Services.BrowserLoginHelper.GetProfileDir("facebook");
                                    break;
                                case "2":
                                    loginUrl = "https://tiktok.com/login";
                                    cookieFile = "tiktok_cookies.txt";
                                    profileDir = VideoSubtitleExtractor.Services.BrowserLoginHelper.GetProfileDir("tiktok");
                                    break;
                                default:
                                    Console.WriteLine("Nhập URL trang web muốn đăng nhập:");
                                    loginUrl = Console.ReadLine();
                                    cookieFile = "custom_cookies.txt";
                                    profileDir = VideoSubtitleExtractor.Services.BrowserLoginHelper.GetProfileDir("custom");
                                    break;
                            }
                            bool hasCookie = File.Exists(cookieFile) && new FileInfo(cookieFile).Length > 0;
                            bool loginSuccess = false;
                            if (hasCookie)
                            {
                                Console.WriteLine($"Đã tìm thấy file cookie ({cookieFile}). Dùng lại session cũ? (Y/N)");
                                string reuse = Console.ReadLine()?.Trim().ToUpper();
                                if (reuse == "Y")
                                {
                                    Console.WriteLine($"Sử dụng lại session đã đăng nhập từ {cookieFile}.");
                                    loginSuccess = true;
                                }
                                else
                                {
                                    VideoSubtitleExtractor.Services.BrowserLoginHelper.LaunchAndLogin(loginUrl, cookieFile, profileDir);
                                    Console.WriteLine("Sau khi đăng nhập xong, nhấn Enter để tiếp tục...");
                                    Console.ReadLine();
                                    loginSuccess = File.Exists(cookieFile) && new FileInfo(cookieFile).Length > 0;
                                }
                            }
                            else
                            {
                                VideoSubtitleExtractor.Services.BrowserLoginHelper.LaunchAndLogin(loginUrl, cookieFile, profileDir);
                                Console.WriteLine("Sau khi đăng nhập xong, nhấn Enter để tiếp tục...");
                                Console.ReadLine();
                                loginSuccess = File.Exists(cookieFile) && new FileInfo(cookieFile).Length > 0;
                            }
                            if (loginSuccess)
                            {
                                Console.WriteLine($"Đã lưu phiên truy cập thành công vào {cookieFile}.");
                            }
                            else
                            {
                                Console.WriteLine($"Không lưu được phiên truy cập vào {cookieFile}.");
                                Console.WriteLine("1. Thử lại");
                                Console.WriteLine("2. Chuyển sang trích xuất video");
                                Console.WriteLine("3. Quay về menu chính");
                                string retryChoice = Console.ReadLine()?.Trim();
                                if (retryChoice == "1") continue;
                                if (retryChoice == "2") goto case2;
                                break;
                            }
                            Console.WriteLine("1. Chuyển sang trích xuất video");
                            Console.WriteLine("2. Quay về menu chính");
                            string nextChoice = Console.ReadLine()?.Trim();
                            if (nextChoice == "1") goto case2;
                            break;
                        }
                        break;
                    case "2":
                    case2:
                        string cookies = null;
                        string fileName = "";
                        Console.WriteLine("Enter video/audio URL (YouTube, TikTok, Facebook, mp4, mp3, m4a...):");
                        string mediaUrlOrigin = Console.ReadLine();
                        string mediaUrlProcessing = "";
                        if (!MediaHelper.IsSupportedMediaUrl(mediaUrlOrigin))
                        {
                            Console.WriteLine("Invalid or unsupported video URL.");
                            break;
                        }
                        if (MediaHelper.IsFacebookUrl(mediaUrlOrigin))
                        {
                            Console.WriteLine("Resolving Facebook video link using Selenium...");
                            (string resolvedUrl, string resolvedFileName) = FacebookService.GetFacebookDownloadInfo(mediaUrlOrigin);
                            if (!string.IsNullOrWhiteSpace(resolvedUrl))
                            {
                                mediaUrlProcessing = resolvedUrl;
                                fileName = resolvedFileName;
                            }
                            else
                            {
                                Console.WriteLine("Failed to resolve Facebook video link.");
                                break;
                            }
                        }
                        else if (MediaHelper.IsTikTokUrl(mediaUrlOrigin))
                        {
                            Console.WriteLine("Resolving TikTok video URL (HTML parse)...");
                            (mediaUrlProcessing, cookies) = await TiktokService.ResolveTikTokDownloadLinkViaHttp(mediaUrlOrigin);
                            if (mediaUrlProcessing == null)
                            {
                                Console.WriteLine("Failed to resolve TikTok video URL.");
                                break;
                            }
                        }
                        string outputDir = "outputs";
                        if (!Directory.Exists(outputDir))
                            Directory.CreateDirectory(outputDir);
                        string stableMediaFile = Path.Combine(outputDir, MediaHelper.GetStableFileNameFromUrl(mediaUrlProcessing, ".mp4"));
                        string stableAudioFile = Path.Combine(outputDir, MediaHelper.GetStableFileNameFromUrl(mediaUrlProcessing, ".wav"));
                        string stableTranscriptFile = Path.Combine(outputDir, MediaHelper.GetStableFileNameFromUrl(mediaUrlProcessing, ".txt"));
                        foreach (var file in Directory.GetFiles(outputDir, "media_*.mp4"))
                        {
                            var info = new FileInfo(file);
                            if (info.Exists && info.LastWriteTime < DateTime.Now.AddDays(-1))
                            {
                                try { File.Delete(file); } catch { }
                            }
                        }
                        foreach (var file in Directory.GetFiles(outputDir, "media_*.wav"))
                        {
                            var info = new FileInfo(file);
                            if (info.Exists && info.LastWriteTime < DateTime.Now.AddDays(-1))
                            {
                                try { File.Delete(file); } catch { }
                            }
                        }
                        if (File.Exists(stableTranscriptFile))
                        {
                            Console.WriteLine($"Đã có transcript: {stableTranscriptFile}. Không cần xử lý lại.");
                            // Đọc và in nội dung file transcript
                            string transcriptContent = File.ReadAllText(stableTranscriptFile);
                            Console.WriteLine("Nội dung video:");
                            Console.WriteLine(transcriptContent);
                        }
                        else
                        {
                            if (!File.Exists(stableMediaFile))
                            {
                                await Downloader.DownloadFileWithHttpClientAsync(mediaUrlProcessing, stableMediaFile, cookies);
                            }
                            try
                            {
                                await AudioConverter.ConvertToWavWithFFMpegCore(stableMediaFile, stableAudioFile);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("FFmpeg failed to convert audio: " + ex.Message);
                                Console.WriteLine($"Giữ lại file media/audio dở: {stableMediaFile}");
                                break;
                            }
                            Console.WriteLine("Transcribing audio with Whisper.NET...");
                            try
                            {
                                await WhisperTranscriber.TranscribeAsync(stableAudioFile, stableTranscriptFile);
                                try { File.Delete(stableMediaFile); } catch { }
                                try { File.Delete(stableAudioFile); } catch { }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("Lỗi khi trích xuất transcript: " + ex.Message);
                                Console.WriteLine($"Giữ lại file media/audio dở: {stableMediaFile}, {stableAudioFile}");
                                break;
                            }
                        }
                        // Gợi ý mở lại trang web với session đã lưu nếu có
                        string[] possibleProfiles = new[] { "facebook", "tiktok", "custom" };
                        var availableProfiles = new List<(string, string, string)>();
                        foreach (var p in possibleProfiles)
                        {
                            string cfile = p + "_cookies.txt";
                            string pdir = VideoSubtitleExtractor.Services.BrowserLoginHelper.GetProfileDir(p);
                            if (File.Exists(cfile) && new FileInfo(cfile).Length > 0)
                                availableProfiles.Add((p, cfile, pdir));
                        }
                        if (availableProfiles.Count > 0)
                        {
                            Console.WriteLine("Bạn có muốn mở lại trang web với session đã đăng nhập? (Y/N)");
                            string openWeb = Console.ReadLine()?.Trim().ToUpper();
                            if (openWeb == "Y")
                            {
                                Console.WriteLine("Nhập lại URL muốn mở (hoặc để trống dùng URL vừa nhập):");
                                string openUrl = Console.ReadLine();
                                if (string.IsNullOrWhiteSpace(openUrl)) openUrl = mediaUrlOrigin;
                                Console.WriteLine("Chọn profile:");
                                for (int i = 0; i < availableProfiles.Count; i++)
                                {
                                    Console.WriteLine($"{i + 1}. {availableProfiles[i].Item1}");
                                }
                                int idx = 0;
                                if (availableProfiles.Count == 1)
                                {
                                    idx = 0;
                                }
                                else
                                {
                                    Console.Write("Chọn số profile: ");
                                    int.TryParse(Console.ReadLine(), out idx);
                                    idx = Math.Max(0, Math.Min(idx - 1, availableProfiles.Count - 1));
                                }
                                var (p, cfile, pdir) = availableProfiles[idx];
                                VideoSubtitleExtractor.Services.BrowserLoginHelper.OpenSiteWithSession(openUrl, cfile, pdir);
                            }
                        }
                        break;
                    case "3":
                        Console.WriteLine("\n=== GOOGLE ADVANCED SEARCH ===");

                        Console.Write("Chọn công cụ tìm kiếm (1. Google, 2. Bing) [mặc định: Bing]: ");
                        string engineChoice = Console.ReadLine()?.Trim();
                        string engine = (engineChoice == "1" || engineChoice?.ToLower() == "google") ? "google" : "bing";

                        Console.Write("Nhập từ khóa tìm kiếm: ");
                        string keyword = Console.ReadLine();
                        if (string.IsNullOrWhiteSpace(keyword))
                        {
                            Console.WriteLine("Từ khóa không hợp lệ.");
                            break;
                        }
                        Console.Write("Nhập số lượng kết quả tối đa muốn lấy (0 = lấy hết): ");
                        int maxResults = 0;
                        int.TryParse(Console.ReadLine(), out maxResults);
                        if (maxResults < 0) maxResults = 0;
                        Console.Write("Nhập site cần tìm kiếm (bỏ trống nếu không): ");
                        string site = Console.ReadLine();
                        Console.Write("Nhập khoảng thời gian (ví dụ: past year, past month, past 24 hours, bỏ trống nếu không): ");
                        string timeRange = Console.ReadLine();
                        Console.Write("Nhập loại file cần tìm (ví dụ: pdf, docx, xlsx, bỏ trống nếu không): ");
                        string filetype = Console.ReadLine();
                        Console.Write("Bật chế độ nâng cao? (Y/N): ");
                        string advMode = Console.ReadLine()?.Trim().ToUpper();
                        string allWords = "", exactPhrase = "", anyWords = "", noneWords = "", region = "", language = "";
                        if (advMode == "Y")
                        {
                            Console.Write("Từ khóa phải chứa tất cả các từ (all these words): ");
                            allWords = Console.ReadLine();
                            Console.Write("Cụm từ chính xác (exact phrase): ");
                            exactPhrase = Console.ReadLine();
                            Console.Write("Chứa bất kỳ từ nào (any of these words): ");
                            anyWords = Console.ReadLine();
                            Console.Write("Không chứa các từ này (none of these words): ");
                            noneWords = Console.ReadLine();
                            Console.Write("Ngôn ngữ (ví dụ: lang_vi, lang_en, bỏ trống nếu không): ");
                            language = Console.ReadLine();
                            Console.Write("Khu vực (region, ví dụ: countryVN, countryUS, bỏ trống nếu không): ");
                            region = Console.ReadLine();
                        }
                        // Build Google search URL
                        var urlParams = new List<string>();
                        if (!string.IsNullOrWhiteSpace(allWords)) urlParams.Add(Uri.EscapeDataString(allWords));
                        if (!string.IsNullOrWhiteSpace(exactPhrase)) urlParams.Add("\"" + Uri.EscapeDataString(exactPhrase) + "\"");
                        if (!string.IsNullOrWhiteSpace(anyWords)) urlParams.Add(string.Join(" ", anyWords.Split(' ').Select(w => Uri.EscapeDataString(w))));
                        if (!string.IsNullOrWhiteSpace(noneWords)) urlParams.Add("-" + string.Join(" -", noneWords.Split(' ').Select(w => Uri.EscapeDataString(w))));
                        if (!string.IsNullOrWhiteSpace(keyword)) urlParams.Add(Uri.EscapeDataString(keyword));
                        string q = string.Join(" ", urlParams);
                        string searchUrl = $"https://www.bing.com/search?q={q}";
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
                        Console.WriteLine($"\nĐang tìm kiếm với URL: {searchUrl}\n");
                        Console.Write("Bạn có muốn scrape và xuất kết quả ra CSV không? (Y/N): ");
                        string export = Console.ReadLine()?.Trim().ToUpper();
                        if (export == "Y")
                        {
                            string outputCsvDir = "outputs";
                            if (!Directory.Exists(outputCsvDir)) Directory.CreateDirectory(outputCsvDir);
                            string csvPath = Path.Combine(outputCsvDir, $"google_results_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
                            if (engine == "google")
                                await ScrapeGoogleMultiPageAsync(searchUrl, csvPath, maxResults);
                            else
                                await ScrapeBingMultiPageAsync(searchUrl, csvPath, maxResults);
                        }
                        break;
                    case "4":
                        loop = false;
                        Console.WriteLine("\n=== Đã thoát chương trình ===\n");
                        break;
                    default:
                        Console.WriteLine("Lựa chọn không hợp lệ. Vui lòng chọn lại.");
                        break;
                }
            }
        }
        public static async Task ScrapeGoogleMultiPageAsync(string searchUrl, string outputCsvPath, int maxResults)
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) Chrome/122.0.0.0 Safari/537.36");
            var html = await httpClient.GetStringAsync(searchUrl);

            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);

            var results = new List<(string Title, string Url, string Snippet)>();
            var nodes = doc.DocumentNode.SelectNodes("//div[contains(@class,'tF2Cxc')]");
            if (nodes != null)
            {
                foreach (var node in nodes)
                {
                    var aNode = node.SelectSingleNode(".//div[contains(@class,'yuRUbf')]/a");
                    var titleNode = aNode?.SelectSingleNode(".//h3[contains(@class,'LC20lb')]");
                    var snippetNode = node.SelectSingleNode(".//div[contains(@class,'VwiC3b')]");

                    string title = titleNode?.InnerText?.Trim() ?? "";
                    string url = aNode?.GetAttributeValue("href", "") ?? "";
                    string snippet = snippetNode?.InnerText?.Trim() ?? "";

                    if (!string.IsNullOrWhiteSpace(title) && !string.IsNullOrWhiteSpace(url))
                        results.Add((title, url, snippet));
                }
            }

            if (results.Count == 0)
            {
                Console.WriteLine("Không lấy được kết quả nào. Có thể Google đã chặn bot hoặc thay đổi cấu trúc HTML.");
                File.WriteAllText("outputs/debug.html", html);
                Console.WriteLine("Đã lưu HTML kết quả vào outputs/debug.html để kiểm tra.");
                return;
            }

            using var writer = new StreamWriter(outputCsvPath, false, System.Text.Encoding.UTF8);
            writer.WriteLine("Title,Url,Snippet");
            foreach (var r in results)
            {
                writer.WriteLine($"\"{r.Title.Replace("\"", "\"\"")}\",\"{r.Url.Replace("\"", "\"\"")}\",\"{r.Snippet.Replace("\"", "\"\"")}\"");
            }
            Console.WriteLine($"Đã lưu kết quả vào: {outputCsvPath}");
        }
        public static async Task ScrapeBingMultiPageAsync(string searchUrl, string outputCsvPath, int maxResults = 0)
        {
            var results = new List<(string Title, string Url, string Snippet)>();
            string nextUrl = searchUrl;
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
                                results.Add((title, url, snippet));
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
                    Console.WriteLine("Dừng scrape và xử lý các kết quả đã lấy được...");
                    break;
                }
            }

            if (results.Count == 0)
            {
                Console.WriteLine("Không lấy được kết quả nào. Có thể Bing đã chặn bot hoặc thay đổi cấu trúc HTML.");
                if (!string.IsNullOrEmpty(lastHtml))
                {
                    File.WriteAllText("outputs/debug_bing.html", lastHtml);
                    Console.WriteLine("Đã lưu HTML kết quả vào outputs/debug_bing.html để kiểm tra.");
                }
                return;
            }

            using var writer = new StreamWriter(outputCsvPath, false, System.Text.Encoding.UTF8);
            writer.WriteLine("Title,Url,Snippet");
            foreach (var r in results)
            {
                writer.WriteLine($"\"{r.Title.Replace("\"", "\"\"")}\",\"{r.Url.Replace("\"", "\"\"")}\",\"{r.Snippet.Replace("\"", "\"\"")}\"");
            }
            Console.WriteLine($"Đã lưu kết quả vào: {outputCsvPath}");
        }
    }
}
