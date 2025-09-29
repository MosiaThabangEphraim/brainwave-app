using BrainWave.APP.ViewModels;
using BrainWave.APP.Services;

namespace BrainWave.APP.Views;

public partial class PasswordUpdatePage : ContentPage
{
    private readonly PasswordUpdateViewModel _vm;
    
    public PasswordUpdatePage()
    {
        InitializeComponent();
        _vm = new PasswordUpdateViewModel();
        BindingContext = _vm;
    }
    
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        // Initialize SupabaseService if needed
        if (_vm != null)
        {
            var supabaseService = _vm.GetSupabaseService();
            if (supabaseService != null)
            {
                await supabaseService.InitializeAsync();
            }
        }
    }
}
