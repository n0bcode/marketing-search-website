using System;
using System.Collections.Generic;
using System.IO;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Newtonsoft.Json;

namespace VideoSubtitleExtractor.Services
{
    public static class BrowserLoginHelper
    {
        // Cho phép lưu nhiều profile đăng nhập cho từng nền tảng
        public static void LaunchAndLogin(string url, string cookieFile, string? userDataDir = null)
        {
            var options = new ChromeOptions();
            if (!string.IsNullOrEmpty(userDataDir))
            {
                options.AddArgument($"--user-data-dir={userDataDir}");
            }
            using (var driver = new ChromeDriver(options))
            {
                driver.Navigate().GoToUrl(url);
                Console.WriteLine("Hãy đăng nhập trong trình duyệt. Khi hoàn tất, nhấn Enter ở đây để lưu cookie...");
                Console.ReadLine();

                var cookies = driver.Manage().Cookies.AllCookies;
                File.WriteAllText(cookieFile, JsonConvert.SerializeObject(cookies));
                Console.WriteLine($"Đã lưu cookie vào {cookieFile}");
            }
        }

        public static List<Cookie> LoadCookies(string cookieFile)
        {
            if (!File.Exists(cookieFile)) return new List<Cookie>();
            var json = File.ReadAllText(cookieFile);
            return JsonConvert.DeserializeObject<List<Cookie>>(json) ?? new List<Cookie>();
        }

        // Mở lại trang web với session đã đăng nhập
        public static void OpenSiteWithSession(string url, string cookieFile, string? userDataDir = null)
        {
            var options = new ChromeOptions();
            if (!string.IsNullOrEmpty(userDataDir))
            {
                options.AddArgument($"--user-data-dir={userDataDir}");
            }
            using (var driver = new ChromeDriver(options))
            {
                driver.Navigate().GoToUrl("about:blank");
                var cookies = LoadCookies(cookieFile);
                foreach (var cookie in cookies)
                {
                    try { driver.Manage().Cookies.AddCookie(cookie); } catch { }
                }
                driver.Navigate().GoToUrl(url);
                Console.WriteLine("Đã mở trang web với session đã đăng nhập. Nhấn Enter để đóng trình duyệt...");
                Console.ReadLine();
            }
        }

        // Cho phép chọn profile đăng nhập cũ hoặc đăng nhập mới
        public static string GetProfileDir(string platform)
        {
            string baseDir = Path.Combine(Environment.CurrentDirectory, "browser_profiles");
            if (!Directory.Exists(baseDir)) Directory.CreateDirectory(baseDir);
            string profileDir = Path.Combine(baseDir, platform);
            if (!Directory.Exists(profileDir)) Directory.CreateDirectory(profileDir);
            return profileDir;
        }
    }
}