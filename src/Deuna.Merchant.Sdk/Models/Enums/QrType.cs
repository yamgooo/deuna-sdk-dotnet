using System.Text.Json.Serialization;

namespace Deuna.Merchant.Sdk.Models.Enums;

/// <summary>
/// Specifies the type of QR code to generate.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<QrType>))]
public enum QrType
{
    /// <summary>
    /// A fixed QR code permanently tied to a point of sale. 
    /// Expiration time must not be set for this type.
    /// </summary>
    [JsonStringEnumMemberName("static")]
    Static,

    /// <summary>
    /// A dynamic, single-use QR code or payment link.
    /// Supports setting an expiration time.
    /// </summary>
    [JsonStringEnumMemberName("dynamic")]
    Dynamic
}
