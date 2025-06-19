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

                string inputFile = "downloaded_input";
                string audioFile = "output.wav";
                string transcriptFile = "output.txt";

                if (IsYouTubeUrl(mediaUrl))
                {
                    inputFile += ".mp4";
                    DownloadVideoWithVideoLibrary(mediaUrl, inputFile);
                }
                else
                {
                    string extension = Path.GetExtension(mediaUrl)?.Split('?')[0] ?? ".mp4";
                    inputFile += extension;
                    DownloadFileWithHttpClient(mediaUrl, inputFile);
                }

                await ConvertToWavWithFFMpegCore(inputFile, audioFile);

                Console.WriteLine("Transcribing audio with Whisper.NET...");

                using var whisperFactory = WhisperFactory.FromPath("whisper-models/ggml-base.bin");
                using var processor = whisperFactory.CreateBuilder().WithLanguage("vi").Build();

                using (var fileStream = File.OpenRead(audioFile))
                {
                    // Validate WAV header
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

                Console.WriteLine("\n=== Continued ===\n");
            }
            while (loop);
        }

        static bool IsYouTubeUrl(string url)
        {
            return url.Contains("youtube.com", StringComparison.OrdinalIgnoreCase)
                || url.Contains("youtu.be", StringComparison.OrdinalIgnoreCase);
        }

        static void DownloadVideoWithVideoLibrary(string url, string outputFile)
        {
            Console.WriteLine("Downloading video with VideoLibrary...");
            var youTube = YouTube.Default;
            var video = youTube.GetVideo(url);
            File.WriteAllBytes(outputFile, video.GetBytes());
        }

        static void DownloadFileWithHttpClient(string url, string outputFile)
        {
            Console.WriteLine("Downloading file with HttpClient...");
            using (var client = new HttpClient())
            using (var response = client.GetAsync(url).Result)
            {
                response.EnsureSuccessStatusCode();
                using (var fs = new FileStream(outputFile, FileMode.Create, FileAccess.Write))
                {
                    response.Content.CopyToAsync(fs).Wait();
                }
            }
        }

        static async Task ConvertToWavWithFFMpegCore(string inputPath, string outputPath)
        {
            Console.WriteLine("Converting to WAV using FFMpegCore...");
            await FFMpegArguments
                .FromFileInput(inputPath)
                .OutputToFile(outputPath, true, options => options
                    .WithAudioCodec("pcm_s16le")
                    .WithCustomArgument("-ar 16000 -ac 1"))
                .ProcessAsynchronously();
        }
    }
}