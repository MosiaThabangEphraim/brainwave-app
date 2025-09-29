using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace BrainWave.APP.Models
{
    [Table("user_badge")]
    public class UserBadge : BaseModel
    {
        [PrimaryKey("badgeid")]
        public int badgeid { get; set; }

        [PrimaryKey("userid")]
        public int userid { get; set; }

        [Column("date_earned")]
        public DateTime date_earned { get; set; }
    }
}
