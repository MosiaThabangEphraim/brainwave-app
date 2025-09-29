using BrainWave.APP.Models;
using BrainWave.APP.Services;
using BrainWave.APP.Database;
using System.Collections.ObjectModel;

namespace BrainWave.APP.ViewModels
{
    public class AdminTasksViewModel : BaseViewModel
    {
        private readonly SupabaseService _supabaseService;
        public ObservableCollection<AdminTaskDto> Items { get; } = new();

        // Filters
        public string? UserID { get; set; } = string.Empty;
        public string? Title { get; set; } = string.Empty;
        public string? Task_Status { get; set; } = string.Empty;
        public string? Priority_Level { get; set; } = string.Empty;

        public AdminTaskDto Editing { get; set; } = new();

        public AdminTasksViewModel(SupabaseService supabaseService)
        {
            _supabaseService = supabaseService;
        }

        public async Task RefreshAsync()
        {
            var filters = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(UserID)) filters["UserID"] = UserID;
            if (!string.IsNullOrEmpty(Title)) filters["Title"] = Title;
            if (!string.IsNullOrEmpty(Task_Status)) filters["Task_Status"] = Task_Status;
            if (!string.IsNullOrEmpty(Priority_Level)) filters["Priority_Level"] = Priority_Level;
            Items.Clear();
            var tasks = await _supabaseService.GetAllTasksAsync();
            
            // Apply filters manually since Supabase doesn't support complex filtering in this simple implementation
            var filteredTasks = tasks.Where(t => 
                (string.IsNullOrEmpty(UserID) || t.userid.ToString() == UserID) &&
                (string.IsNullOrEmpty(Title) || t.title.Contains(Title, StringComparison.OrdinalIgnoreCase)) &&
                (string.IsNullOrEmpty(Task_Status) || t.task_status == Task_Status) &&
                (string.IsNullOrEmpty(Priority_Level) || t.priority_level == Priority_Level)
            ).ToList();
            
            foreach (var task in filteredTasks)
            {
                var adminTask = new AdminTaskDto
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
                    Category = "General", // Default category
                    CreatedAt = DateTime.Now, // Default created date
                    UpdatedAt = DateTime.Now, // Default updated date
                    UserName = "User" // We'll need to get this from user data if needed
                };
                Items.Add(adminTask);
            }
        }

        public async Task UpdateAsync()
        {
            var task = new Database.TaskItem
            {
                taskid = Editing.TaskID,
                title = Editing.Title,
                description = Editing.Description,
                due_date = Editing.DueDate,
                task_status = Editing.Status,
                priority_level = Editing.Priority,
                userid = Editing.UserID
            };
            if (await _supabaseService.UpdateTaskAsync(task)) await RefreshAsync();
        }

        public async Task DeleteAsync(AdminTaskDto t)
        {
            if (await _supabaseService.DeleteTaskAsync(t.TaskID)) await RefreshAsync();
        }
    }
}