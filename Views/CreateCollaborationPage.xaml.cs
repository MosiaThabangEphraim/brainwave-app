using BrainWave.APP.ViewModels;

namespace BrainWave.APP.Views;

public partial class CreateCollaborationPage : ContentPage
{
    private readonly CollaborationViewModel _viewModel;

    public CreateCollaborationPage()
    {
        InitializeComponent();
        _viewModel = (CollaborationViewModel)BindingContext;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadAsync();
    }

    private async void BackButton_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("///collab");
    }
}