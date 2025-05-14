using System;
using System.ComponentModel.DataAnnotations;

namespace Api.Models
{
    public class SecretToken
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid(); // Sử dụng GUID cho Id

        [Required]
        [StringLength(100)] // Giới hạn chiều dài
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(500)] // Giới hạn chiều dài
        public string Token { get; set; } = string.Empty; // Nên mã hóa token

        [Required]
        public string Service { get; set; } = string.Empty;

        public string Note { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true; // Trạng thái token

        public DateTime? ExpirationDate { get; set; } // Ngày hết hạn
    }
}
