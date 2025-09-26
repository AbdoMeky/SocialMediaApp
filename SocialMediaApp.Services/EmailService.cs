using Microsoft.Extensions.Options;
using SocialMediaApp.Core.DTO.ResultDTO;
using SocialMediaApp.Core.Helper;
using SocialMediaApp.Core.Services;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;

namespace SocialMediaApp.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings emailSettings;

        public EmailService(IOptions<EmailSettings> emailSettings)
        {
            this.emailSettings = emailSettings.Value;
        }
        public string GenerateVerificatonCode(int length = 6)
        {
            var randomNumber = new byte[length];
            RandomNumberGenerator.Fill(randomNumber);
            return string.Join("", randomNumber.Select(b => (b % 10).ToString()));
        }

        public async Task<IntResult> SendEmailAsync(string toEmail, string subject, string message)
        {
            try
            {
                string? email = emailSettings.Email;
                var password = emailSettings.Password;

                using (var client = new SmtpClient(emailSettings.SmtpServer, 587))
                {
                    client.Credentials = new NetworkCredential(email, password);
                    client.UseDefaultCredentials = false;
                    client.EnableSsl = true;
                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(email),
                        Subject = subject,
                        Body = message,
                        IsBodyHtml = true,
                    };
                    mailMessage.To.Add(toEmail);
                    await client.SendMailAsync(mailMessage);
                    return new IntResult { Id = 1 };
                }
            }
            catch (Exception ex)
            {
                return new IntResult { Message = ex.Message };
            }
        }
    }
}
