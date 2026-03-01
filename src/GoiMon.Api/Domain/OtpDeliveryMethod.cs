namespace GoiMon.Api.Domain;

/// <summary>
/// Delivery method for One-Time Password verification.
/// </summary>
public enum OtpDeliveryMethod
{
    /// <summary>
    /// Send OTP via email.
    /// </summary>
    Email = 0,

    /// <summary>
    /// Send OTP via SMS.
    /// </summary>
    Sms = 1
}
