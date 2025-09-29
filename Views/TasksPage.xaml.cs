using BrainWave.APP.Models;       // ✅ Correct namespace for TaskDtos
using BrainWave.APP.Services;
using BrainWave.APP.ViewModels;
using Microsoft.Maui.Controls;

namespace BrainWave.APP.Views
{
    public partial class TasksPage : ContentPage
    {
        private readonly TasksViewModel _vm;

        // Default constructor (manual instantiation)
        public TasksPage()
        {
            InitializeComponent();

            // Manually create DatabaseService and ViewModel
            var supabaseService = new SupabaseService();
            var databaseService = new DatabaseService(supabaseService);
            _vm = new TasksViewModel(databaseService, supabaseService);

            BindingContext = _vm;
        }

        // Constructor for Dependency Injection
        public TasksPage(TasksViewModel vm)
        {
            InitializeComponent();
            _vm = vm;
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
                
                await _vm.RefreshAsync();
            }
        }

        private async void Refresh_Clicked(object s, EventArgs e)
            => await _vm.RefreshAsync();





        private async void Update_Clicked(object s, EventArgs e)
        {
            if (_vm != null)
                await _vm.UpdateAsync();
        }

        private async void SwipeItem_Invoked(object s, EventArgs e)
        {
            if ((s as SwipeItem)?.BindingContext is TaskDtos item)
                await _vm.DeleteAsync(item);
        }
        
        private async void HelpButton_Clicked(object sender, EventArgs e)
        {
            var helpText = @"💡 Tasks Help

➕ Create - Create a new task to track your work

🔄 Refresh - Refresh the task list to see latest changes

🚪 Logout - Sign out of your account

View - View task details and edit them

Complete - Mark this task as completed (only shows for In Progress tasks)

Export - Export this task to a file

Create Task - Create a new task with the entered details

Update Task - Update the existing task with new details

Back to Tasks - Return to the task list view

Use the search bar and filters to find specific tasks quickly!";
            
            await DisplayAlert("💡 Tasks Help", helpText, "OK");
        }
        
    }
}
