namespace Deuna.Merchant.Sdk.Abstractions;

/// <summary>
/// Top-level façade for the DEUNA Merchant SDK.
/// Provides access to all API operations through strongly typed sub-clients.
/// </summary>
public interface IDeunaMerchantClient
{
    /// <summary>
    /// Access to payment operations: request, info, cancel, and refund.
    /// </summary>
    IPaymentClient Payments { get; }
}
