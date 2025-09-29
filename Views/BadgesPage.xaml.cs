using BrainWave.APP.ViewModels;
using BrainWave.APP.Services;

namespace BrainWave.APP.Views;

public partial class BadgesPage : ContentPage
{
    private readonly BadgesViewModel _vm;
    
    public BadgesPage()
    {
        InitializeComponent();
        _vm = new BadgesViewModel(new DatabaseService(new SupabaseService()));
        BindingContext = _vm;
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
            
            // Always refresh badges when page appears
            System.Diagnostics.Debug.WriteLine("BadgesPage OnAppearing - Refreshing badges...");
            await _vm.LoadAsync();
        }
    }

    private async void HelpButton_Clicked(object sender, EventArgs e)
    {
        var helpText = @"💡 Badges Help

🏅 My Badges - View all your productivity achievements and progress

Check Badges - Check for new badges you may have earned

🔄 Refresh - Refresh the badges list to see latest achievements

🚪 Logout - Sign out of your account

📊 Your Progress - See your current progress and next badge requirements

🏆 All Badges - View all available badges and their requirements

Badges are earned by completing tasks and reaching productivity milestones. Keep working to unlock more achievements!";

        await DisplayAlert("💡 Badges Help", helpText, "OK");
    }
}
