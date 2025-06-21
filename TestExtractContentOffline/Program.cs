using System;
using System.IO;
using System.Net.Http;
using VideoLibrary; // Install-Package VideoLibrary
using Whisper.net; // Install-Package Whisper.net
using FFMpegCore; // Install-Package FFMpegCore
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Newtonsoft.Json.Linq;

namespace VideoSubtitleExtractor
{
    class Program
    {
        static async Task Main(string[] args)
        {
            bool loop = true;
            do
            {
                Console.WriteLine("Enter video/audio URL (YouTube, TikTok, Facebook, mp4, mp3, m4a...):");
                string mediaUrl = Console.ReadLine();

                string? fileName = null;

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

                if (string.IsNullOrWhiteSpace(fileName))
                {
                    fileName = GetFileNameFromUrl(mediaUrl);
                }

                string baseName = Path.GetFileNameWithoutExtension(fileName);
                string inputFile = fileName;
                string outputDir = "outputs";

                if (!Directory.Exists(outputDir))
                    Directory.CreateDirectory(outputDir);

                string audioFile = Path.Combine(outputDir, baseName + ".wav");
                string transcriptFile = Path.Combine(outputDir, baseName + ".txt");

                await DownloadFileWithHttpClientAsync(mediaUrl, inputFile);

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
                    {
                        throw new InvalidDataException("File is not a valid RIFF WAV file.");
                    }
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

        static bool IsYouTubeUrl(string url) => url.Contains("youtube.com", StringComparison.OrdinalIgnoreCase) || url.Contains("youtu.be", StringComparison.OrdinalIgnoreCase);

        static bool IsFacebookUrl(string url) => url.Contains("facebook.com", StringComparison.OrdinalIgnoreCase);

        static string GetFileNameFromUrl(string url)
        {
            Uri uri = new Uri(url);
            string name = Path.GetFileName(uri.LocalPath);

            if (string.IsNullOrWhiteSpace(name))
            {
                name = "downloaded_input";
            }

            string ext = Path.GetExtension(name);
            if (string.IsNullOrWhiteSpace(ext))
            {
                ext = ".mp4";
            }

            name = Path.GetFileNameWithoutExtension(name);
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                name = name.Replace(c, '_');
            }

            name += $"_{DateTime.Now:yyyyMMdd_HHmmss}";

            return name + ext;
        }

        static void DownloadVideoWithVideoLibrary(string url, string outputFile)
        {
            Console.WriteLine("Downloading video with VideoLibrary...");
            var youTube = YouTube.Default;
            var video = youTube.GetVideo(url);
            File.WriteAllBytes(outputFile, video.GetBytes());
        }

        static async Task DownloadFileWithHttpClientAsync(string url, string outputFile)
        {
            Console.WriteLine("Downloading file with HttpClient...");
            using var client = new HttpClient();
            using var response = await client.GetAsync(url);
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

            if (File.Exists(tempMp4))
                File.Delete(tempMp4);
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
