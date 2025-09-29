using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace BrainWave.APP.Models
{
    [Table("task")]
    public class TaskItem : BaseModel
    {
        [PrimaryKey("taskid")]
        public int taskid { get; set; }

        [Column("userid")]
        public int userid { get; set; }

        [Column("title")]
        public string title { get; set; } = string.Empty;

        [Column("description")]
        public string description { get; set; } = string.Empty;

        [Column("due_date")]
        public DateTime? due_date { get; set; }

        [Column("task_status")]
        public string task_status { get; set; } = string.Empty;

        [Column("priority_level")]
        public string priority_level { get; set; } = string.Empty;
    }
}
