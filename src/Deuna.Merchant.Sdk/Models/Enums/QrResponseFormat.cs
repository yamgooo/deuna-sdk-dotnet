namespace Deuna.Merchant.Sdk.Models.Enums;

/// <summary>
/// Controls which fields are included in the <c>RequestPayment</c> response.
/// The DEUNA API uses numeric string values; this class exposes named constants
/// for the documented formats while remaining open for undocumented values.
/// </summary>
public enum QrResponseFormat
{
    /// <summary>
    /// QR only, returned as a base64 string (500x500px).
    /// </summary>
    QrOnly = 0,

    /// <summary>
    /// Payment link (<c>deeplink</c>) only.
    /// </summary>
    DeeplinkOnly = 1,

    /// <summary>
    /// Both QR base64 and <c>deeplink</c>.
    /// </summary>
    QrAndDeeplink = 2,

    /// <summary>
    /// 6-digit numeric code (<c>numericCode</c>). Fixed 3-minute expiry.
    /// </summary>
    NumericCode = 3,

    /// <summary>
    /// Temporary 6-digit code + QR base64.
    /// </summary>
    NumericCodeAndQr = 4
}
