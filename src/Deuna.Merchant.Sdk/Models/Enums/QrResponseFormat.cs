namespace Deuna.Merchant.Sdk.Models.Enums;

/// <summary>
/// Controls which fields are included in the <c>RequestPayment</c> response.
/// The DEUNA API uses numeric string values; this class exposes named constants
/// for the documented formats while remaining open for undocumented values.
/// </summary>
/// <remarks>
/// Observed format semantics from the Postman collection:
/// <list type="table">
///   <item><term><see cref="DeeplinkOnly"/></term><description>Response includes only <c>deeplink</c>.</description></item>
///   <item><term><see cref="QrOnly"/></term><description>Response includes only <c>qr</c> (base-64 PNG data URI).</description></item>
///   <item><term><see cref="QrAndDeeplink"/></term><description>Response includes both <c>qr</c> and <c>deeplink</c>.</description></item>
///   <item><term><see cref="Full"/></term><description>Full response (observed format "5" in the collection).</description></item>
/// </list>
/// Pass any other string value to use undocumented formats.
/// </remarks>
public static class QrResponseFormat
{
    /// <summary>Format "0" — response contains <c>deeplink</c> only.</summary>
    public const string DeeplinkOnly = "0";

    /// <summary>Format "1" — response contains <c>qr</c> (base-64 PNG data URI) only.</summary>
    public const string QrOnly = "1";

    /// <summary>Format "2" — response contains both <c>qr</c> and <c>deeplink</c>.</summary>
    public const string QrAndDeeplink = "2";

    /// <summary>Format "5" — full response (all available fields).</summary>
    public const string Full = "5";
}
