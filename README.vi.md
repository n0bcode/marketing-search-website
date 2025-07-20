[English](./README.md)

# Ứng dụng Web Tìm kiếm và Phân tích Marketing

Đây là một dự án ứng dụng web phục vụ cho việc phân tích và tìm kiếm marketing, bao gồm một frontend được xây dựng bằng Angular và một backend Web API sử dụng ASP.NET Core.

## Yêu cầu cài đặt

Trước khi bắt đầu, hãy đảm bảo bạn đã cài đặt các phần mềm sau:

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js và npm](https://nodejs.org/en/)
- [Angular CLI](https://angular.io/cli)

## Hướng dẫn cài đặt và khởi chạy

Làm theo các hướng dẫn sau để cài đặt và chạy dự án trên máy của bạn.

### Backend (ASP.NET Core Web API)

1.  **Di chuyển đến thư mục backend:**

    ```bash
    cd backend-webAPI/Api
    ```

2.  **Chạy ứng dụng:**

    ```bash
    dotnet run
    ```

    API sẽ chạy trên các cổng `https://localhost:7228` và `http://localhost:5046`. Bạn có thể xem danh sách các API endpoint và thử nghiệm chúng thông qua Swagger tại `https://localhost:7228/swagger`.

### Frontend (Angular)

1.  **Di chuyển đến thư mục frontend:**

    ```bash
    cd frontend-angular
    ```

2.  **Cài đặt các gói phụ thuộc:**

    ```bash
    npm install
    ```

3.  **Chạy ứng dụng:**

    ```bash
    npm start
    ```

    Máy chủ phát triển frontend sẽ được khởi động. Mở trình duyệt và truy cập `http://localhost:4200/`. Ứng dụng sẽ tự động tải lại nếu bạn thay đổi bất kỳ file mã nguồn nào.

## Cấu hình

Các khóa API và chuỗi kết nối nhạy cảm không được đưa trực tiếp vào kho lưu trữ vì lý do bảo mật. Chúng được quản lý thông qua `appsettings.json` và `appsettings.Development.json`.

- `appsettings.json`: Chứa các giá trị mặc định hoặc giữ chỗ cho cấu hình. File này được đưa vào kho lưu trữ.
- `appsettings.Development.json`: Chứa các khóa nhạy cảm và chuỗi kết nối được sử dụng trong quá trình phát triển. File này **không** được đưa vào kho lưu trữ và bị Git bỏ qua.

Để thiết lập môi trường phát triển cục bộ của bạn:

1.  Di chuyển đến thư mục API backend: `backend-webAPI/Api`.
2.  Tạo một file có tên `appsettings.Development.json` trong thư mục này nếu nó chưa tồn tại.
3.  Điền `appsettings.Development.json` với các khóa và chuỗi kết nối thực tế của bạn. Bạn có thể sử dụng cấu trúc từ `appsettings.Development.json` đã có trong kho lưu trữ làm mẫu, thay thế các giá trị `DELETED` hoặc `YOUR_...` bằng các giá trị thực tế của bạn.

    Cấu trúc ví dụ cho `appsettings.Development.json`:

    ```json
    {
      "ApiSettings": {
        "GoogleApi": {
          "ApiKey": "YOUR_GOOGLE_API_KEY"
        },
        "FacebookApi": {
          "AccessToken": "YOUR_FACEBOOK_ACCESS_TOKEN"
        },
        "GeminiApi": {
          "SecretToken": "YOUR_GEMINI_SECRET_TOKEN"
        }
      },
      "EmailSettings": {
        "GoogleSender": {
          "Username": "YOUR_GOOGLE_SENDER_USERNAME",
          "Email": "YOUR_GOOGLE_SENDER_EMAIL",
          "Password": "YOUR_GOOGLE_SENDER_PASSWORD"
        },
        "TwilioSMSSender": {
          "SID": "YOUR_TWILIO_SID",
          "AuthToken": "YOUR_TWILIO_AUTH_TOKEN"
        },
        "TwilioEmailSender": {
          "ApiKey": "YOUR_TWILIO_EMAIL_API_KEY"
        }
      },
      "ConnectionStrings": {
        "MongoDb": "YOUR_MONGODB_CONNECTION_STRING"
      }
    }
    ```

### Cách lấy Token API

Để đảm bảo ứng dụng hoạt động chính xác, bạn sẽ cần lấy các khóa/token API từ các nhà cung cấp dịch vụ tương ứng và thêm chúng vào file `appsettings.Development.json` của bạn.

- **Google API Key (Serper API):**
  Khóa này được sử dụng cho chức năng tìm kiếm Google thông qua Serper API. Serper API cung cấp kết quả tìm kiếm từ Google mà không cần tích hợp trực tiếp với Google Custom Search API.

  1.  Truy cập trang web [Serper API](https://serper.dev/).
  2.  Đăng ký hoặc đăng nhập vào tài khoản của bạn.
  3.  Lấy khóa API của bạn từ bảng điều khiển Serper.
  4.  Sao chép khóa API đã tạo và dán nó vào vị trí `YOUR_GOOGLE_API_KEY` trong `appsettings.Development.json`.

- **Facebook Access Token:**

  1.  Truy cập trang web [Facebook for Developers](https://developers.facebook.com/).
  2.  Tạo một ứng dụng mới hoặc chọn một ứng dụng hiện có.
  3.  Điều hướng đến "Tools" > "Graph API Explorer" hoặc tham khảo tài liệu API Facebook cụ thể cho loại access token cần thiết (ví dụ: User Access Token, Page Access Token, App Access Token).
  4.  Lấy access token phù hợp và dán nó vào vị trí `YOUR_FACEBOOK_ACCESS_TOKEN` trong `appsettings.Development.json`.

- **Gemini Secret Token:**
  1.  Truy cập [Google AI Studio](https://aistudio.google.com/app/apikey) hoặc [Google Cloud Console](https://console.cloud.google.com/apis/credentials).
  2.  Nếu sử dụng Google AI Studio, hãy tạo khóa API trực tiếp.
  3.  Nếu sử dụng Google Cloud Console, hãy tạo một dự án mới hoặc chọn một dự án hiện có, sau đó điều hướng đến "APIs & Services" > "Credentials" và tạo khóa API.
  4.  Sao chép khóa API đã tạo và dán nó vào vị trí `YOUR_GEMINI_SECRET_TOKEN` trong `appsettings.Development.json`.

## Cài đặt bổ sung

Một số tính năng của dự án yêu cầu tải thêm các file phụ trợ.

### Whisper Model

Tính năng chuyển đổi video thành văn bản sử dụng Whisper model. Bạn cần tải file `ggml-base.bin` và đặt nó vào thư mục `backend-webAPI/Api/whisper-models`.

- **Link tải:** [ggml-base.bin](https://huggingface.co/ggerganov/whisper.cpp/resolve/main/ggml-base.bin)
- **Thư mục đích:** `backend-webAPI/Api/whisper-models`

Hãy tạo thư mục `whisper-models` nếu nó chưa tồn tại.

## Tài liệu API

API được tài liệu hóa bằng Swagger. Sau khi backend đã chạy, bạn có thể truy cập giao diện Swagger tại:

[https://localhost:7228/swagger](https://localhost:7228/swagger)

## Đóng góp

Xin cảm ơn tất cả những người bạn đã đóng góp cho dự án này！

<p align="center">
    <a href="https://github.com/n0bcode/marketing-search-website/graphs/contributors">
      <img src="https://contrib.rocks/image?repo=n0bcode/marketing-search-website" style="max-width: 400px;" />
    </a>
</p>

**Thống kê đóng góp nhánh Dev：**

<p align="center">
    <img src="https://repobeats.axiom.co/api/embed/d23176e25029ed25e6813f558c5f220e86b591cc.svg" alt="Dev Branch code analysis" style="max-width: 80%; border-radius: 5px;">
</p>

## Cảm ơn sự ủng hộ của bạn
