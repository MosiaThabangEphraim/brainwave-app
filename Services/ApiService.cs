using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using BrainWave.APP.Models;
using static BrainWave.APP.Constants;

namespace BrainWave.APP.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        public ApiService()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(API_BASE);
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        // Authentication
        public async Task<string> LoginAsync(string email, string password)
        {
            try
            {
                var loginRequest = new { Email = email, Password = password };
                var json = JsonSerializer.Serialize(loginRequest, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("/api/auth/login", content);
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var loginResponse = JsonSerializer.Deserialize<LoginResponse>(responseContent, _jsonOptions);
                    return loginResponse?.Token ?? string.Empty;
                }
                return string.Empty;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public async Task<bool> RegisterAsync(RegisterRequest request)
        {
            try
            {
                var registerRequest = new 
                { 
                    F_Name = request.FirstName, 
                    L_Name = request.LastName, 
                    Email = request.Email, 
                    Password = request.Password,
                    Role = request.Role
                };
                var json = JsonSerializer.Serialize(registerRequest, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("/api/auth/register", content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void SetAuthToken(string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        // Tasks
        public async Task<List<TaskDtos>> GetTasksAsync(Dictionary<string, string>? filters = null)
        {
            try
            {
                var queryString = "";
                if (filters != null && filters.Count > 0)
                {
                    var queryParams = new List<string>();
                    foreach (var filter in filters)
                    {
                        queryParams.Add($"{filter.Key}={Uri.EscapeDataString(filter.Value)}");
                    }
                    queryString = "?" + string.Join("&", queryParams);
                }

                var response = await _httpClient.GetAsync($"/api/tasks{queryString}");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<List<TaskDtos>>(content, _jsonOptions) ?? new List<TaskDtos>();
                }
                return new List<TaskDtos>();
            }
            catch (Exception)
            {
                return new List<TaskDtos>();
            }
        }

        public async Task<TaskDtos?> GetTaskAsync(int taskId)
        {
            var response = await _httpClient.GetAsync($"/api/tasks/{taskId}");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<TaskDtos>(content, _jsonOptions);
            }
            return null;
        }

        public async Task<bool> CreateTaskAsync(TaskDtos task)
        {
            var json = JsonSerializer.Serialize(task, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/api/tasks", content);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateTaskAsync(TaskDtos task)
        {
            var json = JsonSerializer.Serialize(task, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"/api/tasks/{task.TaskID}", content);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteTaskAsync(int taskId)
        {
            var response = await _httpClient.DeleteAsync($"/api/tasks/{taskId}");
            return response.IsSuccessStatusCode;
        }

        // Reminders
        public async Task<List<ReminderModel>> GetRemindersAsync()
        {
            var response = await _httpClient.GetAsync("/api/reminders");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<ReminderModel>>(content, _jsonOptions) ?? new List<ReminderModel>();
            }
            return new List<ReminderModel>();
        }

        public async Task<bool> CreateReminderAsync(ReminderModel reminder)
        {
            var json = JsonSerializer.Serialize(reminder, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/api/reminders", content);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateReminderAsync(ReminderModel reminder)
        {
            var json = JsonSerializer.Serialize(reminder, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"/api/reminders/{reminder.ReminderID}", content);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteReminderAsync(int reminderId)
        {
            var response = await _httpClient.DeleteAsync($"/api/reminders/{reminderId}");
            return response.IsSuccessStatusCode;
        }

        // User Profile
        public async Task<UserDtos?> GetUserProfileAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/user");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<UserDtos>(content, _jsonOptions);
                }
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<bool> UpdateUserProfileAsync(UserDtos user)
        {
            try
            {
                var json = JsonSerializer.Serialize(user, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"/api/user/{user.UserID}/profile", content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // Badges
        public async Task<List<BadgeDtos>> GetBadgesAsync()
        {
            var response = await _httpClient.GetAsync("/api/badges");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<BadgeDtos>>(content, _jsonOptions) ?? new List<BadgeDtos>();
            }
            return new List<BadgeDtos>();
        }

        // Collaboration
        public async Task<List<CollaborationModel>> GetCollaborationsAsync()
        {
            var response = await _httpClient.GetAsync("/api/collaboration");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<CollaborationModel>>(content, _jsonOptions) ?? new List<CollaborationModel>();
            }
            return new List<CollaborationModel>();
        }

        public async Task<bool> CreateCollaborationAsync(CollaborationModel collaboration)
        {
            var json = JsonSerializer.Serialize(collaboration, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/api/collaboration", content);
            return response.IsSuccessStatusCode;
        }

        // Export
        public async Task<bool> ExportDataAsync(string exportType)
        {
            var request = new { ExportType = exportType };
            var json = JsonSerializer.Serialize(request, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/api/export", content);
            return response.IsSuccessStatusCode;
        }

        // Admin methods (placeholder implementations)
        public async Task<string> AdminLoginAsync(string username, string password)
        {
            try
            {
                var loginRequest = new { Username = username, Password = password };
                var json = JsonSerializer.Serialize(loginRequest, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("/api/admin/login", content);
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var loginResponse = JsonSerializer.Deserialize<AdminLoginResponse>(responseContent, _jsonOptions);
                    return loginResponse?.Token ?? string.Empty;
                }
                return string.Empty;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public async Task<List<AdminUserDto>> AdminGetUsersAsync(Dictionary<string, string>? filters = null)
        {
            try
            {
                var queryParams = new List<string>();
                if (filters != null)
                {
                    foreach (var filter in filters)
                    {
                        queryParams.Add($"{filter.Key}={Uri.EscapeDataString(filter.Value)}");
                    }
                }
                
                var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
                var response = await _httpClient.GetAsync($"/api/admin/users{queryString}");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<List<AdminUserDto>>(content, _jsonOptions) ?? new List<AdminUserDto>();
                }
                return new List<AdminUserDto>();
            }
            catch (Exception)
            {
                return new List<AdminUserDto>();
            }
        }

        public async Task<bool> AdminUpdateUserAsync(int id, UserDtos user)
        {
            try
            {
                var json = JsonSerializer.Serialize(user, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"/api/admin/users/{id}", content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> AdminDeleteUserAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"/api/admin/users/{id}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public Task<List<AdminTaskDto>> AdminGetTasksAsync(Dictionary<string, string>? filters = null)
        {
            // Placeholder - would need admin endpoint
            return Task.FromResult(new List<AdminTaskDto>());
        }

        public Task<bool> AdminUpdateTaskAsync(AdminTaskDto task)
        {
            // Placeholder - would need admin endpoint
            return Task.FromResult(false);
        }

        public Task<bool> AdminDeleteTaskAsync(int id)
        {
            // Placeholder - would need admin endpoint
            return Task.FromResult(false);
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }

    // Helper classes for API responses
    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
    }

    public class RegisterRequest
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = "User";
    }
}