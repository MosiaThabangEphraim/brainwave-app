using BrainWave.APP.Models;
using BrainWave.APP.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;
using BrainWave.APP.ViewModels;
namespace BrainWave.APP.ViewModels;
public class DashboardViewModel : BaseViewModel
{
    private readonly DatabaseService _databaseService;
    private readonly NavigationService _nav;

    // Default constructor for XAML binding
    public DashboardViewModel() : this(new DatabaseService(new SupabaseService()), new NavigationService())
    {
    }

    public DashboardViewModel(DatabaseService databaseService, NavigationService nav) : base()
    {
        _databaseService = databaseService;
        _nav = nav;
        RefreshCommand = new Command(async () => await LoadAsync());
        LogoutCommand = new Command(async () => await LogoutAsync());
    }

    public ObservableCollection<TaskDtos> Upcoming { get; } = new();
    
    private int activeTasksCount;
    public int ActiveTasksCount
    {
        get => activeTasksCount;
        set => Set(ref activeTasksCount, value);
    }
    
    private int dueTodayCount;
    public int DueTodayCount
    {
        get => dueTodayCount;
        set => Set(ref dueTodayCount, value);
    }
    
    private int completedTasksCount;
    public int CompletedTasksCount
    {
        get => completedTasksCount;
        set => Set(ref completedTasksCount, value);
    }
    
    private int remindersCount;
    public int RemindersCount
    {
        get => remindersCount;
        set => Set(ref remindersCount, value);
    }
    
    private int badgesCount;
    public int BadgesCount
    {
        get => badgesCount;
        set => Set(ref badgesCount, value);
    }

    public async Task LoadAsync()
    {
        if (IsBusy) return; 
        IsBusy = true;
        try
        {
            // Get current user first
            var currentUser = await _databaseService.GetCurrentUserAsync();
            if (currentUser != null)
            {
                var tasks = await _databaseService.GetTasksByUserIdAsync(currentUser.userid);
                var reminders = await _databaseService.GetRemindersByUserIdAsync(currentUser.userid);
                
                // Convert tasks to TaskDtos
                var taskDtos = tasks.Select(t => new TaskDtos
                {
                    TaskID = t.taskid,
                    Title = t.title,
                    Description = t.description,
                    DueDate = t.due_date.Kind == DateTimeKind.Utc ? t.due_date.ToLocalTime() : 
                             t.due_date.Kind == DateTimeKind.Unspecified ? 
                             DateTime.SpecifyKind(t.due_date, DateTimeKind.Local) : t.due_date,
                    Status = t.task_status,
                    Priority = t.priority_level,
                    UserID = t.userid,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                }).ToList();
                
                // Update collections
                Upcoming.Clear();
                foreach (var task in taskDtos.Where(t => t.Status != "Completed" && t.DueDate >= DateTime.Now))
                {
                    Upcoming.Add(task);
                }
                
                // Calculate statistics
                ActiveTasksCount = taskDtos.Count(t => t.Status == "In Progress");
                CompletedTasksCount = taskDtos.Count(t => t.Status == "Completed");
                
                // Calculate due today count (tasks due today)
                var today = DateTime.Today;
                DueTodayCount = taskDtos.Count(t => t.DueDate.Date == today && t.Status != "Completed");
                
                RemindersCount = reminders.Count;
                
                // Load badges count
                await LoadBadgesCountAsync(currentUser.userid);
            }

        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Dashboard load error: {ex.Message}");
        }
        finally 
        { 
            IsBusy = false; 
        }
    }

    public Task GoProfile() => _nav.GoAsync("///profile");
    public Task GoTasks() => _nav.GoAsync("///tasks");
    public Task GoReminders() => _nav.GoAsync("///reminders");
    public Task GoCollab() => _nav.GoAsync("///collab");

    public SupabaseService? GetSupabaseService()
    {
        return _databaseService.GetSupabaseService();
    }

    public ICommand RefreshCommand { get; }
    public ICommand LogoutCommand { get; }

    private async Task LoadBadgesCountAsync(int userId)
    {
        try
        {
            var badges = await _databaseService.GetUserBadgesAsync(userId);
            BadgesCount = badges.Count;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading badges count: {ex.Message}");
            BadgesCount = 0;
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