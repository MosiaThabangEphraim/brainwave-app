using BrainWave.APP.Models;
using BrainWave.APP.Services;
using BrainWave.APP.Database;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace BrainWave.APP.ViewModels
{
    public class CollaborationDetailViewModel : BaseViewModel
    {
        private readonly DatabaseService _databaseService;
        private CollaborationModel _collaboration = new CollaborationModel();
        private TaskDtos _relatedTask = new TaskDtos();
        private ObservableCollection<CollaborationMember> _collaborationMembers = new ObservableCollection<CollaborationMember>();
        private bool _hasPendingChanges = false;
        private string _errorMessage = string.Empty;

        public CollaborationDetailViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));

            // Initialize commands
            BackCommand = new Command(async () => await BackAsync());
            ViewCommand = new Command(async () => await ViewCollaborationAsync());
            ViewUsersCommand = new Command(async () => await ViewUsersAsync());
        }

        public CollaborationModel Collaboration
        {
            get 
            {
                System.Diagnostics.Debug.WriteLine($"🔧 Collaboration property getter called - _collaboration is null: {_collaboration == null}");
                if (_collaboration != null)
                {
                    System.Diagnostics.Debug.WriteLine($"🔧 Collaboration property getter - Name: '{_collaboration.Name}', Description: '{_collaboration.Description}', Role: '{_collaboration.Role}'");
                }
                return _collaboration;
            }
            set 
            {
                System.Diagnostics.Debug.WriteLine($"🔧 Collaboration property setter called with: Name='{value?.Name}', Description='{value?.Description}', Role='{value?.Role}'");
                System.Diagnostics.Debug.WriteLine($"🔧 Collaboration property setter - value is null: {value == null}");
                System.Diagnostics.Debug.WriteLine($"🔧 Collaboration property setter - _collaboration before Set: {_collaboration == null}");
                
                Set(ref _collaboration, value);
                
                System.Diagnostics.Debug.WriteLine($"🔧 Collaboration property setter - _collaboration after Set: {_collaboration == null}");
                if (_collaboration != null)
                {
                    System.Diagnostics.Debug.WriteLine($"🔧 Collaboration property setter completed: Name='{_collaboration.Name}', Description='{_collaboration.Description}', Role='{_collaboration.Role}'");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("🔧 Collaboration property setter completed: _collaboration is null!");
                }
                
                // Force property change notifications for all collaboration properties
                System.Diagnostics.Debug.WriteLine("🔧 Collaboration property setter: Triggering property change notifications");
                OnPropertyChanged(nameof(Collaboration));
                if (_collaboration != null)
                {
                    OnPropertyChanged(nameof(Collaboration.Name));
                    OnPropertyChanged(nameof(Collaboration.Description));
                    OnPropertyChanged(nameof(Collaboration.Role));
                }
            }
        }

        public TaskDtos RelatedTask
        {
            get => _relatedTask;
            set => Set(ref _relatedTask, value);
        }

        public ObservableCollection<CollaborationMember> CollaborationMembers
        {
            get => _collaborationMembers;
            set => Set(ref _collaborationMembers, value);
        }

        public bool HasPendingChanges
        {
            get => _hasPendingChanges;
            set => Set(ref _hasPendingChanges, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => Set(ref _errorMessage, value);
        }

        public bool CanDeleteCollaboration => Collaboration.Role?.ToLower() == "owner";

        public string TaskInfo
        {
            get
            {
                if (RelatedTask?.TaskID > 0)
                {
                    return $"📋 {RelatedTask.Title}\n📅 Due: {RelatedTask.DueDate:MMM dd, yyyy}\n📊 Status: {RelatedTask.Status}";
                }
                return "No related task found";
            }
        }

        public ICommand BackCommand { get; }
        public ICommand ViewCommand { get; }
        public ICommand ViewUsersCommand { get; }

        public async void SetCollaboration(CollaborationModel collaboration)
        {
            System.Diagnostics.Debug.WriteLine($"🔧 SetCollaboration called with: Name='{collaboration?.Name}', Description='{collaboration?.Description}', Role='{collaboration?.Role}'");
            System.Diagnostics.Debug.WriteLine($"🔧 SetCollaboration - collaboration is null: {collaboration == null}");
            
            if (collaboration == null)
            {
                System.Diagnostics.Debug.WriteLine("🔧 SetCollaboration: collaboration parameter is null, returning early");
                return;
            }
            
            System.Diagnostics.Debug.WriteLine($"🔧 SetCollaboration: Before setting - _collaboration is null: {_collaboration == null}");
            
            Collaboration = collaboration;
            
            System.Diagnostics.Debug.WriteLine($"🔧 SetCollaboration: After setting - _collaboration is null: {_collaboration == null}");
            System.Diagnostics.Debug.WriteLine($"🔧 SetCollaboration: After setting - Collaboration property is null: {Collaboration == null}");
            
            if (Collaboration != null)
            {
                System.Diagnostics.Debug.WriteLine($"🔧 SetCollaboration: Collaboration set - Name='{Collaboration.Name}', Description='{Collaboration.Description}', Role='{Collaboration.Role}'");
                System.Diagnostics.Debug.WriteLine($"🔧 SetCollaboration: Collaboration properties - Name is null/empty: {string.IsNullOrEmpty(Collaboration.Name)}, Description is null/empty: {string.IsNullOrEmpty(Collaboration.Description)}, Role is null/empty: {string.IsNullOrEmpty(Collaboration.Role)}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("🔧 SetCollaboration: Collaboration property is null after setting!");
            }
            
            await LoadRelatedTaskAsync();
            await LoadCollaborationMembersAsync();
            
            // Force all property change notifications
            System.Diagnostics.Debug.WriteLine("🔧 SetCollaboration: Triggering property change notifications");
            OnPropertyChanged(nameof(Collaboration));
            if (Collaboration != null)
            {
                OnPropertyChanged(nameof(Collaboration.Name));
                OnPropertyChanged(nameof(Collaboration.Description));
                OnPropertyChanged(nameof(Collaboration.Role));
                OnPropertyChanged(nameof(Collaboration.Token));
            }
            OnPropertyChanged(nameof(TaskInfo));
            OnPropertyChanged(nameof(CanDeleteCollaboration));
            OnPropertyChanged(nameof(HasPendingChanges));
            
            System.Diagnostics.Debug.WriteLine($"🔧 SetCollaboration completed: Name='{Collaboration?.Name}', Description='{Collaboration?.Description}', Role='{Collaboration?.Role}'");
        }

        private async Task LoadRelatedTaskAsync()
        {
            try
            {
                if (Collaboration.TaskID > 0)
                {
                    var task = await _databaseService.GetTaskByIdAsync(Collaboration.TaskID);
                    if (task != null)
                    {
                        RelatedTask = new TaskDtos
                        {
                            TaskID = task.taskid,
                            Title = task.title,
                            Description = task.description,
                            DueDate = task.due_date.Kind == DateTimeKind.Utc ? task.due_date.ToLocalTime() : 
                                     task.due_date.Kind == DateTimeKind.Unspecified ? 
                                     DateTime.SpecifyKind(task.due_date, DateTimeKind.Local) : task.due_date,
                            Status = task.task_status,
                            UserID = task.userid
                        };
                        OnPropertyChanged(nameof(TaskInfo));
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading related task: {ex.Message}");
            }
        }

        private async Task LoadCollaborationMembersAsync()
        {
            try
            {
                IsBusy = true;
                var members = await _databaseService.GetCollaborationMembersAsync(Collaboration.CollaborationID);
                CollaborationMembers.Clear();
                
                foreach (var member in members)
                {
                    CollaborationMembers.Add(member);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading collaboration members: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", $"Failed to load collaboration members: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task UpdateCollaborationFieldAsync(string fieldName, string newValue)
        {
            try
            {
                switch (fieldName.ToLower())
                {
                    case "title":
                        Collaboration.Name = newValue;
                        OnPropertyChanged(nameof(Collaboration.Name));
                        break;
                    case "description":
                        Collaboration.Description = newValue;
                        OnPropertyChanged(nameof(Collaboration.Description));
                        break;
                    case "role":
                        Collaboration.Role = newValue;
                        OnPropertyChanged(nameof(Collaboration.Role));
                        break;
                    default:
                        await Shell.Current.DisplayAlert("Error", $"Unknown field: {fieldName}", "OK");
                        return;
                }

                HasPendingChanges = true;
                OnPropertyChanged(nameof(Collaboration));
                OnPropertyChanged(nameof(HasPendingChanges));
                await Shell.Current.DisplayAlert("Success", $"{fieldName} updated. Click 'Save Changes' to persist.", "OK");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to update {fieldName.ToLower()}: {ex.Message}", "OK");
            }
        }

        public async Task SaveChangesAsync()
        {
            try
            {
                IsBusy = true;
                ErrorMessage = string.Empty;

                // Convert CollaborationModel to Database.Collaboration
                var dbCollaboration = new Database.Collaboration
                {
                    collaborationid = Collaboration.CollaborationID,
                    taskid = Collaboration.TaskID,
                    collaboration_title = Collaboration.Name,
                    collaboration_description = Collaboration.Description,
                    collaboration_token = Collaboration.Token
                };

                // Update collaboration
                var collaborationUpdated = await _databaseService.UpdateCollaborationAsync(dbCollaboration);
                if (!collaborationUpdated)
                {
                    await Shell.Current.DisplayAlert("Error", "Failed to update collaboration details.", "OK");
                    return;
                }

                // Update user role if changed
                if (!string.IsNullOrEmpty(Collaboration.Role))
                {
                    var roleUpdated = await _databaseService.UpdateUserCollaborationRoleAsync(Collaboration.CollaborationID, Collaboration.UserID, Collaboration.Role);
                    if (!roleUpdated)
                    {
                        await Shell.Current.DisplayAlert("Warning", "Collaboration details updated, but failed to update your role.", "OK");
                    }
                    else
                    {
                        // Refresh collaboration members to show updated role
                        await LoadCollaborationMembersAsync();
                    }
                }

                HasPendingChanges = false;
                
                // Force UI refresh
                OnPropertyChanged(nameof(Collaboration));
                OnPropertyChanged(nameof(Collaboration.Name));
                OnPropertyChanged(nameof(Collaboration.Description));
                OnPropertyChanged(nameof(Collaboration.Role));
                OnPropertyChanged(nameof(HasPendingChanges));
                
                await Shell.Current.DisplayAlert("Success", "Collaboration updated successfully!", "OK");
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to save changes: {ex.Message}";
                await Shell.Current.DisplayAlert("Error", ErrorMessage, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task LeaveCollaborationAsync()
        {
            try
            {
                var confirmed = await Shell.Current.DisplayAlert("Leave Collaboration", 
                    "Are you sure you want to leave this collaboration?", "Yes", "No");

                if (confirmed)
                {
                    IsBusy = true;
                    ErrorMessage = string.Empty;

                    await _databaseService.RemoveUserFromCollaborationAsync(Collaboration.CollaborationID, Collaboration.UserID);

                    await Shell.Current.DisplayAlert("Success", "You have left the collaboration.", "OK");
                    await BackAsync();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to leave collaboration: {ex.Message}";
                await Shell.Current.DisplayAlert("Error", ErrorMessage, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task DeleteCollaborationAsync()
        {
            try
            {
                var confirmed = await Shell.Current.DisplayAlert("Delete Collaboration", 
                    "Are you sure you want to delete this collaboration? This action cannot be undone.", "Yes", "No");

                if (confirmed)
                {
                    IsBusy = true;
                    ErrorMessage = string.Empty;

                    await _databaseService.DeleteCollaborationAsync(Collaboration.CollaborationID);

                    await Shell.Current.DisplayAlert("Success", "Collaboration deleted successfully.", "OK");
                    await BackAsync();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to delete collaboration: {ex.Message}";
                await Shell.Current.DisplayAlert("Error", ErrorMessage, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task ViewCollaborationAsync()
        {
            try
            {
                // This is already the detail view, so just show a message
                await Shell.Current.DisplayAlert("Collaboration Details", 
                    "You are already viewing the collaboration details.", "OK");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to view collaboration: {ex.Message}", "OK");
            }
        }

        private async Task ViewUsersAsync()
        {
            try
            {
                // Load collaboration members
                var members = await _databaseService.GetCollaborationMembersAsync(Collaboration.CollaborationID);
                
                if (members == null || !members.Any())
                {
                    await Shell.Current.DisplayAlert("Collaboration Users", 
                        $"No users found in '{Collaboration.Name}'", 
                        "OK");
                    return;
                }

                // Create user list message
                var userList = new System.Text.StringBuilder();
                userList.AppendLine($"Users in '{Collaboration.Name}' ({members.Count} total):");
                userList.AppendLine();
                
                foreach (var member in members)
                {
                    userList.AppendLine($"👤 {member.Name}");
                    userList.AppendLine($"   📧 {member.Email}");
                    userList.AppendLine($"   🎭 Role: {member.Role}");
                    userList.AppendLine();
                }

                await Shell.Current.DisplayAlert("Collaboration Users", userList.ToString(), "OK");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to view users: {ex.Message}", "OK");
            }
        }

        private async Task BackAsync()
        {
            try
            {
                await Shell.Current.GoToAsync("///collab");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error navigating back: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", $"Failed to navigate back: {ex.Message}", "OK");
            }
        }

        public Services.SupabaseService? GetSupabaseService()
        {
            return _databaseService?.GetSupabaseService();
        }
    }

}