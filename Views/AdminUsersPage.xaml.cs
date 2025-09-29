using BrainWave.APP.ViewModels;
using BrainWave.APP.Models;

namespace BrainWave.APP.Views
{
    public partial class AdminUsersPage : ContentPage
    {
        private readonly AdminUsersViewModel _vm;
        
        public AdminUsersPage(AdminUsersViewModel vm = null)
        {
            InitializeComponent();
            _vm = vm ?? new AdminUsersViewModel();
            BindingContext = _vm;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _vm.LoadUsersAsync();
        }

        private async void Back_Clicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("admin/dashboard");
        }

        private async void AddUser_Clicked(object sender, EventArgs e)
        {
            await _vm.AddUserAsync();
        }

        private async void ViewUser_Clicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is AdminUserDto user)
            {
                await _vm.ViewUserAsync(user);
            }
        }



        private async void DeleteUser_Clicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is AdminUserDto user)
            {
                await _vm.DeleteUserAsync(user);
            }
        }
    }
}