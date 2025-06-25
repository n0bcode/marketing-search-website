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
            do
            {
                Console.WriteLine("Bạn có muốn đăng nhập vào mạng xã hội trước khi tải/trích xuất? (Y/N)");
                string loginChoice = Console.ReadLine()?.Trim().ToUpper();
                if (loginChoice == "Y")
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
                    if (hasCookie)
                    {
                        Console.WriteLine($"Đã tìm thấy file cookie ({cookieFile}). Dùng lại session cũ? (Y/N)");
                        string reuse = Console.ReadLine()?.Trim().ToUpper();
                        if (reuse == "Y")
                        {
                            Console.WriteLine($"Sử dụng lại session đã đăng nhập từ {cookieFile}.");
                        }
                        else
                        {
                            VideoSubtitleExtractor.Services.BrowserLoginHelper.LaunchAndLogin(loginUrl, cookieFile, profileDir);
                            Console.WriteLine("Sau khi đăng nhập xong, nhấn Enter để tiếp tục...");
                            Console.ReadLine();
                        }
                    }
                    else
                    {
                        VideoSubtitleExtractor.Services.BrowserLoginHelper.LaunchAndLogin(loginUrl, cookieFile, profileDir);
                        Console.WriteLine("Sau khi đăng nhập xong, nhấn Enter để tiếp tục...");
                        Console.ReadLine();
                    }
                }

                string cookies = null;
                string fileName = "";

                Console.WriteLine("Enter video/audio URL (YouTube, TikTok, Facebook, mp4, mp3, m4a...):");
                string mediaUrl = Console.ReadLine();

                if (!MediaHelper.IsSupportedMediaUrl(mediaUrl))
                {
                    Console.WriteLine("Invalid or unsupported video URL.");
                    continue;
                }

                if (MediaHelper.IsFacebookUrl(mediaUrl))
                {
                    Console.WriteLine("Resolving Facebook video link using Selenium...");
                    (string resolvedUrl, string resolvedFileName) = FacebookService.GetFacebookDownloadInfo(mediaUrl);
                    if (!string.IsNullOrWhiteSpace(resolvedUrl))
                    {
                        mediaUrl = resolvedUrl;
                        fileName = resolvedFileName;
                    }
                    else
                    {
                        Console.WriteLine("Failed to resolve Facebook video link.");
                        continue;
                    }
                }
                else if (MediaHelper.IsTikTokUrl(mediaUrl))
                {
                    Console.WriteLine("Resolving TikTok video URL (HTML parse)...");
                    (mediaUrl, cookies) = await TiktokService.ResolveTikTokDownloadLinkViaHttp(mediaUrl);
                    if (mediaUrl == null)
                    {
                        Console.WriteLine("Failed to resolve TikTok video URL.");
                        continue;
                    }
                }

                // Sử dụng tên file ổn định dựa trên hash URL
                string outputDir = "outputs";
                if (!Directory.Exists(outputDir))
                    Directory.CreateDirectory(outputDir);
                string stableMediaFile = Path.Combine(outputDir, MediaHelper.GetStableFileNameFromUrl(mediaUrl, ".mp4"));
                string stableAudioFile = Path.Combine(outputDir, MediaHelper.GetStableFileNameFromUrl(mediaUrl, ".wav"));
                string stableTranscriptFile = Path.Combine(outputDir, MediaHelper.GetStableFileNameFromUrl(mediaUrl, ".txt"));

                // Xóa các file media/audio dở quá 1 ngày
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

                // Nếu đã có transcript thì dùng lại, không xử lý lại
                if (File.Exists(stableTranscriptFile))
                {
                    Console.WriteLine($"Đã có transcript: {stableTranscriptFile}. Không cần xử lý lại.");
                }
                else
                {
                    // Nếu có file media/audio dở (chưa quá 1 ngày), dùng lại, nếu không thì tải mới
                    if (!File.Exists(stableMediaFile))
                    {
                        await Downloader.DownloadFileWithHttpClientAsync(mediaUrl, stableMediaFile, cookies);
                    }
                    try
                    {
                        await AudioConverter.ConvertToWavWithFFMpegCore(stableMediaFile, stableAudioFile);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("FFmpeg failed to convert audio: " + ex.Message);
                        Console.WriteLine($"Giữ lại file media/audio dở: {stableMediaFile}");
                        continue;
                    }

                    Console.WriteLine("Transcribing audio with Whisper.NET...");
                    try
                    {
                        await WhisperTranscriber.TranscribeAsync(stableAudioFile, stableTranscriptFile);
                        // Xóa file media/audio sau khi xử lý xong
                        try { File.Delete(stableMediaFile); } catch { }
                        try { File.Delete(stableAudioFile); } catch { }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Lỗi khi trích xuất transcript: " + ex.Message);
                        Console.WriteLine($"Giữ lại file media/audio dở: {stableMediaFile}, {stableAudioFile}");
                        continue;
                    }
                }

                Console.WriteLine("Bạn có muốn mở lại trang web với session đã đăng nhập? (Y/N)");
                string openWeb = Console.ReadLine()?.Trim().ToUpper();
                if (openWeb == "Y")
                {
                    Console.WriteLine("Nhập lại URL muốn mở (hoặc để trống dùng URL vừa nhập):");
                    string openUrl = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(openUrl)) openUrl = mediaUrl;
                    Console.WriteLine("Chọn profile: 1. Facebook  2. TikTok  3. Khác");
                    string openProfile = Console.ReadLine()?.Trim();
                    string cookieFile = "";
                    string profileDir = "";
                    switch (openProfile)
                    {
                        case "1":
                            cookieFile = "facebook_cookies.txt";
                            profileDir = VideoSubtitleExtractor.Services.BrowserLoginHelper.GetProfileDir("facebook");
                            break;
                        case "2":
                            cookieFile = "tiktok_cookies.txt";
                            profileDir = VideoSubtitleExtractor.Services.BrowserLoginHelper.GetProfileDir("tiktok");
                            break;
                        default:
                            cookieFile = "custom_cookies.txt";
                            profileDir = VideoSubtitleExtractor.Services.BrowserLoginHelper.GetProfileDir("custom");
                            break;
                    }
                    VideoSubtitleExtractor.Services.BrowserLoginHelper.OpenSiteWithSession(openUrl, cookieFile, profileDir);
                }
                Console.WriteLine("\n=== Do you want to try again? ('Space' to cancel; 'Y' to continue) ===\n");
                string? tryAgain = Console.ReadLine();
                loop = !string.IsNullOrEmpty(tryAgain);
                Console.WriteLine(loop ? "\n=== Continued ===\n" : "\n=== Cancelled ===\n");
            }
            while (loop);
        }
    }
}
