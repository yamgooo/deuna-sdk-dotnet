# ADR-003 — System.Text.Json Source Generation (AOT-Friendly Serialization)

**Date:** 2026-09-12  
**Status:** Accepted

## Context

The SDK must deserialize DEUNA's JSON responses, which contain specific API shapes (e.g. inconsistent string formats for `date`, various error response shapes).
Historically, .NET libraries used reflection-based `Newtonsoft.Json` or `System.Text.Json`. 
However, modern .NET deployments (like AWS Lambda or Azure Container Apps) increasingly use Native AOT, which forbids reflection. 

We need a serialization approach that guarantees the SDK can be used in AOT-compiled services while accurately mapping DEUNA's wire formats.

## Decision

Use **Source-Generated `System.Text.Json`** via `JsonSerializerContext`.

All models (Requests, Responses, and Webhooks) are registered in a partial `DeunaJsonContext`. 
The `PaymentClient` uses this context directly during `PostAsync` rather than relying on reflection.

All serialization in `PaymentClient` is done via `JsonTypeInfo<T>` overloads
(`ReadFromJsonAsync`, `PostAsJsonAsync`), ensuring no reflection is used in the hot path.

## Consequences

✅ SDK works correctly in Native AOT and trimmed deployments (Blazor WASM, NativeAOT).  
✅ First-call performance is better — no JIT-time reflection.  
✅ Zero runtime surprises from missing members or renamed properties.  
⚠️ Every new request/response type must be added to `DeunaJsonContext` — a small but
   non-zero maintenance cost. The compiler will warn on `GetTypeInfo()` calls for
   unregistered types.
