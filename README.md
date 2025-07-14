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

## Configuration

Sensitive API keys and connection strings are not committed directly to the repository for security reasons. They are managed through `appsettings.json` and `appsettings.Development.json`.

*   `appsettings.json`: Contains default or placeholder values for configuration. This file is committed to the repository.
*   `appsettings.Development.json`: Contains sensitive keys and connection strings used during development. This file is **not** committed to the repository and is ignored by Git.

To set up your local development environment:

1.  Navigate to the backend API directory: `backend-webAPI/Api`.
2.  Create a file named `appsettings.Development.json` in this directory if it doesn't already exist.
3.  Populate `appsettings.Development.json` with your actual sensitive keys and connection strings. You can use the structure from the `appsettings.Development.json` that was previously in the repository as a template, replacing the `DELETED` or `YOUR_...` placeholders with your actual values.

    Example structure for `appsettings.Development.json`:
    ```json
    {
      "ApiSettings": {
        "GoogleApi": {
          "ApiKey": "YOUR_GOOGLE_API_KEY"
        },
        "FacebookApi": {
          "AccessToken": "YOUR_FACEBOOK_ACCESS_TOKEN"
        },
        "TwitterApi": {
          "BearerToken": "YOUR_TWITTER_BEARER_TOKEN"
        },
        "InstagramApi": {
          "AccessToken": "YOUR_INSTAGRAM_ACCESS_TOKEN"
        },
        "ThreadsApi": {
          "ID": "YOUR_THREADS_ID",
          "AccessToken": "YOUR_THREADS_ACCESS_TOKEN"
        },
        "OpenApi": {
          "SecretToken": "YOUR_OPENAI_SECRET_TOKEN"
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