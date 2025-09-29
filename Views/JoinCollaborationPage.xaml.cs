using BrainWave.APP.ViewModels;

namespace BrainWave.APP.Views;

public partial class JoinCollaborationPage : ContentPage
{
    private readonly CollaborationViewModel _viewModel;

    public JoinCollaborationPage()
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

    private async void CancelButton_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("///collab");
    }
}



