using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Google.Cloud.Speech.V1;
using FFMpegCore;
using FFMpegCore.Pipes;

namespace Api.Services.VideoServices
{
    public class VideoProcessingService
    {
        // Đường dẫn đến FFMpeg
        private static string ffmpegPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"ffmpeg.exe");

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

        public async Task<string> ExtractContentFromAudioUrl(string audioUrl, string languageCode = "vi-VN")
        {
            using var httpClient = new HttpClient();
            using var audioStream = await httpClient.GetStreamAsync(audioUrl);

            // Chuyển đổi stream sang wav bằng FFMpegCore (in-memory)
            using var outputStream = new MemoryStream();
            await FFMpegArguments
                .FromPipeInput(new StreamPipeSource(audioStream))
                .OutputToPipe(new StreamPipeSink(outputStream), options => options
                    .WithAudioCodec("pcm_s16le")
                    .WithCustomArgument("-ar 16000 -ac 1"))
                .ProcessAsynchronously();

            outputStream.Position = 0;

            // Gửi outputStream vào Google Speech API (nếu API hỗ trợ stream)
            // Nếu không, lưu tạm vào file hệ thống tạm thời (Path.GetTempFileName()), nhưng nên xóa ngay sau khi dùng

            // ... gọi Google Speech API với outputStream ...

            return "text result";
        }
        #region [PRIVATE METHOD]

        private static async Task DownloadVideo(string url, string outputPath)
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

        private static async Task ExtractAudio(string videoFilePath, string audioFilePath)
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

        private static async Task<string> ConvertAudioToText(string audioFilePath, string languageCode = "vi-VN")
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

        private static void CleanUpTempFiles(string videoFilePath, string audioFilePath)
        {
            if (File.Exists(videoFilePath))
                File.Delete(videoFilePath);

            if (File.Exists(audioFilePath))
                File.Delete(audioFilePath);
        }
        private static async Task DownloadAudio(string audioUrl, string audioFilePath)
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
        #endregion
    }
}