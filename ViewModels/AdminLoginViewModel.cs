using BrainWave.APP.Services;
namespace BrainWave.APP.ViewModels;
public class AdminLoginViewModel : BaseViewModel
{
    private readonly AuthenticationService _auth;
    private readonly NavigationService _nav;
    
    public string Username { get; set; } = "admin"; // default admin username
    public string Password { get; set; } = "admin123!";

    // Default constructor for XAML binding
    public AdminLoginViewModel()
    {
        // This will be resolved by dependency injection
        _auth = new AuthenticationService(new SupabaseService());
        _nav = new NavigationService();
    }

    public AdminLoginViewModel(AuthenticationService auth, NavigationService nav) : base()
    {
        _auth = auth;
        _nav = nav;
    }

    public async Task LoginAsync()
    {
        var success = await _auth.AdminLoginAsync(Username, Password);
        if (success && _auth.IsAdmin)
            await _nav.GoAsync("///admin/users");
    }
}