using System.Text.Json.Serialization;

namespace Deuna.Merchant.Sdk.Models.Responses;

/// <summary>
/// Error payload returned by the DEUNA API on failure (e.g., HTTP 400).
/// Observed from the refund-already-processed scenario in the Postman collection:
/// <code>{ "statusCode": 400, "message": "Refund was already processed…" }</code>
/// </summary>
public sealed class DeunaErrorResponse
{
    /// <summary>The HTTP status code echoed in the response body.</summary>
    [JsonPropertyName("statusCode")]
    public int StatusCode { get; init; }

    /// <summary>Human-readable error description from the API.</summary>
    [JsonPropertyName("message")]
    public string Message { get; init; } = string.Empty;
}
