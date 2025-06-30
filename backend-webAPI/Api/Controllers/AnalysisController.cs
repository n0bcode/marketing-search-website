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
        #region Fields

        private readonly GoogleSearchService _searchService;
        private readonly TwitterSearchService _searchServiceTwitter;
        private readonly GeminiAIService _geminiService;
        private readonly IUnitOfWork _unit;
        private readonly IUnitOfWorkMongo _unitMongo;
        private readonly ILogger<AnalysisController> _logger;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="AnalysisController"/> class.
        /// </summary>
        /// <param name="searchService">The Google search service.</param>
        /// <param name="searchServiceTwitter">The Twitter search service.</param>
        /// <param name="geminiAIService">The Gemini AI service.</param>
        /// <param name="unitOfWork">The unit of work for SQL database operations.</param>
        /// <param name="unitOfWorkMongo">The unit of work for MongoDB operations.</param>
        /// <param name="logger">The logger for the AnalysisController.</param>
        public AnalysisController(GoogleSearchService searchService,
                                  TwitterSearchService searchServiceTwitter,
                                  GeminiAIService geminiAIService,
                                  IUnitOfWork unitOfWork,
                                  IUnitOfWorkMongo unitOfWorkMongo,
                                  ILogger<AnalysisController> logger)
        {
            _searchService = searchService;
            _searchServiceTwitter = searchServiceTwitter;
            _geminiService = geminiAIService;
            _unit = unitOfWork;
            _unitMongo = unitOfWorkMongo;
            _logger = logger;
        }

        #endregion

        #region Public Endpoints

        /// <summary>
        /// A simple GET endpoint for testing purposes.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> indicating the status.</returns>
        [HttpGet]
        public IActionResult GetGood()
        {
            return Ok("Good@");
        }

        /// <summary>
        /// Performs a Google search and then analyzes the results using Gemini AI.
        /// </summary>
        /// <param name="query">The Google search request parameters.</param>
        /// <param name="idTokenGoogleChange">Optional: User-specific token for Google API if different from default.</param>
        /// <param name="idTokenGeminiChange">Optional: User-specific token for Gemini AI API if different from default.</param>
        /// <remarks>
        /// Example Request Body:
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
        /// <returns>An <see cref="IActionResult"/> containing the Gemini AI analysis response.</returns>
        [ProducesResponseType(typeof(GeminiAIResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [HttpPost]
        public async Task<IActionResult> SearchGoogleAndAnalysis([FromBody] GoogleRequest query, [FromQuery] string? idTokenGoogleChange, [FromQuery] string? idTokenGeminiChange)
        {
            ResponseAPI<GeminiAIResponse> responseApi = new();
            try
            {
                #region Google Search

                GoogleResponse? googleResults;
                if (string.IsNullOrEmpty(idTokenGoogleChange))
                {
                    googleResults = await _searchService.SearchAsync(query);
                }
                else
                {
                    googleResults = await _searchService.SearchWithUserTokenCofigAsync(query, idTokenGoogleChange);
                }

                if (googleResults == null || googleResults.Organic == null || !googleResults.Organic.Any())
                {
                    string errorMessage = "No Google search results found for query: " + query.q;
                    _logger.LogWarning(errorMessage);
                    responseApi.Message = errorMessage;
                    responseApi.StatusCode = (int)HttpStatusCode.NotFound;
                    return NotFound(responseApi);
                }

                var responseAddKey = await _unitMongo.Keywords.AddKeywordAndGetIdAsync(new KeywordModel()
                {
                    Keyword = query.q,
                    Source = $"Google-{query.type}",
                });

                #endregion

                #region Gemini AI Analysis

                string analysisInput = $"Google search data: {GetAnalysisInput(googleResults)}";

                ResponseAPI<GeminiAIResponse> geminiResponse;
                if (string.IsNullOrEmpty(idTokenGeminiChange))
                {
                    geminiResponse = await _geminiService.AnalyzeAsync(GeminiAIRequest.CreateWithQueryAndPrompt(query.q, analysisInput));
                }
                else
                {
                    geminiResponse = await _geminiService.AnalyzeWithTokenUserConfigAsync(GeminiAIRequest.CreateWithQueryAndPrompt(query.q, analysisInput), idTokenGeminiChange);
                }

                if (!geminiResponse.Success || geminiResponse.Data == null)
                {
                    string errorMessage = geminiResponse.Message ?? "No analysis results found from Gemini AI.";
                    _logger.LogWarning("Gemini AI analysis failed for query {Query}: {ErrorMessage}", query.q, errorMessage);
                    responseApi.Message = errorMessage;
                    responseApi.StatusCode = geminiResponse.StatusCode;
                    responseApi.Errors.AddRange(geminiResponse.Errors);
                    return StatusCode(responseApi.StatusCode, responseApi);
                }

                geminiResponse.Data.KeywordId = responseAddKey.Data!; // Assign ID from Keywords table to analysis result
                DataSaver.SaveData(geminiResponse, $"Gemini-{query.type}", responseAddKey.Data!);
                DataSaver.SaveData(googleResults!, $"Google-{query.type}", responseAddKey.Data!);

                geminiResponse.Data.GeneralSearchResults.AddRange(googleResults.ToGeneralSearchResults());
                geminiResponse.Data.SiteSearch = query.as_sitesearch;

                responseApi.Success = true;
                responseApi.StatusCode = (int)HttpStatusCode.OK;
                responseApi.Data = geminiResponse.Data;

                #endregion

                return Ok(responseApi);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred during Google search and analysis for query: {Query}", query.q);
                responseApi.Message = "An unexpected error occurred: " + ex.Message;
                responseApi.StatusCode = (int)HttpStatusCode.InternalServerError;
                responseApi.Errors.Add(ex.Message);
                return StatusCode((int)HttpStatusCode.InternalServerError, responseApi);
            }
        }

        /// <summary>
        /// Analyzes a given link using Gemini AI.
        /// </summary>
        /// <param name="link">The URL link to be analyzed.</param>
        /// <param name="idTokenChange">Optional: User-specific token for Gemini AI API if different from default.</param>
        /// <returns>An <see cref="IActionResult"/> containing the analysis result for the link.</returns>
        [ProducesResponseType(typeof(AnalysisLink), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [HttpPost]
        public async Task<IActionResult> AnalysisLink([FromQuery] string? link, [FromQuery] string? idTokenChange)
        {
            if (string.IsNullOrEmpty(link))
            {
                _logger.LogWarning("AnalysisLink: Link parameter is null or empty.");
                return BadRequest("Link cannot be null or empty.");
            }

            AnalysisLink? analysisLinkResult = await _unitMongo.AnalysisLinks.GetAnalysisLinkOrNot(link);

            if (analysisLinkResult == null || string.IsNullOrEmpty(analysisLinkResult.AnalysisText))
            {
                GeminiAIRequest geminiRequest = GeminiAIRequest.CreateWithPrompt(link, false);

                ResponseAPI<GeminiAIResponse> geminiResponse;
                if (string.IsNullOrEmpty(idTokenChange))
                {
                    geminiResponse = await _geminiService.AnalyzeAsync(geminiRequest);
                }
                else
                {
                    geminiResponse = await _geminiService.AnalyzeWithTokenUserConfigAsync(geminiRequest, idTokenChange);
                }

                if (!geminiResponse.Success || geminiResponse.Data == null || !geminiResponse.Data.Candidates.Any() || geminiResponse.Data.Candidates[0].Content == null || !geminiResponse.Data.Candidates[0].Content.Parts.Any())
                {
                    string errorMessage = geminiResponse.Message ?? "No analysis results found from Gemini AI for the provided link.";
                    _logger.LogWarning("Gemini AI analysis failed for link {Link}: {ErrorMessage}", link, errorMessage);
                    return NotFound(errorMessage);
                }

                analysisLinkResult = await _unitMongo.AnalysisLinks.AddOrUpdateText(new AnalysisLink()
                {
                    LinkOrKeyword = link,
                    AnalysisText = string.Join("", geminiResponse.Data.Candidates[0].Content.Parts.Select(x => x.Text))
                });
            }

            return Ok(analysisLinkResult);
        }

        /// <summary>
        /// Retrieves previously saved analysis results based on a keyword ID.
        /// </summary>
        /// <param name="idKeyword">The ID of the keyword to retrieve analysis for.</param>
        /// <returns>An <see cref="IActionResult"/> containing the old analysis data.</returns>
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.OK)]
        [HttpGet("{idKeyword}")]
        public IActionResult GetOldAnalysis(string idKeyword)
        {
            var response = DataSaver.GetFromLog(idKeyword);
            if (response == null)
            {
                return NotFound($"No analysis data found for keyword ID: {idKeyword}");
            }
            return Ok(response);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Formats Google search results into a string suitable for AI analysis.
        /// </summary>
        /// <param name="googleResults">The Google search response containing organic results.</param>
        /// <returns>A formatted string of Google search results.</returns>
        private string GetAnalysisInput(GoogleResponse googleResults)
        {
            if (googleResults == null || googleResults.Organic == null || !googleResults.Organic.Any())
            {
                return "No Google search results.";
            }

            var sb = new StringBuilder();

            sb.AppendLine("--- Google Search Results ---");

            foreach (var result in googleResults.Organic)
            {
                sb.AppendLine("--- Result ---");
                sb.AppendLine($"Title: {result.Title}");
                sb.AppendLine($"Date: {result.Date}");
                sb.AppendLine($"Position: {result.Position}");
                sb.AppendLine($"Link: {result.Link}");
                sb.AppendLine($"Snippet: {result.Snippet}");
                sb.AppendLine();
            }

            sb.AppendLine("--- End of Search Results ---");

            return sb.ToString();
        }

        /// <summary>
        /// Formats Twitter search results into a string suitable for AI analysis.
        /// This method is currently a placeholder and not actively used in the controller.
        /// </summary>
        /// <param name="twitterResults">The Twitter search response.</param>
        /// <returns>A formatted string of Twitter search results.</returns>
        private string GetAnalysisInput(TwitterResponse twitterResults)
        {
            if (twitterResults == null || twitterResults.Data == null)
            {
                return "No Twitter search results.";
            }

            // This is a simple example. You can add more detailed information from Twitter results here.
            return $"Total results: {twitterResults.Data.Count}";
        }

        #endregion
    }
}
