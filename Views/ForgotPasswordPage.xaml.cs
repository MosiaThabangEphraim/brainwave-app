using BrainWave.APP.ViewModels;

namespace BrainWave.APP.Views
{
    public partial class ForgotPasswordPage : ContentPage
    {
        private readonly ForgotPasswordViewModel _viewModel;

        public ForgotPasswordPage()
        {
            InitializeComponent();
            _viewModel = new ForgotPasswordViewModel();
            BindingContext = _viewModel;
        }
    }
}

