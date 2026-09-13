using System.Text.Json.Serialization;
using Deuna.Merchant.Sdk.Models.Requests;
using Deuna.Merchant.Sdk.Models.Responses;

namespace Deuna.Merchant.Sdk.Serialization;

/// <summary>
/// Source-generated <see cref="JsonSerializerContext"/> for all DEUNA SDK types.
/// Enables AOT-friendly, reflection-free serialization for the hot path.
/// </summary>
[JsonSerializable(typeof(PaymentRequest))]
[JsonSerializable(typeof(PaymentInfoRequest))]
[JsonSerializable(typeof(CancelTransactionRequest))]
[JsonSerializable(typeof(RefundRequest))]
[JsonSerializable(typeof(PaymentResponse))]
[JsonSerializable(typeof(PaymentInfoResponse))]
[JsonSerializable(typeof(CancelTransactionResponse))]
[JsonSerializable(typeof(RefundResponse))]
[JsonSerializable(typeof(DeunaErrorResponse))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    WriteIndented = false,
    DefaultIgnoreCondition = JsonIgnoreCondition.Never)]
public partial class DeunaJsonContext : JsonSerializerContext
{
}
