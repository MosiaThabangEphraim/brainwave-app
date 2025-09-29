using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace BrainWave.APP.Database
{
    [Table("user_badge")]
    public class UserBadge : BaseModel
    {
        [Column("badgeid")]
        public int badgeid { get; set; }
        
        [Column("userid")]
        public int userid { get; set; }
        
        [Column("date_earned")]
        public DateTime date_earned { get; set; }
    }
}


