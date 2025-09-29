using BrainWave.APP.ViewModels;
using BrainWave.APP.Services;
using BrainWave.APP.Models;
using System.Diagnostics;

namespace BrainWave.APP.Views;
public partial class RemindersPage : ContentPage
{
    private readonly RemindersViewModel _vm;
    private ReminderModel _currentReminder;
    
    public RemindersPage()
    {
        InitializeComponent();
        
        // Use the ViewModel from XAML binding context instead of creating a new one
        _vm = (RemindersViewModel)BindingContext;
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
            
            await _vm.LoadAsync();
        }
    }
    private async void Add_Clicked(object s, EventArgs e) => await _vm.CreateAsync();
    
    private void SetQuickTime_Clicked(object sender, EventArgs e)
    {
        if (sender is Button button)
        {
            var timeText = button.Text;
            TimeSpan timeSpan;
            
            switch (timeText)
            {
                case "9:00 AM":
                    timeSpan = new TimeSpan(9, 0, 0);
                    break;
                case "2:00 PM":
                    timeSpan = new TimeSpan(14, 0, 0);
                    break;
                case "6:00 PM":
                    timeSpan = new TimeSpan(18, 0, 0);
                    break;
                default:
                    return;
            }
            
            _vm.Editing.ReminderTimeSpan = timeSpan;
        }
    }
    
    // Popup methods
    public void ShowReminderPopup(ReminderModel reminder)
    {
        _currentReminder = reminder;
        
        // Populate popup fields (only date and time since title/description are removed)
        PopupDate.Date = reminder.ReminderTime.Date;
        PopupTime.Time = reminder.ReminderTime.TimeOfDay;
        
        // Show popup
        ReminderPopup.IsVisible = true;
    }
    
        private void ClosePopup_Clicked(object sender, EventArgs e)
        {
            ReminderPopup.IsVisible = false;
            _currentReminder = null;
        }
        
        private void ClosePopupOverlay_Clicked(object sender, EventArgs e)
        {
            ReminderPopup.IsVisible = false;
            _currentReminder = null;
        }
    
    private async void UpdateReminder_Clicked(object sender, EventArgs e)
    {
        if (_currentReminder == null) return;
        
        try
        {
            // Update the reminder with popup values (only time since title/description are removed)
            _currentReminder.ReminderTime = PopupDate.Date.Add(PopupTime.Time);
            
            // Call the ViewModel's update method
            await _vm.UpdateReminderAsync(_currentReminder);
            
            // Close popup
            ReminderPopup.IsVisible = false;
            _currentReminder = null;
            
            // Refresh the list
            await _vm.LoadAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to update reminder: {ex.Message}", "OK");
        }
    }
    
    private async void DeleteReminder_Clicked(object sender, EventArgs e)
    {
        if (_currentReminder == null) return;
        
        var result = await DisplayAlert("Confirm Delete", 
            "Are you sure you want to delete this reminder?", 
            "Yes", "No");
            
        if (result)
        {
            try
            {
                await _vm.DeleteReminderAsync(_currentReminder);
                ReminderPopup.IsVisible = false;
                _currentReminder = null;
                
                // Refresh the list
                await _vm.LoadAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to delete reminder: {ex.Message}", "OK");
            }
        }
    }
    
    private async void HelpButton_Clicked(object sender, EventArgs e)
    {
        var helpText = @"💡 Reminders Help

➕ Create - Create a new email reminder for a task

🔄 Refresh - Refresh the reminders list to see latest changes

🚪 Logout - Sign out of your account

View - View reminder details and edit the reminder time

Delete - Delete this reminder permanently

Search - Use the search bar to find specific reminders

Sort By - Sort reminders by Title or Reminder Time

Sort Order - Choose Ascending or Descending order

Quick Time Buttons - Set common reminder times (9:00 AM, 2:00 PM, 6:00 PM)

Reminders are sent via email to help you stay on track with your tasks!";
        
        await DisplayAlert("💡 Reminders Help", helpText, "OK");
    }
    
    
}