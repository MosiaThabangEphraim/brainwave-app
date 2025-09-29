using BrainWave.APP.ViewModels;
using BrainWave.APP.Models;

namespace BrainWave.APP.Views
{
    [QueryProperty(nameof(User), "User")]
    public partial class AdminUserDetailsPage : ContentPage
    {
        private readonly AdminUserDetailsViewModel _vm;
        
        public AdminUserDetailsPage(AdminUserDetailsViewModel vm = null)
        {
            InitializeComponent();
            _vm = vm ?? new AdminUserDetailsViewModel();
            BindingContext = _vm;
        }

        public AdminUserDto User
        {
            set
            {
                if (value != null)
                {
                    _vm.SetUser(value);
                }
            }
        }

        private async void EditFirstName_Clicked(object sender, EventArgs e)
        {
            await _vm.EditFieldAsync("First Name", _vm.User.F_Name, async (newValue) => 
            {
                await _vm.UpdateUserFieldAsync("f_name", newValue);
            });
        }

        private async void EditLastName_Clicked(object sender, EventArgs e)
        {
            await _vm.EditFieldAsync("Last Name", _vm.User.L_Name, async (newValue) => 
            {
                await _vm.UpdateUserFieldAsync("l_name", newValue);
            });
        }

        private async void EditEmail_Clicked(object sender, EventArgs e)
        {
            await _vm.EditFieldAsync("Email", _vm.User.Email, async (newValue) => 
            {
                await _vm.UpdateUserFieldAsync("email", newValue);
            });
        }

        private async void EditRole_Clicked(object sender, EventArgs e)
        {
            await _vm.EditRoleAsync();
        }

        private async void EditPassword_Clicked(object sender, EventArgs e)
        {
            await _vm.EditPasswordAsync();
        }

        private async void ConfirmUpdate_Clicked(object sender, EventArgs e)
        {
            await _vm.ConfirmUpdateAsync();
        }

        private async void DeleteUser_Clicked(object sender, EventArgs e)
        {
            var result = await DisplayAlert("Delete User", "Are you sure you want to delete this user?", "Yes", "No");
            if (result)
            {
                await _vm.DeleteUserAsync();
            }
        }

        private async void Back_Clicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("admin/users");
        }
    }
}
