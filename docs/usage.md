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
    "Environment": "Production",
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
| `Environment` | `DeunaEnvironment` | `Production` | Target `Production` or `Qa` environments |
| `BaseUrl` | `string` | _(auto-set)_ | API base URL — overrides `Environment` if explicitly set |
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
    opts.Environment = DeunaEnvironment.Production;
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
    QrType = QrType.Dynamic,                         // Required: QrType.Dynamic or QrType.Static
    ExpiredTime = 60,                                // Optional: minutes until expiration (Dynamic only)
    Amount = 29.99m,                                 // Must be > 0
    Detail = "Invoice #1234",                        // Optional description
    InternalTransactionReference = "order-9876",     // Your internal reference (<= 20 chars)
    Format = QrResponseFormat.QrAndDeeplink,         // Required: QrResponseFormat enum
});

Console.WriteLine(response.TransactionId); // GUID from DEUNA
Console.WriteLine(response.Deeplink);      // nullable string
Console.WriteLine(response.Qr);            // nullable data URI (base64 PNG)
Console.WriteLine(response.NumericCode);   // nullable 6-digit code (Format 3 or 4)
```

**`QrResponseFormat` constants:**

| Constant | Wire value | Response fields |
|---|---|---|
| `DeeplinkOnly` | `1` | `deeplink` |
| `QrOnly` | `2` | `qr` |
| `QrAndDeeplink` | `3` | `qr` + `deeplink` |
| `Full` | `4` | `qr` + `deeplink` + `numericCode` |
| `NumericCodeOnly` | `5` | `numericCode` |

---

### GetInfoAsync

Polls the status of a transaction.
> ⚠️ **Rate Limit Warning:** This endpoint is rate-limited to **3 requests per minute (3 TPM)**.
> Rely on Webhooks for primary status updates and only fall back to `GetInfoAsync` after a delay.

```csharp
var info = await deuna.Payments.GetInfoAsync(new PaymentInfoRequest
{
    IdTransactionReference = transactionId,
    IdType = IdType.TransactionId, // IdType constants: TransactionId, InternalTransactionReference, TransferNumber
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
| `DeunaValidationException` | Guard-clause failure on a null, empty, or incorrectly constrained parameter |
| `DeunaBadRequestException` | API returned HTTP 400 Bad Request |
| `DeunaNotFoundException` | API returned HTTP 404 Not Found |
| `DeunaConflictException` | API returned HTTP 409 Conflict |
| `DeunaRateLimitException` | API returned HTTP 429 Too Many Requests |
| `DeunaServerException` | API returned HTTP 5xx Server Error |
| `DeunaApiException` | API returned an unmapped non-2xx status; carries `StatusCode` and `RawResponse` |
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


## Webhooks

DEUNA uses Webhooks to notify your backend when a payment is processed. Since webhooks do not use signatures, you should whitelist DEUNA's IPs or use a secret path segment (e.g. `/webhooks/deuna/your-secret-token`).

```csharp
app.MapPost("/webhooks/deuna", async (HttpRequest request) =>
{
    using var reader = new StreamReader(request.Body);
    var payloadString = await reader.ReadToEndAsync();
    
    var payload = DeunaWebhookParser.Parse(payloadString);
    if (payload.Status == "APPROVED")
    {
        // Process payment
    }
    
    return Results.Ok();
});
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
