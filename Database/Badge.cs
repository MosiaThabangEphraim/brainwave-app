using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace BrainWave.APP.Database
{
    [Table("badge")]
    public class Badge : BaseModel
    {
        [PrimaryKey("badgeid")]
        public int badgeid { get; set; }
        
        [Column("badge_type")]
        public string badge_type { get; set; } = string.Empty;
        
        [Column("badge_description")]
        public string badge_description { get; set; } = string.Empty;
    }
}








