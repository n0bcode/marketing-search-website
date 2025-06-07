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
using Api.Repositories.IRepositories;
using Api.Models;

namespace Api.Services.VideoServices
{
    public class VideoProcessingService
    {
        private readonly IMemoryCache _cache;
        private readonly IUnitOfWork _unit;
        private readonly Automations.SeleniumManager _seleniumManager;
        public VideoProcessingService(IMemoryCache cache, IUnitOfWork unit, Automations.SeleniumManager seleniumManager)
        {
            _cache = cache;
            _unit = unit;
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
            var dbResult = await _unit.AnalysisLinks.GetAnalysisLinkOrNot(videoUrl);
            if (dbResult != null)
                return dbResult.ResultData;

            // 2. Kiểm tra cache
            if (string.IsNullOrWhiteSpace(languageCode))
                languageCode = "vi-VN"; // Mặc định là tiếng Việt nếu không có mã ngôn ngữ

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

                // 3. Lưu vào DB sau khi phân tích xong
                var result = new AnalysisLink
                {
                    LinkOrKeyword = videoUrl,
                    Platform = platform, // hoặc facebook, youtube, ...
                    ResultData = textContent // hoặc serialize object to JSON
                };
                await _unit.AnalysisLinks.AddAsync(result);

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
        public async Task<string> ExtractContentFromAudio(string audioUrl, string languageCode = "vi-VN", string platform = "null")
        {
            // 1. Kiểm tra DB trước
            var dbResult = await _unit.AnalysisLinks.GetAnalysisLinkOrNot(audioUrl);
            if (dbResult != null)
                return dbResult.ResultData;

            // 2. Kiểm tra cache
            if (string.IsNullOrWhiteSpace(languageCode))
                languageCode = "vi-VN"; // Mặc định là tiếng Việt nếu không có mã ngôn ngữ
            string tempAudioFilePath = Path.Combine(Path.GetTempPath(), "temp_audio.m4a"); // Tệp âm thanh tạm thời
            string convertedAudioFilePath = Path.Combine(Path.GetTempPath(), "temp_audio.wav"); // Tệp âm thanh đã chuyển đổi

            try
            {
                // Tải tệp âm thanh gốc về
                await DownloadFile(audioUrl, tempAudioFilePath);

                // Chuyển đổi âm thanh từ M4A sang WAV
                await ConvertAudioFormat(tempAudioFilePath, convertedAudioFilePath);

                var res = await ConvertAudioToText(convertedAudioFilePath, languageCode);

                // 3. Lưu vào DB sau khi phân tích xong
                var result = new AnalysisLink
                {
                    LinkOrKeyword = audioUrl,
                    Platform = platform, // hoặc facebook, youtube, ...
                    ResultData = res // hoặc serialize object to JSON
                };
                await _unit.AnalysisLinks.AddAsync(result);
                // Chuyển đổi âm thanh đã chuyển đổi sang văn bản
                return res;
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
                // Dọn dẹp các tệp tạm thời
                CleanUpFile(tempAudioFilePath);
                CleanUpFile(convertedAudioFilePath);
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
