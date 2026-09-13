using System.Net;

namespace Deuna.Merchant.Sdk.Exceptions;

/// <summary>
/// Thrown when the DEUNA API returns a non-success HTTP status code.
/// Carries the raw HTTP status code and the error payload from the server.
/// </summary>
public sealed class DeunaApiException : DeunaException
{
    /// <summary>The HTTP status code returned by the API.</summary>
    public HttpStatusCode StatusCode { get; }

    /// <summary>
    /// The raw response body returned by the API, useful for logging and diagnostics.
    /// May be null if the response had no body.
    /// </summary>
    public string? RawResponse { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="DeunaApiException"/>.
    /// </summary>
    /// <param name="statusCode">The HTTP status code returned by the DEUNA API.</param>
    /// <param name="message">A human-readable description of the error.</param>
    /// <param name="rawResponse">The raw response body from the API, if any.</param>
    public DeunaApiException(HttpStatusCode statusCode, string message, string? rawResponse = null)
        : base(message)
    {
        StatusCode = statusCode;
        RawResponse = rawResponse;
    }

    /// <summary>
    /// Returns a string representation that includes the status code and message,
    /// but never includes API secrets or sensitive data.
    /// </summary>
    public override string ToString() =>
        $"{nameof(DeunaApiException)}: HTTP {(int)StatusCode} {StatusCode} — {Message}";
}
