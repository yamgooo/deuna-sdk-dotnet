using System.Text.Json.Serialization;

namespace Deuna.Merchant.Sdk.Models.Responses;

/// <summary>
/// Response from the <c>POST /merchant/v1/payment/request</c> endpoint.
/// The set of populated fields depends on the <c>format</c> value sent in the request.
/// </summary>
public sealed class PaymentResponse
{
    /// <summary>
    /// Status code returned by the API. The collection shows <c>"1"</c> for a successful
    /// payment creation. Kept as a string to remain open for future API values.
    /// </summary>
    [JsonPropertyName("status")]
    public string Status { get; init; } = string.Empty;

    /// <summary>
    /// The unique DEUNA transaction identifier for this payment.
    /// Use this value when polling <c>GetInfoAsync</c> or calling <c>CancelAsync</c>.
    /// </summary>
    [JsonPropertyName("transactionId")]
    public string TransactionId { get; init; } = string.Empty;

    /// <summary>
    /// Deep-link URL that opens the DEUNA payment flow in a mobile app.
    /// Present when <c>format</c> is <see cref="Enums.QrResponseFormat.DeeplinkOnly"/>
    /// or <see cref="Enums.QrResponseFormat.QrAndDeeplink"/>.
    /// </summary>
    [JsonPropertyName("deeplink")]
    public string? Deeplink { get; init; }

    /// <summary>
    /// Base-64 encoded PNG image as a data URI (<c>data:image/png;base64,…</c>)
    /// representing the QR code.
    /// Present when <c>format</c> is <see cref="Enums.QrResponseFormat.QrOnly"/>
    /// or <see cref="Enums.QrResponseFormat.QrAndDeeplink"/>.
    /// </summary>
    [JsonPropertyName("qr")]
    public string? Qr { get; init; }
}
