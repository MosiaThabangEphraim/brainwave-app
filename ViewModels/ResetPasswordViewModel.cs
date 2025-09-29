using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows.Input;
using BrainWave.APP.Services;

namespace BrainWave.APP.ViewModels
{
    public class ResetPasswordViewModel : INotifyPropertyChanged
    {
        private readonly SupabaseService _supabaseService;
        private readonly PasswordResetTokenService _tokenService;
        private string _email = string.Empty;
        private string _token = string.Empty;
        private string _newPassword = string.Empty;
        private string _confirmPassword = string.Empty;
        private bool _isNewPasswordVisible = false;
        private bool _isConfirmPasswordVisible = false;
        private bool _isFormValid = false;
        private string _errorMessage = string.Empty;
        private bool _isLoading = false;

        public string Email
        {
            get => _email;
            set
            {
                if (_email != value)
                {
                    _email = value;
                    OnPropertyChanged();
                    ValidateForm();
                }
            }
        }

        public string Token
        {
            get => _token;
            set
            {
                if (_token != value)
                {
                    _token = value;
                    OnPropertyChanged();
                    ValidateForm();
                }
            }
        }

        public string NewPassword
        {
            get => _newPassword;
            set
            {
                if (_newPassword != value)
                {
                    _newPassword = value;
                    OnPropertyChanged();
                    ValidateForm();
                }
            }
        }

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set
            {
                if (_confirmPassword != value)
                {
                    _confirmPassword = value;
                    OnPropertyChanged();
                    ValidateForm();
                }
            }
        }

        public bool IsNewPasswordVisible
        {
            get => _isNewPasswordVisible;
            set
            {
                if (_isNewPasswordVisible != value)
                {
                    _isNewPasswordVisible = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsConfirmPasswordVisible
        {
            get => _isConfirmPasswordVisible;
            set
            {
                if (_isConfirmPasswordVisible != value)
                {
                    _isConfirmPasswordVisible = value;
                    OnPropertyChanged();
                }
            }
        }

        public string NewPasswordVisibilityIcon => IsNewPasswordVisible ? "eye_off.png" : "eye.png";
        public string ConfirmPasswordVisibilityIcon => IsConfirmPasswordVisible ? "eye_off.png" : "eye.png";

        public bool IsFormValid
        {
            get => _isFormValid;
            set
            {
                if (_isFormValid != value)
                {
                    _isFormValid = value;
                    OnPropertyChanged();
                }
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                if (_errorMessage != value)
                {
                    _errorMessage = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (_isLoading != value)
                {
                    _isLoading = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand ResetPasswordCommand { get; }
        public ICommand BackToLoginCommand { get; }
        public ICommand ToggleNewPasswordVisibilityCommand { get; }
        public ICommand ToggleConfirmPasswordVisibilityCommand { get; }

        public ResetPasswordViewModel()
        {
            _supabaseService = new SupabaseService();
            _tokenService = new PasswordResetTokenService();
            ResetPasswordCommand = new Command(async () => await ResetPasswordAsync(), () => !IsLoading);
            BackToLoginCommand = new Command(async () => await BackToLoginAsync());
            ToggleNewPasswordVisibilityCommand = new Command(() => IsNewPasswordVisible = !IsNewPasswordVisible);
            ToggleConfirmPasswordVisibilityCommand = new Command(() => IsConfirmPasswordVisible = !IsConfirmPasswordVisible);
        }

        private void ValidateForm()
        {
            var isValid = !string.IsNullOrWhiteSpace(Email) && 
                         IsValidEmail(Email) &&
                         !string.IsNullOrWhiteSpace(Token) && 
                         Token.Length == 6 && 
                         !string.IsNullOrWhiteSpace(NewPassword) && 
                         !string.IsNullOrWhiteSpace(ConfirmPassword) &&
                         IsValidPassword(NewPassword) &&
                         NewPassword == ConfirmPassword;
            
            IsFormValid = isValid;
            
            // Show validation errors
            if (!string.IsNullOrWhiteSpace(Email) && !IsValidEmail(Email))
            {
                ErrorMessage = "Please enter a valid email address.";
            }
            else if (!string.IsNullOrWhiteSpace(Token) && Token.Length != 6)
            {
                ErrorMessage = "Token must be exactly 6 digits.";
            }
            else if (!string.IsNullOrWhiteSpace(NewPassword) && !IsValidPassword(NewPassword))
            {
                ErrorMessage = "Password must be at least 8 characters long and contain uppercase, lowercase, and numeric characters.";
            }
            else if (!string.IsNullOrWhiteSpace(NewPassword) && !string.IsNullOrWhiteSpace(ConfirmPassword) && NewPassword != ConfirmPassword)
            {
                ErrorMessage = "Passwords do not match.";
            }
            else if (isValid)
            {
                ErrorMessage = string.Empty;
            }
        }

        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private bool IsValidPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
                return false;

            // Check for at least one uppercase letter
            if (!Regex.IsMatch(password, @"[A-Z]"))
                return false;

            // Check for at least one lowercase letter
            if (!Regex.IsMatch(password, @"[a-z]"))
                return false;

            // Check for at least one number
            if (!Regex.IsMatch(password, @"[0-9]"))
                return false;

            return true;
        }

        public async Task ResetPasswordAsync()
        {
            // Clear any previous error messages
            ErrorMessage = string.Empty;

            // Validate email
            if (string.IsNullOrWhiteSpace(Email) || !IsValidEmail(Email))
            {
                ErrorMessage = "Please enter a valid email address.";
                return;
            }

            // Validate token
            if (string.IsNullOrWhiteSpace(Token) || Token.Length != 6)
            {
                ErrorMessage = "Please enter a valid 6-digit token.";
                return;
            }

            // Validate password
            if (string.IsNullOrWhiteSpace(NewPassword))
            {
                ErrorMessage = "Please enter a new password.";
                return;
            }

            if (!IsValidPassword(NewPassword))
            {
                ErrorMessage = "Password must be at least 8 characters long and contain uppercase, lowercase, and numeric characters.";
                return;
            }

            if (NewPassword != ConfirmPassword)
            {
                ErrorMessage = "Passwords do not match.";
                return;
            }

            IsLoading = true;
            ((Command)ResetPasswordCommand).ChangeCanExecute();

            try
            {
                System.Diagnostics.Debug.WriteLine($"ResetPasswordAsync - Token: '{Token}', Email: '{Email}'");
                
                // Validate token (without email requirement)
                if (!_tokenService.ValidateToken(Token))
                {
                    System.Diagnostics.Debug.WriteLine($"Token validation failed for token: '{Token}'");
                    ErrorMessage = "Invalid or expired token. Please request a new password reset.";
                    return;
                }
                
                System.Diagnostics.Debug.WriteLine("Token validation successful");

                // Get user from database using the entered email
                var user = await _supabaseService.GetUserByEmailAsync(Email);
                if (user == null)
                {
                    ErrorMessage = "User not found. Please check your email address or request a new password reset.";
                    return;
                }

                // Update password in database
                user.password_hash = BCrypt.Net.BCrypt.HashPassword(NewPassword);
                var updateSuccess = await _supabaseService.UpdateUserAsync(user);

                if (updateSuccess)
                {
                    // Mark token as used
                    _tokenService.MarkTokenAsUsed(Token);

                    await Shell.Current.DisplayAlert("Success", "Your password has been reset successfully. You can now log in with your new password.", "OK");
                    await Shell.Current.GoToAsync("///LoginPage");
                }
                else
                {
                    ErrorMessage = "Failed to update password. Please try again.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An error occurred: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
                ((Command)ResetPasswordCommand).ChangeCanExecute();
            }
        }

        public async Task BackToLoginAsync()
        {
            await Shell.Current.GoToAsync("///LoginPage");
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}