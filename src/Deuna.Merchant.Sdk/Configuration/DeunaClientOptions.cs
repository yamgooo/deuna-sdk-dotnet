using System.ComponentModel.DataAnnotations;

namespace Deuna.Merchant.Sdk.Configuration;

/// <summary>
/// Configuration options for the DEUNA Merchant SDK client.
/// Bind this class to your <c>appsettings.json</c> section or populate it
/// programmatically when calling <c>AddDeunaMerchantClient</c>.
/// </summary>
public sealed class DeunaClientOptions
{
    /// <summary>
    /// The configuration section name used when binding from <c>IConfiguration</c>.
    /// </summary>
    public const string SectionName = "DeunaClient";

    /// <summary>
    /// The base URL of the DEUNA Merchant API.
    /// <para>
    /// Production: <c>https://apis-merchant.pdn.deunalab.com</c><br/>
    /// Development: <c>https://deuna-dev.apigee.net</c><br/>
    /// Test/Staging: <c>https://deuna-test.apigee.net</c>
    /// </para>
    /// Required. Must be a valid absolute URI.
    /// </summary>
    [Required]
    [Url]
    public string BaseUrl { get; set; } = "https://apis-merchant.pdn.deunalab.com";

    /// <summary>
    /// Your DEUNA API key. Sent as the <c>x-api-key</c> request header.
    /// Required. Never log or expose this value.
    /// </summary>
    [Required]
    [MinLength(1)]
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Your DEUNA API secret. Sent as the <c>x-api-secret</c> request header.
    /// Required. Never log or expose this value.
    /// </summary>
    [Required]
    [MinLength(1)]
    public string ApiSecret { get; set; } = string.Empty;

    /// <summary>
    /// Total timeout for an individual HTTP request, including all retry attempts.
    /// Defaults to 30 seconds.
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Maximum number of retry attempts on transient failures.
    /// Defaults to 3. Set to 0 to disable retries.
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>
    /// Maximum delay between retry attempts (used as the ceiling for exponential back-off with jitter).
    /// Defaults to 15 seconds.
    /// </summary>
    public TimeSpan MaxRetryDelay { get; set; } = TimeSpan.FromSeconds(15);
}
