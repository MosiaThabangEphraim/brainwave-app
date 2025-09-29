using BrainWave.APP.Models;
using BrainWave.APP.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace BrainWave.APP.ViewModels
{
    public class HelpViewModel : BaseViewModel
    {
        private readonly EmailService _emailService;
        private string _contactName = string.Empty;
        private string _contactEmail = string.Empty;
        private string _contactSubject = string.Empty;
        private string _contactMessage = string.Empty;
        private bool _isFormValid = false;

        public ObservableCollection<FaqModel> FaqItems { get; } = new();

        public string ContactName
        {
            get => _contactName;
            set 
            { 
                Set(ref _contactName, value);
                ValidateForm();
            }
        }

        public string ContactEmail
        {
            get => _contactEmail;
            set 
            { 
                Set(ref _contactEmail, value);
                ValidateForm();
            }
        }

        public string ContactSubject
        {
            get => _contactSubject;
            set 
            { 
                Set(ref _contactSubject, value);
                ValidateForm();
            }
        }

        public string ContactMessage
        {
            get => _contactMessage;
            set 
            { 
                Set(ref _contactMessage, value);
                ValidateForm();
            }
        }

        public bool IsFormValid
        {
            get => _isFormValid;
            set => Set(ref _isFormValid, value);
        }

        public ICommand SubmitContactCommand { get; }

        public HelpViewModel()
        {
            _emailService = new EmailService();
            SubmitContactCommand = new Command(async () => await SubmitContactAsync());
            InitializeFaqItems();
        }

        private void InitializeFaqItems()
        {
            FaqItems.Clear();

            // Top 10 FAQ items with current app features
            FaqItems.Add(new FaqModel
            {
                Question = "How do I create a new task?",
                Answer = "Go to the Tasks tab, click the 'Create New' button at the top, fill in the task details (title, description, due date, priority), and click 'Create Task'. The due date will be set to tomorrow by default."
            });

            FaqItems.Add(new FaqModel
            {
                Question = "How do I mark a task as completed?",
                Answer = "In the Tasks list, tap the checkmark button next to any task, or click 'View' on a task and use the 'Mark as Complete' option. Completed tasks will show a green checkmark."
            });

            FaqItems.Add(new FaqModel
            {
                Question = "How do I set email reminders for my tasks?",
                Answer = "Go to the Reminders tab, click 'Create New' at the top, select a task to associate with the reminder, set the date and time, and confirm. You'll receive an email reminder at the scheduled time."
            });

            FaqItems.Add(new FaqModel
            {
                Question = "How do I collaborate with others on tasks?",
                Answer = "Go to the Collaborations tab, click 'Create New' at the top, select a task, enter your role (e.g., 'Project Manager', 'Developer'), and share the generated token with others. They can join using the token."
            });

            FaqItems.Add(new FaqModel
            {
                Question = "How do I join a collaboration using a token?",
                Answer = "In the Collaborations tab, click the 'Join Collaboration' button, enter the collaboration token, specify your role, and click 'Join'. You'll then have access to the shared task."
            });

            FaqItems.Add(new FaqModel
            {
                Question = "How do I search and filter my tasks?",
                Answer = "Use the search bar at the top of each tab to search by title or description. Use the Priority and Status dropdowns to filter tasks. You can also sort by title, due date, priority, or creation date."
            });

            FaqItems.Add(new FaqModel
            {
                Question = "What are badges and how do I earn them?",
                Answer = "Badges are achievements based on completed tasks: Amateur (1-25 tasks), Achiever (26-50), Task Master (51-100), and Productivity Champion (100+ tasks). Check your Profile tab to see your current badge."
            });

            FaqItems.Add(new FaqModel
            {
                Question = "Can I edit or delete tasks after creating them?",
                Answer = "Yes! Click 'View' on any task to see details, then use the 'Edit' option for each field to modify it, or use the 'Delete' button to remove it permanently. Changes are saved automatically."
            });

            FaqItems.Add(new FaqModel
            {
                Question = "How do I export my tasks?",
                Answer = "In the Tasks tab, click 'View' on any task, then click 'Export'. Choose between PDF or TXT format, and the file will be saved to your Documents folder with all task details."
            });

            FaqItems.Add(new FaqModel
            {
                Question = "How do I contact support?",
                Answer = "Use the contact form in the Help tab to send us a message. We'll receive your email at mybrainwave@outlook.com and respond directly to your email address. Make sure to provide a valid email address."
            });
        }

        private void ValidateForm()
        {
            var isValid = !string.IsNullOrWhiteSpace(ContactName) &&
                         !string.IsNullOrWhiteSpace(ContactEmail) &&
                         !string.IsNullOrWhiteSpace(ContactSubject) &&
                         !string.IsNullOrWhiteSpace(ContactMessage) &&
                         IsValidEmail(ContactEmail);
            
            IsFormValid = isValid;
        }

        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        public async Task SubmitContactAsync()
        {
            if (!IsFormValid)
            {
                await Shell.Current.DisplayAlert("Validation Error", "Please fill in all required fields with a valid email address.", "OK");
                return;
            }

            try
            {
                System.Diagnostics.Debug.WriteLine($"HelpViewModel: Sending contact email from {ContactName} ({ContactEmail})");

                // Send email using SendGrid
                var emailSent = await _emailService.SendContactEmailAsync(ContactName, ContactEmail, ContactSubject, ContactMessage);

                if (emailSent)
                {
                    await Shell.Current.DisplayAlert("Success", "Your message has been sent successfully! We'll get back to you soon.", "OK");
                    
                    // Clear form
                    ContactName = string.Empty;
                    ContactEmail = string.Empty;
                    ContactSubject = string.Empty;
                    ContactMessage = string.Empty;
                }
                else
                {
                    await Shell.Current.DisplayAlert("Error", "Failed to send message. Please try again later.", "OK");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"HelpViewModel: Error sending contact email: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
        }

    }
}

