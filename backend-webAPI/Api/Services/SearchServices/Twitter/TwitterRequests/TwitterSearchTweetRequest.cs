using Newtonsoft.Json;

namespace Api.Services.SearchServices.Twitter.TwitterRequests
{
    public class TwitterSearchTweetRequest : TwitterRequest
    {
        /// <summary>
        /// YYYY-MM-DDTHH:mm:ssZ. The oldest UTC timestamp from which the Posts will be provided. Timestamp is in second granularity and is inclusive (i.e. 12:00:01 includes the first second of the minute).
        /// </summary>

        [JsonProperty("start_time")]
        public string StartTime { get; set; } = string.Empty;
        /// <summary>
        /// YYYY-MM-DDTHH:mm:ssZ. The newest, most recent UTC timestamp to which the Posts will be provided. Timestamp is in second granularity and is exclusive (i.e. 12:00:01 excludes the first second of the minute).
        /// </summary>

        [JsonProperty("end_time")]
        public string EndTime { get; set; } = string.Empty;
        /// <summary>
        /// Returns results with a Post ID greater than (that is, more recent than) the specified ID.
        /// </summary>
        [JsonProperty("since_id")]
        public string SinceId { get; set; } = string.Empty;
        /// <summary>
        /// Returns results with a Post ID less than (that is, older than) the specified ID.
        /// </summary>
        [JsonProperty("until_id")]
        public string UntilId { get; set; } = string.Empty;
        /// <summary>
        /// This parameter is used to get the next 'page' of results. The value used with the parameter is pulled directly from the response provided by the API, and should not be modified.
        /// </summary>

        [JsonProperty("pagination_token")]
        public string PaginationToken { get; set; } = string.Empty;
        /// <summary>
        /// This order in which to return results.
        /// </summary>
        [JsonProperty("sort_order")]
        public string SortOrder { get; set; } = string.Empty; // Sử dụng string vì nó là enum
        /// <summary>
        /// A comma separated list of Media fields to display.
        /// </summary>
        [JsonProperty("media.fields")]
        public List<string> MediaFields { get; set; } = new List<string>();
        /// <summary>
        /// A comma separated list of Poll fields to display.
        /// </summary>
        [JsonProperty("poll.fields")]
        public List<string> PollFields { get; set; } = new List<string>();
        /// <summary>
        /// A comma separated list of Place fields to display.
        /// </summary>
        [JsonProperty("place.fields")]
        public List<string> PlaceFields { get; set; } = new List<string>();
    }
}
