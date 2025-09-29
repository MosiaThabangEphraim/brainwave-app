using BrainWave.APP.Models;
using BrainWave.Maui.Models;
using BrainWave.APP.ViewModels;
using Microsoft.Maui.Controls;

namespace BrainWave.APP.Views
{
    public partial class AdminTasksPage : ContentPage
    {
        private readonly AdminTasksViewModel _vm;
        public AdminTasksPage(AdminTasksViewModel vm) { InitializeComponent(); BindingContext = _vm = vm; }
        protected override async void OnAppearing() { base.OnAppearing(); await _vm.RefreshAsync(); }
        private async void Search_Clicked(object s, EventArgs e) => await _vm.RefreshAsync();
        private void Edit_Clicked(object s, EventArgs e)
        {
            if ((s as Button)?.CommandParameter is AdminTaskDto t)
            {
                _vm.Editing = new AdminTaskDto
                {
                    TaskID = t.TaskID,
                    UserID = t.UserID,
                    UserName = t.UserName,
                    Description = t.Description,
                    Title = t.Title,
                    Due_Date = t.Due_Date,
                    Task_Status = t.Task_Status,
                    Priority_Level = t.Priority_Level
                }; BindingContext = null; BindingContext = _vm;
            }
        }
        private async void Save_Clicked(object s, EventArgs e) => await _vm.UpdateAsync();
        private async void Delete_Invoked(object s, EventArgs e)
        { if ((s as SwipeItem)?.BindingContext is AdminTaskDto t) await _vm.DeleteAsync(t); }
    }
}