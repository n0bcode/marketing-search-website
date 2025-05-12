using Newtonsoft.Json;

namespace Api.Services.AIServices.Gemini
{
    public class GeminiRequest
    {
        public GeminiRequest(string content, string note = "Không cần trả lời theo yêu cầu mà chỉ thực hiện yêu cầu của tôi. Cảm ơn.", bool isUseNote = false)
        {
            var contentRequest = new ContentRequest();
            contentRequest.AddPart(content);
            if (isUseNote)
            {
                contentRequest.AddPart($"Chú ý: {note}");
            }
            this.Contents.Add(contentRequest);
        }

        public GeminiRequest(string query, string prompt)
        {
            // Khởi tạo ContentRequest với prompt
            var contentRequest = new ContentRequest();
            contentRequest.AddPart($"Đóng vai trò là chuyên gia phân tích dữ liệu. Dưới đây là thông tin từ API: {prompt}");
            contentRequest.AddPart($"Phân tích và cung cấp thông tin liên quan đến từ khóa '{query}' mà không cần giải thích thêm.");
            contentRequest.AddPart("Không cần phản hồi lại thông tin prompt, hãy phản hồi như một chuyên gia.");
            contentRequest.AddPart("Lưu ý: Cung cấp thông tin cho các phản hồi sau trong khoảng 100 chữ.");
            contentRequest.AddPart($"Nếu không có dữ liệu nào liên quan đến truy vấn, hãy giải thích lý do có thể là do API không tìm thấy thông tin hoặc giới hạn token tài khoản.");
            contentRequest.AddPromptQuerySearch(query);
            contentRequest.AddPart(prompt);
            contentRequest.AddDefaultRelatedPrompt();
            contentRequest.AddAnalysisPromptGoodAndBad();
            contentRequest.AddDefaultIgnoreTrashInfoPrompt();
            this.Contents.Add(contentRequest);
        }

        public GeminiRequest(string link)
        {
            var contentRequest = new ContentRequest();
            contentRequest.AddPart("Cung cấp phản hồi ngắn gọn (200 ký tự) về tình trạng trang web tại đường dẫn này.");
            contentRequest.AddPart("Tóm tắt thông tin hữu ích như mã số thuế, tình trạng doanh nghiệp.");
            contentRequest.AddPart($"Đường dẫn trang web cần phân tích: {link}. Tổng hợp và đánh giá thông tin liên quan đến doanh nghiệp.");
            contentRequest.AddPart("Nếu không phải là thông tin doanh nghiệp, hãy phân tích như một trang web bình thường.");
            contentRequest.AddAnalysisPromptGoodAndBad();
            contentRequest.AddDefaultIgnoreTrashInfoPrompt();
            this.Contents.Add(contentRequest);
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
            Parts.Add(new PartRequest("Phân tích tốt xấu hoặc trung bình dựa trên đánh giá của người dùng về từ khóa tìm kiếm."));
        }

        public void AddDefaultRelatedPrompt()
        {
            Parts.Add(new PartRequest("Phân tích chung về các thông tin được tìm thấy nhiều nhất và lập mục 'Từ khóa liên quan' ở cuối nội dung."));
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
