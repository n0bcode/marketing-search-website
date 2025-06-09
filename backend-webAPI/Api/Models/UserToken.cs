using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Api.Constants;
using MongoDB.Bson.Serialization.Attributes;

namespace Api.Models
{
    /// <summary>
    /// Lớp đại diện cho một token mà người dùng thay thế cho token mặc định được dùng chạy cho dịch vụ trong hệ thống.
    /// </summary>
    public class UserToken
    {
        [Key]
        [BsonId]
        public int Id { get; set; } // Khóa chính

        /// <summary>
        ///     Token mà người dùng cung cập cho dịch vụ.
        ///     Token này sẽ được dùng để thay thế cho token mặc định được dùng chạy cho dịch vụ trong hệ thống.
        /// </summary>
        [Required]
        public string Token { get; set; } = string.Empty;

        public string? IpAddress { get; set; } // Địa chỉ IP của người dùng
        public string? UserAgent { get; set; } // Thông tin trình duyệt của người dùng

        [RegularExpression($"{TypeServices.GoogleSearch}, {TypeServices.TwitterSearch}, {TypeServices.GeminiAI}, {TypeServices.GoogleSender}, {TypeServices.TwilioSender}")]
        public string? TypeService { get; set; } // Loại dịch vụ mà người dùng cung cấp token thay thế

        public DateTime CreatedAt { get; set; } // Thời gian tạo token

        public DateTime UpdatedAt { get; set; } // Thời gian sửa token gần nhất
    }
}