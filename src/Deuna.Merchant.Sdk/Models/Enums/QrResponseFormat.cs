using System.Text.Json.Serialization;
using Deuna.Merchant.Sdk.Serialization;

namespace Deuna.Merchant.Sdk.Models.Enums;

/// <summary>
/// Controls which fields are included in the <c>POST /merchant/v1/payment/request</c> response.
/// <para>
/// The DEUNA API requires these as numeric string values (<c>"0"</c> to <c>"5"</c>).
/// This enum provides strongly-typed, documented constants serialized as strings via
/// <see cref="QrResponseFormatJsonConverter"/>.
/// </para>
/// </summary>
[JsonConverter(typeof(QrResponseFormatJsonConverter))]
public enum QrResponseFormat
{
    /// <summary>
    /// Payment link (<c>deeplink</c>) only.
    /// Wire value: <c>"0"</c>.
    /// </summary>
    DeeplinkOnly = 0,

    /// <summary>
    /// QR code base64 image (<c>qr</c>) only.
    /// Wire value: <c>"1"</c>.
    /// </summary>
    QrOnly = 1,

    /// <summary>
    /// Both QR code base64 and payment link (<c>deeplink</c>).
    /// Wire value: <c>"2"</c>.
    /// </summary>
    QrAndDeeplink = 2,

    /// <summary>
    /// 6-digit numeric code (<c>numericCode</c>). Fixed 3-minute expiry.
    /// Wire value: <c>"3"</c>.
    /// </summary>
    NumericCode = 3,

    /// <summary>
    /// Temporary 6-digit code (<c>numericCode</c>) and QR code base64 (<c>qr</c>).
    /// Wire value: <c>"4"</c>.
    /// </summary>
    NumericCodeAndQr = 4,

    /// <summary>
    /// Complete format: QR base64 (<c>qr</c>), payment link (<c>deeplink</c>), and 6-digit code (<c>numericCode</c>).
    /// Wire value: <c>"5"</c>.
    /// </summary>
    All = 5,
}
