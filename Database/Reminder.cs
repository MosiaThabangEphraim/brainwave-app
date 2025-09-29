using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace BrainWave.APP.Database
{
    [Table("reminder")]
    public class Reminder : BaseModel
    {
        [PrimaryKey("reminderid")]
        public int reminderid { get; set; }
        
        [Column("taskid")]
        public int taskid { get; set; }
        
        [Column("reminder_type")]
        public string reminder_type { get; set; } = "Email";
        
        [Column("notify_time")]
        public DateTime notify_time { get; set; }
    }
}
