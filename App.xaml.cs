// MOSIA TE – 54607949
// KOMANE K – 44298919
// BOSELE KV – 46381848
// MABENA T – 50745646
// MLILWANA N – 45756635


namespace BrainWave.APP;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        MainPage = new AppShell();

        // Register routes
        Routing.RegisterRoute(nameof(Views.LoginPage), typeof(Views.LoginPage));
        Routing.RegisterRoute(nameof(Views.RegisterPage), typeof(Views.RegisterPage));
        Routing.RegisterRoute(nameof(Views.DashboardPage), typeof(Views.DashboardPage));
        Routing.RegisterRoute("admin/dashboard", typeof(Views.AdminDashboardPage));
        Routing.RegisterRoute("admin/users", typeof(Views.AdminUsersPage));
        Routing.RegisterRoute("admin/user-details", typeof(Views.AdminUserDetailsPage));
        Routing.RegisterRoute("admin/credentials", typeof(Views.AdminCredentialsPage));
        Routing.RegisterRoute("admin/tasks", typeof(Views.AdminTasksPage));
        Routing.RegisterRoute("TaskDetailPage", typeof(Views.TaskDetailPage));
        Routing.RegisterRoute("CollaborationDetailPage", typeof(Views.CollaborationDetailPage));
        Routing.RegisterRoute("test", typeof(Views.TestPage));
    }

    protected override async void OnStart()
    {
        base.OnStart();
        
        // Initialize Supabase
        try
        {
            var supabaseService = new Services.SupabaseService();
            await supabaseService.InitializeAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Supabase initialization error: {ex.Message}");
        }
    }
}
