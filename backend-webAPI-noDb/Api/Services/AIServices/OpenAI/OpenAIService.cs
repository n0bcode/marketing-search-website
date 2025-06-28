using Api.Services.SearchServices;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks; // Thêm using này

namespace Api.Services.AIServices.OpenAI
{
    public class OpenAIService
    {
        private readonly HttpClient _httpClient;
        private readonly string _secretToken;
        private readonly string _baseUrl; // Lưu trữ BaseUrl

        public OpenAIService(HttpClient httpClient, IOptions<ApiSettings> apiSettings)
        {
            _httpClient = httpClient;
            _secretToken = apiSettings.Value.OpenApi.SecretToken;
            _baseUrl = apiSettings.Value.OpenApi.BaseUrl; // Lưu trữ BaseUrl
            // Đặt Authorization header ở đây, một lần duy nhất khi khởi tạo
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _secretToken);
        }

        public async Task<string> GetCompletionAsync(string prompt)
        {

            var requestData = new
            {
                model = "o1-mini", // Hoặc model bạn muốn sử dụng
                prompt = prompt,
                max_tokens = 100
            };

            string jsonRequest = JsonConvert.SerializeObject(requestData);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            // Sử dụng _httpClient đã được khởi tạo và baseUrl
            HttpResponseMessage response = await _httpClient.PostAsync($"{_baseUrl}/completions", content);

            if (response.IsSuccessStatusCode)
            {
                string jsonResponse = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrEmpty(jsonResponse))
                {
                    return "Không có phản hồi từ OpenAI.";
                }
                dynamic responseObject = JsonConvert.DeserializeObject(jsonResponse)!;
                // Kiểm tra xem responseObject có chứa trường "choices" không
                return responseObject.choices[0].text;
            }
            else
            {
                return $"Lỗi: {response.StatusCode} - {await response.Content.ReadAsStringAsync()} - {await response.Content.ReadAsStringAsync()}"; // Thêm nội dung lỗi để debug
            }
        }
    }
}
