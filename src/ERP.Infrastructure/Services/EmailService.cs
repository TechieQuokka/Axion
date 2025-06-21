using ERP.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Mail;

namespace ERP.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = true)
        {
            try
            {
                var smtpSettings = _configuration.GetSection("EmailSettings");
                var smtpServer = smtpSettings["SmtpServer"];
                var smtpPort = int.Parse(smtpSettings["SmtpPort"] ?? "587");
                var smtpUsername = smtpSettings["Username"];
                var smtpPassword = smtpSettings["Password"];
                var fromEmail = smtpSettings["FromEmail"];
                var fromName = smtpSettings["FromName"];

                using var client = new SmtpClient(smtpServer, smtpPort)
                {
                    Credentials = new NetworkCredential(smtpUsername, smtpPassword),
                    EnableSsl = true
                };

                var message = new MailMessage
                {
                    From = new MailAddress(fromEmail ?? smtpUsername!, fromName ?? "ERP System"),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = isHtml
                };

                message.To.Add(to);

                await client.SendMailAsync(message);
                _logger.LogInformation("Email sent successfully to {To}", to);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {To}", to);
                return false;
            }
        }

        public async Task<bool> SendEmailAsync(string to, string subject, string body, IEnumerable<string>? cc = null, IEnumerable<string>? bcc = null, bool isHtml = true)
        {
            try
            {
                var smtpSettings = _configuration.GetSection("EmailSettings");
                var smtpServer = smtpSettings["SmtpServer"];
                var smtpPort = int.Parse(smtpSettings["SmtpPort"] ?? "587");
                var smtpUsername = smtpSettings["Username"];
                var smtpPassword = smtpSettings["Password"];
                var fromEmail = smtpSettings["FromEmail"];
                var fromName = smtpSettings["FromName"];

                using var client = new SmtpClient(smtpServer, smtpPort)
                {
                    Credentials = new NetworkCredential(smtpUsername, smtpPassword),
                    EnableSsl = true
                };

                var message = new MailMessage
                {
                    From = new MailAddress(fromEmail ?? smtpUsername!, fromName ?? "ERP System"),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = isHtml
                };

                message.To.Add(to);

                if (cc != null)
                {
                    foreach (var ccEmail in cc)
                    {
                        message.CC.Add(ccEmail);
                    }
                }

                if (bcc != null)
                {
                    foreach (var bccEmail in bcc)
                    {
                        message.Bcc.Add(bccEmail);
                    }
                }

                await client.SendMailAsync(message);
                _logger.LogInformation("Email sent successfully to {To}", to);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {To}", to);
                return false;
            }
        }
    }
}
