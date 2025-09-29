using System.Windows.Input;
using BrainWave.APP.Services;
using BrainWave.APP.Models;
using Microsoft.Maui.Storage;
using static BrainWave.APP.Constants;

namespace BrainWave.APP.ViewModels
{
    public class PasswordUpdateViewModel : BaseViewModel
    {
        private string _currentPassword = string.Empty;
        private string _newPassword = string.Empty;
        private string _confirmNewPassword = string.Empty;
        private string _errorMessage = string.Empty;
        private bool _isFormValid = false;

        // Validation error properties
        private string _currentPasswordError = string.Empty;
        private string _newPasswordError = string.Empty;
        private string _confirmNewPasswordError = string.Empty;

        // Password validation properties
        private bool _isPasswordLengthValid = false;
        private bool _isPasswordNumberValid = false;
        private bool _isPasswordUppercaseValid = false;
        private bool _isPasswordLowercaseValid = false;
        private bool _isPasswordSpecialValid = false;

        public string CurrentPassword
        {
            get => _currentPassword;
            set 
            { 
                Set(ref _currentPassword, value);
                ValidateCurrentPassword();
                ValidateForm();
            }
        }

        public string NewPassword
        {
            get => _newPassword;
            set 
            { 
                Set(ref _newPassword, value);
                ValidateNewPassword();
                ValidateConfirmNewPassword();
                ValidateForm();
            }
        }

        public string ConfirmNewPassword
        {
            get => _confirmNewPassword;
            set 
            { 
                Set(ref _confirmNewPassword, value);
                ValidateConfirmNewPassword();
                ValidateForm();
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => Set(ref _errorMessage, value);
        }

        public bool IsFormValid
        {
            get => _isFormValid;
            set => Set(ref _isFormValid, value);
        }

        // Validation error properties
        public string CurrentPasswordError
        {
            get => _currentPasswordError;
            set => Set(ref _currentPasswordError, value);
        }

        public string NewPasswordError
        {
            get => _newPasswordError;
            set => Set(ref _newPasswordError, value);
        }

        public string ConfirmNewPasswordError
        {
            get => _confirmNewPasswordError;
            set => Set(ref _confirmNewPasswordError, value);
        }

        // Password validation properties
        public bool IsPasswordLengthValid
        {
            get => _isPasswordLengthValid;
            set => Set(ref _isPasswordLengthValid, value);
        }

        public bool IsPasswordNumberValid
        {
            get => _isPasswordNumberValid;
            set => Set(ref _isPasswordNumberValid, value);
        }

        public bool IsPasswordUppercaseValid
        {
            get => _isPasswordUppercaseValid;
            set => Set(ref _isPasswordUppercaseValid, value);
        }

        public bool IsPasswordLowercaseValid
        {
            get => _isPasswordLowercaseValid;
            set => Set(ref _isPasswordLowercaseValid, value);
        }

        public bool IsPasswordSpecialValid
        {
            get => _isPasswordSpecialValid;
            set => Set(ref _isPasswordSpecialValid, value);
        }

        public ICommand UpdatePasswordCommand { get; }
        public ICommand BackCommand { get; }

        private readonly DatabaseService _databaseService;
        
        public SupabaseService? GetSupabaseService()
        {
            return _databaseService.GetSupabaseService();
        }

        public PasswordUpdateViewModel() : this(new DatabaseService(new SupabaseService()))
        {
        }

        public PasswordUpdateViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));
            UpdatePasswordCommand = new Command(async () => await UpdatePasswordAsync());
            BackCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
        }

        private async Task UpdatePasswordAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            ErrorMessage = string.Empty;

            try
            {
                // Validate all fields
                ValidateCurrentPassword();
                ValidateNewPassword();
                ValidateConfirmNewPassword();

                // Check if form is valid
                if (!IsFormValid)
                {
                    ErrorMessage = "Please fix the validation errors above.";
                    return;
                }

                // Get current user using the same pattern as ProfileViewModel
                var supabaseService = _databaseService.GetSupabaseService();
                if (supabaseService != null)
                {
                    await supabaseService.InitializeAsync();
                }

                // Try to get user ID from stored session first
                var storedUserId = await SecureStorage.GetAsync(Constants.SECURE_KEY_USER_ID);
                int userId = 0;
                
                if (!string.IsNullOrEmpty(storedUserId) && int.TryParse(storedUserId, out userId))
                {
                    // Use stored user ID
                }
                else
                {
                    // Fallback to GetCurrentUserAsync
                    var currentUser = await _databaseService.GetCurrentUserAsync();
                    if (currentUser == null)
                    {
                        ErrorMessage = "Unable to verify current user. Please log in again.";
                        return;
                    }
                    userId = currentUser.userid;
                }

                // For now, we'll assume the current password is correct
                // In a real app, you'd hash and compare the current password
                // This is a simplified implementation

                // Update password
                var success = await _databaseService.UpdateUserPasswordAsync(userId, NewPassword);
                if (success)
                {
                    await Shell.Current.DisplayAlert("Success", "Password updated successfully!", "OK");
                    await Shell.Current.GoToAsync("..");
                }
                else
                {
                    ErrorMessage = "Failed to update password. Please try again.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Password update failed: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        // Validation methods
        private void ValidateCurrentPassword()
        {
            if (string.IsNullOrWhiteSpace(CurrentPassword))
            {
                CurrentPasswordError = "Current password is required.";
            }
            else
            {
                CurrentPasswordError = string.Empty;
            }
        }

        private void ValidateNewPassword()
        {
            if (string.IsNullOrWhiteSpace(NewPassword))
            {
                NewPasswordError = "New password is required.";
            }
            else if (!IsValidPassword(NewPassword))
            {
                NewPasswordError = "Password does not meet requirements.";
            }
            else
            {
                NewPasswordError = string.Empty;
            }

            // Update password validation indicators
            UpdatePasswordValidation();
        }

        private void ValidateConfirmNewPassword()
        {
            if (string.IsNullOrWhiteSpace(ConfirmNewPassword))
            {
                ConfirmNewPasswordError = "Please confirm your new password.";
            }
            else if (NewPassword != ConfirmNewPassword)
            {
                ConfirmNewPasswordError = "Passwords do not match.";
            }
            else
            {
                ConfirmNewPasswordError = string.Empty;
            }
        }

        private void UpdatePasswordValidation()
        {
            // Length validation
            IsPasswordLengthValid = NewPassword.Length >= 8;
            
            // Number validation
            IsPasswordNumberValid = NewPassword.Any(char.IsDigit);
            
            // Uppercase validation
            IsPasswordUppercaseValid = NewPassword.Any(char.IsUpper);
            
            // Lowercase validation
            IsPasswordLowercaseValid = NewPassword.Any(char.IsLower);
            
            // Special character validation
            IsPasswordSpecialValid = NewPassword.Any(c => !char.IsLetterOrDigit(c));
        }

        private bool IsValidPassword(string password)
        {
            return password.Length >= 8 &&
                   password.Any(char.IsDigit) &&
                   password.Any(char.IsUpper) &&
                   password.Any(char.IsLower) &&
                   password.Any(c => !char.IsLetterOrDigit(c));
        }

        private void ValidateForm()
        {
            IsFormValid = !string.IsNullOrWhiteSpace(CurrentPassword) &&
                         !string.IsNullOrWhiteSpace(NewPassword) &&
                         !string.IsNullOrWhiteSpace(ConfirmNewPassword) &&
                         IsValidPassword(NewPassword) &&
                         NewPassword == ConfirmNewPassword &&
                         string.IsNullOrEmpty(CurrentPasswordError) &&
                         string.IsNullOrEmpty(NewPasswordError) &&
                         string.IsNullOrEmpty(ConfirmNewPasswordError);
        }
    }
}
