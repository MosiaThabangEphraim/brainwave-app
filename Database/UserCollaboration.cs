using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace BrainWave.APP.Database
{
    [Table("user_collaboration")]
    public class UserCollaboration : BaseModel
    {
        [Column("userid")]
        public int userid { get; set; }
        
        [Column("collaborationid")]
        public int collaborationid { get; set; }
        
        [Column("collaboration_role")]
        public string collaboration_role { get; set; } = string.Empty;
    }
}


