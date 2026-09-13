using Deuna.Merchant.Sdk.Abstractions;
using Deuna.Merchant.Sdk.Clients;
using Deuna.Merchant.Sdk.Configuration;
using Deuna.Merchant.Sdk.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Deuna.Merchant.Sdk.DependencyInjection;

/// <summary>
/// Provides the <c>AddDeunaMerchantClient</c> extension method for registering
/// the DEUNA Merchant SDK into an <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the DEUNA Merchant SDK services, including the HTTP client pipeline
    /// with authentication, resilience (retry, circuit-breaker, timeout), and DI bindings.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configure">A delegate to configure <see cref="DeunaClientOptions"/>.</param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    /// <example>
    /// <code>
    /// builder.Services.AddDeunaMerchantClient(opts =>
    /// {
    ///     opts.BaseUrl   = "https://apis-merchant.pdn.deunalab.com";
    ///     opts.ApiKey    = Environment.GetEnvironmentVariable("DEUNA_API_KEY")!;
    ///     opts.ApiSecret = Environment.GetEnvironmentVariable("DEUNA_API_SECRET")!;
    /// });
    /// </code>
    /// </example>
    public static IServiceCollection AddDeunaMerchantClient(
        this IServiceCollection services,
        Action<DeunaClientOptions> configure)
    {
        // 1. Bind and validate options.
        services
            .AddOptions<DeunaClientOptions>()
            .Configure(configure)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<IValidateOptions<DeunaClientOptions>, DeunaClientOptionsValidator>();

        // 2. Register the auth handler (transient — one instance per HttpClient pipeline creation).
        services.AddTransient<DeunaAuthHandler>();

        // 3. Configure the named HttpClient with resilience.
        services
            .AddHttpClient(PaymentClient.HttpClientName, (sp, client) =>
            {
                var opts = sp.GetRequiredService<IOptions<DeunaClientOptions>>().Value;
                client.BaseAddress = new Uri(opts.BaseUrl.TrimEnd('/') + "/");
                client.Timeout = opts.Timeout;
                client.DefaultRequestHeaders.Accept.Add(
                    new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            })
            .AddHttpMessageHandler<DeunaAuthHandler>()
            .AddStandardResilienceHandler(opts =>
            {
                // Override retry policy with our options-driven values.
                opts.Retry.MaxRetryAttempts = 3;
                opts.Retry.Delay = TimeSpan.FromSeconds(1);
            });

        // 4. Register sub-clients and the top-level facade.
        services.AddSingleton<IPaymentClient, PaymentClient>();
        services.AddSingleton<IDeunaMerchantClient, DeunaMerchantClient>();

        return services;
    }

    /// <summary>
    /// Registers the DEUNA Merchant SDK services by binding options from
    /// <see cref="Microsoft.Extensions.Configuration.IConfiguration"/> under the
    /// <see cref="DeunaClientOptions.SectionName"/> key.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configuration">The application configuration (e.g., from <c>appsettings.json</c>).</param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    /// <example>
    /// <code>
    /// // appsettings.json:
    /// // {
    /// //   "DeunaClient": {
    /// //     "BaseUrl": "https://apis-merchant.pdn.deunalab.com",
    /// //     "ApiKey": "...",
    /// //     "ApiSecret": "..."
    /// //   }
    /// // }
    /// builder.Services.AddDeunaMerchantClient(builder.Configuration);
    /// </code>
    /// </example>
    public static IServiceCollection AddDeunaMerchantClient(
        this IServiceCollection services,
        Microsoft.Extensions.Configuration.IConfiguration configuration)
    {
        var section = configuration.GetSection(DeunaClientOptions.SectionName);
        return services.AddDeunaMerchantClient(opts =>
        {
            var baseUrl = section["BaseUrl"];
            if (!string.IsNullOrWhiteSpace(baseUrl))
            {
                opts.BaseUrl = baseUrl;
            }

            var apiKey = section["ApiKey"];
            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                opts.ApiKey = apiKey;
            }

            var apiSecret = section["ApiSecret"];
            if (!string.IsNullOrWhiteSpace(apiSecret))
            {
                opts.ApiSecret = apiSecret;
            }

            var timeout = section["Timeout"];
            if (!string.IsNullOrWhiteSpace(timeout) && TimeSpan.TryParse(timeout, out var t))
            {
                opts.Timeout = t;
            }

            var maxRetry = section["MaxRetryAttempts"];
            if (!string.IsNullOrWhiteSpace(maxRetry) && int.TryParse(maxRetry, out var r))
            {
                opts.MaxRetryAttempts = r;
            }
        });
    }
}
