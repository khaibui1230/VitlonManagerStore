using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net.Mail;
using System.Net;
using System.Threading.Tasks;

namespace QuanVitLonManager.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailSender> _logger;

        public EmailSender(IConfiguration configuration, ILogger<EmailSender> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            try
            {
                var smtpSettings = _configuration.GetSection("SmtpSettings");
                var client = new SmtpClient
                {
                    Host = smtpSettings["Host"] ?? "smtp.gmail.com",
                    Port = int.Parse(smtpSettings["Port"] ?? "587"),
                    EnableSsl = true,
                    Credentials = new NetworkCredential(
                        smtpSettings["Username"],
                        smtpSettings["Password"]
                    )
                };

                await client.SendMailAsync(
                    new MailMessage(
                        from: smtpSettings["Username"] ?? "",
                        to: email,
                        subject: subject,
                        body: htmlMessage
                    )
                    {
                        IsBodyHtml = true
                    }
                );

                _logger.LogInformation($"Email sent successfully to {email}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to send email to {email}. Error: {ex.Message}");
                throw;
            }
        }
    }
}