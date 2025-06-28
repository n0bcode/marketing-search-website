using Api.Services.SearchServices.Twitter.TwitterRequests;
using Newtonsoft.Json;
using System.Reflection;
using System.Web;

namespace Api.Services.SearchServices.Twitter
{
    public static class TwitterExtension
    {
        public static string ToQueryString(this TwitterRequest request)
        {
            var properties = request.GetType().GetProperties();
            var queryParams = new List<string>();

            foreach (var property in properties)
            {
                var jsonPropertyAttribute = property.GetCustomAttribute<JsonPropertyAttribute>();
                string propertyName = jsonPropertyAttribute?.PropertyName ?? property.Name; // Lấy tên từ JsonPropertyAttribute hoặc sử dụng tên thuộc tính

                var value = property.GetValue(request);

                if (value != null)
                {
                    if (value is string stringValue)
                    {
                        if (!string.IsNullOrEmpty(stringValue))
                        {
                            queryParams.Add($"{HttpUtility.UrlEncode(propertyName)}={HttpUtility.UrlEncode(stringValue)}");
                        }
                    }
                    else if (value is int intValue)
                    {
                        queryParams.Add($"{HttpUtility.UrlEncode(propertyName)}={intValue}");
                    }
                    else if (value is List<string> listValue && listValue.Any())
                    {
                        // Chuyển đổi danh sách thành chuỗi phân tách bằng dấu phẩy
                        string commaSeparatedValues = string.Join(",", listValue.Select(HttpUtility.UrlEncode));
                        queryParams.Add($"{HttpUtility.UrlEncode(propertyName)}={commaSeparatedValues}");
                    }
                }
            }

            return string.Join("&", queryParams);
        }
    }
}
