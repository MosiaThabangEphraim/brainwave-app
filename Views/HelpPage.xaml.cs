using BrainWave.APP.ViewModels;
using BrainWave.APP.Services;

namespace BrainWave.APP.Views;

public partial class HelpPage : ContentPage
{
    private readonly HelpViewModel _viewModel;

    public HelpPage()
    {
        InitializeComponent();
        _viewModel = new HelpViewModel();
        BindingContext = _viewModel;
    }

    private async void SendMessage_Clicked(object sender, EventArgs e)
    {
        System.Diagnostics.Debug.WriteLine("SendMessage_Clicked event handler called!");
        await _viewModel.SubmitContactAsync();
    }
}



