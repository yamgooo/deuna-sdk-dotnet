using System.Net;
using System.Text;

namespace Deuna.Merchant.Sdk.UnitTests.Helpers;

/// <summary>
/// A test-only <see cref="HttpMessageHandler"/> that returns a pre-configured
/// <see cref="HttpResponseMessage"/> without making any real network calls.
/// Also records the most-recent outgoing request for assertion.
/// </summary>
internal sealed class MockHttpMessageHandler : HttpMessageHandler
{
    private readonly HttpStatusCode _statusCode;
    private readonly string _responseBody;
    private readonly string _contentType;

    /// <summary>The last request sent through this handler.</summary>
    public HttpRequestMessage? LastRequest { get; private set; }

    /// <summary>The body of the last request, read asynchronously.</summary>
    public string? LastRequestBody { get; private set; }

    public MockHttpMessageHandler(
        System.Net.HttpStatusCode statusCode,
        string responseBody,
        string contentType = "application/json")
    {
        _statusCode = statusCode;
        _responseBody = responseBody;
        _contentType = contentType;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        LastRequest = request;
        if (request.Content is not null)
        {
            LastRequestBody = await request.Content.ReadAsStringAsync(cancellationToken);
        }

        return new HttpResponseMessage(_statusCode)
        {
            Content = new StringContent(_responseBody, Encoding.UTF8, _contentType),
        };
    }
}
