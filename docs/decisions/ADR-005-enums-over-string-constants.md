# ADR-005 — Strongly-Typed Enums Over String Constants

**Date:** 2026-09-12  
**Status:** Accepted (Supersedes [ADR-001](ADR-001-string-constants-over-enums.md))

## Context

Initially, the DEUNA SDK utilized `string` properties and `static class` string constants for properties like `QrType` and `QrResponseFormat` (as documented in ADR-001). This was due to the belief that the DEUNA API values were undocumented and highly volatile.

However, after reviewing the official DEUNA PDF documentation, the values for these properties are in fact strictly bounded and strictly numerical (e.g. `QrResponseFormat` ranges exactly from `1` to `5`, representing `DeeplinkOnly`, `QrAndDeeplink`, etc.).

## Decision

Migrate `QrType` and `QrResponseFormat` to proper C# `enum` types, using `System.Text.Json.Serialization.JsonStringEnumConverter` and `[JsonStringEnumMemberName]` where applicable, or relying on their underlying integer value when serialized as numbers.

## Consequences

- **Positive:** Increased developer productivity. Consumers can rely on IntelliSense and strict type-safety when initializing `PaymentRequest`, avoiding magic strings or integer typos.
- **Positive:** Cleaner internal validation logic.
- **Negative:** If DEUNA introduces a new `format` or `qrType` without updating the PDF docs, SDK consumers will be temporarily blocked from using it until a new SDK version is published. (This risk is mitigated by the official PDFs serving as a reliable contract).
