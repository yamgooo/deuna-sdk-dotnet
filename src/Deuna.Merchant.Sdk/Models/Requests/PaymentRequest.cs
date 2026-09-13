using System.Text.Json.Serialization;

namespace Deuna.Merchant.Sdk.Models.Requests;

/// <summary>
/// Request body for the <c>POST /merchant/v1/payment/request</c> endpoint.
/// Instructs DEUNA to create a new payment QR or deeplink for the given amount.
/// </summary>
public sealed class PaymentRequest
{
    /// <summary>
    /// Identifier for the point of sale (POS terminal) initiating the payment.
    /// Required.
    /// </summary>
    [JsonPropertyName("pointOfSale")]
    public string PointOfSale { get; init; } = string.Empty;

    /// <summary>
    /// Type of QR code to generate. Use <see cref="Enums.QrType.Dynamic"/> for standard usage.
    /// Required.
    /// </summary>
    [JsonPropertyName("qrType")]
    public Enums.QrType QrType { get; init; } = Enums.QrType.Dynamic;

    /// <summary>
    /// Expiration time in minutes for a dynamic QR code or link.
    /// Min: 1. Max: 720 (12 hours). Defaults to 720 if omitted on the API side.
    /// Must not be set if <see cref="QrType"/> is <see cref="Enums.QrType.Static"/>.
    /// </summary>
    [JsonPropertyName("expiredTime")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? ExpiredTime { get; init; }

    /// <summary>
    /// Transaction amount (decimal). Must be greater than zero.
    /// Required.
    /// </summary>
    [JsonPropertyName("amount")]
    public decimal Amount { get; init; }

    /// <summary>
    /// Optional free-text description of the transaction displayed to the payer.
    /// </summary>
    [JsonPropertyName("detail")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Detail { get; init; }

    /// <summary>
    /// Merchant's own reference for this transaction. Used to correlate with your
    /// internal systems. Required.
    /// </summary>
    [JsonPropertyName("internalTransactionReference")]
    public string InternalTransactionReference { get; init; } = string.Empty;

    /// <summary>
    /// Controls which fields appear in the response.
    /// Required.
    /// </summary>
    [JsonPropertyName("format")]
    public Enums.QrResponseFormat Format { get; init; } = Enums.QrResponseFormat.QrAndDeeplink;
}
