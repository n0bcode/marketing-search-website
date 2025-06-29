using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Api.Models;

namespace Api.DTOs
{
    public class SecretTokenRequestDTO
    {
        [Required]
        [StringLength(100)] // Giới hạn chiều dài
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(500)] // Giới hạn chiều dài
        public string Token { get; set; } = string.Empty; // Mã hóa token

        [Required]
        public string Service { get; set; } = string.Empty;

        public string Note { get; set; } = string.Empty;
        public SecretTokenRequestDTO(string name, string token, string service, string note)
        {
            Name = name;
            Token = token;
            Service = service;
            Note = note;
        }
    }

    public static class SecretTokenRequestDTOExtensions
    {
        public static SecretToken Format(this SecretTokenRequestDTO secretToken)
        {
            return new SecretToken
            {
                Name = secretToken.Name,
                Token = secretToken.Token,
                Service = secretToken.Service,
                Note = secretToken.Note
            };
        }
    }
}
