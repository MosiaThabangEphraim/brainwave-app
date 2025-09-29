using BrainWave.APP.ViewModels;

namespace BrainWave.APP.Views
{
    public partial class AdminCredentialsPage : ContentPage
    {
        private readonly AdminCredentialsViewModel _vm;
        
        public AdminCredentialsPage(AdminCredentialsViewModel vm = null)
        {
            InitializeComponent();
            _vm = vm ?? new AdminCredentialsViewModel();
            BindingContext = _vm;
        }

        private async void UpdateCredentials_Clicked(object sender, EventArgs e)
        {
            await _vm.UpdateCredentialsAsync();
        }

        private async void Cancel_Clicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("admin/dashboard");
        }

        private async void Back_Clicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("admin/dashboard");
        }
    }
}
