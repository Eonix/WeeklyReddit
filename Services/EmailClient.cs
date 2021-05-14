using System;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MimeKit;

namespace WeeklyReddit.Services
{
    public sealed class EmailClient : IDisposable
    {
        private readonly EmailOptions _options;
        private readonly SmtpClient _smtpClient = new SmtpClient();

        public EmailClient(EmailOptions options)
        {
            _options = options;
            _smtpClient.ServerCertificateValidationCallback += (_, __, ___, ____) => true;
            _smtpClient.AuthenticationMechanisms.Remove("XOAUTH2");
        }

        public async Task SendAsync(EmailContent content)
        {
            var mimeMessage = new MimeMessage();
            mimeMessage.From.Add(new MailboxAddress(content.FromName, content.FromAddress));
            mimeMessage.To.Add(MailboxAddress.Parse(content.To));
            mimeMessage.Subject = content.Subject;

            mimeMessage.Body = new TextPart("html") { Text = content.Content };

            await _smtpClient.ConnectAsync(_options.SmtpServer, _options.SmtpPort);
            await _smtpClient.AuthenticateAsync(_options.Username, _options.Password);

            await _smtpClient.SendAsync(mimeMessage);
            await _smtpClient.DisconnectAsync(true);
        }

        public void Dispose()
        {
            _smtpClient.Dispose();
        }
    }
}