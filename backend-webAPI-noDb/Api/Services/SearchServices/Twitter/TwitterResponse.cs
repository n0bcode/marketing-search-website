using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Api.Services.SearchServices.Twitter
{
    public class TwitterResponse
    {
        [JsonProperty("data")]
        public List<Tweet> Data { get;set; } = new();

        [JsonProperty("errors")]
        public List<TwitterError> Errors { get;set; } = new();

        [JsonProperty("includes")]
        public Includes Includes { get;set; } = new();

        [JsonProperty("meta")]
        public Meta Meta { get;set; } = new();
    }
    public class Tweet
    {
        [JsonProperty("author_id")]
        public string AuthorId { get;set; } = string.Empty;

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get;set; } = new();

        [JsonProperty("id")]
        public string Id { get;set; } = string.Empty;

        [JsonProperty("text")]
        public string Text { get;set; } = string.Empty;

        [JsonProperty("username")]
        public string Username { get;set; } = string.Empty;
    }

    public class TwitterError
    {
        [JsonProperty("detail")]
        public string Detail { get;set; } = string.Empty;

        [JsonProperty("status")]
        public int Status { get;set; } = new();

        [JsonProperty("title")]
        public string Title { get;set; } = string.Empty;

        [JsonProperty("type")]
        public string Type { get;set; } = string.Empty;
    }

    public class Includes
    {
        [JsonProperty("media")]
        public List<Media> Media { get;set; } = new();

        [JsonProperty("places")]
        public List<Place> Places { get;set; } = new();

        [JsonProperty("polls")]
        public List<Poll> Polls { get;set; } = new();

        [JsonProperty("topics")]
        public List<Topic> Topics { get;set; } = new();

        [JsonProperty("tweets")]
        public List<Tweet> Tweets { get;set; } = new();

        [JsonProperty("users")]
        public List<User> Users { get;set; } = new();
    }

    public class Media
    {
        [JsonProperty("height")]
        public int Height { get;set; } = new();

        [JsonProperty("media_key")]
        public string MediaKey { get;set; } = string.Empty;

        [JsonProperty("type")]
        public string Type { get;set; } = string.Empty;

        [JsonProperty("width")]
        public int Width { get;set; } = new();
    }

    public class Place
    {
        [JsonProperty("contained_within")]
        public List<string> ContainedWithin { get;set; } = new();

        [JsonProperty("country")]
        public string Country { get;set; } = string.Empty;

        [JsonProperty("country_code")]
        public string CountryCode { get;set; } = string.Empty;

        [JsonProperty("full_name")]
        public string FullName { get;set; } = string.Empty;

        [JsonProperty("geo")]
        public Geo Geo { get;set; } = new();

        [JsonProperty("id")]
        public string Id { get;set; } = string.Empty;

        [JsonProperty("name")]
        public string Name { get;set; } = string.Empty;

        [JsonProperty("place_type")]
        public string PlaceType { get;set; } = string.Empty;
    }

    public class Geo
    {
        [JsonProperty("bbox")]
        public List<double> Bbox { get;set; } = new();

        [JsonProperty("geometry")]
        public Geometry Geometry { get;set; } = new();

        [JsonProperty("properties")]
        public Dictionary<string, object> Properties { get;set; } = new();

        [JsonProperty("type")]
        public string Type { get;set; } = string.Empty;
    }

    public class Geometry
    {
        [JsonProperty("coordinates")]
        public List<double> Coordinates { get;set; } = new();

        [JsonProperty("type")]
        public string Type { get;set; } = string.Empty;
    }

    public class Poll
    {
        [JsonProperty("duration_minutes")]
        public int DurationMinutes { get;set; } = new();

        [JsonProperty("end_datetime")]
        public DateTime EndDatetime { get;set; } = new();

        [JsonProperty("id")]
        public string Id { get;set; } = string.Empty;

        [JsonProperty("options")]
        public List<Option> Options { get;set; } = new();

        [JsonProperty("voting_status")]
        public string VotingStatus { get;set; } = string.Empty;
    }

    public class Option
    {
        [JsonProperty("label")]
        public string Label { get;set; } = string.Empty;

        [JsonProperty("position")]
        public int Position { get;set; } = new();

        [JsonProperty("votes")]
        public int Votes { get;set; } = new();
    }

    public class Topic
    {
        [JsonProperty("description")]
        public string Description { get;set; } = string.Empty;

        [JsonProperty("id")]
        public string Id { get;set; } = string.Empty;

        [JsonProperty("name")]
        public string Name { get;set; } = string.Empty;
    }

    public class User
    {
        [JsonProperty("created_at")]
        public DateTime CreatedAt { get;set; } = new();

        [JsonProperty("id")]
        public string Id { get;set; } = string.Empty;

        [JsonProperty("name")]
        public string Name { get;set; } = string.Empty;

        [JsonProperty("protected")]
        public bool Protected { get;set; } = new();

        [JsonProperty("username")]
        public string Username { get;set; } = string.Empty;
    }

    public class Meta
    {
        [JsonProperty("newest_id")]
        public string NewestId { get;set; } = string.Empty;

        [JsonProperty("next_token")]
        public string NextToken { get;set; } = string.Empty;

        [JsonProperty("oldest_id")]
        public string OldestId { get;set; } = string.Empty;

        [JsonProperty("result_count")]
        public int ResultCount { get;set; } = new();
    }
}