namespace BrainWave.Maui.Models;
public class ExportDtos
{
    public int ExportID { get; set; }
    public int UserID { get; set; }
    public int TaskID { get; set; }
    public string Export_Format { get; set; } = string.Empty;
    public DateTime Date_Requested { get; set; }
}
