using Microsoft.Maui;
using Microsoft.Maui.Hosting;
using BrainWave.APP.Services;
using BrainWave.APP.ViewModels;
using BrainWave.APP.Views;

namespace BrainWave.APP;
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Register services and viewmodels
        builder.Services.AddSingleton<SupabaseService>();
        builder.Services.AddSingleton<AuthenticationService>();
        builder.Services.AddSingleton<NavigationService>();
        builder.Services.AddTransient<DashboardViewModel>();
        builder.Services.AddTransient<TasksViewModel>();
        builder.Services.AddTransient<ProfileViewModel>();
        builder.Services.AddTransient<RemindersViewModel>();
        builder.Services.AddTransient<CollaborationViewModel>();
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<RegisterViewModel>();
        builder.Services.AddTransient<AdminLoginViewModel>();
        builder.Services.AddTransient<AdminUsersViewModel>();
        builder.Services.AddTransient<AdminUserDetailsViewModel>();
        builder.Services.AddTransient<AdminCredentialsViewModel>();
        builder.Services.AddTransient<AdminTasksViewModel>();
        builder.Services.AddTransient<AdminDashboardViewModel>();
        builder.Services.AddTransient<CollaborationDetailViewModel>();

        // Register views
        builder.Services.AddTransient<DashboardPage>();
        builder.Services.AddTransient<TasksPage>();
        builder.Services.AddTransient<ProfilePage>();
        builder.Services.AddTransient<RemindersPage>();
        builder.Services.AddTransient<CollaborationPage>();
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<RegisterPage>();
        builder.Services.AddTransient<AdminLoginPage>();
        builder.Services.AddTransient<AdminUsersPage>();
        builder.Services.AddTransient<AdminUserDetailsPage>();
        builder.Services.AddTransient<AdminCredentialsPage>();
        builder.Services.AddTransient<AdminTasksPage>();
        builder.Services.AddTransient<AdminDashboardPage>();
        builder.Services.AddTransient<CollaborationDetailPage>();

        return builder.Build();
    }
}








