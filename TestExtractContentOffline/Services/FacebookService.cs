using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Newtonsoft.Json.Linq;

namespace VideoSubtitleExtractor.Services
{
    public static class FacebookService
    {
        public static (string?, string?) GetFacebookDownloadInfo(string url)
        {
            string? realUrl = null;
            string? fileName = null;
            var options = new ChromeOptions();
            using (IWebDriver driver = new ChromeDriver(options))
            {
                driver.Navigate().GoToUrl("https://getsave.net/vi");
                System.Threading.Thread.Sleep(5000);
                try
                {
                    var url_field = driver.FindElement(By.Id("postUrl"));
                    url_field.SendKeys(url);
                    url_field.SendKeys(Keys.Enter);
                    System.Threading.Thread.Sleep(5000);

                    var download_button = driver.FindElements(By.XPath("//div[contains(@class, 'media-card')]"))
                        .FirstOrDefault(e => e.GetAttribute("data-link").Contains("audioProcess"));
                    if (download_button != null)
                    {
                        string apiUrl = download_button.GetAttribute("data-link");
                        using var client = new HttpClient();
                        var response = client.GetStringAsync(apiUrl).Result;
                        var json = JObject.Parse(response);

                        realUrl = json["fileUrl"]?.ToString();
                        fileName = json["fileName"]?.ToString();

                        Console.WriteLine("Resolved Facebook file URL: " + realUrl);
                        Console.WriteLine("Resolved file name: " + fileName);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error while resolving Facebook video: " + ex.Message);
                }
            }
            return (realUrl, fileName);
        }
    }
}