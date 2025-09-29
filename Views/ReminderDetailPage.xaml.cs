using BrainWave.APP.Models;
using BrainWave.APP.ViewModels;
using BrainWave.APP.Services;

namespace BrainWave.APP.Views;

public partial class ReminderDetailPage : ContentPage
{
    private readonly ReminderDetailViewModel _vm;
    
    public ReminderDetailPage()
    {
        InitializeComponent();
        _vm = new ReminderDetailViewModel(new DatabaseService(new SupabaseService()));
        BindingContext = _vm;
    }

    public ReminderDetailPage(ReminderModel reminder) : this()
    {
        if (_vm != null)
        {
            _vm.SetReminder(reminder);
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        System.Diagnostics.Debug.WriteLine($"ReminderDetailPage: OnAppearing called. CurrentState.Location: {Shell.Current.CurrentState.Location?.OriginalString}");

        // Parse the query string to get the reminder data
        if (Shell.Current.CurrentState.Location is not null)
        {
            var uri = new Uri(Shell.Current.CurrentState.Location.OriginalString);
            var queryParams = System.Web.HttpUtility.ParseQueryString(uri.Query);
            var reminderJson = queryParams["Reminder"];

            if (!string.IsNullOrEmpty(reminderJson))
            {
                try
                {
                    var reminder = System.Text.Json.JsonSerializer.Deserialize<ReminderModel>(reminderJson);
                    if (reminder != null)
                    {
                        _vm.SetReminder(reminder);
                        System.Diagnostics.Debug.WriteLine($"ReminderDetailPage: Reminder loaded successfully: {reminder.Title}");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"ReminderDetailPage: Error deserializing reminder: {ex.Message}");
                    Shell.Current.DisplayAlert("Error", $"Failed to load reminder details: {ex.Message}", "OK");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("ReminderDetailPage: No reminder data found in query string.");
            }
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("ReminderDetailPage: Shell.Current.CurrentState.Location is null.");
        }
        
        // Initialize Supabase service
        _vm.GetSupabaseService()?.InitializeAsync();
    }

    private async void EditTitle_Clicked(object sender, EventArgs e)
    {
        if (_vm?.Reminder != null)
        {
            await _vm.EditFieldAsync("Title", _vm.Reminder.Title, async (newValue) => 
            {
                await _vm.UpdateReminderFieldAsync("title", newValue);
            });
        }
    }

    private async void EditDescription_Clicked(object sender, EventArgs e)
    {
        if (_vm?.Reminder != null)
        {
            await _vm.EditFieldAsync("Description", _vm.Reminder.Description, async (newValue) => 
            {
                await _vm.UpdateReminderFieldAsync("description", newValue);
            });
        }
    }

    private void ReminderDatePicker_DateSelected(object sender, DateChangedEventArgs e)
    {
        // Update the reminder time with the new date
        if (_vm?.Reminder != null)
        {
            var currentTime = _vm.Reminder.ReminderTime.TimeOfDay;
            _vm.Reminder.ReminderTime = e.NewDate.Date + currentTime;
        }
    }

    private void ReminderTimePicker_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "Time" && sender is TimePicker timePicker && _vm?.Reminder != null)
        {
            // Update the reminder time with the new time
            var currentDate = _vm.Reminder.ReminderTime.Date;
            _vm.Reminder.ReminderTime = currentDate + timePicker.Time;
        }
    }

    private async void ConfirmUpdate_Clicked(object sender, EventArgs e)
    {
        if (_vm != null)
        {
            await _vm.ConfirmUpdateAsync();
        }
    }

    private async void DeleteReminder_Clicked(object sender, EventArgs e)
    {
        if (_vm != null)
        {
            await _vm.DeleteReminderAsync();
        }
    }
}
