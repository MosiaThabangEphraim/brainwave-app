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
                var client = new SendGridClient(_apiKey);
                var from = new EmailAddress(FromEmail, FromName);
                var to = new EmailAddress(recipientEmail);
                var subject = "Password Reset - BrainWave App";
                var plainTextContent = $"Please use the following token to reset your password: {resetToken}";
                var htmlContent = $@"
                    <html>
                    <body>
                        <h2>Password Reset Request</h2>
                        <p>You have requested to reset your password for BrainWave App.</p>
                        <p>Please use the following token to reset your password:</p>
                        <p><strong>{resetToken}</strong></p>
                        <p>This token will expire in 1 hour.</p>
                        <p>If you did not request this password reset, please ignore this email.</p>
                        <br>
                        <p>Best regards,<br>BrainWave Support Team</p>
                    </body>
                    </html>";

                var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
                var response = await client.SendEmailAsync(msg);

                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> SendWelcomeEmailAsync(string recipientEmail, string userName)
        {
            try
            {
                var client = new SendGridClient(_apiKey);
                var from = new EmailAddress(FromEmail, FromName);
                var to = new EmailAddress(recipientEmail);
                var subject = "Welcome to BrainWave App!";
                var plainTextContent = $"Welcome to BrainWave App, {userName}! We're excited to have you on board.";
                var htmlContent = $@"
                    <html>
                    <body>
                        <h2>Welcome to BrainWave App!</h2>
                        <p>Hello {userName},</p>
                        <p>Welcome to BrainWave App! We're excited to have you on board.</p>
                        <p>With BrainWave, you can:</p>
                        <ul>
                            <li>Manage your tasks efficiently</li>
                            <li>Set up reminders for important events</li>
                            <li>Collaborate with team members</li>
                            <li>Track your productivity</li>
                        </ul>
                        <p>Get started by exploring the dashboard and creating your first task!</p>
                        <br>
                        <p>Best regards,<br>BrainWave Support Team</p>
                    </body>
                    </html>";

                var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
                var response = await client.SendEmailAsync(msg);

                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> SendNotificationEmailAsync(string recipientEmail, string subject, string message)
        {
            try
            {
                var client = new SendGridClient(_apiKey);
                var from = new EmailAddress(FromEmail, FromName);
                var to = new EmailAddress(recipientEmail);
                var plainTextContent = message;
                var htmlContent = $@"
                    <html>
                    <body>
                        <h2>BrainWave Notification</h2>
                        <p>{message}</p>
                        <br>
                        <p>Best regards,<br>BrainWave Support Team</p>
                    </body>
                    </html>";

                var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
                var response = await client.SendEmailAsync(msg);

                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }
}
