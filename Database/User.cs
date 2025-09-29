using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace BrainWave.APP.Database
{
    [Table("User")]
    public class User : BaseModel
    {
        [PrimaryKey("userid")]
        public int userid { get; set; }
        
        [Column("f_name")]
        public string f_name { get; set; } = string.Empty;
        
        [Column("l_name")]
        public string l_name { get; set; } = string.Empty;
        
        [Column("email")]
        public string email { get; set; } = string.Empty;
        
        [Column("password_hash")]
        public string password_hash { get; set; } = string.Empty;
        
        [Column("role")]
        public string role { get; set; } = "Student";
        
        [Column("profile_picture")]
        public string? profile_picture { get; set; }
    }
}
