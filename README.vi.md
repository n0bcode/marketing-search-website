[English](./README.md)

# Ứng dụng Web Tìm kiếm và Phân tích Marketing

Đây là một dự án ứng dụng web phục vụ cho việc phân tích và tìm kiếm marketing, bao gồm một frontend được xây dựng bằng Angular và một backend Web API sử dụng ASP.NET Core.

## Yêu cầu cài đặt

Trước khi bắt đầu, hãy đảm bảo bạn đã cài đặt các phần mềm sau:

*   [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
*   [Node.js và npm](https://nodejs.org/en/)
*   [Angular CLI](https://angular.io/cli)

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

## Cài đặt bổ sung

Một số tính năng của dự án yêu cầu tải thêm các file phụ trợ.

### Whisper Model

Tính năng chuyển đổi video thành văn bản sử dụng Whisper model. Bạn cần tải file `ggml-base.bin` và đặt nó vào thư mục `backend-webAPI/Api/whisper-models`.

*   **Link tải:** [ggml-base.bin](https://huggingface.co/ggerganov/whisper.cpp/resolve/main/ggml-base.bin)
*   **Thư mục đích:** `backend-webAPI/Api/whisper-models`

Hãy tạo thư mục `whisper-models` nếu nó chưa tồn tại.

## Tài liệu API

API được tài liệu hóa bằng Swagger. Sau khi backend đã chạy, bạn có thể truy cập giao diện Swagger tại:

[https://localhost:7228/swagger](https://localhost:7228/swagger)
