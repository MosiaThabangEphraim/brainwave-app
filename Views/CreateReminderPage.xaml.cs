using BrainWave.APP.ViewModels;
using BrainWave.APP.Services;

namespace BrainWave.APP.Views;

public partial class CreateReminderPage : ContentPage
{
    private readonly RemindersViewModel _vm;

    public CreateReminderPage()
    {
        InitializeComponent();
        
        // Manually create services and ViewModel (same pattern as TasksPage)
        var supabaseService = new SupabaseService();
        var databaseService = new DatabaseService(supabaseService);
        var notificationService = new NotificationService();
        _vm = new RemindersViewModel(databaseService, notificationService, supabaseService);
        
        BindingContext = _vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        // Initialize SupabaseService if needed
        if (_vm != null)
        {
            // Get the SupabaseService from the ViewModel
            var supabaseService = _vm.GetSupabaseService();
            if (supabaseService != null)
            {
                await supabaseService.InitializeAsync();
            }
            
            await _vm.LoadAsync();
        }
        
        // Attach event handlers for time picker
        if (ReminderTimePicker != null)
        {
            ReminderTimePicker.PropertyChanged += ReminderTimePicker_PropertyChanged;
            ReminderTimePicker.Focused += ReminderTimePicker_Focused;
            ReminderTimePicker.Unfocused += ReminderTimePicker_Unfocused;
        }
    }
    
    private void ReminderTimePicker_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "Time" && sender is TimePicker timePicker && _vm?.Editing != null)
        {
            // Update the reminder time with the new time
            var currentDate = _vm.Editing.ReminderTime.Date;
            _vm.Editing.ReminderTime = currentDate + timePicker.Time;
        }
    }
    
    private void ReminderDatePicker_DateSelected(object sender, DateChangedEventArgs e)
    {
        if (_vm?.Editing != null)
        {
            // Update the reminder time with the new date
            var currentTime = _vm.Editing.ReminderTime.TimeOfDay;
            _vm.Editing.ReminderTime = e.NewDate.Date + currentTime;
        }
    }
    
    private void ReminderTimePicker_Focused(object sender, FocusEventArgs e)
    {
        // Handle focus event if needed
    }
    
    private void ReminderTimePicker_Unfocused(object sender, FocusEventArgs e)
    {
        if (sender is TimePicker timePicker && _vm?.Editing != null)
        {
            var currentDate = _vm.Editing.ReminderTime.Date;
            _vm.Editing.ReminderTime = currentDate + timePicker.Time;
        }
    }
    
    
}

