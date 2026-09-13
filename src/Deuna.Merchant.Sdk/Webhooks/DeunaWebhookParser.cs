using System.Text.Json;
using Deuna.Merchant.Sdk.Models.Webhooks;
using Deuna.Merchant.Sdk.Serialization;

namespace Deuna.Merchant.Sdk.Webhooks;

/// <summary>
/// Helper class for consuming DEUNA webhooks.
/// </summary>
public static class DeunaWebhookParser
{
    /// <summary>
    /// Deserializes the JSON payload sent by DEUNA to a <see cref="DeunaPaymentWebhookPayload"/> object.
    /// Note: DEUNA does not document any signature validation for webhooks. 
    /// Ensure your endpoint is secured by IP allowlisting or a secret token in the URL/headers.
    /// </summary>
    /// <param name="jsonPayload">The raw JSON string received from DEUNA.</param>
    /// <returns>The deserialized payload.</returns>
    /// <exception cref="JsonException">Thrown if the JSON is malformed or incompatible.</exception>
    public static DeunaPaymentWebhookPayload Parse(string jsonPayload)
    {
        return JsonSerializer.Deserialize(jsonPayload, DeunaJsonContext.Default.DeunaPaymentWebhookPayload)
            ?? throw new JsonException("Failed to deserialize the DEUNA webhook payload.");
    }
}
