using System.Text.Json.Serialization;

namespace Deuna.Merchant.Sdk.Models.Webhooks;

/// <summary>
/// Payload sent by DEUNA to your webhook endpoint upon a successful payment.
/// </summary>
public sealed class DeunaPaymentWebhookPayload
{
    /// <summary>Status of the transaction (e.g., "SUCCESS").</summary>
    [JsonPropertyName("status")]
    public string Status { get; init; } = string.Empty;

    /// <summary>Transaction amount.</summary>
    [JsonPropertyName("amount")]
    public decimal Amount { get; init; }

    /// <summary>DEUNA-issued transaction identifier.</summary>
    [JsonPropertyName("idTransaction")]
    public string IdTransaction { get; init; } = string.Empty;

    /// <summary>Merchant's internal transaction reference.</summary>
    [JsonPropertyName("internalTransactionReference")]
    public string InternalTransactionReference { get; init; } = string.Empty;

    /// <summary>Bank transfer number.</summary>
    [JsonPropertyName("transferNumber")]
    public string TransferNumber { get; init; } = string.Empty;

    /// <summary>Raw date/time string.</summary>
    [JsonPropertyName("date")]
    public string Date { get; init; } = string.Empty;

    /// <summary>Merchant branch identifier.</summary>
    [JsonPropertyName("branchId")]
    public string BranchId { get; init; } = string.Empty;

    /// <summary>Point-of-sale identifier.</summary>
    [JsonPropertyName("posId")]
    public string PosId { get; init; } = string.Empty;

    /// <summary>Currency code (e.g., "USD").</summary>
    [JsonPropertyName("currency")]
    public string Currency { get; init; } = string.Empty;

    /// <summary>Transaction description.</summary>
    [JsonPropertyName("description")]
    public string Description { get; init; } = string.Empty;

    /// <summary>National identification number of the payer.</summary>
    [JsonPropertyName("customerIdentification")]
    public string CustomerIdentification { get; init; } = string.Empty;

    /// <summary>Full name of the payer.</summary>
    [JsonPropertyName("customerFullName")]
    public string CustomerFullName { get; init; } = string.Empty;
}
