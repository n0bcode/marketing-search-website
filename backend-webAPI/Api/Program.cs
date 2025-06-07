
using System;
using Api.Data;
using Api.Services;
using Microsoft.CodeAnalysis.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Reflection;
using Api.Services.SearchServices;
using Api.Services.SearchServices.Google;
using Api.Services.SearchServices.Twitter;
using Api.Services.AIServices.Gemini;
using Api.Repositories;
using Api.Repositories.IRepositories;
using Api.Services.RedisCacheService;
using Api.DbInitializer;
using Api.Services.VideoServices;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System.IO;
using Microsoft.Extensions.Hosting;
using Api.Automations;

namespace Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddHttpClient<GoogleSearchService>();
            builder.Services.AddHttpClient<TwitterSearchService>();

            builder.Services.AddHttpClient<GeminiAIService>();

            builder.Services.AddScoped<SeleniumManager>();
            builder.Services.AddScoped<VideoProcessingService>();

            builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

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

            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

            builder.Services.AddSingleton<IRedisCacheService, RedisCacheService>();
            builder.Services.AddScoped<VideoProcessingService>();

            builder.Services.AddScoped<IDbInitializer, DbInitializer.DbInitializer>();

            builder.Services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = builder.Configuration["Redis:Configuration"];
                options.InstanceName = builder.Configuration["Redis:InstanceName"];
            });

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

            SeedDatabaes(); //Method tạo CSDL nếu chưa có

            app.MapControllers();

            app.Run();



            #region Func tạo CConstantsL 
            void SeedDatabaes()
            {
                using (var seedScope = app.Services.CreateScope())
                {
                    var dbInitializer = seedScope.ServiceProvider.GetRequiredService<IDbInitializer>();
                    try
                    {
                        dbInitializer.Initializer();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error: " + ex.Message);
                    }
                }
            }
            #endregion
        }
    }
}
