using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api.Automations;
using Api.Models;
using Api.Repositories.IRepositories;
using Api.Services.AIServices.Gemini;
using Api.Services.VideoServices;
using Microsoft.AspNetCore.Mvc;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace Api.Controllers
{
    /// <summary>
    /// Controller for processing video-related operations, including downloading, content extraction, and AI analysis.
    /// </summary>
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class VideoProcessingController : ControllerBase
    {
        #region Fields

        private readonly Automations.SeleniumManager _seleniumManager;
        private readonly VideoProcessingService _videoProcessingService;
        private readonly GeminiAIService _geminiAIService;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="VideoProcessingController"/> class.
        /// </summary>
        /// <param name="geminiAIService">The Gemini AI service.</param>
        /// <param name="videoProcessingService">The video processing service.</param>
        /// <param name="seleniumManager">The Selenium manager for browser automation.</param>
        public VideoProcessingController(GeminiAIService geminiAIService, VideoProcessingService videoProcessingService, Automations.SeleniumManager seleniumManager)
        {
            _seleniumManager = seleniumManager;
            _videoProcessingService = videoProcessingService;
            _geminiAIService = geminiAIService;
        }

        #endregion

        #region Public Endpoints

        /// <summary>
        /// Lấy link tải về video TikTok.
        /// </summary>
        /// <param name="videoUrl">Link gốc bài viết có video trên Tiktok.</param>
        /// <returns>Một <see cref="IActionResult"/> chứa link tải về video TikTok.</returns>
        [HttpPost]
        public async Task<IActionResult> GetLinkDownloadTikTokVideo([FromQuery] string videoUrl)
        {
            if (string.IsNullOrEmpty(videoUrl) || !videoUrl.Contains("tiktok.com"))
            {
                return BadRequest("URL không hợp lệ. Vui lòng cung cấp một URL TikTok hợp lệ.");
            }

            var downloadUrlResponse = await _videoProcessingService.GetTikTokDownloadLink(videoUrl);
            if (downloadUrlResponse != null && downloadUrlResponse.Success && !string.IsNullOrEmpty(downloadUrlResponse.Data))
            {
                return Ok(new { DownloadUrl = downloadUrlResponse.Data });
            }

            return BadRequest(downloadUrlResponse?.Message ?? "Không thể lấy link tải về video TikTok.");
        }

        /// <summary>
        /// Trích xuất nội dung từ video TikTok.
        /// </summary>
        /// <param name="videoUrl">Link video TikTok.</param>
        /// <param name="languageCode">Ngôn ngữ để lấy nội dung từ video (VD: en-US, vi-VN).</param>
        /// <param name="isTransLinkToLinkDownload">Có cần chuyển đổi link thành link có thể download video? Mặc định là true.</param>
        /// <returns>Một <see cref="IActionResult"/> chứa nội dung văn bản được trích xuất từ video.</returns>
        [HttpPost]
        public async Task<IActionResult> ExtractContentFromLinkVideoTikTok([FromQuery] string videoUrl, [FromQuery] string languageCode, [FromQuery] bool isTransLinkToLinkDownload = true)
        {
            ResponseAPI<string> downloadLinkResponse = new();
            if (string.IsNullOrEmpty(videoUrl))
            {
                return BadRequest("URL không hợp lệ. Vui lòng cung cấp một URL video.");
            }

            string finalVideoUrl = videoUrl;
            if (isTransLinkToLinkDownload)
            {
                downloadLinkResponse = await _videoProcessingService.GetTikTokDownloadLink(videoUrl);
                if (!downloadLinkResponse.Success || string.IsNullOrEmpty(downloadLinkResponse.Data))
                {
                    return BadRequest(downloadLinkResponse?.Message ?? "Không thể lấy link tải về video TikTok.");
                }
                finalVideoUrl = downloadLinkResponse.Data;
            }

            try
            {
                var textContent = await _videoProcessingService.ExtractContentFromVideo(finalVideoUrl, languageCode, "tiktok");
                return Ok(new { TextContent = textContent });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Đã xảy ra lỗi trong quá trình trích xuất nội dung: {ex.Message}");
            }
        }

        /// <summary>
        /// Trích xuất nội dung từ video TikTok và phân tích bằng Gemini AI.
        /// </summary>
        /// <param name="videoUrl">Link video TikTok.</param>
        /// <param name="languageCode">Ngôn ngữ để lấy nội dung từ video (VD: en-US, vi-VN). Mặc định là vi-VN.</param>
        /// <param name="isTransLinkToLinkDownload">Có cần chuyển đổi link thành link có thể download video? Mặc định là true.</param>
        /// <returns>Một <see cref="IActionResult"/> chứa kết quả phân tích từ Gemini AI.</returns>
        [HttpPost]
        public async Task<IActionResult> ExtractContentFromLinkVideoTikTokAndAnalysis([FromQuery] string videoUrl, [FromQuery] string languageCode = "vi-VN", [FromQuery] bool isTransLinkToLinkDownload = true)
        {
            if (string.IsNullOrEmpty(videoUrl))
            {
                return BadRequest("URL không hợp lệ. Vui lòng cung cấp một URL video.");
            }

            string basedLink = videoUrl;
            string finalVideoUrl = videoUrl;

            if (isTransLinkToLinkDownload)
            {
                var downloadLinkResponse = await _seleniumManager.GetTikTokDownloadLink(videoUrl);
                if (!downloadLinkResponse.Success || string.IsNullOrEmpty(downloadLinkResponse.Data))
                {
                    return BadRequest(downloadLinkResponse?.Message ?? "Không thể lấy link tải về video TikTok.");
                }
                finalVideoUrl = downloadLinkResponse.Data;
            }

            try
            {
                var videoContent = await _videoProcessingService.ExtractContentFromVideo(finalVideoUrl, languageCode);

                var mediaAnalysisPrompt = $"Phân tích nội dung video từ link: {basedLink}. Nội dung video là: {videoContent}." +
                                      "\nNếu không phải nội dung video, hãy phân tích như một trang web thông thường.";
                var request = GeminiAIRequest.CreateWithContentMediaPrompt(linkMediaSocial: basedLink, contentMedia: videoContent);
                var response = await _geminiAIService.AnalyzeAsync(request);

                if (response == null || !response.Success || response.Data == null)
                {
                    return BadRequest(response?.Message ?? "Không thể phân tích nội dung video bằng Gemini AI.");
                }

                response.Data.Note = $"Nội dung video: {videoContent}";
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Đã xảy ra lỗi trong quá trình trích xuất và phân tích nội dung video: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy link tải về video Facebook.
        /// </summary>
        /// <param name="videoUrl">Link gốc bài viết có video trên Facebook.</param>
        /// <returns>Một <see cref="IActionResult"/> chứa link tải về video Facebook.</returns>
        [HttpPost]
        public async Task<IActionResult> GetLinkDownloadFacebookVideo([FromQuery] string videoUrl)
        {
            if (string.IsNullOrEmpty(videoUrl) || !videoUrl.Contains("facebook.com"))
            {
                return BadRequest("URL không hợp lệ. Vui lòng cung cấp một URL Facebook hợp lệ.");
            }

            var downloadResponse = await _videoProcessingService.GetFacebookDownloadLink(videoUrl);
            if (downloadResponse != null && downloadResponse.Success && !string.IsNullOrEmpty(downloadResponse.Data))
            {
                return Ok(new { DownloadUrl = downloadResponse.Data });
            }

            return BadRequest(downloadResponse?.Message ?? "Không thể lấy link tải về video Facebook.");
        }

        /// <summary>
        /// Trích xuất nội dung từ video Facebook.
        /// </summary>
        /// <param name="videoUrl">Link video Facebook.</param>
        /// <param name="languageCode">Ngôn ngữ để lấy nội dung từ video (VD: en-US, vi-VN).</param>
        /// <param name="isTransLinkToLinkDownload">Có chuyển đổi video gốc thành link tải được video? Mặc định là true.</param>
        /// <returns>Một <see cref="IActionResult"/> chứa nội dung văn bản được trích xuất từ video.</returns>
        [HttpPost]
        public async Task<IActionResult> ExtractContentFromLinkVideoFacebook([FromQuery] string videoUrl, [FromQuery] string languageCode = "vi-VN", [FromQuery] bool isTransLinkToLinkDownload = true)
        {
            ResponseAPI<string> downloadAudioLinkResponse = new();
            if (string.IsNullOrEmpty(videoUrl))
            {
                return BadRequest("URL không hợp lệ. Vui lòng cung cấp một URL video.");
            }

            string finalVideoUrl = videoUrl;
            if (isTransLinkToLinkDownload)
            {
                downloadAudioLinkResponse = await _videoProcessingService.GetFacebookDownloadLink(videoUrl);
                if (!downloadAudioLinkResponse.Success || string.IsNullOrEmpty(downloadAudioLinkResponse.Data))
                {
                    return BadRequest(downloadAudioLinkResponse?.Message ?? "Không thể lấy link tải về video Facebook.");
                }
                finalVideoUrl = downloadAudioLinkResponse.Data;
            }

            try
            {
                var textContent = await _videoProcessingService.ExtractContentFromAudio(finalVideoUrl, languageCode, "facebook");
                return Ok(new { TextContent = textContent });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Đã xảy ra lỗi trong quá trình trích xuất nội dung: {ex.Message}");
            }
        }

        /// <summary>
        /// Trích xuất nội dung từ video Facebook và phân tích bằng Gemini AI.
        /// </summary>
        /// <param name="videoUrl">Link video Facebook.</param>
        /// <param name="languageCode">Ngôn ngữ để lấy nội dung từ video (VD: en-US, vi-VN). Mặc định là vi-VN.</param>
        /// <param name="isTransLinkToLinkDownload">Có chuyển đổi video gốc thành link tải được video? Mặc định là true.</param>
        /// <returns>Một <see cref="IActionResult"/> chứa kết quả phân tích từ Gemini AI.</returns>
        [HttpPost]
        public async Task<IActionResult> ExtractContentFromLinkVideoFacebookAndAnalysis([FromQuery] string videoUrl, [FromQuery] string languageCode = "vi-VN", [FromQuery] bool isTransLinkToLinkDownload = true)
        {
            if (string.IsNullOrEmpty(videoUrl))
            {
                return BadRequest("URL không hợp lệ. Vui lòng cung cấp một URL video.");
            }

            string basedLink = videoUrl;
            string finalVideoUrl = videoUrl;

            if (isTransLinkToLinkDownload)
            {
                var downloadLinkResponse = await _seleniumManager.GetFacebookDownloadLink(videoUrl);
                if (!downloadLinkResponse.Success || string.IsNullOrEmpty(downloadLinkResponse.Data))
                {
                    return BadRequest(downloadLinkResponse?.Message ?? "Không thể lấy link tải về video Facebook.");
                }
                finalVideoUrl = downloadLinkResponse.Data;
            }

            try
            {
                var videoContent = await _videoProcessingService.ExtractContentFromAudio(finalVideoUrl, languageCode);

                var mediaAnalysisPrompt = $"Phân tích nội dung video từ link: {basedLink}. Nội dung video là: {videoContent}." +
                                      "\nNếu không phải nội dung video, hãy phân tích như một trang web thông thường.";
                var request = GeminiAIRequest.CreateWithContentMediaPrompt(linkMediaSocial: basedLink, contentMedia: videoContent);
                var response = await _geminiAIService.AnalyzeAsync(request);

                if (response == null || !response.Success || response.Data == null)
                {
                    return BadRequest(response?.Message ?? "Không thể phân tích nội dung video bằng Gemini AI.");
                }

                response.Data.Note = $"Nội dung video: {videoContent}";
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Đã xảy ra lỗi trong quá trình trích xuất và phân tích nội dung video: {ex.Message}");
            }
        }

        #endregion
    }
}