using System.Text.Json;
using BrainWave.APP.Models;
using Microsoft.Maui.Storage;

namespace BrainWave.APP.Services
{
    public class AuthenticationService
    {
        private readonly SupabaseService _supabaseService;
        private UserDtos? _currentUser;
        private string? _authToken;

        public AuthenticationService(SupabaseService supabaseService)
        {
            _supabaseService = supabaseService;
        }

        public bool IsAuthenticated => !string.IsNullOrEmpty(_authToken);
        public UserDtos? CurrentUser => _currentUser;
        public string? AuthToken => _authToken;

        public async Task<bool> LoginAsync(string email, string password)
        {
            try
            {
                var success = await _supabaseService.LoginAsync(email, password);
                if (success)
                {
                    _authToken = "supabase_token"; // Simple token for local use
                    _currentUser = new UserDtos
                    {
                        Email = email,
                        Role = _supabaseService.CurrentUser?.UserMetadata?.GetValueOrDefault("role")?.ToString() ?? "User"
                    };
                    
                    // Store token securely
                    await SecureStorage.SetAsync(Constants.SECURE_KEY_USER_TOKEN, _authToken);
                    
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> RegisterAsync(RegisterRequest request)
        {
            try
            {
                // Register user directly in our database
                var success = await _supabaseService.RegisterAsync(request);
                return success;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Registration error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> LoadStoredTokenAsync()
        {
            try
            {
                var storedToken = await SecureStorage.GetAsync(Constants.SECURE_KEY_USER_TOKEN);
                if (!string.IsNullOrEmpty(storedToken))
                {
                    _authToken = storedToken;
                    
                    // Load stored session from Supabase
                    var success = await _supabaseService.LoadStoredSessionAsync();
                    if (success)
                    {
                        _currentUser = new UserDtos
                        {
                            Email = _supabaseService.CurrentUser?.Email ?? "",
                            Role = _supabaseService.CurrentUser?.UserMetadata?.GetValueOrDefault("role")?.ToString() ?? "User"
                        };
                        return true;
                    }
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task LogoutAsync()
        {
            _authToken = null;
            _currentUser = null;
            
            // Logout from Supabase
            await _supabaseService.LogoutAsync();
            
            // Remove stored token
            SecureStorage.Remove(Constants.SECURE_KEY_USER_TOKEN);
        }

        public async Task<bool> AdminLoginAsync(string username, string password)
        {
            try
            {
                var success = await _supabaseService.AdminLoginAsync(username, password);
                if (success)
                {
                    _authToken = "admin_token";
                    _currentUser = new UserDtos
                    {
                        Email = "admin@brainwave.com",
                        Role = "Admin"
                    };
                    
                    // Store admin token securely
                    await SecureStorage.SetAsync(Constants.SECURE_KEY_ADMIN_TOKEN, _authToken);
                    
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> LoadStoredAdminTokenAsync()
        {
            try
            {
                var storedToken = await SecureStorage.GetAsync(Constants.SECURE_KEY_ADMIN_TOKEN);
                if (!string.IsNullOrEmpty(storedToken))
                {
                    _authToken = storedToken;
                    _currentUser = new UserDtos
                    {
                        Email = "admin@brainwave.com",
                        Role = "Admin"
                    };
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool IsAdmin => _currentUser?.Role?.ToLower() == "admin";
        public bool IsUser => _currentUser?.Role?.ToLower() == "user" || _currentUser?.Role?.ToLower() == "student" || _currentUser?.Role?.ToLower() == "professional";
    }
}
