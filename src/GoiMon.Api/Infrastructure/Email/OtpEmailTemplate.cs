using System.Reflection;

namespace GoiMon.Api.Infrastructure.Email;

public static class OtpEmailTemplate
{
    private static readonly string _template = LoadTemplate();

    public static string Subject => "Your GoiMon verification code";

    public static string Build(string otpCode, int expiryMinutes) =>
        _template
            .Replace("{{otp_code}}", otpCode)
            .Replace("{{expiry_minutes}}", expiryMinutes.ToString())
            .Replace("{{year}}", DateTime.UtcNow.Year.ToString());

    private static string LoadTemplate()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = assembly.GetManifestResourceNames()
            .FirstOrDefault(n => n.EndsWith("otp-email.html"))
            ?? throw new InvalidOperationException("Embedded resource 'otp-email.html' not found.");

        using var stream = assembly.GetManifestResourceStream(resourceName)!;
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
