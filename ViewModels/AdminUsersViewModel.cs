using System.Collections.ObjectModel;
using System.Windows.Input;
using BrainWave.APP.Services;
using BrainWave.APP.Models;

namespace BrainWave.APP.ViewModels
{
    public class AdminUsersViewModel : BaseViewModel
    {
        private ObservableCollection<AdminUserDto> users = new();
        public ObservableCollection<AdminUserDto> Users
        {
            get => users;
            set => Set(ref users, value);
        }

        private string searchText = string.Empty;
        public string SearchText
        {
            get => searchText;
            set
            {
                if (searchText != value)
                {
                    searchText = value;
                    OnPropertyChanged(nameof(SearchText));
                    _ = FilterUsersAsync();
                }
            }
        }

        private string selectedRole = "All";
        public string SelectedRole
        {
            get => selectedRole;
            set
            {
                if (selectedRole != value)
                {
                    selectedRole = value;
                    OnPropertyChanged(nameof(SelectedRole));
                    _ = FilterUsersAsync();
                }
            }
        }

        private string selectedSortBy = "First Name";
        public string SelectedSortBy
        {
            get => selectedSortBy;
            set
            {
                if (selectedSortBy != value)
                {
                    selectedSortBy = value;
                    OnPropertyChanged(nameof(SelectedSortBy));
                    _ = FilterUsersAsync();
                }
            }
        }

        private string selectedSortOrder = "Ascending";
        public string SelectedSortOrder
        {
            get => selectedSortOrder;
            set
            {
                if (selectedSortOrder != value)
                {
                    selectedSortOrder = value;
                    OnPropertyChanged(nameof(SelectedSortOrder));
                    _ = FilterUsersAsync();
                }
            }
        }

        public List<string> RoleOptions { get; } = new() { "All", "Student", "Professional" };
        
        public List<string> SortOptions { get; } = new() { "First Name", "Last Name", "Email" };
        
        public List<string> SortOrderOptions { get; } = new() { "Ascending", "Descending" };

        private readonly AuthenticationService _authService;
        private readonly SupabaseService _supabaseService;
        private List<AdminUserDto> allUsers = new();

        public AdminUsersViewModel() : this(new AuthenticationService(new SupabaseService()), new SupabaseService())
        {
        }

        public AdminUsersViewModel(AuthenticationService authService, SupabaseService supabaseService)
        {
            _authService = authService;
            _supabaseService = supabaseService;
        }

        public async Task LoadUsersAsync()
        {
            if (IsBusy) return;
            IsBusy = true;

            try
            {
                // Load users from Supabase
                var users = await _supabaseService.GetAllUsersAsync();
                System.Diagnostics.Debug.WriteLine($"Loaded {users.Count} users from Supabase");
                
                allUsers = users.Select(u => new AdminUserDto
                {
                    UserID = u.userid,
                    F_Name = u.f_name,
                    L_Name = u.l_name,
                    Email = u.email,
                    Role = u.role,
                    TaskCount = 0 // We could add task count later if needed
                }).ToList();
                
                // Add a test user if no users are found
                if (allUsers.Count == 0)
                {
                    allUsers.Add(new AdminUserDto
                    {
                        UserID = 999,
                        F_Name = "Test",
                        L_Name = "User",
                        Email = "test@example.com",
                        Role = "Student",
                        TaskCount = 0
                    });
                    System.Diagnostics.Debug.WriteLine("Added test user since no users found");
                }
                
                System.Diagnostics.Debug.WriteLine($"Created {allUsers.Count} AdminUserDto objects");
                await FilterUsersAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading users: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", $"Failed to load users: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task FilterUsersAsync()
        {
            try
            {
                var filteredUsers = allUsers.AsEnumerable();

                // Filter by search text
                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    filteredUsers = filteredUsers.Where(u => 
                        u.F_Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                        u.L_Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                        u.Email.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
                }

                // Filter by role
                if (SelectedRole != "All")
                {
                    filteredUsers = filteredUsers.Where(u => u.Role == SelectedRole);
                }

                // Sort by selected criteria
                var isAscending = SelectedSortOrder == "Ascending";
                filteredUsers = SelectedSortBy switch
                {
                    "First Name" => isAscending ? filteredUsers.OrderBy(u => u.F_Name) : filteredUsers.OrderByDescending(u => u.F_Name),
                    "Last Name" => isAscending ? filteredUsers.OrderBy(u => u.L_Name) : filteredUsers.OrderByDescending(u => u.L_Name),
                    "Email" => isAscending ? filteredUsers.OrderBy(u => u.Email) : filteredUsers.OrderByDescending(u => u.Email),
                    _ => isAscending ? filteredUsers.OrderBy(u => u.F_Name) : filteredUsers.OrderByDescending(u => u.F_Name)
                };

                var filteredList = filteredUsers.ToList();
                System.Diagnostics.Debug.WriteLine($"Filtered to {filteredList.Count} users");

                Users.Clear();
                foreach (var user in filteredList)
                {
                    Users.Add(user);
                }
                
                System.Diagnostics.Debug.WriteLine($"Added {Users.Count} users to ObservableCollection");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error filtering users: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", $"Failed to filter users: {ex.Message}", "OK");
            }
        }

        public async Task ViewUserAsync(AdminUserDto user)
        {
            try
            {
                // Navigate to user details page
                await Shell.Current.GoToAsync($"admin/user-details", new Dictionary<string, object>
                {
                    ["User"] = user
                });
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to view user: {ex.Message}", "OK");
            }
        }

        public async Task AddUserAsync()
        {
            try
            {
                var firstName = await Shell.Current.DisplayPromptAsync("Add User", "Enter first name:", "Add", "Cancel");
                if (string.IsNullOrWhiteSpace(firstName)) return;

                var lastName = await Shell.Current.DisplayPromptAsync("Add User", "Enter last name:", "Add", "Cancel");
                if (string.IsNullOrWhiteSpace(lastName)) return;

                var email = await Shell.Current.DisplayPromptAsync("Add User", "Enter email:", "Add", "Cancel");
                if (string.IsNullOrWhiteSpace(email)) return;

                var role = await Shell.Current.DisplayActionSheet("Select Role", "Cancel", null, "Student", "Professional");
                if (role == "Cancel") return;

                var password = await Shell.Current.DisplayPromptAsync("Add User", "Enter password:", "Add", "Cancel");
                if (string.IsNullOrWhiteSpace(password)) return;

                // Create new user
                var newUser = new BrainWave.APP.Database.User
                {
                    f_name = firstName,
                    l_name = lastName,
                    email = email,
                    role = role,
                    password_hash = password, // In production, this should be hashed
                    profile_picture = null
                };

                var success = await _supabaseService.CreateUserAsync(newUser);
                if (success)
                {
                    await LoadUsersAsync(); // Refresh the list
                    await Shell.Current.DisplayAlert("Success", "User added successfully.", "OK");
                }
                else
                {
                    await Shell.Current.DisplayAlert("Error", "Failed to add user.", "OK");
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to add user: {ex.Message}", "OK");
            }
        }

        public async Task EditUserAsync(AdminUserDto user)
        {
            try
            {
                var firstName = await Shell.Current.DisplayPromptAsync("Edit User", "Enter first name:", user.F_Name, "Update", "Cancel");
                if (string.IsNullOrWhiteSpace(firstName)) return;

                var lastName = await Shell.Current.DisplayPromptAsync("Edit User", "Enter last name:", user.L_Name, "Update", "Cancel");
                if (string.IsNullOrWhiteSpace(lastName)) return;

                var email = await Shell.Current.DisplayPromptAsync("Edit User", "Enter email:", user.Email, "Update", "Cancel");
                if (string.IsNullOrWhiteSpace(email)) return;

                var role = await Shell.Current.DisplayActionSheet("Select Role", "Cancel", null, "Student", "Professional", "User", "Admin");
                if (role == "Cancel") return;

                // Update user
                var updatedUser = new BrainWave.APP.Database.User
                {
                    userid = user.UserID,
                    f_name = firstName,
                    l_name = lastName,
                    email = email,
                    role = role,
                    profile_picture = null
                };

                var success = await _supabaseService.UpdateUserAsync(updatedUser);
                if (success)
                {
                    await LoadUsersAsync(); // Refresh the list
                    await Shell.Current.DisplayAlert("Success", "User updated successfully.", "OK");
                }
                else
                {
                    await Shell.Current.DisplayAlert("Error", "Failed to update user.", "OK");
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to update user: {ex.Message}", "OK");
            }
        }

        public async Task DeleteUserAsync(AdminUserDto user)
        {
            try
            {
                var result = await Shell.Current.DisplayAlert(
                    "Delete User", 
                    $"Are you sure you want to delete {user.FullName}?", 
                    "Yes", "No");

                if (result)
                {
                    var success = await _supabaseService.DeleteUserAsync(user.UserID);
                    if (success)
                    {
                        allUsers.Remove(user);
                        Users.Remove(user);
                        await Shell.Current.DisplayAlert("Success", "User deleted successfully.", "OK");
                    }
                    else
                    {
                        await Shell.Current.DisplayAlert("Error", "Failed to delete user.", "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to delete user: {ex.Message}", "OK");
            }
        }
    }
}