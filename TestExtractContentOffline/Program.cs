using System;
using System.IO;
using System.Net.Http;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
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
                Console.WriteLine("3. Thoát");
                Console.Write("Chọn option (1/2/3): ");
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
                        loop = false;
                        Console.WriteLine("\n=== Đã thoát chương trình ===\n");
                        break;
                    default:
                        Console.WriteLine("Lựa chọn không hợp lệ. Vui lòng chọn lại.");
                        break;
                }
            }
        }
    }
}
