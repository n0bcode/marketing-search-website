using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Google.Cloud.Speech.V1;

namespace Api.Services.VideoServices
{
    public class VideoProcessingService
    {
        // Đường dẫn đến FFMpeg
        private const string ffmpegPath = @"D:\Download\ffmpeg-2025-05-15-git-12b853530a-essentials_build\bin\ffmpeg.exe";

        public async Task<string> ExtractContentFromVideo(string videoUrl, string languageCode)
        {
            string videoFilePath = Path.Combine(Path.GetTempPath(), "temp_video.mp4");
            string audioFilePath = Path.Combine(Path.GetTempPath(), "temp_audio.wav");
            string textContent;

            try
            {
                // Tải video từ URL xuống tạm thời
                await DownloadVideo(videoUrl, videoFilePath);

                // Trích xuất nội dung âm thanh thành văn bản
                await ExtractAudio(videoFilePath, audioFilePath);
                textContent = await ConvertAudioToText(audioFilePath, languageCode);

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
                // Xóa tệp tạm thời
                CleanUpTempFiles(videoFilePath, audioFilePath);
            }
        }

        private async Task DownloadVideo(string url, string outputPath)
        {
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                using (var fs = new FileStream(outputPath, FileMode.CreateNew))
                {
                    await response.Content.CopyToAsync(fs);
                }

                var fileInfo = new FileInfo(outputPath);
                if (fileInfo.Length == 0)
                {
                    throw new Exception("Tệp video tải xuống bị rỗng.");
                }
            }
        }

        private async Task ExtractAudio(string videoFilePath, string audioFilePath)
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = ffmpegPath,
                Arguments = $"-i \"{videoFilePath}\" -ar 16000 -ac 1 -q:a 0 -map a \"{audioFilePath}\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using (var process = new Process { StartInfo = processInfo })
            {
                process.Start();
                string output = await process.StandardOutput.ReadToEndAsync();
                string error = await process.StandardError.ReadToEndAsync();
                await process.WaitForExitAsync();

                if (process.ExitCode != 0)
                {
                    throw new Exception($"FFmpeg lỗi: {error}");
                }
            }
        }

        private async Task<string> ConvertAudioToText(string audioFilePath, string languageCode = "vi-VN")
        {
            var speech = SpeechClient.Create();
            var response = await speech.RecognizeAsync(new RecognitionConfig()
            {
                Encoding = RecognitionConfig.Types.AudioEncoding.Linear16,
                SampleRateHertz = 16000,
                LanguageCode = languageCode
            }, RecognitionAudio.FromFile(audioFilePath));

            return string.Join(" ", response.Results.Select(result => result.Alternatives[0].Transcript));
        }

        private void CleanUpTempFiles(string videoFilePath, string audioFilePath)
        {
            if (File.Exists(videoFilePath))
                File.Delete(videoFilePath);

            if (File.Exists(audioFilePath))
                File.Delete(audioFilePath);
        }
        private async Task DownloadAudio(string audioUrl, string audioFilePath)
        {
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(audioUrl);
                response.EnsureSuccessStatusCode();

                using (var fs = new FileStream(audioFilePath, FileMode.CreateNew))
                {
                    await response.Content.CopyToAsync(fs);
                }

                var fileInfo = new FileInfo(audioFilePath);
                if (fileInfo.Length == 0)
                {
                    throw new Exception("Tệp âm thanh tải xuống bị rỗng.");
                }
            }
        }
        public async Task<string> ExtractContentFromAudio(string audioUrl, string languageCode = "vi-VN")
        {
            string audioFilePath = Path.Combine(Path.GetTempPath(), "temp_audio.wav");
            string textContent;

            try
            {
                // Tải âm thanh từ URL xuống tạm thời
                await DownloadAudio(audioUrl, audioFilePath);

                // Trích xuất nội dung từ âm thanh
                textContent = await ConvertAudioToText(audioFilePath, languageCode);

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
                // Xóa tệp tạm thời
                if (File.Exists(audioFilePath))
                    File.Delete(audioFilePath);
            }
        }
    }
}