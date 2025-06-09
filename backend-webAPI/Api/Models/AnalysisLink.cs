using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;

namespace Api.Models
{
    public class AnalysisLink
    {
        [Key]
        [BsonId]
        public int Id { get; set; }
        public string LinkOrKeyword { get; set; } = string.Empty;
        public string Platform { get; set; } = string.Empty;
        public string ResultData { get; set; } = string.Empty; // Lưu JSON kết quả
        public string AnalysisText { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}