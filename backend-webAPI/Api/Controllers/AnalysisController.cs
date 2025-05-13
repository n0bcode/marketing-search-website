using Api.Constants;
using Api.Models;
using Api.Repositories.IRepositories;
using Api.Services.AIServices.Gemini;
using Api.Services.SearchServices;
using Api.Services.SearchServices.Google;
using Api.Services.SearchServices.Twitter;
using Api.Services.SearchServices.Twitter.TwitterRequests;
using Api.Utils;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text;

namespace Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AnalysisController : ControllerBase
    {
        private readonly GoogleSearchService _searchService;
        private readonly TwitterSearchService _searchServiceTwitter;
        private readonly GeminiAIService _geminiService;
        private readonly IUnitOfWork _unit;
        private readonly ILogger<SearchController> _logger; // Thêm logger

        public AnalysisController(GoogleSearchService searchService, TwitterSearchService searchServiceTwitter, GeminiAIService geminiAIService, IUnitOfWork unitOfWork, ILogger<SearchController> logger)
        {
            _searchService = searchService;
            _searchServiceTwitter = searchServiceTwitter;
            _geminiService = geminiAIService;
            _unit = unitOfWork;
            _logger = logger;
        }
        [HttpGet]
        public IActionResult GetGood()
        {
            return Ok("Good@");
        }
        [ProducesResponseType(typeof(GeminiAIResponse), 200)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [HttpPost]
        public async Task<IActionResult> SearchGoogleAndAnalysis(GoogleRequest query)
        {
            try
            {
                // 1. Tìm kiếm
                var googleResults = await _searchService.SearchAsync(query);

                var responseAddKey = await _unit.Keywords.AddKeywordAndGetIdAsync(new KeywordModel()
                {
                    Keyword = query.q,
                    Source = $"Google-{query.type}",
                });

                // 2. Chuẩn bị dữ liệu cho phân tích (Chỉ truyền các thông tin cần thiết)
                string analysisInput = $"Số liệu tìm kiếm phía Google: {GetAnalysisInput(query.FilterGoogleResponse(googleResults))}";

                // 3. Phân tích
                var analysisResult = await _geminiService.AnalyzeAsync(new GeminiRequest(query.q, analysisInput));

                // 4. Xử lý kết quả
                if (analysisResult == null)
                {
                    _logger.LogWarning("Không tìm thấy kết quả phân tích cho truy vấn: {Query}", query.q);
                    return NotFound(ProblemDetailsFactory.CreateProblemDetails(HttpContext, statusCode: 404, title: "Không tìm thấy kết quả phân tích."));
                }

                // var responseAddGeminiKey = await _unit.GeminiAIResponses.AddGeminiAIResponseConfigIdAsync(analysisResult);


                analysisResult.KeywordId = responseAddKey.Data!; // Gán ID từ bảng Keywords vào kết quả phân tích
                DataSaver.SaveData(analysisResult, $"Gemini-{query.type}", responseAddKey.Data!);
                DataSaver.SaveData(googleResults!, $"Google-{query.type}", responseAddKey.Data!);
                // Kết hợp kết quả tìm kiếm Google vào kết quả phân tích
                if (googleResults != null)
                {
                    analysisResult.GeneralSearchResults.AddRange(googleResults.ToGeneralSearchResults());
                }
                analysisResult.SiteSearch = query.site;

                return Ok(analysisResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Đã xảy ra lỗi trong quá trình tìm kiếm và phân tích Google cho truy vấn: {Query}", query.q);
                return StatusCode(500, ProblemDetailsFactory.CreateProblemDetails(HttpContext, statusCode: 500, title: "Lỗi máy chủ nội bộ.", detail: "Đã xảy ra lỗi không mong muốn."));
            }

        }
        [ProducesResponseType(typeof(GeminiAIResponse), 200)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [HttpPost]
        public async Task<IActionResult> SearchTwitterAndAnalysis(TwitterSearchTweetRequest twitterRequest)
        {
            try
            {
                // 1. Tìm kiếm
                var resultTwitter = await _searchServiceTwitter.SearchAsync(twitterRequest);

                var responseAddKey = await _unit.Keywords.AddKeywordAndGetIdAsync(new KeywordModel()
                {
                    Keyword = twitterRequest.Query,
                    Source = $"Twitter-Tweet",
                });
                // 2. Chuẩn bị dữ liệu cho phân tích (Chỉ truyền các thông tin cần thiết)
                string analysisInput = $"Số liệu tìm kiếm phía Twitter: {GetAnalysisInput(resultTwitter)}";

                // 3. Phân tích
                var analysisResult = await _geminiService.AnalyzeAsync(new GeminiRequest(twitterRequest.Query, analysisInput));

                // 4. Xử lý kết quả
                if (analysisResult == null)
                {
                    _logger.LogWarning("Không tìm thấy kết quả phân tích cho truy vấn: {Query}", twitterRequest.Query);
                    return NotFound(ProblemDetailsFactory.CreateProblemDetails(HttpContext, statusCode: 404, title: "Không tìm thấy kết quả phân tích."));
                }

                analysisResult.KeywordId = responseAddKey.Data!; // Gán ID từ bảng Keywords vào kết quả phân tích
                DataSaver.SaveData(analysisResult, $"Gemini-Tweet", responseAddKey.Data!);
                DataSaver.SaveData(resultTwitter!, $"Twitter-Tweet", responseAddKey.Data!);

                // Kết hợp kết quả tìm kiếm Twitter vào kết quả phân tích
                if (resultTwitter != null)
                {
                    analysisResult.GeneralSearchResults.AddRange(resultTwitter.ToGeneralSearchResults());
                }

                return Ok(analysisResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Đã xảy ra lỗi trong quá trình tìm kiếm và phân tích Twitter cho truy vấn: {Query}", twitterRequest.Query);
                return StatusCode(500, ProblemDetailsFactory.CreateProblemDetails(HttpContext, statusCode: 500, title: "Lỗi máy chủ nội bộ.", detail: "Đã xảy ra lỗi không mong muốn."));
            }
        }
        [ProducesResponseType(typeof(GeminiAIResponse), 200)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [HttpPost]
        public async Task<IActionResult> SearchAllPlatformAndAnalysis(AllRequestPlatform request)
        {
            try
            {
                // 1. Tìm kiếm trên Google
                var googleResults = await _searchService.SearchAsync(request.GoogleRequest);
                var responseAddKeyGoogle = await _unit.Keywords.AddKeywordAndGetIdAsync(new KeywordModel()
                {
                    Keyword = request.GoogleRequest.q,
                    Source = $"Google-{request.GoogleRequest.type}",
                });

                // 2. Tìm kiếm trên Twitter
                var resultTwitter = await _searchServiceTwitter.SearchAsync(request.TwitterSearchTweetRequest);
                var responseAddKeyTwitter = await _unit.Keywords.AddKeywordAndGetIdAsync(new KeywordModel()
                {
                    Keyword = request.TwitterSearchTweetRequest.Query,
                    Source = $"Twitter-Tweet",
                });

                // 3. Chuẩn bị dữ liệu cho phân tích
                string googleAnalysisInput = $"Số liệu tìm kiếm phía Google: {GetAnalysisInput(googleResults!)}";
                string twitterAnalysisInput = $"Số liệu tìm kiếm phía Twitter: {GetAnalysisInput(resultTwitter)}";

                // 4. Phân tích
                var googleAnalysisResult = await _geminiService.AnalyzeAsync(new GeminiRequest(request.GoogleRequest.q, googleAnalysisInput));
                var twitterAnalysisResult = await _geminiService.AnalyzeAsync(new GeminiRequest(request.TwitterSearchTweetRequest.Query, twitterAnalysisInput));

                // 5. Xử lý kết quả Google
                if (googleAnalysisResult == null)
                {
                    _logger.LogWarning("Không tìm thấy kết quả phân tích cho truy vấn Google: {Query}", request.GoogleRequest.q);
                    return NotFound(ProblemDetailsFactory.CreateProblemDetails(HttpContext, statusCode: 404, title: "Không tìm thấy kết quả phân tích Google."));
                }

                // 6. Xử lý kết quả Twitter
                if (twitterAnalysisResult == null)
                {
                    _logger.LogWarning("Không tìm thấy kết quả phân tích cho truy vấn Twitter: {Query}", request.TwitterSearchTweetRequest.Query);
                    return NotFound(ProblemDetailsFactory.CreateProblemDetails(HttpContext, statusCode: 404, title: "Không tìm thấy kết quả phân tích Twitter."));
                }

                // 7. Gán ID từ bảng Keywords vào kết quả phân tích
                googleAnalysisResult.KeywordId = responseAddKeyGoogle.Data!;
                twitterAnalysisResult.KeywordId = responseAddKeyTwitter.Data!;

                // 8. Lưu dữ liệu phân tích
                DataSaver.SaveData(googleAnalysisResult, $"Gemini-Google", responseAddKeyGoogle.Data!);
                DataSaver.SaveData(twitterAnalysisResult, $"Gemini-Twitter", responseAddKeyTwitter.Data!);

                // 9. Kết hợp kết quả tìm kiếm vào kết quả phân tích
                if (googleResults != null)
                {
                    googleAnalysisResult.GeneralSearchResults.AddRange(googleResults.ToGeneralSearchResults());
                }

                if (resultTwitter != null)
                {
                    twitterAnalysisResult.GeneralSearchResults.AddRange(resultTwitter.ToGeneralSearchResults());
                }

                // 10. Trả về kết quả phân tích
                return Ok(new
                {
                    GoogleAnalysisResult = googleAnalysisResult,
                    TwitterAnalysisResult = twitterAnalysisResult
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Đã xảy ra lỗi trong quá trình tìm kiếm và phân tích trên cả hai nền tảng.");
                return StatusCode(500, ProblemDetailsFactory.CreateProblemDetails(HttpContext, statusCode: 500, title: "Lỗi máy chủ nội bộ.", detail: "Đã xảy ra lỗi không mong muốn."));
            }
        }

        [ProducesResponseType(typeof(GeminiAIResponse), 200)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [HttpPost]
        public async Task<IActionResult> AnalysisLink(string? link)
        {
            if (link == null) return StatusCode(500);

            AnalysisLink? analysisLinkOrNot = await _unit.AnalysisLinks.GetAnalysisLinkOrNot(link);

            if (analysisLinkOrNot == null || String.IsNullOrEmpty(analysisLinkOrNot.AnalysisText))
            {
                GeminiRequest geminiRequest = new GeminiRequest(link);

                var response = await _geminiService.AnalyzeAsync(geminiRequest);

                var analysisLink = await _unit.AnalysisLinks.AddOrUpdateText(new AnalysisLink() { Link = link, AnalysisText = String.Join("", response.Candidates[0].Content.Parts.Select(x => x.Text)) });
                analysisLinkOrNot = analysisLink;
            }

            return Ok(analysisLinkOrNot);
        }

        [HttpGet("{idKeyword}")]
        public IActionResult GetOldAnalysis(string idKeyword)
        {
            var response = DataSaver.GetFromLog(idKeyword);
            return Ok(response);
        }
        #region [PRIVATE METHOD]

        private string GetAnalysisInput(GoogleResponse googleResults)
        {
            if (googleResults == null || googleResults.Organic == null || !googleResults.Organic.Any())
            {
                return "Không có kết quả tìm kiếm Google.";
            }

            // Sử dụng StringBuilder để xây dựng chuỗi một cách hiệu quả
            var sb = new StringBuilder();

            // Thêm tiêu đề để AI biết cấu trúc dữ liệu
            sb.AppendLine("--- Kết quả tìm kiếm Google ---");

            foreach (var result in googleResults.Organic)
            {
                // Format dữ liệu theo kiểu key-value để dễ phân tích
                sb.AppendLine("--- Kết quả ---");
                sb.AppendLine($"Tiêu đề: {result.Title}");
                sb.AppendLine($"Ngày: {result.Date}");
                sb.AppendLine($"Vị trí: {result.Position}");
                sb.AppendLine($"Liên kết: {result.Link}");
                sb.AppendLine($"Đoạn trích: {result.Snippet}");
                sb.AppendLine(); // Thêm dòng trống để phân tách các kết quả
            }

            // Thêm chú thích cuối để đánh dấu kết thúc dữ liệu
            sb.AppendLine("--- Kết thúc kết quả tìm kiếm ---");

            return sb.ToString();
        }

        private string GetAnalysisInput(TwitterResponse twitterResults)
        {
            if (twitterResults == null || twitterResults.Data == null)
            {
                return "Không có kết quả tìm kiếm Twitter.";
            }

            // Tạo một chuỗi chứa thông tin tóm tắt từ kết quả tìm kiếm Twitter
            return $"Tổng số kết quả: {twitterResults.Data.Count}"; // Ví dụ đơn giản
                                                                    // Bạn có thể thêm thông tin khác, chẳng hạn như tiêu đề và đoạn trích của các kết quả hàng đầu
        }
        #endregion
    }
}
