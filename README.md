# Deuna.Merchant.Sdk

[![Build](https://github.com/erikportilla/deuna-sdk-dotnet/actions/workflows/ci.yml/badge.svg)](https://github.com/erikportilla/deuna-sdk-dotnet/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/Deuna.Merchant.Sdk.svg)](https://www.nuget.org/packages/Deuna.Merchant.Sdk)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

Strongly-typed, async, DI-friendly **.NET 10** SDK for the **DEUNA Merchant Payments API**
(Banco Pichincha / DEUNA). Wraps four endpoints — request payment, query status, cancel,
and refund — with built-in resilience, secure credential injection, and source-generated JSON.

---

## Install

```bash
dotnet add package Deuna.Merchant.Sdk
```

---

## Quick Start

```csharp
// 1. Register (ASP.NET Core / Generic Host)
builder.Services.AddDeunaMerchantClient(opts =>
{
    opts.Environment = DeunaEnvironment.Production; // or DeunaEnvironment.Qa
    opts.ApiKey    = Environment.GetEnvironmentVariable("DEUNA_API_KEY")!;
    opts.ApiSecret = Environment.GetEnvironmentVariable("DEUNA_API_SECRET")!;
});

// 2. Inject and use
public class PaymentService(IDeunaMerchantClient deuna)
{
    public async Task RunAsync()
    {
        // Request payment
        var payment = await deuna.Payments.RequestAsync(new PaymentRequest
        {
            PointOfSale = "462",
            QrType = QrType.Dynamic,
            Amount = 29.99m,
            Detail = "Invoice #1234",
            InternalTransactionReference = "order-9876",
            Format = QrResponseFormat.QrAndDeeplink,
        });
        Console.WriteLine($"Transaction: {payment.TransactionId}");
        Console.WriteLine($"Deeplink:    {payment.Deeplink}");

        // Poll status (fallback) - rely on Webhooks primarily!
        var info = await deuna.Payments.GetInfoAsync(new PaymentInfoRequest
        {
            IdTransactionReference = payment.TransactionId,
            IdType = IdType.TransactionId,
        });
        Console.WriteLine($"Status: {info.Status}"); // "PENDING" or "APPROVED"

        // Cancel (while still PENDING)
        var cancel = await deuna.Payments.CancelAsync(new CancelTransactionRequest
        {
            TransactionId = payment.TransactionId,
            PointOfSale = "462",
        });
        Console.WriteLine(cancel.Message);

        // Refund an approved transaction
        try
        {
            var refund = await deuna.Payments.RefundAsync(new RefundRequest
            {
                TransferNumber = info.TransferNumber,
            });
            Console.WriteLine($"Reversed: {refund.TransactionReverseId}");
        }
        catch (DeunaApiException ex) when (ex.StatusCode == HttpStatusCode.BadRequest)
        {
            Console.Error.WriteLine($"Already refunded: {ex.Message}");
        }
    }
}
```

---

## Configuration

| Option | Default | Description |
|---|---|---|
| `Environment` | `Production` | Target `Production` or `Qa` environments |
| `BaseUrl` | (auto-set) | API base — overrides `Environment` if set explicitly |
| `ApiKey` | _(required)_ | `x-api-key` header |
| `ApiSecret` | _(required)_ | `x-api-secret` header |
| `Timeout` | 30 s | Per-request total timeout |
| `MaxRetryAttempts` | 3 | Retries on transient failures |

### `appsettings.json`

```json
{
  "DeunaClient": {
    "Environment": "Production",
    "ApiKey":  "...",
    "ApiSecret": "..."
  }
}
```

```csharp
builder.Services.AddDeunaMerchantClient(builder.Configuration);
```

---

## Error Handling

```csharp
catch (DeunaValidationException ex) { /* bad input */ }
catch (DeunaRateLimitException ex)  { /* 429 Too Many Requests */ }
catch (DeunaApiException ex)        { /* non-2xx API response — ex.StatusCode, ex.RawResponse */ }
catch (DeunaException ex)           { /* transport or serialization error */ }
```

---

## Documentation

- 📖 [Full usage guide & API reference](docs/usage.md)
- 🏛 [Architecture Decision Records](docs/decisions/)
- 📦 [NuGet package](https://www.nuget.org/packages/Deuna.Merchant.Sdk)

---

## License

[MIT](LICENSE) © 2026 Erik Portilla Pesantez
