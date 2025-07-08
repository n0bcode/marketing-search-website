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
                try
                {
                    bool success = false;
                    do
                    {
                        try
                        {
                            driver.Navigate().GoToUrl(url);
                            success = true;
                        }
                        catch (WebDriverException ex)
                        {
                            if (driver.SessionId == null)
                            {
                                Console.WriteLine("Trình duyệt đã bị tắt bất ngờ. Dừng thao tác đăng nhập.");
                                return;
                            }
                            Console.WriteLine($"Lỗi khi truy cập URL: {ex.Message}");
                            Console.WriteLine("Có thể trang web không phản hồi, timeout hoặc trình duyệt bị chặn. Thử lại? (Y/N)");
                            string retry = Console.ReadLine()?.Trim().ToUpper();
                            if (retry != "Y")
                            {
                                Console.WriteLine("Bỏ qua đăng nhập nền tảng này.");
                                return;
                            }
                        }
                    } while (!success);

                    Console.WriteLine("Hãy đăng nhập trong trình duyệt. Khi hoàn tất, nhấn Enter ở đây để lưu cookie...");
                    Console.ReadLine();

                    if (driver.SessionId == null)
                    {
                        Console.WriteLine("Trình duyệt đã bị tắt trước khi lưu cookie. Không thể lưu session.");
                        return;
                    }
                    var cookies = driver.Manage().Cookies.AllCookies
                        .Select(c => new SerializableCookie
                        {
                            Name = c.Name,
                            Value = c.Value,
                            Domain = c.Domain,
                            Path = c.Path,
                            Expiry = c.Expiry,
                            Secure = c.Secure,
                            IsHttpOnly = c.IsHttpOnly
                        }).ToList();
                    File.WriteAllText(cookieFile, JsonConvert.SerializeObject(cookies));
                    Console.WriteLine($"Đã lưu cookie vào {cookieFile}");

                    // Nếu là Facebook, thực hiện thêm các thao tác đặc biệt
                    if (url.Contains("facebook.com", StringComparison.OrdinalIgnoreCase))
                    {
                        // Lưu riêng các trường cookie quan trọng
                        var fbCookies = driver.Manage().Cookies.AllCookies
                            .Where(c => c.Domain.Contains("facebook.com"))
                            .ToDictionary(c => c.Name, c => c.Value);
                        File.WriteAllText("facebook_cookies_simple.txt",
                            string.Join("; ", fbCookies.Select(kv => $"{kv.Key}={kv.Value}")));
                        Console.WriteLine("Đã lưu facebook_cookies_simple.txt (dễ copy-paste vào extension hoặc tool khác)");

                        // Truy cập trang business để access_token xuất hiện
                        try
                        {
                            driver.Navigate().GoToUrl("https://business.facebook.com/business_locations");
                            System.Threading.Thread.Sleep(3000); // Đợi trang load
                        }
                        catch { }

                        // Thử lấy access_token từ localStorage/sessionStorage
                        try
                        {
                            var token = (string)driver.ExecuteScript(
                                "return (window.localStorage && window.localStorage.getItem('accessToken')) || " +
                                "(window.sessionStorage && window.sessionStorage.getItem('accessToken')) || '';");
                            if (!string.IsNullOrEmpty(token))
                            {
                                File.WriteAllText("facebook_access_token.txt", token);
                                Console.WriteLine("Đã lưu access_token vào facebook_access_token.txt");
                            }
                            else
                            {
                                // Thử lấy từ document.cookie (nếu có)
                                var docCookie = (string)driver.ExecuteScript("return document.cookie;");
                                if (!string.IsNullOrEmpty(docCookie))
                                {
                                    File.WriteAllText("facebook_document_cookie.txt", docCookie);
                                }
                            }
                        }
                        catch { }
                    }

                    // Nếu là Tiktok, thực hiện thêm các thao tác đặc biệt
                    if (url.Contains("tiktok.com", StringComparison.OrdinalIgnoreCase))
                    {
                        // Lưu riêng các trường cookie quan trọng của TikTok
                        var tiktokCookies = driver.Manage().Cookies.AllCookies
                            .Where(c => c.Domain.Contains("tiktok.com"))
                            .ToDictionary(c => c.Name, c => c.Value);

                        // Chỉ lấy các trường phổ biến, có thể mở rộng nếu cần
                        var importantKeys = new[] { "sessionid", "tt_csrf_token", "sid_tt", "passport_csrf_token", "msToken" };
                        var filtered = tiktokCookies
                            .Where(kv => importantKeys.Contains(kv.Key) || kv.Key.StartsWith("session", StringComparison.OrdinalIgnoreCase))
                            .ToDictionary(kv => kv.Key, kv => kv.Value);

                        File.WriteAllText("tiktok_cookies_simple.txt",
                            string.Join("; ", filtered.Select(kv => $"{kv.Key}={kv.Value}")));
                        Console.WriteLine("Đã lưu tiktok_cookies_simple.txt (dễ copy-paste vào extension hoặc tool khác)");
                        // Tự động lấy thông tin user TikTok sau khi đăng nhập
                        try
                        {
                            driver.Navigate().GoToUrl("https://www.tiktok.com/@me");
                            System.Threading.Thread.Sleep(3000); // Đợi trang cá nhân load

                            // Lấy username và user_id từ trang cá nhân (có thể thay đổi tùy giao diện TikTok)
                            var username = (string)driver.ExecuteScript(
                                "try { return document.querySelector('h2[data-e2e=\"user-title\"]')?.textContent || ''; } catch { return ''; }");
                            var userId = (string)driver.ExecuteScript(
                                "try { return window.__INIT_PROPS__ ? Object.values(window.__INIT_PROPS__)[0]?.user?.id || '' : ''; } catch { return ''; }");

                            // Lưu thông tin user vào file
                            var userInfo = new Dictionary<string, string>
                                {
                                    { "username", username },
                                    { "user_id", userId }
                                };
                            File.WriteAllText("tiktok_user_info.json", JsonConvert.SerializeObject(userInfo, Formatting.Indented));
                            Console.WriteLine("Đã lưu thông tin user TikTok vào tiktok_user_info.json");
                        }
                        catch
                        {
                            Console.WriteLine("Không lấy được thông tin user TikTok.");
                        }
                        // (Tuỳ chọn) Truy cập trang cá nhân để cập nhật cookie/token
                        try
                        {
                            driver.Navigate().GoToUrl("https://www.tiktok.com/foryou");
                            System.Threading.Thread.Sleep(3000); // Đợi trang load
                        }
                        catch { }

                        // (Tuỳ chọn) Lấy token từ localStorage/sessionStorage nếu TikTok có sử dụng
                        try
                        {
                            var token = (string)driver.ExecuteScript(
                                "return (window.localStorage && window.localStorage.getItem('access_token')) || " +
                                "(window.sessionStorage && window.sessionStorage.getItem('access_token')) || '';");
                            if (!string.IsNullOrEmpty(token))
                            {
                                File.WriteAllText("tiktok_access_token.txt", token);
                                Console.WriteLine("Đã lưu access_token vào tiktok_access_token.txt");
                            }
                        }
                        catch { }
                    }
                }
                catch (WebDriverException ex)
                {
                    Console.WriteLine("Trình duyệt đã bị tắt hoặc gặp lỗi: " + ex.Message);
                }
            }
        }
        public static List<Cookie> LoadCookies(string cookieFile)
        {
            if (!File.Exists(cookieFile)) return new List<Cookie>();
            var json = File.ReadAllText(cookieFile);
            var list = JsonConvert.DeserializeObject<List<SerializableCookie>>(json) ?? new List<SerializableCookie>();
            return list.Select(c => new Cookie(
                c.Name, c.Value, c.Domain, c.Path, c.Expiry
            )).ToList();
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
                System.Threading.Thread.Sleep(3000); // Đợi trang load

                // Kiểm tra nếu là TikTok thì xác thực session
                if (url.Contains("tiktok.com", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        var currentUrl = driver.Url;
                        if (currentUrl.Contains("login", StringComparison.OrdinalIgnoreCase))
                        {
                            Console.WriteLine("Cảnh báo: Cookie TikTok đã hết hạn hoặc không hợp lệ. Vui lòng đăng nhập lại!");
                        }
                        else
                        {
                            // Thử lấy user_id qua JS (nếu có)
                            var userId = (string)driver.ExecuteScript(
                                "return window.__INIT_PROPS__ ? Object.values(window.__INIT_PROPS__)[0]?.user?.id || '' : '';");
                            if (string.IsNullOrEmpty(userId))
                                Console.WriteLine("Không lấy được user_id TikTok. Có thể session đã hết hạn hoặc chưa đăng nhập thành công.");
                            else
                                Console.WriteLine($"Đã xác thực session TikTok, user_id: {userId}");
                        }
                    }
                    catch { Console.WriteLine("Không xác thực được session TikTok."); }
                }
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
    public class SerializableCookie
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public string Domain { get; set; }
        public string Path { get; set; }
        public DateTime? Expiry { get; set; }
        public bool Secure { get; set; }
        public bool IsHttpOnly { get; set; }
    }
}