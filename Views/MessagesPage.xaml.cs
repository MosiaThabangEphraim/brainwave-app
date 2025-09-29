using BrainWave.APP.ViewModels;

namespace BrainWave.APP.Views;

[QueryProperty(nameof(CollaborationId), "CollaborationId")]
[QueryProperty(nameof(CollaborationName), "CollaborationName")]
public partial class MessagesPage : ContentPage
{
    private MessagesViewModel _vm;
    private int _collaborationId;
    private string _collaborationName = string.Empty;
    private bool _isInitialized = false;

    public MessagesPage()
    {
        InitializeComponent();
        _vm = (MessagesViewModel)BindingContext;
    }

    public int CollaborationId
    {
        set
        {
            System.Diagnostics.Debug.WriteLine($"🔍 MessagesPage: CollaborationId setter called with value: {value}");
            System.Diagnostics.Debug.WriteLine($"🔍 MessagesPage: Previous _collaborationId was: {_collaborationId}");
            
            // If we're switching to a different collaboration, reset initialization
            if (_collaborationId != value)
            {
                System.Diagnostics.Debug.WriteLine($"🔍 MessagesPage: Switching collaboration from {_collaborationId} to {value}, resetting initialization");
                _isInitialized = false;
            }
            
            _collaborationId = value;
            System.Diagnostics.Debug.WriteLine($"🔍 MessagesPage: New _collaborationId is: {_collaborationId}");
            TryInitialize();
        }
    }

    public string CollaborationName
    {
        set
        {
            System.Diagnostics.Debug.WriteLine($"🔍 MessagesPage: CollaborationName setter called with value: '{value}'");
            System.Diagnostics.Debug.WriteLine($"🔍 MessagesPage: Previous _collaborationName was: '{_collaborationName}'");
            
            // If we're switching to a different collaboration name, reset initialization
            if (_collaborationName != value)
            {
                System.Diagnostics.Debug.WriteLine($"🔍 MessagesPage: Switching collaboration name from '{_collaborationName}' to '{value}', resetting initialization");
                _isInitialized = false;
            }
            
            _collaborationName = value ?? string.Empty;
            System.Diagnostics.Debug.WriteLine($"🔍 MessagesPage: New _collaborationName is: '{_collaborationName}'");
            TryInitialize();
        }
    }

    private void TryInitialize()
    {
        if (_collaborationId > 0 && !string.IsNullOrEmpty(_collaborationName) && _vm != null && !_isInitialized)
        {
            System.Diagnostics.Debug.WriteLine($"🔍 MessagesPage: TryInitialize called - ID: {_collaborationId}, Name: '{_collaborationName}'");
            _isInitialized = true;
            
            // Use Task.Run to ensure this runs asynchronously and doesn't block the UI thread
            Task.Run(async () =>
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine($"🔍 MessagesPage: About to call InitializeAsync with ID: {_collaborationId}, Name: '{_collaborationName}'");
                    await _vm.InitializeAsync(_collaborationId, _collaborationName);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ MessagesPage: Error in InitializeAsync: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"❌ MessagesPage: Stack trace: {ex.StackTrace}");
                }
            });
        }
        else
        {
            System.Diagnostics.Debug.WriteLine($"🔍 MessagesPage: TryInitialize skipped - ID: {_collaborationId}, Name: '{_collaborationName}', VM: {_vm != null}, Initialized: {_isInitialized}");
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        try
        {
            System.Diagnostics.Debug.WriteLine($"🔍 MessagesPage: OnAppearing called");
            // The QueryProperty attributes will handle parameter passing automatically
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ MessagesPage: Error in OnAppearing: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"❌ MessagesPage: Stack trace: {ex.StackTrace}");
        }
    }

    private async void BackButton_Clicked(object sender, EventArgs e)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("🔍 MessagesPage: Back button clicked, navigating back to collaborations");
            
            // Try multiple navigation approaches
            if (Shell.Current.Navigation.NavigationStack.Count > 1)
            {
                System.Diagnostics.Debug.WriteLine("🔍 MessagesPage: Using Navigation.PopAsync()");
                await Shell.Current.Navigation.PopAsync();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("🔍 MessagesPage: Using GoToAsync to collaborations");
                await Shell.Current.GoToAsync("///collab");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ MessagesPage: Error navigating back: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"❌ MessagesPage: Stack trace: {ex.StackTrace}");
            
            // Fallback navigation
            try
            {
                System.Diagnostics.Debug.WriteLine("🔍 MessagesPage: Trying fallback navigation to collaborations");
                await Shell.Current.GoToAsync("///collab");
            }
            catch (Exception fallbackEx)
            {
                System.Diagnostics.Debug.WriteLine($"❌ MessagesPage: Fallback navigation also failed: {fallbackEx.Message}");
                await Shell.Current.DisplayAlert("Error", "Failed to navigate back. Please use the tab bar to return to collaborations.", "OK");
            }
        }
    }
}
