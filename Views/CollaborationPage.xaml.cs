using BrainWave.APP.Models;
using BrainWave.APP.ViewModels;
using BrainWave.APP.Services;
using Microsoft.Maui.Controls;

namespace BrainWave.APP.Views
{
    public partial class CollaborationPage : ContentPage
    {
        private readonly CollaborationViewModel _vm;
        private CollaborationModel _currentCollaboration;
        
        public CollaborationPage()
        {
            InitializeComponent();
            _vm = new CollaborationViewModel(new DatabaseService(new SupabaseService()));
        BindingContext = _vm;
        }
        
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            System.Diagnostics.Debug.WriteLine("🔧 CollaborationPage: OnAppearing called.");
            
            // Initialize SupabaseService if needed
            if (_vm != null)
            {
                // Get the SupabaseService from the DatabaseService
                var supabaseService = _vm.GetSupabaseService();
                if (supabaseService != null)
                {
                    await supabaseService.InitializeAsync();
                    System.Diagnostics.Debug.WriteLine("🔧 CollaborationPage: SupabaseService initialized.");
                }
                
                await _vm.LoadAsync();
                System.Diagnostics.Debug.WriteLine("🔧 CollaborationPage: ViewModel LoadAsync completed.");
            }
        }
        
        // Popup methods
        public void ShowCollaborationPopup(CollaborationModel collaboration)
        {
            _currentCollaboration = collaboration;
            
            // Populate popup fields
            PopupCollaborationTitle.Text = collaboration.Name;
            PopupCollaborationDescription.Text = collaboration.Description ?? "";
            PopupUserRole.Text = collaboration.Role ?? "";
            PopupTaskTitle.Text = collaboration.TaskTitle ?? "No task assigned";
            
            // Show popup
            CollaborationPopup.IsVisible = true;
        }
        
        private void CloseCollaborationPopup_Clicked(object sender, EventArgs e)
        {
            CollaborationPopup.IsVisible = false;
            _currentCollaboration = null;
        }
        
        private void CloseCollaborationPopupOverlay_Clicked(object sender, EventArgs e)
        {
            CollaborationPopup.IsVisible = false;
            _currentCollaboration = null;
        }
        
        private async void UpdateCollaboration_Clicked(object sender, EventArgs e)
        {
            if (_currentCollaboration == null) return;
            
            try
            {
                // Update the collaboration with popup values
                _currentCollaboration.Name = PopupCollaborationTitle.Text;
                _currentCollaboration.Description = PopupCollaborationDescription.Text;
                _currentCollaboration.Role = PopupUserRole.Text;
                
                // Call the ViewModel's update method
                await _vm.UpdateCollaborationAsync(_currentCollaboration);
                
                // Close popup
                CollaborationPopup.IsVisible = false;
                _currentCollaboration = null;
                
                // Refresh the list
                await _vm.LoadAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to update collaboration: {ex.Message}", "OK");
            }
        }
        
        private async void LeaveCollaboration_Clicked(object sender, EventArgs e)
        {
            if (_currentCollaboration == null) return;
            
            var result = await DisplayAlert("Confirm Leave", 
                "Are you sure you want to leave this collaboration?", 
                "Yes", "No");
                
            if (result)
            {
                try
                {
                    await _vm.LeaveCollaborationAsync(_currentCollaboration);
                    CollaborationPopup.IsVisible = false;
                    _currentCollaboration = null;
                    
                    // Refresh the list
                    await _vm.LoadAsync();
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error", $"Failed to leave collaboration: {ex.Message}", "OK");
                }
            }
        }
        
        private async void HelpButton_Clicked(object sender, EventArgs e)
        {
            var helpText = @"💡 Collaborations Help

➕ Create - Create a new collaboration project

🔄 Refresh - Refresh the collaborations list to see latest changes

🚪 Logout - Sign out of your account

🔍 Search - Use the search bar to find specific collaborations

Sort By - Sort collaborations by Title

Sort Order - Choose Ascending or Descending order

Join - Join an existing collaboration using a token

View - View collaboration details and edit your role

Token - View the join token to share with others

Users - View all members in this collaboration

Delete - Delete this collaboration permanently (only for creators)

Leave - Leave this collaboration

💾 Update - Save changes to collaboration details

❌ Cancel - Close the collaboration details popup

Collaborations let you work together on tasks with other users!";
            
            await DisplayAlert("💡 Collaborations Help", helpText, "OK");
        }
        
        
    }
}