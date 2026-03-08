namespace GoiMon.Api.Infrastructure.Email;

public interface IEmailService
{
    Task SendOtpAsync(string toEmail, string otpCode, int expiryMinutes);
}
