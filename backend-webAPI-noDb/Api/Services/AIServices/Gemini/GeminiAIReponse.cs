using Api.Models;
using Api.Services.SearchServices;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Api.Services.AIServices.Gemini
{
    public class GeminiAIResponse
    {
        [Key]
        public string Id { get; set; } = string.Empty;

        #region [Model Properties] 
        [JsonPropertyName("candidates")]
        public List<Candidate> Candidates { get; set; } = new();

        [JsonPropertyName("usageMetadata")]
        public UsageMetadata UsageMetadata { get; set; } = new();

        [JsonPropertyName("modelVersion")]
        public string ModelVersion { get; set; } = string.Empty;

        public List<GeneralSearchResult> GeneralSearchResults { get; set; } = new();
        #endregion

        #region [Database Properties] 
        public string? SiteSearch { get; set; } = string.Empty;
        public string Note { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public string KeywordId { get; set; } = string.Empty;
        [ForeignKey("KeywordId")]
        public virtual KeywordModel? Keyword { get; set; }
        #endregion

        #region [Json Deserialize] 
        public static GeminiAIResponse? FromJson(string json)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            return JsonSerializer.Deserialize<GeminiAIResponse?>(json, options);
        }
        #endregion

        #region [Json Serialization] 
        public string ToJson()
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true,  // For readability
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull // Optional:  Don't serialize null values
            };

            return JsonSerializer.Serialize(this, options);
        }
        #endregion


    }
    public class CitationSource
    {
        [JsonPropertyName("startIndex")]
        public int StartIndex { get; set; } = new();

        [JsonPropertyName("endIndex")]
        public int EndIndex { get; set; } = new();
    }

    public class CitationMetadata
    {
        [JsonPropertyName("citationSources")]
        public List<CitationSource> CitationSources { get; set; } = new();
    }

    public class PartResponse
    {
        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;
    }

    public class ContentResponse
    {
        [JsonPropertyName("parts")]
        public List<PartResponse> Parts { get; set; } = new();

        [JsonPropertyName("role")]
        public string Role { get; set; } = string.Empty;
    }

    public class Candidate
    {
        [JsonPropertyName("content")]
        public ContentResponse Content { get; set; } = new();

        [JsonPropertyName("finishReason")]
        public string FinishReason { get; set; } = string.Empty;

        [JsonPropertyName("citationMetadata")]
        public CitationMetadata CitationMetadata { get; set; } = new();

        [JsonPropertyName("avgLogprobs")]
        public double? AvgLogprobs { get; set; } = new();
    }

    public class TokensDetails
    {
        [JsonPropertyName("modality")]
        public string Modality { get; set; } = string.Empty;

        [JsonPropertyName("tokenCount")]
        public int TokenCount { get; set; } = new();
    }

    public class UsageMetadata
    {
        [JsonPropertyName("promptTokenCount")]
        public int PromptTokenCount { get; set; } = new();

        [JsonPropertyName("candidatesTokenCount")]
        public int CandidatesTokenCount { get; set; } = new();

        [JsonPropertyName("totalTokenCount")]
        public int TotalTokenCount { get; set; } = new();

        [JsonPropertyName("promptTokensDetails")]
        public List<TokensDetails> PromptTokensDetails { get; set; } = new();

        [JsonPropertyName("candidatesTokensDetails")]
        public List<TokensDetails> CandidatesTokensDetails { get; set; } = new();
    }
}
