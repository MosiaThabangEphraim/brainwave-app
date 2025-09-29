namespace BrainWave.APP;
public static class Constants
{
    public const string API_BASE = "http://localhost:5104"; // Local development server
    public const string SECURE_KEY_USER_TOKEN = "jwt_user";
    public const string SECURE_KEY_ADMIN_TOKEN = "jwt_admin";
    public const string SECURE_KEY_USER_ID = "user_id";
    
    // Supabase Configuration
    public const string SUPABASE_URL = "https://mduvsjvirrphwhggwfkz.supabase.co";
    public const string SUPABASE_ANON_KEY = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Im1kdXZzanZpcnJwaHdoZ2d3Zmt6Iiwicm9sZSI6ImFub24iLCJpYXQiOjE3NTYzMDgyNTIsImV4cCI6MjA3MTg4NDI1Mn0.Bn4-wJgsoSyteHgRAjN4DViavT59ktxHyzxf50DdhtM";
    
    // Admin Credentials
    public const string ADMIN_USERNAME = "admin";
    public const string ADMIN_PASSWORD = "admin123!";
    
    // Badge Thresholds
    public const int AMATEUR_THRESHOLD = 1;
    public const int ACHIEVER_THRESHOLD = 26;
    public const int TASK_MASTER_THRESHOLD = 51;
    public const int PRODUCTIVITY_CHAMPION_THRESHOLD = 100;
}
