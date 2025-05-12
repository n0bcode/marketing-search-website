using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Services.SearchServices.Google
{
    public class GoogleRequest
    {
        /// <summary>
        /// Chuỗi tìm kiếm từ khóa (ví dụ: "học lập trình C#").
        /// Đây là tham số bắt buộc và không được để trống.
        /// </summary>
        public required string q { get; set; }

        /// <summary>
        /// Mã quốc gia (ví dụ: "vn" cho Việt Nam, "us" cho Hoa Kỳ).
        /// Tham số này có thể được sử dụng để xác định vị trí địa lý của người dùng và điều chỉnh kết quả tìm kiếm theo quốc gia cụ thể.
        /// </summary> 
        /// <summary>
        /// 
        /// </summary>
        /// <value>
        /// 
        /// </value>
        public string? gl { get; set; }

        /// <summary>
        /// Vị trí tìm kiếm dưới dạng vĩ độ và kinh độ (ví dụ: "37.7749,-122.4194" cho San Francisco).
        /// </summary> <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string? location { get; set; }

        /// <summary>
        /// Ngôn ngữ của kết quả tìm kiếm (ví dụ: "en" cho tiếng Anh).
        /// </summary> <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string? hl { get; set; }

        /// <summary>
        /// Khoảng thời gian cho kết quả tìm kiếm (ví dụ: "w" cho tuần trước, "m" cho tháng trước).
        /// </summary> <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string? tbs { get; set; }

        /// <summary>
        /// Số lượng kết quả tìm kiếm để trả về (ví dụ: 10 cho 10 kết quả).
        /// </summary>
        /// <value></value>
        public int? num { get; set; } = 10;

        /// <summary>
        /// Loại tìm kiếm (ví dụ: "search" cho tìm kiếm thông thường).
        /// Tham số này có thể được sử dụng để xác định loại tìm kiếm mà người dùng đang thực hiện.
        /// </summary>
        /// <value></value>
        public string? type { get; set; } = "search";

        /// <summary>
        /// Vd: 'google', 'bing', 'yahoo', 'baidu', 'duckduckgo', 'yandex', 'ask', 'aol', 'ecosia', 'qwant'
        /// </summary> <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string? engine { get; set; }

        /// <summary>
        /// Cụm từ chính xác định loại tìm kiếm mà người dùng đang thực hiện.
        /// </summary>
        /// <returns></returns>
        [NotMapped]
        public string? correctPhrase { get; set; }

        /// <summary>
        /// Bất kỳ từ nào mà bạn muốn tìm kiếm trong kết quả tìm kiếm.
        /// </summary>
        /// <returns>Nhập OR giữa tất cả các từ mà bạn muốn: thu nhỏ OR tiêu chuẩn.</returns>
        [NotMapped]
        public string? anyWords { get; set; }

        /// <summary>
        /// Không có từ nào mà bạn không muốn tìm kiếm trong kết quả tìm kiếm.
        /// </summary>
        /// <returns>
        ///     Đặt dấu trừ ngay trước từ mà bạn không muốn: 
        ///         -loài gặm nhấm, 
        ///         -"Jack Russell"
        /// </returns>
        public string? notWords { get; set; }

        /// <summary>
        /// Trang web hoặc tên miền mà bạn muốn tìm kiếm trong kết quả tìm kiếm.
        /// </summary>
        /// <returns>
        ///     Tìm kiếm trên một trang web (như wikipedia.org ) hoặc giới hạn kết quả của bạn ở một miền như .edu, .org hoặc .gov.
        /// </returns>
        public string? site { get; set; }

        // Phương thức để xây dựng các tham số tìm kiếm
        public Dictionary<string, string> BuildRequestParams()
        {
            var parameters = new Dictionary<string, string>
            {
                { "q", (q ?? string.Empty) }
            };

            if (!string.IsNullOrEmpty(gl))
            {
                parameters.Add("gl", (gl));
            }
            if (!string.IsNullOrEmpty(location))
            {
                parameters.Add("location", (location));
            }
            if (!string.IsNullOrEmpty(hl))
            {
                parameters.Add("hl", (hl));
            }
            if (!string.IsNullOrEmpty(tbs))
            {
                parameters.Add("tbs", (tbs));
            }
            if (num.HasValue)
            {
                parameters.Add("num", num.Value.ToString());
            }
            if (!string.IsNullOrEmpty(engine))
            {
                parameters.Add("engine", (engine));
            }

            return parameters;
        }
        public GoogleResponse FilterGoogleResponse(GoogleResponse response)
        {
            // Lọc các kết quả tìm kiếm dựa trên các tham số đã cung cấp
            if (!string.IsNullOrEmpty(correctPhrase))
            {
                response.Organic = response.Organic.Where(item => item.Title.Contains(correctPhrase, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            if (!string.IsNullOrEmpty(anyWords))
            {
                var words = anyWords.Split(new[] { " OR " }, StringSplitOptions.RemoveEmptyEntries);
                response.Organic = response.Organic.Where(item => words.All(word => item.Title.Contains(word, StringComparison.OrdinalIgnoreCase))).ToList();
            }
            if (!string.IsNullOrEmpty(notWords))
            {
                var words = notWords.Split(new[] { " OR " }, StringSplitOptions.RemoveEmptyEntries);
                response.Organic = response.Organic.Where(item => !words.Any(word => item.Title.Contains(word, StringComparison.OrdinalIgnoreCase))).ToList();
            }
            if (!string.IsNullOrEmpty(site))
            {
                response.Organic = response.Organic.Where(item => item.Link.Contains(site, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            return response;
        }
        public GoogleRequest(string q, string? gl = null, string? location = null, string? hl = null, string? tbs = null, int? num = null, string? type = null, string? engine = null)
        {
            this.q = q;
            this.gl = gl;
            this.location = location;
            this.hl = hl;
            this.tbs = tbs;
            this.num = num;
            this.type = type;
            this.engine = engine;
        }
    }
}