using BrainWave.APP.ViewModels;

namespace BrainWave.APP.Views
{
    public partial class AdminDashboardPage : ContentPage
    {
        private readonly AdminDashboardViewModel _vm;
        
        public AdminDashboardPage(AdminDashboardViewModel vm)
        {
            InitializeComponent();
            BindingContext = _vm = vm;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _vm.LoadDataAsync();
        }

        private async void ManageUsers_Clicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("admin/users");
        }

        private async void UpdateAdminCredentials_Clicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("admin/credentials");
        }

        private async void AddUser_Clicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("admin/users");
        }

        private async void ViewAllUsers_Clicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("admin/users");
        }

        private async void RefreshData_Clicked(object sender, EventArgs e)
        {
            await _vm.LoadDataAsync();
        }

        private async void Logout_Clicked(object sender, EventArgs e)
        {
            var result = await DisplayAlert("Logout", "Are you sure you want to logout?", "Yes", "No");
            if (result)
            {
                await _vm.LogoutAsync();
            }
        }
    }
}
