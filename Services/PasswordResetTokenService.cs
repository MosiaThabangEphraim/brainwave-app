using System.Security.Cryptography;

namespace BrainWave.APP.Services
{
    public class PasswordResetTokenService
    {
        private static readonly Dictionary<string, TokenInfo> _tokens = new Dictionary<string, TokenInfo>();
        private static readonly object _lock = new object();

        public class TokenInfo
        {
            public string Token { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public DateTime ExpiresAt { get; set; }
            public bool IsUsed { get; set; }
        }

        public string GenerateToken(string email)
        {
            System.Diagnostics.Debug.WriteLine($"GenerateToken called for email: '{email}'");
            Console.WriteLine($"🔑 GenerateToken called for email: '{email}'");
            
            // Generate a 6-digit numeric token
            var random = new Random();
            var token = random.Next(100000, 999999).ToString();
            
            var tokenInfo = new TokenInfo
            {
                Token = token,
                Email = email,
                ExpiresAt = DateTime.Now.AddMinutes(15), // 15 minutes expiry
                IsUsed = false
            };

            lock (_lock)
            {
                System.Diagnostics.Debug.WriteLine($"Current tokens in memory: {_tokens.Count}");
                foreach (var kvp in _tokens)
                {
                    System.Diagnostics.Debug.WriteLine($"  Token: {kvp.Key}, Email: '{kvp.Value.Email}', Expires: {kvp.Value.ExpiresAt}, Used: {kvp.Value.IsUsed}");
                }

                // Remove any existing tokens for this email
                var existingTokens = _tokens.Where(kvp => kvp.Value.Email == email).ToList();
                System.Diagnostics.Debug.WriteLine($"Found {existingTokens.Count} existing tokens for email '{email}'");
                foreach (var existingToken in existingTokens)
                {
                    System.Diagnostics.Debug.WriteLine($"Removing existing token: {existingToken.Key}");
                    _tokens.Remove(existingToken.Key);
                }

                // Add new token
                _tokens[token] = tokenInfo;
                System.Diagnostics.Debug.WriteLine($"Added new token: {token} for email: '{email}', expires at: {tokenInfo.ExpiresAt}");
                System.Diagnostics.Debug.WriteLine($"Total tokens after adding: {_tokens.Count}");
            }

            return token;
        }

        public bool ValidateToken(string token, string email)
        {
            System.Diagnostics.Debug.WriteLine($"ValidateToken called with token: '{token}' and email: '{email}'");
            Console.WriteLine($"🔍 ValidateToken called with token: '{token}' and email: '{email}'");
            
            lock (_lock)
            {
                System.Diagnostics.Debug.WriteLine($"Current tokens in memory: {_tokens.Count}");
                foreach (var kvp in _tokens)
                {
                    System.Diagnostics.Debug.WriteLine($"  Token: {kvp.Key}, Email: '{kvp.Value.Email}', Expires: {kvp.Value.ExpiresAt}, Used: {kvp.Value.IsUsed}");
                }

                if (!_tokens.TryGetValue(token, out var tokenInfo))
                {
                    System.Diagnostics.Debug.WriteLine($"Token '{token}' not found in memory");
                    return false;
                }

                System.Diagnostics.Debug.WriteLine($"Token found: {token}, Email: '{tokenInfo.Email}', Expires: {tokenInfo.ExpiresAt}, Used: {tokenInfo.IsUsed}");

                // Check if token is expired
                if (DateTime.Now > tokenInfo.ExpiresAt)
                {
                    System.Diagnostics.Debug.WriteLine($"Token '{token}' is expired. Current time: {DateTime.Now}, Expires: {tokenInfo.ExpiresAt}");
                    _tokens.Remove(token);
                    return false;
                }

                // Check if token is already used
                if (tokenInfo.IsUsed)
                {
                    System.Diagnostics.Debug.WriteLine($"Token '{token}' is already used");
                    return false;
                }

                // Check if email matches
                if (tokenInfo.Email != email)
                {
                    System.Diagnostics.Debug.WriteLine($"Email mismatch. Token email: '{tokenInfo.Email}', Provided email: '{email}'");
                    return false;
                }

                System.Diagnostics.Debug.WriteLine($"Token '{token}' validation successful");
                return true;
            }
        }

        public bool ValidateToken(string token)
        {
            System.Diagnostics.Debug.WriteLine($"ValidateToken called with token: '{token}' (no email required)");
            Console.WriteLine($"🔍 ValidateToken called with token: '{token}' (no email required)");
            
            lock (_lock)
            {
                System.Diagnostics.Debug.WriteLine($"Current tokens in memory: {_tokens.Count}");
                foreach (var kvp in _tokens)
                {
                    System.Diagnostics.Debug.WriteLine($"  Token: {kvp.Key}, Email: '{kvp.Value.Email}', Expires: {kvp.Value.ExpiresAt}, Used: {kvp.Value.IsUsed}");
                }

                if (!_tokens.TryGetValue(token, out var tokenInfo))
                {
                    System.Diagnostics.Debug.WriteLine($"Token '{token}' not found in memory");
                    return false;
                }

                System.Diagnostics.Debug.WriteLine($"Token found: {token}, Email: '{tokenInfo.Email}', Expires: {tokenInfo.ExpiresAt}, Used: {tokenInfo.IsUsed}");

                // Check if token is expired
                if (DateTime.Now > tokenInfo.ExpiresAt)
                {
                    System.Diagnostics.Debug.WriteLine($"Token '{token}' is expired. Current time: {DateTime.Now}, Expires: {tokenInfo.ExpiresAt}");
                    _tokens.Remove(token);
                    return false;
                }

                // Check if token is already used
                if (tokenInfo.IsUsed)
                {
                    System.Diagnostics.Debug.WriteLine($"Token '{token}' is already used");
                    return false;
                }

                System.Diagnostics.Debug.WriteLine($"Token '{token}' validation successful (no email check)");
                return true;
            }
        }

        public void MarkTokenAsUsed(string token)
        {
            lock (_lock)
            {
                if (_tokens.TryGetValue(token, out var tokenInfo))
                {
                    tokenInfo.IsUsed = true;
                }
            }
        }

        public TokenInfo? GetTokenInfo(string token)
        {
            lock (_lock)
            {
                if (_tokens.TryGetValue(token, out var tokenInfo))
                {
                    return tokenInfo;
                }
                return null;
            }
        }

        public void CleanupExpiredTokens()
        {
            lock (_lock)
            {
                var expiredTokens = _tokens.Where(kvp => DateTime.Now > kvp.Value.ExpiresAt).ToList();
                foreach (var expiredToken in expiredTokens)
                {
                    _tokens.Remove(expiredToken.Key);
                }
            }
        }
    }
}