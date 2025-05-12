namespace Api.Services.SearchServices
{
    public class ErrorResponse
    {
        public string Title { get;set; } = string.Empty;
        public string Detail { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int Status { get; set; }
    }
}
