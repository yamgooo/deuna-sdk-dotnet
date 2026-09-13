using Deuna.Merchant.Sdk.Configuration;
using Microsoft.Extensions.Options;

namespace Deuna.Merchant.Sdk.Http;

/// <summary>
/// A <see cref="DelegatingHandler"/> that injects the DEUNA API credentials
/// (<c>x-api-key</c> and <c>x-api-secret</c>) into every outgoing HTTP request.
/// </summary>
/// <remarks>
/// Credentials are never stored in plain text anywhere they could be logged.
/// The handler reads them from <see cref="DeunaClientOptions"/> which must be
/// registered through the options pattern.
/// </remarks>
internal sealed class DeunaAuthHandler : DelegatingHandler
{
    private readonly IOptionsMonitor<DeunaClientOptions> _options;

    /// <summary>
    /// Initializes a new instance of <see cref="DeunaAuthHandler"/>.
    /// </summary>
    /// <param name="options">The monitored options providing API credentials.</param>
    public DeunaAuthHandler(IOptionsMonitor<DeunaClientOptions> options)
    {
        _options = options;
    }

    /// <inheritdoc/>
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var current = _options.CurrentValue;

        // Add headers only if not already present (allows test overrides).
        if (!request.Headers.Contains("x-api-key"))
        {
            request.Headers.Add("x-api-key", current.ApiKey);
        }

        if (!request.Headers.Contains("x-api-secret"))
        {
            request.Headers.Add("x-api-secret", current.ApiSecret);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
