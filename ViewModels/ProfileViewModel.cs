using BrainWave.APP.Models;
using BrainWave.APP.Services;
using BrainWave.APP.ViewModels;
using System.Windows.Input;
using static BrainWave.APP.Constants;
using System.Diagnostics;
using Microsoft.Maui.Storage;

namespace BrainWave.APP.ViewModels;
public class ProfileViewModel : BaseViewModel
{
    private readonly AuthenticationService _authService;
    private readonly SupabaseService _supabaseService;
    private readonly DatabaseService _databaseService;
    
    private UserDtos _me = new();
    public UserDtos Me 
    { 
        get => _me; 
        set => Set(ref _me, value); 
    }
    public ICommand UpdateCommand { get; }
    public ICommand LogoutCommand { get; }
    
    public SupabaseService GetSupabaseService()
    {
        return _supabaseService;
    }
    
    private bool _hasPendingChanges = false;
    public bool HasPendingChanges
    {
        get => _hasPendingChanges;
        set => Set(ref _hasPendingChanges, value);
    }

    public ProfileViewModel() : this(new AuthenticationService(new SupabaseService()), new SupabaseService(), new DatabaseService(new SupabaseService()))
    {
    }

    public ProfileViewModel(AuthenticationService authService, SupabaseService supabaseService, DatabaseService databaseService)
    {
        _authService = authService;
        _supabaseService = supabaseService;
        _databaseService = databaseService;
        UpdateCommand = new Command(async () => await UpdateAsync());
        LogoutCommand = new Command(async () => await LogoutAsync());
    }

    public async Task LoadProfileAsync()
    {
        if (IsBusy) return;
        IsBusy = true;

        try
        {
            // First ensure Supabase is initialized
            await _supabaseService.InitializeAsync();
            
            // Try to get user ID from stored session first
            var storedUserId = await SecureStorage.GetAsync(Constants.SECURE_KEY_USER_ID);
            if (!string.IsNullOrEmpty(storedUserId) && int.TryParse(storedUserId, out int userId))
            {
                // Get user by ID directly
                var user = await _databaseService.GetUserByIdAsync(userId);
                if (user != null)
                {
                    var updatedMe = new UserDtos
                    {
                        UserID = user.userid,
                        F_Name = user.f_name ?? "",
                        L_Name = user.l_name ?? "",
                        Email = user.email ?? "",
                        Role = user.role ?? "",
                        Profile_Picture = user.profile_picture
                    };
                    Me = updatedMe;
                    return;
                }
            }
            
            // Fallback to GetCurrentUserAsync
            var currentUser = await _databaseService.GetCurrentUserAsync();
            if (currentUser != null)
            {
                var updatedMe = new UserDtos
                {
                    UserID = currentUser.userid,
                    F_Name = currentUser.f_name ?? "",
                    L_Name = currentUser.l_name ?? "",
                    Email = currentUser.email ?? "",
                    Role = currentUser.role ?? "",
                    Profile_Picture = currentUser.profile_picture
                };
                Me = updatedMe;
            }
            else
            {
                Debug.WriteLine("Current user not found");
                Me = new UserDtos();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to load profile: {ex.Message}");
            Me = new UserDtos();
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task<bool> UpdateAsync()
    {
        if (IsBusy) return false;
        IsBusy = true;

        try
        {
            // Use the current Me object's UserID directly
            if (Me?.UserID > 0)
            {
                var user = new Database.User
                {
                    userid = Me.UserID,
                    f_name = Me.F_Name,
                    l_name = Me.L_Name,
                    email = Me.Email,
                    role = Me.Role,
                    profile_picture = Me.Profile_Picture,
                    password_hash = "" // Will be handled by the update method
                };
                
                var success = await _databaseService.UpdateUserAsync(user);
                if (success)
                {
                    HasPendingChanges = false;
                    await Shell.Current.DisplayAlert("Success", "Profile updated successfully!", "OK");
                }
                else
                {
                    await Shell.Current.DisplayAlert("Error", "Failed to update profile.", "OK");
                }
                return success;
            }
            else
            {
                await Shell.Current.DisplayAlert("Error", "User information not available. Please refresh and try again.", "OK");
            }
            return false;
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Failed to update profile: {ex.Message}", "OK");
            return false;
        }
        finally
        {
            IsBusy = false;
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
                await Shell.Current.DisplayAlert("Success", $"{fieldName} updated. Click 'Save Changes' to save changes.", "OK");
            }
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
            var currentRole = Me?.Role ?? "";
            var action = await Shell.Current.DisplayActionSheet("Select Role", "Cancel", null, "Student", "Professional");
            
            if (action != "Cancel" && action != currentRole)
            {
                Me.Role = action;
                HasPendingChanges = true;
                await Shell.Current.DisplayAlert("Success", $"Role updated to {action}. Click 'Save Changes' to save changes.", "OK");
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Failed to update role: {ex.Message}", "OK");
        }
    }

    public async Task ChangePhotoAsync()
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
                // Store the file path in the profile picture field
                Me.Profile_Picture = result.FullPath;
                HasPendingChanges = true;
                
                await Shell.Current.DisplayAlert("Success", "Profile picture updated. Click 'Save Changes' to save changes.", "OK");
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Failed to select photo: {ex.Message}", "OK");
        }
    }

    public async Task<bool> SwitchRoleAsync(string newRole)
    {
        if (IsBusy) return false;
        IsBusy = true;

        try
        {
            if (Me == null) return false;

            var success = await _databaseService.UpdateUserRoleAsync(Me.UserID, newRole);
            if (success)
            {
                Me.Role = newRole; // Update the local model
            }
            return success;
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Failed to switch role: {ex.Message}", "OK");
            return false;
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task<bool> DeleteAccountAsync()
    {
        if (IsBusy) return false;
        IsBusy = true;

        try
        {
            if (Me == null) 
            {
                Debug.WriteLine("❌ DeleteAccountAsync: Me is null, cannot delete account");
                return false;
            }

            Debug.WriteLine($"🗑️ DeleteAccountAsync: Starting account deletion for user ID: {Me.UserID}, Email: {Me.Email}");

            // Delete the user account and all associated data
            var success = await _databaseService.DeleteUserAccountAsync(Me.UserID);
            
            Debug.WriteLine($"🗑️ DeleteAccountAsync: Database deletion result: {success}");
            
            if (success)
            {
                Debug.WriteLine("🗑️ DeleteAccountAsync: Database deletion successful, proceeding with logout");
                
                // Logout the user after successful deletion
                await _supabaseService.LogoutAsync();
                Debug.WriteLine("🗑️ DeleteAccountAsync: Logout completed");
                
                // Verify user is actually deleted by trying to get user by email
                var deletedUser = await _supabaseService.GetUserByEmailAsync(Me.Email);
                if (deletedUser == null)
                {
                    Debug.WriteLine("✅ DeleteAccountAsync: User successfully deleted from database - cannot be found by email");
                }
                else
                {
                    Debug.WriteLine($"❌ DeleteAccountAsync: User still exists in database after deletion! UserID: {deletedUser.userid}");
                }
            }
            else
            {
                Debug.WriteLine("❌ DeleteAccountAsync: Database deletion failed");
            }
            
            return success;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌ DeleteAccountAsync: Exception occurred: {ex.Message}");
            Debug.WriteLine($"❌ DeleteAccountAsync: Stack trace: {ex.StackTrace}");
            await Shell.Current.DisplayAlert("Error", $"Failed to delete account: {ex.Message}", "OK");
            return false;
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task LogoutAsync()
    {
        try
        {
            await _authService.LogoutAsync();
            await Shell.Current.GoToAsync("///LoginPage");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Logout failed: {ex.Message}", "OK");
        }
    }
}