using BrainWave.APP.Models;
using BrainWave.APP.Services;
using BrainWave.APP.Database;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Maui.Storage;
using static BrainWave.APP.Constants;

namespace BrainWave.APP.ViewModels
{
    public class CollaborationViewModel : BaseViewModel
    {
        private readonly DatabaseService _databaseService;
        private readonly EmailService _emailService;
        private readonly SupabaseService _supabaseService;
        public ObservableCollection<CollaborationModel> Items { get; } = new();
        public ObservableCollection<TaskDtos> AvailableTasks { get; } = new();
        
        public CollaborationModel Editing { get; set; } = new();
        
        // Selected task tracking
        private TaskDtos? _selectedTask;
        public TaskDtos? SelectedTask 
        { 
            get => _selectedTask; 
            set 
            { 
                _selectedTask = value; 
                OnPropertyChanged(nameof(SelectedTask));
                OnPropertyChanged(nameof(IsTaskSelected));
                OnPropertyChanged(nameof(SelectedTaskDisplayText));
            } 
        }
        
        public bool IsTaskSelected => SelectedTask != null;
        public string SelectedTaskDisplayText => SelectedTask != null ? $"📋 Selected: {SelectedTask.Title}" : "📋 No task selected";
        
        // Join collaboration
        private string _joinToken = string.Empty;
        public string JoinToken
        {
            get => _joinToken;
            set => Set(ref _joinToken, value);
        }

        private string _joinRole = string.Empty;
        public string JoinRole
        {
            get => _joinRole;
            set => Set(ref _joinRole, value);
        }


        
        // View state management
        private bool _showCollaborationsList = true;
        private bool _showTokenSection = false;

        public bool ShowCollaborationsList
        {
            get => _showCollaborationsList;
            set => Set(ref _showCollaborationsList, value);
        }



        public bool ShowTokenSection
        {
            get => _showTokenSection;
            set => Set(ref _showTokenSection, value);
        }

        public string FormTitle => Editing.CollaborationID == 0 ? "Create New Collaboration" : "Edit Collaboration";

        // Sorting and Search properties
        private string _sortBy = "Title";
        public string SortBy
        {
            get => _sortBy;
            set { Set(ref _sortBy, value); ApplySort(); }
        }

        private string _sortOrder = "Ascending";
        public string SortOrder
        {
            get => _sortOrder;
            set { Set(ref _sortOrder, value); ApplySort(); }
        }

        private string _searchText = "";
        public string SearchText
        {
            get => _searchText;
            set { Set(ref _searchText, value); ApplySearchAndSort(); }
        }

        private List<CollaborationModel> _allCollaborations = new();

        public ICommand CreateCommand { get; }
        public ICommand UpdateCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand LeaveCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand ShowCreateFormCommand { get; }
        public ICommand BackToCollaborationsCommand { get; }
        public ICommand SelectTaskCommand { get; }
        public ICommand ShareTokenCommand { get; }
        public ICommand CopyTokenCommand { get; }
        public ICommand ShowJoinCollaborationCommand { get; }
        public ICommand JoinCollaborationCommand { get; }
        public ICommand ViewCommand { get; }
        public ICommand ShowTokenCommand { get; }
        public ICommand ViewUsersCommand { get; }
        public ICommand MessagesCommand { get; }
        public ICommand CancelCommand { get; }

        // Default constructor for XAML binding
        public CollaborationViewModel() : this(new DatabaseService(new SupabaseService()), new SupabaseService())
        {
        }

        public CollaborationViewModel(DatabaseService databaseService) : this(databaseService, new SupabaseService())
        {
        }

        public CollaborationViewModel(DatabaseService databaseService, SupabaseService supabaseService)
        {
            _databaseService = databaseService;
            _emailService = new EmailService();
            _supabaseService = supabaseService;
            CreateCommand = new Command(async () => await CreateAsync());
            UpdateCommand = new Command(async () => await UpdateAsync());
            RefreshCommand = new Command(async () => await LoadAsync());
            LogoutCommand = new Command(async () => await LogoutAsync());
            DeleteCommand = new Command<CollaborationModel>(async (collaboration) => await DeleteAsync(collaboration));
            LeaveCommand = new Command<CollaborationModel>(async (collaboration) => await LeaveAsync(collaboration));
            EditCommand = new Command<CollaborationModel>(async (collaboration) => await EditAsync(collaboration));
            ShowCreateFormCommand = new Command(async () => await NavigateToCreateCollaborationAsync());
            BackToCollaborationsCommand = new Command(() => ShowCollaborationsListView());
            SelectTaskCommand = new Command<TaskDtos>((task) => SelectTask(task));
            ShareTokenCommand = new Command<CollaborationModel>(async (collaboration) => await ShareTokenAsync(collaboration));
            CopyTokenCommand = new Command<CollaborationModel>(async (collaboration) => await CopyTokenAsync(collaboration));
            ShowJoinCollaborationCommand = new Command(async () => await NavigateToJoinCollaborationAsync());
            JoinCollaborationCommand = new Command(async () => await JoinCollaborationAsync());
            ViewCommand = new Command<CollaborationModel>(async (collaboration) => await ViewCollaborationAsync(collaboration));
            ShowTokenCommand = new Command<CollaborationModel>(async (collaboration) => await ShowTokenAsync(collaboration));
            ViewUsersCommand = new Command<CollaborationModel>(async (collaboration) => await ViewUsersAsync(collaboration));
            MessagesCommand = new Command<CollaborationModel>(async (collaboration) => await MessagesAsync(collaboration));
            CancelCommand = new Command(async () => await CancelAsync());
        }

        public async Task LoadAsync()
        {
            if (IsBusy) return;
            IsBusy = true;

            try
            {
                System.Diagnostics.Debug.WriteLine("🔧 CollaborationViewModel: LoadAsync started.");
                
                // First ensure Supabase is initialized
                await _supabaseService.InitializeAsync();
                System.Diagnostics.Debug.WriteLine("🔧 CollaborationViewModel: SupabaseService initialized.");
                
                // Try to get user ID from stored session first (same pattern as ProfileViewModel)
                var storedUserId = await SecureStorage.GetAsync(Constants.SECURE_KEY_USER_ID);
                User? currentUser = null;
                
                System.Diagnostics.Debug.WriteLine($"🔧 CollaborationViewModel: Stored user ID: '{storedUserId}'");
                
                if (!string.IsNullOrEmpty(storedUserId) && int.TryParse(storedUserId, out int userId))
                {
                    // Get user by ID directly
                    currentUser = await _databaseService.GetUserByIdAsync(userId);
                    System.Diagnostics.Debug.WriteLine($"🔧 CollaborationViewModel: Got user by ID: {currentUser?.userid}");
                }
                
                // Fallback to GetCurrentUserAsync if stored user ID approach failed
                if (currentUser == null)
                {
                    System.Diagnostics.Debug.WriteLine("🔧 CollaborationViewModel: Falling back to GetCurrentUserAsync.");
                    currentUser = await _databaseService.GetCurrentUserAsync();
                    System.Diagnostics.Debug.WriteLine($"🔧 CollaborationViewModel: Got current user: {currentUser?.userid}");
                }
                
                if (currentUser != null)
                {
                    System.Diagnostics.Debug.WriteLine($"🔧 CollaborationViewModel: Loading collaborations for user ID: {currentUser.userid}");
                    var collaborationsWithTaskInfo = await _databaseService.GetCollaborationsWithTaskInfoByUserIdAsync(currentUser.userid);
                    _allCollaborations = collaborationsWithTaskInfo.Select(cwti => ConvertToCollaborationModel(cwti.collaboration, cwti.role, cwti.taskTitle)).ToList();
                    System.Diagnostics.Debug.WriteLine($"🔧 CollaborationViewModel: Loaded {_allCollaborations.Count} collaborations.");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("🔧 CollaborationViewModel: No current user found.");
                }
                
                ApplySearchAndSort();
                System.Diagnostics.Debug.WriteLine($"🔧 CollaborationViewModel: Applied search and sort. Items count: {Items.Count}");
                
                // Load available tasks for collaboration creation
                await LoadAvailableTasksAsync();
                System.Diagnostics.Debug.WriteLine("🔧 CollaborationViewModel: LoadAsync completed.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"🔧 CollaborationViewModel: Error in LoadAsync: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"🔧 CollaborationViewModel: Stack trace: {ex.StackTrace}");
                await Shell.Current.DisplayAlert("Error", $"Failed to load collaborations: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task CreateAsync()
        {
            if (IsBusy) return;
            IsBusy = true;

            try
            {
                if (string.IsNullOrWhiteSpace(Editing.Name))
                {
                    await Shell.Current.DisplayAlert("Error", "Please enter a collaboration name.", "OK");
                    return;
                }
                
                if (SelectedTask == null)
                {
                    await Shell.Current.DisplayAlert("Error", "Please select a task to collaborate on.", "OK");
                    return;
                }
                
                if (string.IsNullOrWhiteSpace(Editing.Role))
                {
                    await Shell.Current.DisplayAlert("Error", "Please enter your role in this collaboration.", "OK");
                    return;
                }

                // First ensure Supabase is initialized
                await _supabaseService.InitializeAsync();
                
                // Try to get user ID from stored session first (same pattern as ProfileViewModel)
                var storedUserId = await SecureStorage.GetAsync(Constants.SECURE_KEY_USER_ID);
                User? currentUser = null;
                
                if (!string.IsNullOrEmpty(storedUserId) && int.TryParse(storedUserId, out int userId))
                {
                    // Get user by ID directly
                    currentUser = await _databaseService.GetUserByIdAsync(userId);
                }
                
                // Fallback to GetCurrentUserAsync if stored user ID approach failed
                if (currentUser == null)
                {
                    currentUser = await _databaseService.GetCurrentUserAsync();
                }
                
                if (currentUser == null)
                {
                    await Shell.Current.DisplayAlert("Error", "User not found. Please log in again.", "OK");
                    return;
                }

                // Generate a random token
                Editing.Token = GenerateCollaborationToken();
                Editing.TaskID = SelectedTask.TaskID;
                Editing.TaskTitle = SelectedTask.Title;

                var collaboration = ConvertToCollaboration(Editing);
                var success = await _databaseService.CreateCollaborationAsync(collaboration, currentUser.userid, Editing.Role);
                
                if (success)
                {
                    await LoadAsync();
                    
                    // Display the collaboration token to the user
                    await Shell.Current.DisplayAlert("Collaboration Created!", 
                        $"Your collaboration has been created successfully!\n\n" +
                        $"🔑 Collaboration Token: {Editing.Token}\n\n" +
                        $"You can share this token with others to invite them to join your collaboration. " +
                        $"You can also view and manage this token anytime from the collaboration list.", 
                        "OK");
                    
                    await Shell.Current.GoToAsync("///collab"); // Navigate back to collaborations page
                }
                else
                {
                    await Shell.Current.DisplayAlert("Error", "Failed to create collaboration.", "OK");
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to create collaboration: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }



        public async Task DeleteAsync(CollaborationModel collaboration)
        {
            if (IsBusy) return;
            
            var result = await Shell.Current.DisplayAlert("Delete Collaboration", 
                $"⚠️ WARNING: This will permanently delete the collaboration '{collaboration.Name}' for ALL users!\n\n" +
                $"This action cannot be undone. All members will lose access to this collaboration.\n\n" +
                $"Are you sure you want to continue?", 
                "Delete", "Cancel");
            
            if (!result) return;
            
            IsBusy = true;
            try
            {
                if (await _databaseService.DeleteCollaborationAsync(collaboration.CollaborationID))
                {
                    await LoadAsync();
                    await Shell.Current.DisplayAlert("Success", "Collaboration deleted successfully!", "OK");
                }
                else
                {
                    await Shell.Current.DisplayAlert("Error", "Failed to delete collaboration.", "OK");
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to delete collaboration: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task UpdateCollaborationAsync(CollaborationModel collaboration)
        {
            if (IsBusy) return;
            IsBusy = true;
            
            try
            {
                // Update the collaboration in the database
                var dbCollaboration = ConvertToCollaboration(collaboration);
                var success = await _databaseService.UpdateCollaborationAsync(dbCollaboration);
                
                if (success)
                {
                    // Update user role if changed
                    if (!string.IsNullOrEmpty(collaboration.Role))
                    {
                        var currentUser = await _databaseService.GetCurrentUserAsync();
                        if (currentUser != null)
                        {
                            var roleUpdated = await _databaseService.UpdateUserCollaborationRoleAsync(collaboration.CollaborationID, currentUser.userid, collaboration.Role);
                            if (!roleUpdated)
                            {
                                await Shell.Current.DisplayAlert("Warning", "Collaboration details updated, but failed to update your role.", "OK");
                            }
                        }
                    }
                    
                    await LoadAsync();
                    await Shell.Current.DisplayAlert("Success", "Collaboration updated successfully!", "OK");
                }
                else
                {
                    await Shell.Current.DisplayAlert("Error", "Failed to update collaboration. Please try again.", "OK");
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to update collaboration: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task LeaveCollaborationAsync(CollaborationModel collaboration)
        {
            if (IsBusy) return;
            
            IsBusy = true;
            try
            {
                // Get current user ID
                var currentUser = await _databaseService.GetCurrentUserAsync();
                if (currentUser == null)
                {
                    await Shell.Current.DisplayAlert("Error", "Unable to identify current user.", "OK");
                    return;
                }

                if (await _databaseService.RemoveUserFromCollaborationAsync(collaboration.CollaborationID, currentUser.userid))
                {
                    await LoadAsync();
                    await Shell.Current.DisplayAlert("Success", "You have left the collaboration successfully!", "OK");
                }
                else
                {
                    await Shell.Current.DisplayAlert("Error", "Failed to leave collaboration.", "OK");
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to leave collaboration: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task LeaveAsync(CollaborationModel collaboration)
        {
            if (IsBusy) return;
            
            var result = await Shell.Current.DisplayAlert("Leave Collaboration", 
                $"Are you sure you want to leave the collaboration '{collaboration.Name}'?\n\n" +
                $"You will no longer have access to this collaboration, but other members can continue working on it.", 
                "Leave", "Cancel");
            
            if (!result) return;
            
            IsBusy = true;
            try
            {
                // Get current user ID
                var currentUser = await _databaseService.GetCurrentUserAsync();
                if (currentUser == null)
                {
                    await Shell.Current.DisplayAlert("Error", "Unable to identify current user.", "OK");
                    return;
                }

                if (await _databaseService.RemoveUserFromCollaborationAsync(collaboration.CollaborationID, currentUser.userid))
                {
                    await LoadAsync();
                    await Shell.Current.DisplayAlert("Success", "You have left the collaboration successfully!", "OK");
                }
                else
                {
                    await Shell.Current.DisplayAlert("Error", "Failed to leave collaboration.", "OK");
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to leave collaboration: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }


        private void ShowCollaborationsListView()
        {
            ShowCollaborationsList = true;
            ShowTokenSection = false;
            // Reset form when going back to collaborations list
            Editing = new CollaborationModel();
            SelectedTask = null;
            JoinToken = string.Empty;
            JoinRole = string.Empty;
            OnPropertyChanged(nameof(Editing));
            OnPropertyChanged(nameof(FormTitle));
        }

        public async Task EditAsync(CollaborationModel collaboration)
        {
            // TODO: Implement edit functionality with separate page
            await Shell.Current.DisplayAlert("Info", "Edit functionality will be implemented in a future update.", "OK");
        }

        public async Task UpdateAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            
            try
            {
                if (Editing.CollaborationID == 0)
                {
                    await Shell.Current.DisplayAlert("Error", "Please select a collaboration to update first.", "OK");
                    return;
                }
                
                if (string.IsNullOrWhiteSpace(Editing.Name))
                {
                    await Shell.Current.DisplayAlert("Error", "Please enter a collaboration name.", "OK");
                    return;
                }
                
                // Update functionality not implemented in Supabase version yet
                var success = false;
                if (success)
                {
                    await LoadAsync();
                    await Shell.Current.DisplayAlert("Success", "Collaboration updated successfully!", "OK");
                    ShowCollaborationsListView(); // Go back to collaborations list
                }
                else
                {
                    await Shell.Current.DisplayAlert("Error", "Failed to update collaboration. Please try again.", "OK");
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to update collaboration: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }


        public void SelectTask(TaskDtos task)
        {
            SelectedTask = task;
        }

        private string GenerateCollaborationToken()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 8)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public async Task ShareTokenAsync(CollaborationModel collaboration)
        {
            try
            {
                await Share.Default.RequestAsync(new ShareTextRequest
                {
                    Text = $"Join my collaboration: {collaboration.Name}\nToken: {collaboration.Token}",
                    Title = "Share Collaboration Token"
                });
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to share token: {ex.Message}", "OK");
            }
        }

        public async Task CopyTokenAsync(CollaborationModel collaboration)
        {
            try
            {
                await Clipboard.Default.SetTextAsync(collaboration.Token);
                await Shell.Current.DisplayAlert("Success", "Token copied to clipboard!", "OK");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to copy token: {ex.Message}", "OK");
            }
        }

        public async Task JoinCollaborationAsync()
        {
            if (IsBusy) return;
            IsBusy = true;

            try
            {
                if (string.IsNullOrWhiteSpace(JoinToken))
                {
                    await Shell.Current.DisplayAlert("Error", "Please enter a collaboration token.", "OK");
                    return;
                }

                if (string.IsNullOrWhiteSpace(JoinRole))
                {
                    await Shell.Current.DisplayAlert("Error", "Please select your role in this collaboration.", "OK");
                    return;
                }

                // Find collaboration by token
                var collaboration = await _databaseService.GetCollaborationByTokenAsync(JoinToken);
                if (collaboration != null)
                {
                    // Add current user to the collaboration
                    var currentUser = await _databaseService.GetCurrentUserAsync();
                    if (currentUser != null)
                    {
                        var success = await _databaseService.JoinCollaborationAsync(currentUser.userid, collaboration.collaborationid, JoinRole);
                        
                        if (success)
                        {
                            await LoadAsync();
                            await Shell.Current.DisplayAlert("Success", $"Successfully joined collaboration: {collaboration.collaboration_title}", "OK");
                            await Shell.Current.GoToAsync("///collab");
                        }
                        else
                        {
                            await Shell.Current.DisplayAlert("Error", "Failed to join collaboration.", "OK");
                        }
                    }
                    else
                    {
                        await Shell.Current.DisplayAlert("Error", "User not found. Please log in again.", "OK");
                    }
                }
                else
                {
                    await Shell.Current.DisplayAlert("Error", "Invalid collaboration token.", "OK");
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to join collaboration: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void ApplySort()
        {
            ApplySearchAndSort();
        }

        private void ApplySearchAndSort()
        {
            System.Diagnostics.Debug.WriteLine("=== APPLYSEARCHANDSORT START ===");
            System.Diagnostics.Debug.WriteLine($"_allCollaborations count: {_allCollaborations?.Count ?? 0}");
            System.Diagnostics.Debug.WriteLine($"SearchText: '{SearchText}'");
            System.Diagnostics.Debug.WriteLine($"SortBy: {SortBy}, SortOrder: {SortOrder}");
            
            IEnumerable<CollaborationModel> q = _allCollaborations ?? Enumerable.Empty<CollaborationModel>();
            System.Diagnostics.Debug.WriteLine($"Initial query count: {q.Count()}");

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                q = q.Where(c => 
                    c.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    (c.Description != null && c.Description.Contains(SearchText, StringComparison.OrdinalIgnoreCase)) ||
                    c.Role.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    (c.TaskTitle != null && c.TaskTitle.Contains(SearchText, StringComparison.OrdinalIgnoreCase)));
                System.Diagnostics.Debug.WriteLine($"After search filter count: {q.Count()}");
            }

            // Apply sorting
            q = SortBy switch
            {
                "Title" => SortOrder == "Ascending" ? q.OrderBy(c => c.Name) : q.OrderByDescending(c => c.Name),
                _ => SortOrder == "Ascending" ? q.OrderBy(c => c.Name) : q.OrderByDescending(c => c.Name)
            };
            System.Diagnostics.Debug.WriteLine($"After sorting count: {q.Count()}");

            System.Diagnostics.Debug.WriteLine($"Clearing Items collection (current count: {Items.Count})");
            Items.Clear();
            
            int addedCount = 0;
            foreach (var c in q) 
            {
                Items.Add(c);
                addedCount++;
                System.Diagnostics.Debug.WriteLine($"Added collaboration: ID={c.CollaborationID}, Name='{c.Name}', Role='{c.Role}'");
            }
            
            System.Diagnostics.Debug.WriteLine($"Final Items count: {Items.Count} (added {addedCount} items)");
            System.Diagnostics.Debug.WriteLine("=== APPLYSEARCHANDSORT END ===");
        }

        // Helper: convert Database.Collaboration to CollaborationModel
        private CollaborationModel ConvertToCollaborationModel(Database.Collaboration collaboration, string role = "", string taskTitle = "")
        {
            return new CollaborationModel
            {
                CollaborationID = collaboration.collaborationid,
                Name = collaboration.collaboration_title,
                Description = collaboration.collaboration_description,
                TaskID = collaboration.taskid,
                Token = collaboration.collaboration_token,
                Role = role, // Use the actual role from UserCollaboration
                UserID = 0, // Will be set from context
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                Category = null,
                TaskTitle = taskTitle // Task title from the JOIN query
            };
        }

        // Helper: convert CollaborationModel to Database.Collaboration
        private Database.Collaboration ConvertToCollaboration(CollaborationModel model)
        {
            var collaboration = new Database.Collaboration
            {
                collaboration_title = model.Name,
                collaboration_description = model.Description,
                taskid = model.TaskID,
                collaboration_token = model.Token
            };
            
            // Only set collaborationid if it's a valid ID (for updates), otherwise let database auto-generate
            if (model.CollaborationID > 0)
            {
                collaboration.collaborationid = model.CollaborationID;
            }
            
            return collaboration;
        }

        public SupabaseService? GetSupabaseService()
        {
            return _databaseService.GetSupabaseService();
        }

        public async Task ViewCollaborationAsync(CollaborationModel collaboration)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"🔧 ViewCollaborationAsync: Showing popup for collaboration ID: {collaboration.CollaborationID}, Name: {collaboration.Name}");
                
                // Show popup instead of navigating to new page
                var collaborationPage = Shell.Current.CurrentPage as Views.CollaborationPage;
                if (collaborationPage != null)
                {
                    collaborationPage.ShowCollaborationPopup(collaboration);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"🔧 ViewCollaborationAsync: Error: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", $"Failed to view collaboration: {ex.Message}", "OK");
            }
        }

        public async Task ShowTokenAsync(CollaborationModel collaboration)
        {
            try
            {
                await Shell.Current.DisplayAlert("Collaboration Token", 
                    $"Token: {collaboration.Token}\n\nShare this token with others to invite them to join this collaboration.", 
                    "OK");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to show token: {ex.Message}", "OK");
            }
        }

        public async Task ViewUsersAsync(CollaborationModel collaboration)
        {
            try
            {
                // Load collaboration members
                var members = await _databaseService.GetCollaborationMembersAsync(collaboration.CollaborationID);
                
                if (members == null || !members.Any())
                {
                    await Shell.Current.DisplayAlert("Collaboration Users", 
                        $"No users found in '{collaboration.Name}'", 
                        "OK");
                    return;
                }

                // Create user list message
                var userList = new System.Text.StringBuilder();
                userList.AppendLine($"Users in '{collaboration.Name}' ({members.Count} total):");
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

        public async Task MessagesAsync(CollaborationModel collaboration)
        {
            try
            {
                var collaborationId = collaboration.CollaborationID;
                var collaborationName = collaboration.Name;
                
                System.Diagnostics.Debug.WriteLine($"🔍 MessagesAsync: Starting navigation");
                System.Diagnostics.Debug.WriteLine($"🔍 MessagesAsync: Collaboration ID: {collaborationId}");
                System.Diagnostics.Debug.WriteLine($"🔍 MessagesAsync: Collaboration Name: {collaborationName}");
                
                await Shell.Current.GoToAsync($"///messages", new Dictionary<string, object>
                {
                    ["CollaborationId"] = collaborationId,
                    ["CollaborationName"] = collaborationName
                });
                
                System.Diagnostics.Debug.WriteLine($"🔍 MessagesAsync: Navigation completed");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ MessagesAsync: Error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"❌ MessagesAsync: Stack trace: {ex.StackTrace}");
                await Shell.Current.DisplayAlert("Error", $"Failed to open messages: {ex.Message}", "OK");
            }
        }

        public async Task LogoutAsync()
        {
            try
            {
                var supabaseService = _databaseService.GetSupabaseService();
                if (supabaseService != null)
                {
                    await supabaseService.LogoutAsync();
                    await Shell.Current.GoToAsync("///LoginPage");
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to logout: {ex.Message}", "OK");
            }
        }

        public async Task CancelAsync()
        {
            try
            {
                await Shell.Current.GoToAsync("///collab");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to navigate back: {ex.Message}", "OK");
            }
        }

        public async Task NavigateToCreateCollaborationAsync()
        {
            try
            {
                // Reset the editing collaboration to default values
                Editing = new CollaborationModel
                {
                    Name = "",
                    Description = "",
                    Token = "",
                    TaskID = 0,
                    UserID = 0,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                
                // Load available tasks
                await LoadAvailableTasksAsync();
                
                await Shell.Current.GoToAsync("CreateCollaborationPage");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to navigate to create collaboration page: {ex.Message}", "OK");
            }
        }

        public async Task NavigateToJoinCollaborationAsync()
        {
            try
            {
                // Reset the join form fields
                JoinToken = string.Empty;
                JoinRole = string.Empty;
                
                await Shell.Current.GoToAsync("JoinCollaborationPage");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to navigate to join collaboration page: {ex.Message}", "OK");
            }
        }

        private async Task LoadAvailableTasksAsync()
        {
            try
            {
                // First ensure Supabase is initialized
                await _supabaseService.InitializeAsync();
                
                // Try to get user ID from stored session first (same pattern as ProfileViewModel)
                var storedUserId = await SecureStorage.GetAsync(Constants.SECURE_KEY_USER_ID);
                User? currentUser = null;
                
                if (!string.IsNullOrEmpty(storedUserId) && int.TryParse(storedUserId, out int userId))
                {
                    // Get user by ID directly
                    currentUser = await _databaseService.GetUserByIdAsync(userId);
                }
                
                // Fallback to GetCurrentUserAsync if stored user ID approach failed
                if (currentUser == null)
                {
                    currentUser = await _databaseService.GetCurrentUserAsync();
                }
                
                if (currentUser != null)
                {
                    var tasks = await _databaseService.GetTasksByUserIdAsync(currentUser.userid);
                    AvailableTasks.Clear();
                    foreach (var task in tasks)
                    {
                        AvailableTasks.Add(new TaskDtos
                        {
                            TaskID = task.taskid,
                            Title = task.title,
                            Description = task.description,
                            DueDate = task.due_date.Kind == DateTimeKind.Utc ? task.due_date.ToLocalTime() : 
                                     task.due_date.Kind == DateTimeKind.Unspecified ? 
                                     DateTime.SpecifyKind(task.due_date, DateTimeKind.Local) : task.due_date,
                            Status = task.task_status,
                            Priority = task.priority_level,
                            UserID = task.userid,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        });
                    }
                    System.Diagnostics.Debug.WriteLine($"Loaded {AvailableTasks.Count} available tasks for collaborations");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Current user is null, cannot load available tasks");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading available tasks: {ex.Message}");
            }
        }

    }
}
