# Deuna.Merchant.Sdk — Usage Guide

Complete reference for the DEUNA Merchant SDK for .NET 10.

---

## Table of Contents

1. [Installation](#installation)
2. [Configuration](#configuration)
3. [DI Registration](#di-registration)
4. [API Reference](#api-reference)
   - [RequestAsync](#requestasync)
   - [GetInfoAsync](#getinfoasync)
   - [CancelAsync)](#cancelasync)
   - [RefundAsync](#refundasync)
5. [Error Handling](#error-handling)
6. [Resilience & Retry](#resilience--retry)
7. [Credential Security](#credential-security)
8. [Running the Sample](#running-the-sample)

---

## Installation

```bash
dotnet add package Deuna.Merchant.Sdk
```

Targets **.NET 10** and is compatible with AOT/trimmed deployments.

---

## Configuration

### Option A — `appsettings.json` (recommended for ASP.NET Core)

```json
{
  "DeunaClient": {
    "BaseUrl": "https://apis-merchant.pdn.deunalab.com",
    "ApiKey": "YOUR_API_KEY",
    "ApiSecret": "YOUR_API_SECRET",
    "Timeout": "00:00:30",
    "MaxRetryAttempts": 3,
    "MaxRetryDelay": "00:00:15"
  }
}
```

> ⚠️ **Never commit real credentials to source control.** Use environment variables,
> Azure Key Vault, AWS Secrets Manager, or .NET user secrets (`dotnet user-secrets`) instead.

### `DeunaClientOptions` properties

| Property | Type | Default | Description |
|---|---|---|---|
| `BaseUrl` | `string` | `https://apis-merchant.pdn.deunalab.com` | Production or sandbox URL |
| `ApiKey` | `string` | _(required)_ | `x-api-key` header value |
| `ApiSecret` | `string` | _(required)_ | `x-api-secret` header value |
| `Timeout` | `TimeSpan` | `00:00:30` | Per-request total timeout |
| `MaxRetryAttempts` | `int` | `3` | Retry attempts on transient errors |
| `MaxRetryDelay` | `TimeSpan` | `00:00:15` | Max back-off ceiling |

---

## DI Registration

### Option A — inline delegate

```csharp
builder.Services.AddDeunaMerchantClient(opts =>
{
    opts.BaseUrl   = "https://apis-merchant.pdn.deunalab.com";
    opts.ApiKey    = Environment.GetEnvironmentVariable("DEUNA_API_KEY")!;
    opts.ApiSecret = Environment.GetEnvironmentVariable("DEUNA_API_SECRET")!;
});
```

### Option B — `IConfiguration` binding

```csharp
builder.Services.AddDeunaMerchantClient(builder.Configuration);
// Reads "DeunaClient" section from appsettings.json
```

Both overloads validate options at startup — the app will fail to start rather than
silently sending requests with empty credentials.

---

## API Reference

### RequestAsync

Creates a new payment and returns a QR code and/or deep-link.

```csharp
var response = await deuna.Payments.RequestAsync(new PaymentRequest
{
    PointOfSale = "462",                             // Required: your POS ID
    QrType = QrType.Dynamic,                         // "dynamic" — only supported type
    Amount = 29.99m,                                 // Must be > 0
    Detail = "Invoice #1234",                        // Optional description
    InternalTransactionReference = "order-9876",     // Your internal reference
    Format = QrResponseFormat.QrAndDeeplink,         // "2" — returns both
});

Console.WriteLine(response.TransactionId); // GUID from DEUNA
Console.WriteLine(response.Deeplink);      // nullable
Console.WriteLine(response.Qr);           // nullable data URI (base64 PNG)
```

**`QrResponseFormat` constants:**

| Constant | Wire value | Response fields |
|---|---|---|
| `DeeplinkOnly` | `"0"` | `deeplink` |
| `QrOnly` | `"1"` | `qr` |
| `QrAndDeeplink` | `"2"` | `qr` + `deeplink` |
| `Full` | `"5"` | All available fields |

---

### GetInfoAsync

Polls the status of a transaction. Poll until `status != "PENDING"`.

```csharp
var info = await deuna.Payments.GetInfoAsync(new PaymentInfoRequest
{
    IdTransactionReference = transactionId,
    IdType = "0",
});

// Open string — check by value, not enum
if (info.Status == "APPROVED")
{
    Console.WriteLine($"Transfer number: {info.TransferNumber}");
    Console.WriteLine($"Payer: {info.OrdererName} ({info.OrdererIdentification})");
}

// Best-effort date parsing (API format is inconsistent)
DateTimeOffset? date = info.TryGetParsedDate();
```

**`PaymentInfoResponse` properties:**

| Property | Type | Notes |
|---|---|---|
| `Status` | `string` | `"PENDING"`, `"APPROVED"`, or other future values |
| `InternalTransactionReference` | `string` | Your reference, empty when PENDING |
| `Amount` | `decimal` | |
| `TransactionId` | `string` | DEUNA's ID |
| `TransferNumber` | `string` | Bank transfer number, use for refunds |
| `Date` | `string` | Raw string (inconsistent format) |
| `TryGetParsedDate()` | `DateTimeOffset?` | Best-effort parse of `Date` |
| `BranchId` / `PosId` | `string` | Branch/POS identifiers |
| `Currency` | `string` | e.g. `"USD"` |
| `Description` | `string` | |
| `OrdererName` | `string` | Full name of payer |
| `OrdererIdentification` | `string` | National ID of payer |

> **Wire typo note:** The JSON field is `idTransacionReference` (one `s` in *Transacion*).
> The C# property is correctly spelled `IdTransactionReference`. The SDK handles the mapping
> automatically via `[JsonPropertyName]`.

---

### CancelAsync

Cancels a pending (unpaid) payment. Only works while `status == "PENDING"`.

```csharp
var result = await deuna.Payments.CancelAsync(new CancelTransactionRequest
{
    TransactionId = "e91c7a59-b67d-40fe-b84b-2a29b055b543",
    PointOfSale = "462",
});

Console.WriteLine(result.Message); // "The QR with id 250 has been successfully cleaned"
```

---

### RefundAsync

Initiates a refund (reversal) for an approved transaction.

```csharp
try
{
    var refund = await deuna.Payments.RefundAsync(new RefundRequest
    {
        TransferNumber = "972454362424", // from PaymentInfoResponse.TransferNumber
    });

    Console.WriteLine($"Reversed: {refund.TransactionReverseId}");
}
catch (DeunaApiException ex) when (ex.StatusCode == HttpStatusCode.BadRequest)
{
    // HTTP 400 = already refunded
    Console.Error.WriteLine($"Already processed: {ex.Message}");
}
```

---

## Error Handling

All SDK methods throw subtypes of `DeunaException`:

| Exception | When |
|---|---|
| `DeunaValidationException` | Guard-clause failure on a null or empty parameter |
| `DeunaApiException` | API returned a non-2xx status; carries `StatusCode` and `RawResponse` |
| `DeunaException` | Unexpected transport or serialization error |

```csharp
try
{
    var response = await deuna.Payments.RequestAsync(request);
}
catch (DeunaValidationException ex)
{
    // Bad input — log and fix the code
    logger.LogError("Validation: {Param} — {Msg}", ex.ParameterName, ex.Message);
}
catch (DeunaApiException ex)
{
    // API-side error — may be retried externally
    logger.LogError("DEUNA API {Code}: {Msg}", (int)ex.StatusCode, ex.Message);
}
catch (DeunaException ex)
{
    // Transport/serialization failure
    logger.LogCritical(ex, "Unexpected SDK error");
}
```

---

## Resilience & Retry

The SDK uses `Microsoft.Extensions.Http.Resilience`'s `AddStandardResilienceHandler`,
which provides out-of-the-box:

- **Retry** with exponential back-off + jitter (default: 3 attempts)
- **Circuit breaker** — trips after sustained failures, preventing cascade
- **Timeout** — per-attempt and total

These are applied transparently to every API call. To disable retries in tests:

```csharp
services.AddDeunaMerchantClient(opts =>
{
    opts.MaxRetryAttempts = 0;
    // ...
});
```

See [ADR-002](decisions/ADR-002-standard-resilience-handler.md) for rationale.

---

## Credential Security

- Credentials (`ApiKey`, `ApiSecret`) are injected via `DeunaAuthHandler` — a
  `DelegatingHandler` that never copies them into the request body or URL.
- Structured logging is used for all log messages; secrets are never included in
  log output. The `DeunaApiException.ToString()` output also never includes secrets.
- In production, store credentials in your platform's secret store (Azure Key Vault,
  AWS Secrets Manager, HashiCorp Vault, etc.) and bind them through environment
  variables or `IConfiguration`.

---

## Running the Sample

```bash
# Set your credentials
export DEUNA_API_KEY="your-api-key"
export DEUNA_API_SECRET="your-api-secret"
export DEUNA_POS_ID="your-pos-id"
# Optional: override base URL for sandbox
export DEUNA_BASE_URL="https://deuna-dev.apigee.net"

dotnet run --project samples/Deuna.Merchant.Sdk.Sample
```

The sample demonstrates the full flow: request payment → poll info → cancel.
