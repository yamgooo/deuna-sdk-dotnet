# Changelog

All notable changes to `Deuna.Merchant.Sdk` are documented here.
This project adheres to [Semantic Versioning](https://semver.org/).

---

## [1.0.0] — 2026-09-12

### Added
- **Initial Public Release (Stable)**
- Full integration with the DEUNA Merchant Payments API for .NET 10.
- **Payment Request (`RequestAsync`)**: Generate dynamic or static QR codes and deep links.
- **Payment Query (`GetInfoAsync`)**: Check the status of a transaction (note: strictly rate limited to 3 TPM).
- **Cancellations & Refunds**: Endpoints to cancel pending transactions or void/refund successful ones (`CancelAsync`, `RefundAsync`).
- **Webhook Parsing**: Built-in helper `DeunaWebhookParser` and typed `DeunaPaymentWebhookPayload` to safely consume DEUNA notifications.
- **AOT & Trim Ready**: 100% source-generated JSON serialization, ensuring out-of-the-box compatibility with modern Native AOT and trimmed environments.
- **High-Performance Resilience**: Leverages `Microsoft.Extensions.Http.Resilience` to provide intelligent retries, circuit breaking, and timeout policies that automatically respect DEUNA API rate limits and 4xx/5xx failures.
- **Strongly-Typed Models & Guard Clauses**: Client-side validation stops malformed requests (like invalid QrTypes or oversized IDs) before they ever hit the network, converting them into explicit `DeunaValidationException`s.
- **Specific Error Types**: Precise exception types (`DeunaBadRequestException`, `DeunaRateLimitException`, etc.) make error handling in client applications easy and reliable.
- Includes comprehensive DI integration (`AddDeunaMerchantClient`) for standard ASP.NET Core `appsettings.json` configuration via `DeunaClientOptions`.
