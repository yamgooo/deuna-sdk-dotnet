# ADR-002 — IHttpClientFactory with AddStandardResilienceHandler

**Date:** 2026-09-12  
**Status:** Accepted

## Context

The DEUNA API has strict operational limits, particularly on the `GetInfoAsync` endpoint which enforces a **3 requests per minute (3 TPM)** rate limit. Additionally, transient network errors or HTTP 409 (Conflict) / HTTP 429 (Too Many Requests) responses must be handled gracefully by clients to prevent cascading failures.
The SDK needs built-in resilience (retry, backoff, circuit-breaker, timeout) on outgoing HTTP calls specifically tailored to DEUNA's limits, without hand-rolling custom Polly policies.

Two options existed:
1. Raw `Polly` v8 policies registered manually via `AddPolicyHandler`.
2. `Microsoft.Extensions.Http.Resilience` (`AddStandardResilienceHandler`), which provides pre-configured best-practices.

Use **`Microsoft.Extensions.Http.Resilience`** with `AddStandardResilienceHandler`.

`AddStandardResilienceHandler` provides a pre-configured, composable pipeline of:
- Total timeout (outer)
- Retry with exponential back-off + jitter (crucial for respecting DEUNA's 429 responses and 3 TPM limit)
- Circuit breaker (to prevent hammering the DEUNA API if it goes down)
- Attempt timeout (inner)

All configurable via `DeunaClientOptions` mapping to `HttpStandardResilienceOptions`.

## Consequences

✅ Single package dependency, future-proofed as the `Microsoft.Extensions.Http` stack evolves.  
✅ Retry/circuit-breaker behaviour is tested by Microsoft; no bespoke policy tests needed.  
✅ Telemetry emitted automatically via `System.Diagnostics.Metrics` and OpenTelemetry.  
⚠️ Consumers on older .NET or who need very custom Polly pipelines must override
   the `HttpClientBuilder` — this is documented in `docs/usage.md`.
