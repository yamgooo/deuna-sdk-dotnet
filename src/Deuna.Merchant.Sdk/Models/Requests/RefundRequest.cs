using System.Text.Json.Serialization;

namespace Deuna.Merchant.Sdk.Models.Requests;

/// <summary>
/// Request body for the <c>POST /merchant/v1/payment/void</c> endpoint.
/// Initiates a refund (reversal) for an already-completed transaction.
/// </summary>
public sealed class RefundRequest
{
    /// <summary>
    /// The bank transfer number associated with the approved transaction to reverse.
    /// Obtained from the <c>transferNumber</c> field of a
    /// <see cref="Responses.PaymentInfoResponse"/> with status <c>APPROVED</c>.
    /// Required.
    /// </summary>
    [JsonPropertyName("transferNumber")]
    public string TransferNumber { get; init; } = string.Empty;
}
