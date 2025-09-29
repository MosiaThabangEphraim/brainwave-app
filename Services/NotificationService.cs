using System.Diagnostics;

namespace BrainWave.APP.Services
{
    public class NotificationService
    {
        private readonly EmailService _emailService;

        public NotificationService()
        {
            _emailService = new EmailService();
        }

        public async Task ScheduleReminderNotificationAsync(string title, string description, DateTime reminderTime, string userEmail, string taskTitle = "", DateTime? taskDueDate = null)
        {
            try
            {
                Debug.WriteLine($"🔔 REMINDER: {title} - {description} at {reminderTime:MMM dd, yyyy HH:mm}");
                
                // Check if the reminder time is more than 72 hours in the future (SendGrid limitation for email scheduling)
                var timeUntilReminder = reminderTime - DateTime.Now;
                if (timeUntilReminder.TotalHours > 72)
                {
                    Debug.WriteLine($"⚠️ WARNING: Reminder time is more than 72 hours in the future. SendGrid may not deliver the email, but the reminder will still be stored correctly.");
                    Debug.WriteLine($"⏰ Reminder time: {reminderTime:yyyy-MM-dd HH:mm:ss}");
                    Debug.WriteLine($"⏰ Current time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                    Debug.WriteLine($"⏰ Hours until reminder: {timeUntilReminder.TotalHours:F2}");
                }
                
                // Send email reminder using SendGrid with sendAt property (email scheduling)
                var emailSent = await _emailService.SendReminderEmailAsync(userEmail, title, description, reminderTime, taskTitle, taskDueDate);
                
                if (emailSent)
                {
                    Debug.WriteLine($"✅ Email reminder scheduled successfully for {reminderTime:MMM dd, yyyy HH:mm}");
                }
                else
                {
                    Debug.WriteLine($"❌ Failed to schedule email reminder for {reminderTime:MMM dd, yyyy HH:mm}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Failed to schedule notification: {ex.Message}");
            }
        }

        public async Task ShowTaskReminderAsync(string taskTitle, DateTime dueDate)
        {
            try
            {
                Debug.WriteLine($"📋 TASK REMINDER: {taskTitle} is due on {dueDate:MMM dd, yyyy}");
                
                // In a real app, this would show a system notification
                await Task.Delay(100);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Failed to show task reminder: {ex.Message}");
            }
        }
    }
}

