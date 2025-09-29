using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace BrainWave.APP.Database
{
    [Table("message")]
    public class Message : BaseModel
    {
        [PrimaryKey("messageid")]
        public int messageid { get; set; }

        [Column("collaborationid")]
        public int collaborationid { get; set; }

        [Column("userid")]
        public int userid { get; set; }

        [Column("content")]
        public string content { get; set; } = string.Empty;

        [Column("sent_at")]
        public DateTime sent_at { get; set; }
    }
}
