using System.Globalization;
using System.Text.Json.Serialization;

namespace Deuna.Merchant.Sdk.Models.Responses;

/// <summary>
/// Response from the <c>POST /merchant/v1/payment/info</c> endpoint.
/// Contains current status and full details of a transaction.
/// </summary>
public sealed class PaymentInfoResponse
{
    /// <summary>
    /// Current status of the transaction. Open string — do not switch exhaustively.
    /// Known values from the API: <c>"PENDING"</c>, <c>"APPROVED"</c>, <c>"REVERSED"</c>, <c>"REVERSED_FAILED"</c>.
    /// </summary>
    [JsonPropertyName("status")]
    public string Status { get; init; } = string.Empty;

    /// <summary>
    /// The merchant's internal reference that was submitted when creating the payment.
    /// May be empty when the transaction is still <c>PENDING</c>.
    /// </summary>
    [JsonPropertyName("internalTransactionReference")]
    public string InternalTransactionReference { get; init; } = string.Empty;

    /// <summary>Transaction amount.</summary>
    [JsonPropertyName("amount")]
    public decimal Amount { get; init; }

    /// <summary>The unique DEUNA transaction identifier.</summary>
    [JsonPropertyName("transactionId")]
    public string TransactionId { get; init; } = string.Empty;

    /// <summary>
    /// Bank transfer number assigned once the payment is approved.
    /// Use this value when calling <c>RefundAsync</c>.
    /// Empty when the transaction is still <c>PENDING</c>.
    /// </summary>
    [JsonPropertyName("transferNumber")]
    public string TransferNumber { get; init; } = string.Empty;

    /// <summary>
    /// Raw date/time string as returned by the API.
    /// The API's format is inconsistent across environments (e.g., <c>"7/5/2024, 1:47:29 PM"</c>).
    /// Use <see cref="TryGetParsedDate"/> for a parsed value.
    /// Empty when the transaction is still <c>PENDING</c>.
    /// </summary>
    [JsonPropertyName("date")]
    public string Date { get; init; } = string.Empty;

    /// <summary>Merchant branch identifier.</summary>
    [JsonPropertyName("branchId")]
    public string BranchId { get; init; } = string.Empty;

    /// <summary>Point-of-sale identifier.</summary>
    [JsonPropertyName("posId")]
    public string PosId { get; init; } = string.Empty;

    /// <summary>Currency code (e.g., <c>"USD"</c>).</summary>
    [JsonPropertyName("currency")]
    public string Currency { get; init; } = string.Empty;

    /// <summary>Transaction description as submitted.</summary>
    [JsonPropertyName("description")]
    public string Description { get; init; } = string.Empty;

    /// <summary>Full name of the payer.</summary>
    [JsonPropertyName("ordererName")]
    public string OrdererName { get; init; } = string.Empty;

    /// <summary>National identification number of the payer.</summary>
    [JsonPropertyName("ordererIdentification")]
    public string OrdererIdentification { get; init; } = string.Empty;

    /// <summary>
    /// Attempts to parse <see cref="Date"/> into a <see cref="DateTimeOffset"/>.
    /// Returns <see langword="null"/> if <see cref="Date"/> is null, empty, or cannot be parsed.
    /// </summary>
    /// <returns>
    /// A parsed <see cref="DateTimeOffset"/>, or <see langword="null"/> on failure.
    /// </returns>
    public DateTimeOffset? TryGetParsedDate()
    {
        if (string.IsNullOrWhiteSpace(Date) || Date.Length > 23)
        {
            return null;
        }

        // Try several formats observed in the API and common variations.
        string[] formats =
        [
            "M/d/yyyy, h:mm:ss tt",
            "M/d/yyyy, H:mm:ss",
            "MM/dd/yyyy, h:mm:ss tt",
            "MM/dd/yyyy HH:mm:ss",
            "yyyy-MM-ddTHH:mm:ssZ",
            "yyyy-MM-ddTHH:mm:ss.fffZ",
        ];

        if (DateTimeOffset.TryParseExact(
                Date,
                formats,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AssumeLocal,
                out var result))
        {
            return result;
        }

        // Fallback: let the runtime try any recognisable format.
        if (DateTimeOffset.TryParse(Date, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out var fallback))
        {
            return fallback;
        }

        return null;
    }
}
