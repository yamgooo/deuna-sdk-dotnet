using System.Text.Json.Serialization;

namespace Deuna.Merchant.Sdk.Models.Requests;

/// <summary>
/// Request body for the <c>POST /merchant/v1/payment/cancel</c> endpoint.
/// Cancels a pending (unpaid) payment transaction.
/// </summary>
public sealed class CancelTransactionRequest
{
    /// <summary>
    /// The unique DEUNA transaction identifier to cancel.
    /// Required.
    /// </summary>
    [JsonPropertyName("transactionId")]
    public string TransactionId { get; init; } = string.Empty;

    /// <summary>
    /// The point-of-sale identifier that originally created the transaction.
    /// Required.
    /// </summary>
    [JsonPropertyName("pointOfSale")]
    public string PointOfSale { get; init; } = string.Empty;
}
