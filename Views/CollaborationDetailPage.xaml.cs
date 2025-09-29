using BrainWave.APP.ViewModels;
using BrainWave.APP.Models;

namespace BrainWave.APP.Views
{
    public partial class CollaborationDetailPage : ContentPage
    {
        private readonly CollaborationDetailViewModel _vm;

        public CollaborationDetailPage()
        {
            InitializeComponent();
            _vm = new CollaborationDetailViewModel(new Services.DatabaseService(new Services.SupabaseService()));
            BindingContext = _vm;
        }

        public CollaborationDetailPage(CollaborationModel collaboration) : this()
        {
            if (_vm != null)
            {
                _vm.SetCollaboration(collaboration);
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            System.Diagnostics.Debug.WriteLine($"🔍 CollaborationDetailPage: OnAppearing called. CurrentState.Location: {Shell.Current.CurrentState.Location?.OriginalString}");
            System.Diagnostics.Debug.WriteLine($"🔍 CollaborationDetailPage: ViewModel is null: {_vm == null}");
            System.Diagnostics.Debug.WriteLine($"🔍 CollaborationDetailPage: BindingContext is null: {BindingContext == null}");

            if (Shell.Current.CurrentState.Location is not null)
            {
                var uri = new Uri(Shell.Current.CurrentState.Location.OriginalString);
                var queryParams = System.Web.HttpUtility.ParseQueryString(uri.Query);
                var collaborationJson = queryParams["collaboration"];

                System.Diagnostics.Debug.WriteLine($"🔍 CollaborationDetailPage: Query string: {uri.Query}");
                System.Diagnostics.Debug.WriteLine($"🔍 CollaborationDetailPage: Collaboration JSON from query: {collaborationJson}");

                if (!string.IsNullOrEmpty(collaborationJson))
                {
                    try
                    {
                        var collaboration = System.Text.Json.JsonSerializer.Deserialize<CollaborationModel>(collaborationJson);
                        if (collaboration != null)
                        {
                            System.Diagnostics.Debug.WriteLine($"🔍 CollaborationDetailPage: Deserialized collaboration - ID: {collaboration.CollaborationID}, Name: '{collaboration.Name}', Description: '{collaboration.Description}', Role: '{collaboration.Role}'");
                            System.Diagnostics.Debug.WriteLine($"🔍 CollaborationDetailPage: Collaboration properties - Name is null/empty: {string.IsNullOrEmpty(collaboration.Name)}, Description is null/empty: {string.IsNullOrEmpty(collaboration.Description)}, Role is null/empty: {string.IsNullOrEmpty(collaboration.Role)}");
                            
                            _vm.SetCollaboration(collaboration);
                            
                            // Check if the ViewModel has the collaboration after setting it
                            System.Diagnostics.Debug.WriteLine($"🔍 CollaborationDetailPage: After SetCollaboration - ViewModel.Collaboration is null: {_vm.Collaboration == null}");
                            if (_vm.Collaboration != null)
                            {
                                System.Diagnostics.Debug.WriteLine($"🔍 CollaborationDetailPage: ViewModel.Collaboration - Name: '{_vm.Collaboration.Name}', Description: '{_vm.Collaboration.Description}', Role: '{_vm.Collaboration.Role}'");
                            }
                            
                            System.Diagnostics.Debug.WriteLine($"🔍 CollaborationDetailPage: Collaboration loaded from query string: {collaboration.Name}");
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("🔍 CollaborationDetailPage: Deserialized collaboration is null.");
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"🔍 CollaborationDetailPage: Error deserializing collaboration: {ex.Message}");
                        System.Diagnostics.Debug.WriteLine($"🔍 CollaborationDetailPage: Stack trace: {ex.StackTrace}");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("🔍 CollaborationDetailPage: Collaboration parameter not found or is empty in query string.");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("🔍 CollaborationDetailPage: Shell.Current.CurrentState.Location is null.");
            }

            // Initialize Supabase service
            _vm.GetSupabaseService()?.InitializeAsync();
            
            // Additional debugging for UI binding
            System.Diagnostics.Debug.WriteLine($"🔍 CollaborationDetailPage: Final check - ViewModel.Collaboration is null: {_vm.Collaboration == null}");
            if (_vm.Collaboration != null)
            {
                System.Diagnostics.Debug.WriteLine($"🔍 CollaborationDetailPage: Final ViewModel.Collaboration - Name: '{_vm.Collaboration.Name}', Description: '{_vm.Collaboration.Description}', Role: '{_vm.Collaboration.Role}'");
            }
        }

        private async void EditTitle_Clicked(object sender, EventArgs e)
        {
            if (_vm?.Collaboration != null)
            {
                var currentValue = _vm.Collaboration.Name ?? "";
                var newValue = await DisplayPromptAsync("Edit Title", "Enter new collaboration title:", "Update", "Cancel", currentValue);
                if (!string.IsNullOrWhiteSpace(newValue) && newValue != currentValue)
                {
                    await _vm.UpdateCollaborationFieldAsync("title", newValue);
                }
            }
        }

        private async void EditDescription_Clicked(object sender, EventArgs e)
        {
            if (_vm?.Collaboration != null)
            {
                var currentValue = _vm.Collaboration.Description ?? "";
                var newValue = await DisplayPromptAsync("Edit Description", "Enter new collaboration description:", "Update", "Cancel", currentValue);
                if (!string.IsNullOrWhiteSpace(newValue) && newValue != currentValue)
                {
                    await _vm.UpdateCollaborationFieldAsync("description", newValue);
                }
            }
        }

        private async void EditRole_Clicked(object sender, EventArgs e)
        {
            if (_vm?.Collaboration != null)
            {
                var currentValue = _vm.Collaboration.Role ?? "";
                var newValue = await DisplayPromptAsync("Edit Role", "Enter your role in this collaboration:", "Update", "Cancel", currentValue);
                if (!string.IsNullOrWhiteSpace(newValue) && newValue != currentValue)
                {
                    await _vm.UpdateCollaborationFieldAsync("role", newValue);
                }
            }
        }

        private async void CopyToken_Clicked(object sender, EventArgs e)
        {
            if (_vm?.Collaboration != null)
            {
                await Clipboard.SetTextAsync(_vm.Collaboration.Token);
                await DisplayAlert("Token Copied", "Collaboration token has been copied to clipboard.", "OK");
            }
        }

        private async void SaveChanges_Clicked(object sender, EventArgs e)
        {
            if (_vm != null)
            {
                await _vm.SaveChangesAsync();
            }
        }

        private async void LeaveCollaboration_Clicked(object sender, EventArgs e)
        {
            if (_vm != null)
            {
                await _vm.LeaveCollaborationAsync();
            }
        }

        private async void DeleteCollaboration_Clicked(object sender, EventArgs e)
        {
            if (_vm != null)
            {
                await _vm.DeleteCollaborationAsync();
            }
        }
    }
}