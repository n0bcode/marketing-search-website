using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Api.Models;
using Api.Services.SearchServices;
using Api.Services.AIServices.Gemini;
using Newtonsoft.Json;
using Api.Utils;

namespace Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<KeywordModel> Keywords { get; set; }
        // public DbSet<GeminiAIResponse> GeminiAIResponses { get; set; }
        public DbSet<AnalysisLink> AnalysisLinks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<KeywordModel>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Keyword).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Source).IsRequired();
                entity.Property(e => e.Term).IsRequired();
                entity.Property(e => e.SocialMediaInfo).IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.UpdatedAt).IsRequired();
                entity.Property(e => e.Note).IsRequired().HasMaxLength(500);
            });

            // modelBuilder.Entity<GeminiAIResponse>(entity =>
            // {
            //     entity.HasKey(e => e.Id);
            //     entity.Property(e => e.ModelVersion).IsRequired().HasMaxLength(50);
            //     entity.Property(e => e.Note).IsRequired().HasMaxLength(500);
            //     entity.Property(e => e.CreatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
            //     entity.Property(e => e.UpdatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP");
            //     entity.Property(e => e.KeywordId).IsRequired().HasMaxLength(50); // Ensure this matches the type in KeywordModel
            //     entity.Property(e => e.Candidates).HasConversion(new JsonDbConverter()).IsRequired(); // Assuming Candidates is a JSON string or similar
            //     entity.Property(e => e.UsageMetadata).HasConversion(new JsonDbConverter()).IsRequired(); // Assuming UsageMetadata is a JSON string or similar

            //     entity.HasOne(e => e.Keyword)
            //           .WithMany(e => e.GeminiAIResponses)
            //           .HasForeignKey(e => e.KeywordId)
            //           .OnDelete(DeleteBehavior.Cascade); // Optional: Define the delete behavior
            // });

            modelBuilder.Entity<AnalysisLink>(entity =>
            {
                entity.HasKey(e => e.Id);
            });
        }
    }
}