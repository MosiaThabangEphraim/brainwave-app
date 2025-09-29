# BrainWave App

A comprehensive task management and collaboration application built with .NET MAUI, featuring user authentication, task management, reminders, collaboration tools, and admin dashboard capabilities.

## Features

### Core Functionality
- **User Authentication & Registration** - Secure user accounts with password reset functionality
- **Task Management** - Create, edit, and organize tasks with priorities and due dates
- **Reminders System** - Set up and manage reminders for important tasks
- **Collaboration Tools** - Work together on projects with team members
- **Admin Dashboard** - Comprehensive admin panel for user and task management
- **Profile Management** - User profiles with badges and achievements
- **Real-time Messaging** - Communication system for team collaboration

### Technical Features
- **Cross-Platform** - Runs on Android, iOS, macOS, and Windows
- **Supabase Integration** - Backend-as-a-Service for authentication and data storage
- **Email Services** - Email notifications and password reset functionality
- **Secure Storage** - Encrypted local storage for sensitive data
- **MVVM Architecture** - Clean separation of concerns with ViewModels

## Supported Platforms

- **Android** (API 21+)
- **iOS** (11.0+)
- **macOS** (13.1+)
- **Windows** (10.0.17763.0+)

## Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Visual Studio 2022](https://visualstudio.microsoft.com/vs/) with MAUI workload
- [Android Studio](https://developer.android.com/studio) (for Android development)
- [Xcode](https://developer.apple.com/xcode/) (for iOS development on macOS)

## Installation & Setup

### 1. Clone the Repository
`ash
git clone https://github.com/MosiaThabangEphraim/brainwave-app.git
cd brainwave-app
`

### 2. Configure Supabase
1. Create a [Supabase](https://supabase.com) account and project
2. Update the Constants.cs file with your Supabase credentials:
`csharp
public static class Constants
{
    public const string SUPABASE_URL = "your-supabase-url";
    public const string SUPABASE_ANON_KEY = "your-supabase-anon-key";
}
`

### 3. Database Setup
The app uses the following main tables:
- users - User accounts and profiles
- 	ask - Task management
- eminder - Reminder system
- collaboration - Team collaboration
- message - Messaging system
- adge - User achievements

### 4. Build and Run
`ash
# Restore packages
dotnet restore

# Build the project
dotnet build

# Run on specific platform
dotnet run --framework net8.0-android
dotnet run --framework net8.0-ios
dotnet run --framework net8.0-windows10.0.19041.0
`

## Project Structure

`
BrainWave.App/
â”œâ”€â”€ Models/                 # Data models and DTOs
â”œâ”€â”€ ViewModels/            # MVVM ViewModels
â”œâ”€â”€ Views/                 # XAML pages and UI
â”œâ”€â”€ Services/              # Business logic and API services
â”œâ”€â”€ Database/              # Database models
â”œâ”€â”€ Converters/            # XAML value converters
â”œâ”€â”€ Behaviors/             # Custom behaviors
â”œâ”€â”€ Platforms/             # Platform-specific implementations
â”œâ”€â”€ Resources/             # Images, fonts, and assets
â””â”€â”€ Constants.cs           # Application constants
`

## Key Services

- **SupabaseService** - Handles all Supabase operations and authentication
- **AuthenticationService** - User authentication and session management
- **NavigationService** - App navigation logic
- **EmailService** - Email notifications and password reset
- **NotificationService** - Push notifications
- **DatabaseService** - Local database operations

## Dependencies

- **Microsoft.Maui.Controls** - MAUI framework
- **Supabase** - Backend services
- **BCrypt.Net-Next** - Password hashing
- **Newtonsoft.Json** - JSON serialization
- **SendGrid** - Email services
- **MailKit** - Email functionality

## Getting Started

1. **Register/Login** - Create an account or sign in
2. **Dashboard** - View your tasks and reminders overview
3. **Tasks** - Create and manage your tasks
4. **Reminders** - Set up important reminders
5. **Collaboration** - Invite team members and collaborate
6. **Profile** - Manage your profile and view achievements

## Admin Features

The app includes a comprehensive admin dashboard with:
- User management and details
- Task oversight and management
- System analytics and reports
- User credential management

## Security

- Password hashing with BCrypt
- Secure token-based authentication
- Encrypted local storage
- HTTPS communication with Supabase

## Contributing

1. Fork the repository
2. Create a feature branch (git checkout -b feature/AmazingFeature)
3. Commit your changes (git commit -m 'Add some AmazingFeature')
4. Push to the branch (git push origin feature/AmazingFeature)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Author

**Mosia Thabang**
- GitHub: [@MosiaThabangEphraim](https://github.com/MosiaThabangEphraim)

## Acknowledgments

- Built with [.NET MAUI](https://docs.microsoft.com/en-us/dotnet/maui/)
- Backend powered by [Supabase](https://supabase.com/)
- Icons and assets from [Material Design](https://material.io/)

## Support

If you have any questions or need help, please:
- Open an issue on GitHub
- Contact the author directly

---

**Note**: This is a cross-platform task management application designed to help individuals and teams stay organized and productive. The app leverages modern technologies to provide a seamless experience across all major platforms.
