using BrainWave.APP.ViewModels;

namespace BrainWave.APP.Views;

public partial class CreateTaskPage : ContentPage
{
    public CreateTaskPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        // Initialize the ViewModel
        if (BindingContext is TasksViewModel viewModel)
        {
            await viewModel.RefreshAsync();
        }
    }
    
    private void DueDatePicker_DateSelected(object sender, DateChangedEventArgs e)
    {
        if (BindingContext is TasksViewModel viewModel && viewModel.Editing != null)
        {
            // Ensure the selected date is treated as local time
            var localDate = DateTime.SpecifyKind(e.NewDate, DateTimeKind.Local);
            viewModel.Editing.DueDate = localDate;
        }
    }
}

