using BrainWave.APP.ViewModels;
using BrainWave.APP.Services;

namespace BrainWave.APP.Views;
public partial class DashboardPage : ContentPage
{
    private readonly DashboardViewModel _vm;
    
    public DashboardPage()
    {
        InitializeComponent();
        _vm = new DashboardViewModel(new DatabaseService(new SupabaseService()), new NavigationService());
        BindingContext = _vm;
        
    }
    
    
    
    private async void HelpButton_Clicked(object sender, EventArgs e)
    {
        var helpText = @"💡 Dashboard Help

🔄 Refresh - Refresh dashboard data and statistics

➕ New Task - Create a new task to track your work

🔔 Reminders - View and manage your email reminders

👥 Collaborate - Join or create collaboration projects

👤 Profile - View and edit your profile settings

🚪 Logout - Sign out of your account

This dashboard shows your productivity overview with quick stats and upcoming tasks.";
        
        await DisplayAlert("💡 Dashboard Help", helpText, "OK");
    }
    
    
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        // Initialize SupabaseService if needed
        if (_vm != null)
        {
            // Get the SupabaseService from the DatabaseService
            var supabaseService = _vm.GetSupabaseService();
            if (supabaseService != null)
            {
                await supabaseService.InitializeAsync();
            }
            
            if (!_vm.Upcoming.Any()) 
                await _vm.LoadAsync();
        }
    }
    private async void Profile_Clicked(object s, EventArgs e) => await _vm.GoProfile();
    private async void Tasks_Clicked(object s, EventArgs e) => await _vm.GoTasks();
    private async void Collab_Clicked(object s, EventArgs e) => await _vm.GoCollab();
    private async void Reminders_Clicked(object s, EventArgs e) => await _vm.GoReminders();
    private async void NewTask_Clicked(object s, EventArgs e) => await _vm.GoTasks();
}