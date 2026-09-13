# ADR-002 — IHttpClientFactory with AddStandardResilienceHandler

**Date:** 2026-09-12  
**Status:** Accepted

## Context

The SDK needs resilience (retry, backoff, circuit-breaker, timeout) on outgoing HTTP calls
without hand-rolling Polly policies.

Two options existed:
1. Raw `Polly` v8 policies registered manually via `AddPolicyHandler`.
2. `Microsoft.Extensions.Http.Resilience` (`AddStandardResilienceHandler`), which is the
   current Microsoft-endorsed abstraction over Polly v8.

## Decision

Use **`Microsoft.Extensions.Http.Resilience`** with `AddStandardResilienceHandler`.

`AddStandardResilienceHandler` provides a pre-configured, composable pipeline of:
- Total timeout (outer)
- Retry with exponential back-off + jitter
- Circuit breaker
- Attempt timeout (inner)

All configurable via `HttpStandardResilienceOptions`.

## Consequences

✅ Single package dependency, future-proofed as the `Microsoft.Extensions.Http` stack evolves.  
✅ Retry/circuit-breaker behaviour is tested by Microsoft; no bespoke policy tests needed.  
✅ Telemetry emitted automatically via `System.Diagnostics.Metrics` and OpenTelemetry.  
⚠️ Consumers on older .NET or who need very custom Polly pipelines must override
   the `HttpClientBuilder` — this is documented in `docs/usage.md`.
