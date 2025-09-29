using System.ComponentModel;

namespace BrainWave.APP.Models;
public class CollaborationModel : INotifyPropertyChanged
{
    private int _collaborationID;
    private string _name = string.Empty;
    private string? _description;
    private int _taskID;
    private string _token = string.Empty;
    private string _role = string.Empty;
    private int _userID;
    private DateTime _createdAt;
    private DateTime _updatedAt;
    private string? _category;
    private string? _taskTitle;
    private string? _taskStatus;

    public int CollaborationID 
    { 
        get => _collaborationID; 
        set => SetProperty(ref _collaborationID, value); 
    }
    
    public string Name 
    { 
        get 
        {
            System.Diagnostics.Debug.WriteLine($"🔧 CollaborationModel.Name getter called - _name: '{_name}'");
            return _name;
        }
        set 
        {
            System.Diagnostics.Debug.WriteLine($"🔧 CollaborationModel.Name setter called - old: '{_name}', new: '{value}'");
            SetProperty(ref _name, value);
            System.Diagnostics.Debug.WriteLine($"🔧 CollaborationModel.Name setter completed - _name: '{_name}'");
        }
    }
    
    public string? Description 
    { 
        get 
        {
            System.Diagnostics.Debug.WriteLine($"🔧 CollaborationModel.Description getter called - _description: '{_description}'");
            return _description;
        }
        set 
        {
            System.Diagnostics.Debug.WriteLine($"🔧 CollaborationModel.Description setter called - old: '{_description}', new: '{value}'");
            SetProperty(ref _description, value);
            System.Diagnostics.Debug.WriteLine($"🔧 CollaborationModel.Description setter completed - _description: '{_description}'");
        }
    }
    
    public int TaskID 
    { 
        get => _taskID; 
        set => SetProperty(ref _taskID, value); 
    }
    
    public string Token 
    { 
        get => _token; 
        set => SetProperty(ref _token, value); 
    }
    
    public string Role 
    { 
        get 
        {
            System.Diagnostics.Debug.WriteLine($"🔧 CollaborationModel.Role getter called - _role: '{_role}'");
            return _role;
        }
        set 
        {
            System.Diagnostics.Debug.WriteLine($"🔧 CollaborationModel.Role setter called - old: '{_role}', new: '{value}'");
            SetProperty(ref _role, value);
            System.Diagnostics.Debug.WriteLine($"🔧 CollaborationModel.Role setter completed - _role: '{_role}'");
        }
    }
    
    public int UserID 
    { 
        get => _userID; 
        set => SetProperty(ref _userID, value); 
    }
    
    public DateTime CreatedAt 
    { 
        get => _createdAt; 
        set => SetProperty(ref _createdAt, value); 
    }
    
    public DateTime UpdatedAt 
    { 
        get => _updatedAt; 
        set => SetProperty(ref _updatedAt, value); 
    }
    
    public string? Category 
    { 
        get => _category; 
        set => SetProperty(ref _category, value); 
    }
    
    // Navigation properties
    public string? TaskTitle 
    { 
        get => _taskTitle; 
        set => SetProperty(ref _taskTitle, value); 
    }
    
    public string? TaskStatus 
    { 
        get => _taskStatus; 
        set => SetProperty(ref _taskStatus, value); 
    }
    
    // Legacy property for backward compatibility
    public int Id 
    { 
        get => CollaborationID; 
        set => CollaborationID = value; 
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetProperty<T>(ref T backingStore, T value, [System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(backingStore, value))
            return false;

        backingStore = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}