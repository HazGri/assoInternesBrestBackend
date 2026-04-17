using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace AssoInternesBrest.API.Services
{
    public class EmailService(IConfiguration configuration) : IEmailService
    {
        private readonly IConfiguration _configuration = configuration;

        public async Task SendAsync(string to, string subject, string body)
        {
            MimeMessage message = new MimeMessage();
            message.From.Add(new MailboxAddress(
                _configuration["Smtp:FromName"]!,
                _configuration["Smtp:FromEmail"]!));
            message.To.Add(MailboxAddress.Parse(to));
            message.Subject = subject;
            message.Body = new TextPart("plain") { Text = body };

            using SmtpClient client = new SmtpClient();
            await client.ConnectAsync(
                _configuration["Smtp:Host"]!,
                int.Parse(_configuration["Smtp:Port"]!),
                SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(
                _configuration["Smtp:Username"]!,
                _configuration["Smtp:Password"]!);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}
