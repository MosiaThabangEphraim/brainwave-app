using SendGrid;
using SendGrid.Helpers.Mail;
using System.Threading.Tasks;

namespace BrainWave.APP.Services
{
    public class EmailService
    {
        private readonly string _apiKey;
        private const string FromEmail = "mybrainwave@outlook.com";
        private const string FromName = "BrainWave Support";

        public EmailService()
        {
            // TODO: Replace with your SendGrid API key
            // Get your API key from: https://app.sendgrid.com/settings/api_keys
            _apiKey = "YOUR_SENDGRID_API_KEY_HERE";
        }

        public async Task<bool> SendPasswordResetEmailAsync(string recipientEmail, string resetToken)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"EmailService: Sending password reset email to {recipientEmail} with token {resetToken}");

                var client = new SendGridClient(_apiKey);
                var from = new EmailAddress(FromEmail, FromName);
                var to = new EmailAddress(recipientEmail);
                var subject = "Password Reset Request - BrainWave";

                var plainTextContent = $"You requested to reset your password for your BrainWave account. Use the token below to reset your password: {resetToken}. This token is valid for 15 minutes only. If you didn't request this reset, please ignore this email.";

                var htmlContent = $@"
                    <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
                        <div style='background-color: #4A90E2; color: white; padding: 20px; text-align: center; border-radius: 10px 10px 0 0;'>
                            <h1 style='margin: 0; font-size: 24px;'>🔐 BrainWave</h1>
                            <p style='margin: 10px 0 0 0; font-size: 16px;'>Password Reset Request</p>
                        </div>
                        
                        <div style='background-color: #f8f9fa; padding: 30px; border-radius: 0 0 10px 10px; border: 1px solid #e9ecef;'>
                            <h2 style='color: #333; margin-top: 0;'>Reset Your Password</h2>
                            
                            <p style='color: #666; font-size: 16px; line-height: 1.5;'>
                                You requested to reset your password for your BrainWave account. 
                                Use the token below to reset your password:
                            </p>
                            
                            <div style='background-color: #fff; border: 2px solid #4A90E2; border-radius: 8px; padding: 20px; text-align: center; margin: 20px 0;'>
                                <h3 style='color: #4A90E2; margin: 0 0 10px 0; font-size: 18px;'>Your Reset Token</h3>
                                <div style='font-family: monospace; font-size: 24px; font-weight: bold; color: #333; letter-spacing: 2px; background-color: #f8f9fa; padding: 15px; border-radius: 5px;'>
                                    {resetToken}
                                </div>
                            </div>
                            
                            <p style='color: #666; font-size: 14px; line-height: 1.5;'>
                                <strong>Important:</strong>
                                <br>• This token is valid for 15 minutes only
                                <br>• Enter this token exactly as shown (case-sensitive)
                                <br>• If you didn't request this reset, please ignore this email
                            </p>
                            
                            <div style='margin-top: 30px; padding-top: 20px; border-top: 1px solid #e9ecef;'>
                                <p style='color: #999; font-size: 12px; margin: 0;'>
                                    This email was sent from BrainWave Task Management System
                                    <br>© 2025 BrainWave. All rights reserved.
                                </p>
                            </div>
                        </div>
                    </div>";

                var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
                var response = await client.SendEmailAsync(msg);

                System.Diagnostics.Debug.WriteLine($"SendGrid response status: {response.StatusCode}");
                return response.IsSuccessStatusCode;
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Email sending error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SendContactEmailAsync(string senderName, string senderEmail, string subject, string message)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"EmailService: Sending contact email FROM {FromEmail} TO mosiathabangephraim2@gmail.com - User info: {senderName} ({senderEmail})");
                System.Diagnostics.Debug.WriteLine($"EmailService: Using SendGrid API Key: {_apiKey.Substring(0, 10)}...");

                var client = new SendGridClient(_apiKey);
                var from = new EmailAddress(FromEmail, FromName); // FROM: mybrainwave@outlook.com
                var to = new EmailAddress("mosiathabangephraim2@gmail.com", "Thabang Mosia"); // TO: Gmail (working reliably)
                var emailSubject = $"Contact Form: {subject}";

                var plainTextContent = $"New contact form submission from {senderName} ({senderEmail}). Subject: {subject}. Message: {message}";

                var htmlContent = $@"
                    <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
                        <div style='background-color: #4A90E2; color: white; padding: 20px; text-align: center; border-radius: 10px 10px 0 0;'>
                            <h1 style='margin: 0; font-size: 24px;'>📧 BrainWave</h1>
                            <p style='margin: 10px 0 0 0; font-size: 16px;'>Contact Form Submission</p>
                        </div>
                        
                        <div style='background-color: #f8f9fa; padding: 30px; border-radius: 0 0 10px 10px; border: 1px solid #e9ecef;'>
                            <h2 style='color: #333; margin-top: 0;'>New Contact Form Message</h2>
                            
                            <div style='background-color: #fff; border: 2px solid #4A90E2; border-radius: 8px; padding: 20px; margin: 20px 0;'>
                                <h3 style='color: #4A90E2; margin: 0 0 15px 0; font-size: 18px;'>Contact Information</h3>
                                <p style='color: #333; font-size: 16px; margin: 8px 0;'>
                                    <strong>Name:</strong> {senderName}
                                </p>
                                <p style='color: #333; font-size: 16px; margin: 8px 0;'>
                                    <strong>Email:</strong> <a href='mailto:{senderEmail}' style='color: #4A90E2; text-decoration: none; font-weight: bold;'>{senderEmail}</a>
                                </p>
                                <p style='color: #333; font-size: 16px; margin: 8px 0;'>
                                    <strong>Subject:</strong> {subject}
                                </p>
                            </div>
                            
                            <div style='background-color: #fff; border: 1px solid #e9ecef; border-radius: 8px; padding: 20px; margin: 20px 0;'>
                                <h3 style='color: #333; margin-top: 0; font-size: 18px;'>Message</h3>
                                <p style='color: #666; font-size: 16px; line-height: 1.5; white-space: pre-wrap; margin: 10px 0;'>{message}</p>
                            </div>
                            
                            <div style='margin-top: 30px; padding-top: 20px; border-top: 1px solid #e9ecef;'>
                                <p style='color: #999; font-size: 12px; margin: 0;'>
                                    This email was sent from BrainWave Task Management System
                                    <br>© 2025 BrainWave. All rights reserved.
                                </p>
                            </div>
                        </div>
                    </div>";

                var msg = MailHelper.CreateSingleEmail(from, to, emailSubject, plainTextContent, htmlContent);
                var response = await client.SendEmailAsync(msg);

                System.Diagnostics.Debug.WriteLine($"SendGrid response status: {response.StatusCode}");
                System.Diagnostics.Debug.WriteLine($"SendGrid response headers: {string.Join(", ", response.Headers.Select(h => $"{h.Key}={string.Join(",", h.Value)}"))}");
                
                var responseBody = await response.Body.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"SendGrid response body: {responseBody}");
                
                // Check for specific SendGrid error messages
                if (!string.IsNullOrEmpty(responseBody))
                {
                    System.Diagnostics.Debug.WriteLine($"SendGrid response contains body content - this might indicate an issue");
                }
                
                // Log the actual email addresses being used
                System.Diagnostics.Debug.WriteLine($"Email details - FROM: {from.Email} ({from.Name}), TO: {to.Email}");
                
                return response.IsSuccessStatusCode;
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Email sending error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SendReminderEmailAsync(string recipientEmail, string reminderTitle, string reminderDescription, DateTime reminderTime, string taskTitle = "", DateTime? taskDueDate = null)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"EmailService: Scheduling reminder email to {recipientEmail} for {reminderTime:MMM dd, yyyy HH:mm}");

                var client = new SendGridClient(_apiKey);
                var from = new EmailAddress(FromEmail, FromName);
                var to = new EmailAddress(recipientEmail);
                var subject = $"⏰ Reminder: {reminderTitle}";

                var plainTextContent = $"Reminder: {reminderTitle}\n\nDescription: {reminderDescription}\n\nTask: {taskTitle}\n\nTask Due Date: {(taskDueDate?.ToString("MMMM dd, yyyy 'at' h:mm tt") ?? "Not specified")}\n\nThis is an automated reminder from BrainWave.";

                var htmlContent = $@"
                    <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
                        <div style='background: linear-gradient(135deg, #4A90E2, #357ABD); color: white; padding: 25px; text-align: center; border-radius: 12px 12px 0 0;'>
                            <h1 style='margin: 0; font-size: 28px;'>⏰ BrainWave Reminder</h1>
                            <p style='margin: 10px 0 0 0; font-size: 16px; opacity: 0.9;'>Don't miss this important task!</p>
                        </div>
                        
                        <div style='background-color: #f8f9fa; padding: 30px; border-radius: 0 0 12px 12px; border: 1px solid #e9ecef;'>
                            <div style='background-color: #fff; border-left: 4px solid #4A90E2; padding: 20px; margin-bottom: 20px; border-radius: 0 8px 8px 0;'>
                                <h2 style='color: #333; margin-top: 0; font-size: 22px;'>{reminderTitle}</h2>
                                <p style='color: #666; font-size: 16px; line-height: 1.5; margin: 10px 0;'>{reminderDescription}</p>
                            </div>
                            
                            <div style='background-color: #E3F2FD; border: 1px solid #BBDEFB; border-radius: 8px; padding: 20px; margin: 20px 0;'>
                                <h3 style='color: #1976D2; margin-top: 0; font-size: 18px;'>📅 Task Details</h3>
                                <p style='color: #1976D2; font-size: 16px; margin: 5px 0;'>
                                    <strong>📋 Task:</strong> {taskTitle}
                                </p>
                                <p style='color: #1976D2; font-size: 16px; margin: 5px 0;'>
                                    <strong>📅 Due Date:</strong> {(taskDueDate?.ToString("MMMM dd, yyyy 'at' h:mm tt") ?? "Not specified")}
                                </p>
                            </div>
                            
                            <div style='background-color: #E8F5E8; border: 1px solid #C8E6C9; border-radius: 8px; padding: 15px; margin: 20px 0;'>
                                <p style='color: #2E7D32; font-size: 14px; margin: 0; text-align: center;'>
                                    <strong>💡 Tip:</strong> Check your BrainWave app to mark this task as complete or update its status.
                                </p>
                            </div>
                            
                            <div style='margin-top: 30px; padding-top: 20px; border-top: 1px solid #e9ecef;'>
                                <p style='color: #999; font-size: 12px; margin: 0; text-align: center;'>
                                    This is an automated reminder from BrainWave Task Management System
                                    <br>© 2025 BrainWave. All rights reserved.
                                </p>
                            </div>
                        </div>
                    </div>";

        var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
        
        // Set the sendAt property to schedule the email for the reminder time
        // Convert DateTime to Unix timestamp (seconds since epoch)
        var unixTimestamp = ((DateTimeOffset)reminderTime).ToUnixTimeSeconds();
        msg.SendAt = unixTimestamp;
        
        System.Diagnostics.Debug.WriteLine($"Scheduling email for Unix timestamp: {unixTimestamp} (DateTime: {reminderTime:yyyy-MM-dd HH:mm:ss})");
        
        var response = await client.SendEmailAsync(msg);
        
        System.Diagnostics.Debug.WriteLine($"SendGrid reminder email response: {response.StatusCode}");
        System.Diagnostics.Debug.WriteLine($"Email scheduled for delivery at: {reminderTime:yyyy-MM-dd HH:mm:ss}");
                
                return response.IsSuccessStatusCode;
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Reminder email scheduling error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> TestEmailConnectionAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("EmailService: Testing SendGrid connection...");
                
                var client = new SendGridClient(_apiKey);
                var response = await client.RequestAsync(method: SendGridClient.Method.GET, urlPath: "user/account");
                
                System.Diagnostics.Debug.WriteLine($"SendGrid connection test response: {response.StatusCode}");
                return response.IsSuccessStatusCode;
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"EmailService: SendGrid connection test failed: {ex.Message}");
                return false;
            }
        }
    }
}
