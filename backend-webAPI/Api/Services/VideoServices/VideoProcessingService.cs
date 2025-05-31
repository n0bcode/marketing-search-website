using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Google.Cloud.Speech.V1;
using FFMpegCore;
using FFMpegCore.Pipes;
using Microsoft.Extensions.Caching.Memory;

namespace Api.Services.VideoServices
{
    public class VideoProcessingService
    {
        private readonly IMemoryCache _cache;

        public VideoProcessingService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public async Task<string> ExtractContentFromVideo(string videoUrl, string languageCode)
        {
            string cacheKey = GenerateCacheKey(videoUrl, languageCode);
            if (_cache.TryGetValue(cacheKey, out string cachedResult))
            {
                return cachedResult;
            }

            string videoFilePath = Path.Combine(Path.GetTempPath(), "temp_video.mp4");
            string audioFilePath = Path.Combine(Path.GetTempPath(), "temp_audio.wav");

            try
            {
                await DownloadFile(videoUrl, videoFilePath);
                await ExtractAudio(videoFilePath, audioFilePath);
                string textContent = await ConvertAudioToText(audioFilePath, languageCode);

                if (string.IsNullOrWhiteSpace(textContent))
                    throw new Exception("Không thể trích xuất nội dung từ video.");

                _cache.Set(cacheKey, textContent, TimeSpan.FromHours(12));
                return textContent;
            }
            catch (HttpRequestException httpEx)
            {
                throw new Exception($"Lỗi khi tải video: {httpEx.Message}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Đã xảy ra lỗi: {ex.Message}");
            }
            finally
            {
                CleanUpTempFiles(videoFilePath, audioFilePath);
            }
        }

        public async Task<string> ExtractContentFromAudio(string audioUrl, string languageCode = "vi-VN")
        {
            string audioFilePath = Path.Combine(Path.GetTempPath(), "temp_audio.wav");
            try
            {
                await DownloadFile(audioUrl, audioFilePath);
                return await ConvertAudioToText(audioFilePath, languageCode);
            }
            catch (HttpRequestException httpEx)
            {
                throw new Exception($"Lỗi khi tải âm thanh: {httpEx.Message}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Đã xảy ra lỗi: {ex.Message}");
            }
            finally
            {
                CleanUpFile(audioFilePath);
            }
        }

        private static string GenerateCacheKey(string url, string languageCode) =>
            $"video_analysis_{url}_{languageCode}";

        private static async Task DownloadFile(string url, string outputPath)
        {
            using var client = new HttpClient();
            using var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            using var fs = new FileStream(outputPath, FileMode.CreateNew);
            await response.Content.CopyToAsync(fs);

            if (new FileInfo(outputPath).Length == 0)
                throw new Exception("Tệp tải xuống bị rỗng.");
        }

        private static async Task ExtractAudio(string videoFilePath, string audioFilePath)
        {
            await FFMpegArguments
                .FromFileInput(videoFilePath)
                .OutputToFile(audioFilePath, true, options => options
                    .WithAudioCodec("pcm_s16le")
                    .WithCustomArgument("-ar 16000 -ac 1"))
                .ProcessAsynchronously();
        }

        private static async Task<string> ConvertAudioToText(string audioFilePath, string languageCode = "vi-VN")
        {
            var speech = SpeechClient.Create();
            var response = await speech.RecognizeAsync(new RecognitionConfig
            {
                Encoding = RecognitionConfig.Types.AudioEncoding.Linear16,
                SampleRateHertz = 16000,
                LanguageCode = languageCode
            }, RecognitionAudio.FromFile(audioFilePath));

            return string.Join(" ", response.Results.Select(result => result.Alternatives[0].Transcript));
        }

        private static void CleanUpTempFiles(params string[] filePaths)
        {
            foreach (var filePath in filePaths)
            {
                CleanUpFile(filePath);
            }
        }

        private static void CleanUpFile(string filePath)
        {
            if (File.Exists(filePath))
                File.Delete(filePath);
        }

        public async Task<string> ExtractContentFromAudioUrl(string audioUrl, string languageCode = "vi-VN")
        {
            using var httpClient = new HttpClient();
            using var audioStream = await httpClient.GetStreamAsync(audioUrl);
            using var outputStream = new MemoryStream();

            await FFMpegArguments
                .FromPipeInput(new StreamPipeSource(audioStream))
                .OutputToPipe(new StreamPipeSink(outputStream), options => options
                    .WithAudioCodec("pcm_s16le")
                    .WithCustomArgument("-ar 16000 -ac 1"))
                .ProcessAsynchronously();

            outputStream.Position = 0;
            // Gọi Google Speech API với outputStream ở đây

            return "text result"; // Cần hoàn thành phương thức này
        }
    }
}
