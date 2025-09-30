using Supabase;
using BrainWave.APP.Database;
using Supabase.Postgrest.Models;
using System.Diagnostics;

namespace BrainWave.APP.Services
{
    public class DatabaseService
    {
        private readonly SupabaseService _supabaseService;

        public DatabaseService(SupabaseService supabaseService)
        {
            _supabaseService = supabaseService;
        }

        public SupabaseService? GetSupabaseService()
        {
            return _supabaseService;
        }

        // User operations
        public async Task<User?> GetCurrentUserAsync()
        {
            try
            {
                Debug.WriteLine($"🔍 GetCurrentUserAsync: Starting...");
                
                var currentUser = _supabaseService.CurrentUser;
                Debug.WriteLine($"🔍 GetCurrentUserAsync: Supabase CurrentUser: {currentUser != null}");
                
                if (currentUser == null) 
                {
                    Debug.WriteLine($"🔍 GetCurrentUserAsync: CurrentUser is null, returning null");
                    return null;
                }

                Debug.WriteLine($"🔍 GetCurrentUserAsync: Looking for user with email: {currentUser.Email}");

                var response = await _supabaseService.Client
                    .From<User>()
                    .Where(x => x.email == currentUser.Email)
                    .Single();

                Debug.WriteLine($"🔍 GetCurrentUserAsync: Found user: {response != null}");
                if (response != null)
                {
                    Debug.WriteLine($"🔍 GetCurrentUserAsync: User ID: {response.userid}, Email: {response.email}, Name: {response.f_name} {response.l_name}");
                }

                return response;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"🔍 GetCurrentUserAsync: Error getting current user: {ex.Message}");
                Debug.WriteLine($"🔍 GetCurrentUserAsync: Stack trace: {ex.StackTrace}");
                return null;
            }
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            try
            {
                var response = await _supabaseService.Client
                    .From<User>()
                    .Where(x => x.email == email)
                    .Single();

                return response;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting user by email: {ex.Message}");
                return null;
            }
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            try
            {
                Debug.WriteLine($"🔍 GetUserByIdAsync: Getting user with ID: {userId}");
                
                var response = await _supabaseService.Client
                    .From<User>()
                    .Where(x => x.userid == userId)
                    .Single();

                Debug.WriteLine($"🔍 GetUserByIdAsync: Found user: {response != null}");
                if (response != null)
                {
                    Debug.WriteLine($"🔍 GetUserByIdAsync: User ID: {response.userid}, Email: {response.email}, Name: {response.f_name} {response.l_name}");
                }

                return response;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"🔍 GetUserByIdAsync: Error getting user by ID: {ex.Message}");
                Debug.WriteLine($"🔍 GetUserByIdAsync: Stack trace: {ex.StackTrace}");
                return null;
            }
        }

        public async Task<bool> CreateUserAsync(User user)
        {
            try
            {
                var response = await _supabaseService.Client
                    .From<User>()
                    .Insert(user);

                return response != null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error creating user: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            try
            {
                var response = await _supabaseService.Client
                    .From<User>()
                    .Where(x => x.userid == user.userid)
                    .Set(x => x.f_name, user.f_name)
                    .Set(x => x.l_name, user.l_name)
                    .Set(x => x.email, user.email)
                    .Set(x => x.role, user.role)
                    .Set(x => x.profile_picture, user.profile_picture)
                    .Update();

                return response != null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating user: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateUserPasswordAsync(int userId, string newPassword)
        {
            try
            {
                // Hash the password before storing it
                var hashedPassword = BCrypt.Net.BCrypt.HashPassword(newPassword);
                
                var response = await _supabaseService.Client
                    .From<User>()
                    .Where(x => x.userid == userId)
                    .Set(x => x.password_hash, hashedPassword)
                    .Update();

                return response != null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating user password: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateUserRoleAsync(int userId, string newRole)
        {
            try
            {
                var response = await _supabaseService.Client
                    .From<User>()
                    .Where(x => x.userid == userId)
                    .Set(x => x.role, newRole)
                    .Update();

                return response != null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating user role: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteUserAccountAsync(int userId)
        {
            try
            {
                Debug.WriteLine($"🗑️ DeleteUserAccountAsync: Starting deletion for user ID: {userId}");

                // First, verify user exists before deletion
                var existingUser = await _supabaseService.Client
                    .From<User>()
                    .Where(x => x.userid == userId)
                    .Single();
                
                if (existingUser == null)
                {
                    Debug.WriteLine($"❌ DeleteUserAccountAsync: User with ID {userId} not found in database");
                    return false;
                }
                Debug.WriteLine($"✅ DeleteUserAccountAsync: User found - Email: {existingUser.email}, Name: {existingUser.f_name} {existingUser.l_name}");

                // First, get user's tasks to delete associated reminders
                var userTasks = await _supabaseService.Client
                    .From<Database.TaskItem>()
                    .Where(x => x.userid == userId)
                    .Get();
                Debug.WriteLine($"🗑️ DeleteUserAccountAsync: Found {userTasks.Models?.Count ?? 0} tasks for user");

                // Delete user's reminders (through tasks)
                int reminderCount = 0;
                foreach (var task in userTasks.Models ?? new List<Database.TaskItem>())
                {
                    var reminders = await _supabaseService.Client
                        .From<Reminder>()
                        .Where(x => x.taskid == task.taskid)
                        .Get();
                    
                    if (reminders.Models?.Any() == true)
                    {
                        await _supabaseService.Client
                            .From<Reminder>()
                            .Where(x => x.taskid == task.taskid)
                            .Delete();
                        reminderCount += reminders.Models.Count;
                    }
                }
                Debug.WriteLine($"🗑️ DeleteUserAccountAsync: Deleted {reminderCount} reminders");

                // Delete user's tasks
                await _supabaseService.Client
                    .From<Database.TaskItem>()
                    .Where(x => x.userid == userId)
                    .Delete();
                Debug.WriteLine($"🗑️ DeleteUserAccountAsync: Deleted user's tasks");

                // Delete user's collaborations
                await _supabaseService.Client
                    .From<UserCollaboration>()
                    .Where(x => x.userid == userId)
                    .Delete();
                Debug.WriteLine($"🗑️ DeleteUserAccountAsync: Deleted user's collaborations");

                // Delete user's badges
                await _supabaseService.Client
                    .From<UserBadge>()
                    .Where(x => x.userid == userId)
                    .Delete();
                Debug.WriteLine($"🗑️ DeleteUserAccountAsync: Deleted user's badges");

                // Delete user's exports
                await _supabaseService.Client
                    .From<Export>()
                    .Where(x => x.userid == userId)
                    .Delete();
                Debug.WriteLine($"🗑️ DeleteUserAccountAsync: Deleted user's exports");

                // Finally, delete the user account
                await _supabaseService.Client
                    .From<User>()
                    .Where(x => x.userid == userId)
                    .Delete();
                Debug.WriteLine($"🗑️ DeleteUserAccountAsync: Deleted user account");

                // Verify user is actually deleted
                var verifyUser = await _supabaseService.Client
                    .From<User>()
                    .Where(x => x.userid == userId)
                    .Single();
                
                if (verifyUser == null)
                {
                    Debug.WriteLine("✅ DeleteUserAccountAsync: User successfully deleted from database");
                    return true;
                }
                else
                {
                    Debug.WriteLine($"❌ DeleteUserAccountAsync: User still exists after deletion! UserID: {verifyUser.userid}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ DeleteUserAccountAsync: Exception occurred: {ex.Message}");
                Debug.WriteLine($"❌ DeleteUserAccountAsync: Stack trace: {ex.StackTrace}");
                return false;
            }
        }

        // Task operations
        public async Task<List<Database.TaskItem>> GetTasksByUserIdAsync(int userId)
        {
            try
            {
                Debug.WriteLine($"🔍 GetTasksByUserIdAsync: Getting tasks for user ID: {userId}");
                
                var response = await _supabaseService.Client
                    .From<Database.TaskItem>()
                    .Where(x => x.userid == userId)
                    .Get();

                var tasks = response.Models.ToList();
                Debug.WriteLine($"🔍 GetTasksByUserIdAsync: Found {tasks.Count} tasks for user {userId}");
                
                if (tasks.Count > 0)
                {
                    foreach (var task in tasks)
                    {
                        Debug.WriteLine($"🔍 GetTasksByUserIdAsync: Task - ID: {task.taskid}, Title: '{task.title}', Status: {task.task_status}");
                    }
                }
                else
                {
                    Debug.WriteLine($"🔍 GetTasksByUserIdAsync: No tasks found for user {userId}");
                }

                return tasks;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"🔍 GetTasksByUserIdAsync: Error getting tasks by user ID: {ex.Message}");
                Debug.WriteLine($"🔍 GetTasksByUserIdAsync: Stack trace: {ex.StackTrace}");
                return new List<Database.TaskItem>();
            }
        }

        public async Task<Database.TaskItem?> GetTaskByIdAsync(int taskId)
        {
            try
            {
                var response = await _supabaseService.Client
                    .From<Database.TaskItem>()
                    .Where(x => x.taskid == taskId)
                    .Single();

                return response;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting task by ID: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> CreateTaskAsync(Database.TaskItem task)
        {
            try
            {
                Debug.WriteLine($"Creating task: Title={task.title}, UserID={task.userid}, Status={task.task_status}");
                
                var response = await _supabaseService.Client
                    .From<Database.TaskItem>()
                    .Insert(task);

                Debug.WriteLine($"Task creation response: {response?.Models?.Count ?? 0} items inserted");
                return response != null && response.Models != null && response.Models.Count > 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error creating task: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                return false;
            }
        }

        public async Task<bool> UpdateTaskAsync(Database.TaskItem task)
        {
            try
            {
                var response = await _supabaseService.Client
                    .From<Database.TaskItem>()
                    .Where(x => x.taskid == task.taskid)
                    .Set(x => x.title, task.title)
                    .Set(x => x.description, task.description)
                    .Set(x => x.due_date, task.due_date)
                    .Set(x => x.task_status, task.task_status)
                    .Set(x => x.priority_level, task.priority_level)
                    .Update();

                return response != null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating task: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteTaskAsync(int taskId)
        {
            try
            {
                // Delete associated reminders first
                await _supabaseService.Client
                    .From<Reminder>()
                    .Where(x => x.taskid == taskId)
                    .Delete();

                // Delete the task
                await _supabaseService.Client
                    .From<Database.TaskItem>()
                    .Where(x => x.taskid == taskId)
                    .Delete();

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error deleting task: {ex.Message}");
                return false;
            }
        }

        // Reminder operations
        public async Task<List<Reminder>> GetRemindersByUserIdAsync(int userId)
        {
            try
            {
                Debug.WriteLine($"Getting reminders for user ID: {userId}");
                
                // Follow the User → Task → Reminder relationship chain
                // This implements the logic: SELECT r.* FROM reminder r JOIN task t ON r.taskid = t.taskid WHERE t.userid = :userId
                // But using two queries since Supabase doesn't support direct JOINs in this way
                
                // First, get all tasks for the user
                var userTasks = await GetTasksByUserIdAsync(userId);
                var taskIds = userTasks.Select(t => t.taskid).ToList();
                
                Debug.WriteLine($"Found {userTasks.Count} tasks for user {userId}");
                Debug.WriteLine($"Task IDs: [{string.Join(", ", taskIds)}]");

                if (!taskIds.Any()) 
                {
                    Debug.WriteLine("No tasks found for user, returning empty reminder list");
                    return new List<Reminder>();
                }

                // Now get all reminders for these tasks in one query
                // This is equivalent to: SELECT r.reminderid, r.taskid, r.reminder_type, r.notify_time 
                // FROM reminder r WHERE r.taskid IN (SELECT t.taskid FROM task t WHERE t.userid = :userId)
                var response = await _supabaseService.Client
                    .From<Database.Reminder>()
                    .Select("reminderid, taskid, reminder_type, notify_time")
                    .Get();

                Debug.WriteLine($"Found {response.Models?.Count ?? 0} total reminders in database");
                
                // Filter reminders to only include those for the user's tasks
                var filteredReminders = response.Models?.Where(r => taskIds.Contains(r.taskid)).ToList() ?? new List<Database.Reminder>();
                
                Debug.WriteLine($"Found {filteredReminders.Count} reminders for user {userId}");
                
                if (filteredReminders.Count > 0)
                {
                    foreach (var reminder in filteredReminders)
                    {
                        Debug.WriteLine($"Reminder: ID={reminder.reminderid}, TaskID={reminder.taskid}, Type={reminder.reminder_type}, Time={reminder.notify_time}");
                    }
                }

                return filteredReminders;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting reminders by user ID: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                return new List<Reminder>();
            }
        }

        public async Task<bool> CreateReminderAsync(Reminder reminder)
        {
            try
            {
                Debug.WriteLine($"Starting reminder creation: taskid={reminder.taskid}, reminder_type='{reminder.reminder_type}', notify_time={reminder.notify_time}");
                Debug.WriteLine($"Reminder object: reminderid={reminder.reminderid}");
                
                // Validate the taskid
                if (reminder.taskid <= 0)
                {
                    Debug.WriteLine($"ERROR: Invalid taskid: {reminder.taskid}");
                    return false;
                }
                
                // Validate the notify_time
                if (reminder.notify_time == default(DateTime))
                {
                    Debug.WriteLine($"ERROR: Invalid notify_time: {reminder.notify_time}");
                    return false;
                }

                var response = await _supabaseService.Client
                    .From<Database.Reminder>()
                    .Insert(reminder);

                Debug.WriteLine($"Reminder insert response: {response != null}");
                Debug.WriteLine($"Reminder Models count: {response?.Models?.Count ?? 0}");
                
                if (response != null && response.Models?.Count > 0)
                {
                    Debug.WriteLine($"SUCCESS: Reminder created with ID: {response.Models.First().reminderid}");
                }

                return response != null && response.Models != null && response.Models.Count > 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error creating reminder: {ex.Message}");
                Debug.WriteLine($"Reminder creation error stack trace: {ex.StackTrace}");
                return false;
            }
        }

        public async Task<bool> UpdateReminderAsync(Reminder reminder)
        {
            try
            {
                var response = await _supabaseService.Client
                    .From<Database.Reminder>()
                    .Where(x => x.reminderid == reminder.reminderid)
                    .Set(x => x.reminder_type, reminder.reminder_type)
                    .Set(x => x.notify_time, reminder.notify_time)
                    .Update();

                return response != null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating reminder: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteReminderAsync(int reminderId)
        {
            try
            {
                await _supabaseService.Client
                    .From<Reminder>()
                    .Where(x => x.reminderid == reminderId)
                    .Delete();

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error deleting reminder: {ex.Message}");
                return false;
            }
        }

        // Collaboration operations
        public async Task<List<Collaboration>> GetCollaborationsByUserIdAsync(int userId)
        {
            try
            {
                Debug.WriteLine($"=== GetCollaborationsByUserIdAsync START ===");
                Debug.WriteLine($"Looking for collaborations for user ID: {userId}");
                
                // Get collaborations where user is a participant
                var userCollaborations = await _supabaseService.Client
                    .From<UserCollaboration>()
                    .Where(x => x.userid == userId)
                    .Get();

                Debug.WriteLine($"Found {userCollaborations.Models?.Count ?? 0} user_collaboration entries");
                
                if (userCollaborations.Models?.Count > 0)
                {
                    foreach (var uc in userCollaborations.Models)
                    {
                        Debug.WriteLine($"UserCollaboration: userid={uc.userid}, collaborationid={uc.collaborationid}, role='{uc.collaboration_role}'");
                    }
                }

                var collaborationIds = userCollaborations.Models?.Select(uc => uc.collaborationid).ToList() ?? new List<int>();
                Debug.WriteLine($"Extracted collaboration IDs: [{string.Join(", ", collaborationIds)}]");

                if (!collaborationIds.Any()) 
                {
                    Debug.WriteLine("No collaboration IDs found, returning empty list");
                    return new List<Collaboration>();
                }

                // Fetch all collaborations first, then filter in memory (same issue as reminders)
                var allCollaborationsResponse = await _supabaseService.Client
                    .From<Database.Collaboration>()
                    .Get();
                
                Debug.WriteLine($"Found {allCollaborationsResponse.Models?.Count ?? 0} total collaboration records");
                
                // Filter collaborations to only include those for the user
                var response = new { Models = allCollaborationsResponse.Models?.Where(c => collaborationIds.Contains(c.collaborationid)).ToList() };

                Debug.WriteLine($"Found {response.Models?.Count ?? 0} collaboration records");
                
                if (response.Models?.Count > 0)
                {
                    foreach (var collab in response.Models)
                    {
                        Debug.WriteLine($"Collaboration: ID={collab.collaborationid}, Title='{collab.collaboration_title}', Token='{collab.collaboration_token}'");
                    }
                }

                Debug.WriteLine($"=== GetCollaborationsByUserIdAsync END ===");
                return response.Models?.ToList() ?? new List<Collaboration>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting collaborations by user ID: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                return new List<Collaboration>();
            }
        }

        // Enhanced method to get collaborations with user roles, task title, and task status
        public async Task<List<(Collaboration collaboration, string role, string taskTitle, string taskStatus)>> GetCollaborationsWithTaskInfoByUserIdAsync(int userId)
        {
            try
            {
                Debug.WriteLine($"=== GetCollaborationsWithTaskInfoByUserIdAsync START ===");
                Debug.WriteLine($"Looking for collaborations with task info for user ID: {userId}");
                
                // Step 1: Get user_collaboration entries for the user
                var userCollaborations = await _supabaseService.Client
                    .From<UserCollaboration>()
                    .Where(x => x.userid == userId)
                    .Get();

                Debug.WriteLine($"Found {userCollaborations.Models?.Count ?? 0} user_collaboration entries");
                
                if (userCollaborations.Models?.Count == 0)
                {
                    Debug.WriteLine("No user_collaboration entries found, returning empty list");
                    return new List<(Collaboration, string, string, string)>();
                }

                var collaborationIds = userCollaborations.Models.Select(uc => uc.collaborationid).ToList();
                Debug.WriteLine($"Extracted collaboration IDs: [{string.Join(", ", collaborationIds)}]");

                // Step 2: Get collaboration details using those IDs
                // Fetch all collaborations first, then filter in memory (same issue as reminders)
                var allCollaborationsResponse = await _supabaseService.Client
                    .From<Database.Collaboration>()
                    .Get();
                
                Debug.WriteLine($"Found {allCollaborationsResponse.Models?.Count ?? 0} total collaboration records");
                
                // Filter collaborations to only include those for the user
                var collaborationsResponse = new { Models = allCollaborationsResponse.Models?.Where(c => collaborationIds.Contains(c.collaborationid)).ToList() };

                Debug.WriteLine($"Found {collaborationsResponse.Models?.Count ?? 0} collaboration records");
                
                var result = new List<(Collaboration, string, string, string)>();
                
                if (collaborationsResponse.Models?.Count > 0)
                {
                    // Step 3: For each collaboration, get the associated task information
                    foreach (var collab in collaborationsResponse.Models)
                    {
                        // Find the user's role for this collaboration
                        var userCollab = userCollaborations.Models.FirstOrDefault(uc => uc.collaborationid == collab.collaborationid);
                        var role = userCollab?.collaboration_role ?? "Unknown";
                        
                        // Get task information for this collaboration
                        string taskTitle = "Unknown Task";
                        string taskStatus = "Unknown";
                        
                        try
                        {
                            var taskResponse = await _supabaseService.Client
                                .From<Database.TaskItem>()
                                .Where(x => x.taskid == collab.taskid)
                                .Single();
                            
                            if (taskResponse != null)
                            {
                                taskTitle = taskResponse.title ?? "Unknown Task";
                                taskStatus = taskResponse.task_status ?? "Unknown";
                            }
                        }
                        catch (Exception taskEx)
                        {
                            Debug.WriteLine($"Error getting task info for collaboration {collab.collaborationid}: {taskEx.Message}");
                        }
                        
                        Debug.WriteLine($"Collaboration: ID={collab.collaborationid}, Title='{collab.collaboration_title}', Role='{role}', TaskTitle='{taskTitle}', TaskStatus='{taskStatus}'");
                        result.Add((collab, role, taskTitle, taskStatus));
                    }
                }

                Debug.WriteLine($"=== GetCollaborationsWithTaskInfoByUserIdAsync END ===");
                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting collaborations with task info by user ID: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                return new List<(Collaboration, string, string, string)>();
            }
        }

        // New method to get collaborations with user roles
        public async Task<List<(Collaboration collaboration, string role)>> GetCollaborationsWithRolesByUserIdAsync(int userId)
        {
            try
            {
                Debug.WriteLine($"=== GetCollaborationsWithRolesByUserIdAsync START ===");
                Debug.WriteLine($"Looking for collaborations with roles for user ID: {userId}");
                
                // Get collaborations where user is a participant
                var userCollaborations = await _supabaseService.Client
                    .From<UserCollaboration>()
                    .Where(x => x.userid == userId)
                    .Get();

                Debug.WriteLine($"Found {userCollaborations.Models?.Count ?? 0} user_collaboration entries");
                
                if (userCollaborations.Models?.Count == 0)
                {
                    Debug.WriteLine("No user_collaboration entries found, returning empty list");
                    return new List<(Collaboration, string)>();
                }

                var collaborationIds = userCollaborations.Models.Select(uc => uc.collaborationid).ToList();
                Debug.WriteLine($"Extracted collaboration IDs: [{string.Join(", ", collaborationIds)}]");

                // Fetch all collaborations first, then filter in memory (same issue as reminders)
                var allCollaborationsResponse = await _supabaseService.Client
                    .From<Database.Collaboration>()
                    .Get();
                
                Debug.WriteLine($"Found {allCollaborationsResponse.Models?.Count ?? 0} total collaboration records");
                
                // Filter collaborations to only include those for the user
                var response = new { Models = allCollaborationsResponse.Models?.Where(c => collaborationIds.Contains(c.collaborationid)).ToList() };

                Debug.WriteLine($"Found {response.Models?.Count ?? 0} collaboration records");
                
                var result = new List<(Collaboration, string)>();
                
                if (response.Models?.Count > 0)
                {
                    foreach (var collab in response.Models)
                    {
                        // Find the user's role for this collaboration
                        var userCollab = userCollaborations.Models.FirstOrDefault(uc => uc.collaborationid == collab.collaborationid);
                        var role = userCollab?.collaboration_role ?? "Unknown";
                        
                        Debug.WriteLine($"Collaboration: ID={collab.collaborationid}, Title='{collab.collaboration_title}', Role='{role}'");
                        result.Add((collab, role));
                    }
                }

                Debug.WriteLine($"=== GetCollaborationsWithRolesByUserIdAsync END ===");
                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting collaborations with roles by user ID: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                return new List<(Collaboration, string)>();
            }
        }

        public async Task<Collaboration?> GetCollaborationByTokenAsync(string token)
        {
            try
            {
                Debug.WriteLine($"GetCollaborationByTokenAsync called with token: {token}");
                
                // Ensure Supabase is initialized
                Debug.WriteLine("Initializing Supabase...");
                await _supabaseService.InitializeAsync();
                
                Debug.WriteLine("Querying Supabase for collaboration by token...");
                var response = await _supabaseService.Client
                    .From<Database.Collaboration>()
                    .Where(x => x.collaboration_token == token)
                    .Single();

                Debug.WriteLine($"Found collaboration: {response != null}");
                if (response != null)
                {
                    Debug.WriteLine($"Collaboration ID: {response.collaborationid}, Title: {response.collaboration_title}");
                }
                
                return response;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting collaboration by token: {ex.Message}");
                Debug.WriteLine($"Exception type: {ex.GetType().Name}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                return null;
            }
        }

        public async Task<bool> CreateCollaborationAsync(Collaboration collaboration, int userId, string role)
        {
            try
            {
                Debug.WriteLine($"=== STARTING COLLABORATION CREATION ===");
                Debug.WriteLine($"User ID: {userId}");
                Debug.WriteLine($"Role: '{role}'");
                Debug.WriteLine($"Collaboration Title: '{collaboration.collaboration_title}'");
                Debug.WriteLine($"Collaboration Token: '{collaboration.collaboration_token}'");
                Debug.WriteLine($"Task ID: {collaboration.taskid}");
                Debug.WriteLine($"Collaboration ID (before insert): {collaboration.collaborationid}");

                // Validate inputs
                if (userId <= 0)
                {
                    Debug.WriteLine($"ERROR: Invalid userId: {userId}");
                    return false;
                }
                
                if (string.IsNullOrWhiteSpace(role))
                {
                    Debug.WriteLine($"ERROR: Invalid role: '{role}'");
                    return false;
                }
                
                if (string.IsNullOrWhiteSpace(collaboration.collaboration_token))
                {
                    Debug.WriteLine($"ERROR: Invalid collaboration_token: '{collaboration.collaboration_token}'");
                    return false;
                }

                if (string.IsNullOrWhiteSpace(collaboration.collaboration_title))
                {
                    Debug.WriteLine($"ERROR: Invalid collaboration_title: '{collaboration.collaboration_title}'");
                    return false;
                }

                if (collaboration.taskid <= 0)
                {
                    Debug.WriteLine($"ERROR: Invalid taskid: {collaboration.taskid}");
                    return false;
                }

                // Create the collaboration
                Debug.WriteLine($"Inserting collaboration into database...");
                var response = await _supabaseService.Client
                    .From<Database.Collaboration>()
                    .Insert(collaboration);

                Debug.WriteLine($"Collaboration insert response: {response != null}");
                Debug.WriteLine($"Collaboration Models count: {response?.Models?.Count ?? 0}");
                
                if (response?.Models?.Count > 0)
                {
                    var insertedCollaboration = response.Models.First();
                    Debug.WriteLine($"SUCCESS: Collaboration inserted with ID: {insertedCollaboration.collaborationid}");
                    
                    // Use the ID from the response instead of querying
                    int collaborationId = insertedCollaboration.collaborationid;
                    
                    Debug.WriteLine($"Creating user_collaboration entry: userid={userId}, collaborationid={collaborationId}, role='{role}'");
                    
                    // Create user_collaboration entry to link user to collaboration
                    var userCollaboration = new UserCollaboration
                    {
                        userid = userId,
                        collaborationid = collaborationId,
                        collaboration_role = role
                    };

                    Debug.WriteLine($"UserCollaboration object created: userid={userCollaboration.userid}, collaborationid={userCollaboration.collaborationid}, role='{userCollaboration.collaboration_role}'");

                    // Insert user_collaboration using raw SQL for bridge table
                    try
                    {
                        var userCollaborationResponse = await _supabaseService.Client
                            .From<UserCollaboration>()
                            .Insert(userCollaboration);
                        
                        Debug.WriteLine($"User collaboration insert response: {userCollaborationResponse != null}");
                        Debug.WriteLine($"User collaboration Models count: {userCollaborationResponse?.Models?.Count ?? 0}");
                        
                        if (userCollaborationResponse != null)
                        {
                            Debug.WriteLine($"SUCCESS: User collaboration created successfully!");
                            Debug.WriteLine($"=== COLLABORATION CREATION COMPLETED SUCCESSFULLY ===");
                            return true;
                        }
                        else
                        {
                            Debug.WriteLine($"ERROR: User collaboration insert failed - response is null");
                            return false;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"ERROR inserting user_collaboration: {ex.Message}");
                        Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                        return false;
                    }
                }
                else
                {
                    Debug.WriteLine($"ERROR: Collaboration insert failed - no models returned");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERROR in CreateCollaborationAsync: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                return false;
            }
        }

        public async Task<bool> JoinCollaborationAsync(int userId, int collaborationId, string role)
        {
            try
            {
                Debug.WriteLine($"=== STARTING JOIN COLLABORATION ===");
                Debug.WriteLine($"User ID: {userId}");
                Debug.WriteLine($"Collaboration ID: {collaborationId}");
                Debug.WriteLine($"Role: '{role}'");

                // Validate inputs
                if (userId <= 0)
                {
                    Debug.WriteLine($"ERROR: Invalid userId: {userId}");
                    return false;
                }
                
                if (collaborationId <= 0)
                {
                    Debug.WriteLine($"ERROR: Invalid collaborationId: {collaborationId}");
                    return false;
                }
                
                if (string.IsNullOrWhiteSpace(role))
                {
                    Debug.WriteLine($"ERROR: Invalid role: '{role}'");
                    return false;
                }
                
                var userCollaboration = new UserCollaboration
                {
                    userid = userId,
                    collaborationid = collaborationId,
                    collaboration_role = role
                };

                Debug.WriteLine($"UserCollaboration object created: userid={userCollaboration.userid}, collaborationid={userCollaboration.collaborationid}, role='{userCollaboration.collaboration_role}'");

                // Insert user_collaboration with error handling
                Debug.WriteLine($"Inserting user_collaboration into database...");
                try
                {
                    var response = await _supabaseService.Client
                        .From<UserCollaboration>()
                        .Insert(userCollaboration);

                    Debug.WriteLine($"Join collaboration insert response: {response != null}");
                    Debug.WriteLine($"Join collaboration Models count: {response?.Models?.Count ?? 0}");
                    
                    if (response != null)
                    {
                        Debug.WriteLine($"SUCCESS: User joined collaboration successfully!");
                        Debug.WriteLine($"=== JOIN COLLABORATION COMPLETED SUCCESSFULLY ===");
                        return true;
                    }
                    else
                    {
                        Debug.WriteLine($"ERROR: Join collaboration insert failed - response is null");
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"ERROR inserting user_collaboration in join: {ex.Message}");
                    Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERROR in JoinCollaborationAsync: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                return false;
            }
        }

        public async Task<bool> DeleteCollaborationAsync(int collaborationId)
        {
            try
            {
                // Delete user collaborations first
                await _supabaseService.Client
                    .From<UserCollaboration>()
                    .Where(x => x.collaborationid == collaborationId)
                    .Delete();

                // Delete the collaboration
                await _supabaseService.Client
                    .From<Collaboration>()
                    .Where(x => x.collaborationid == collaborationId)
                    .Delete();

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error deleting collaboration: {ex.Message}");
                return false;
            }
        }

        public async Task<List<CollaborationMember>> GetCollaborationMembersAsync(int collaborationId)
        {
            try
            {
                var response = await _supabaseService.Client
                    .From<UserCollaboration>()
                    .Select("userid, collaboration_role")
                    .Where(x => x.collaborationid == collaborationId)
                    .Get();

                var members = new List<CollaborationMember>();
                
                if (response.Models != null)
                {
                    foreach (var userCollab in response.Models)
                    {
                        var user = await GetUserByIdAsync(userCollab.userid);
                        if (user != null)
                        {
                            members.Add(new CollaborationMember
                            {
                                Name = $"{user.f_name} {user.l_name}".Trim(),
                                Email = user.email,
                                Role = userCollab.collaboration_role
                            });
                        }
                    }
                }

                return members;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting collaboration members: {ex.Message}");
                return new List<CollaborationMember>();
            }
        }

        public async Task<bool> UpdateCollaborationAsync(Database.Collaboration collaboration)
        {
            try
            {
                Debug.WriteLine($"=== UPDATING COLLABORATION ===");
                Debug.WriteLine($"Collaboration ID: {collaboration.collaborationid}");
                Debug.WriteLine($"Title: '{collaboration.collaboration_title}'");
                Debug.WriteLine($"Description: '{collaboration.collaboration_description}'");
                Debug.WriteLine($"Token: '{collaboration.collaboration_token}'");

                var response = await _supabaseService.Client
                    .From<Database.Collaboration>()
                    .Where(x => x.collaborationid == collaboration.collaborationid)
                    .Set(x => x.collaboration_title, collaboration.collaboration_title)
                    .Set(x => x.collaboration_description, collaboration.collaboration_description)
                    .Update();

                Debug.WriteLine($"Update response: {response?.Models?.Count ?? 0} rows affected");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating collaboration: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                return false;
            }
        }

        public async Task<bool> UpdateUserCollaborationRoleAsync(int collaborationId, int userId, string newRole)
        {
            try
            {
                Debug.WriteLine($"=== DatabaseService: UPDATING USER COLLABORATION ROLE ===");
                Debug.WriteLine($"Collaboration ID: {collaborationId}");
                Debug.WriteLine($"User ID: {userId}");
                Debug.WriteLine($"New Role: '{newRole}'");

                var result = await _supabaseService.UpdateUserCollaborationRoleAsync(collaborationId, userId, newRole);
                
                Debug.WriteLine($"DatabaseService role update result: {result}");
                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"DatabaseService error updating user collaboration role: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                return false;
            }
        }

        public async Task<bool> RemoveUserFromCollaborationAsync(int collaborationId, int userId)
        {
            try
            {
                await _supabaseService.Client
                    .From<UserCollaboration>()
                    .Where(x => x.collaborationid == collaborationId && x.userid == userId)
                    .Delete();

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error removing user from collaboration: {ex.Message}");
                return false;
            }
        }

        // Badge operations
        public async Task<List<Badge>> GetAllBadgesAsync()
        {
            try
            {
                var response = await _supabaseService.Client
                    .From<Database.Badge>()
                    .Get();

                return response.Models.ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting all badges: {ex.Message}");
                return new List<Badge>();
            }
        }

        public async Task<List<UserBadge>> GetUserBadgesAsync(int userId)
        {
            try
            {
                var response = await _supabaseService.Client
                    .From<Database.UserBadge>()
                    .Where(x => x.userid == userId)
                    .Get();

                return response.Models.ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting user badges: {ex.Message}");
                return new List<UserBadge>();
            }
        }

        public async Task<bool> AwardBadgeAsync(int userId, int badgeId)
        {
            try
            {
                Debug.WriteLine($"Attempting to award badge {badgeId} to user {userId}");
                
                var userBadge = new UserBadge
                {
                    userid = userId,
                    badgeid = badgeId,
                    date_earned = DateTime.UtcNow
                };

                try
                {
                    var response = await _supabaseService.Client
                        .From<Database.UserBadge>()
                        .Insert(userBadge);

                    Debug.WriteLine($"Badge insert response: {response != null}");
                    Debug.WriteLine($"Badge Models count: {response?.Models?.Count ?? 0}");
                    
                    bool success = response != null;
                    Debug.WriteLine($"Badge award result: {success} (response: {response != null})");
                    
                    if (success)
                    {
                        Debug.WriteLine($"Successfully awarded badge {badgeId} to user {userId}");
                    }
                    else
                    {
                        Debug.WriteLine($"Failed to award badge {badgeId} to user {userId}");
                    }

                    return success;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"ERROR inserting user_badge: {ex.Message}");
                    Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error awarding badge {badgeId} to user {userId}: {ex.Message}");
                return false;
            }
        }

        public async Task CheckAndAwardBadgesAsync(int userId)
        {
            try
            {
                Debug.WriteLine($"Checking badges for user {userId}");
                
                // Get completed tasks count for this user
                var tasks = await GetTasksByUserIdAsync(userId);
                var completedCount = tasks.Count(t => t.task_status == "Completed");
                Debug.WriteLine($"User {userId} has {completedCount} completed tasks");
                
                // Get existing badges for this user
                var existingBadges = await GetUserBadgesAsync(userId);
                var existingBadgeIds = existingBadges.Select(b => b.badgeid).ToList();
                Debug.WriteLine($"User {userId} already has badges: [{string.Join(", ", existingBadgeIds)}]");
                
                // Badge thresholds
                const int AMATEUR_THRESHOLD = 1;
                const int ACHIEVER_THRESHOLD = 26;
                const int TASK_MASTER_THRESHOLD = 51;
                const int PRODUCTIVITY_CHAMPION_THRESHOLD = 100;
                
                // Award badges based on thresholds - check each badge independently
                if (completedCount >= AMATEUR_THRESHOLD && !existingBadgeIds.Contains(1))
                {
                    var success = await AwardBadgeAsync(userId, 1); // Amateur (1-25 tasks)
                    if (success)
                        Debug.WriteLine($"✅ Successfully awarded Amateur badge to user {userId} (completed: {completedCount})");
                    else
                        Debug.WriteLine($"❌ Failed to award Amateur badge to user {userId}");
                }
                
                if (completedCount >= ACHIEVER_THRESHOLD && !existingBadgeIds.Contains(2))
                {
                    var success = await AwardBadgeAsync(userId, 2); // Achiever (26-50 tasks)
                    if (success)
                        Debug.WriteLine($"✅ Successfully awarded Achiever badge to user {userId} (completed: {completedCount})");
                    else
                        Debug.WriteLine($"❌ Failed to award Achiever badge to user {userId}");
                }
                
                if (completedCount >= TASK_MASTER_THRESHOLD && !existingBadgeIds.Contains(3))
                {
                    var success = await AwardBadgeAsync(userId, 3); // Task Master (51-100 tasks)
                    if (success)
                        Debug.WriteLine($"✅ Successfully awarded Task Master badge to user {userId} (completed: {completedCount})");
                    else
                        Debug.WriteLine($"❌ Failed to award Task Master badge to user {userId}");
                }
                
                if (completedCount >= PRODUCTIVITY_CHAMPION_THRESHOLD && !existingBadgeIds.Contains(4))
                {
                    var success = await AwardBadgeAsync(userId, 4); // Productivity Champion (100+ tasks)
                    if (success)
                        Debug.WriteLine($"✅ Successfully awarded Productivity Champion badge to user {userId} (completed: {completedCount})");
                    else
                        Debug.WriteLine($"❌ Failed to award Productivity Champion badge to user {userId}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error checking and awarding badges: {ex.Message}");
            }
        }

        // Export operations
        public async Task<bool> CreateExportAsync(Export export)
        {
            try
            {
                var response = await _supabaseService.Client
                    .From<Export>()
                    .Insert(export);

                return response != null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error creating export: {ex.Message}");
                return false;
            }
        }

        public async Task<List<Export>> GetExportsByUserIdAsync(int userId)
        {
            try
            {
                var response = await _supabaseService.Client
                    .From<Export>()
                    .Where(x => x.userid == userId)
                    .Get();

                return response.Models.ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting exports by user ID: {ex.Message}");
                return new List<Export>();
            }
        }

        // Debug method to check user_collaboration table
        public async Task<List<UserCollaboration>> GetAllUserCollaborationsAsync()
        {
            try
            {
                var response = await _supabaseService.Client
                    .From<Database.UserCollaboration>()
                    .Get();

                Debug.WriteLine($"Found {response.Models?.Count ?? 0} user_collaboration entries");
                
                if (response.Models?.Count > 0)
                {
                    foreach (var uc in response.Models)
                    {
                        Debug.WriteLine($"UserCollaboration: userid={uc.userid}, collaborationid={uc.collaborationid}, role='{uc.collaboration_role}'");
                    }
                }

                return response.Models.ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting all user collaborations: {ex.Message}");
                return new List<UserCollaboration>();
            }
        }

        // Password Reset operations
        public async Task<bool> ResetUserPasswordAsync(int userId, string newPassword)
        {
            try
            {
                // Hash the new password
                var hashedPassword = BCrypt.Net.BCrypt.HashPassword(newPassword);

                var response = await _supabaseService.Client
                    .From<User>()
                    .Where(x => x.userid == userId)
                    .Set(x => x.password_hash, hashedPassword)
                    .Update();

                Debug.WriteLine($"Password reset successfully for user {userId}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error resetting password: {ex.Message}");
                return false;
            }
        }

        // Message operations
        public async Task<List<BrainWave.APP.Models.MessageDto>> GetMessagesByCollaborationIdAsync(int collaborationId)
        {
            try
            {
                Debug.WriteLine($"🔍 GetMessagesByCollaborationIdAsync: Getting messages for collaboration ID: {collaborationId}");

                var response = await _supabaseService.Client
                    .From<Database.Message>()
                    .Where(x => x.collaborationid == collaborationId)
                    .Get();

                Debug.WriteLine($"🔍 GetMessagesByCollaborationIdAsync: Raw response from database - {response.Models.Count} messages found");
                
                var messages = new List<BrainWave.APP.Models.MessageDto>();
                
                foreach (var message in response.Models)
                {
                    Debug.WriteLine($"🔍 GetMessagesByCollaborationIdAsync: Processing message ID: {message.messageid}, Collaboration ID: {message.collaborationid}, Content: '{message.content.Substring(0, Math.Min(30, message.content.Length))}...'");
                    
                    // Get user information for each message
                    var user = await GetUserByIdAsync(message.userid);
                    
                    messages.Add(new BrainWave.APP.Models.MessageDto
                    {
                        MessageId = message.messageid,
                        CollaborationId = message.collaborationid,
                        UserId = message.userid,
                        Content = message.content,
                        SentAt = message.sent_at,
                        UserName = user != null ? $"{user.f_name} {user.l_name}" : "Unknown User",
                        UserEmail = user?.email ?? "unknown@email.com"
                    });
                }

                Debug.WriteLine($"🔍 GetMessagesByCollaborationIdAsync: Returning {messages.Count} messages for collaboration ID: {collaborationId}");
                return messages;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting messages by collaboration ID: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                return new List<BrainWave.APP.Models.MessageDto>();
            }
        }

        public async Task<bool> SendMessageAsync(int collaborationId, int userId, string content)
        {
            try
            {
                Debug.WriteLine($"🔍 SendMessageAsync: Sending message for collaboration ID: {collaborationId}, user ID: {userId}");

                var message = new Database.Message
                {
                    collaborationid = collaborationId,
                    userid = userId,
                    content = content,
                    sent_at = DateTime.UtcNow
                };

                var response = await _supabaseService.Client
                    .From<Database.Message>()
                    .Insert(message);

                Debug.WriteLine($"🔍 SendMessageAsync: Message sent successfully");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error sending message: {ex.Message}");
                return false;
            }
        }
    }

    public class CollaborationMember
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}
