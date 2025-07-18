
using System;
using Api.Services;
using Microsoft.OpenApi.Models;
using System.Reflection;
using Api.Services.SearchServices;
using Api.Services.SearchServices.Google;
using Api.Services.SearchServices.Twitter;
using Api.Services.AIServices.Gemini;

using Api.Services.VideoServices;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System.IO;
using Microsoft.Extensions.Hosting;
using Api.Automations;
using Api.Repositories.MongoDb;
using Api.Data;

namespace Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddHttpClient<GoogleSearchService>();
            builder.Services.AddHttpClient<TwitterSearchService>();
            builder.Services.AddScoped<BingSearchService>();

            builder.Services.AddHttpClient<GeminiAIService>();

            builder.Services.AddScoped<SeleniumManager>();
            builder.Services.AddScoped<VideoProcessingService>();

            builder.Services.AddSingleton<MongoDbContext>();

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "MarketingSearchAPI", Version = "v1" });
                #region Format thêm comment lên môi action
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
                #endregion
            });
            // Gắn ApiSettings vào DI
            builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection("ApiSettings"));

            builder.Services.AddScoped<IUnitOfWorkMongo, UnitOfWorkMongo>();

            builder.Services.AddScoped<VideoProcessingService>();

            builder.Services.AddMemoryCache();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            // ... bên trong method Configure
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseCors(options =>
            {
                options.AllowAnyOrigin();
                options.AllowAnyHeader();
                options.AllowAnyMethod();
            });

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
