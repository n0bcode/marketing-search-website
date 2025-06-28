using Newtonsoft.Json;

namespace Api.Services.AIServices.Gemini
{
    public class GeminiAIRequest
    {
        private GeminiAIRequest() { }

        public static GeminiAIRequest CreateWithContent(string content, string note = "Không cần trả lời theo yêu cầu mà chỉ thực hiện yêu cầu của tôi. Cảm ơn.", bool isUseNote = false)
        {
            var req = new GeminiAIRequest();
            req.InitializeContentRequest($"{content}{(isUseNote ? $"\nChú ý: {note}" : string.Empty)}");
            return req;
        }
        public static GeminiAIRequest CreateWithQueryAndPrompt(string query, string prompt)
        {
            var req = new GeminiAIRequest();
            var initialPrompt = $"Đóng vai trò là chuyên gia phân tích dữ liệu. Dưới đây là thông tin từ API, hãy phân tích chung mà không cần giải thích từng nội dung: {prompt}";
            var queryPrompt = $"Phân tích thông tin liên quan đến từ khóa '{query}'.";
            var noDataPrompt = "Nếu không có dữ liệu liên quan, hãy cho biết lý do khả năng không tìm thấy thông tin.";

            req.InitializeContentRequest(initialPrompt, queryPrompt, noDataPrompt);
            req.AddCommonPrompts(query, true);
            return req;
        }

        public static GeminiAIRequest CreateWithPrompt(string prompt, bool isNormalPrompt = false)
        {
            var req = new GeminiAIRequest();
            if (isNormalPrompt)
            {
                req.InitializeContentRequest(prompt);
                req.AddCommonPrompts();
                return req;
            }
            var linkAnalysisPrompt = $"Cung cấp phản hồi ngắn gọn (200 ký tự) về tình trạng trang web tại: {prompt}. Tóm tắt thông tin như mã số thuế, tình trạng doanh nghiệp." +
                                      "\nNếu không phải thông tin doanh nghiệp, cần phân tích như một trang web thông thường.";

            req.InitializeContentRequest(linkAnalysisPrompt);
            req.AddCommonPrompts();
            return req;
        }
        public static GeminiAIRequest CreateWithContentMediaPrompt(string linkMediaSocial, string contentMedia)
        {
            var req = new GeminiAIRequest();
            string mediaPrompt = $"Đây là link bài viết/video của 1 nền tảng mạng xã hội, đây là nội dung của video chính của bài viết này : '{contentMedia}'. Nếu bạn không nhận được nội dung nào trong ngoặc đơn thì có lẽ công cụ không có nội dung hoặc âm thanh trong video có quá nhiều tạp âm nên không thể quét. Bạn sẽ đề phân tích các nội dung khác từ link socical media và đề cử người dùng có thể trực tiếp xem video để hiểu rõ hơn.";
            req.InitializeContentRequest(mediaPrompt);
            req.Contents[0].AddIgnoreTrashInfoPrompt();
            return req;
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
                "\n        + [Tóm tắt tiêu đề với khung a có link bao phủ] (Đánh giá web site vì sao tích cực)" +
                "\n        + ..." +
                "\n    - Tiêu cực [Số bài tiêu cực]:" +
                "\n        + [Tóm tắt tiêu đề với khung a có link bao phủ] (Lí do)" +
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
