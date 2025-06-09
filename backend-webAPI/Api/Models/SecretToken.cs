using System;
using System.ComponentModel.DataAnnotations;
using Api.DTOs.SecretTokenDTO;
using MongoDB.Bson.Serialization.Attributes;

namespace Api.Models
{
    public class SecretToken
    {
        [Key]
        [BsonId]
        public string Id { get; set; } = Guid.NewGuid().ToString(); // Sử dụng GUID cho Id

        [Required]
        [StringLength(100)] // Giới hạn chiều dài
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(500)] // Giới hạn chiều dài
        public string Token { get; set; } = string.Empty; // Mã hóa token

        [Required]
        public string Service { get; set; } = string.Empty;

        public string Note { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true; // Trạng thái token
    }
    public static class SecretTokenReponseDTOExtensions
    {
        public static SecretTokenResponseDTO Format(this SecretToken secretToken)
        {
            return new SecretTokenResponseDTO
            (
                secretToken.Id,
                secretToken.Name,
                secretToken.Service,
                secretToken.Note
            );
        }
    }
}
