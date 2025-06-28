using Api.Services.SearchServices.Google;
using Api.Services.SearchServices.Twitter.TwitterRequests;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Api.Services.SearchServices.Twitter
{
    public class TwitterSearchService : ISearchService<TwitterRequest, TwitterResponse?>
    {
        private readonly HttpClient _httpClient;
        private readonly string _bearToken;

        public TwitterSearchService(HttpClient httpClient, IOptions<ApiSettings> apiSettings)
        {
            _httpClient = httpClient;
            _bearToken = apiSettings.Value.TwitterApi.BearerToken;
            _httpClient.BaseAddress = new Uri(apiSettings.Value.TwitterApi.BaseUrl); // Sửa: Sử dụng BaseUrl
        }
        public async Task<TwitterResponse?> SearchAsync(TwitterRequest request)
        {
            TwitterResponse twitterResponse = new();
            try
            {
                // Xây dựng các tham số từ yêu cầu
                var parameters = TwitterExtension.ToQueryString(request);

                // **Sửa: Không cần chuyển tham số thành JSON cho phương thức GET (phổ biến cho Twitter Search)**
                // var requestContent = new StringContent(JsonConvert.SerializeObject(parameters), Encoding.UTF8, "application/json");

                // **Sửa: Tạo Uri với tham số query string**
                // ! The fuck over there about this path with need to fix in the future
                var requestUri = $"/2/tweets/search/recent?{parameters}"; // Thêm dấu "?" vào trước chuỗi truy vấn

                // **Sửa: Thêm Authorization vào Header**
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _bearToken); // Sử dụng AuthenticationHeaderValue

                // **Sửa: Gửi yêu cầu GET (thường dùng cho Twitter Search)**
                var response = await _httpClient.GetAsync(requestUri); // Sử dụng GetAsync và requestUri

                // Kiểm tra mã trạng thái phản hồi
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    twitterResponse = JsonConvert.DeserializeObject<TwitterResponse>(jsonResponse)!;
                    return twitterResponse;
                }
                else if ((int)response.StatusCode == 429)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error content: {errorContent}");
                }
                else
                {
                    // **Quan trọng: Xử lý các mã lỗi khác nhau từ Twitter API**
                    Console.WriteLine($"Request failed with status code: {response.StatusCode}");
                    // In ra nội dung lỗi để debug:
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error content: {errorContent}");
                }
            }
            catch (Exception ex)
            {
                // Xử lý lỗi nếu cần thiết
                Console.WriteLine($"Error: {ex.Message}");
            }

            return twitterResponse; // Hoặc xử lý lỗi phù hợp
        }
    }
}
