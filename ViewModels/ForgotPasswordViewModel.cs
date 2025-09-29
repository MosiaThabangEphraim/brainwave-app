using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using BrainWave.APP.Services;

namespace BrainWave.APP.ViewModels
{
    public class ForgotPasswordViewModel : INotifyPropertyChanged
    {
        private readonly SupabaseService _supabaseService;
        private readonly EmailService _emailService;
        private readonly PasswordResetTokenService _tokenService;
        private string _email = string.Empty;
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

        public ICommand SendTokenCommand { get; }
        public ICommand BackToLoginCommand { get; }

        public ForgotPasswordViewModel()
        {
            _supabaseService = new SupabaseService();
            _emailService = new EmailService();
            _tokenService = new PasswordResetTokenService();
            SendTokenCommand = new Command(async () => await SendTokenAsync(), () => !IsLoading);
            BackToLoginCommand = new Command(async () => await BackToLoginAsync());
        }

        private void ValidateForm()
        {
            var isValid = !string.IsNullOrWhiteSpace(Email) && IsValidEmail(Email);
            IsFormValid = isValid;
            
            // Clear error message when user starts typing
            if (!string.IsNullOrWhiteSpace(Email))
            {
                ErrorMessage = string.Empty;
            }
        }

        private bool IsValidEmail(string email)
        {
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

        public async Task SendTokenAsync()
        {
            System.Diagnostics.Debug.WriteLine($"SendTokenAsync called with email: {Email}");
            
            // Clear any previous error messages
            ErrorMessage = string.Empty;

            // Check if email is empty
            if (string.IsNullOrWhiteSpace(Email))
            {
                ErrorMessage = "Email address is required. Please enter your email address.";
                return;
            }

            // Check if email format is valid
            if (!IsValidEmail(Email))
            {
                ErrorMessage = "Please enter a valid email address";
                return;
            }

            System.Diagnostics.Debug.WriteLine("Starting email validation and sending process...");
            IsLoading = true;
            ((Command)SendTokenCommand).ChangeCanExecute();

            bool emailSent = false;
            string errorMsg = string.Empty;

            try
            {
                // Check if email exists in the system
                System.Diagnostics.Debug.WriteLine($"Checking if user exists for email: {Email}");
                var user = await _supabaseService.GetUserByEmailAsync(Email);
                
                if (user == null)
                {
                    System.Diagnostics.Debug.WriteLine("User not found in database");
                    errorMsg = "No account found with this email address. Please check your email or create a new account.";
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"User found: {user.email}");
                    
                    // Generate reset token
                    var resetToken = _tokenService.GenerateToken(Email);
                    System.Diagnostics.Debug.WriteLine($"Generated token: {resetToken}");
                    
                    // Send password reset email
                    System.Diagnostics.Debug.WriteLine("Attempting to send email...");
                    emailSent = await _emailService.SendPasswordResetEmailAsync(Email, resetToken);
                    
                    if (!emailSent)
                    {
                        System.Diagnostics.Debug.WriteLine("Email sending failed");
                        errorMsg = "Failed to send email. Please try again later.";
                    }
                }
            }
            catch (Exception ex)
            {
                errorMsg = $"An error occurred: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"SendTokenAsync error: {ex}");
            }
            finally
            {
                IsLoading = false;
                ((Command)SendTokenCommand).ChangeCanExecute();
                System.Diagnostics.Debug.WriteLine("SendTokenAsync completed - loading state reset");
            }

            // Handle results after finally block
            if (!string.IsNullOrEmpty(errorMsg))
            {
                ErrorMessage = errorMsg;
            }
            else if (emailSent)
            {
                System.Diagnostics.Debug.WriteLine("Email sent successfully");
                
                // Navigate to reset password page (user will enter email manually)
                await Shell.Current.GoToAsync("ResetPasswordPage");
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
