using BrainWave.APP.Models;
using BrainWave.APP.Services;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BrainWave.APP.ViewModels
{
    public class TaskDetailViewModel : BaseViewModel
    {
        private readonly DatabaseService _databaseService;
        private TaskDtos _task;
        private string _errorMessage = string.Empty;
        private bool hasPendingChanges = false;

        public TaskDetailViewModel() : this(new DatabaseService(new SupabaseService()))
        {
        }

        public TaskDetailViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));
            
            // Initialize commands
            UpdateCommand = new Command(async () => await UpdateTaskAsync());
            DeleteCommand = new Command(async () => await DeleteTaskAsync());
            MarkCompletedCommand = new Command(async () => await MarkTaskCompletedAsync());
            MarkInProgressCommand = new Command(async () => await MarkTaskInProgressAsync());
            BackCommand = new Command(async () => await GoBackAsync());
        }

        public bool HasPendingChanges
        {
            get => hasPendingChanges;
            set => Set(ref hasPendingChanges, value);
        }

        public TaskDtos Task
        {
            get => _task;
            set => Set(ref _task, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => Set(ref _errorMessage, value);
        }

        public string TaskInfo
        {
            get
            {
                if (Task == null) return string.Empty;
                
                var createdDate = Task.CreatedAt.ToString("MMM dd, yyyy");
                var updatedDate = Task.UpdatedAt.ToString("MMM dd, yyyy");
                var dueDate = Task.DueDate.ToString("MMM dd, yyyy");
                
                return $"Created: {createdDate}\nLast Updated: {updatedDate}\nDue Date: {dueDate}";
            }
        }

        public bool CanMarkCompleted => Task?.Status != "Completed";
        public bool CanMarkInProgress => Task?.Status != "In Progress";

        public ICommand UpdateCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand MarkCompletedCommand { get; }
        public ICommand MarkInProgressCommand { get; }
        public ICommand BackCommand { get; }

        public void SetTask(TaskDtos task)
        {
            Task = new TaskDtos
            {
                TaskID = task.TaskID,
                Title = task.Title,
                Description = task.Description,
                DueDate = task.DueDate,
                Priority = task.Priority,
                Status = task.Status,
                UserID = task.UserID,
                Category = task.Category,
                CreatedAt = task.CreatedAt,
                UpdatedAt = task.UpdatedAt
            };
            
            OnPropertyChanged(nameof(Task));
            OnPropertyChanged(nameof(TaskInfo));
            OnPropertyChanged(nameof(CanMarkCompleted));
            OnPropertyChanged(nameof(CanMarkInProgress));
        }

        private async Task UpdateTaskAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            ErrorMessage = string.Empty;

            try
            {
                if (string.IsNullOrWhiteSpace(Task.Title))
                {
                    ErrorMessage = "Please enter a task title.";
                    return;
                }

                var success = await _databaseService.UpdateTaskAsync(ConvertToTaskItem(Task));
                if (success)
                {
                    await Shell.Current.DisplayAlert("Success", "Task updated successfully!", "OK");
                    OnPropertyChanged(nameof(TaskInfo));
                    OnPropertyChanged(nameof(CanMarkCompleted));
                    OnPropertyChanged(nameof(CanMarkInProgress));
                }
                else
                {
                    ErrorMessage = "Failed to update task. Please try again.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to update task: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task DeleteTaskAsync()
        {
            if (IsBusy) return;

            var result = await Shell.Current.DisplayAlert("Delete Task", 
                $"Are you sure you want to delete the task '{Task.Title}'?", 
                "Delete", "Cancel");
            
            if (!result) return;

            IsBusy = true;
            ErrorMessage = string.Empty;

            try
            {
                var success = await _databaseService.DeleteTaskAsync(Task.TaskID);
                if (success)
                {
                    await Shell.Current.DisplayAlert("Success", "Task deleted successfully!", "OK");
                    await Shell.Current.GoToAsync("..");
                }
                else
                {
                    ErrorMessage = "Failed to delete task.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to delete task: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task MarkTaskCompletedAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            ErrorMessage = string.Empty;

            try
            {
                Task.Status = "Completed";
                Task.UpdatedAt = DateTime.UtcNow;
                
                var success = await _databaseService.UpdateTaskAsync(ConvertToTaskItem(Task));
                if (success)
                {
                    // Check and award badges after task completion
                    var currentUser = await _databaseService.GetCurrentUserAsync();
                    if (currentUser != null)
                    {
                        await _databaseService.CheckAndAwardBadgesAsync(currentUser.userid);
                    }
                    
                    await Shell.Current.DisplayAlert("Success", "Task marked as completed!", "OK");
                    OnPropertyChanged(nameof(CanMarkCompleted));
                    OnPropertyChanged(nameof(CanMarkInProgress));
                    OnPropertyChanged(nameof(TaskInfo));
                }
                else
                {
                    ErrorMessage = "Failed to update task status.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to update task: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task MarkTaskInProgressAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            ErrorMessage = string.Empty;

            try
            {
                Task.Status = "In Progress";
                Task.UpdatedAt = DateTime.UtcNow;
                
                var success = await _databaseService.UpdateTaskAsync(ConvertToTaskItem(Task));
                if (success)
                {
                    await Shell.Current.DisplayAlert("Success", "Task marked as in progress!", "OK");
                    OnPropertyChanged(nameof(CanMarkCompleted));
                    OnPropertyChanged(nameof(CanMarkInProgress));
                    OnPropertyChanged(nameof(TaskInfo));
                }
                else
                {
                    ErrorMessage = "Failed to update task status.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to update task: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        // Helper: convert TaskDtos to Database.TaskItem
        private Database.TaskItem ConvertToTaskItem(TaskDtos task)
        {
            // Add one day to compensate for time zone conversion issues
            var adjustedDueDate = task.DueDate.AddDays(1);
            
            return new Database.TaskItem
            {
                taskid = task.TaskID,
                title = task.Title,
                description = task.Description,
                due_date = adjustedDueDate.Kind == DateTimeKind.Local ? adjustedDueDate.ToUniversalTime() : 
                          adjustedDueDate.Kind == DateTimeKind.Unspecified ? 
                          DateTime.SpecifyKind(adjustedDueDate, DateTimeKind.Local).ToUniversalTime() : adjustedDueDate,
                task_status = task.Status,
                priority_level = task.Priority,
                userid = task.UserID
            };
        }

        public async Task GoBackAsync()
        {
            try
            {
                await Shell.Current.GoToAsync("///tasks");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to go back: {ex.Message}", "OK");
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

        public async Task UpdateTaskFieldAsync(string fieldName, string newValue)
        {
            try
            {
                // Update the specific field in the Task object (UI only)
                switch (fieldName.ToLower())
                {
                    case "title":
                        Task.Title = newValue;
                        break;
                    case "description":
                        Task.Description = newValue;
                        break;
                }

                // Trigger UI updates
                OnPropertyChanged(nameof(Task));
                OnPropertyChanged(nameof(Task.Title));
                OnPropertyChanged(nameof(Task.Description));
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to update {fieldName.ToLower()}: {ex.Message}", "OK");
            }
        }

        public async Task EditDueDateAsync()
        {
            try
            {
                var currentDate = Task.DueDate;
                var result = await Shell.Current.DisplayAlert("Edit Due Date", 
                    $"Current due date: {currentDate:MMM dd, yyyy}\n\nWould you like to change it?", 
                    "Change Date", "Cancel");
                
                if (result)
                {
                    // Use a simple prompt with better date parsing
                    var newDateString = await Shell.Current.DisplayPromptAsync("Select New Due Date", 
                        "Enter new due date (MM/DD/YYYY):", "Update", "Cancel", 
                        currentDate.ToString("MM/dd/yyyy"));
                    
                    if (!string.IsNullOrWhiteSpace(newDateString))
                    {
                        // Try multiple date formats for better compatibility
                        DateTime parsedDate;
                        bool parseSuccess = false;
                        
                        // Try MM/dd/yyyy format first
                        if (DateTime.TryParseExact(newDateString, "MM/dd/yyyy", null, System.Globalization.DateTimeStyles.None, out parsedDate))
                        {
                            parseSuccess = true;
                        }
                        // Try M/d/yyyy format (single digits)
                        else if (DateTime.TryParseExact(newDateString, "M/d/yyyy", null, System.Globalization.DateTimeStyles.None, out parsedDate))
                        {
                            parseSuccess = true;
                        }
                        // Try general parsing as fallback
                        else if (DateTime.TryParse(newDateString, out parsedDate))
                        {
                            parseSuccess = true;
                        }
                        
                        if (parseSuccess)
                        {
                            // Ensure the date is treated as local time
                            var localDate = DateTime.SpecifyKind(parsedDate, DateTimeKind.Local);
                            Task.DueDate = localDate;
                            HasPendingChanges = true;
                            OnPropertyChanged(nameof(Task));
                            OnPropertyChanged(nameof(Task.DueDate));
                            await Shell.Current.DisplayAlert("Success", "Due date updated. Click 'Confirm Update' to save changes.", "OK");
                        }
                        else
                        {
                            await Shell.Current.DisplayAlert("Error", "Invalid date format. Please use MM/DD/YYYY format.", "OK");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to update due date: {ex.Message}", "OK");
            }
        }

        public async Task EditPriorityAsync()
        {
            try
            {
                var newPriority = await Shell.Current.DisplayActionSheet("Select Priority", "Cancel", null, "Low", "Medium", "High", "Critical");
                if (newPriority != "Cancel" && newPriority != Task.Priority)
                {
                    Task.Priority = newPriority;
                    HasPendingChanges = true;
                    OnPropertyChanged(nameof(Task));
                    OnPropertyChanged(nameof(Task.Priority));
                    await Shell.Current.DisplayAlert("Success", "Priority updated. Click 'Confirm Update' to save changes.", "OK");
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to update priority: {ex.Message}", "OK");
            }
        }

        public async Task EditStatusAsync()
        {
            try
            {
                var newStatus = await Shell.Current.DisplayActionSheet("Select Status", "Cancel", null, "In Progress", "Completed");
                if (newStatus != "Cancel" && newStatus != Task.Status)
                {
                    Task.Status = newStatus;
                    HasPendingChanges = true;
                    OnPropertyChanged(nameof(Task));
                    OnPropertyChanged(nameof(Task.Status));
                    await Shell.Current.DisplayAlert("Success", "Status updated. Click 'Confirm Update' to save changes.", "OK");
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to update status: {ex.Message}", "OK");
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

                // Add one day to compensate for time zone conversion issues
                var adjustedDueDate = Task.DueDate.AddDays(1);
                
                var taskItem = new BrainWave.APP.Database.TaskItem
                {
                    taskid = Task.TaskID,
                    title = Task.Title,
                    description = Task.Description,
                    due_date = adjustedDueDate.Kind == DateTimeKind.Local ? adjustedDueDate.ToUniversalTime() : 
                              adjustedDueDate.Kind == DateTimeKind.Unspecified ? 
                              DateTime.SpecifyKind(adjustedDueDate, DateTimeKind.Local).ToUniversalTime() : adjustedDueDate,
                    priority_level = Task.Priority,
                    task_status = Task.Status,
                    userid = Task.UserID
                };
                var success = await _databaseService.UpdateTaskAsync(taskItem);
                if (success)
                {
                    HasPendingChanges = false;
                    await Shell.Current.DisplayAlert("Success", "Task updated successfully in database!", "OK");
                }
                else
                {
                    await Shell.Current.DisplayAlert("Error", "Failed to update task in database.", "OK");
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to update task: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
