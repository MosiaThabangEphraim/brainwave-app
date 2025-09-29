using BrainWave.APP.Models;
using BrainWave.APP.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace BrainWave.APP.ViewModels
{
    public class BadgesViewModel : BaseViewModel
    {
        private readonly DatabaseService _databaseService;
        private int _completedTasksCount;
        private string _currentBadgeName = string.Empty;
        private string _nextBadgeProgress = string.Empty;

        public ObservableCollection<BadgeModel> AllBadges { get; } = new();

        public int CompletedTasksCount
        {
            get => _completedTasksCount;
            set => Set(ref _completedTasksCount, value);
        }

        public string CurrentBadgeName
        {
            get => _currentBadgeName;
            set => Set(ref _currentBadgeName, value);
        }

        public string NextBadgeProgress
        {
            get => _nextBadgeProgress;
            set => Set(ref _nextBadgeProgress, value);
        }

        public ICommand LoadCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand CheckBadgesCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand RefreshBadgesCommand { get; }

        // Default constructor for XAML binding
        public BadgesViewModel() : this(new DatabaseService(new SupabaseService()))
        {
        }

        public BadgesViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));
            LoadCommand = new Command(async () => await LoadAsync());
            RefreshCommand = new Command(async () => await RefreshBadgesAsync());
            CheckBadgesCommand = new Command(async () => await ManuallyCheckBadgesAsync());
            LogoutCommand = new Command(async () => await LogoutAsync());
            RefreshBadgesCommand = new Command(async () => await RefreshBadgesAsync());
            
            // Initialize badges
            InitializeBadges();
        }

        private void InitializeBadges()
        {
            AllBadges.Clear();
            
            // Amateur Badge (1-25 tasks)
            AllBadges.Add(new BadgeModel
            {
                BadgeId = 1,
                Name = "Amateur",
                Description = "Complete 1-25 tasks",
                Slogan = "Every journey begins with a single step!",
                Icon = "🌱",
                RequiredTasks = 1,
                BadgeColor = Colors.LightGreen
            });

            // Achiever Badge (26-50 tasks)
            AllBadges.Add(new BadgeModel
            {
                BadgeId = 2,
                Name = "Achiever",
                Description = "Complete 26-50 tasks",
                Slogan = "Building momentum, one task at a time!",
                Icon = "⭐",
                RequiredTasks = 26,
                BadgeColor = Colors.Orange
            });

            // Task Master Badge (51-100 tasks)
            AllBadges.Add(new BadgeModel
            {
                BadgeId = 3,
                Name = "Task Master",
                Description = "Complete 51-100 tasks",
                Slogan = "Master of productivity and efficiency!",
                Icon = "🎯",
                RequiredTasks = 51,
                BadgeColor = Colors.Blue
            });

            // Productivity Champion Badge (100+ tasks)
            AllBadges.Add(new BadgeModel
            {
                BadgeId = 4,
                Name = "Productivity Champion",
                Description = "Complete 100+ tasks",
                Slogan = "The ultimate productivity warrior!",
                Icon = "👑",
                RequiredTasks = 100,
                BadgeColor = Colors.Purple
            });
        }

        public async Task LoadAsync()
        {
            if (IsBusy) return;
            IsBusy = true;

            try
            {
                // Get current user
                var currentUser = await _databaseService.GetCurrentUserAsync();
                if (currentUser == null) return;

                // Get completed tasks count
                var tasks = await _databaseService.GetTasksByUserIdAsync(currentUser.userid);
                CompletedTasksCount = tasks.Count(t => t.task_status == "Completed");
                System.Diagnostics.Debug.WriteLine($"BadgesViewModel: User {currentUser.userid} has {CompletedTasksCount} completed tasks");

                // Get earned badges from database
                var earnedBadges = await _databaseService.GetUserBadgesAsync(currentUser.userid);
                var earnedBadgeIds = earnedBadges.Select(b => b.badgeid).ToList();
                System.Diagnostics.Debug.WriteLine($"BadgesViewModel: User {currentUser.userid} has earned badges: [{string.Join(", ", earnedBadgeIds)}]");

                // Update badge statuses based on database records
                UpdateBadgeStatuses(earnedBadgeIds);

                // Update current badge and next badge progress
                UpdateCurrentBadgeInfo();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to load badges: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void UpdateBadgeStatuses(List<int> earnedBadgeIds)
        {
            System.Diagnostics.Debug.WriteLine($"UpdateBadgeStatuses called with earned badge IDs: [{string.Join(", ", earnedBadgeIds)}]");
            
            foreach (var badge in AllBadges)
            {
                // Check if badge is earned in database
                bool wasAchieved = badge.IsAchieved;
                badge.IsAchieved = earnedBadgeIds.Contains(badge.BadgeId);
                
                System.Diagnostics.Debug.WriteLine($"Badge {badge.Name} (ID: {badge.BadgeId}): WasAchieved={wasAchieved}, IsAchieved={badge.IsAchieved}");
                
                if (badge.IsAchieved)
                {
                    badge.StatusColor = Colors.Green;
                    badge.StatusText = "Earned";
                    System.Diagnostics.Debug.WriteLine($"✅ Badge {badge.Name} is EARNED - StatusColor=Green, StatusText=Earned");
                }
                else
                {
                    badge.StatusColor = Colors.Gray;
                    badge.StatusText = "Locked";
                    System.Diagnostics.Debug.WriteLine($"🔒 Badge {badge.Name} is LOCKED - StatusColor=Gray, StatusText=Locked");
                }
                
                // Force property change notification
                badge.OnPropertyChanged(nameof(badge.IsAchieved));
                badge.OnPropertyChanged(nameof(badge.StatusColor));
                badge.OnPropertyChanged(nameof(badge.StatusText));
            }
        }

        private void UpdateCurrentBadgeInfo()
        {
            // Find current badge (highest achieved badge)
            var currentBadge = AllBadges
                .Where(b => b.IsAchieved)
                .OrderByDescending(b => b.RequiredTasks)
                .FirstOrDefault();

            if (currentBadge != null)
            {
                CurrentBadgeName = currentBadge.Name;
            }
            else
            {
                CurrentBadgeName = "No Badge Yet";
            }

            // Find next badge to achieve
            var nextBadge = AllBadges
                .Where(b => !b.IsAchieved)
                .OrderBy(b => b.RequiredTasks)
                .FirstOrDefault();

            if (nextBadge != null)
            {
                var tasksNeeded = Math.Max(0, nextBadge.RequiredTasks - CompletedTasksCount);
                NextBadgeProgress = $"{nextBadge.Name} ({tasksNeeded} more tasks)";
            }
            else
            {
                NextBadgeProgress = "All badges earned! 🎉";
            }
        }

        public SupabaseService? GetSupabaseService()
        {
            return _databaseService.GetSupabaseService();
        }

        public async Task RefreshBadgesAsync()
        {
            await LoadAsync();
        }

        public async Task ManuallyCheckBadgesAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Manually checking badges...");
                await LoadAsync();
                System.Diagnostics.Debug.WriteLine("Badge check completed");
                var currentUser = await _databaseService.GetCurrentUserAsync();
                if (currentUser != null)
                {
                    await _databaseService.CheckAndAwardBadgesAsync(currentUser.userid);
                    await LoadAsync(); // Refresh the display
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to check badges: {ex.Message}", "OK");
            }
        }

        public async Task LogoutAsync()
        {
            try
            {
                var supabaseService = _databaseService.GetSupabaseService();
                if (supabaseService != null)
                {
                    await supabaseService.LogoutAsync();
                    await Shell.Current.GoToAsync("///LoginPage");
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to logout: {ex.Message}", "OK");
            }
        }
    }
}
