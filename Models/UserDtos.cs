using System.ComponentModel;

namespace BrainWave.APP.Models;
public class UserDtos : INotifyPropertyChanged
{
    private int _userID;
    private string _f_Name = string.Empty;
    private string _l_Name = string.Empty;
    private string _email = string.Empty;
    private string? _role;
    private string? _profile_Picture;

    public int UserID 
    { 
        get => _userID; 
        set { _userID = value; OnPropertyChanged(); }
    }
    
    public string F_Name 
    { 
        get => _f_Name; 
        set { _f_Name = value; OnPropertyChanged(); }
    }
    
    public string L_Name 
    { 
        get => _l_Name; 
        set { _l_Name = value; OnPropertyChanged(); }
    }
    
    public string Email 
    { 
        get => _email; 
        set { _email = value; OnPropertyChanged(); }
    }
    
    public string? Role 
    { 
        get => _role; 
        set { _role = value; OnPropertyChanged(); }
    }
    
    public string? Profile_Picture 
    { 
        get => _profile_Picture; 
        set { _profile_Picture = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class AdminUserDto : UserDtos
{
    public int TaskCount { get; set; }
    public string FullName => $"{F_Name} {L_Name}";
}

public class AdminLoginResponse
{
    public string Token { get; set; } = "";
    public string Role { get; set; } = "";
    public string Message { get; set; } = "";
}

public class TopUserDto
{
    public int UserID { get; set; }
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Role { get; set; } = "";
    public int CompletedTasks { get; set; }
    public int TotalTasks { get; set; }
    public double CompletionRate => TotalTasks > 0 ? (double)CompletedTasks / TotalTasks * 100 : 0;
}