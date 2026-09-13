using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Ardalis.GuardClauses;
using Deuna.Merchant.Sdk.Abstractions;
using Deuna.Merchant.Sdk.Exceptions;
using Deuna.Merchant.Sdk.Models.Requests;
using Deuna.Merchant.Sdk.Models.Responses;
using Deuna.Merchant.Sdk.Serialization;
using Microsoft.Extensions.Logging;

namespace Deuna.Merchant.Sdk.Clients;

/// <summary>
/// Implements <see cref="IPaymentClient"/> by calling the DEUNA Merchant Payments API
/// over HTTP. Transport concerns (auth, retries, circuit-breaker) are handled by the
/// delegating-handler pipeline configured in <c>ServiceCollectionExtensions</c>.
/// </summary>
internal sealed class PaymentClient : IPaymentClient
{
    internal const string HttpClientName = "DeunaMerchantClient";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<PaymentClient> _logger;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        TypeInfoResolver = DeunaJsonContext.Default,
    };

    /// <summary>
    /// Initializes a new instance of <see cref="PaymentClient"/>.
    /// </summary>
    public PaymentClient(IHttpClientFactory httpClientFactory, ILogger<PaymentClient> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<PaymentResponse> RequestAsync(
        PaymentRequest request,
        CancellationToken cancellationToken = default)
    {
        Guard(request, nameof(request));
        GuardString(request.PointOfSale, nameof(request.PointOfSale));
        GuardString(request.QrType, nameof(request.QrType));
        GuardPositive(request.Amount, nameof(request.Amount));
        GuardString(request.InternalTransactionReference, nameof(request.InternalTransactionReference));
        GuardString(request.Format, nameof(request.Format));

        _logger.LogInformation(
            "Requesting payment for POS={PointOfSale} Amount={Amount} Format={Format}",
            request.PointOfSale, request.Amount, request.Format);

        return await PostAsync<PaymentRequest, PaymentResponse>(
            "merchant/v1/payment/request",
            request,
            DeunaJsonContext.Default.PaymentRequest,
            DeunaJsonContext.Default.PaymentResponse,
            cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<PaymentInfoResponse> GetInfoAsync(
        PaymentInfoRequest request,
        CancellationToken cancellationToken = default)
    {
        Guard(request, nameof(request));
        GuardString(request.IdTransactionReference, nameof(request.IdTransactionReference));
        GuardString(request.IdType, nameof(request.IdType));

        _logger.LogInformation(
            "Querying payment info for TransactionReference={TransactionReference} IdType={IdType}",
            request.IdTransactionReference, request.IdType);

        return await PostAsync<PaymentInfoRequest, PaymentInfoResponse>(
            "merchant/v1/payment/info",
            request,
            DeunaJsonContext.Default.PaymentInfoRequest,
            DeunaJsonContext.Default.PaymentInfoResponse,
            cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<CancelTransactionResponse> CancelAsync(
        CancelTransactionRequest request,
        CancellationToken cancellationToken = default)
    {
        Guard(request, nameof(request));
        GuardString(request.TransactionId, nameof(request.TransactionId));
        GuardString(request.PointOfSale, nameof(request.PointOfSale));

        _logger.LogInformation(
            "Cancelling transaction TransactionId={TransactionId} POS={PointOfSale}",
            request.TransactionId, request.PointOfSale);

        return await PostAsync<CancelTransactionRequest, CancelTransactionResponse>(
            "merchant/v1/payment/cancel",
            request,
            DeunaJsonContext.Default.CancelTransactionRequest,
            DeunaJsonContext.Default.CancelTransactionResponse,
            cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<RefundResponse> RefundAsync(
        RefundRequest request,
        CancellationToken cancellationToken = default)
    {
        Guard(request, nameof(request));
        GuardString(request.TransferNumber, nameof(request.TransferNumber));

        _logger.LogInformation(
            "Initiating refund for TransferNumber={TransferNumber}",
            request.TransferNumber);

        return await PostAsync<RefundRequest, RefundResponse>(
            "merchant/v1/payment/void",
            request,
            DeunaJsonContext.Default.RefundRequest,
            DeunaJsonContext.Default.RefundResponse,
            cancellationToken).ConfigureAwait(false);
    }

    // ─── Private helpers ──────────────────────────────────────────────────────

    /// <summary>
    /// Executes a POST request, deserializes the response, and maps errors to typed exceptions.
    /// </summary>
    private async Task<TResponse> PostAsync<TRequest, TResponse>(
        string relativeUrl,
        TRequest requestBody,
        System.Text.Json.Serialization.Metadata.JsonTypeInfo<TRequest> requestTypeInfo,
        System.Text.Json.Serialization.Metadata.JsonTypeInfo<TResponse> responseTypeInfo,
        CancellationToken cancellationToken)
        where TRequest : class
        where TResponse : class
    {
        var client = _httpClientFactory.CreateClient(HttpClientName);

        HttpResponseMessage response;
        try
        {
            response = await client.PostAsJsonAsync(
                relativeUrl,
                requestBody,
                requestTypeInfo,
                cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not DeunaException)
        {
            throw new DeunaException(
                $"An unexpected error occurred while calling '{relativeUrl}': {ex.Message}", ex);
        }

        if (!response.IsSuccessStatusCode)
        {
            await ThrowApiExceptionAsync(response, relativeUrl, cancellationToken).ConfigureAwait(false);
        }

        TResponse? result;
        try
        {
            result = await response.Content
                .ReadFromJsonAsync(responseTypeInfo, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (JsonException ex)
        {
            throw new DeunaException(
                $"Failed to deserialize the DEUNA API response from '{relativeUrl}'.", ex);
        }

        if (result is null)
        {
            throw new DeunaException($"The DEUNA API returned an empty response for '{relativeUrl}'.");
        }

        _logger.LogInformation("Request to '{Url}' completed successfully.", relativeUrl);
        return result;
    }

    /// <summary>
    /// Reads the error body and throws a <see cref="DeunaApiException"/> with the best
    /// available message. Never leaks secrets.
    /// </summary>
    private static async Task ThrowApiExceptionAsync(
        HttpResponseMessage response,
        string relativeUrl,
        CancellationToken cancellationToken)
    {
        string rawBody = string.Empty;
        string errorMessage;

        try
        {
            rawBody = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

            // Try to parse the documented error shape first.
            var errorPayload = JsonSerializer.Deserialize(rawBody, DeunaJsonContext.Default.DeunaErrorResponse);
            errorMessage = !string.IsNullOrWhiteSpace(errorPayload?.Message)
                ? errorPayload.Message
                : $"The DEUNA API returned HTTP {(int)response.StatusCode} for '{relativeUrl}'.";
        }
        catch
        {
            errorMessage = $"The DEUNA API returned HTTP {(int)response.StatusCode} for '{relativeUrl}'.";
        }

        throw new DeunaApiException(response.StatusCode, errorMessage, rawBody);
    }

    /// <summary>
    /// Validates that an object parameter is not null,
    /// wrapping any guard exception in a <see cref="DeunaValidationException"/>.
    /// </summary>
    private static void Guard<T>(T? value, string paramName) where T : class
    {
        try
        {
            Ardalis.GuardClauses.Guard.Against.Null(value, paramName);
        }
        catch (ArgumentNullException ex)
        {
            throw new DeunaValidationException(ex.Message, paramName, ex);
        }
    }

    /// <summary>
    /// Validates that a decimal parameter is positive (> 0),
    /// wrapping the guard exception in a <see cref="DeunaValidationException"/>.
    /// </summary>
    private static void GuardPositive(decimal value, string paramName)
    {
        try
        {
            Ardalis.GuardClauses.Guard.Against.NegativeOrZero(value, paramName);
        }
        catch (ArgumentException ex)
        {
            throw new DeunaValidationException(ex.Message, paramName, ex);
        }
    }

    /// <summary>
    /// Validates that a string parameter is not null or whitespace,
    /// wrapping the guard exception in a <see cref="DeunaValidationException"/>.
    /// </summary>
    private static void GuardString(string? value, string paramName)
    {
        try
        {
            Ardalis.GuardClauses.Guard.Against.NullOrWhiteSpace(value, paramName);
        }
        catch (ArgumentException ex)
        {
            throw new DeunaValidationException(ex.Message, paramName, ex);
        }
    }
}
