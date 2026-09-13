namespace Deuna.Merchant.Sdk.Models.Enums;

/// <summary>
/// Specifies the type of QR code to generate.
/// The DEUNA API currently defines "dynamic" as the standard type.
/// </summary>
public static class QrType
{
    /// <summary>A dynamic QR code that encodes a specific transaction amount.</summary>
    public const string Dynamic = "dynamic";
}
