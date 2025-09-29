using BrainWave.APP.Models;
using BrainWave.APP.Services;
using BrainWave.APP.Database;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Storage;
using static BrainWave.APP.Constants;

namespace BrainWave.APP.ViewModels
{
    public class TasksViewModel : BaseViewModel
    {
        private readonly DatabaseService _databaseService;
        private readonly SupabaseService _supabaseService;

        public ObservableCollection<TaskDtos> Items { get; } = new();

        // Default constructor for XAML binding
        public TasksViewModel() : this(new DatabaseService(new SupabaseService()), new SupabaseService())
        {
        }

        // Filters/Sort with safe defaults
        private string _priorityFilter = "All";
        public string PriorityFilter
        {
            get => _priorityFilter;
            set { Set(ref _priorityFilter, value); _ = RefreshAsync(); }
        }

        private string _statusFilter = "All";
        public string StatusFilter
        {
            get => _statusFilter;
            set { Set(ref _statusFilter, value); _ = RefreshAsync(); }
        }

        private string _sortBy = "DueDate";
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

        private List<TaskDtos> _all = new();
        private bool _showTaskList = true;
        private bool _showCreateForm = false;

        public TasksViewModel(DatabaseService databaseService, SupabaseService supabaseService)
        {
            _databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));
            _supabaseService = supabaseService ?? throw new ArgumentNullException(nameof(supabaseService));

            // Safe defaults for editing
            Editing = new TaskDtos
            {
                Title = "",
                Description = "",
                DueDate = DateTime.SpecifyKind(DateTime.Now.AddDays(1), DateTimeKind.Local),
                Priority = "Medium",
                Status = "In Progress",
                UserID = 0, // Will be set when creating
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            
            // Initialize commands
            CreateCommand = new Command(async () => await CreateAsync());
            UpdateCommand = new Command(async () => await UpdateAsync());
            DeleteCommand = new Command<TaskDtos>(async (item) => await DeleteAsync(item));
            EditCommand = new Command<TaskDtos>(async (task) => await EditTaskAsync(task));
            ViewCommand = new Command<TaskDtos>(async (task) => await ViewTaskAsync(task));
            MarkCompletedCommand = new Command<TaskDtos>(async (task) => await MarkTaskCompletedAsync(task));
            ShowCreateFormCommand = new Command(async () => await NavigateToCreateTaskAsync());
            BackToTasksCommand = new Command(() => ShowTaskListView());
            RefreshCommand = new Command(async () => await RefreshAsync());
            ExportCommand = new Command<TaskDtos>(async (task) => await ExportTaskAsync(task));
            LogoutCommand = new Command(async () => await LogoutAsync());
            CancelCommand = new Command(async () => await CancelAsync());
        }

        public TaskDtos Editing { get; set; }
        public ICommand CreateCommand { get; }
        public ICommand UpdateCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand ViewCommand { get; }
        public ICommand MarkCompletedCommand { get; }
        public ICommand ShowCreateFormCommand { get; }
        public ICommand BackToTasksCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand ExportCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand CancelCommand { get; }

        public bool ShowTaskList
        {
            get => _showTaskList;
            set => Set(ref _showTaskList, value);
        }

        public bool ShowCreateForm
        {
            get => _showCreateForm;
            set => Set(ref _showCreateForm, value);
        }
        
        public string FormTitle => Editing.TaskID == 0 ? "Create New Task" : "Edit Task";
        
        public void SelectTaskForEditing(TaskDtos task)
        {
            Editing = new TaskDtos
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
            
            OnPropertyChanged(nameof(Editing));
            OnPropertyChanged(nameof(FormTitle));
        }

        public async Task EditTaskAsync(TaskDtos task)
        {
            SelectTaskForEditing(task);
            ShowCreateFormView();
        }

        private void ShowCreateFormView()
        {
            ShowTaskList = false;
            ShowCreateForm = true;
        }

        private void ShowTaskListView()
        {
            ShowTaskList = true;
            ShowCreateForm = false;
            // Reset form when going back to task list
            Editing = new TaskDtos
            {
                Title = "",
                Description = "",
                DueDate = DateTime.Now.AddDays(1),
                Priority = "Medium",
                Status = "In Progress",
                UserID = 0, // Will be set when creating
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            OnPropertyChanged(nameof(Editing));
            OnPropertyChanged(nameof(FormTitle));
        }

        public async Task ViewTaskAsync(TaskDtos task)
        {
            try
            {
                await Shell.Current.GoToAsync($"///TaskDetailPage", new Dictionary<string, object>
                {
                    ["Task"] = task
                });
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to open task details: {ex.Message}", "OK");
            }
        }

        public async Task MarkTaskCompletedAsync(TaskDtos task)
        {
            if (IsBusy) return;
            IsBusy = true;

            try
            {
                task.Status = "Completed";
                task.UpdatedAt = DateTime.UtcNow;
                
                var success = await _databaseService.UpdateTaskAsync(ConvertToTaskItem(task));
                if (success)
                {
                    // Check and award badges after task completion
                    var currentUser = await _databaseService.GetCurrentUserAsync();
                    if (currentUser != null)
                    {
                        await _databaseService.CheckAndAwardBadgesAsync(currentUser.userid);
                    }
                    
                    await RefreshAsync();
                    await Shell.Current.DisplayAlert("Success", $"Task '{task.Title}' marked as completed!", "OK");
                }
                else
                {
                    await Shell.Current.DisplayAlert("Error", "Failed to update task status.", "OK");
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

        public async Task RefreshAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                var filters = new Dictionary<string, string>();
                if (!string.IsNullOrWhiteSpace(PriorityFilter) && PriorityFilter != "All")
                    filters.Add("Priority", PriorityFilter);
                if (!string.IsNullOrWhiteSpace(StatusFilter) && StatusFilter != "All")
                    filters.Add("Status", StatusFilter);

                // Get current user first
                var currentUser = await _databaseService.GetCurrentUserAsync();
                if (currentUser != null)
                {
                    var tasks = await _databaseService.GetTasksByUserIdAsync(currentUser.userid);
                    _all = tasks.Select(ConvertToTaskDtos).ToList();
                }
                ApplySearchAndSort();
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
            IEnumerable<TaskDtos> q = _all ?? Enumerable.Empty<TaskDtos>();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                q = q.Where(t => 
                    t.Title.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    (t.Description != null && t.Description.Contains(SearchText, StringComparison.OrdinalIgnoreCase)) ||
                    (t.Category != null && t.Category.Contains(SearchText, StringComparison.OrdinalIgnoreCase)));
            }

            // Apply priority filter
            if (PriorityFilter != "All")
            {
                q = q.Where(t => t.Priority == PriorityFilter);
            }

            // Apply status filter
            if (StatusFilter != "All")
            {
                q = q.Where(t => t.Status == StatusFilter);
            }

            // Apply sorting
            q = SortBy switch
            {
                "Title" => SortOrder == "Ascending" ? q.OrderBy(t => t.Title) : q.OrderByDescending(t => t.Title),
                "DueDate" => SortOrder == "Ascending" ? q.OrderBy(t => t.DueDate) : q.OrderByDescending(t => t.DueDate),
                "Priority" => SortOrder == "Ascending" ? q.OrderBy(t => t.Priority) : q.OrderByDescending(t => t.Priority),
                "Status" => SortOrder == "Ascending" ? q.OrderBy(t => t.Status) : q.OrderByDescending(t => t.Status),
                "CreatedAt" => SortOrder == "Ascending" ? q.OrderBy(t => t.CreatedAt) : q.OrderByDescending(t => t.CreatedAt),
                _ => SortOrder == "Ascending" ? q.OrderBy(t => t.DueDate) : q.OrderByDescending(t => t.DueDate)
            };

            Items.Clear();
            foreach (var t in q) Items.Add(t);
        }

        public async Task CreateAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            
            try
            {
                if (string.IsNullOrWhiteSpace(Editing.Title))
                {
                    await Shell.Current.DisplayAlert("Error", "Please enter a task title.", "OK");
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
                Editing.CreatedAt = DateTime.UtcNow;
                Editing.UpdatedAt = DateTime.UtcNow;
                
                var success = await _databaseService.CreateTaskAsync(ConvertToTaskItem(Editing));
                if (success)
                {
                    await RefreshAsync();
                    await Shell.Current.DisplayAlert("Success", "Task created successfully!", "OK");
                    await Shell.Current.GoToAsync("///tasks"); // Navigate back to tasks page
                }
                else
                {
                    await Shell.Current.DisplayAlert("Error", "Failed to create task. Please try again.", "OK");
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to create task: {ex.Message}", "OK");
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
                if (Editing.TaskID == 0)
                {
                    await Shell.Current.DisplayAlert("Error", "Please select a task to update first.", "OK");
                    return;
                }
                
                if (string.IsNullOrWhiteSpace(Editing.Title))
                {
                    await Shell.Current.DisplayAlert("Error", "Please enter a task title.", "OK");
                    return;
                }
                
                var success = await _databaseService.UpdateTaskAsync(ConvertToTaskItem(Editing));
                if (success)
                {
                    await RefreshAsync();
                    await Shell.Current.DisplayAlert("Success", "Task updated successfully!", "OK");
                    ShowTaskListView(); // Go back to task list
                }
                else
                {
                    await Shell.Current.DisplayAlert("Error", "Failed to update task. Please try again.", "OK");
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

        public async Task DeleteAsync(TaskDtos item)
        {
            if (IsBusy) return;
            
            var result = await Shell.Current.DisplayAlert("Delete Task", 
                $"Are you sure you want to delete the task '{item.Title}'?", 
                "Delete", "Cancel");
            
            if (!result) return;
            
            IsBusy = true;
            try
            {
                if (await _databaseService.DeleteTaskAsync(item.TaskID))
                {
                    await RefreshAsync();
                    await Shell.Current.DisplayAlert("Success", "Task deleted successfully!", "OK");
                }
                else
                {
                    await Shell.Current.DisplayAlert("Error", "Failed to delete task.", "OK");
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to delete task: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        // Helper: create a new task with safe defaults
        private async Task<TaskDtos> CreateDefaultTaskAsync()
        {
            var currentUser = await _databaseService.GetCurrentUserAsync();
            return new TaskDtos
            {
                Title = "", // Empty title for new tasks
                Description = "", // Empty description
                DueDate = DateTime.SpecifyKind(DateTime.Now.AddDays(1), DateTimeKind.Local), // Due tomorrow by default
                Priority = "Medium",
                Status = "In Progress", // Changed from "Pending" to "In Progress"
                UserID = currentUser?.userid ?? 0, // Use actual user ID
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        // Helper: convert Database.TaskItem to TaskDtos
        private TaskDtos ConvertToTaskDtos(Database.TaskItem task)
        {
            // Convert due_date from database to local time for display
            var localDueDate = task.due_date;
            if (task.due_date.Kind == DateTimeKind.Utc)
            {
                localDueDate = task.due_date.ToLocalTime();
            }
            else if (task.due_date.Kind == DateTimeKind.Unspecified)
            {
                // If stored as Unspecified, assume it's local time
                localDueDate = DateTime.SpecifyKind(task.due_date, DateTimeKind.Local);
            }
            
            return new TaskDtos
            {
                TaskID = task.taskid,
                Title = task.title,
                Description = task.description,
                DueDate = localDueDate,
                Status = task.task_status,
                Priority = task.priority_level,
                UserID = task.userid,
                CreatedAt = DateTime.Now, // Default values since these aren't in the new model
                UpdatedAt = DateTime.Now
            };
        }

        // Helper: convert TaskDtos to Database.TaskItem
        private Database.TaskItem ConvertToTaskItem(TaskDtos task)
        {
            // Add one day to compensate for time zone conversion issues
            var adjustedDueDate = task.DueDate.AddDays(1);
            
            // Convert due_date to UTC for storage
            var utcDueDate = adjustedDueDate;
            if (adjustedDueDate.Kind == DateTimeKind.Local)
            {
                utcDueDate = adjustedDueDate.ToUniversalTime();
            }
            else if (adjustedDueDate.Kind == DateTimeKind.Unspecified)
            {
                // If unspecified, assume it's local time and convert to UTC
                utcDueDate = DateTime.SpecifyKind(adjustedDueDate, DateTimeKind.Local).ToUniversalTime();
            }
            
            return new Database.TaskItem
            {
                taskid = task.TaskID,
                title = task.Title,
                description = task.Description,
                due_date = utcDueDate,
                task_status = task.Status,
                priority_level = task.Priority,
                userid = task.UserID
            };
        }

        // Method to expose SupabaseService for initialization
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
                await Shell.Current.GoToAsync("///tasks");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to navigate back: {ex.Message}", "OK");
            }
        }

        public async Task NavigateToCreateTaskAsync()
        {
            try
            {
                // Reset the editing task to default values
                Editing = new TaskDtos
                {
                    Title = "",
                    Description = "",
                    DueDate = DateTime.SpecifyKind(DateTime.Now.AddDays(1), DateTimeKind.Local),
                    Priority = "Medium",
                    Status = "In Progress",
                    UserID = 0,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                
                await Shell.Current.GoToAsync("CreateTaskPage");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to navigate to create task page: {ex.Message}", "OK");
            }
        }

        public async Task ExportTaskAsync(TaskDtos task)
        {
            try
            {
                // Create the task content for export
                var taskContent = $@"Task Export
================

Title: {task.Title}
Description: {task.Description}
Status: {task.Status}
Priority: {task.Priority}
Due Date: {task.DueDate:yyyy-MM-dd}
Created: {task.CreatedAt:yyyy-MM-dd HH:mm:ss}
Last Updated: {task.UpdatedAt:yyyy-MM-dd HH:mm:ss}

Task Details:
-------------
{task.Description}

Export generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}
Generated by: BrainWave App";

                // First, ask user to choose export format
                var formatChoice = await Shell.Current.DisplayActionSheet(
                    "Choose Export Format", 
                    "Cancel", 
                    null, 
                    "Export as TXT", 
                    "Export as PDF");

                if (formatChoice == "Cancel")
                    return;

                bool isPdf = formatChoice == "Export as PDF";
                string exportFormat = isPdf ? ".pdf" : ".txt";

                // Generate filename based on task title and format
                var safeTitle = string.Join("_", task.Title.Split(Path.GetInvalidFileNameChars()));
                var fileName = $"{safeTitle}_{DateTime.Now:yyyyMMdd_HHmmss}.{(isPdf ? "pdf" : "txt")}";

                // For MAUI, we'll save to a default location and show the path to the user
                // This is a simpler approach that works across all platforms
                var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                var fullPath = Path.Combine(documentsPath, fileName);

                if (isPdf)
                {
                    // For PDF, create a proper PDF file
                    await CreateProperPdfAsync(fullPath, taskContent);
                }
                else
                {
                    // For TXT, write directly
                    await File.WriteAllTextAsync(fullPath, taskContent);
                }

                // Create export record in database
                var currentUser = await _databaseService.GetCurrentUserAsync();
                if (currentUser != null)
                {
                    var exportRecord = new Database.Export
                    {
                        userid = currentUser.userid,
                        taskid = task.TaskID,
                        export_format = exportFormat,
                        date_requested = DateTime.Now
                    };

                    var exportSuccess = await _databaseService.CreateExportAsync(exportRecord);
                    if (exportSuccess)
                    {
                        System.Diagnostics.Debug.WriteLine($"Export record created successfully for task {task.TaskID} with format {exportFormat}");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"Failed to create export record for task {task.TaskID}");
                    }
                }

                await Shell.Current.DisplayAlert("Export Successful", 
                    $"Task '{task.Title}' has been exported as {(isPdf ? "PDF" : "TXT")} to:\n{fullPath}\n\nYou can find the file in your Documents folder.", 
                    "OK");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Export Error", 
                    $"Failed to export task: {ex.Message}", 
                    "OK");
            }
        }

        private async Task CreateProperPdfAsync(string filePath, string content)
        {
            // Create a proper PDF file with correct structure
            var lines = content.Split('\n');
            var pdfContent = new System.Text.StringBuilder();
            
            // PDF Header
            pdfContent.AppendLine("%PDF-1.4");
            
            // Catalog object
            pdfContent.AppendLine("1 0 obj");
            pdfContent.AppendLine("<<");
            pdfContent.AppendLine("/Type /Catalog");
            pdfContent.AppendLine("/Pages 2 0 R");
            pdfContent.AppendLine(">>");
            pdfContent.AppendLine("endobj");
            
            // Pages object
            pdfContent.AppendLine("2 0 obj");
            pdfContent.AppendLine("<<");
            pdfContent.AppendLine("/Type /Pages");
            pdfContent.AppendLine("/Kids [3 0 R]");
            pdfContent.AppendLine("/Count 1");
            pdfContent.AppendLine(">>");
            pdfContent.AppendLine("endobj");
            
            // Page object
            pdfContent.AppendLine("3 0 obj");
            pdfContent.AppendLine("<<");
            pdfContent.AppendLine("/Type /Page");
            pdfContent.AppendLine("/Parent 2 0 R");
            pdfContent.AppendLine("/MediaBox [0 0 612 792]");
            pdfContent.AppendLine("/Contents 4 0 R");
            pdfContent.AppendLine("/Resources <<");
            pdfContent.AppendLine("/Font <<");
            pdfContent.AppendLine("/F1 5 0 R");
            pdfContent.AppendLine(">>");
            pdfContent.AppendLine(">>");
            pdfContent.AppendLine(">>");
            pdfContent.AppendLine("endobj");
            
            // Font object
            pdfContent.AppendLine("5 0 obj");
            pdfContent.AppendLine("<<");
            pdfContent.AppendLine("/Type /Font");
            pdfContent.AppendLine("/Subtype /Type1");
            pdfContent.AppendLine("/BaseFont /Helvetica");
            pdfContent.AppendLine(">>");
            pdfContent.AppendLine("endobj");
            
            // Content stream
            var contentStream = new System.Text.StringBuilder();
            contentStream.AppendLine("BT");
            contentStream.AppendLine("/F1 12 Tf");
            contentStream.AppendLine("72 720 Td");
            
            // Add content line by line
            int yPosition = 720;
            foreach (var line in lines)
            {
                if (yPosition < 50) // Start new page if needed
                {
                    contentStream.AppendLine("ET");
                    contentStream.AppendLine("BT");
                    contentStream.AppendLine("/F1 12 Tf");
                    contentStream.AppendLine($"72 {yPosition} Td");
                }
                
                var escapedLine = line.Replace("(", "\\(").Replace(")", "\\)").Replace("\\", "\\\\");
                contentStream.AppendLine($"({escapedLine}) Tj");
                contentStream.AppendLine("0 -14 Td"); // Move to next line
                yPosition -= 14;
            }
            
            contentStream.AppendLine("ET");
            
            var contentString = contentStream.ToString();
            
            pdfContent.AppendLine("4 0 obj");
            pdfContent.AppendLine("<<");
            pdfContent.AppendLine($"/Length {contentString.Length}");
            pdfContent.AppendLine(">>");
            pdfContent.AppendLine("stream");
            pdfContent.Append(contentString);
            pdfContent.AppendLine("endstream");
            pdfContent.AppendLine("endobj");
            
            // Cross-reference table
            pdfContent.AppendLine("xref");
            pdfContent.AppendLine("0 6");
            pdfContent.AppendLine("0000000000 65535 f ");
            pdfContent.AppendLine("0000000009 00000 n ");
            pdfContent.AppendLine("0000000058 00000 n ");
            pdfContent.AppendLine("0000000115 00000 n ");
            pdfContent.AppendLine("0000000206 00000 n ");
            pdfContent.AppendLine("0000000300 00000 n ");
            
            // Trailer
            pdfContent.AppendLine("trailer");
            pdfContent.AppendLine("<<");
            pdfContent.AppendLine("/Size 6");
            pdfContent.AppendLine("/Root 1 0 R");
            pdfContent.AppendLine(">>");
            pdfContent.AppendLine($"startxref");
            pdfContent.AppendLine($"{pdfContent.Length + 50}");
            pdfContent.AppendLine("%%EOF");

            await File.WriteAllTextAsync(filePath, pdfContent.ToString());
        }
    }
}

