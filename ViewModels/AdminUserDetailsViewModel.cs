using BrainWave.APP.Services;
using BrainWave.APP.Models;

namespace BrainWave.APP.ViewModels
{
    public class AdminUserDetailsViewModel : BaseViewModel
    {
        private AdminUserDto user = new();
        public AdminUserDto User
        {
            get => user;
            set => Set(ref user, value);
        }

        private string newPassword = string.Empty;
        public string NewPassword
        {
            get => newPassword;
            set => Set(ref newPassword, value);
        }

        // Track if there are pending changes
        private bool hasPendingChanges = false;
        public bool HasPendingChanges
        {
            get => hasPendingChanges;
            set => Set(ref hasPendingChanges, value);
        }

        public List<string> RoleOptions { get; } = new() { "Student", "Professional" };

        private readonly SupabaseService _supabaseService;

        public AdminUserDetailsViewModel() : this(new SupabaseService())
        {
        }

        public AdminUserDetailsViewModel(SupabaseService supabaseService)
        {
            _supabaseService = supabaseService;
        }

        public void SetUser(AdminUserDto user)
        {
            if (user != null)
            {
                User = user;
                HasPendingChanges = false; // Reset pending changes when loading new user
                NewPassword = string.Empty; // Clear password field
                OnPropertyChanged(nameof(User));
                OnPropertyChanged(nameof(User.F_Name));
                OnPropertyChanged(nameof(User.L_Name));
                OnPropertyChanged(nameof(User.Email));
                OnPropertyChanged(nameof(User.Role));
            }
        }

        public async Task EditFieldAsync(string fieldName, string currentValue, Func<string, Task> updateAction)
        {
            try
            {
                var newValue = await Shell.Current.DisplayPromptAsync($"Edit {fieldName}", $"Enter new {fieldName.ToLower()}:", "Update", "Cancel", currentValue);
                if (!string.IsNullOrWhiteSpace(newValue) && newValue != currentValue)
                {
                    await updateAction(newValue);
                    HasPendingChanges = true;
                    await Shell.Current.DisplayAlert("Success", $"{fieldName} updated. Click 'Confirm Update' to save changes.", "OK");
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to update {fieldName.ToLower()}: {ex.Message}", "OK");
            }
        }

        public async Task UpdateUserFieldAsync(string fieldName, string newValue)
        {
            try
            {
                // Update the specific field in the User object (UI only)
                switch (fieldName.ToLower())
                {
                    case "f_name":
                        User.F_Name = newValue;
                        break;
                    case "l_name":
                        User.L_Name = newValue;
                        break;
                    case "email":
                        User.Email = newValue;
                        break;
                    case "role":
                        User.Role = newValue;
                        break;
                }

                // Trigger UI updates
                OnPropertyChanged(nameof(User));
                OnPropertyChanged(nameof(User.F_Name));
                OnPropertyChanged(nameof(User.L_Name));
                OnPropertyChanged(nameof(User.Email));
                OnPropertyChanged(nameof(User.Role));
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to update {fieldName.ToLower()}: {ex.Message}", "OK");
            }
        }

        public async Task EditRoleAsync()
        {
            try
            {
                var newRole = await Shell.Current.DisplayActionSheet("Select Role", "Cancel", null, "Student", "Professional");
                if (newRole != "Cancel" && newRole != User.Role)
                {
                    await UpdateUserFieldAsync("role", newRole);
                    HasPendingChanges = true;
                    await Shell.Current.DisplayAlert("Success", "Role updated. Click 'Confirm Update' to save changes.", "OK");
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to update role: {ex.Message}", "OK");
            }
        }

        public async Task EditPasswordAsync()
        {
            try
            {
                var newPassword = await Shell.Current.DisplayPromptAsync("Edit Password", "Enter new password:", "Update", "Cancel", "");
                if (!string.IsNullOrWhiteSpace(newPassword))
                {
                    NewPassword = newPassword;
                    HasPendingChanges = true;
                    await Shell.Current.DisplayAlert("Success", "Password updated. Click 'Confirm Update' to save changes.", "OK");
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to update password: {ex.Message}", "OK");
            }
        }

        public async Task ConfirmUpdateAsync()
        {
            if (IsBusy) return;
            IsBusy = true;

            try
            {
                if (!HasPendingChanges)
                {
                    await Shell.Current.DisplayAlert("Info", "No changes to save.", "OK");
                    return;
                }

                // Create updated user object with all current values
                var updatedUser = new BrainWave.APP.Database.User
                {
                    userid = User.UserID,
                    f_name = User.F_Name,
                    l_name = User.L_Name,
                    email = User.Email,
                    role = User.Role,
                    profile_picture = null
                };

                // Update password if it was changed
                if (!string.IsNullOrWhiteSpace(NewPassword))
                {
                    updatedUser.password_hash = BCrypt.Net.BCrypt.HashPassword(NewPassword);
                }

                var success = await _supabaseService.UpdateUserAsync(updatedUser);
                if (success)
                {
                    HasPendingChanges = false;
                    NewPassword = string.Empty; // Clear password field
                    await Shell.Current.DisplayAlert("Success", "User updated successfully in database!", "OK");
                }
                else
                {
                    await Shell.Current.DisplayAlert("Error", "Failed to update user in database.", "OK");
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to update user: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task DeleteUserAsync()
        {
            if (IsBusy) return;
            IsBusy = true;

            try
            {
                var success = await _supabaseService.DeleteUserAsync(User.UserID);
                if (success)
                {
                    await Shell.Current.DisplayAlert("Success", "User deleted successfully.", "OK");
                    await Shell.Current.GoToAsync("admin/users");
                }
                else
                {
                    await Shell.Current.DisplayAlert("Error", "Failed to delete user.", "OK");
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to delete user: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
