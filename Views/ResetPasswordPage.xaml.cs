using BrainWave.APP.ViewModels;

namespace BrainWave.APP.Views
{
    public partial class ResetPasswordPage : ContentPage
    {
        private readonly ResetPasswordViewModel _viewModel;

        public ResetPasswordPage()
        {
            InitializeComponent();
            _viewModel = new ResetPasswordViewModel();
            BindingContext = _viewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            
            // Clear any existing email - user will enter it manually
            _viewModel.Email = string.Empty;
            System.Diagnostics.Debug.WriteLine("ResetPasswordPage: Email field cleared - user will enter manually");
        }
    }
}