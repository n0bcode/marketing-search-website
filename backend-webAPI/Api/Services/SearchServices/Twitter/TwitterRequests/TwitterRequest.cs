using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;

namespace Api.Services.SearchServices.Twitter.TwitterRequests
{
    public class TwitterRequest
    {
        /// <summary>
        /// One query/rule/filter for matching Posts. Refer to https://t.co/rulelength to identify the max query length.
        /// </summary>
        [JsonProperty("query")]
        public string Query { get;set; } = string.Empty;
        /// <summary>
        /// The maximum number of search results to be returned by a request.
        /// </summary>
        /// <value>
        ///     10
        /// </value>

        [JsonProperty("max_results")]
        public int MaxResults { get; set; } = 10;
        /// <summary>
        /// This parameter is used to get the next 'page' of results. The value used with the parameter is pulled directly from the response provided by the API, and should not be modified.
        /// </summary>

        [JsonProperty("next_token")]
        public string NextToken { get;set; } = string.Empty;
        /// <summary>
        /// A comma separated list of Tweet fields to display.
        /// </summary>
        [JsonProperty("tweet.fields")]
        public List<string> TweetFields { get; set; } = new List<string>();
        /// <summary>
        /// A comma separated list of fields to expand.
        /// </summary>

        [JsonProperty("expansions")]
        public List<string> Expansions { get; set; } = new List<string>();
        /// <summary>
        /// A comma separated list of User fields to display.
        /// </summary>
        [JsonProperty("user.fields")]
        public List<string> UserFields { get; set; } = new List<string>();
    }
}