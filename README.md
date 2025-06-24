# ChatApp

A modern real-time chat application built with ASP.NET Core, Blazor, and SignalR.

## Features
- Real-time messaging between users
- User authentication and registration
- Chat list with last message preview
- Media sharing (images, videos, PDFs, text files)
- Responsive design for desktop and mobile
- User presence and typing indicators
- Sidebar with shared media view

## Technologies Used
- ASP.NET Core
- Blazor Server
- SignalR
- Entity Framework Core
- Identity for authentication

## Project Structure
```
ChatApp/
  ChatApp.sln
  ChatApp.Web/           # Main web project
    Components/          # Blazor components
    Controllers/         # API controllers
    Data/                # EF Core models and context
    Hubs/                # SignalR hubs
    Migrations/          # EF Core migrations
    Services/            # Business logic
    ViewModels/          # View models
    wwwroot/             # Static files (CSS, JS, images, uploads)
```

## Getting Started

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- SQL Server (or change connection string for your DB)

### Default Login
- Register a new user on first run.

## File Uploads
Uploaded files are stored in `wwwroot/uploads/`.

## Customization
- Update styles in `wwwroot/chat-app.css` and related CSS/SCSS files.
- Change authentication or database settings in `appsettings.json`.

## License
MIT License. See [LICENSE](LICENSE) for details. 