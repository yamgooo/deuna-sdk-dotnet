using System.Text.Json.Serialization;

namespace Deuna.Merchant.Sdk.Models.Responses;

/// <summary>
/// Response from the <c>POST /merchant/v1/payment/void</c> endpoint
/// when the refund is processed successfully.
/// </summary>
public sealed class RefundResponse
{
    /// <summary>
    /// Indicates whether the refund was executed successfully.
    /// </summary>
    [JsonPropertyName("status")]
    public bool Status { get; init; }

    /// <summary>
    /// Human-readable confirmation message,
    /// e.g. <c>"Refund executed successfully for transferNumber 018928363049"</c>.
    /// </summary>
    [JsonPropertyName("message")]
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// The identifier for the reversal transaction created by DEUNA.
    /// </summary>
    [JsonPropertyName("transactionReverseId")]
    public string TransactionReverseId { get; init; } = string.Empty;
}
