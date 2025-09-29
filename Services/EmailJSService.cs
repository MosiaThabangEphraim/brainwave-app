using Newtonsoft.Json;
using System.Text;

namespace BrainWave.APP.Services
{
    public class EmailJSService
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private const string EmailJsUrl = "https://api.emailjs.com/api/v1.0/email/send";
        
        // EmailJS configuration - replace with your actual values
        private const string ServiceId = "service_ds2a2iq";
        private const string TemplateId = "template_aswsjfg";
        private const string PublicKey = "WG7hJxWO7LUL_z0mC";
        private const string ToEmail = "mybrainwave@outlook.com";

        public async Task<(bool success, string errorMessage)> SendContactEmailAsync(string name, string email, string subject, string message)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"EmailJSService: Sending contact email from {name} ({email})");
                System.Diagnostics.Debug.WriteLine($"EmailJSService: Using ServiceId: {ServiceId}, TemplateId: {TemplateId}, PublicKey: {PublicKey}");

                var emailData = new
                {
                    service_id = ServiceId,
                    template_id = TemplateId,
                    user_id = PublicKey,
                    template_params = new
                    {
                        name = name,
                        email = email,
                        subject = subject,
                        message = message,
                        to_email = ToEmail
                    }
                };

                var json = JsonConvert.SerializeObject(emailData);
                System.Diagnostics.Debug.WriteLine($"EmailJSService: Request JSON: {json}");
                
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(EmailJsUrl, content);
                
                System.Diagnostics.Debug.WriteLine($"EmailJS response status: {response.StatusCode}");
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"EmailJS response content: {responseContent}");
                    return (true, "Email sent successfully");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"EmailJS error response: {errorContent}");
                    return (false, $"EmailJS API Error: {response.StatusCode} - {errorContent}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"EmailJS Error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"EmailJS Stack Trace: {ex.StackTrace}");
                return (false, $"Connection Error: {ex.Message}");
            }
        }

        public async Task<bool> TestEmailJSConnectionAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("EmailJSService: Testing EmailJS connection...");
                
                // Send a test email with minimal data
                var (success, errorMessage) = await SendContactEmailAsync(
                    "Test User", 
                    "test@example.com", 
                    "Test Email", 
                    "This is a test email to verify EmailJS connection."
                );
                
                System.Diagnostics.Debug.WriteLine($"EmailJS connection test result: {success}");
                if (!success)
                {
                    System.Diagnostics.Debug.WriteLine($"EmailJS test error: {errorMessage}");
                }
                return success;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"EmailJSService: EmailJS connection test failed: {ex.Message}");
                return false;
            }
        }
    }
}
