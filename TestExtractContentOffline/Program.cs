using System;
using System.IO;
using System.Net.Http;
using VideoLibrary;
using Whisper.net;
using FFMpegCore;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace VideoSubtitleExtractor
{
    class Program
    {
        static async Task Main(string[] args)
        {
            bool loop = true;
            do
            {
                string cookies = null;
                string fileName = "";

                Console.WriteLine("Enter video/audio URL (YouTube, TikTok, Facebook, mp4, mp3, m4a...):");
                string mediaUrl = Console.ReadLine();

                if (!IsSupportedMediaUrl(mediaUrl))
                {
                    Console.WriteLine("Invalid or unsupported video URL.");
                    continue;
                }
                if (IsFacebookUrl(mediaUrl))
                {
                    Console.WriteLine("Resolving Facebook video link using Selenium...");
                    (string? resolvedUrl, string? resolvedFileName) = GetFacebookDownloadInfo(mediaUrl);
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
                if (IsTikTokUrl(mediaUrl))
                {
                    Console.WriteLine("Resolving TikTok video URL (HTML parse)...");
                    (mediaUrl, cookies) = await ResolveTikTokDownloadLinkViaHttp(mediaUrl);
                    if (mediaUrl == null)
                    {
                        Console.WriteLine("Failed to resolve TikTok video URL.");
                        continue;
                    }
                }

                fileName = GetFileNameFromUrl(mediaUrl);
                string baseName = Path.GetFileNameWithoutExtension(fileName);
                string inputFile = fileName;
                string outputDir = "outputs";

                if (!Directory.Exists(outputDir))
                    Directory.CreateDirectory(outputDir);

                string audioFile = Path.Combine(outputDir, baseName + ".wav");
                string transcriptFile = Path.Combine(outputDir, baseName + ".txt");

                await DownloadFileWithHttpClientAsync(mediaUrl, inputFile, cookies);

                try
                {
                    await ConvertToWavWithFFMpegCore(inputFile, audioFile);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("FFmpeg failed to convert audio: " + ex.Message);
                    continue;
                }

                Console.WriteLine("Transcribing audio with Whisper.NET...");

                using var whisperFactory = WhisperFactory.FromPath("whisper-models/ggml-base.bin");
                using var processor = whisperFactory.CreateBuilder().WithLanguage("vi").Build();

                using (var fileStream = File.OpenRead(audioFile))
                {
                    byte[] header = new byte[4];
                    fileStream.Read(header, 0, 4);
                    string riffHeader = System.Text.Encoding.ASCII.GetString(header);
                    if (riffHeader != "RIFF")
                        throw new InvalidDataException("File is not a valid RIFF WAV file.");

                    fileStream.Position = 0;
                    var segments = new List<SegmentData>();
                    await foreach (var segment in processor.ProcessAsync(fileStream))
                    {
                        segments.Add(segment);
                    }
                    string transcript = string.Join(Environment.NewLine, segments.Select(s => s.Text));
                    File.WriteAllText(transcriptFile, transcript);

                    Console.WriteLine("\n=== Transcription Result ===\n");
                    Console.WriteLine(transcript);
                }

                Console.WriteLine("\n=== Do you want to try again? ('Space' to cancel; 'Y' to continue) ===\n");
                string? tryAgain = Console.ReadLine();
                if (string.IsNullOrEmpty(tryAgain))
                {
                    loop = false;
                    Console.WriteLine("\n=== Cancelled ===\n");
                }
                else
                {
                    Console.WriteLine("\n=== Continued ===\n");
                }
            }
            while (loop);
        }

        static bool IsSupportedMediaUrl(string url) =>
            Uri.TryCreate(url, UriKind.Absolute, out var uriResult) &&
            (IsYouTubeUrl(url) || IsFacebookUrl(url) || IsTikTokUrl(url) ||
            url.EndsWith(".mp4") || url.EndsWith(".mp3") || url.EndsWith(".m4a"));

        static bool IsYouTubeUrl(string url) => url.Contains("youtube.com", StringComparison.OrdinalIgnoreCase) || url.Contains("youtu.be", StringComparison.OrdinalIgnoreCase);

        static bool IsFacebookUrl(string url) => url.Contains("facebook.com", StringComparison.OrdinalIgnoreCase);

        static bool IsTikTokUrl(string url) => url.Contains("tiktok.com", StringComparison.OrdinalIgnoreCase);

        static string GetFileNameFromUrl(string url)
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

        static async Task<(string? videoUrl, string? cookies)> ResolveTikTokDownloadLinkViaHttp(string tiktokUrl)
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

        static async Task DownloadFileWithHttpClientAsync(string downloadUrl, string outputFile, string? cookieString = null)
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

        static async Task ConvertToWavWithFFMpegCore(string inputPath, string outputPath)
        {
            Console.WriteLine("Checking input format before converting to WAV...");
            string tempMp4 = "temp.mp4";
            await FFMpegArguments
                .FromFileInput(inputPath)
                .OutputToFile(tempMp4, true, options => options
                    .WithVideoCodec("copy")
                    .WithAudioCodec("aac"))
                .ProcessAsynchronously();
            Console.WriteLine("Converting to WAV using FFMpegCore...");
            await FFMpegArguments
                .FromFileInput(tempMp4)
                .OutputToFile(outputPath, true, options => options
                    .WithAudioCodec("pcm_s16le")
                    .WithCustomArgument("-ar 16000 -ac 1"))
                .ProcessAsynchronously();
            if (File.Exists(tempMp4)) File.Delete(tempMp4);
        }

        public static (string?, string?) GetFacebookDownloadInfo(string url)
        {
            string? realUrl = null;
            string? fileName = null;
            var options = new ChromeOptions();
            using (IWebDriver driver = new ChromeDriver(options))
            {
                driver.Navigate().GoToUrl("https://getsave.net/vi");
                System.Threading.Thread.Sleep(5000);
                try
                {
                    var url_field = driver.FindElement(By.Id("postUrl"));
                    url_field.SendKeys(url);
                    url_field.SendKeys(Keys.Enter);
                    System.Threading.Thread.Sleep(5000);

                    var download_button = driver.FindElements(By.XPath("//div[contains(@class, 'media-card')]"))
                        .FirstOrDefault(e => e.GetAttribute("data-link").Contains("audioProcess"));
                    if (download_button != null)
                    {
                        string apiUrl = download_button.GetAttribute("data-link");
                        using var client = new HttpClient();
                        var response = client.GetStringAsync(apiUrl).Result;
                        var json = JObject.Parse(response);

                        realUrl = json["fileUrl"]?.ToString();
                        fileName = json["fileName"]?.ToString();

                        Console.WriteLine("Resolved Facebook file URL: " + realUrl);
                        Console.WriteLine("Resolved file name: " + fileName);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error while resolving Facebook video: " + ex.Message);
                }
            }
            return (realUrl, fileName);
        }
    }
}
