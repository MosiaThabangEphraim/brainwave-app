using BrainWave.APP.Services;

namespace BrainWave.APP.ViewModels
{
    public class AdminCredentialsViewModel : BaseViewModel
    {
        private string newUsername = string.Empty;
        public string NewUsername
        {
            get => newUsername;
            set => Set(ref newUsername, value);
        }

        private string newPassword = string.Empty;
        public string NewPassword
        {
            get => newPassword;
            set => Set(ref newPassword, value);
        }

        private string confirmPassword = string.Empty;
        public string ConfirmPassword
        {
            get => confirmPassword;
            set => Set(ref confirmPassword, value);
        }

        private string currentUsername = string.Empty;
        public string CurrentUsername
        {
            get => currentUsername;
            set => Set(ref currentUsername, value);
        }

        private string currentPassword = string.Empty;
        public string CurrentPassword
        {
            get => currentPassword;
            set => Set(ref currentPassword, value);
        }

        private readonly SupabaseService _supabaseService;

        public AdminCredentialsViewModel(SupabaseService supabaseService = null)
        {
            _supabaseService = supabaseService ?? new SupabaseService();
            LoadCurrentCredentials();
        }

        private async void LoadCurrentCredentials()
        {
            try
            {
                // Get stored credentials from SecureStorage
                var storedUsername = await SecureStorage.GetAsync("admin_username");
                var storedPassword = await SecureStorage.GetAsync("admin_password");
                
                if (!string.IsNullOrEmpty(storedUsername) && !string.IsNullOrEmpty(storedPassword))
                {
                    CurrentUsername = storedUsername;
                    CurrentPassword = storedPassword;
                }
                else
                {
                    // Fallback to hardcoded credentials
                    CurrentUsername = "admin";
                    CurrentPassword = "admin123!";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading current credentials: {ex.Message}");
                // Fallback to hardcoded credentials
                CurrentUsername = "admin";
                CurrentPassword = "admin123!";
            }
        }

        public async Task UpdateCredentialsAsync()
        {
            if (IsBusy) return;
            IsBusy = true;

            try
            {
                // Validate inputs
                if (string.IsNullOrWhiteSpace(NewUsername))
                {
                    await Shell.Current.DisplayAlert("Error", "Username is required.", "OK");
                    return;
                }

                if (string.IsNullOrWhiteSpace(NewPassword))
                {
                    await Shell.Current.DisplayAlert("Error", "Password is required.", "OK");
                    return;
                }

                if (NewPassword != ConfirmPassword)
                {
                    await Shell.Current.DisplayAlert("Error", "Passwords do not match.", "OK");
                    return;
                }

                if (NewPassword.Length < 6)
                {
                    await Shell.Current.DisplayAlert("Error", "Password must be at least 6 characters long.", "OK");
                    return;
                }

                // Update admin credentials in the database
                var success = await _supabaseService.UpdateAdminCredentialsAsync(NewUsername, NewPassword);
                
                if (success)
                {
                    await Shell.Current.DisplayAlert("Success", "Admin credentials updated successfully!", "OK");
                    
                    // Update current credentials display
                    CurrentUsername = NewUsername;
                    CurrentPassword = NewPassword;
                    
                    // Clear form
                    NewUsername = string.Empty;
                    NewPassword = string.Empty;
                    ConfirmPassword = string.Empty;
                    
                    // Navigate back to dashboard
                    await Shell.Current.GoToAsync("admin/dashboard");
                }
                else
                {
                    await Shell.Current.DisplayAlert("Error", "Failed to update admin credentials.", "OK");
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to update credentials: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
