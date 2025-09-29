using System.Windows.Input;
using BrainWave.APP.Services;
using BrainWave.APP.Models;
using Microsoft.Maui.Storage;

namespace BrainWave.APP.ViewModels
{
    public class RegisterViewModel : BaseViewModel
    {
        private string firstName = string.Empty;
        public string FirstName
        {
            get => firstName;
            set 
            { 
                Set(ref firstName, value);
                ValidateFirstName();
            }
        }

        private string lastName = string.Empty;
        public string LastName
        {
            get => lastName;
            set 
            { 
                Set(ref lastName, value);
                ValidateLastName();
            }
        }

        private string email = string.Empty;
        public string Email
        {
            get => email;
            set 
            { 
                Set(ref email, value);
                _ = ValidateEmailAsync();
                ValidateConfirmEmail();
            }
        }

        private string password = string.Empty;
        public string Password
        {
            get => password;
            set 
            { 
                Set(ref password, value);
                ValidatePassword();
                ValidateConfirmPassword();
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

        private string confirmPassword = string.Empty;
        public string ConfirmPassword
        {
            get => confirmPassword;
            set 
            { 
                Set(ref confirmPassword, value);
                ValidateConfirmPassword();
            }
        }

        private bool isConfirmPasswordVisible = false;
        public bool IsConfirmPasswordVisible
        {
            get => isConfirmPasswordVisible;
            set
            {
                if (isConfirmPasswordVisible != value)
                {
                    isConfirmPasswordVisible = value;
                    OnPropertyChanged(nameof(IsConfirmPasswordVisible));
                    OnPropertyChanged(nameof(ConfirmPasswordVisibilityIcon));
                }
            }
        }

        public string ConfirmPasswordVisibilityIcon => IsConfirmPasswordVisible ? "👁️" : "👁️‍🗨️";

        private string confirmEmail = string.Empty;
        public string ConfirmEmail
        {
            get => confirmEmail;
            set 
            { 
                Set(ref confirmEmail, value);
                ValidateConfirmEmail();
            }
        }

        private string errorMessage = string.Empty;
        public string ErrorMessage
        {
            get => errorMessage;
            set => Set(ref errorMessage, value);
        }

        // Validation error properties
        private string firstNameError = string.Empty;
        public string FirstNameError
        {
            get => firstNameError;
            set => Set(ref firstNameError, value);
        }

        private string lastNameError = string.Empty;
        public string LastNameError
        {
            get => lastNameError;
            set => Set(ref lastNameError, value);
        }

        private string emailError = string.Empty;
        public string EmailError
        {
            get => emailError;
            set => Set(ref emailError, value);
        }

        private string confirmEmailError = string.Empty;
        public string ConfirmEmailError
        {
            get => confirmEmailError;
            set => Set(ref confirmEmailError, value);
        }

        private string confirmPasswordError = string.Empty;
        public string ConfirmPasswordError
        {
            get => confirmPasswordError;
            set => Set(ref confirmPasswordError, value);
        }

        // Password validation properties
        private bool isPasswordLengthValid = false;
        public bool IsPasswordLengthValid
        {
            get => isPasswordLengthValid;
            set => Set(ref isPasswordLengthValid, value);
        }

        private bool isPasswordNumberValid = false;
        public bool IsPasswordNumberValid
        {
            get => isPasswordNumberValid;
            set => Set(ref isPasswordNumberValid, value);
        }

        private bool isPasswordUppercaseValid = false;
        public bool IsPasswordUppercaseValid
        {
            get => isPasswordUppercaseValid;
            set => Set(ref isPasswordUppercaseValid, value);
        }

        private bool isPasswordLowercaseValid = false;
        public bool IsPasswordLowercaseValid
        {
            get => isPasswordLowercaseValid;
            set => Set(ref isPasswordLowercaseValid, value);
        }

        private bool isPasswordSpecialValid = false;
        public bool IsPasswordSpecialValid
        {
            get => isPasswordSpecialValid;
            set => Set(ref isPasswordSpecialValid, value);
        }

        private string selectedRole = "Student";
        public string SelectedRole
        {
            get => selectedRole;
            set => Set(ref selectedRole, value);
        }

        private bool isStudentSelected = true;
        public bool IsStudentSelected
        {
            get => isStudentSelected;
            set => Set(ref isStudentSelected, value);
        }

        private bool isProfessionalSelected = false;
        public bool IsProfessionalSelected
        {
            get => isProfessionalSelected;
            set => Set(ref isProfessionalSelected, value);
        }

        private ImageSource profileImageSource = "default_profile.png";
        public ImageSource ProfileImageSource
        {
            get => profileImageSource;
            set => Set(ref profileImageSource, value);
        }

        public ICommand RegisterCommand { get; }
        public ICommand GoToLoginCommand { get; }
        public ICommand SelectStudentCommand { get; }
        public ICommand SelectProfessionalCommand { get; }
        public ICommand SelectImageCommand { get; }
        public ICommand TogglePasswordVisibilityCommand { get; }
        public ICommand ToggleConfirmPasswordVisibilityCommand { get; }

        private readonly AuthenticationService _authService;

        public RegisterViewModel() : this(new AuthenticationService(new SupabaseService()))
        {
        }

        public RegisterViewModel(AuthenticationService authService)
        {
            _authService = authService;
            RegisterCommand = new Command(async () => await RegisterAsync());
            GoToLoginCommand = new Command(async () =>
                await Shell.Current.GoToAsync("///LoginPage"));
            SelectStudentCommand = new Command(() => SelectRole("Student"));
            SelectProfessionalCommand = new Command(() => SelectRole("Professional"));
            SelectImageCommand = new Command(async () => await SelectImageAsync());
            TogglePasswordVisibilityCommand = new Command(() => IsPasswordVisible = !IsPasswordVisible);
            ToggleConfirmPasswordVisibilityCommand = new Command(() => IsConfirmPasswordVisible = !IsConfirmPasswordVisible);
        }

        private void SelectRole(string role)
        {
            SelectedRole = role;
            IsStudentSelected = role == "Student";
            IsProfessionalSelected = role == "Professional";
        }

        private async Task SelectImageAsync()
        {
            try
            {
                var result = await FilePicker.Default.PickAsync(new PickOptions
                {
                    PickerTitle = "Select Profile Picture",
                    FileTypes = FilePickerFileType.Images
                });

                if (result != null)
                {
                    ProfileImageSource = ImageSource.FromFile(result.FullPath);
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to select image: {ex.Message}", "OK");
            }
        }

        private async Task RegisterAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            ErrorMessage = string.Empty;

            try
            {
                // Validate all fields
                ValidateFirstName();
                ValidateLastName();
                await ValidateEmailAsync();
                ValidateConfirmEmail();
                ValidatePassword();
                ValidateConfirmPassword();

                // Check if form is valid
                if (!IsFormValid())
                {
                    ErrorMessage = "Please fix the validation errors above.";
                    return;
                }

                var registerRequest = new RegisterRequest
                {
                    FirstName = FirstName,
                    LastName = LastName,
                    Email = Email,
                    Password = Password,
                    Role = SelectedRole
                };

                var success = await _authService.RegisterAsync(registerRequest);
                if (success)
                {
                    // Navigate to login page after successful registration
                    await Shell.Current.DisplayAlert("Success", "Registration successful! Please log in.", "OK");
                    await Shell.Current.GoToAsync("///LoginPage");
                }
                else
                {
                    ErrorMessage = "Registration failed. Please try again.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Registration failed: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        // Validation methods
        private void ValidateFirstName()
        {
            if (string.IsNullOrWhiteSpace(FirstName))
            {
                FirstNameError = "First name is required.";
            }
            else
            {
                FirstNameError = string.Empty;
            }
        }

        private void ValidateLastName()
        {
            if (string.IsNullOrWhiteSpace(LastName))
            {
                LastNameError = "Last name is required.";
            }
            else
            {
                LastNameError = string.Empty;
            }
        }

        private async Task ValidateEmailAsync()
        {
            if (string.IsNullOrWhiteSpace(Email))
            {
                EmailError = "Email is required.";
            }
            else if (!IsValidEmail(Email))
            {
                EmailError = "Please enter a valid email address.";
            }
            else
            {
                // Check if email already exists
                try
                {
                    var databaseService = new DatabaseService(new SupabaseService());
                    var existingUser = await databaseService.GetUserByEmailAsync(Email);
                    if (existingUser != null)
                    {
                        EmailError = "This email address is already registered.";
                    }
                    else
                    {
                        EmailError = string.Empty;
                    }
                }
                catch (Exception ex)
                {
                    EmailError = "Error checking email availability.";
                    System.Diagnostics.Debug.WriteLine($"Email validation error: {ex.Message}");
                }
            }
        }

        private void ValidateConfirmEmail()
        {
            if (string.IsNullOrWhiteSpace(ConfirmEmail))
            {
                ConfirmEmailError = "Please confirm your email address.";
            }
            else if (Email != ConfirmEmail)
            {
                ConfirmEmailError = "Email addresses do not match.";
            }
            else
            {
                ConfirmEmailError = string.Empty;
            }
        }

        private void ValidatePassword()
        {
            // Length validation
            IsPasswordLengthValid = Password.Length >= 8;
            
            // Number validation
            IsPasswordNumberValid = Password.Any(char.IsDigit);
            
            // Uppercase validation
            IsPasswordUppercaseValid = Password.Any(char.IsUpper);
            
            // Lowercase validation
            IsPasswordLowercaseValid = Password.Any(char.IsLower);
            
            // Special character validation
            IsPasswordSpecialValid = Password.Any(c => !char.IsLetterOrDigit(c));
        }

        private void ValidateConfirmPassword()
        {
            if (string.IsNullOrWhiteSpace(ConfirmPassword))
            {
                ConfirmPasswordError = "Please confirm your password.";
            }
            else if (Password != ConfirmPassword)
            {
                ConfirmPasswordError = "Passwords do not match.";
            }
            else
            {
                ConfirmPasswordError = string.Empty;
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

        private bool IsFormValid()
        {
            return !string.IsNullOrWhiteSpace(FirstName) &&
                   !string.IsNullOrWhiteSpace(LastName) &&
                   !string.IsNullOrWhiteSpace(Email) &&
                   !string.IsNullOrWhiteSpace(ConfirmEmail) &&
                   !string.IsNullOrWhiteSpace(Password) &&
                   !string.IsNullOrWhiteSpace(ConfirmPassword) &&
                   IsValidEmail(Email) &&
                   Email == ConfirmEmail &&
                   Password == ConfirmPassword &&
                   IsPasswordLengthValid &&
                   IsPasswordNumberValid &&
                   IsPasswordUppercaseValid &&
                   IsPasswordLowercaseValid &&
                   IsPasswordSpecialValid;
        }
    }
}