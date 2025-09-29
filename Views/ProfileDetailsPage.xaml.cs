using BrainWave.APP.ViewModels;

namespace BrainWave.APP.Views
{
    public partial class ProfileDetailsPage : ContentPage
    {
        private readonly ProfileViewModel _vm;
        
        public ProfileDetailsPage()
        {
            InitializeComponent();
            _vm = new ProfileViewModel();
            BindingContext = _vm;
        }

        public ProfileDetailsPage(ProfileViewModel vm)
        {
            InitializeComponent();
            _vm = vm ?? new ProfileViewModel();
            BindingContext = _vm;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            
            // Initialize SupabaseService if needed
            if (_vm != null)
            {
                // Get the SupabaseService from the DatabaseService
                var supabaseService = _vm.GetSupabaseService();
                if (supabaseService != null)
                {
                    await supabaseService.InitializeAsync();
                }
                
                await _vm.LoadProfileAsync();
            }
        }

        private async void EditFirstName_Clicked(object sender, EventArgs e)
        {
            await _vm.EditFieldAsync("First Name", _vm.Me?.F_Name ?? "", async (newValue) => 
            {
                _vm.Me.F_Name = newValue;
            });
        }

        private async void EditLastName_Clicked(object sender, EventArgs e)
        {
            await _vm.EditFieldAsync("Last Name", _vm.Me?.L_Name ?? "", async (newValue) => 
            {
                _vm.Me.L_Name = newValue;
            });
        }

        private async void EditEmail_Clicked(object sender, EventArgs e)
        {
            await _vm.EditFieldAsync("Email", _vm.Me?.Email ?? "", async (newValue) => 
            {
                _vm.Me.Email = newValue;
            });
        }

        private async void EditRole_Clicked(object sender, EventArgs e)
        {
            await _vm.EditRoleAsync();
        }

        private async void ChangePhoto_Clicked(object sender, EventArgs e)
        {
            await _vm.ChangePhotoAsync();
        }

        private async void SaveChanges_Clicked(object sender, EventArgs e)
        {
            await _vm.UpdateAsync();
        }

        private async void ChangePassword_Clicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("PasswordUpdatePage");
        }

        private async void Back_Clicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}
