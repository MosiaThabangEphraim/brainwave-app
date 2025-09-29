using BrainWave.APP.Models;
using BrainWave.APP.ViewModels;
using BrainWave.APP.Services;

namespace BrainWave.APP.Views;

[QueryProperty(nameof(Task), "Task")]
public partial class TaskDetailPage : ContentPage
{
    private readonly TaskDetailViewModel _vm;
    
    public TaskDetailPage()
    {
        InitializeComponent();
        _vm = new TaskDetailViewModel(new DatabaseService(new SupabaseService()));
        BindingContext = _vm;
    }

    public TaskDetailPage(TaskDtos task) : this()
    {
        if (_vm != null)
        {
            _vm.SetTask(task);
        }
    }

    public TaskDtos Task
    {
        set
        {
            if (value != null)
            {
                _vm.SetTask(value);
            }
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        
        // Initialize Supabase service
        _vm.GetSupabaseService()?.InitializeAsync();
    }


    private async void EditTitle_Clicked(object sender, EventArgs e)
    {
        await _vm.EditFieldAsync("Title", _vm.Task.Title, async (newValue) => 
        {
            await _vm.UpdateTaskFieldAsync("title", newValue);
        });
    }

    private async void EditDescription_Clicked(object sender, EventArgs e)
    {
        await _vm.EditFieldAsync("Description", _vm.Task.Description, async (newValue) => 
        {
            await _vm.UpdateTaskFieldAsync("description", newValue);
        });
    }

    private async void EditDueDate_Clicked(object sender, EventArgs e)
    {
        await _vm.EditDueDateAsync();
    }

    private async void EditPriority_Clicked(object sender, EventArgs e)
    {
        await _vm.EditPriorityAsync();
    }

    private async void EditStatus_Clicked(object sender, EventArgs e)
    {
        await _vm.EditStatusAsync();
    }

    private async void ConfirmUpdate_Clicked(object sender, EventArgs e)
    {
        await _vm.ConfirmUpdateAsync();
    }

    private async void MarkComplete_Clicked(object sender, EventArgs e)
    {
        await _vm.MarkTaskCompletedAsync();
    }

    private async void DeleteTask_Clicked(object sender, EventArgs e)
    {
        var result = await DisplayAlert("Delete Task", "Are you sure you want to delete this task?", "Yes", "No");
        if (result)
        {
            await _vm.DeleteTaskAsync();
        }
    }
}
