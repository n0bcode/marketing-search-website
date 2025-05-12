using Api.Services.SearchServices;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace Api.Services.AIServices.Gemini
{
    public class GeminiAIService
    {
        private readonly HttpClient _httpClient;
        private readonly string _secretToken;
        private readonly string _baseUrl; // Lưu trữ BaseUrl

        public GeminiAIService(HttpClient httpClient, IOptions<ApiSettings> apiSettings)
        {
            _httpClient = httpClient;
            _secretToken = apiSettings.Value.GeminiApi.SecretToken;
            _baseUrl = apiSettings.Value.GeminiApi.BaseUrl; // Lưu trữ BaseUrl+
        }

        public async Task<GeminiAIResponse?> AnalyzeAsync(GeminiRequest prompt)
        {
            GeminiAIResponse? responseObject = new();

            string jsonRequest = JsonConvert.SerializeObject(prompt);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            // Sử dụng _httpClient đã được khởi tạo và baseUrl
            HttpResponseMessage response = await _httpClient.PostAsync($"{_baseUrl}/v1beta/models/gemini-2.0-flash:generateContent?key={_secretToken}", content);

            if (response.IsSuccessStatusCode)
            {
                string jsonResponse = await response.Content.ReadAsStringAsync();
                responseObject = JsonConvert.DeserializeObject<GeminiAIResponse?>(jsonResponse);
                return responseObject;
            }
            else
            {
                Console.WriteLine($"Lỗi: {response.StatusCode} - {await response.Content.ReadAsStringAsync()} - {await response.Content.ReadAsStringAsync()}"); // Thêm nội dung lỗi để debug
                return responseObject;
            }
        }
    }
}
