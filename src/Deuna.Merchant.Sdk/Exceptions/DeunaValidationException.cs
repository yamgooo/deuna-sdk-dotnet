namespace Deuna.Merchant.Sdk.Exceptions;

/// <summary>
/// Thrown when client-side input validation fails before a request is sent.
/// This is produced by guard clauses on public method parameters.
/// </summary>
public sealed class DeunaValidationException : DeunaException
{
    /// <summary>The name of the parameter that failed validation.</summary>
    public string? ParameterName { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="DeunaValidationException"/>.
    /// </summary>
    /// <param name="message">Description of the validation failure.</param>
    /// <param name="parameterName">The name of the invalid parameter.</param>
    public DeunaValidationException(string message, string? parameterName = null)
        : base(message)
    {
        ParameterName = parameterName;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="DeunaValidationException"/> wrapping
    /// a guard-clause exception.
    /// </summary>
    /// <param name="message">Description of the validation failure.</param>
    /// <param name="parameterName">The name of the invalid parameter.</param>
    /// <param name="innerException">The original exception from the guard clause library.</param>
    public DeunaValidationException(string message, string? parameterName, Exception innerException)
        : base(message, innerException)
    {
        ParameterName = parameterName;
    }
}
