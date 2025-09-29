using BrainWave.APP.Models;
using BrainWave.APP.Services;
using BrainWave.APP.Database;
using System.Collections.ObjectModel;
using BrainWave.APP.ViewModels;
using System.Windows.Input;
using System.Diagnostics;
using Microsoft.Maui.Storage;
using static BrainWave.APP.Constants;

namespace BrainWave.APP.ViewModels;
    public class RemindersViewModel : BaseViewModel
{
        private readonly DatabaseService _databaseService;
        private readonly NotificationService _notificationService;
        private readonly SupabaseService _supabaseService;
            public ObservableCollection<ReminderModel> Items { get; } = new();
        public ObservableCollection<TaskDtos> AvailableTasks { get; } = new();
        
        // Date and Time picker data
        public ObservableCollection<string> AvailableDates { get; } = new();
        public ObservableCollection<string> AvailableTimes { get; } = new();
        
        private string _selectedDateText = "";
        public string SelectedDateText 
        { 
            get => _selectedDateText; 
            set 
            { 
                _selectedDateText = value; 
                OnPropertyChanged(nameof(SelectedDateText));
                UpdateReminderDate();
            } 
        }
        
        private string _selectedTimeText = "";
        public string SelectedTimeText 
        { 
            get => _selectedTimeText; 
            set 
            { 
                _selectedTimeText = value; 
                OnPropertyChanged(nameof(SelectedTimeText));
                UpdateReminderTime();
            } 
        }
        private ReminderModel _editing = new() { ReminderTime = DateTime.Today.AddHours(9) };
        public ReminderModel Editing 
        { 
            get => _editing; 
            set 
            { 
                _editing = value; 
                OnPropertyChanged(nameof(Editing));
            } 
        }
        
        // Selected task tracking
        private TaskDtos _selectedTask;
        public TaskDtos SelectedTask 
        { 
            get => _selectedTask; 
            set 
            { 
                _selectedTask = value; 
                OnPropertyChanged(nameof(SelectedTask));
                OnPropertyChanged(nameof(IsTaskSelected));
                OnPropertyChanged(nameof(SelectedTaskDisplayText));
                OnPropertyChanged(nameof(CreateButtonText));
            } 
        }
        
        public bool IsTaskSelected => SelectedTask != null;
        public string SelectedTaskDisplayText => SelectedTask != null ? $"📋 Selected: {SelectedTask.Title}" : "📋 No task selected";
        public string CreateButtonText => IsTaskSelected ? "Review & Confirm Reminder" : "Review & Confirm Reminder";
        
        // View state management
        private bool _showRemindersList = true;
        private bool _showCreateForm = false;

        public bool ShowRemindersList
        {
            get => _showRemindersList;
            set => Set(ref _showRemindersList, value);
        }

        public bool ShowCreateForm
        {
            get => _showCreateForm;
            set => Set(ref _showCreateForm, value);
        }

        public string FormTitle => Editing.ReminderID == 0 ? "Create New Reminder" : "Edit Reminder";

        // Sorting and Search properties
        private string _sortBy = "ReminderTime";
        public string SortBy
        {
            get => _sortBy;
            set { Set(ref _sortBy, value); ApplySort(); }
        }

        private string _sortOrder = "Ascending";
        public string SortOrder
        {
            get => _sortOrder;
            set { Set(ref _sortOrder, value); ApplySort(); }
        }

        private string _searchText = "";
        public string SearchText
        {
            get => _searchText;
            set { Set(ref _searchText, value); ApplySearchAndSort(); }
        }


        private List<ReminderModel> _allReminders = new();

        // Commands
        public ICommand CreateCommand { get; }
        public ICommand UpdateCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand SelectTaskCommand { get; }
        public ICommand ConfirmReminderCommand { get; }
        public ICommand ViewCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand MarkCompletedCommand { get; }
        public ICommand ShowCreateFormCommand { get; }
        public ICommand BackToRemindersCommand { get; }
        public ICommand CancelCommand { get; }

        // Default constructor for XAML binding
        public RemindersViewModel() : this(new DatabaseService(new SupabaseService()), new NotificationService(), new SupabaseService())
        {
        }

        public RemindersViewModel(DatabaseService databaseService) : this(databaseService, new NotificationService(), new SupabaseService())
        {
        }

        public RemindersViewModel(DatabaseService databaseService, NotificationService notificationService) : this(databaseService, notificationService, new SupabaseService())
        {
        }

        public RemindersViewModel(DatabaseService databaseService, NotificationService notificationService, SupabaseService supabaseService) : base()
        {
            _databaseService = databaseService;
            _notificationService = notificationService;
            _supabaseService = supabaseService;
            
            // Initialize commands
            CreateCommand = new Command(async () => await CreateAsync());
            UpdateCommand = new Command(async () => await UpdateAsync());
            RefreshCommand = new Command(async () => await LoadAsync());
            LogoutCommand = new Command(async () => await LogoutAsync());
            SelectTaskCommand = new Command<TaskDtos>(async (task) => await SelectTaskForReminderAsync(task));
            ConfirmReminderCommand = new Command(async () => await CreateAsync());
            ViewCommand = new Command<ReminderModel>(async (reminder) => await ViewReminderAsync(reminder));
            EditCommand = new Command<ReminderModel>(async (reminder) => await EditReminderAsync(reminder));
            DeleteCommand = new Command<ReminderModel>(async (reminder) => await DeleteReminderAsync(reminder));
            MarkCompletedCommand = new Command<ReminderModel>(async (reminder) => await MarkCompletedAsync(reminder));
            ShowCreateFormCommand = new Command(async () => await NavigateToCreateReminderAsync());
            BackToRemindersCommand = new Command(() => ShowRemindersListView());
            CancelCommand = new Command(async () => await CancelAsync());
            
            // Initialize date and time data
            InitializeDateAndTimeData();
        }

    public async Task LoadAsync()
    {
            if (IsBusy) return;
            IsBusy = true;
            
            try
            {
                // First ensure Supabase is initialized
                await _supabaseService.InitializeAsync();
                
                // Try to get user ID from stored session first (same pattern as ProfileViewModel)
                var storedUserId = await SecureStorage.GetAsync(Constants.SECURE_KEY_USER_ID);
                User? currentUser = null;
                
                if (!string.IsNullOrEmpty(storedUserId) && int.TryParse(storedUserId, out int userId))
                {
                    // Get user by ID directly
                    currentUser = await _databaseService.GetUserByIdAsync(userId);
                }
                
                // Fallback to GetCurrentUserAsync if stored user ID approach failed
                if (currentUser == null)
                {
                    currentUser = await _databaseService.GetCurrentUserAsync();
                }
                
                if (currentUser != null)
                {
                    // First check if user has any tasks
                    var userTasks = await _databaseService.GetTasksByUserIdAsync(currentUser.userid);
                    
                    var reminders = await _databaseService.GetRemindersByUserIdAsync(currentUser.userid);
                    
                    // Get all tasks for the user in one query for efficiency
                    var taskLookup = userTasks.ToDictionary(t => t.taskid, t => t);
                    
                    // Convert reminders with task details
                    _allReminders = new List<ReminderModel>();
                    foreach (var reminder in reminders)
                    {
                        var reminderModel = ConvertToReminderModel(reminder, taskLookup);
                        _allReminders.Add(reminderModel);
                    }
                }
                
                ApplySearchAndSort();
                
                // Load available tasks for reminder creation
                await LoadAvailableTasksAsync();
                
                // Load available tasks for reminder creation
                AvailableTasks.Clear();
                if (currentUser != null)
                {
                    var tasks = await _databaseService.GetTasksByUserIdAsync(currentUser.userid);
                    foreach (var task in tasks)
                    {
                        AvailableTasks.Add(new TaskDtos
                        {
                            TaskID = task.taskid,
                            Title = task.title,
                            Description = task.description,
                            DueDate = task.due_date.Kind == DateTimeKind.Utc ? task.due_date.ToLocalTime() : 
                                     task.due_date.Kind == DateTimeKind.Unspecified ? 
                                     DateTime.SpecifyKind(task.due_date, DateTimeKind.Local) : task.due_date,
                            Status = task.task_status,
                            Priority = task.priority_level,
                            UserID = task.userid,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle error silently
            }
            finally
            {
                IsBusy = false;
            }
        }

                public async Task SelectTaskForReminderAsync(TaskDtos task)
        {
            SelectedTask = task;
            
            var newEditing = new ReminderModel
            {
                Title = $"Reminder: {task.Title}",
                Description = $"Don't forget to work on: {task.Title}",
                ReminderTime = task.DueDate.AddHours(-1), // 1 hour before due date
                UserID = 1
            };
            Editing = newEditing;
            
            // Show professional confirmation
            await Shell.Current.DisplayAlert("✅ Task Selected", 
                $"Task '{task.Title}' has been selected for reminder.\n\n" +
                $"Reminder will be set for: {Editing.ReminderTime:MMM dd, yyyy 'at' h:mm tt}\n\n" +
                $"You can modify the date, time, and details below before confirming the reminder.", 
                "Continue");
        }

        public async Task ShowReminderConfirmationAsync()
        {
            if (SelectedTask == null)
            {
                await Shell.Current.DisplayAlert("⚠️ Missing Information", "Please select a task for the reminder.", "OK");
                return;
            }

            if (Editing.ReminderTime <= DateTime.Now)
            {
                await Shell.Current.DisplayAlert("⚠️ Invalid Time", "Please select a future date and time for the reminder.", "OK");
                return;
            }

            var confirmationMessage = $"Please confirm your reminder details:\n\n" +
                                    $"📋 Task: {SelectedTask.Title}\n" +
                                    $"📝 Description: {SelectedTask.Description ?? "None"}\n" +
                                    $"⏰ Date & Time: {Editing.ReminderTime:MMMM dd, yyyy 'at' h:mm tt}\n" +
                                    $"📧 Type: Email\n\n" +
                                    $"This reminder will be created and scheduled.";

            var result = await Shell.Current.DisplayAlert("🔔 Confirm Reminder", confirmationMessage, "✅ Create Reminder", "✏️ Edit Details");
            
            if (result)
            {
                await CreateAsync();
            }
        }

            public async Task CreateAsync()
        {
            if (IsBusy)
            {
                return;
            }
            IsBusy = true;
            
            try
            {
                // Validate that a task is selected
                if (SelectedTask == null)
                {
                    await Shell.Current.DisplayAlert("Error", "Please select a task for the reminder.", "OK");
                    return;
                }
                
                // Check for duplicate reminders
                if (await CheckForDuplicateReminder())
                {
                    return;
                }
                
                // First ensure Supabase is initialized
                await _supabaseService.InitializeAsync();
                
                // Try to get user ID from stored session first (same pattern as ProfileViewModel)
                var storedUserId = await SecureStorage.GetAsync(Constants.SECURE_KEY_USER_ID);
                User? currentUser = null;
                
                if (!string.IsNullOrEmpty(storedUserId) && int.TryParse(storedUserId, out int userId))
                {
                    // Get user by ID directly
                    currentUser = await _databaseService.GetUserByIdAsync(userId);
                }
                
                // Fallback to GetCurrentUserAsync if stored user ID approach failed
                if (currentUser == null)
                {
                    currentUser = await _databaseService.GetCurrentUserAsync();
                }
                
                if (currentUser == null)
                {
                    await Shell.Current.DisplayAlert("Error", "User not found. Please log in again.", "OK");
                    return;
                }
                
                Editing.UserID = currentUser.userid;
                Editing.TaskID = SelectedTask.TaskID; // Set the task ID
                Editing.CreatedAt = DateTime.UtcNow;
                Editing.UpdatedAt = DateTime.UtcNow;

                var dbReminder = ConvertToReminder(Editing);
                var success = await _databaseService.CreateReminderAsync(dbReminder);
                if (success) 
                { 
                    // Schedule email reminder using SendGrid with sendAt property
                    await _notificationService.ScheduleReminderNotificationAsync(
                        SelectedTask.Title, // Use task title
                        SelectedTask.Description ?? "", // Use task description
                        Editing.ReminderTime,
                        currentUser.email,
                        SelectedTask.Title,
                        SelectedTask.DueDate);
                    
                    await LoadAsync();
                    await Shell.Current.DisplayAlert("🎉 Reminder Created Successfully!", 
                        $"Your reminder has been created and stored!\n\n" +
                        $"📋 Task: {SelectedTask.Title}\n" +
                        $"⏰ Date & Time: {Editing.ReminderTime:MMMM dd, yyyy 'at' h:mm tt}\n" +
                        $"📧 Email: {currentUser.email}\n\n" +
                        $"📧 An email notification will be sent to you at the reminder time using SendGrid's scheduled delivery.", 
                        "Done");
                    await Shell.Current.GoToAsync("///reminders"); // Navigate back to reminders page
                }
                else
                {
                    await Shell.Current.DisplayAlert("❌ Error", "Failed to create reminder. Please try again.", "OK");
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("❌ Error", $"Failed to create reminder: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
                        }
        }

        private void InitializeDateAndTimeData()
        {
            // Initialize available dates (next 30 days)
            AvailableDates.Clear();
            for (int i = 0; i < 30; i++)
            {
                var date = DateTime.Today.AddDays(i);
                AvailableDates.Add(date.ToString("MMM dd, yyyy"));
            }

            // Initialize available times (every 30 minutes from 6 AM to 10 PM)
            AvailableTimes.Clear();
            for (int hour = 6; hour <= 22; hour++)
            {
                for (int minute = 0; minute < 60; minute += 30)
                {
                    var time = new TimeSpan(hour, minute, 0);
                    var timeString = time.ToString(@"hh\:mm");
                    var ampm = hour < 12 ? "AM" : "PM";
                    var displayHour = hour > 12 ? hour - 12 : (hour == 0 ? 12 : hour);
                    AvailableTimes.Add($"{displayHour}:{minute:D2} {ampm}");
                }
            }

            // Set default values
            SelectedDateText = AvailableDates.FirstOrDefault() ?? "";
            SelectedTimeText = AvailableTimes.FirstOrDefault() ?? "";
        }

        private void UpdateReminderDate()
        {
            if (string.IsNullOrEmpty(SelectedDateText)) return;

            try
            {
                // Parse the selected date text (e.g., "Mar 15, 2024")
                if (DateTime.TryParse(SelectedDateText, out DateTime selectedDate))
                {
                    var currentTime = Editing.ReminderTime.TimeOfDay;
                    Editing.ReminderTime = selectedDate.Date + currentTime;
                }
            }
            catch (Exception ex)
            {
                // Handle error silently
            }
        }

        private void UpdateReminderTime()
        {
            if (string.IsNullOrEmpty(SelectedTimeText)) return;

            try
            {
                // Parse the selected time text (e.g., "2:30 PM")
                var timeParts = SelectedTimeText.Split(' ');
                if (timeParts.Length == 2)
                {
                    var timeValue = timeParts[0];
                    var ampm = timeParts[1];

                    var hourMinute = timeValue.Split(':');
                    if (hourMinute.Length == 2)
                    {
                        var hour = int.Parse(hourMinute[0]);
                        var minute = int.Parse(hourMinute[1]);

                        // Convert to 24-hour format
                        if (ampm == "PM" && hour != 12)
                            hour += 12;
                        else if (ampm == "AM" && hour == 12)
                            hour = 0;

                        var timeSpan = new TimeSpan(hour, minute, 0);
                        var currentDate = Editing.ReminderTime.Date;
                        Editing.ReminderTime = currentDate + timeSpan;
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle error silently
            }
        }

        public async Task ViewReminderAsync(ReminderModel reminder)
        {
            if (IsBusy) return;
            
            try
            {
                // Show popup instead of navigating to new page
                var remindersPage = Shell.Current.CurrentPage as Views.RemindersPage;
                if (remindersPage != null)
                {
                    remindersPage.ShowReminderPopup(reminder);
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to open reminder details: {ex.Message}", "OK");
            }
        }

        public async Task EditReminderAsync(ReminderModel reminder)
        {
            if (IsBusy) return;
            
            try
            {
                // Load the reminder into the editing form
                Editing = new ReminderModel
                {
                    ReminderID = reminder.ReminderID,
                    Title = reminder.Title,
                    Description = reminder.Description,
                    ReminderTime = reminder.ReminderTime,
                    UserID = reminder.UserID,
                    CreatedAt = reminder.CreatedAt,
                    UpdatedAt = reminder.UpdatedAt
                };
                
                // Update the selected date and time text
                SelectedDateText = reminder.ReminderTime.ToString("MMM dd, yyyy");
                SelectedTimeText = reminder.ReminderTime.ToString("h:mm tt");
                
                OnPropertyChanged(nameof(Editing));
                ShowCreateFormView();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to load reminder for editing: {ex.Message}", "OK");
            }
        }

        public async Task UpdateReminderAsync(ReminderModel reminder)
        {
            if (IsBusy) return;
            IsBusy = true;
            
            try
            {
                reminder.UpdatedAt = DateTime.UtcNow;
                var success = await _databaseService.UpdateReminderAsync(ConvertToReminder(reminder));
                if (success)
                {
                    // Get current user for email scheduling
                    var currentUser = await _databaseService.GetCurrentUserAsync();
                    if (currentUser != null)
                    {
                        // Get the task title for the reminder
                        var task = await _databaseService.GetTaskByIdAsync(reminder.TaskID);
                        var taskTitle = task?.title ?? "Unknown Task";
                        
                        // Schedule updated email reminder
                        await _notificationService.ScheduleReminderNotificationAsync(
                            reminder.Title, 
                            reminder.Description ?? "", 
                            reminder.ReminderTime,
                            currentUser.email,
                            taskTitle,
                            task?.due_date);
                    }
                    
                    await LoadAsync();
                    await Shell.Current.DisplayAlert("Success", "Reminder updated successfully! Email notification has been rescheduled.", "OK");
                }
                else
                {
                    await Shell.Current.DisplayAlert("Error", "Failed to update reminder. Please try again.", "OK");
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to update reminder: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task DeleteReminderAsync(ReminderModel reminder)
        {
            if (IsBusy) return;
            
            IsBusy = true;
            try
            {
                if (await _databaseService.DeleteReminderAsync(reminder.ReminderID))
                {
                    await LoadAsync();
                    await Shell.Current.DisplayAlert("Success", "Reminder deleted successfully!", "OK");
                }
                else
                {
                    await Shell.Current.DisplayAlert("Error", "Failed to delete reminder.", "OK");
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to delete reminder: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void ShowCreateFormView()
        {
            ShowRemindersList = false;
            ShowCreateForm = true;
        }

        private void ShowRemindersListView()
        {
            ShowRemindersList = true;
            ShowCreateForm = false;
            // Reset form when going back to reminders list
            Editing = new ReminderModel { ReminderTime = DateTime.Today.AddHours(9) };
            SelectedTask = null;
            OnPropertyChanged(nameof(Editing));
            OnPropertyChanged(nameof(FormTitle));
        }

        public async Task MarkCompletedAsync(ReminderModel reminder)
        {
            if (IsBusy) return;
            IsBusy = true;

            try
            {
                reminder.IsCompleted = true;
                reminder.UpdatedAt = DateTime.UtcNow;
                
                var success = await _databaseService.UpdateReminderAsync(ConvertToReminder(reminder));
                if (success)
                {
                    await LoadAsync();
                    await Shell.Current.DisplayAlert("Success", $"Reminder '{reminder.Title}' marked as completed!", "OK");
                }
                else
                {
                    await Shell.Current.DisplayAlert("Error", "Failed to update reminder status.", "OK");
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to update reminder: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task UpdateAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            
            try
            {
                if (Editing.ReminderID == 0)
                {
                    await Shell.Current.DisplayAlert("Error", "Please select a reminder to update first.", "OK");
                    return;
                }
                
                
                Editing.UpdatedAt = DateTime.UtcNow;
                var success = await _databaseService.UpdateReminderAsync(ConvertToReminder(Editing));
                if (success)
                {
                    // Get current user for email scheduling
                    var currentUser = await _databaseService.GetCurrentUserAsync();
                    if (currentUser != null && SelectedTask != null)
                    {
                        // Schedule updated email reminder
                        await _notificationService.ScheduleReminderNotificationAsync(
                            SelectedTask.Title, // Use task title
                            SelectedTask.Description ?? "", // Use task description
                            Editing.ReminderTime,
                            currentUser.email,
                            SelectedTask.Title,
                            SelectedTask.DueDate);
                    }
                    
                    await LoadAsync();
                    await Shell.Current.DisplayAlert("Success", "Reminder updated successfully! Email notification has been rescheduled.", "OK");
                    ShowRemindersListView(); // Go back to reminders list
                }
                else
                {
                    await Shell.Current.DisplayAlert("Error", "Failed to update reminder. Please try again.", "OK");
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to update reminder: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void ApplySort()
        {
            ApplySearchAndSort();
        }

        private void ApplySearchAndSort()
        {
            IEnumerable<ReminderModel> q = _allReminders ?? Enumerable.Empty<ReminderModel>();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                q = q.Where(r => 
                    r.Title.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    (r.Description != null && r.Description.Contains(SearchText, StringComparison.OrdinalIgnoreCase)));
            }

            // Apply sorting
            q = SortBy switch
            {
                "Title" => SortOrder == "Ascending" ? q.OrderBy(r => r.Title) : q.OrderByDescending(r => r.Title),
                "ReminderTime" => SortOrder == "Ascending" ? q.OrderBy(r => r.ReminderTime) : q.OrderByDescending(r => r.ReminderTime),
                _ => SortOrder == "Ascending" ? q.OrderBy(r => r.ReminderTime) : q.OrderByDescending(r => r.ReminderTime)
            };

            Items.Clear();
            foreach (var r in q) 
            { 
                Items.Add(r);
            }
        }

        // Helper: convert Database.Reminder to ReminderModel (simple version like tasks)
        private ReminderModel ConvertToReminderModel(Database.Reminder reminder)
        {
            // Convert UTC time from database back to local time for display
            var localTime = reminder.notify_time;
            if (reminder.notify_time.Kind == DateTimeKind.Utc)
            {
                localTime = reminder.notify_time.ToLocalTime();
            }
            else if (reminder.notify_time.Kind == DateTimeKind.Unspecified)
            {
                // If stored as Unspecified, assume it's UTC and convert to local
                localTime = DateTime.SpecifyKind(reminder.notify_time, DateTimeKind.Utc).ToLocalTime();
            }
            
            return new ReminderModel
            {
                ReminderID = reminder.reminderid,
                Title = $"Reminder for Task {reminder.taskid}", // Will be updated with task title in the other method
                Description = $"Task reminder for task ID {reminder.taskid}",
                ReminderTime = localTime,
                IsCompleted = false, // Default value
                UserID = 0, // Will be set properly when we have the task info
                TaskID = reminder.taskid,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
        }

        // Helper: convert Database.Reminder to ReminderModel (async version for when we need task details)
        private async Task<ReminderModel> ConvertToReminderModelAsync(Database.Reminder reminder)
        {
            try
            {
                // Get the associated task to get title and description
                var task = await _databaseService.GetTaskByIdAsync(reminder.taskid);
                
                if (task == null)
                {
                    // Task not found, use default values
                }
                else
                {
                    // Task found
                }
                
                // Convert UTC time from database back to local time for display
                var localTime = reminder.notify_time;
                if (reminder.notify_time.Kind == DateTimeKind.Utc)
                {
                    localTime = reminder.notify_time.ToLocalTime();
                }
                else if (reminder.notify_time.Kind == DateTimeKind.Unspecified)
                {
                    // If stored as Unspecified, assume it's UTC and convert to local
                    localTime = DateTime.SpecifyKind(reminder.notify_time, DateTimeKind.Utc).ToLocalTime();
                }
                
                
                var reminderModel = new ReminderModel
                {
                    ReminderID = reminder.reminderid,
                    Title = task?.title ?? "Reminder", // Use task title
                    Description = task?.description ?? "Task reminder", // Use task description
                    ReminderTime = localTime, // Convert UTC to local time
                    IsCompleted = false, // Default value
                    UserID = task?.userid ?? 0,
                    TaskID = reminder.taskid,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                
                return reminderModel;
            }
            catch (Exception ex)
            {
                return new ReminderModel
                {
                    ReminderID = reminder.reminderid,
                    Title = "Reminder",
                    Description = "Task reminder",
                    ReminderTime = reminder.notify_time, // Use as-is without conversion
                    IsCompleted = false,
                    UserID = 0,
                    TaskID = reminder.taskid,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
            }
        }

        public async Task RefreshAsync()
        {
            await LoadAsync();
        }

        private ReminderModel ConvertToReminderModel(Database.Reminder reminder, Dictionary<int, Database.TaskItem> taskLookup)
        {
            try
            {
                
                // Get the associated task from the lookup dictionary
                var task = taskLookup.GetValueOrDefault(reminder.taskid);
                
                if (task == null)
                {
                    // Task not found, use default values
                }
                else
                {
                    // Task found
                }
                
                // Convert UTC time from database back to local time for display
                var localTime = reminder.notify_time;
                if (reminder.notify_time.Kind == DateTimeKind.Utc)
                {
                    localTime = reminder.notify_time.ToLocalTime();
                }
                else if (reminder.notify_time.Kind == DateTimeKind.Unspecified)
                {
                    // If stored as Unspecified, assume it's UTC and convert to local
                    localTime = DateTime.SpecifyKind(reminder.notify_time, DateTimeKind.Utc).ToLocalTime();
                }
                
                
                var reminderModel = new ReminderModel
                {
                    ReminderID = reminder.reminderid,
                    Title = task?.title ?? "Reminder", // Use task title
                    Description = task?.description ?? "Task reminder", // Use task description
                    ReminderTime = localTime, // Convert UTC to local time
                    IsCompleted = false, // Default value
                    UserID = task?.userid ?? 0,
                    TaskID = reminder.taskid,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                
                return reminderModel;
            }
            catch (Exception ex)
            {
                return new ReminderModel
                {
                    ReminderID = reminder.reminderid,
                    Title = "Reminder",
                    Description = "Task reminder",
                    ReminderTime = reminder.notify_time, // Use as-is without conversion
                    IsCompleted = false,
                    UserID = 0,
                    TaskID = reminder.taskid,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
            }
        }

        // Helper: convert ReminderModel to Database.Reminder
        private Database.Reminder ConvertToReminder(ReminderModel reminder)
        {
            
            // Ensure the time is stored exactly as the user selected it
            // Convert to UTC to avoid time zone issues when storing
            var utcTime = DateTime.SpecifyKind(reminder.ReminderTime, DateTimeKind.Utc);
            if (reminder.ReminderTime.Kind == DateTimeKind.Local)
            {
                utcTime = reminder.ReminderTime.ToUniversalTime();
            }
            else if (reminder.ReminderTime.Kind == DateTimeKind.Unspecified)
            {
                // If unspecified, assume it's local time and convert to UTC
                utcTime = DateTime.SpecifyKind(reminder.ReminderTime, DateTimeKind.Local).ToUniversalTime();
            }
            
            var dbReminder = new Database.Reminder
            {
                taskid = reminder.TaskID, // Use the reminder's task ID
                reminder_type = "Email",
                notify_time = utcTime // Store as UTC
            };
            
            
            // Only set reminderid if it's a valid ID (for updates), otherwise let database auto-generate
            if (reminder.ReminderID > 0)
            {
                dbReminder.reminderid = reminder.ReminderID;
            }
            
            return dbReminder;
        }

        // Check for duplicate reminders
        private async Task<bool> CheckForDuplicateReminder()
        {
            try
            {
                if (SelectedTask == null) return false;
                
                // Get existing reminders for this task
                var existingReminders = await _databaseService.GetRemindersByUserIdAsync(Editing.UserID);
                var taskReminders = existingReminders.Where(r => r.taskid == SelectedTask.TaskID).ToList();
                
                
                // Check if there's already a reminder with the same time (within 1 minute tolerance)
                var duplicateReminder = taskReminders.FirstOrDefault(r => 
                    Math.Abs((r.notify_time - Editing.ReminderTime).TotalMinutes) < 1);
                
                if (duplicateReminder != null)
                {
                    await Shell.Current.DisplayAlert("⚠️ Duplicate Reminder", 
                        $"A reminder for this task already exists at {duplicateReminder.notify_time:MMMM dd, yyyy 'at' h:mm tt}. Please choose a different time.", 
                        "OK");
                    return true;
                }
                
                return false;
            }
            catch (Exception ex)
            {
                return false; // Allow creation if check fails
            }
        }

        public SupabaseService? GetSupabaseService()
        {
            return _databaseService.GetSupabaseService();
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

        public async Task CancelAsync()
        {
            try
            {
                await Shell.Current.GoToAsync("///reminders");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to navigate back: {ex.Message}", "OK");
            }
        }

        public async Task NavigateToCreateReminderAsync()
        {
            try
            {
                // Reset the editing reminder to default values
                Editing = new ReminderModel
                {
                    Title = "",
                    Description = "",
                    ReminderTime = DateTime.Today.AddHours(9),
                    IsCompleted = false,
                    UserID = 0,
                    TaskID = SelectedTask?.TaskID ?? 0,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                
                // Load available tasks
                await LoadAvailableTasksAsync();
                
                await Shell.Current.GoToAsync("CreateReminderPage");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to navigate to create reminder page: {ex.Message}", "OK");
            }
        }

        private async Task LoadAvailableTasksAsync()
        {
            try
            {
                // First ensure Supabase is initialized
                await _supabaseService.InitializeAsync();
                
                // Try to get user ID from stored session first (same pattern as ProfileViewModel)
                var storedUserId = await SecureStorage.GetAsync(Constants.SECURE_KEY_USER_ID);
                User? currentUser = null;
                
                if (!string.IsNullOrEmpty(storedUserId) && int.TryParse(storedUserId, out int userId))
                {
                    // Get user by ID directly
                    currentUser = await _databaseService.GetUserByIdAsync(userId);
                }
                
                // Fallback to GetCurrentUserAsync if stored user ID approach failed
                if (currentUser == null)
                {
                    currentUser = await _databaseService.GetCurrentUserAsync();
                }
                
                if (currentUser != null)
                {
                    var tasks = await _databaseService.GetTasksByUserIdAsync(currentUser.userid);
                    AvailableTasks.Clear();
                    foreach (var task in tasks)
                    {
                        AvailableTasks.Add(new TaskDtos
                        {
                            TaskID = task.taskid,
                            Title = task.title,
                            Description = task.description,
                            DueDate = task.due_date.Kind == DateTimeKind.Utc ? task.due_date.ToLocalTime() : 
                                     task.due_date.Kind == DateTimeKind.Unspecified ? 
                                     DateTime.SpecifyKind(task.due_date, DateTimeKind.Local) : task.due_date,
                            Status = task.task_status,
                            Priority = task.priority_level,
                            UserID = task.userid,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle error silently
            }
        }
    }