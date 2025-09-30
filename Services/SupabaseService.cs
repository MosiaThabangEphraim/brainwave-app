


using Supabase;
using Supabase.Gotrue;
using BrainWave.APP.Models;
using BrainWave.APP.Database;
using Microsoft.Maui.Storage;
using static BrainWave.APP.Constants;
using System.Diagnostics;

namespace BrainWave.APP.Services
{
    public class SupabaseService
    {
        private readonly Supabase.Client _supabase;
        private Supabase.Gotrue.User? _currentUser;

        public SupabaseService()
        {
            var options = new SupabaseOptions
            {
                AutoConnectRealtime = true
            };
            
            _supabase = new Supabase.Client(SUPABASE_URL, SUPABASE_ANON_KEY, options);
        }

        public async Task InitializeAsync()
        {
            await _supabase.InitializeAsync();
            
            // Try to restore current user from storage
            await RestoreCurrentUserAsync();
        }

        public bool IsAuthenticated => _currentUser != null;
        public Supabase.Gotrue.User? CurrentUser => _currentUser;
        public Supabase.Client Client => _supabase;

        private async Task SaveCurrentUserAsync()
        {
            if (_currentUser != null)
            {
                await SecureStorage.SetAsync("current_user_email", _currentUser.Email ?? "");
                await SecureStorage.SetAsync("current_user_id", _currentUser.Id ?? "");
            }
        }

        private async Task RestoreCurrentUserAsync()
        {
            try
            {
                Debug.WriteLine("🔍 RestoreCurrentUserAsync: Starting...");
                
                var email = await SecureStorage.GetAsync("current_user_email");
                var userId = await SecureStorage.GetAsync("current_user_id");
                
                Debug.WriteLine($"🔍 RestoreCurrentUserAsync: Email from storage: '{email}'");
                Debug.WriteLine($"🔍 RestoreCurrentUserAsync: UserId from storage: '{userId}'");
                
                if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(userId))
                {
                    Debug.WriteLine("🔍 RestoreCurrentUserAsync: Found stored user data, verifying user still exists in database");
                    
                    // Verify the user still exists in the database
                    if (int.TryParse(userId, out int userIdInt))
                    {
                        var userExists = await Client
                            .From<Database.User>()
                            .Where(x => x.userid == userIdInt)
                            .Single();
                        
                        if (userExists != null)
                        {
                            Debug.WriteLine("🔍 RestoreCurrentUserAsync: User verified in database, creating user object");
                            _currentUser = new Supabase.Gotrue.User
                            {
                                Id = userId,
                                Email = email
                            };
                            Debug.WriteLine($"🔍 RestoreCurrentUserAsync: User restored - ID: {_currentUser.Id}, Email: {_currentUser.Email}");
                        }
                        else
                        {
                            Debug.WriteLine("🔍 RestoreCurrentUserAsync: User not found in database, clearing stored session");
                            await LogoutAsync();
                        }
                    }
                    else
                    {
                        Debug.WriteLine("🔍 RestoreCurrentUserAsync: Invalid user ID format, clearing stored session");
                        await LogoutAsync();
                    }
                }
                else
                {
                    Debug.WriteLine("🔍 RestoreCurrentUserAsync: No stored user data found");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"🔍 RestoreCurrentUserAsync: Error restoring current user: {ex.Message}");
                Debug.WriteLine($"🔍 RestoreCurrentUserAsync: Stack trace: {ex.StackTrace}");
                
                // If there's an error, clear the session to be safe
                await LogoutAsync();
            }
        }

        public async Task LogoutAsync()
        {
            Debug.WriteLine("🚪 LogoutAsync: Starting logout process");
            
            var currentEmail = await SecureStorage.GetAsync("current_user_email");
            var currentId = await SecureStorage.GetAsync("current_user_id");
            Debug.WriteLine($"🚪 LogoutAsync: Current user before logout - Email: {currentEmail}, ID: {currentId}");
            
            _currentUser = null;
            SecureStorage.Remove("current_user_email");
            SecureStorage.Remove("current_user_id");
            
            Debug.WriteLine("🚪 LogoutAsync: Cleared current user and removed secure storage");
            
            // Verify logout by checking if we can still get the user
            var verifyEmail = await SecureStorage.GetAsync("current_user_email");
            var verifyId = await SecureStorage.GetAsync("current_user_id");
            Debug.WriteLine($"🚪 LogoutAsync: Verification after logout - Email: {verifyEmail ?? "null"}, ID: {verifyId ?? "null"}");
        }

        // Authentication Methods
        public async Task<bool> LoginAsync(string email, string password)
        {
            try
            {
                Debug.WriteLine($"🔐 LoginAsync: Attempting login for email: {email}");
                
                // Get user from our database
                var user = await GetUserByEmailAsync(email);
                if (user != null)
                {
                    Debug.WriteLine($"🔐 LoginAsync: User found in database - ID: {user.userid}, Email: {user.email}");
                    
                    // Verify password using BCrypt
                    if (BCrypt.Net.BCrypt.Verify(password, user.password_hash))
                    {
                        Debug.WriteLine("🔐 LoginAsync: Password verification successful");
                        
                        // Create a mock Supabase user for compatibility
                        _currentUser = new Supabase.Gotrue.User
                        {
                            Id = user.userid.ToString(),
                            Email = user.email,
                            UserMetadata = new Dictionary<string, object>
                            {
                                { "role", user.role },
                                { "first_name", user.f_name },
                                { "last_name", user.l_name }
                            }
                        };
                        
                        // Store user session
                        await SecureStorage.SetAsync(SECURE_KEY_USER_ID, user.userid.ToString());
                        await SaveCurrentUserAsync();
                        
                        Debug.WriteLine("🔐 LoginAsync: User session stored successfully");
                        return true;
                    }
                    else
                    {
                        Debug.WriteLine("🔐 LoginAsync: Password verification failed");
                    }
                }
                else
                {
                    Debug.WriteLine("🔐 LoginAsync: User not found in database");
                }
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"🔐 LoginAsync: Exception occurred: {ex.Message}");
                Debug.WriteLine($"🔐 LoginAsync: Stack trace: {ex.StackTrace}");
                return false;
            }
        }

        public async Task<bool> RegisterAsync(RegisterRequest request)
        {
            try
            {
                // Check if user already exists
                var existingUser = await GetUserByEmailAsync(request.Email);
                if (existingUser != null)
                {
                    System.Diagnostics.Debug.WriteLine("User already exists");
                    return false;
                }

                // Create new user in our database
                var user = new Database.User
                {
                    f_name = request.FirstName,
                    l_name = request.LastName,
                    email = request.Email,
                    password_hash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                    role = request.Role ?? "Student",
                    profile_picture = null
                };

                var success = await CreateUserAsync(user);
                return success;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Registration error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> LoadStoredSessionAsync()
        {
            try
            {
                var storedUserId = await SecureStorage.GetAsync(SECURE_KEY_USER_ID);
                if (!string.IsNullOrEmpty(storedUserId) && int.TryParse(storedUserId, out int userId))
                {
                    var user = await GetUserByIdAsync(userId);
                    if (user != null)
                    {
                        // Create a mock Supabase user for compatibility
                        _currentUser = new Supabase.Gotrue.User
                        {
                            Id = user.userid.ToString(),
                            Email = user.email,
                            UserMetadata = new Dictionary<string, object>
                            {
                                { "role", user.role },
                                { "first_name", user.f_name },
                                { "last_name", user.l_name }
                            }
                        };
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Load session error: {ex.Message}");
                return false;
            }
        }



        // Admin Authentication (using stored credentials with fallback to hardcoded)
        public async Task<bool> AdminLoginAsync(string username, string password)
        {
            // First, try to get stored admin credentials
            var storedUsername = await SecureStorage.GetAsync("admin_username");
            var storedPassword = await SecureStorage.GetAsync("admin_password");
            
            // Check against stored credentials first, then fallback to hardcoded
            bool isValidCredentials = false;
            
            if (!string.IsNullOrEmpty(storedUsername) && !string.IsNullOrEmpty(storedPassword))
            {
                // Use stored credentials
                isValidCredentials = (username == storedUsername && password == storedPassword);
            }
            else
            {
                // Fallback to hardcoded credentials
                isValidCredentials = (username == ADMIN_USERNAME && password == ADMIN_PASSWORD);
            }
            
            if (isValidCredentials)
            {
                // Create a mock admin user
                _currentUser = new Supabase.Gotrue.User
                {
                    Id = "admin",
                    Email = "admin@brainwave.com",
                    UserMetadata = new Dictionary<string, object>
                    {
                        { "role", "Admin" },
                        { "first_name", "Admin" },
                        { "last_name", "User" }
                    }
                };
                
                await SecureStorage.SetAsync(SECURE_KEY_ADMIN_TOKEN, "admin_token");
                return true;
            }
            return false;
        }

        // User Management
        public async Task<List<Database.User>> GetAllUsersAsync()
        {
            try
            {
                var response = await _supabase
                    .From<Database.User>()
                    .Get();
                
                return response.Models ?? new List<Database.User>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Get users error: {ex.Message}");
                return new List<Database.User>();
            }
        }

        public async Task<Database.User?> GetUserByIdAsync(int userId)
        {
            try
            {
                var response = await _supabase
                    .From<Database.User>()
                    .Where(x => x.userid == userId)
                    .Single();
                
                return response;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Get user error: {ex.Message}");
                return null;
            }
        }

        public async Task<Database.User?> GetUserByEmailAsync(string email)
        {
            try
            {
                var response = await _supabase
                    .From<Database.User>()
                    .Where(x => x.email == email)
                    .Single();
                
                return response;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Get user by email error: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> CreateUserAsync(Database.User user)
        {
            try
            {
                await _supabase
                    .From<Database.User>()
                    .Insert(user);
                
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Create user error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateUserAsync(Database.User user)
        {
            try
            {
                var query = _supabase
                    .From<Database.User>()
                    .Where(x => x.userid == user.userid)
                    .Set(x => x.f_name, user.f_name)
                    .Set(x => x.l_name, user.l_name)
                    .Set(x => x.email, user.email)
                    .Set(x => x.role, user.role)
                    .Set(x => x.profile_picture, user.profile_picture);

                // Only update password_hash if it's not empty
                if (!string.IsNullOrEmpty(user.password_hash))
                {
                    query = query.Set(x => x.password_hash, user.password_hash);
                }

                await query.Update();
                
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Update user error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteUserAsync(int userId)
        {
            try
            {
                await _supabase
                    .From<Database.User>()
                    .Where(x => x.userid == userId)
                    .Delete();
                
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Delete user error: {ex.Message}");
                return false;
            }
        }

        // Task Management
        public async Task<List<Database.TaskItem>> GetAllTasksAsync()
        {
            try
            {
                var response = await _supabase
                    .From<Database.TaskItem>()
                    .Get();
                
                return response.Models ?? new List<Database.TaskItem>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Get tasks error: {ex.Message}");
                return new List<Database.TaskItem>();
            }
        }

        public async Task<List<Database.TaskItem>> GetTasksByUserIdAsync(int userId)
        {
            try
            {
                var response = await _supabase
                    .From<Database.TaskItem>()
                    .Where(x => x.userid == userId)
                    .Get();
                
                return response.Models ?? new List<Database.TaskItem>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Get user tasks error: {ex.Message}");
                return new List<Database.TaskItem>();
            }
        }

        public async Task<bool> CreateTaskAsync(Database.TaskItem task)
        {
            try
            {
                await _supabase
                    .From<Database.TaskItem>()
                    .Insert(task);
                
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Create task error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateTaskAsync(Database.TaskItem task)
        {
            try
            {
                await _supabase
                    .From<Database.TaskItem>()
                    .Where(x => x.taskid == task.taskid)
                    .Set(x => x.title, task.title)
                    .Set(x => x.description, task.description)
                    .Set(x => x.due_date, task.due_date)
                    .Set(x => x.task_status, task.task_status)
                    .Set(x => x.priority_level, task.priority_level)
                    .Update();
                
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Update task error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteTaskAsync(int taskId)
        {
            try
            {
                await _supabase
                    .From<Database.TaskItem>()
                    .Where(x => x.taskid == taskId)
                    .Delete();
                
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Delete task error: {ex.Message}");
                return false;
            }
        }

        // Reminder Management
        public async Task<List<Database.Reminder>> GetAllRemindersAsync()
        {

            try
            {
                var response = await _supabase
                    .From<Database.Reminder>()
                    .Get();
                
                return response.Models ?? new List<Database.Reminder>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Get reminders error: {ex.Message}");
                return new List<Database.Reminder>();
            }
        }

        public async Task<List<Database.Reminder>> GetRemindersByTaskIdAsync(int taskId)
        {
            try
            {
                var response = await _supabase
                    .From<Database.Reminder>()
                    .Where(x => x.taskid == taskId)
                    .Get();
                
                return response.Models ?? new List<Database.Reminder>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Get reminders by task error: {ex.Message}");
                return new List<Database.Reminder>();
            }
        }

        public async Task<bool> CreateReminderAsync(Database.Reminder reminder)
        {
            try
            {
                await _supabase
                    .From<Database.Reminder>()
                    .Insert(reminder);
                
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Create reminder error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateReminderAsync(Database.Reminder reminder)
        {
            try
            {
                await _supabase
                    .From<Database.Reminder>()
                    .Where(x => x.reminderid == reminder.reminderid)
                    .Set(x => x.reminder_type, reminder.reminder_type)
                    .Set(x => x.notify_time, reminder.notify_time)
                    .Update();
                
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Update reminder error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteReminderAsync(int reminderId)
        {
            try
            {
                await _supabase
                    .From<Database.Reminder>()
                    .Where(x => x.reminderid == reminderId)
                    .Delete();
                
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Delete reminder error: {ex.Message}");
                return false;
            }
        }

        // Badge Management
        public async Task<List<Database.Badge>> GetAllBadgesAsync()
        {
            try
            {
                var response = await _supabase
                    .From<Database.Badge>()
                    .Get();
                
                return response.Models ?? new List<Database.Badge>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Get badges error: {ex.Message}");
                return new List<Database.Badge>();
            }
        }

        public async Task<List<Database.UserBadge>> GetUserBadgesAsync(int userId)
        {
            try
            {
                var response = await _supabase
                    .From<Database.UserBadge>()
                    .Where(x => x.userid == userId)
                    .Get();
                
                return response.Models ?? new List<Database.UserBadge>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Get user badges error: {ex.Message}");
                return new List<Database.UserBadge>();
            }
        }

        public async Task<bool> AwardBadgeAsync(int userId, int badgeId)
        {
            try
            {
                var userBadge = new Database.UserBadge
                {
                    userid = userId,
                    badgeid = badgeId,
                    date_earned = DateTime.Now
                };
                
                await _supabase
                    .From<Database.UserBadge>()
                    .Insert(userBadge);
                
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Award badge error: {ex.Message}");
                return false;
            }
        }

        // Collaboration Management
        public async Task<List<Database.Collaboration>> GetAllCollaborationsAsync()
        {
            try
            {
                var response = await _supabase
                    .From<Database.Collaboration>()
                    .Get();
                
                return response.Models ?? new List<Database.Collaboration>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Get collaborations error: {ex.Message}");
                return new List<Database.Collaboration>();
            }
        }

        public async Task<List<Database.Collaboration>> GetCollaborationsByTaskIdAsync(int taskId)
        {
            try
            {
                var response = await _supabase
                    .From<Database.Collaboration>()
                    .Where(x => x.taskid == taskId)
                    .Get();
                
                return response.Models ?? new List<Database.Collaboration>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Get collaborations by task error: {ex.Message}");
                return new List<Database.Collaboration>();
            }
        }

        public async Task<bool> CreateCollaborationAsync(Database.Collaboration collaboration)
        {
            try
            {
                await _supabase
                    .From<Database.Collaboration>()
                    .Insert(collaboration);
                
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Create collaboration error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> JoinCollaborationAsync(int userId, int collaborationId, string role)
        {
            try
            {
                var userCollaboration = new Database.UserCollaboration
                {
                    userid = userId,
                    collaborationid = collaborationId,
                    collaboration_role = role
                };
                
                await _supabase
                    .From<Database.UserCollaboration>()
                    .Insert(userCollaboration);
                
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Join collaboration error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateCollaborationAsync(Models.CollaborationModel collaboration)
        {
            try
            {
                var dbCollaboration = new Database.Collaboration
                {
                    collaborationid = collaboration.CollaborationID,
                    taskid = collaboration.TaskID,
                    collaboration_title = collaboration.Name,
                    collaboration_description = collaboration.Description,
                    collaboration_token = collaboration.Token
                };

                await _supabase
                    .From<Database.Collaboration>()
                    .Where(x => x.collaborationid == collaboration.CollaborationID)
                    .Update(dbCollaboration);

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Update collaboration error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteCollaborationAsync(int collaborationId)
        {
            try
            {
                await _supabase
                    .From<Database.Collaboration>()
                    .Where(x => x.collaborationid == collaborationId)
                    .Delete();

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Delete collaboration error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateUserCollaborationRoleAsync(int collaborationId, int userId, string newRole)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"=== SupabaseService: UPDATING USER COLLABORATION ROLE ===");
                System.Diagnostics.Debug.WriteLine($"Collaboration ID: {collaborationId}");
                System.Diagnostics.Debug.WriteLine($"User ID: {userId}");
                System.Diagnostics.Debug.WriteLine($"New Role: '{newRole}'");

                var response = await _supabase
                    .From<Database.UserCollaboration>()
                    .Where(x => x.collaborationid == collaborationId && x.userid == userId)
                    .Set(x => x.collaboration_role, newRole)
                    .Update();

                System.Diagnostics.Debug.WriteLine($"SupabaseService role update response: {response?.Models?.Count ?? 0} rows affected");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SupabaseService error updating user collaboration role: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                return false;
            }
        }

        // Export Management
        public async Task<bool> CreateExportAsync(Database.Export export)
        {
            try
            {
                await _supabase
                    .From<Database.Export>()
                    .Insert(export);
                
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Create export error: {ex.Message}");
                return false;
            }
        }

        public async Task<List<Database.Export>> GetExportsByUserIdAsync(int userId)
        {
            try
            {
                var response = await _supabase
                    .From<Database.Export>()
                    .Where(x => x.userid == userId)
                    .Get();
                
                return response.Models ?? new List<Database.Export>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Get exports by user error: {ex.Message}");
                return new List<Database.Export>();
            }
        }

        // Badge System Logic - Moved to DatabaseService.cs

        public bool IsAdmin => _currentUser?.UserMetadata?.GetValueOrDefault("role")?.ToString()?.ToLower() == "admin";
        public bool IsUser => _currentUser?.UserMetadata?.GetValueOrDefault("role")?.ToString()?.ToLower() == "user" ||
                             _currentUser?.UserMetadata?.GetValueOrDefault("role")?.ToString()?.ToLower() == "student" ||
                             _currentUser?.UserMetadata?.GetValueOrDefault("role")?.ToString()?.ToLower() == "professional";

        public async Task<bool> UpdateAdminCredentialsAsync(string newUsername, string newPassword)
        {
            try
            {
                // Store the new admin credentials in SecureStorage
                await SecureStorage.SetAsync("admin_username", newUsername);
                await SecureStorage.SetAsync("admin_password", newPassword);
                
                Debug.WriteLine($"Admin credentials updated - Username: {newUsername}");
                
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating admin credentials: {ex.Message}");
                return false;
            }
        }
    }
}
