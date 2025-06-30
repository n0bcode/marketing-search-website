using Api.Constants;
using Api.Models;
using Api.Repositories.IRepositories;
using Api.Repositories.MongoDb;
using Api.Services.RedisCacheService;
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
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class SearchController : ControllerBase
    {
        private readonly GoogleSearchService _searchService;
        private readonly TwitterSearchService _searchServiceTwitter;

        private readonly IUnitOfWork _unit;
        private readonly IUnitOfWorkMongo _unitMongo;
        private readonly IRedisCacheService _redis;


        public SearchController(GoogleSearchService searchService,
                                TwitterSearchService searchServiceTwitter,
                                IUnitOfWork unit,
                                IUnitOfWorkMongo unitOfWorkMongo,
                                IRedisCacheService redisCache)
        {
            _searchService = searchService;
            _searchServiceTwitter = searchServiceTwitter;
            _unit = unit;
            _unitMongo = unitOfWorkMongo;
            _redis = redisCache;
        }

        [HttpGet]
        public async Task<IActionResult> SearchGoogle([FromQuery] GoogleRequest request)
        {
            var results = await _searchService.SearchAsync(request);
            return Ok(results);
        }

        [HttpGet]
        public async Task<IActionResult> SearchTwitter([FromQuery] TwitterSearchTweetRequest request)
        {
            var results = await _searchServiceTwitter.SearchAsync(request);
            return Ok(results);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllKeyword()
        {
            ResponseAPI<IEnumerable<KeywordModel>>? response = new();
            try
            {
                response = _redis.GetData<ResponseAPI<IEnumerable<KeywordModel>>>("exceptionCacheKeyword");

                if (response == null)
                {
                    response = await _unitMongo.Keywords.GetAllKeywordsAsync();
                    _redis.SetData<ResponseAPI<IEnumerable<KeywordModel>>>("exceptionCacheKeyword", response);
                }
            }
            catch (Exception)
            {
                response = await _unitMongo.Keywords.GetAllKeywordsAsync();
            }
            return Ok(response);
        }
    }
}