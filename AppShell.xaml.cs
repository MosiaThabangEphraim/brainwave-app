using static BrainWave.APP.Constants;
using BrainWave.APP.Services;
using System.Diagnostics;

// MOSIA TE – 54607949
// KOMANE K – 44298919
// BOSELE KV – 46381848
// MABENA T – 50745646
// MLILWANA N – 45756635

namespace BrainWave.APP;

public partial class AppShell : Shell
{
    private readonly AuthenticationService _authService;

    public AppShell()
    {
        InitializeComponent();
        _authService = new AuthenticationService(new SupabaseService());
        
        // Register routes
        Routing.RegisterRoute("ProfileDetailsPage", typeof(Views.ProfileDetailsPage));
        Routing.RegisterRoute("CreateTaskPage", typeof(Views.CreateTaskPage));
        Routing.RegisterRoute("CreateReminderPage", typeof(Views.CreateReminderPage));
        Routing.RegisterRoute("CreateCollaborationPage", typeof(Views.CreateCollaborationPage));
        Routing.RegisterRoute("JoinCollaborationPage", typeof(Views.JoinCollaborationPage));
        Routing.RegisterRoute("ForgotPasswordPage", typeof(Views.ForgotPasswordPage));
        Routing.RegisterRoute("ResetPasswordPage", typeof(Views.ResetPasswordPage));
        Routing.RegisterRoute("PasswordUpdatePage", typeof(Views.PasswordUpdatePage));
        Routing.RegisterRoute("ReminderDetailPage", typeof(Views.ReminderDetailPage));
        Routing.RegisterRoute("CollaborationDetailPage", typeof(Views.CollaborationDetailPage));
        
        // Always start with login page visible
        HideMainTabs();
        
        // Initialize Supabase and check authentication status
        Device.BeginInvokeOnMainThread(async () =>
        {
            await Task.Delay(500); // Give the shell more time to initialize
            try
            {
                // Initialize Supabase first
                var supabaseService = new SupabaseService();
                await supabaseService.InitializeAsync();
                
                // Check if user is already authenticated
                var isAuthenticated = await _authService.LoadStoredTokenAsync();
                if (isAuthenticated)
                {
                    // User is authenticated, show main tabs
                    ShowMainTabs();
                }
                else
                {
                    // User is not authenticated, show login page
                    HideMainTabs();
                    await Shell.Current.GoToAsync("///LoginPage");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"AppShell initialization error: {ex.Message}");
                // Default to login page on error
                HideMainTabs();
                await Shell.Current.GoToAsync("///LoginPage");
            }
        });
    }

    private async Task CheckAuthenticationStatus()
    {
        try
        {
            var isAuthenticated = await _authService.LoadStoredTokenAsync();
            if (!isAuthenticated)
            {
                // User is not authenticated, hide main tabs and navigate to login
                HideMainTabs();
                await Shell.Current.GoToAsync("///LoginPage");
            }
            else
            {
                // User is authenticated, show main tabs and navigate to appropriate dashboard
                ShowMainTabs();
                if (_authService.IsAdmin)
                {
                    await Shell.Current.GoToAsync("admin/dashboard");
                }
                else
                {
                    await Shell.Current.GoToAsync("///dashboard");
                }
            }
        }
        catch (Exception ex)
        {
            // If there's an error, default to login page
            System.Diagnostics.Debug.WriteLine($"Navigation error: {ex.Message}");
            try
            {
                HideMainTabs();
                await Shell.Current.GoToAsync("///LoginPage");
            }
            catch
            {
                // If navigation fails, just continue - the shell will show the default page
            }
        }
    }

    public async Task LogoutAsync()
    {
        await _authService.LogoutAsync();
        HideMainTabs();
        await Shell.Current.GoToAsync("///LoginPage");
    }

    public void ShowMainTabs()
    {
        MainTabBar.IsVisible = true;
    }

    public void HideMainTabs()
    {
        MainTabBar.IsVisible = false;
    }
}
