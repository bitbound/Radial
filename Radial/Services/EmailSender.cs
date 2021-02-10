using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Radial.Services
{
    public interface IEmailSenderEx
    {
        public Task<bool> TrySendEmail(string email, string subject, string htmlMessage);
    }

    public class EmailSender : IEmailSender, IEmailSenderEx
    {
        private readonly IApplicationConfig _appConfig;
        private readonly ILogger<EmailSender> _logger;

        public EmailSender(IApplicationConfig appConfig, ILogger<EmailSender> logger)
        {
            _appConfig = appConfig;
            _logger = logger;
        }


     

        public Task<bool> TrySendEmail(string email, string subject, string htmlMessage)
        {
            try
            {
                SendEmail(email, subject, htmlMessage);
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email.");
                return Task.FromResult(false);
            }
        }

        private void SendEmail(string email, string subject, string htmlMessage)
        {
            var mailClient = new SmtpClient
            {
                Host = _appConfig.SmtpHost,
                Port = _appConfig.SmtpPort,
                EnableSsl = _appConfig.SmtpEnableSsl,
                Credentials = new NetworkCredential(_appConfig.SmtpUserName, _appConfig.SmtpPassword),
                DeliveryMethod = SmtpDeliveryMethod.Network
            };

            var from = new MailAddress(_appConfig.SmtpEmail, _appConfig.SmtpDisplayName, System.Text.Encoding.UTF8);
            var to = new MailAddress(email);

            var mailMessage = new MailMessage(from, to)
            {
                IsBodyHtml = true,
                Subject = subject,
                Body = htmlMessage
            };

            mailClient.Send(mailMessage);
            _logger.LogInformation($"Email successfully sent to {email}.  Subject: \"{subject}\".");
        }
    }
}
