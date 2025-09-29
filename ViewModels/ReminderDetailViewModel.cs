using BrainWave.APP.Models;
using BrainWave.APP.Services;
using System.Windows.Input;

namespace BrainWave.APP.ViewModels
{
    public class ReminderDetailViewModel : BaseViewModel
    {
        private readonly DatabaseService _databaseService;
        private ReminderModel _reminder = new ReminderModel();
        private TaskDtos _relatedTask = new TaskDtos();
        private List<TaskDtos> _availableTasks = new List<TaskDtos>();
        private TaskDtos? _selectedTask;
        private bool _hasPendingChanges = false;
        private string _errorMessage = string.Empty;

        public ReminderDetailViewModel() : this(new DatabaseService(new SupabaseService()))
        {
        }

        public ReminderDetailViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
            
            // Initialize commands
            BackCommand = new Command(async () => await BackAsync());
        }

        public ICommand BackCommand { get; }

        public ReminderModel Reminder
        {
            get => _reminder;
            set => Set(ref _reminder, value);
        }

        public TaskDtos RelatedTask
        {
            get => _relatedTask;
            set => Set(ref _relatedTask, value);
        }

        public List<TaskDtos> AvailableTasks
        {
            get => _availableTasks;
            set => Set(ref _availableTasks, value);
        }

        public TaskDtos? SelectedTask
        {
            get => _selectedTask;
            set => Set(ref _selectedTask, value);
        }

        public bool HasPendingChanges
        {
            get => _hasPendingChanges;
            set => Set(ref _hasPendingChanges, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => Set(ref _errorMessage, value);
        }

        public async void SetReminder(ReminderModel reminder)
        {
            Reminder = reminder;
            
            // Initialize reminder with default values if needed
            if (string.IsNullOrEmpty(Reminder.Title))
                Reminder.Title = "Untitled Reminder";
            if (string.IsNullOrEmpty(Reminder.Description))
                Reminder.Description = "No description";
            if (Reminder.ReminderTime == DateTime.MinValue)
                Reminder.ReminderTime = DateTime.Now.AddHours(1); // Default to 1 hour from now
            
            // Load available tasks
            await LoadAvailableTasksAsync();
            
            // Set the selected task if we have a related task
            if (Reminder.ReminderID > 0)
            {
                // Try to find the related task in the available tasks
                var relatedTask = AvailableTasks.FirstOrDefault(t => t.TaskID == Reminder.ReminderID);
                if (relatedTask != null)
                {
                    SelectedTask = relatedTask;
                }
            }
            
            // Trigger UI updates
            OnPropertyChanged(nameof(Reminder));
            OnPropertyChanged(nameof(Reminder.Title));
            OnPropertyChanged(nameof(Reminder.Description));
            OnPropertyChanged(nameof(Reminder.ReminderTime));
        }

        private async Task LoadAvailableTasksAsync()
        {
            try
            {
                IsBusy = true;
                var currentUser = await _databaseService.GetCurrentUserAsync();
                if (currentUser != null)
                {
                    var tasks = await _databaseService.GetTasksByUserIdAsync(currentUser.userid);
                    if (tasks != null)
                    {
                        // Convert TaskItem to TaskDtos
                        AvailableTasks = tasks.Select(t => new TaskDtos
                        {
                            TaskID = t.taskid,
                            Title = t.title,
                            Description = t.description ?? string.Empty,
                            DueDate = t.due_date.Kind == DateTimeKind.Utc ? t.due_date.ToLocalTime() : 
                                     t.due_date.Kind == DateTimeKind.Unspecified ? 
                                     DateTime.SpecifyKind(t.due_date, DateTimeKind.Local) : t.due_date,
                            Status = t.task_status,
                            Priority = t.priority_level,
                            UserID = t.userid,
                            CreatedAt = DateTime.Now, // TaskItem doesn't have created_at
                            UpdatedAt = DateTime.Now  // TaskItem doesn't have updated_at
                        }).ToList();
                    }
                    else
                    {
                        AvailableTasks = new List<TaskDtos>();
                    }
                    OnPropertyChanged(nameof(AvailableTasks));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading available tasks: {ex.Message}");
                AvailableTasks = new List<TaskDtos>();
            }
            finally
            {
                IsBusy = false;
            }
        }

        public SupabaseService? GetSupabaseService()
        {
            return _databaseService.GetSupabaseService();
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

        public async Task UpdateReminderFieldAsync(string fieldName, string newValue)
        {
            try
            {
                // Update the specific field in the Reminder object (UI only)
                switch (fieldName.ToLower())
                {
                    case "title":
                        Reminder.Title = newValue;
                        break;
                    case "description":
                        Reminder.Description = newValue;
                        break;
                }

                // Trigger UI updates
                OnPropertyChanged(nameof(Reminder));
                OnPropertyChanged(nameof(Reminder.Title));
                OnPropertyChanged(nameof(Reminder.Description));
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to update {fieldName.ToLower()}: {ex.Message}", "OK");
            }
        }



        public async Task ConfirmUpdateAsync()
        {
            try
            {
                IsBusy = true;
                ErrorMessage = string.Empty;

                // Convert ReminderModel to Database.Reminder
                var dbReminder = new Database.Reminder
                {
                    reminderid = Reminder.ReminderID,
                    taskid = SelectedTask?.TaskID ?? 0, // Use selected task ID
                    reminder_type = "Email", // Always email for reminders
                    notify_time = Reminder.ReminderTime
                };

                var success = await _databaseService.UpdateReminderAsync(dbReminder);
                if (success)
                {
                    HasPendingChanges = false;
                    await Shell.Current.DisplayAlert("Success", "Reminder updated successfully!", "OK");
                }
                else
                {
                    ErrorMessage = "Failed to update reminder. Please try again.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error updating reminder: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Error updating reminder: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }


        public async Task DeleteReminderAsync()
        {
            try
            {
                var confirmed = await Shell.Current.DisplayAlert("Delete Reminder", 
                    "Are you sure you want to delete this reminder? This action cannot be undone.", "Yes", "No");

                if (confirmed)
                {
                    IsBusy = true;
                    ErrorMessage = string.Empty;

                    var success = await _databaseService.DeleteReminderAsync(Reminder.ReminderID);
                    
                    if (success)
                    {
                        await Shell.Current.DisplayAlert("Success", "Reminder deleted successfully!", "OK");
                        await BackAsync();
                    }
                    else
                    {
                        ErrorMessage = "Failed to delete reminder. Please try again.";
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error deleting reminder: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Error deleting reminder: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task BackAsync()
        {
            try
            {
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error navigating back: {ex.Message}");
            }
        }
    }
}
