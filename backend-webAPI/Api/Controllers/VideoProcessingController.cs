using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api.Automations;
using Api.Repositories.IRepositories;
using Api.Services.AIServices.Gemini;
using Api.Services.VideoServices;
using Microsoft.AspNetCore.Mvc;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class VideoProcessingController : ControllerBase
    {
        private readonly Automations.SeleniumManager _seleniumManager;
        private readonly VideoProcessingService _videoProcessingService;
        private readonly GeminiAIService _geminiAIService;
        public VideoProcessingController(GeminiAIService geminiAIService)
        {
            _seleniumManager = new Automations.SeleniumManager(); // Khởi tạo SeleniumManager
            _videoProcessingService = new VideoProcessingService(); // Khởi tạo VideoProcessingService
            _geminiAIService = geminiAIService; // Khởi tạo GeminiAIService
        }
        #region [Public API]
        /// <summary>
        /// Lấy link tải về video TikTok
        /// </summary>
        /// <param name="videoUrl">Link gốc bài viết có video trên Tiktok</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> GetLinkDownloadTikTokVideo(string videoUrl)
        {
            // Kiểm tra URL có hợp lệ không
            if (string.IsNullOrEmpty(videoUrl) || !videoUrl.Contains("tiktok.com"))
            {
                return BadRequest("URL không hợp lệ.");
            }

            var downloadUrl = await _seleniumManager.GetTikTokDownloadLink(videoUrl);
            if (downloadUrl != null)
            {
                return Ok(new { DownloadUrl = downloadUrl });
            }

            return BadRequest("Không thể lấy link tải về.");
        }
        /// <summary>
        /// Trích xuất nội dung từ video TikTok
        /// </summary>
        /// <param name="videoUrl"> Link video tiktok </param>
        /// <param name="languageCode"> Ngôn ngữ để lấy nội dung từ video (VD: en-US)</param>
        /// <param name="isTransLinkToLinkDownload"> Có cần chuyển đổi link thành link có thể download video? </param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> ExtractContentFromLinkVideoTikTok(string videoUrl, string languageCode, bool isTransLinkToLinkDownload = true)
        {
            // Kiểm tra URL có hợp lệ không
            if (string.IsNullOrEmpty(videoUrl))
            {
                return BadRequest("URL không hợp lệ.");
            }
            if (isTransLinkToLinkDownload)
            {
                videoUrl = _seleniumManager.GetTikTokDownloadLink(videoUrl).Result?.Data ?? string.Empty;
                // Kiểm tra xem videoUrl có hợp lệ không
                if (string.IsNullOrEmpty(videoUrl))
                {
                    return BadRequest("Không thể lấy link tải về.");
                }
            }

            try
            {
                var textContent = await _videoProcessingService.ExtractContentFromVideo(videoUrl, languageCode);
                return Ok(new { TextContent = textContent });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Đã xảy ra lỗi: {ex.Message}");
            }
        }
        [HttpPost]
        public async Task<IActionResult> ExtractContentFromLinkVideoTikTokAndAnalysis(string videoUrl, string languageCode = "vi-VN", bool isTransLinkToLinkDownload = true)
        {
            // Kiểm tra URL có hợp lệ không
            if (string.IsNullOrEmpty(videoUrl))
            {
                return BadRequest("URL không hợp lệ.");
            }
            string basedLink = videoUrl;
            if (isTransLinkToLinkDownload)
            {
                videoUrl = _seleniumManager.GetTikTokDownloadLink(videoUrl).Result?.Data ?? string.Empty;
                // Kiểm tra xem videoUrl có hợp lệ không
                if (string.IsNullOrEmpty(videoUrl))
                {
                    return BadRequest("Không thể lấy link tải về.");
                }
            }

            try
            {
                var videoContent = await _videoProcessingService.ExtractContentFromVideo(videoUrl, languageCode);

                var mediaAnalysisPrompt = $"Phân tích nội dung video từ link: {basedLink}. Nội dung video là: {videoContent}." +
                                      "\nNếu không phải nội dung video, hãy phân tích như một trang web thông thường.";
                var request = GeminiAIRequest.CreateWithContentMediaPrompt(videoUrl, mediaAnalysisPrompt);
                var response = await _geminiAIService.AnalyzeAsync(request);
                if (response == null || !response.Success)
                {
                    return BadRequest("Không thể phân tích nội dung video.");
                }
                if (response.Data == null)
                {
                    return BadRequest("Phản hồi không xử lí thành công.");
                }
                response.Data.Note = $"Nội dung video: {videoContent}";
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Đã xảy ra lỗi: {ex.Message}");
            }
        }
        /// <summary>
        /// Lấy link tải về video Facebook
        /// </summary>
        /// <param name="videoUrl">Link bài viết gốc có video</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> GetLinkDownloadFacebookVideo(string videoUrl)
        {
            // Kiểm tra URL có hợp lệ không
            if (string.IsNullOrEmpty(videoUrl) || !videoUrl.Contains("facebook.com"))
            {
                return BadRequest("URL không hợp lệ.");
            }

            var downloadResponse = await _seleniumManager.GetFacebookDownloadLink(videoUrl);
            if (downloadResponse != null)
            {
                return Ok(downloadResponse);
            }

            return BadRequest("Không thể lấy link tải về.");
        }
        /// <summary>
        /// Trích xuất nội dung từ video Facebook
        /// </summary>
        /// <param name="audioUrl"> Link trên Facebook có video</param>
        /// <param name="languageCode">Ngôn ngữ xác định để trích xuất nội dung từ video</param>
        /// <param name="isTransLinkToLinkDownload">Có chuyển đổi video gốc thành link tải được video?</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> ExtractContentFromLinkVideoFacebook(string audioUrl, string languageCode = "vi-VN", bool isTransLinkToLinkDownload = true)
        {
            // Kiểm tra URL có hợp lệ không
            if (string.IsNullOrEmpty(audioUrl))
            {
                return BadRequest("URL không hợp lệ.");
            }
            if (isTransLinkToLinkDownload)
            {
                audioUrl = _seleniumManager.GetFacebookDownloadLink(audioUrl).Result?.Data ?? string.Empty;
                // Kiểm tra xem audioUrl có hợp lệ không
                if (string.IsNullOrEmpty(audioUrl))
                {
                    return BadRequest("Không thể lấy link tải về.");
                }
            }

            try
            {
                var textContent = await _videoProcessingService.ExtractContentFromAudio(audioUrl, languageCode);
                return Ok(new { TextContent = textContent });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Đã xảy ra lỗi: {ex.Message}");
            }
        }

        /// <summary>
        ///     
        /// </summary>
        /// <param name="audioUrl"></param>
        /// <param name="languageCode"></param>
        /// <param name="isTransLinkToLinkDownload"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> ExtractContentFromLinkVideoFacebookAndAnalysis(string audioUrl, string languageCode = "vi-VN", bool isTransLinkToLinkDownload = true)
        {
            // Kiểm tra URL có hợp lệ không
            if (string.IsNullOrEmpty(audioUrl))
            {
                return BadRequest("URL không hợp lệ.");
            }
            string basedLink = audioUrl;
            if (isTransLinkToLinkDownload)
            {
                audioUrl = _seleniumManager.GetFacebookDownloadLink(audioUrl).Result?.Data ?? string.Empty;
                // Kiểm tra xem audioUrl có hợp lệ không
                if (string.IsNullOrEmpty(audioUrl))
                {
                    return BadRequest("Không thể lấy link tải về.");
                }
            }

            try
            {
                var videoContent = await _videoProcessingService.ExtractContentFromAudio(audioUrl, languageCode);

                var mediaAnalysisPrompt = $"Phân tích nội dung video từ link: {basedLink}. Nội dung video là: {videoContent}." +
                                      "\nNếu không phải nội dung video, hãy phân tích như một trang web thông thường.";
                var request = GeminiAIRequest.CreateWithContentMediaPrompt(linkMediaSocial: audioUrl, mediaAnalysisPrompt);
                var response = await _geminiAIService.AnalyzeAsync(request);
                if (response == null || !response.Success)
                {
                    return BadRequest("Không thể phân tích nội dung video.");
                }
                if (response.Data == null)
                {
                    return BadRequest("Phản hồi không xử lí thành công.");
                }
                response.Data.Note = $"Nội dung video: {videoContent}";
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Đã xảy ra lỗi: {ex.Message}");
            }
        }
        #endregion

        #region [Private Methods]

        #endregion
    }
}