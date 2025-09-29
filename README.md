# BrainWave App

BrainWave is a comprehensive cross-platform task management and productivity application built with .NET MAUI. It's designed to help individuals and teams stay organized, productive, and connected through advanced task management, collaboration tools, and intelligent reminders.

## What BrainWave Does

BrainWave is a complete productivity suite that transforms how you manage your daily tasks and collaborate with others. The app serves as your personal and professional command center, offering:

### Core Functionality

**Task Management System**
- Create, edit, and organize tasks with detailed descriptions
- Set priorities (High, Medium, Low) to focus on what matters most
- Assign due dates and track progress in real-time
- Categorize tasks by projects or themes
- Mark tasks as completed with visual progress indicators

**Smart Reminders**
- Set up intelligent reminders for important tasks and deadlines
- Receive notifications before due dates
- Create recurring reminders for routine activities
- Customize reminder timing and frequency
- Never miss important deadlines again

**Team Collaboration**
- Invite team members to collaborate on projects
- Share tasks and assign responsibilities
- Real-time messaging system for team communication
- Track team progress and productivity
- Manage team permissions and access levels

**User Management & Authentication**
- Secure user registration and login system
- Password reset functionality via email
- User profile management with personal information
- Achievement badges and progress tracking
- Role-based access control

**Admin Dashboard**
- Comprehensive admin panel for system management
- User management and oversight capabilities
- Task monitoring and analytics
- System performance metrics
- User credential management

### Technical Features

**Cross-Platform Compatibility**
- Runs seamlessly on Android, iOS, macOS, and Windows
- Native performance on all supported platforms
- Consistent user experience across devices
- Platform-specific optimizations

**Backend Integration**
- Supabase integration for secure data storage
- Real-time data synchronization
- Cloud-based user authentication
- Scalable database architecture

**Security & Privacy**
- BCrypt password hashing for maximum security
- Encrypted local storage for sensitive data
- HTTPS communication for all network requests
- Secure token-based authentication

**Email Services**
- SendGrid integration for email notifications
- Password reset emails
- Welcome emails for new users
- Custom notification emails

## Supported Platforms

- **Android** (API 21+) - Full native Android experience
- **iOS** (11.0+) - Optimized for iPhone and iPad
- **macOS** (13.1+) - Native Mac application
- **Windows** (10.0.17763.0+) - Windows desktop and tablet support

## Prerequisites

Before getting started with BrainWave, ensure you have:

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) - Required for building the application
- [Visual Studio 2022](https://visualstudio.microsoft.com/vs/) with MAUI workload - For development
- [Android Studio](https://developer.android.com/studio) - For Android development and testing
- [Xcode](https://developer.apple.com/xcode/) - For iOS development (macOS only)

## Installation & Setup

### 1. Clone the Repository
`ash
git clone https://github.com/MosiaThabangEphraim/brainwave-app.git
cd brainwave-app
`

### 2. Configure Supabase Backend
1. Create a [Supabase](https://supabase.com) account and new project
2. Set up your database tables (users, tasks, reminders, collaborations, messages, badges)
3. Update the Constants.cs file with your Supabase credentials:
`csharp
public static class Constants
{
    public const string SUPABASE_URL = "your-supabase-project-url";
    public const string SUPABASE_ANON_KEY = "your-supabase-anon-key";
}
`

### 3. Configure Email Services (Optional)
1. Create a [SendGrid](https://sendgrid.com) account
2. Generate an API key
3. Update Services/EmailService.cs:
`csharp
_apiKey = "your-sendgrid-api-key-here";
`

### 4. Build and Run
`ash
# Restore NuGet packages
dotnet restore

# Build the project
dotnet build

# Run on specific platform
dotnet run --framework net8.0-android    # Android
dotnet run --framework net8.0-ios         # iOS
dotnet run --framework net8.0-windows10.0.19041.0  # Windows
`

## Project Structure

`
BrainWave.App/
â”œâ”€â”€ Models/                 # Data models and DTOs for API communication
â”œâ”€â”€ ViewModels/            # MVVM ViewModels for UI logic
â”œâ”€â”€ Views/                 # XAML pages and user interface
â”œâ”€â”€ Services/              # Business logic and API services
â”œâ”€â”€ Database/              # Database models and entities
â”œâ”€â”€ Converters/            # XAML value converters for data binding
â”œâ”€â”€ Behaviors/             # Custom behaviors for UI elements
â”œâ”€â”€ Platforms/             # Platform-specific implementations
â”œâ”€â”€ Resources/             # Images, fonts, and static assets
â””â”€â”€ Constants.cs           # Application-wide constants and configuration
`

## Key Services Explained

**SupabaseService**
- Handles all database operations and real-time updates
- Manages user authentication and session management
- Provides secure API communication with the backend

**AuthenticationService**
- Manages user login, registration, and session handling
- Implements secure password hashing and validation
- Handles user profile management and updates

**NavigationService**
- Controls app navigation flow between different pages
- Manages page transitions and routing
- Handles deep linking and navigation state

**EmailService**
- Sends password reset emails and notifications
- Manages email templates and formatting
- Integrates with SendGrid for reliable email delivery

**NotificationService**
- Handles push notifications and local alerts
- Manages notification scheduling and delivery
- Provides cross-platform notification support

**DatabaseService**
- Manages local data caching and offline support
- Handles data synchronization with the cloud
- Provides data persistence and retrieval

## Dependencies

- **Microsoft.Maui.Controls** - Core MAUI framework for cross-platform development
- **Supabase** - Backend-as-a-Service for database and authentication
- **BCrypt.Net-Next** - Secure password hashing library
- **Newtonsoft.Json** - JSON serialization and deserialization
- **SendGrid** - Email delivery service
- **MailKit** - Email functionality and SMTP support

## Getting Started Guide

### For New Users
1. **Download and Install** - Get BrainWave from your platform's app store or build from source
2. **Create Account** - Register with your email and create a secure password
3. **Complete Profile** - Add your personal information and preferences
4. **Explore Dashboard** - Familiarize yourself with the main interface
5. **Create First Task** - Add your first task to get started
6. **Set Up Reminders** - Configure reminders for important deadlines

### For Teams
1. **Invite Members** - Send collaboration invites to team members
2. **Create Projects** - Set up shared projects and task lists
3. **Assign Tasks** - Distribute work among team members
4. **Track Progress** - Monitor team productivity and completion rates
5. **Communicate** - Use the built-in messaging system for team coordination

## Admin Features

The admin dashboard provides comprehensive system management:

**User Management**
- View all registered users and their activity
- Manage user permissions and access levels
- Monitor user engagement and productivity metrics
- Handle user support requests and issues

**Task Oversight**
- View all tasks across the entire system
- Monitor task completion rates and trends
- Identify bottlenecks and productivity issues
- Generate reports on task management effectiveness

**System Analytics**
- Track application usage statistics
- Monitor system performance and health
- Generate insights on user behavior patterns
- Create custom reports and dashboards

**Security Management**
- Monitor authentication attempts and security events
- Manage user credentials and access controls
- Review security logs and potential threats
- Implement security policies and compliance measures

## Security Features

**Data Protection**
- All passwords are hashed using BCrypt with salt
- Sensitive data is encrypted before storage
- Secure communication using HTTPS/TLS
- Regular security audits and updates

**Privacy Compliance**
- User data is handled according to privacy best practices
- Clear data retention policies
- User control over personal information
- Transparent data usage policies

**Access Control**
- Role-based permissions system
- Multi-factor authentication support
- Session management and timeout controls
- Secure API key management

## Contributing

We welcome contributions to BrainWave! Here's how you can help:

1. **Fork the Repository** - Create your own copy of the project
2. **Create Feature Branch** - git checkout -b feature/AmazingFeature
3. **Make Changes** - Implement your improvements or bug fixes
4. **Test Thoroughly** - Ensure your changes work across all platforms
5. **Commit Changes** - git commit -m 'Add some AmazingFeature'
6. **Push to Branch** - git push origin feature/AmazingFeature
7. **Open Pull Request** - Submit your changes for review

### Development Guidelines
- Follow C# coding standards and best practices
- Write comprehensive unit tests for new features
- Update documentation for any API changes
- Ensure cross-platform compatibility
- Test on multiple devices and platforms

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Author

**Mosia Thabang**
- GitHub: [@MosiaThabangEphraim](https://github.com/MosiaThabangEphraim)
- Email: Contact through GitHub

## Acknowledgments

- Built with [.NET MAUI](https://docs.microsoft.com/en-us/dotnet/maui/) - Microsoft's cross-platform framework
- Backend powered by [Supabase](https://supabase.com/) - Open source Firebase alternative
- UI components inspired by [Material Design](https://material.io/) - Google's design system
- Email services provided by [SendGrid](https://sendgrid.com/) - Reliable email delivery

## Support

If you encounter any issues or have questions:

1. **Check Documentation** - Review this README and inline code comments
2. **Search Issues** - Look through existing GitHub issues for solutions
3. **Create Issue** - Open a new issue with detailed problem description
4. **Contact Author** - Reach out directly through GitHub

## Roadmap

Future features and improvements planned for BrainWave:

- **Advanced Analytics** - Detailed productivity insights and reporting
- **Mobile Notifications** - Push notifications for mobile platforms
- **File Attachments** - Support for attaching files to tasks
- **Calendar Integration** - Sync with Google Calendar, Outlook, etc.
- **Time Tracking** - Built-in time tracking for tasks
- **API Access** - RESTful API for third-party integrations
- **Themes** - Customizable UI themes and color schemes
- **Offline Mode** - Full offline functionality with sync

---

**BrainWave** - Transform your productivity with intelligent task management and seamless team collaboration. Built for the modern, connected world.
