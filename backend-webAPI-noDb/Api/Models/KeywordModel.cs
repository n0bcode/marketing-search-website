using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.DataAnnotations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Api.Services.AIServices.Gemini;


namespace Api.Models
{
    public class KeywordModel
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString(); // Ensure Id is set to a new Guid

        [Required]
        [StringLength(100)]
        public required string Keyword { get; set; } = string.Empty;
        public List<string> RelatedKeyword { get; set; } = [];
        [Required]
        public required string Source { get; set; } = string.Empty;
        public string Term { get; set; } = string.Empty;
        public string SocialMediaInfo { get; set; } = string.Empty;
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        [Required]
        public string Note { get; set; } = string.Empty;

    }
}