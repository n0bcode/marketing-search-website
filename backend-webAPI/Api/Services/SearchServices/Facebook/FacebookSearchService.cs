using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Api.Services.SearchServices.Facebook.Requests;

namespace Api.Services.SearchServices.Facebook
{
    public class FacebookSearchService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public FacebookSearchService(HttpClient httpClient, IOptions<ApiSettings> apiSettings)
        {
            _httpClient = httpClient;
            _apiKey = apiSettings.Value.FacebookApi.AccessToken;
            _httpClient.BaseAddress = new Uri(apiSettings.Value.GoogleApi.BaseUrl);
        }
        public async Task<dynamic?> SearchAsync(FacebookPageSearchRequest request)
        {
            // GoogleResponse googleResponse = new();
            try
            {
                // Gửi yêu cầu POST
                var response = await _httpClient.GetAsync(request.ToRequestUrl());
                //&debug=all
                // Kiểm tra mã trạng thái phản hồi
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    // googleResponse = JsonConvert.DeserializeObject<GoogleResponse>(jsonResponse)!;
                    return jsonResponse;
                }
            }
            catch (Exception ex)
            {
                // Xử lý lỗi nếu cần thiết
                Console.WriteLine($"Error: {ex.Message}");
            }

            return null; // Hoặc xử lý lỗi phù hợp
        }
    }
}