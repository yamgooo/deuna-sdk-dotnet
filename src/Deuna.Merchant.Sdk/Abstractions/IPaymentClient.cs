using Deuna.Merchant.Sdk.Models.Requests;
using Deuna.Merchant.Sdk.Models.Responses;

namespace Deuna.Merchant.Sdk.Abstractions;

/// <summary>
/// Exposes operations for the DEUNA Merchant Payments API.
/// </summary>
public interface IPaymentClient
{
    /// <summary>
    /// Creates a new payment request, generating a QR code and/or deep-link
    /// for the consumer to complete the payment.
    /// </summary>
    /// <param name="request">The payment request parameters.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A <see cref="PaymentResponse"/> with the transaction ID and optional QR/deeplink.</returns>
    /// <exception cref="Exceptions.DeunaValidationException">Thrown when <paramref name="request"/> or its required fields are invalid.</exception>
    /// <exception cref="Exceptions.DeunaApiException">Thrown when the API returns a non-success status code.</exception>
    /// <exception cref="Exceptions.DeunaException">Thrown on unexpected transport or serialization errors.</exception>
    Task<PaymentResponse> RequestAsync(PaymentRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the current status and details of an existing transaction.
    /// </summary>
    /// <param name="request">The query parameters identifying the transaction.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A <see cref="PaymentInfoResponse"/> with full transaction details.</returns>
    /// <exception cref="Exceptions.DeunaValidationException">Thrown when <paramref name="request"/> or its required fields are invalid.</exception>
    /// <exception cref="Exceptions.DeunaApiException">Thrown when the API returns a non-success status code.</exception>
    /// <exception cref="Exceptions.DeunaException">Thrown on unexpected transport or serialization errors.</exception>
    Task<PaymentInfoResponse> GetInfoAsync(PaymentInfoRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancels a pending (unpaid) payment transaction.
    /// </summary>
    /// <param name="request">The cancellation request identifying the transaction and POS.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A <see cref="CancelTransactionResponse"/> confirming the cancellation.</returns>
    /// <exception cref="Exceptions.DeunaValidationException">Thrown when <paramref name="request"/> or its required fields are invalid.</exception>
    /// <exception cref="Exceptions.DeunaApiException">Thrown when the API returns a non-success status code.</exception>
    /// <exception cref="Exceptions.DeunaException">Thrown on unexpected transport or serialization errors.</exception>
    Task<CancelTransactionResponse> CancelAsync(CancelTransactionRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Initiates a refund (void) for a completed transaction.
    /// </summary>
    /// <param name="request">The refund request containing the transfer number.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A <see cref="RefundResponse"/> confirming the reversal.</returns>
    /// <exception cref="Exceptions.DeunaValidationException">Thrown when <paramref name="request"/> or its required fields are invalid.</exception>
    /// <exception cref="Exceptions.DeunaApiException">
    /// Thrown when the API returns a non-success status code.
    /// A <see cref="System.Net.HttpStatusCode.BadRequest"/> (400) is raised when the
    /// transfer has already been refunded.
    /// </exception>
    /// <exception cref="Exceptions.DeunaException">Thrown on unexpected transport or serialization errors.</exception>
    Task<RefundResponse> RefundAsync(RefundRequest request, CancellationToken cancellationToken = default);
}
