namespace GoiMon.Api.Infrastructure.Email;

public class EmailSettings
{
    /// <summary>SMTP server hostname. Hotmail: smtp-mail.outlook.com | Gmail: smtp.gmail.com</summary>
    public string Host { get; set; } = string.Empty;

    /// <summary>SMTP port. Use 587 for STARTTLS (recommended).</summary>
    public int Port { get; set; } = 587;

    /// <summary>Enable STARTTLS. Always true for port 587.</summary>
    public bool EnableSsl { get; set; } = true;

    /// <summary>Your email address (sender).</summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>App password (not your account password). See docs for setup.</summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>Display name shown in the From field.</summary>
    public string DisplayName { get; set; } = "GoiMon";
}
