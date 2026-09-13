# Changelog

All notable changes to `Deuna.Merchant.Sdk` are documented here.
This project adheres to [Semantic Versioning](https://semver.org/).

---

## [0.1.0] — 2026-09-12

### Added
- Initial release of `Deuna.Merchant.Sdk`
- `IPaymentClient.RequestAsync` — create a payment QR / deeplink (4 format variants)
- `IPaymentClient.GetInfoAsync` — query transaction status with `TryGetParsedDate()` helper
- `IPaymentClient.CancelAsync` — cancel a pending payment
- `IPaymentClient.RefundAsync` — void/refund an approved transaction; maps 400 to `DeunaApiException`
- `AddDeunaMerchantClient(Action<DeunaClientOptions>)` and `AddDeunaMerchantClient(IConfiguration)` DI extensions
- `DeunaAuthHandler` — `x-api-key` / `x-api-secret` injection via `DelegatingHandler`
- `Microsoft.Extensions.Http.Resilience` standard pipeline (retry, circuit-breaker, timeout)
- Source-generated `DeunaJsonContext` for AOT/trimming compatibility
- `Ardalis.GuardClauses` guard-clause validation on all public inputs
- Typed exceptions: `DeunaException`, `DeunaApiException`, `DeunaValidationException`
- Unit tests (xUnit + FluentAssertions + mocked `HttpMessageHandler`) — 16 tests
- Integration tests (WireMock.Net) — 6 tests
- Console sample app reading credentials from environment variables
- GitHub Actions CI (`ci.yml`) and release (`release.yml`) workflows
- Architecture Decision Records (ADR-001, ADR-002, ADR-003)
- Full XML doc comments on all public API surface
