using BrainWave.APP.Models;
using BrainWave.APP.ViewModels;
using Microsoft.Maui.Controls;

namespace BrainWave.APP.Views
{
    public partial class ProfilePage : ContentPage
    {
        private readonly ProfileViewModel _vm;
        
        public ProfilePage()
        {
            InitializeComponent();
            _vm = new ProfileViewModel();
            BindingContext = _vm;
        }
        
        public ProfilePage(ProfileViewModel vm) 
        { 
            InitializeComponent(); 
            BindingContext = _vm = vm; 
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
                
                // Load the user profile data
                await _vm.LoadProfileAsync();
            }
        }
        private async void Save_Clicked(object s, EventArgs e)
        {
            if (await _vm.UpdateAsync()) await DisplayAlert("Saved", "Profile updated", "OK");
            else await DisplayAlert("Error", "Failed to update", "OK");
        }
        private async void UpdatePassword_Clicked(object s, EventArgs e)
        {
            await Shell.Current.GoToAsync("PasswordUpdatePage");
        }


        private async void DeleteAccount_Clicked(object s, EventArgs e)
        {
            // First warning
            var firstWarning = await DisplayAlert(
                "⚠️ Delete Account", 
                "Are you sure you want to permanently delete your account?\n\nThis will delete ALL your data including:\n• All tasks\n• All reminders\n• All collaborations\n• All badges\n• Your profile\n\nThis action CANNOT be undone!", 
                "Yes, Delete My Account", 
                "Cancel");
            
            if (!firstWarning) return;

            // Second confirmation
            var secondWarning = await DisplayAlert(
                "🚨 Final Warning", 
                "This is your final warning!\n\nDeleting your account will permanently remove ALL your data from our servers.\n\nAre you absolutely certain you want to proceed?", 
                "Yes, Delete Everything", 
                "Cancel");
            
            if (!secondWarning) return;

            // Third confirmation with typing requirement
            var confirmText = await DisplayPromptAsync(
                "Type 'DELETE' to confirm",
                "To confirm account deletion, please type 'DELETE' in the box below:",
                "Delete Account",
                "Cancel",
                "Type DELETE here",
                -1,
                Keyboard.Default);

            if (confirmText?.ToUpper() != "DELETE")
            {
                await DisplayAlert("Cancelled", "Account deletion cancelled. Your account is safe.", "OK");
                return;
            }

            // Proceed with deletion
            if (await _vm.DeleteAccountAsync())
            {
                await DisplayAlert("Account Deleted", "Your account and all data have been permanently deleted.", "OK");
                await Shell.Current.GoToAsync("///LoginPage");
            }
            else
            {
                await DisplayAlert("Error", "Failed to delete account. Please try again or contact support.", "OK");
            }
        }

        private async void Logout_Clicked(object s, EventArgs e) 
        { 
            var result = await DisplayAlert("Logout", "Are you sure you want to logout?", "Yes", "No");
            if (result)
            {
                await _vm.LogoutAsync();
            }
        }


        private async void UpdateDetails_Clicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("ProfileDetailsPage");
        }
    }
}