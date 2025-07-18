using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Api.Models;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace Api.Automations
{
    public class SeleniumManager
    {
        private IWebDriver driver;

        public SeleniumManager()
        {
            var options = new ChromeOptions();
            options.AddArgument("--headless"); // Chạy không có giao diện (nếu cần)
            driver = new ChromeDriver(options);
        }

        public void OpenUrl(string url)
        {
            driver.Navigate().GoToUrl(url);
        }

        public string GetPageTitle()
        {
            return driver.Title;
        }

        public void Quit()
        {
            driver.Quit();
        }

        // Thêm các phương thức khác

        public void ClickElement(By by)
        {
            var element = driver.FindElement(by);
            element.Click();
        }

        public void EnterText(By by, string text)
        {
            var element = driver.FindElement(by);
            element.SendKeys(text);
        }

        public string GetElementText(By by)
        {
            var element = driver.FindElement(by);
            return element.Text;
        }

        public void WaitForElementToBeVisible(By by, int timeoutInSeconds)
        {
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutInSeconds));
            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(by));
        }
        #region [Get Facebook Download Link]
        /// <summary>
        /// Lấy link tải về video Facebook từ getsave
        /// </summary>
        /// <param name="url">Link video Facebook</param>
        /// <returns>Link tải về video</returns>
        /// <exception cref="Exception">Nếu không tìm thấy phần tử hoặc có lỗi trong quá trình tải</exception>
        public async Task<ResponseAPI<string>> GetFacebookDownloadLink(string url)
        {
            ResponseAPI<string> response = new();
            // Cài đặt các tùy chọn cho trình duyệt Brave
            var options = new ChromeOptions();
            options.AddArgument("--headless"); // Chạy không có giao diện (nếu cần)
            // Khởi tạo ChromeDriver
            using (IWebDriver driver = new ChromeDriver(options))
            {
                // Mở trang Snaptik
                driver.Navigate().GoToUrl("https://getsave.net/vi");

                System.Threading.Thread.Sleep(5000); // Đợi 5 giây
                try
                {
                    var url_field = driver.FindElement(By.Id("postUrl"))!;
                    var submit_button = driver.FindElement(By.Id("loadVideos"))!; // Sử dụng class để tìm button
                    var modal_error = driver.FindElement(By.ClassName("modal-dialog")); // Tìm modal thông báo lỗi

                    try
                    {
                        // Nhấp vào nút đóng modal
                        var close_button = driver.FindElement(By.ClassName("modal-close")); // Tìm nút đóng modal

                        if (close_button != null && close_button.Displayed)
                        {
                            close_button.Click(); // Nhấn nút đóng
                        }
                    }
                    catch (NoSuchElementException)
                    {
                        // Không tìm thấy nút đóng modal, bỏ qua
                        Console.WriteLine("Không tìm thấy nút đóng modal, bỏ qua");
                    }

                    // Nhập thông tin link tiktok
                    url_field.SendKeys(url);
                    url_field.SendKeys(Keys.Enter); // Nhấn Enter để gửi

                    if (modal_error.Displayed)
                    {
                        throw new Exception("Lỗi: " + modal_error.Text);
                    }

                    System.Threading.Thread.Sleep(5000); // Đợi 5 giây

                    // Tìm nút Download
                    var download_button = driver.FindElements(By.XPath("//div[contains(@class, 'media-card')]")).Where(e => e.GetAttribute("data-link").Contains("audioProcess")).FirstOrDefault(); // Dùng class để xác định

                    if (download_button == null)
                    {
                        throw new Exception("Không tìm thấy nút tải về.");
                    }

                    // Giả định URL tải về được hiển thị trong một phần tử khác
                    string? urlDownload = download_button.GetAttribute("data-link"); // Lấy URL từ thuộc tính href

                    Console.WriteLine("User ID or Download URL: " + urlDownload);
                    // Truy cập vào URL tải xuống để lấy thông tin chi tiết về file tải xuống
                    using (var client = new HttpClient())
                    {
                        var responseJson = await client.GetAsync(urlDownload);
                        if (responseJson.IsSuccessStatusCode)
                        {
                            var responseBody = await responseJson.Content.ReadAsStringAsync();
                            var jsonData = JsonConvert.DeserializeObject<DownloadInfo>(responseBody);
                            Console.WriteLine("Thông tin chi tiết về file tải xuống:");
                            Console.WriteLine($"Percent: {jsonData.percent!}");
                            Console.WriteLine($"File Size: {jsonData.fileSize!}");
                            Console.WriteLine($"File Name: {jsonData.fileName!}");
                            Console.WriteLine($"Estimated File Size: {jsonData.estimatedFileSize!}");
                            Console.WriteLine($"File URL: {jsonData.fileUrl!}");

                            response.SetSuccessResponse("Lấy link tải về thành công!");
                            response.SetData(jsonData.fileUrl!);
                            Console.WriteLine("Success extract link download video facebook!");
                        }
                        else
                        {
                            Console.WriteLine("Lỗi khi truy cập vào URL tải xuống");
                        }
                    }
                }
                catch (NoSuchElementException e)
                {
                    Console.WriteLine("Không tìm thấy phần tử: " + e.Message);
                }
                catch (ElementClickInterceptedException e)
                {
                    Console.WriteLine("Một phần tử chặn nhấp chuột: " + e.Message);
                }
                finally
                {
                    // Đảm bảo đóng trình duyệt
                    driver.Quit();
                }
            }
            return response;
        }
        #region [DownloadInfo]
        public class DownloadInfo
        {
            public string? percent { get; set; }
            public string? fileSize { get; set; }
            public string? fileName { get; set; }
            public string? estimatedFileSize { get; set; }
            public string? fileUrl { get; set; }
        }
        #endregion
        #endregion
    }
}