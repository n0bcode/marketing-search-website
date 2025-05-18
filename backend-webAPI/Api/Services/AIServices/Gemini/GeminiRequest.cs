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
            var initialPrompt = $"Đóng vai trò là chuyên gia phân tích dữ liệu. Dưới đây là thông tin từ API, hãy phân tích chung mà không cần giải thích từng nội dung: {prompt}";
            var queryPrompt = $"Phân tích thông tin liên quan đến từ khóa '{query}'.";
            var noDataPrompt = "Nếu không có dữ liệu liên quan, hãy cho biết lý do khả năng không tìm thấy thông tin.";

            InitializeContentRequest(initialPrompt, queryPrompt, noDataPrompt);
            AddCommonPrompts(query, true);
        }

        public GeminiRequest(string prompt, bool isNormalPrompt = false)
        {
            if (isNormalPrompt)
            {
                InitializeContentRequest(prompt);
                AddCommonPrompts();
                return;
            }
            var linkAnalysisPrompt = $"Cung cấp phản hồi ngắn gọn (200 ký tự) về tình trạng trang web tại: {prompt}. Tóm tắt thông tin như mã số thuế, tình trạng doanh nghiệp." +
                                      "\nNếu không phải thông tin doanh nghiệp, cần phân tích như một trang web thông thường.";

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
            contentRequest.AddAnalysisPrompt();
            contentRequest.AddDotmarkPrompt();
            contentRequest.AddIgnoreTrashInfoPrompt();
            if (isFormatWithHtml)
            {
                contentRequest.AddFormatHtmlPrompt();
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

        public void AddIgnoreTrashInfoPrompt()
        {
            Parts.Add(new PartRequest("Bỏ qua các nội dung không liên quan."));
        }
        public void AddAnalysisPrompt()
        {
            Parts.Add(new PartRequest("Phân tích dữ liệu theo cấu trúc dưới đây cho từ khóa tìm kiếm. Phân loại các đánh giá thành năm mức độ: Rất tích cực, Tích cực, Bình thường, Tiêu cực, Rất tiêu cực. Cấu trúc phân tích sẽ được tổ chức như sau:" +
                "\n\nTiêu đề nội dung phân tích:" +
                "\nNguồn gốc:" +
                "\nĐánh giá kết quả:" +
                "\n    - Tích cực [Số bài tích cực]:" +
                "\n        + [link website] (Đánh giá web site vì sao tích cực)" +
                "\n        + ..." +
                "\n    - Tiêu cực [Số bài tiêu cực]:" +
                "\n        + [link website] (Lí do)" +
                "\n        + ..." +
                "\nĐánh giá chung:"
            ));
        }


        public void AddFormatHtmlPrompt()
        {
            Parts.Add(new PartRequest("Gửi dữ liệu phân tích với định dạng HTML, sử dụng các class từ tailwindcss để trình bày nội dung phân tích. Chỉ phản hồi nội dung HTML mà không cần giải thích. Không cần format nhiều class cho khung div ngoài cùng."));
        }

        public void AddDotmarkPrompt()
        {
            Parts.Add(new PartRequest("Sử dụng chức năng dotmark để định dạng nội dung."));
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
