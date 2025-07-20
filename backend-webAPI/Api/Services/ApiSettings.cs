using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Services
{
    public class ApiSettings
    {
        public GoogleApiSettings GoogleApi { get; set; } = new();
        public FacebookApiSettings FacebookApi { get; set; } = new();
        public OpenApiSettings OpenApi { get; set; } = new();
        public GeminiApiSettings GeminiApi { get; set; } = new();
    }
    public class GoogleApiSettings
    {
        public string Name { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
    }

    public class FacebookApiSettings
    {
        public string Name { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = string.Empty;
        public string AccessToken { get; set; } = string.Empty;
    }
    public class OpenApiSettings
    {
        public string Name { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = string.Empty;
        public string SecretToken { get; set; } = string.Empty;
    }
    public class GeminiApiSettings
    {
        public string Name { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = string.Empty;
        public string SecretToken { get; set; } = string.Empty;
    }
}