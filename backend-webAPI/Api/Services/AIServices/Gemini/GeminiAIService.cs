using Api.Constants;
using Api.Models;
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
        public async Task<ResponseAPI<GeminiAIResponse>> AnalyzeAsync(GeminiRequest prompt)
        {
            var responseApi = new ResponseAPI<GeminiAIResponse>();
            GeminiAIResponse? responseObject = new();
            string jsonRequest = JsonConvert.SerializeObject(prompt);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            int maxRetries = 5; // Số lần thử tối đa
            int retryDelay = 2000; // Thời gian chờ giữa các lần thử (ms)
            for (int i = 0; i < maxRetries; i++)
            {
                HttpResponseMessage response = await _httpClient.PostAsync($"{_baseUrl}/v1beta/models/gemini-2.0-flash:generateContent?key={_secretToken}", content);

                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    responseObject = JsonConvert.DeserializeObject<GeminiAIResponse?>(jsonResponse);

                    responseApi.Success = true;
                    responseApi.StatusCode = (int)response.StatusCode;
                    responseApi.Data = responseObject; // Gán dữ liệu vào ResponseAPI
                    return responseApi;
                }
                else
                {
                    // Cập nhật thông tin lỗi
                    string errorMessage = await response.Content.ReadAsStringAsync();
                    responseApi.StatusCode = (int)response.StatusCode;
                    responseApi.Message = $"Lỗi: {response.StatusCode} - {errorMessage}";
                    responseApi.Errors.Add(responseApi.Message);
                    Console.WriteLine(responseApi.Message);

                    // Nếu gặp lỗi 503, chờ một khoảng thời gian trước khi thử lại
                    if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
                    {
                        await Task.Delay(retryDelay);
                        retryDelay *= 2; // Tăng thời gian chờ
                    }
                    else
                    {
                        // Nếu không phải là lỗi 503, thoát vòng lặp
                        return responseApi;
                    }
                }
            }

            // Nếu hết số lần thử mà vẫn không thành công
            responseApi.Message = "Đã vượt quá số lần thử mà vẫn không thành công.";
            responseApi.Errors.Add(responseApi.Message);
            return responseApi;
        }
    }
}
