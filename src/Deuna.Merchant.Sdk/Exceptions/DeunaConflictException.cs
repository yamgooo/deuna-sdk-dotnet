using System.Net;

namespace Deuna.Merchant.Sdk.Exceptions;

/// <summary>
/// Thrown when the DEUNA API returns a 409 Conflict.
/// </summary>
public class DeunaConflictException : DeunaApiException
{
    public DeunaConflictException(string message, string? rawResponse = null)
        : base(HttpStatusCode.Conflict, message, rawResponse)
    {
    }
}
