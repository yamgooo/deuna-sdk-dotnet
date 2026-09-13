using Microsoft.Extensions.Options;

namespace Deuna.Merchant.Sdk.Configuration;

/// <summary>
/// Validates <see cref="DeunaClientOptions"/> at startup using the options-pattern
/// <see cref="IValidateOptions{TOptions}"/> contract.
/// Provides clear, actionable error messages rather than cryptic null-reference
/// exceptions at call time.
/// </summary>
internal sealed class DeunaClientOptionsValidator : IValidateOptions<DeunaClientOptions>
{
    /// <inheritdoc/>
    public ValidateOptionsResult Validate(string? name, DeunaClientOptions options)
    {
        var failures = new List<string>();

        if (string.IsNullOrWhiteSpace(options.BaseUrl))
        {
            failures.Add($"{nameof(options.BaseUrl)} must not be null or empty.");
        }

        if (!Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out _))
        {
            failures.Add($"{nameof(options.BaseUrl)} must be a valid absolute URI. Got: '{options.BaseUrl}'.");
        }

        if (string.IsNullOrWhiteSpace(options.ApiKey))
        {
            failures.Add($"{nameof(options.ApiKey)} must not be null or empty.");
        }

        if (string.IsNullOrWhiteSpace(options.ApiSecret))
        {
            failures.Add($"{nameof(options.ApiSecret)} must not be null or empty.");
        }

        if (options.Timeout <= TimeSpan.Zero)
        {
            failures.Add($"{nameof(options.Timeout)} must be a positive duration.");
        }

        if (options.MaxRetryAttempts < 0)
        {
            failures.Add($"{nameof(options.MaxRetryAttempts)} must be >= 0.");
        }

        return failures.Count > 0
            ? ValidateOptionsResult.Fail(failures)
            : ValidateOptionsResult.Success;
    }
}
