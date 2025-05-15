using Newtonsoft.Json;

namespace Api.Services.AIServices.Gemini
{
    public class GeminiRequest
    {
        public GeminiRequest(string content, string note = "Không cần trả lời theo yêu cầu mà chỉ thực hiện yêu cầu của tôi. Cảm ơn.", bool isUseNote = false)
        {
            InitializeContentRequest($"{content}{(isUseNote ? $"\nChú ý: {note}" : string.Empty)}");
        }

        public GeminiRequest(string query, string prompt)
        {
            var initialPrompt = $"Đóng vai trò là chuyên gia phân tích dữ liệu. Dưới đây là thông tin từ API, không cần phân tích riêng từng nội dung tìm kiếm mà hãy phân tích chung: {prompt}";
            var queryPrompt = $"Phân tích và cung cấp thông tin liên quan đến từ khóa '{query}' mà không cần giải thích thêm.";
            var noDataPrompt = "Nếu không có dữ liệu nào liên quan đến truy vấn, hãy giải thích lý do có thể là do API không tìm thấy thông tin, giới hạn token tài khoản.";

            InitializeContentRequest(initialPrompt, queryPrompt, noDataPrompt);
            AddCommonPrompts(query, true);
        }

        public GeminiRequest(string link)
        {
            var linkAnalysisPrompt = $"Cung cấp phản hồi ngắn gọn (200 ký tự) về tình trạng trang web tại đường dẫn này." +
                                      $"\nTóm tắt thông tin hữu ích như mã số thuế, tình trạng doanh nghiệp." +
                                      $"\nĐường dẫn trang web cần phân tích: {link}. Tổng hợp và đánh giá thông tin liên quan đến doanh nghiệp." +
                                      "\nNếu không phải là thông tin doanh nghiệp, hãy phân tích như một trang web bình thường.";

            InitializeContentRequest(linkAnalysisPrompt);
            AddCommonPrompts();
        }

        private void InitializeContentRequest(params string[] parts)
        {
            var contentRequest = new ContentRequest();
            foreach (var part in parts)
            {
                contentRequest.AddPart(part);
            }
            this.Contents.Add(contentRequest);
        }

        private void AddCommonPrompts(string? query = null, bool isFormatWithHtml = false)
        {
            var contentRequest = this.Contents.Last();
            contentRequest.AddAnalysisPromptGoodAndBad();
            contentRequest.AddDotmarkUsefulPrompt();
            contentRequest.AddDefaultIgnoreTrashInfoPrompt();
            if (isFormatWithHtml)
            {
                contentRequest.AddFormatHtmllPrompt();
            }
            if (query != null)
            {
                contentRequest.AddPromptQuerySearch(query);
            }
        }

        [JsonProperty("contents")]
        public List<ContentRequest> Contents { get; set; } = new List<ContentRequest>();
    }

    public class ContentRequest
    {
        [JsonProperty("parts")]
        public List<PartRequest> Parts { get; set; } = new List<PartRequest>();

        public void AddPart(string text)
        {
            Parts.Add(new PartRequest(text));
        }

        public void AddPromptQuerySearch(string query)
        {
            Parts.Add(new PartRequest($"Người dùng tìm kiếm thông tin theo từ khóa: '{query}'"));
        }

        public void AddDefaultIgnoreTrashInfoPrompt()
        {
            Parts.Add(new PartRequest("Bỏ qua các nội dung không liên quan."));
        }

        public void AddAnalysisPromptGoodAndBad()
        {
            Parts.Add(new PartRequest("Đọc toàn bộ nội dung trong trang web và phân tích tốt xấu hoặc trung bình dựa trên đánh giá của người dùng hoặc bài viết đối với từ khóa tìm kiếm."));
        }

        public void AddFormatHtmllPrompt()
        {
            Parts.Add(new PartRequest("Gửi dữ liệu phân tích với định dạng HTML, không cần có khung html mà chỉ phản hồi body, sử dụng các class từ tailwindcss để format nội dung. Không cần import các thư viện khác. Không cần bao phủ nội dung phản hồi với kiểu code ```html ``` mà chỉ cần phản hồi nội dung html. Không cần có các thẻ head, style, body, html. Chỉ phản hồi với dữ liệu html mà không cần giải thích thêm."));
        }

        public void AddDotmarkUsefulPrompt()
        {
            Parts.Add(new PartRequest("Sử dụng tối đa chức năng của dotmark về việc xuống dòng, đánh dấu nội dung. Vì tôi sử dụng dotmark-it để format về dạng html."));
        }
    }

    public class PartRequest
    {
        public PartRequest(string text)
        {
            Text = text;
        }

        [JsonProperty("text")]
        public string Text { get; set; } = string.Empty;
    }
}
