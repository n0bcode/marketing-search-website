using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api.Constants;
using Api.Models;
using Newtonsoft.Json;

namespace Api.Utils
{
    public static class DataSaver
    {
        /// <summary>
        ///    Saves the provided data to a JSON file in a directory named after the guild Id of the keyword.
        ///    The directory is created if it does not exist.
        ///   The file name is generated based on the current timestamp and the platform name.
        /// </summary>
        /// <param name="data">The data to be saved.</param>
        /// <param name="pathSave">The path where the data will be saved.</param>
        public static void BasicSaveData(object data, string pathSave = "data")
        {
            // Tạo thư mục dựa trên tên guild
            string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "Logs", pathSave);
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            // Tạo tên file dựa trên thời gian ghi và tên nền tảng
            string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            string fileName = $"{timestamp}.json";

            // Lưu dữ liệu vào file
            string filePath = Path.Combine(folderPath, fileName);
            File.WriteAllText(filePath, JsonConvert.SerializeObject(data, Newtonsoft.Json.Formatting.Indented));
        }
        /// <summary>
        ///     Saves the provided data to a JSON file in a directory named after the guild Id of the keyword.
        ///     The directory is created if it does not exist.
        ///     The file name is generated based on the current timestamp and the platform name.
        /// </summary>
        public static void SaveData(object data, string platformName, string guildName)
        {
            // Tạo thư mục dựa trên tên guild
            string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "Logs");
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            // Tạo tên file dựa trên thời gian ghi và tên nền tảng
            string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            string fileName = $"{guildName}_{timestamp + '_'}{platformName}.json";

            // Lưu dữ liệu vào file
            string filePath = Path.Combine(folderPath, fileName);
            // Kiểm tra xem tệp đã tồn tại chưa
            List<object> existingData = new List<object>();
            if (File.Exists(filePath))
            {
                // Đọc dữ liệu hiện có từ tệp
                string existingJson = File.ReadAllText(filePath);
                existingData = JsonConvert.DeserializeObject<List<object>>(existingJson) ?? new List<object>();
            }
            // Thêm dữ liệu mới vào mảng
            existingData.Add(data);


            // Chuyển đổi mảng thành JSON
            string jsonData = JsonConvert.SerializeObject(existingData, Newtonsoft.Json.Formatting.Indented);

            // Lưu dữ liệu vào tệp
            File.WriteAllText(filePath, jsonData);
        }
        public static dynamic GetFromLog(string guildName)
        {
            // Tạo đường dẫn tới thư mục Logs
            string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "Logs");

            // Tạo tên file
            string fileName = $"{guildName}*Gemini*.json"; // Nên tìm kiếm tất cả file với tên guild suốt

            // Tìm tất cả file phù hợp
            string[] filePaths = Directory.GetFiles(folderPath, fileName);

            // Nếu không tìm thấy file nào, trả về null
            if (filePaths.Length == 0)
            {
                return new ResponseAPI<object>() { Success = false, Message = "Không tìm thấy file nào" };
            }

            // Chọn file đầu tiên (hoặc bạn có thể thay đổi logic để chọn file cụ thể)
            string filePath = filePaths[0];

            // Đọc nội dung file
            string jsonData = File.ReadAllText(filePath);

            return jsonData;
        }
        public static dynamic GetAndFormatDataFromLogs(Type type, string guildName)
        {
            // Tạo đường dẫn tới thư mục Logs
            string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "Logs");

            // Tạo tên file
            string fileName = $"{guildName}_Gemini*.json"; // Nên tìm kiếm tất cả file với tên guild suốt

            // Tìm tất cả file phù hợp
            string[] filePaths = Directory.GetFiles(folderPath, fileName);

            // Nếu không tìm thấy file nào, trả về null
            if (filePaths.Length == 0)
            {
                return null;
            }

            // Chọn file đầu tiên (hoặc bạn có thể thay đổi logic để chọn file cụ thể)
            string filePath = filePaths[0];

            // Đọc nội dung file
            string jsonData = File.ReadAllText(filePath);

            // Ép kiểu dữ liệu
            dynamic data = JsonConvert.DeserializeObject(jsonData, type);

            return data;
        }

    }
}