using Api.Services.SearchServices.Google;
using Api.Services.SearchServices.Twitter;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Api.Services.SearchServices
{
    public class GeneralSearchResult
    {
        public string Id { get; }
        public string Title { get; }
        public string Description { get; }
        public string Url { get; }
        public string Date { get; } = string.Empty;
        public DateTime? CreatedAt { get; }
        public string Author { get; }
        public string Source { get; }

        public GeneralSearchResult(
            string id,
            string title,
            string description,
            string url,
            string date,
            DateTime? createdAt,
            string author,
            string source)
        {
            Id = id;
            Title = title;
            Description = description;
            Url = url;
            Date = date;
            CreatedAt = createdAt;
            Author = author;
            Source = source;
        }
    }

    public static class SearchResultExtensions
    {
        public static List<GeneralSearchResult> ToGeneralSearchResults(this GoogleResponse googleResponse)
        {
            if (googleResponse == null || googleResponse.Organic == null)
            {
                return new List<GeneralSearchResult>(); // Hoặc throw một exception tùy thuộc vào yêu cầu của bạn
            }

            return googleResponse.Organic.Select(aSearch => new GeneralSearchResult(
                Guid.NewGuid().ToString(),
                aSearch.Title,
                aSearch.Snippet,
                aSearch.Link,
                aSearch.Date,
                null,
                string.Empty,
                "Google")).ToList();
        }

        public static List<GeneralSearchResult> ToGeneralSearchResults(this TwitterResponse twitterResponse)
        {
            if (twitterResponse == null || twitterResponse.Data == null)
            {
                return new List<GeneralSearchResult>(); // Hoặc throw một exception tùy thuộc vào yêu cầu của bạn
            }

            return twitterResponse.Data.Select(tweet => new GeneralSearchResult(
                tweet.Id,
                tweet.Text,
                tweet.Text,
                $"https://twitter.com/{tweet.Username}/status/{tweet.Id}",
                tweet.CreatedAt.ToString("yyyy-MM-dd"),
                tweet.CreatedAt,
                tweet.Username,
                "Twitter")).ToList();
        }
    }
}
