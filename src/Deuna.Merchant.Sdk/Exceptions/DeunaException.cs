namespace Deuna.Merchant.Sdk.Exceptions;

/// <summary>
/// Base exception for all errors originating from the DEUNA Merchant SDK.
/// </summary>
public class DeunaException : Exception
{
    /// <summary>Initializes a new instance of <see cref="DeunaException"/>.</summary>
    public DeunaException() { }

    /// <summary>Initializes a new instance of <see cref="DeunaException"/> with a message.</summary>
    public DeunaException(string message) : base(message) { }

    /// <summary>Initializes a new instance of <see cref="DeunaException"/> with a message and inner exception.</summary>
    public DeunaException(string message, Exception innerException) : base(message, innerException) { }
}
