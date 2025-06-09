using Api.Constants;
using Api.Models;
using Api.Repositories.IRepositories;
using Api.Repositories.MongoDb;
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
        private readonly IUnitOfWorkMongo _unitMongo;
        private readonly ILogger<SearchController> _logger; // Thêm logger

        public AnalysisController(GoogleSearchService searchService,
                                  TwitterSearchService searchServiceTwitter,
                                  GeminiAIService geminiAIService,
                                  IUnitOfWork unitOfWork,
                                  IUnitOfWorkMongo unitOfWorkMongo,
                                  ILogger<SearchController> logger)
        {
            _searchService = searchService;
            _searchServiceTwitter = searchServiceTwitter;
            _geminiService = geminiAIService;
            _unit = unitOfWork;
            _unitMongo = unitOfWorkMongo;
            _logger = logger;
        }
        [HttpGet]
        public IActionResult GetGood()
        {
            return Ok("Good@");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        /// <param name="idTokenGoogleChange"></param>
        /// <param name="idTokenGeminiChange"></param>
        /// <remarks>
        /// {
        ///   "q": "Cảm giác mệt và nóng người thì cần làm gì",
        ///   "gl": "VN",
        ///   "location": "Hà Nội",
        ///   "hl": "vi",
        ///   "tbs": "qdr:d",
        ///   "num": 10,
        ///   "type": "text",
        ///   "engine": "search",
        ///   "as_epq": "",
        ///   "as_oq": "",
        ///   "as_eq": "",
        ///   "as_sitesearch": ""
        /// }
        /// </remarks>
        /// <returns></returns>
        [ProducesResponseType(typeof(GeminiAIResponse), 200)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [HttpPost]
        public async Task<IActionResult> SearchGoogleAndAnalysis([FromBody] GoogleRequest query, [FromQuery] string? idTokenGoogleChange, [FromQuery] string? idTokenGeminiChange)
        {
            ResponseAPI<GeminiAIResponse>? response = new();
            try
            {
                // 1. Tìm kiếm
                GoogleResponse? googleResults = new();
                if (string.IsNullOrEmpty(idTokenGoogleChange))
                {
                    googleResults = await _searchService.SearchAsync(query);
                }
                else googleResults = await _searchService.SearchWithUserTokenCofigAsync(query, idTokenGoogleChange);


                // Kiểm tra kết quả tìm kiếm
                if (googleResults == null || googleResults.Organic == null || !googleResults.Organic.Any())
                {
                    _logger.LogWarning("Không có kết quả tìm kiếm Google cho truy vấn: {Query}", query.q);
                    throw new Exception("Không có kết quả tìm kiếm Google cho truy vấn: " + query.q);
                }

                var responseAddKey = await _unitMongo.Keywords.AddKeywordAndGetIdAsync(new KeywordModel()
                {
                    Keyword = query.q,
                    Source = $"Google-{query.type}",
                });

                // 2. Chuẩn bị dữ liệu cho phân tích
                string analysisInput = $"Số liệu tìm kiếm phía Google: {GetAnalysisInput(googleResults)}";

                // 3. Phân tích
                if (string.IsNullOrEmpty(idTokenGeminiChange))
                {
                    response = await _geminiService.AnalyzeAsync(GeminiAIRequest.CreateWithQueryAndPrompt(query.q, analysisInput));
                }
                else response = await _geminiService.AnalyzeWithTokenUserConfigAsync(GeminiAIRequest.CreateWithQueryAndPrompt(query.q, analysisInput), idTokenGeminiChange);
                // 4. Xử lý kết quả
                if (response == null)
                {
                    _logger.LogWarning("Không tìm thấy kết quả phân tích cho truy vấn: {Query}", query.q);
                    throw new Exception("Không tìm thấy kết quả phân tích cho truy vấn: " + query.q);
                }

                if (response.Data == null)
                {
                    _logger.LogWarning("Không có dữ liệu phân tích cho truy vấn: {Query}", query.q);
                    throw new Exception("Không có dữ liệu phân tích cho truy vấn: " + query.q);
                }

                response.Data.KeywordId = responseAddKey.Data!; // Gán ID từ bảng Keywords vào kết quả phân tích
                DataSaver.SaveData(response, $"Gemini-{query.type}", responseAddKey.Data!);
                DataSaver.SaveData(googleResults!, $"Google-{query.type}", responseAddKey.Data!);

                // Kết hợp kết quả tìm kiếm Google vào kết quả phân tích
                response.Data.GeneralSearchResults.AddRange(googleResults.ToGeneralSearchResults());
                response.Data.SiteSearch = query.as_sitesearch;

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Đã xảy ra lỗi trong quá trình tìm kiếm và phân tích Google cho truy vấn: {Query}", query.q);
                return BadRequest(response);
            }
        }
        [ProducesResponseType(typeof(GeminiAIResponse), 200)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [HttpPost]
        public async Task<IActionResult> AnalysisLink(string? link, string? idTokenChange)
        {
            if (link == null) return StatusCode(500);

            AnalysisLink? analysisLinkOrNot = await _unitMongo.AnalysisLinks.GetAnalysisLinkOrNot(link);

            if (analysisLinkOrNot == null || String.IsNullOrEmpty(analysisLinkOrNot.AnalysisText))
            {
                GeminiAIRequest geminiRequest = GeminiAIRequest.CreateWithPrompt(link, false);

                ResponseAPI<GeminiAIResponse>? response = new();
                if (string.IsNullOrEmpty(idTokenChange))
                {
                    response = await _geminiService.AnalyzeAsync(geminiRequest);
                }
                else
                {
                    response = await _geminiService.AnalyzeWithTokenUserConfigAsync(geminiRequest, idTokenChange);
                }

                if (response == null || response.Data == null)
                {
                    _logger.LogWarning("Không tìm thấy kết quả phân tích cho liên kết: {Link}", link);
                    return NotFound(ProblemDetailsFactory.CreateProblemDetails(HttpContext, statusCode: 404, title: "Không tìm thấy kết quả phân tích."));
                }
                var analysisLink = await _unitMongo.AnalysisLinks.AddOrUpdateText(new AnalysisLink() { LinkOrKeyword = link, AnalysisText = String.Join("", response.Data.Candidates[0].Content.Parts.Select(x => x.Text)) });
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
