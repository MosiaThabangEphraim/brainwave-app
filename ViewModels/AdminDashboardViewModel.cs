using System.Windows.Input;
using BrainWave.APP.Services;
using BrainWave.APP.Database;
using BrainWave.APP.Models;

namespace BrainWave.APP.ViewModels
{
    public class AdminDashboardViewModel : BaseViewModel
    {
        private int totalUsers;
        public int TotalUsers
        {
            get => totalUsers;
            set => Set(ref totalUsers, value);
        }

        private int totalTasks;
        public int TotalTasks
        {
            get => totalTasks;
            set => Set(ref totalTasks, value);
        }

        private int completedTasks;
        public int CompletedTasks
        {
            get => completedTasks;
            set => Set(ref completedTasks, value);
        }

        private int inProgressTasks;
        public int InProgressTasks
        {
            get => inProgressTasks;
            set => Set(ref inProgressTasks, value);
        }

        private int pendingTasks;
        public int PendingTasks
        {
            get => pendingTasks;
            set => Set(ref pendingTasks, value);
        }

        private int totalReminders;
        public int TotalReminders
        {
            get => totalReminders;
            set => Set(ref totalReminders, value);
        }

        private int totalCollaborations;
        public int TotalCollaborations
        {
            get => totalCollaborations;
            set => Set(ref totalCollaborations, value);
        }

        private int totalExports;
        public int TotalExports
        {
            get => totalExports;
            set => Set(ref totalExports, value);
        }

        private int totalBadges;
        public int TotalBadges
        {
            get => totalBadges;
            set => Set(ref totalBadges, value);
        }

        private int studentCount;
        public int StudentCount
        {
            get => studentCount;
            set => Set(ref studentCount, value);
        }

        private int professionalCount;
        public int ProfessionalCount
        {
            get => professionalCount;
            set => Set(ref professionalCount, value);
        }

        private List<TopUserDto> topUsers = new();
        public List<TopUserDto> TopUsers
        {
            get => topUsers;
            set => Set(ref topUsers, value);
        }

        private readonly AuthenticationService _authService;
        private readonly SupabaseService _supabaseService;

        public AdminDashboardViewModel() : this(new AuthenticationService(new SupabaseService()), new SupabaseService())
        {
        }

        public AdminDashboardViewModel(AuthenticationService authService, SupabaseService supabaseService)
        {
            _authService = authService;
            _supabaseService = supabaseService;
        }

        public async Task LoadDataAsync()
        {
            if (IsBusy) return;
            IsBusy = true;

            try
            {
                // Load admin statistics from database
                var allUsers = await _supabaseService.GetAllUsersAsync();
                var allTasks = await _supabaseService.GetAllTasksAsync();
                
                // User statistics
                TotalUsers = allUsers.Count;
                StudentCount = allUsers.Count(u => u.role?.ToLower() == "student");
                ProfessionalCount = allUsers.Count(u => u.role?.ToLower() == "professional");
                
                // Debug output to help identify the mismatch
                System.Diagnostics.Debug.WriteLine($"Total Users: {TotalUsers}");
                System.Diagnostics.Debug.WriteLine($"Students: {StudentCount}");
                System.Diagnostics.Debug.WriteLine($"Professionals: {ProfessionalCount}");
                System.Diagnostics.Debug.WriteLine($"Sum: {StudentCount + ProfessionalCount}");
                
                // List all user roles for debugging
                var roleGroups = allUsers.GroupBy(u => u.role).Select(g => new { Role = g.Key, Count = g.Count() });
                foreach (var group in roleGroups)
                {
                    System.Diagnostics.Debug.WriteLine($"Role '{group.Role}': {group.Count} users");
                }
                
                // Task statistics
                TotalTasks = allTasks.Count;
                CompletedTasks = allTasks.Count(t => t.task_status?.ToLower() == "completed");
                InProgressTasks = allTasks.Count(t => t.task_status?.ToLower() == "in progress");
                PendingTasks = 0; // No pending tasks
                
                // Get other statistics
                await LoadAdditionalStatisticsAsync();
                
                // Get top users
                await LoadTopUsersAsync();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to load data: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task LoadAdditionalStatisticsAsync()
        {
            try
            {
                // Get reminders count
                var allReminders = await _supabaseService.Client
                    .From<BrainWave.APP.Database.Reminder>()
                    .Get();
                TotalReminders = allReminders.Models.Count;

                // Get collaborations count
                var allCollaborations = await _supabaseService.Client
                    .From<BrainWave.APP.Database.Collaboration>()
                    .Get();
                TotalCollaborations = allCollaborations.Models.Count;

                // Get exports count
                var allExports = await _supabaseService.Client
                    .From<BrainWave.APP.Database.Export>()
                    .Get();
                TotalExports = allExports.Models.Count;

                // Get badges count
                var allBadges = await _supabaseService.Client
                    .From<BrainWave.APP.Database.Badge>()
                    .Get();
                TotalBadges = allBadges.Models.Count;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading additional statistics: {ex.Message}");
                // Set defaults if there's an error
                TotalReminders = 0;
                TotalCollaborations = 0;
                TotalExports = 0;
                TotalBadges = 0;
            }
        }

        private async Task LoadTopUsersAsync()
        {
            try
            {
                var allUsers = await _supabaseService.GetAllUsersAsync();
                var allTasks = await _supabaseService.GetAllTasksAsync();
                
                var topUsersList = new List<TopUserDto>();
                
                foreach (var user in allUsers)
                {
                    var userTasks = allTasks.Where(t => t.userid == user.userid).ToList();
                    var completedTasks = userTasks.Count(t => t.task_status?.ToLower() == "completed");
                    
                    if (userTasks.Count > 0) // Only include users with tasks
                    {
                        topUsersList.Add(new TopUserDto
                        {
                            UserID = user.userid,
                            FullName = $"{user.f_name} {user.l_name}",
                            Email = user.email,
                            Role = user.role ?? "User",
                            CompletedTasks = completedTasks,
                            TotalTasks = userTasks.Count
                        });
                    }
                }
                
                // Sort by completion rate and take top 5
                TopUsers = topUsersList
                    .OrderByDescending(u => u.CompletionRate)
                    .ThenByDescending(u => u.CompletedTasks)
                    .Take(3)
                    .ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading top users: {ex.Message}");
                TopUsers = new List<TopUserDto>();
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
                System.Diagnostics.Debug.WriteLine($"Logout error: {ex.Message}");
                // Try alternative navigation
                try
                {
                    await Shell.Current.GoToAsync("LoginPage");
                }
                catch
                {
                    // If all navigation fails, just continue
                }
            }
        }
    }
}
