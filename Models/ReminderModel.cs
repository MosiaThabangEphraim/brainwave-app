using System.ComponentModel;

namespace BrainWave.APP.Models;
public class ReminderModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    public int ReminderID { get; set; }
    
    private string _title = string.Empty;
    public string Title 
    { 
        get => _title; 
        set 
        { 
            _title = value; 
            OnPropertyChanged(nameof(Title));
        } 
    }
    
    private string? _description;
    public string? Description 
    { 
        get => _description; 
        set 
        { 
            _description = value; 
            OnPropertyChanged(nameof(Description));
        } 
    }
    
    private DateTime _reminderTime = DateTime.Today.AddHours(9);
    public DateTime ReminderTime 
    { 
        get => _reminderTime; 
        set 
        { 
            _reminderTime = value; 
            OnPropertyChanged(nameof(ReminderTime));
        } 
    }
    
    public bool IsCompleted { get; set; } = false;
    public int UserID { get; set; }
    public int TaskID { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Legacy properties for backward compatibility
    public int Id 
    { 
        get => ReminderID; 
        set => ReminderID = value; 
    }
    public string TaskName 
    { 
        get => Title; 
        set => Title = value; 
    }
    public DateTime ReminderDate 
    { 
        get => ReminderTime.Date; 
        set 
        { 
            ReminderTime = value.Date + ReminderTime.TimeOfDay; 
            OnPropertyChanged(nameof(ReminderDate));
            OnPropertyChanged(nameof(ReminderTime));
        } 
    }
    public TimeSpan ReminderTimeSpan 
    { 
        get => _reminderTime.TimeOfDay; 
        set 
        { 
            _reminderTime = _reminderTime.Date + value; 
            OnPropertyChanged(nameof(ReminderTimeSpan));
            OnPropertyChanged(nameof(ReminderTime));
        } 
    }
}