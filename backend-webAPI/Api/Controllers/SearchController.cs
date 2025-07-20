using Api.Constants;
using Api.Models;
using Api.Repositories.MongoDb;
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

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchController"/> class.
        /// </summary>
        /// <param name="searchService">The Google search service.</param>
        /// <param name="searchServiceTwitter">The Twitter search service.</param>
        /// <param name="bingSearchService">The Bing search service.</param>
        public SearchController(GoogleSearchService searchService,
                                TwitterSearchService searchServiceTwitter,
                                BingSearchService bingSearchService)
        {
            _searchService = searchService;
            _searchServiceTwitter = searchServiceTwitter;
            _bingSearchService = bingSearchService;
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
        /// <param name="request">The search request containing the query and other parameters.</param>
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

        #endregion
    }
}