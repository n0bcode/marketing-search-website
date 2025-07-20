using System;
using System.Collections.Generic;

namespace Api.Models
{
    public class ResponseAPI<T>
    {
        public bool Success { get; set; } = false;
        public int StatusCode { get; set; } = 500; // Mặc định là lỗi server
        public string Message { get; set; } = "Phản hồi không xác định";
        public T? Data { get; set; }
        public List<string> Errors { get; set; } = new List<string>();

        public void SetSuccessResponse(string? message = "Dữ liệu đã xử lí thành công!", int statusCode = 200, T? data = default(T))
        {
            this.Success = true;
            this.StatusCode = statusCode;
            this.Message = message ?? "Dữ liệu đã xử lí thành công!";
            Data = data;
        }

        public void SetData(T dataSet)
        {
            this.Data = dataSet;
        }

        public void SetErrorResponse(Exception ex = null!, int statusCode = 500)
        {
            this.Success = false;
            this.StatusCode = statusCode;
            this.Message = ex?.Message ?? "Lỗi không xác định";
        }

        public void SetUnauthorizedResponse(int statusCode = 401)
        {
            this.Success = false;
            this.StatusCode = statusCode;
            this.Message = "Vui lòng truy cập để sử dụng tính năng.";
        }

        public void AddError(string error)
        {
            this.Errors.Add(error);
        }
    }
}
