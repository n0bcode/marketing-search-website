using Api.Constants;
using Api.Models;
using Api.Repositories.IRepositories;
using Api.Repositories.MongoDb;
using Api.Services.RedisCacheService;
using Api.Services.SearchServices;
using Api.Services.SearchServices.Google;
using Api.Services.SearchServices.Twitter;
using Api.Services.SearchServices.Twitter.TwitterRequests;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Controllers
{
    /// <summary>
    /// Controller for handling search-related operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class SearchController : ControllerBase
    {
        #region Fields

        private readonly GoogleSearchService _searchService;
        private readonly TwitterSearchService _searchServiceTwitter;
        private readonly BingSearchService _bingSearchService;
        private readonly IUnitOfWork _unit;
        private readonly IUnitOfWorkMongo _unitMongo;
        private readonly IRedisCacheService _redis;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchController"/> class.
        /// </summary>
        /// <param name="searchService">The Google search service.</param>
        /// <param name="searchServiceTwitter">The Twitter search service.</param>
        /// <param name="bingSearchService">The Bing search service.</param>
        /// <param name="unit">The unit of work for SQL database operations.</param>
        /// <param name="unitOfWorkMongo">The unit of work for MongoDB operations.</param>
        /// <param name="redisCache">The Redis cache service.</param>
        public SearchController(GoogleSearchService searchService,
                                TwitterSearchService searchServiceTwitter,
                                BingSearchService bingSearchService,
                                IUnitOfWork unit,
                                IUnitOfWorkMongo unitOfWorkMongo,
                                IRedisCacheService redisCache)
        {
            _searchService = searchService;
            _searchServiceTwitter = searchServiceTwitter;
            _bingSearchService = bingSearchService;
            _unit = unit;
            _unitMongo = unitOfWorkMongo;
            _redis = redisCache;
        }

        #endregion

        #region Public Endpoints

        /// <summary>
        /// Performs a Google search based on the provided request parameters.
        /// </summary>
        /// <param name="request">The Google search request parameters.</param>
        /// <returns>An <see cref="IActionResult"/> containing the Google search results.</returns>
        [HttpGet]
        public async Task<IActionResult> SearchGoogle([FromQuery] SearchRequest request)
        {
            var results = await _searchService.SearchAsync(request);
            return Ok(results);
        }

        /// <summary>
        /// Performs a Bing search based on the provided request parameters.
        /// </summary>
        /// <param name="query">The search query.</param>
        /// <param name="maxResults">Maximum number of results to retrieve.</param>
        /// <param name="site">Specific site to search within.</param>
        /// <param name="timeRange">Time range for the search (e.g., "past year").</param>
        /// <param name="filetype">Specific file type to search for.</param>
        /// <param name="language">Language for the search.</param>
        /// <param name="region">Region for the search.</param>
        /// <returns>An <see cref="IActionResult"/> containing the Bing search results.</returns>
        [HttpPost]
        public async Task<IActionResult> SearchBing([FromBody] SearchRequest request)
        {
            var results = await _bingSearchService.SearchBing(request);
            return Ok(results);
        }

        /// <summary>
        /// Performs a Twitter search based on the provided request parameters.
        /// </summary>
        /// <param name="request">The Twitter search tweet request parameters.</param>
        /// <returns>An <see cref="IActionResult"/> containing the Twitter search results.</returns>
        [HttpGet]
        public async Task<IActionResult> SearchTwitter([FromQuery] TwitterSearchTweetRequest request)
        {
            var results = await _searchServiceTwitter.SearchAsync(request);
            return Ok(results);
        }

        /// <summary>
        /// Retrieves all keywords from the database, utilizing Redis cache for performance.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> containing a list of <see cref="KeywordModel"/> objects.</returns>
        [HttpGet]
        public async Task<IActionResult> GetAllKeyword()
        {
            ResponseAPI<IEnumerable<KeywordModel>>? response = null;
            string cacheKey = "allKeywordsCache"; // Define a specific cache key

            try
            {
                response = _redis.GetData<ResponseAPI<IEnumerable<KeywordModel>>>(cacheKey);

                if (response == null)
                {
                    response = await _unitMongo.Keywords.GetAllKeywordsAsync();
                    if (response != null && response.Success) // Only cache if the operation was successful
                    {
                        _redis.SetData(cacheKey, response); // Cache for 5 minutes
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception for debugging purposes
                // _logger.LogError(ex, "Error retrieving keywords from cache or database."); 
                // Assuming _logger is available, otherwise use Console.WriteLine or similar
                Console.WriteLine($"Error retrieving keywords: {ex.Message}");
                response = await _unitMongo.Keywords.GetAllKeywordsAsync(); // Fallback to database if cache fails
            }

            if (response == null || !response.Success)
            {
                return StatusCode(response?.StatusCode ?? 500, response?.Message ?? "An error occurred while retrieving keywords.");
            }

            return Ok(response);
        }

        #endregion
    }
}