# Tổng quan dự án Marketing Search

Đây là một dự án web được thiết kế để tìm kiếm và phân tích thông tin marketing. Dự án bao gồm một frontend được xây dựng bằng Angular và hai phiên bản backend được xây dựng bằng .NET 8.

## Cấu trúc thư mục

Dự án được chia thành các thư mục con chính sau:

- `frontend-angular`: Chứa mã nguồn cho ứng dụng frontend.
- `backend-webAPI`: Chứa mã nguồn cho backend API có kết nối cơ sở dữ liệu.
- `backend-webAPI-noDb`: Chứa mã nguồn cho backend API không có kết nối cơ sở dữ liệu.
- `RealisticScaping`: Một dự án web chưa hoàn chỉnh hoặc đã bị loại bỏ. Không có đủ thông tin để xác định công nghệ hoặc mục đích cụ thể.
- `TestExtractContentOffline`: Một thư mục chứa các tài nguyên để xử lý media ngoại tuyến, bao gồm các tệp nhị phân của FFmpeg và các mô hình của Whisper.

## Thành phần và Luồng hoạt động

### Các thành phần chính

Dự án có thể được chia thành 3 thành phần chính:

1.  **Frontend (`frontend-angular`):**

    - **Vai trò:** Là giao diện người dùng (UI), nơi người dùng tương tác trực tiếp với ứng dụng.
    - **Trách nhiệm:**
      - Hiển thị các trang web, ô nhập liệu (để nhập từ khóa tìm kiếm, URL video, v.v.).
      - Gửi các yêu cầu (HTTP requests) đến Backend API khi người dùng thực hiện một hành động (ví dụ: nhấn nút tìm kiếm).
      - Nhận dữ liệu (ở định dạng JSON) từ Backend và hiển thị một cách trực quan cho người dùng (dưới dạng bảng, biểu đồ, hoặc văn bản).
      - Sử dụng Angular Material và Tailwind CSS để xây dựng một giao diện hiện đại và thân thiện.

2.  **Backend (`backend-webAPI`):**

    - **Vai trò:** Là "bộ não" của ứng dụng, xử lý tất cả các logic nghiệp vụ phức tạp.
    - **Trách nhiệm:**
      - **Cung cấp API:** Xây dựng các endpoints (ví dụ: `/api/search`, `/api/analysis`, `/api/video`) để Frontend có thể giao tiếp.
      - **Xử lý dữ liệu:** Nhận yêu cầu từ Frontend, thực hiện các tác vụ như tìm kiếm, phân tích.
      - **Tương tác với Database (MongoDB Atlas):** Sử dụng MongoDB Driver để lưu trữ và truy xuất dữ liệu như kết quả phân tích, thông tin từ khóa, v.v.
      - **Web Scraping (Cào dữ liệu):** Sử dụng **Selenium** để tự động hóa một trình duyệt web, truy cập vào các trang web khác để thu thập dữ liệu theo yêu cầu.
      - **Xử lý Video/Audio:**
        - Sử dụng thư viện như `VideoLibrary` để tải video từ các nguồn như YouTube.
        - Sử dụng **FFmpeg** để xử lý các tệp media, ví dụ như tách âm thanh từ video.
        - Sử dụng **Whisper.net** để chuyển đổi giọng nói trong tệp âm thanh thành văn bản (speech-to-text).
      - **Caching:** Sử dụng **Redis** để lưu trữ các kết quả truy vấn thường xuyên, giúp tăng tốc độ phản hồi cho người dùng.
      - **Tích hợp dịch vụ ngoài:** Giao tiếp với các dịch vụ của Google Cloud, gửi email qua SendGrid, hoặc gửi tin nhắn qua Twilio.

3.  **Tài nguyên hỗ trợ (`TestExtractContentOffline`):**
    - **Vai trò:** Không phải là một ứng dụng chạy được, mà là một kho chứa các công cụ và mô hình cần thiết cho Backend.
    - **Thành phần:**
      - `ffmpeg-binaries`: Các tệp thực thi của FFmpeg. Backend sẽ gọi đến các tệp này để xử lý media.
      - `whisper-models`: Các mô hình ngôn ngữ đã được huấn luyện của Whisper. Backend sẽ tải các mô hình này để thực hiện việc nhận dạng giọng nói.

### Luồng hoạt động (Workflows)

Ứng dụng này hoạt động theo mô hình client-server, nơi Frontend (Angular) đóng vai trò là giao diện người dùng và Backend (ASP.NET Core Web API) xử lý logic nghiệp vụ và tương tác với dữ liệu. Dưới đây là một vài ví dụ về luồng hoạt động chính giữa hai thành phần này, cùng với các luồng tương tác nội bộ trong frontend Angular:

#### Luồng Frontend Angular - Tương tác nội bộ

1. **Khởi tạo ứng dụng:** Khi ứng dụng Angular khởi động, các thành phần chính như `AppComponent` được tải. Nó khởi tạo các dịch vụ toàn cục như `AuthService` để kiểm tra trạng thái đăng nhập của người dùng và `ConfigService` để tải cấu hình ứng dụng.
2. **Điều hướng (Navigation):** Angular Router được sử dụng để điều hướng giữa các trang (ví dụ: từ trang chính đến trang tìm kiếm hoặc trang phân tích video). Các sự kiện điều hướng kích hoạt các `Route Guards` để kiểm tra quyền truy cập hoặc tải dữ liệu trước khi hiển thị trang.
3. **Quản lý trạng thái (State Management):** Ứng dụng có thể sử dụng NgRx, Redux, hoặc một giải pháp quản lý trạng thái khác để lưu trữ thông tin như kết quả tìm kiếm, trạng thái người dùng, hoặc tiến trình phân tích video. Các `Actions` được gửi đi để cập nhật trạng thái, và các `Selectors` được sử dụng để truy xuất dữ liệu từ trạng thái toàn cục.
4. **Tương tác người dùng:** Các thành phần giao diện như `SearchComponent` hoặc `VideoAnalysisComponent` lắng nghe các sự kiện người dùng (như nhấn nút hoặc nhập liệu). Chúng gọi đến các phương thức trong dịch vụ tương ứng (`SearchService`, `VideoService`) để xử lý logic nghiệp vụ.
5. **Giao tiếp với Backend:** Các dịch vụ như `ApiService` hoặc `HttpService` đóng gói các yêu cầu HTTP đến Backend. Chúng sử dụng Angular `HttpClient` để gửi yêu cầu và xử lý phản hồi, thường thông qua các `Observables` để quản lý dữ liệu bất đồng bộ.
6. **Cập nhật giao diện:** Khi nhận được dữ liệu từ Backend, các thành phần giao diện được cập nhật thông qua cơ chế `Change Detection` của Angular. Dữ liệu được hiển thị cho người dùng qua các template HTML với sự hỗ trợ của các directive và binding.
7. **Xử lý lỗi:** Các `Error Interceptors` hoặc cơ chế xử lý lỗi trong dịch vụ sẽ bắt các lỗi từ Backend (như lỗi 404, 500) và hiển thị thông báo lỗi cho người dùng thông qua các thành phần như `ToastComponent` hoặc `AlertComponent`.

Các luồng nội bộ này trong Angular được thiết kế để hoạt động liền mạch với nhau, đảm bảo rằng ứng dụng phản hồi nhanh chóng với các hành động của người dùng và duy trì trạng thái nhất quán trong suốt quá trình sử dụng.

#### Luồng 1: Tìm kiếm từ khóa đơn giản

1.  **Frontend (Angular):** Người dùng nhập một từ khóa vào ô tìm kiếm và nhấn "Enter".
2.  **Frontend (Angular):** Gửi một HTTP GET request đến endpoint `/api/search?keyword=...` trên **Backend API**.
3.  **Backend (ASP.NET Core Web API):** Nhận yêu cầu từ Frontend.
    a. Kiểm tra trong **Redis cache** xem từ khóa này đã được tìm kiếm gần đây chưa.
    b. Nếu có trong cache, lấy kết quả và trả về ngay.
    c. Nếu không, truy vấn vào cơ sở dữ liệu **MongoDB** để tìm kiếm thông tin.
    d. Lưu kết quả mới vào **Redis cache** để sử dụng cho các lần sau.
4.  **Backend (ASP.NET Core Web API):** Trả về một danh sách kết quả (dưới dạng JSON) cho Frontend.
5.  **Frontend (Angular):** Nhận dữ liệu JSON và hiển thị kết quả một cách trực quan cho người dùng.

#### Luồng 2: Phân tích nội dung một video

1.  **Frontend (Angular):** Người dùng dán một URL video (ví dụ: từ YouTube) vào ô nhập liệu và nhấn "Phân tích".
2.  **Frontend (Angular):** Gửi một HTTP POST request đến endpoint `/api/video/analyze` trên **Backend API**, kèm theo URL của video trong body request.
3.  **Backend (ASP.NET Core Web API):** Bắt đầu một tác vụ xử lý video:
    a. Thông báo cho Frontend biết rằng quá trình xử lý đã bắt đầu (có thể trả về một `taskId` để Frontend theo dõi trạng thái).
    b. Sử dụng thư viện `VideoLibrary` để tải tệp video từ URL.
    c. Sử dụng **FFmpeg** để tách tệp âm thanh (.mp3 hoặc .wav) từ video vừa tải về.
    d. Sử dụng **Whisper.net** (với các mô hình trong `whisper-models`) để chuyển đổi toàn bộ giọng nói trong tệp âm thanh thành văn bản.
    e. Thực hiện phân tích trên văn bản vừa được tạo ra (ví dụ: đếm số lần xuất hiện từ khóa, phân tích cảm xúc, v.v.).
    f. Lưu kết quả phân tích vào cơ sở dữ liệu **MongoDB**.
4.  **Frontend (Angular):** Có thể định kỳ kiểm tra trạng thái của tác vụ bằng cách gọi đến `/api/task/status/{taskId}` (nếu có triển khai cơ chế theo dõi tác vụ).
5.  **Frontend (Angular):** Khi Backend hoàn tất, nhận được thông báo và tải kết quả phân tích cuối cùng để hiển thị cho người dùng.

## Hướng dẫn xây dựng lại dự án

Để xây dựng lại dự án, bạn cần thực hiện các bước sau:

1.  **Cài đặt môi trường:**
    - Cài đặt Node.js và npm.
    - Cài đặt .NET 8 SDK.
2.  **Frontend:**
    - Di chuyển vào thư mục `frontend-angular`.
    - Chạy `npm install` để cài đặt các thư viện cần thiết.
    - Chạy `npm start` để khởi động ứng dụng frontend.
3.  **Backend:**
    - Mở một terminal khác và di chuyển vào thư mục `backend-webAPI/Api` hoặc `backend-webAPI-noDb/Api`.
    - Chạy `dotnet restore` để cài đặt các gói NuGet.
    - Chạy `dotnet run` để khởi động backend API.

**Lưu ý:**

- Bạn cần cấu hình các chuỗi kết nối và khóa API trong các tệp `appsettings.json` của các dự án backend.
