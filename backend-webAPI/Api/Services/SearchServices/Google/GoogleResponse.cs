using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Api.Services.SearchServices.Google
{
    public class GoogleResponse
    {
        public SearchRequest? SearchParameters { get; set; }
        public KnowledgeGraph KnowledgeGraphRelated { get; set; } = new();
        public List<OrganicSearchResult> Organic { get; set; } = new();
        public List<RelatedSearch> RelatedSearches { get; set; } = new();
        public int Credits { get; set; }
        public class KnowledgeGraph
        {
            public string Title { get; set; } = string.Empty;
            public string Type { get; set; } = string.Empty;
            public string ImageUrl { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public string DescriptionSource { get; set; } = string.Empty;
            public string DescriptionLink { get; set; } = string.Empty;
            public Dictionary<string, string> Attributes { get; set; } = new();
        }
        public class OrganicSearchResult
        {
            public string Title { get; set; } = string.Empty;
            public string Link { get; set; } = string.Empty;
            public string Snippet { get; set; } = string.Empty;
            public int Position { get; set; }
            public string Date { get; set; } = string.Empty;
        }
        public class RelatedSearch
        {
            public string Query { get; set; } = string.Empty;
        }

    }
}