# Marketing Search Website

## Overview

The Marketing Search Website is a web application designed to help users search for marketing information based on keywords across various social media platforms. The application utilizes a .NET Core Web API for the backend and React for the frontend, providing a seamless user experience.

## Project Structure

The project is divided into two main parts: the backend and the frontend.

### Backend

- **MarketingSearchAPI.sln**: Solution file for the .NET Core Web API project.
- **Controllers**: Contains the `SearchController.cs` which handles search requests.
- **Models**: Contains the `KeywordModel.cs` which defines the structure of keyword data.
- **Services**: Contains the `KeywordService.cs` which manages keyword searches and variations.
- **Data**: Contains the `AppDbContext.cs` for managing the SQLite database.
- **Program.cs**: Entry point of the application.
- **Startup.cs**: Configures services and middleware.
- **appsettings.json**: Configuration settings for the application.
- **appsettings.Development.json**: Development-specific configuration settings.
- **MarketingSearchAPI.Tests**: Contains unit tests for the API.

### Frontend

- **public/index.html**: Main HTML file for the React application.
- **public/favicon.ico**: Favicon for the website.
- **src/components**: Contains React components such as `SearchBar`, `SearchResults`, and `ApiStatusNotification`.
- **src/pages**: Contains page components like `HomePage` and `ApiDocsPage`.
- **src/services/apiService.js**: Functions for making HTTP requests to the backend API.
- **src/App.jsx**: Main application component.
- **src/index.js**: Entry point for the React application.
- **src/routes.js**: Defines the routes for the application.
- **package.json**: Configuration file for npm.
- **.env**: Environment variables for the React application.

## Features

- **Keyword Search**: Users can input keywords to search for relevant marketing information.
- **Keyword Variations**: The system processes variations of keywords using Regex for accurate search results.
- **API Status Notifications**: Users are informed about the status of the APIs being used.
- **Swagger Integration**: Provides a user-friendly interface for testing and documenting the API.

## Technologies Used

- **Backend**: .NET Core (5.0 or newer), Entity Framework Core, SQLite
- **Frontend**: React, Axios, React Router

## Getting Started

1. Clone the repository.
2. Navigate to the `backend` directory and run the API using .NET CLI.
3. Navigate to the `frontend` directory and install dependencies using npm.
4. Start the React application.

## Documentation

For detailed API documentation, visit the Swagger UI available at `/api/docs`.

## Contribution

Contributions are welcome! Please submit a pull request or open an issue for any enhancements or bug fixes.

## License

This project is licensed under the MIT License.
