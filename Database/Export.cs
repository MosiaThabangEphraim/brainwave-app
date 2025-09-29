using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace BrainWave.APP.Database
{
    [Table("export")]
    public class Export : BaseModel
    {
        [PrimaryKey("exportid")]
        public int exportid { get; set; }
        
        [Column("userid")]
        public int userid { get; set; }
        
        [Column("taskid")]
        public int taskid { get; set; }
        
        [Column("export_format")]
        public string export_format { get; set; } = string.Empty;
        
        [Column("date_requested")]
        public DateTime date_requested { get; set; }
    }
}








