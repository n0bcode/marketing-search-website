using Newtonsoft.Json;
using System.Collections.Generic;

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
                contentRequest.AddPart($"Cần chú ý: {note}");
            }
            this.Contents.Add(contentRequest);
        }
        public GeminiRequest(string query, string prompt)
        {
            // Khởi tạo ContentRequest với prompt
            var contentRequest = new ContentRequest();
            contentRequest.AddPart($"Hãy đóng vai trò là một chuyên gia phân tích dữ liệu. Dưới đây là các thông tin được tìm thấy từ API: {prompt}");
            contentRequest.AddPart($"Bạn không cần phải giải thích về các thông tin này, mà chỉ cần phân tích và đưa ra các thông tin liên quan đến từ khóa '{query}'.");
            contentRequest.AddPart($"Hãy phân tích các thông tin này và đưa ra các thông tin liên quan đến từ khóa '{query}'.");
            contentRequest.AddPart("Không cần phản hồi lại những thông tin prompt được gửi đến này, hãy phản hồi như 1 chuyên gia mà không cần có nhấn mạnh gì cả.");
            contentRequest.AddPart("Ngoài ra, tôi cũng sẽ lưu dữ liệu này vào CSDL, nên ở dòng nội dung cuối theo sau các từ khóa liên quan, hãy tạo thêm 1 đoạn chữ như nhắc nhở cung cấp thông tin cho các phản hồi sau đó của bạn trong khoảng 100 chữ.");
            contentRequest.AddPart($"Nếu bạn không nhận được bất cứ dữ liệu nào theo sau truy vấn ở dưới thì có thể là do API không tìm thấy thông tin nào liên quan liên quan đến truy vấn của người dùng, hoặc có thể là do giới hạn của token tài khoản được tích hợp vào hệ thống cho việc tìm kiếm, bạn hãy giải thích dễ hiểu về các khả nằng có thể xảy ra để giải thích về việc tại sao người dùng không thấy phản hồi tìm kiếm của họ. Cảm ơn vì đã hỗ trợ.");
            contentRequest.AddPromptQuerySearch(query);
            contentRequest.AddPart(prompt);
            contentRequest.AddDefaultRelatedPrompt();
            contentRequest.AddDefaultIgnoreTrashInfoPrompt();
            this.Contents.Add(contentRequest);
        }
        public GeminiRequest(string link)
        {
            // Khởi tạo ContentRequest với prompt
            var contentRequest = new ContentRequest();
            contentRequest.AddPart("Vui lòng cung cấp phản hồi ngắn gọn trong 200 ký tự theo phong cách Marketing về tình trạng của trang web cho doanh nghiệp tại đường dẫn này.");
            contentRequest.AddPart("Ngoài ra, hãy tóm tắt những thông tin hữu ích cơ bản mà người dùng có thể quan tâm trên trang web này như trạng thái mã số thuế, tình trạng doanh nghiệp v.v.");
            contentRequest.AddPart($"Đây là đường dẫn tới trang web mà bạn cần phân tích: {link}. Hãy tổng hợp và đánh giá thông tin liên quan đến lĩnh vực của doanh nghiệp này.");
            contentRequest.AddPart("Nếu không phải đường dẫn có thông tin liên quan đến doanh nghiệp thì phân tích theo hướng khác như là một trang web bình thường.");
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
        public ContentRequest()
        {
            // Khởi tạo danh sách Parts
            Parts = new List<PartRequest>();
        }


        public void AddPart(string text)
        {
            Parts.Add(new PartRequest(text));
        }

        public void AddPromptQuerySearch(string query)
        {
            Parts.Add(new PartRequest($"Người dùng đã tìm kiếm thông tin theo từ khóa: '{query}'"));
        }

        public void AddDefaultIgnoreTrashInfoPrompt()
        {
            Parts.Add(new PartRequest("Bỏ phân tích các nội dung rác."));
        }
        public void AddAnalysisPromptGoodAndBad()
        {
            Parts.Add(new PartRequest("Thêm phân tích về tốt xấu hoặc trung bình dựa vào đánh giá của người dùng về từ khóa tìm kiếm này này."));
        }

        public void AddDefaultRelatedPrompt()
        {
            Parts.Add(new PartRequest("Theo dữ liệu API được gửi, hãy phân tích chung về các thông tin được tìm thấy nhiều nhất. Những thông tin khác thì lập 1 mục 'Từ khóa liên quan' đặt ở cuối nội dung phân tích."));
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