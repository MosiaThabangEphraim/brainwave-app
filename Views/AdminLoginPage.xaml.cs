using BrainWave.APP.ViewModels;
namespace BrainWave.APP.Views;
public partial class AdminLoginPage : ContentPage
{
    private readonly AdminLoginViewModel _vm;
    public AdminLoginPage(AdminLoginViewModel vm) { InitializeComponent(); BindingContext = _vm = vm; }
    private async void Login_Clicked(object s, EventArgs e) => await _vm.LoginAsync();
}