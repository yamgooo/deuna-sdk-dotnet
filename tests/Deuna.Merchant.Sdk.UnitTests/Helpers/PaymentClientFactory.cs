using Deuna.Merchant.Sdk.Clients;
using Microsoft.Extensions.Logging.Abstractions;

namespace Deuna.Merchant.Sdk.UnitTests.Helpers;

/// <summary>
/// Factory helpers to create <see cref="PaymentClient"/> instances backed by
/// a <see cref="MockHttpMessageHandler"/> without a real DI container.
/// </summary>
internal static class PaymentClientFactory
{
    public static (PaymentClient Client, MockHttpMessageHandler Handler) Create(
        System.Net.HttpStatusCode statusCode,
        string responseBody,
        string baseUrl = "https://test.deuna.local/")
    {
        var handler = new MockHttpMessageHandler(statusCode, responseBody);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri(baseUrl) };
        var factory = new SingletonHttpClientFactory(PaymentClient.HttpClientName, httpClient);
        var logger = NullLogger<PaymentClient>.Instance;
        var client = new PaymentClient(factory, logger);
        return (client, handler);
    }

    /// <summary>
    /// A minimal <see cref="IHttpClientFactory"/> that always returns a pre-built client.
    /// </summary>
    private sealed class SingletonHttpClientFactory : IHttpClientFactory
    {
        private readonly string _name;
        private readonly HttpClient _client;

        public SingletonHttpClientFactory(string name, HttpClient client)
        {
            _name = name;
            _client = client;
        }

        public HttpClient CreateClient(string name) =>
            name == _name
                ? _client
                : throw new InvalidOperationException($"Unknown client name: {name}");
    }
}
