using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace UserService.Infrastructure.SendEmail
{
    public class SendEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SendEmailService> _logger; // Thêm logger

        public SendEmailService(IConfiguration configuration, ILogger<SendEmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendVerifyCodeEmailAsync(string email, string verifyCode)
        {
            try
            {
                var message = new MimeMessage();
                string fromEmail = _configuration["Smtp:FromEmail"] ?? throw new ArgumentNullException("Smtp:FromEmail not configured.");
                string smtpHost = _configuration["Smtp:Host"] ?? throw new ArgumentNullException("Smtp:Host not configured.");
                // Sử dụng TryParse để an toàn hơn khi parse cổng
                if (!int.TryParse(_configuration["Smtp:Port"], out int smtpPort))
                {
                    throw new InvalidOperationException("Smtp:Port is not configured correctly or is not a valid number.");
                }
                string smtpUsername = _configuration["Smtp:Username"] ?? throw new ArgumentNullException("Smtp:Username not configured.");
                string smtpPassword = _configuration["Smtp:Password"] ?? throw new ArgumentNullException("Smtp:Password not configured.");

                message.From.Add(new MailboxAddress("YourApp", fromEmail));
                message.To.Add(new MailboxAddress("", email));
                message.Subject = "Forget Password Request";

                message.Body = new TextPart("plain")
                {
                    Text = $"Mã xác thực của bạn là: {verifyCode}"
                };

                using var client = new SmtpClient();
                // Kết nối với TLS/SSL
                await client.ConnectAsync(smtpHost, smtpPort, SecureSocketOptions.StartTls).ConfigureAwait(false);
                // Hoặc SecureSocketOptions.SslOnConnect nếu port là 465

                await client.AuthenticateAsync(smtpUsername, smtpPassword).ConfigureAwait(false);
                await client.SendAsync(message).ConfigureAwait(false);
                await client.DisconnectAsync(true).ConfigureAwait(false);

                _logger.LogInformation("Verification email sent successfully to {Email}.", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while sending verification email to {Email}.", email);
                throw; // Ném lại mọi lỗi khác
            }
        }
    }
}
