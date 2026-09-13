using Deuna.Merchant.Sdk.Abstractions;

namespace Deuna.Merchant.Sdk.Clients;

/// <summary>
/// Top-level façade implementing <see cref="IDeunaMerchantClient"/>.
/// Aggregates all sub-clients behind a single entry point so callers use
/// <c>client.Payments.RequestAsync(…)</c> without needing to resolve sub-clients directly.
/// </summary>
public sealed class DeunaMerchantClient : IDeunaMerchantClient
{
    /// <inheritdoc/>
    public IPaymentClient Payments { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="DeunaMerchantClient"/>.
    /// </summary>
    /// <param name="payments">The payment operations client.</param>
    public DeunaMerchantClient(IPaymentClient payments)
    {
        Payments = payments;
    }
}
