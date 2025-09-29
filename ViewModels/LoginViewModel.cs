using System.Windows.Input;
using BrainWave.APP.Services;
using BrainWave.APP.Models;
using Microsoft.Maui.Storage;
using static BrainWave.APP.Constants;

namespace BrainWave.APP.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private string email = string.Empty;
        public string Email
        {
            get => email;
            set
            {
                if (email != value)
                {
                    email = value;
                    OnPropertyChanged(nameof(Email));
                }
            }
        }

        private string password = string.Empty;
        public string Password
        {
            get => password;
            set
            {
                if (password != value)
                {
                    password = value;
                    OnPropertyChanged(nameof(Password));
                }
            }
        }

        private bool isPasswordVisible = false;
        public bool IsPasswordVisible
        {
            get => isPasswordVisible;
            set
            {
                if (isPasswordVisible != value)
                {
                    isPasswordVisible = value;
                    OnPropertyChanged(nameof(IsPasswordVisible));
                    OnPropertyChanged(nameof(PasswordVisibilityIcon));
                }
            }
        }

        public string PasswordVisibilityIcon => IsPasswordVisible ? "👁️" : "👁️‍🗨️";

        private string errorMessage = string.Empty;
        public string ErrorMessage
        {
            get => errorMessage;
            set
            {
                if (errorMessage != value)
                {
                    errorMessage = value;
                    OnPropertyChanged(nameof(ErrorMessage));
                }
            }
        }

        private bool isUserLogin = true;
        public bool IsUserLogin
        {
            get => isUserLogin;
            set
            {
                if (isUserLogin != value)
                {
                    isUserLogin = value;
                    OnPropertyChanged(nameof(IsUserLogin));
                    OnPropertyChanged(nameof(IsAdminLogin));
                    OnPropertyChanged(nameof(LoginPlaceholder));
                    OnPropertyChanged(nameof(LoginKeyboard));
                }
            }
        }

        public bool IsAdminLogin => !isUserLogin;

        public string LoginPlaceholder => IsUserLogin ? "Email Address" : "Admin Username";
        public Microsoft.Maui.Keyboard LoginKeyboard => IsUserLogin ? Microsoft.Maui.Keyboard.Email : Microsoft.Maui.Keyboard.Default;

        public ICommand LoginCommand { get; }
        public ICommand GoToRegisterCommand { get; }
        public ICommand SelectUserLoginCommand { get; }
        public ICommand SelectAdminLoginCommand { get; }
        public ICommand TogglePasswordVisibilityCommand { get; }
        public ICommand ForgotPasswordCommand { get; }
        public ICommand ExitCommand { get; }

        private readonly AuthenticationService _authService;

        public LoginViewModel() : this(new AuthenticationService(new SupabaseService()))
        {
        }

        public LoginViewModel(AuthenticationService authService)
        {
            _authService = authService;
            LoginCommand = new Command(async () => await LoginAsync());
            GoToRegisterCommand = new Command(async () =>
                await Shell.Current.GoToAsync("///RegisterPage"));
            SelectUserLoginCommand = new Command(() => IsUserLogin = true);
            SelectAdminLoginCommand = new Command(() => IsUserLogin = false);
            TogglePasswordVisibilityCommand = new Command(() => IsPasswordVisible = !IsPasswordVisible);
            ForgotPasswordCommand = new Command(async () => await Shell.Current.GoToAsync("///ForgotPasswordPage"));
            ExitCommand = new Command(async () => await ExitAppAsync());
        }

        private async Task LoginAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            ErrorMessage = string.Empty;

            try
            {
                bool success;
                if (IsUserLogin)
                {
                    // Regular user login
                    success = await _authService.LoginAsync(Email, Password);
                    if (success)
                    {
                        // Show main tabs and navigate to dashboard
                        var shell = Shell.Current as AppShell;
                        if (shell != null)
                        {
                            shell.ShowMainTabs();
                        }
                        await Shell.Current.GoToAsync("///dashboard");
                    }
                    else
                    {
                        ErrorMessage = "Invalid email or password.";
                    }
                }
                else
                {
                    // Admin login - use admin authentication
                    success = await _authService.AdminLoginAsync(Email, Password);
                    if (success && _authService.IsAdmin)
                    {
                        // Show main tabs and navigate to admin dashboard
                        var shell = Shell.Current as AppShell;
                        if (shell != null)
                        {
                            shell.ShowMainTabs();
                        }
                        await Shell.Current.GoToAsync("admin/dashboard");
                    }
                    else
                    {
                        ErrorMessage = "Invalid admin credentials.";
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Login failed: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task ExitAppAsync()
        {
            try
            {
                var result = await Shell.Current.DisplayAlert("Exit App", 
                    "Are you sure you want to exit BrainWave?", 
                    "Exit", "Cancel");
                
                if (result)
                {
                    // Close the application
                    Application.Current?.Quit();
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to exit app: {ex.Message}", "OK");
            }
        }
    }
}
