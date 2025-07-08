[Tiếng Việt](./README.vi.md)

# Marketing Search Web Application

This project is a web application for marketing search analysis, consisting of an Angular frontend and an ASP.NET Core Web API backend.

## Prerequisites

Before you begin, ensure you have the following installed:

*   [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
*   [Node.js and npm](https://nodejs.org/en/)
*   [Angular CLI](https://angular.io/cli)

## Getting Started

Follow these instructions to get the project up and running on your local machine.

### Backend (ASP.NET Core Web API)

1.  **Navigate to the backend directory:**
    ```bash
    cd backend-webAPI/Api
    ```

2.  **Run the application:**
    ```bash
    dotnet run
    ```

    The API will be running on `https://localhost:7228` and `http://localhost:5046`. You can view the available API endpoints and test them using Swagger at `https://localhost:7228/swagger`.

### Frontend (Angular)

1.  **Navigate to the frontend directory:**
    ```bash
    cd frontend-angular
    ```

2.  **Install the dependencies:**
    ```bash
    npm install
    ```

3.  **Run the application:**
    ```bash
    npm start
    ```

    The frontend development server will start. Open your browser and navigate to `http://localhost:4200/`. The app will automatically reload if you change any of the source files.

## Additional Setup

Some features of this project require additional files to be downloaded.

### Whisper Model

The video transcription feature uses the Whisper model. You need to download the `ggml-base.bin` file and place it in the `backend-webAPI/Api/whisper-models` directory.

*   **Download link:** [ggml-base.bin](https://huggingface.co/ggerganov/whisper.cpp/resolve/main/ggml-base.bin)
*   **Destination directory:** `backend-webAPI/Api/whisper-models`

Create the `whisper-models` directory if it doesn't exist.

## API Documentation

The API is documented using Swagger. Once the backend is running, you can access the Swagger UI at:

[https://localhost:7228/swagger](https://localhost:7228/swagger)