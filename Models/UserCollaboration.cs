using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace BrainWave.APP.Models
{
    [Table("user_collaboration")]
    public class UserCollaboration : BaseModel
    {
        [PrimaryKey("userid")]
        public int userid { get; set; }

        [PrimaryKey("collaborationid")]
        public int collaborationid { get; set; }

        [Column("collaboration_role")]
        public string collaboration_role { get; set; } = string.Empty;
    }
}
