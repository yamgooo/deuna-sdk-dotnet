using System.Text.Json.Serialization;

namespace Deuna.Merchant.Sdk.Models.Requests;

/// <summary>
/// Request body for the <c>POST /merchant/v1/payment/info</c> endpoint.
/// Retrieves the current status and details of a previously created transaction.
/// </summary>
public sealed class PaymentInfoRequest
{
    /// <summary>
    /// The transaction reference to look up.
    /// <para>
    /// <b>Wire name:</b> <c>idTransacionReference</c> (note the intentional API typo —
    /// the JSON property name exactly matches the DEUNA API contract).
    /// </para>
    /// Required.
    /// </summary>
    [JsonPropertyName("idTransacionReference")]
    public string IdTransactionReference { get; init; } = string.Empty;

    /// <summary>
    /// Type of the identifier provided in <see cref="IdTransactionReference"/>.
    /// Use constants from <see cref="IdType"/>.
    /// </summary>
    [JsonPropertyName("idType")]
    public string IdType { get; init; } = Requests.IdType.TransactionId;
}
