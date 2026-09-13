using Deuna.Merchant.Sdk.Abstractions;
using Deuna.Merchant.Sdk.DependencyInjection;
using Deuna.Merchant.Sdk.Exceptions;
using Deuna.Merchant.Sdk.Models.Enums;
using Deuna.Merchant.Sdk.Models.Requests;
using Deuna.Merchant.Sdk.Models.Responses;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// ---------------------------------------------------------------------------
// DEUNA Merchant SDK — Sample Console Application
//
// Credentials and configuration are read exclusively from environment variables.
// Never hardcode secrets in source code.
//
// Required environment variables:
//   DEUNA_BASE_URL    (default: https://apis-merchant.pdn.deunalab.com)
//   DEUNA_API_KEY     (required)
//   DEUNA_API_SECRET  (required)
//   DEUNA_POS_ID      (required — your point-of-sale identifier)
// ---------------------------------------------------------------------------

var baseUrl = Environment.GetEnvironmentVariable("DEUNA_BASE_URL") ?? "https://apis-merchant.pdn.deunalab.com";
var apiKey = Environment.GetEnvironmentVariable("DEUNA_API_KEY") ?? string.Empty;
var apiSecret = Environment.GetEnvironmentVariable("DEUNA_API_SECRET") ?? string.Empty;
var pointOfSale = Environment.GetEnvironmentVariable("DEUNA_POS_ID") ?? string.Empty;

if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(apiSecret) || string.IsNullOrWhiteSpace(pointOfSale))
{
    Console.Error.WriteLine(
        "ERROR: Set the environment variables DEUNA_API_KEY, DEUNA_API_SECRET, and DEUNA_POS_ID before running this sample.");
    return 1;
}

// ─── DI setup ────────────────────────────────────────────────────────────────
var services = new ServiceCollection();

services.AddLogging(logging =>
    logging.AddConsole().SetMinimumLevel(LogLevel.Information));

services.AddDeunaMerchantClient(opts =>
{
    opts.BaseUrl = baseUrl;
    opts.ApiKey = apiKey;
    opts.ApiSecret = apiSecret;
});

await using var provider = services.BuildServiceProvider();
var deuna = provider.GetRequiredService<IDeunaMerchantClient>();

// ─── Step 1: Request a payment ───────────────────────────────────────────────
Console.WriteLine("── Step 1: Requesting payment …");

PaymentResponse paymentResponse;
try
{
    paymentResponse = await deuna.Payments.RequestAsync(new PaymentRequest
    {
        PointOfSale = pointOfSale,
        QrType = QrType.Dynamic,
        Amount = 0.10m,
        Detail = "SDK sample end-to-end demo",
        InternalTransactionReference = $"sample-{Guid.NewGuid():N}",
        Format = QrResponseFormat.DeeplinkOnly,
    });

    Console.WriteLine($"  ✔ TransactionId : {paymentResponse.TransactionId}");
    Console.WriteLine($"  ✔ Deeplink      : {paymentResponse.Deeplink}");
}
catch (DeunaValidationException ex)
{
    Console.Error.WriteLine($"  ✖ Validation error: {ex.Message}");
    return 2;
}
catch (DeunaApiException ex)
{
    Console.Error.WriteLine($"  ✖ API error {(int)ex.StatusCode}: {ex.Message}");
    return 3;
}

// ─── Step 2: Poll payment info ───────────────────────────────────────────────
Console.WriteLine("\n── Step 2: Polling payment info (PENDING expected) …");

try
{
    var info = await deuna.Payments.GetInfoAsync(new PaymentInfoRequest
    {
        IdTransactionReference = paymentResponse.TransactionId,
        IdType = "0",
    });

    Console.WriteLine($"  ✔ Status   : {info.Status}");
    Console.WriteLine($"  ✔ Amount   : {info.Amount} {info.Currency}");
    Console.WriteLine($"  ✔ Date     : {info.Date} → parsed: {info.TryGetParsedDate()?.ToString("u") ?? "(not yet set)"}");
}
catch (DeunaApiException ex)
{
    Console.Error.WriteLine($"  ✖ API error {(int)ex.StatusCode}: {ex.Message}");
}

// ─── Step 3: Cancel the pending payment ──────────────────────────────────────
Console.WriteLine("\n── Step 3: Cancelling payment …");

try
{
    var cancel = await deuna.Payments.CancelAsync(new CancelTransactionRequest
    {
        TransactionId = paymentResponse.TransactionId,
        PointOfSale = pointOfSale,
    });

    Console.WriteLine($"  ✔ {cancel.Message}");
}
catch (DeunaApiException ex)
{
    Console.Error.WriteLine($"  ✖ API error {(int)ex.StatusCode}: {ex.Message}");
}

Console.WriteLine("\nSample completed.");
return 0;
