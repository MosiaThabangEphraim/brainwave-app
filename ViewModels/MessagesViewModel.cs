using BrainWave.APP.Models;
using BrainWave.APP.Services;
using BrainWave.APP.Database;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Maui.Storage;
using static BrainWave.APP.Constants;

namespace BrainWave.APP.ViewModels
{
    public class MessagesViewModel : BaseViewModel
    {
        private readonly DatabaseService _databaseService;
        private int _collaborationId;
        private string _collaborationName = string.Empty;
        private string _newMessage = string.Empty;
        private bool _isLoading = false;
        private readonly SemaphoreSlim _loadMessagesSemaphore = new SemaphoreSlim(1, 1);

        public ObservableCollection<MessageDto> Messages { get; } = new();

        public string CollaborationName
        {
            get => _collaborationName;
            set => Set(ref _collaborationName, value);
        }

        public string NewMessage
        {
            get => _newMessage;
            set => Set(ref _newMessage, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => Set(ref _isLoading, value);
        }

        public ICommand SendMessageCommand { get; }
        public ICommand RefreshMessagesCommand { get; }

        public MessagesViewModel()
        {
            _databaseService = new DatabaseService(new SupabaseService());
            SendMessageCommand = new Command(async () => await SendMessageAsync());
            RefreshMessagesCommand = new Command(async () => await LoadMessagesAsync());
        }

        public async Task InitializeAsync(int collaborationId, string collaborationName)
        {
            System.Diagnostics.Debug.WriteLine($"🔍 MessagesViewModel: InitializeAsync called with collaborationId: {collaborationId}, collaborationName: '{collaborationName}'");
            System.Diagnostics.Debug.WriteLine($"🔍 MessagesViewModel: Previous collaborationId: {_collaborationId}");
            
            // Always update the collaboration context - no guards
            _collaborationId = collaborationId;
            CollaborationName = collaborationName;
            
            System.Diagnostics.Debug.WriteLine($"🔍 MessagesViewModel: Set _collaborationId = {_collaborationId}, CollaborationName = '{CollaborationName}'");
            
            await LoadMessagesAsync();
        }

        private async Task LoadMessagesAsync()
        {
            if (!await _loadMessagesSemaphore.WaitAsync(100)) // Wait max 100ms
            {
                System.Diagnostics.Debug.WriteLine("🔍 MessagesViewModel: LoadMessagesAsync already in progress, skipping...");
                return;
            }

            try
            {
                System.Diagnostics.Debug.WriteLine($"🔍 MessagesViewModel: LoadMessagesAsync started for collaborationId: {_collaborationId}");
                
                IsLoading = true;
                Messages.Clear();

                System.Diagnostics.Debug.WriteLine($"🔍 MessagesViewModel: Calling GetMessagesByCollaborationIdAsync with collaborationId: {_collaborationId}");
                var messages = await _databaseService.GetMessagesByCollaborationIdAsync(_collaborationId);
                
                System.Diagnostics.Debug.WriteLine($"🔍 MessagesViewModel: Retrieved {messages?.Count ?? 0} messages");
                
                if (messages != null)
                {
                    foreach (var message in messages)
                    {
                        System.Diagnostics.Debug.WriteLine($"🔍 MessagesViewModel: Adding message - ID: {message.MessageId}, CollaborationId: {message.CollaborationId}, User: {message.UserName}, Content: '{message.Content.Substring(0, Math.Min(50, message.Content.Length))}...'");
                        Messages.Add(message);
                    }
                }
                
                System.Diagnostics.Debug.WriteLine($"✅ MessagesViewModel: LoadMessagesAsync completed. Total messages in collection: {Messages.Count}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ MessagesViewModel: Exception in LoadMessagesAsync: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"❌ MessagesViewModel: Stack trace: {ex.StackTrace}");
                await Shell.Current.DisplayAlert("Error", "Failed to load messages. Please try again.", "OK");
            }
            finally
            {
                IsLoading = false;
                _loadMessagesSemaphore.Release();
            }
        }

        private async Task SendMessageAsync()
        {
            if (string.IsNullOrWhiteSpace(NewMessage))
            {
                await Shell.Current.DisplayAlert("Validation Error", "Please enter a message.", "OK");
                return;
            }

            try
            {
                System.Diagnostics.Debug.WriteLine("🔍 MessagesViewModel: SendMessageAsync started");
                System.Diagnostics.Debug.WriteLine($"🔍 MessagesViewModel: CollaborationId = {_collaborationId}");
                System.Diagnostics.Debug.WriteLine($"🔍 MessagesViewModel: NewMessage = '{NewMessage}'");
                
                // Try to get user ID from stored session first (same pattern as CollaborationViewModel)
                var storedUserId = await SecureStorage.GetAsync(Constants.SECURE_KEY_USER_ID);
                User? currentUser = null;
                
                System.Diagnostics.Debug.WriteLine($"🔍 MessagesViewModel: Stored user ID: '{storedUserId}'");
                
                if (!string.IsNullOrEmpty(storedUserId) && int.TryParse(storedUserId, out int userId))
                {
                    // Get user by ID directly
                    currentUser = await _databaseService.GetUserByIdAsync(userId);
                    System.Diagnostics.Debug.WriteLine($"🔍 MessagesViewModel: Got user by ID: {currentUser?.userid}");
                }
                
                // Fallback to GetCurrentUserAsync if stored user ID approach failed
                if (currentUser == null)
                {
                    System.Diagnostics.Debug.WriteLine("🔍 MessagesViewModel: Falling back to GetCurrentUserAsync.");
                    currentUser = await _databaseService.GetCurrentUserAsync();
                    System.Diagnostics.Debug.WriteLine($"🔍 MessagesViewModel: Got current user: {currentUser?.userid}");
                }
                
                if (currentUser == null)
                {
                    System.Diagnostics.Debug.WriteLine("❌ MessagesViewModel: No current user found");
                    await Shell.Current.DisplayAlert("Error", "You must be logged in to send messages.", "OK");
                    return;
                }
                
                System.Diagnostics.Debug.WriteLine($"✅ MessagesViewModel: Found current user - ID: {currentUser.userid}, Name: {currentUser.f_name} {currentUser.l_name}, Email: {currentUser.email}");
                System.Diagnostics.Debug.WriteLine($"🔍 MessagesViewModel: Calling SendMessageAsync with userId: {currentUser.userid}");

                var success = await _databaseService.SendMessageAsync(_collaborationId, currentUser.userid, NewMessage.Trim());
                
                System.Diagnostics.Debug.WriteLine($"🔍 MessagesViewModel: SendMessageAsync result: {success}");
                
                if (success)
                {
                    System.Diagnostics.Debug.WriteLine("✅ MessagesViewModel: Message sent successfully, clearing input and refreshing");
                    NewMessage = string.Empty;
                    await LoadMessagesAsync(); // Refresh messages to show the new one
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("❌ MessagesViewModel: Failed to send message");
                    await Shell.Current.DisplayAlert("Error", "Failed to send message. Please try again.", "OK");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ MessagesViewModel: Exception in SendMessageAsync: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"❌ MessagesViewModel: Stack trace: {ex.StackTrace}");
                await Shell.Current.DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
        }
    }
}
