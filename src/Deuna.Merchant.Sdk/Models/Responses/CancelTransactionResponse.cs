using System.Text.Json.Serialization;

namespace Deuna.Merchant.Sdk.Models.Responses;

/// <summary>
/// Response from the <c>POST /merchant/v1/payment/cancel</c> endpoint.
/// </summary>
public sealed class CancelTransactionResponse
{
    /// <summary>The transaction identifier that was cancelled.</summary>
    [JsonPropertyName("transactionId")]
    public string TransactionId { get; init; } = string.Empty;

    /// <summary>
    /// Human-readable confirmation message from the API,
    /// e.g. <c>"The QR with id 250 has been successfully cleaned"</c>.
    /// </summary>
    [JsonPropertyName("message")]
    public string Message { get; init; } = string.Empty;
}
