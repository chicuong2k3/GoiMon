using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;

namespace GoiMon.Api.Infrastructure.Email;

public class SmtpEmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IOptions<EmailSettings> settings, ILogger<SmtpEmailService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SendOtpAsync(string toEmail, string otpCode, int expiryMinutes)
    {
        var subject = OtpEmailTemplate.Subject;
        var body = OtpEmailTemplate.Build(otpCode, expiryMinutes);

        using var message = new MailMessage
        {
            From = new MailAddress(_settings.Username, _settings.DisplayName),
            Subject = subject,
            Body = body,
            IsBodyHtml = true,
        };
        message.To.Add(toEmail);

        using var client = new SmtpClient(_settings.Host, _settings.Port)
        {
            Credentials = new NetworkCredential(_settings.Username, _settings.Password),
            EnableSsl = _settings.EnableSsl,
            DeliveryMethod = SmtpDeliveryMethod.Network,
        };

        try
        {
            await client.SendMailAsync(message);
            _logger.LogInformation("OTP email sent to {Email}", toEmail);
        }
        catch (SmtpException ex)
        {
            _logger.LogError(ex, "Failed to send OTP email to {Email}. Check Email settings in appsettings.", toEmail);
            throw;
        }
    }
}
