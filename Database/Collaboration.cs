using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace BrainWave.APP.Database
{
    [Table("collaboration")]
    public class Collaboration : BaseModel
    {
        [PrimaryKey("collaborationid")]
        public int collaborationid { get; set; }
        
        [Column("taskid")]
        public int taskid { get; set; }
        
        [Column("collaboration_title")]
        public string collaboration_title { get; set; } = string.Empty;
        
        [Column("collaboration_description")]
        public string? collaboration_description { get; set; }
        
        [Column("collaboration_token")]
        public string collaboration_token { get; set; } = string.Empty;
    }
}

