using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api.Services.SearchServices.Google;
using Api.Services.SearchServices.Twitter.TwitterRequests;

namespace Api.Services.SearchServices
{
    public class AllRequestPlatform
    {
        public GoogleRequest? GoogleRequest { get; set; }
        public TwitterSearchTweetRequest? TwitterSearchTweetRequest { get; set; }
    }
}