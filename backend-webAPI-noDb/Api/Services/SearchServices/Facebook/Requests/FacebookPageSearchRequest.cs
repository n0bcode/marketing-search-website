using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Services.SearchServices.Facebook.Requests
{
    public class FacebookPageSearchRequest
    {

        // Thuộc tính truy vấn
        public string Query { get; set; }          // Từ khóa tìm kiếm
        public string AccessToken { get; set; }     // Mã truy cập
        public string[] Fields { get; set; }        // Các trường thông tin cần trả về

        // Khởi tạo với các giá trị mặc định
        public FacebookPageSearchRequest(string query, string accessToken, params string[] fields)
        {
            Query = query;
            AccessToken = accessToken;
            Fields = fields;
        }

        // Phương thức để tạo URL truy vấn
        public string ToRequestUrl()
        {
            // Tạo chuỗi fields cho truy vấn
            string fieldsParameter = string.Join(",", Fields);

            // Tạo URL truy vấn
            return $"https://graph.facebook.com/pages/search?q={Uri.EscapeDataString(Query)}&fields={fieldsParameter}&access_token={AccessToken}";
        }

    }
}