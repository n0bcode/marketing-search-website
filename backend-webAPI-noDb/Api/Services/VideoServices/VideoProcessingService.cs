using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using FFMpegCore;
using FFMpegCore.Pipes;
using Microsoft.Extensions.Caching.Memory;
using Api.Models;
using Api.Services.VideoServices.Sub;

namespace Api.Services.VideoServices
{
    public class VideoProcessingService
    {
        private readonly IMemoryCache _cache;
        private readonly Automations.SeleniumManager _seleniumManager;
        public VideoProcessingService(IMemoryCache cache, Automations.SeleniumManager seleniumManager)
        {
            _cache = cache;
            _seleniumManager = seleniumManager;
        }
        public async Task<ResponseAPI<string>> GetTikTokDownloadLink(string videoUrl)
        {
            return await _seleniumManager.GetTikTokDownloadLink(videoUrl);
        }

        public async Task<ResponseAPI<string>> GetFacebookDownloadLink(string videoUrl)
        {
            return await _seleniumManager.GetFacebookDownloadLink(videoUrl);
        }

        public async Task<string> ExtractContentFromVideo(string videoUrl, string languageCode, string platform = "null")
        {
            // 1. Kiểm tra DB trước
            var dbResult = new AnalysisLink();
            if (dbResult != null)
                return dbResult.ResultData;

            // 2. Kiểm tra cache
            if (string.IsNullOrWhiteSpace(languageCode))
                languageCode = "vi"; // Whisper dùng "vi" thay vì "vi-VN"

            string cacheKey = GenerateCacheKey(videoUrl, languageCode);
            if (_cache.TryGetValue(cacheKey, out string cachedResult))
            {
                return cachedResult;
            }

            string videoFilePath = Path.Combine(Path.GetTempPath(), "temp_video.mp4");
            string audioFilePath = Path.Combine(Path.GetTempPath(), "temp_audio.wav");
            string transcriptFilePath = Path.Combine(Path.GetTempPath(), "temp_transcript.txt");

            try
            {
                await DownloadFile(videoUrl, videoFilePath);
                await AudioConverter.ConvertToWavWithFFMpegCore(videoFilePath, audioFilePath);
                await WhisperTranscriber.TranscribeAsync(audioFilePath, transcriptFilePath);

                string textContent = File.ReadAllText(transcriptFilePath);

                if (string.IsNullOrWhiteSpace(textContent))
                    throw new Exception("Không thể trích xuất nội dung từ video.");

                // 3. Lưu vào DB sau khi phân tích xong
                var result = new AnalysisLink
                {
                    LinkOrKeyword = videoUrl,
                    Platform = platform,
                    ResultData = textContent
                };

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
                CleanUpTempFiles(videoFilePath, audioFilePath, transcriptFilePath);
            }
        }

        public async Task<string> ExtractContentFromAudio(string audioUrl, string languageCode = "vi", string platform = "null")
        {
            // 1. Kiểm tra DB trước
            var dbResult = new AnalysisLink();
            if (dbResult != null)
                return dbResult.ResultData;

            if (string.IsNullOrWhiteSpace(languageCode))
                languageCode = "vi";

            string tempAudioFilePath = Path.Combine(Path.GetTempPath(), "temp_audio.m4a");
            string convertedAudioFilePath = Path.Combine(Path.GetTempPath(), "temp_audio.wav");
            string transcriptFilePath = Path.Combine(Path.GetTempPath(), "temp_transcript.txt");

            try
            {
                await DownloadFile(audioUrl, tempAudioFilePath);
                await AudioConverter.ConvertToWavWithFFMpegCore(tempAudioFilePath, convertedAudioFilePath);
                await WhisperTranscriber.TranscribeAsync(convertedAudioFilePath, transcriptFilePath);

                string textContent = File.ReadAllText(transcriptFilePath);

                var result = new AnalysisLink
                {
                    LinkOrKeyword = audioUrl,
                    Platform = platform,
                    ResultData = textContent
                };

                return textContent;
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
                CleanUpFile(tempAudioFilePath);
                CleanUpFile(convertedAudioFilePath);
                CleanUpFile(transcriptFilePath);
            }
        }
        // Phương thức để chuyển đổi định dạng âm thanh
        private static async Task ConvertAudioFormat(string inputFilePath, string outputFilePath)
        {
            await FFMpegArguments
                .FromFileInput(inputFilePath)
                .OutputToFile(outputFilePath, true, options => options
                    .WithAudioCodec("pcm_s16le")
                    .WithCustomArgument("-ar 16000 -ac 1"))
                .ProcessAsynchronously();
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
