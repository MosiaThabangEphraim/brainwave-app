namespace BrainWave.APP.Models;
public class TaskDtos
{
    public int TaskID { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime DueDate { get; set; }
    public string Status { get; set; } = "In Progress";
    public string Priority { get; set; } = "Medium";
    public int UserID { get; set; }
    public string? Category { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Legacy properties for backward compatibility
    public DateTime? Due_Date 
    { 
        get => DueDate; 
        set => DueDate = value ?? DateTime.Now; 
    }
    public string? Task_Status 
    { 
        get => Status; 
        set => Status = value ?? "In Progress"; 
    }
    public string? Priority_Level 
    { 
        get => Priority; 
        set => Priority = value ?? "Medium"; 
    }
}

public class AdminTaskDto : TaskDtos
{
    public int UserID { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string PriorityColor => Priority_Level switch
    {
        "High" => "#FF0000",
        "Medium" => "#FFA500",
        "Low" => "#008000",
        _ => "#808080"
    };
    public string StatusColor => Task_Status switch
    {
        "In Progress" => "#2196F3",
        "Completed" => "#4CAF50",
        _ => "#808080"
    };
}