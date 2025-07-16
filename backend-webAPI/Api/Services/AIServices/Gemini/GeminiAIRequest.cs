
using Newtonsoft.Json;

namespace Api.Services.AIServices.Gemini
{
    /// <summary>
    /// Represents a request to the Gemini AI model.
    /// </summary>
    public class GeminiAIRequest
    {
        #region Constructors

        private GeminiAIRequest() { }

        #endregion

        #region Static Factory Methods

        /// <summary>
        /// Creates a Gemini AI request with general content.
        /// </summary>
        /// <param name="content">The main content for the AI to process.</param>
        /// <param name="note">Optional: A note to include in the request.</param>
        /// <param name="isUseNote">Determines if the note should be included.</param>
        /// <returns>A new <see cref="GeminiAIRequest"/> instance.</returns>
        public static GeminiAIRequest CreateWithContent(string content, string note = "Do not answer as a request, just fulfill my request. Thank you.", bool isUseNote = false)
        {
            var req = new GeminiAIRequest();
            req.InitializeContentRequest($"{content}{(isUseNote ? $"\nNote: {note}" : string.Empty)}");
            return req;
        }

        /// <summary>
        /// Creates a Gemini AI request for query-based analysis, including a prompt for data analysis.
        /// </summary>
        /// <param name="query">The search query related to the analysis.</param>
        /// <param name="prompt">The data from the API to be analyzed.</param>
        /// <returns>A new <see cref="GeminiAIRequest"/> instance.</returns>
        public static GeminiAIRequest CreateWithQueryAndPrompt(string query, string prompt)
        {
            var req = new GeminiAIRequest();
            var initialPrompt = $"Act as a data analysis expert. Below is information from the API, analyze it generally without explaining each content: {prompt}";
            var queryPrompt = $"Analyze information related to the keyword '{query}'.";
            var noDataPrompt = "If there is no relevant data, please state the reason why the information could not be found.";

            req.InitializeContentRequest(initialPrompt, queryPrompt, noDataPrompt);
            req.AddCommonPrompts(query);
            return req;
        }

        /// <summary>
        /// Creates a Gemini AI request for general prompting or link analysis.
        /// </summary>
        /// <param name="prompt">The prompt text or link to be analyzed.</param>
        /// <param name="isNormalPrompt">If true, treats the prompt as a normal text prompt; otherwise, treats it as a link for analysis.</param>
        /// <returns>A new <see cref="GeminiAIRequest"/> instance.</returns>
        public static GeminiAIRequest CreateWithPrompt(string prompt, bool isNormalPrompt = false)
        {
            var req = new GeminiAIRequest();
            if (isNormalPrompt)
            {
                req.InitializeContentRequest(prompt);
                req.AddCommonPrompts();
                return req;
            }
            var linkAnalysisPrompt = $"Provide a brief response (200 characters) about the website status at: {prompt}. Summarize information such as tax codes, business status." +
                                      "\nIf it's not business information, analyze it as a normal website.";

            req.InitializeContentRequest(linkAnalysisPrompt);
            return req;
        }

        /// <summary>
        /// Creates a Gemini AI request for content media analysis.
        /// </summary>
        /// <param name="linkMediaSocial">The social media link.</param>
        /// <param name="contentMedia">The extracted content from the media.</param>
        /// <returns>A new <see cref="GeminiAIRequest"/> instance.</returns>
        public static GeminiAIRequest CreateWithContentMediaPrompt(string linkMediaSocial, string contentMedia)
        {
            var req = new GeminiAIRequest();
            string mediaPrompt = $"This is a social media post/video link, and this is the main video content of this post: '{contentMedia}'. If you don't receive any content in parentheses, it's likely that the tool has no content or the audio in the video is too noisy to scan. You will analyze other content from the social media link and recommend users to watch the video directly for better understanding.";
            req.InitializeContentRequest(mediaPrompt);
            req.Contents[0].AddIgnoreTrashInfoPrompt();
            return req;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Initializes the content request with an array of parts.
        /// </summary>
        /// <param name="parts">An array of strings representing parts of the content.</param>
        private void InitializeContentRequest(params string[] parts)
        {
            var contentRequest = new ContentRequest();
            foreach (var part in parts)
            {
                contentRequest.AddPart(part);
            }
            this.Contents.Add(contentRequest);
        }

        /// <summary>
        /// Adds common prompts to the last content request.
        /// </summary>
        /// <param name="query">Optional: The search query to include in the prompt.</param>
        private void AddCommonPrompts(string? query = null)
        {
            var contentRequest = this.Contents.Last();
            contentRequest.AddAnalysisPrompt();
            contentRequest.AddIgnoreTrashInfoPrompt();
            if (query != null)
            {
                contentRequest.AddPromptQuerySearch(query);
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the list of content requests for the Gemini AI model.
        /// </summary>
        [JsonProperty("contents")]
        public List<ContentRequest> Contents { get; set; } = new List<ContentRequest>();

        #endregion
    }

    /// <summary>
    /// Represents a content request within a Gemini AI request.
    /// </summary>
    public class ContentRequest
    {
        #region Properties

        /// <summary>
        /// Gets or sets the list of parts within the content request.
        /// </summary>
        [JsonProperty("parts")]
        public List<PartRequest> Parts { get; set; } = new List<PartRequest>();

        #endregion

        #region Public Methods

        /// <summary>
        /// Adds a text part to the content request.
        /// </summary>
        /// <param name="text">The text content to add.</param>
        public void AddPart(string text)
        {
            Parts.Add(new PartRequest(text));
        }

        /// <summary>
        /// Adds a prompt part indicating the user's search query.
        /// </summary>
        /// <param name="query">The user's search query.</param>
        public void AddPromptQuerySearch(string query)
        {
            Parts.Add(new PartRequest($"User searched for information by keyword: '{query}'"));
        }

        /// <summary>
        /// Adds a prompt part to ignore irrelevant content.
        /// </summary>
        public void AddIgnoreTrashInfoPrompt()
        {
            Parts.Add(new PartRequest("Ignore irrelevant content."));
        }

        /// <summary>
        /// Adds a prompt part to instruct the AI to generate a structured JSON analysis.
        /// </summary>
        public void AddAnalysisPrompt()
{
    Parts.Add(new PartRequest(
        "Hãy phân tích và đánh giá doanh nghiệp dựa trên dữ liệu dưới dạng JSON. Nội dung JSON phải được viết bằng tiếng Việt, rõ ràng, súc tích, chuyên nghiệp và thân thiện với người dùng, phù hợp để hiển thị trong ứng dụng Angular với AngularCSS. Đảm bảo tất cả các giá trị chuỗi được thoát đúng cách cho JSON.\n\n" +
        "Các tiêu chí đánh giá tập trung vào: chất lượng sản phẩm/dịch vụ, uy tín doanh nghiệp, phản hồi của khách hàng, điểm mạnh, điểm yếu, đề xuất cải thiện.\n\n" +
        "Cấu trúc JSON:\n{\n" +
        "    \"tieuDe\": \"[Tiêu đề nội dung phân tích doanh nghiệp]\",\n" +
        "    \"nguonDuLieu\": \"[Nguồn dữ liệu]\",\n" +
        "    \"danhGia\": {\n" +
        "        \"tichCuc\": {\n" +
        "            \"soLuong\": [Số lượng bài viết tích cực],\n" +
        "            \"baiViet\": [\n" +
        "                {\n" +
        "                    \"tomTat\": \"[Tóm tắt bài viết tích cực về doanh nghiệp, ngắn gọn, dễ đọc, tập trung vào điểm mạnh, dịch vụ tốt, uy tín, phản hồi tốt từ khách hàng...]\",\n" +
        "                    \"lyDo\": \"[Lý do đánh giá tích cực, ví dụ: dịch vụ chuyên nghiệp, sản phẩm chất lượng, chăm sóc khách hàng tốt...]\",\n" +
        "                    \"lienKet\": \"[Liên kết đến bài viết]\"\n" +
        "                }\n" +
        "            ]\n" +
        "        },\n" +
        "        \"tieuCuc\": {\n" +
        "            \"soLuong\": [Số lượng bài viết tiêu cực],\n" +
        "            \"baiViet\": [\n" +
        "                {\n" +
        "                    \"tomTat\": \"[Tóm tắt bài viết tiêu cực về doanh nghiệp, ngắn gọn, dễ đọc, tập trung vào điểm yếu, vấn đề gặp phải, phản hồi chưa tốt...]\",\n" +
        "                    \"lyDo\": \"[Lý do đánh giá tiêu cực, ví dụ: dịch vụ chậm, sản phẩm chưa đạt kỳ vọng, phản hồi khách hàng chưa tốt...]\",\n" +
        "                    \"lienKet\": \"[Liên kết đến bài viết]\"\n" +
        "                }\n" +
        "            ]\n" +
        "        },\n" +
        "        \"danhGiaChung\": \"[Nội dung đánh giá tổng quan doanh nghiệp, tổng hợp ưu điểm, nhược điểm, đề xuất cải thiện nếu có, trình bày ngắn gọn, dễ hiểu]\"\n" +
        "    },\n" +
        "    \"thongBaoKhongCoDuLieu\": \"[Thông báo thân thiện nếu không tìm thấy dữ liệu liên quan, ví dụ: 'Không tìm thấy dữ liệu liên quan cho doanh nghiệp này.']\"\n" +
        "}\n\n" +
        "Chỉ phản hồi bằng nội dung JSON, không giải thích gì thêm."
    ));
}

        #endregion
    }

    /// <summary>
    /// Represents a part of a content request.
    /// </summary>
    public class PartRequest
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PartRequest"/> class.
        /// </summary>
        /// <param name="text">The text content of the part.</param>
        public PartRequest(string text)
        {
            Text = text;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the text content of the part.
        /// </summary>
        [JsonProperty("text")]
        public string Text { get; set; } = string.Empty;

        #endregion
    }
}
