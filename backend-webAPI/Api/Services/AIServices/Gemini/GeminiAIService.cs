using Api.Constants;
using Api.Models;
using Api.Repositories.MongoDb;
using Api.Services.SearchServices;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace Api.Services.AIServices.Gemini
{
    /// <summary>
    /// Service for interacting with the Gemini AI API.
    /// </summary>
    public class GeminiAIService
    {
        #region Fields

        private readonly HttpClient _httpClient;
        private readonly string _secretToken;
        private readonly string _baseUrl;
        private readonly IUnitOfWorkMongo _unitOfWorkMongo;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="GeminiAIService"/> class.
        /// </summary>
        /// <param name="httpClient">The HTTP client for making API requests.</param>
        /// <param name="apiSettings">The API settings for Gemini configuration.</param>
        /// <param name="unitOfWorkMongo">The unit of work for database operations.</param>
        public GeminiAIService(HttpClient httpClient, IOptions<ApiSettings> apiSettings, IUnitOfWorkMongo unitOfWorkMongo)
        {
            _httpClient = httpClient;
            _secretToken = apiSettings.Value.GeminiApi.SecretToken;
            _baseUrl = apiSettings.Value.GeminiApi.BaseUrl;
            _unitOfWorkMongo = unitOfWorkMongo;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Analyzes a prompt using the Gemini AI model with the default secret token.
        /// Implements a retry mechanism for Service Unavailable (503) errors.
        /// </summary>
        /// <param name="prompt">The Gemini AI request prompt.</param>
        /// <returns>A <see cref="ResponseAPI{GeminiAIResponse}"/> containing the AI analysis response.</returns>
        public async Task<ResponseAPI<GeminiAIResponse>> AnalyzeAsync(GeminiAIRequest prompt)
        {
            return await AnalyzeInternalAsync(prompt, _secretToken);
        }

        /// <summary>
        /// Analyzes a prompt using the Gemini AI model with a user-configured token.
        /// Implements a retry mechanism for Service Unavailable (503) errors.
        /// </summary>
        /// <param name="prompt">The Gemini AI request prompt.</param>
        /// <param name="tokenId">The ID of the user's secret token.</param>
        /// <returns>A <see cref="ResponseAPI{GeminiAIResponse}"/> containing the AI analysis response.</returns>
        public async Task<ResponseAPI<GeminiAIResponse>> AnalyzeWithTokenUserConfigAsync(GeminiAIRequest prompt, string tokenId)
        {
            var responseApi = new ResponseAPI<GeminiAIResponse>();
            var secretToken = await _unitOfWorkMongo.SecretTokens.GetByIdAsync(tokenId);
            string? tokenDecrypted = secretToken?.Token;
            if (string.IsNullOrEmpty(tokenDecrypted))
            {
                responseApi.Message = "Invalid or non-existent token.";
                responseApi.Errors.Add(responseApi.Message);
                return responseApi;
            }
            return await AnalyzeInternalAsync(prompt, tokenDecrypted);
        }

        /// <summary>
        /// Internal method to analyze a prompt using the Gemini AI model with a specified secret token.
        /// Implements a retry mechanism for Service Unavailable (503) errors.
        /// </summary>
        /// <param name="prompt">The Gemini AI request prompt.</param>
        /// <param name="secretToken">The secret token to use for the API request.</param>
        /// <returns>A <see cref="ResponseAPI{GeminiAIResponse}"/> containing the AI analysis response.</returns>
        private async Task<ResponseAPI<GeminiAIResponse>> AnalyzeInternalAsync(GeminiAIRequest prompt, string secretToken)
        {
            var responseApi = new ResponseAPI<GeminiAIResponse>();
            GeminiAIResponse? responseObject;
            string jsonRequest = JsonConvert.SerializeObject(prompt);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            int maxRetries = 5;
            int retryDelay = 2000;

            for (int i = 0; i < maxRetries; i++)
            {
                HttpResponseMessage response = await _httpClient.PostAsync($"{_baseUrl}/v1beta/models/gemini-2.0-flash:generateContent?key={secretToken}", content);

                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = (await response.Content.ReadAsStringAsync()).Trim(); // Trim whitespace
                    string cleanedJsonResponse = jsonResponse;
                    try
                    {
                        var jsonMatch = System.Text.RegularExpressions.Regex.Match(jsonResponse, @"```(?:json)?\s*([\s\S]*?)\s*```");
                        if (jsonMatch.Success && jsonMatch.Groups.Count > 1)
                        {
                            cleanedJsonResponse = jsonMatch.Groups[1].Value.Trim().TrimStart('\n', '\r').TrimEnd('\n', '\r');
                        }

                        // If regex didn't find a JSON block or cleaned response is invalid, try to extract JSON manually
                        if (string.IsNullOrEmpty(cleanedJsonResponse) || !cleanedJsonResponse.StartsWith("{"))
                        {
                            int startIndex = jsonResponse.IndexOf("{");
                            int endIndex = jsonResponse.LastIndexOf("}");
                            if (startIndex >= 0 && endIndex > startIndex)
                            {
                                cleanedJsonResponse = jsonResponse.Substring(startIndex, endIndex - startIndex + 1);
                            }
                            else
                            {
                                cleanedJsonResponse = jsonResponse;
                            }
                        }

                        Console.WriteLine("Attempting to deserialize JSON: " + cleanedJsonResponse.Substring(0, Math.Min(cleanedJsonResponse.Length, 200)) + (cleanedJsonResponse.Length > 200 ? "..." : ""));

                        responseObject = JsonConvert.DeserializeObject<GeminiAIResponse?>(cleanedJsonResponse);
                        if (responseObject == null)
                        {
                            Console.WriteLine("Deserialization failed. Cleaned JSON response: " + cleanedJsonResponse);
                            responseApi.Message = "Failed to deserialize the response from Gemini AI.";
                            responseApi.Errors.Add(responseApi.Message);
                            return responseApi;
                        }
                    }
                    catch (JsonReaderException ex)
                    {
                        Console.WriteLine("JSON Parsing Error: " + ex.Message);
                        Console.WriteLine("Raw Response: " + jsonResponse);
                        responseApi.Message = "Error parsing JSON response from Gemini AI: " + ex.Message;
                        responseApi.Errors.Add(responseApi.Message);
                        return responseApi;
                    }

                    responseApi.Success = true;
                    responseApi.StatusCode = (int)response.StatusCode;
                    responseApi.Data = responseObject;
                    return responseApi;
                }
                else
                {
                    string errorMessage = await response.Content.ReadAsStringAsync();
                    responseApi.StatusCode = (int)response.StatusCode;
                    responseApi.Message = $"Error: {response.StatusCode} - {errorMessage}";
                    responseApi.Errors.Add(responseApi.Message);
                    Console.WriteLine(responseApi.Message);

                    if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
                    {
                        await Task.Delay(retryDelay);
                        retryDelay *= 2;
                    }
                    else
                    {
                        return responseApi;
                    }
                }
            }

            responseApi.Message = "Maximum retry attempts exceeded.";
            responseApi.Errors.Add(responseApi.Message);
            return responseApi;
        }

        #endregion
    }
}
