# ADR-001 — String Constants Instead of Enums for Extensible API Values

**Date:** 2026-09-12  
**Status:** Superseded by ADR-005

## Context

The DEUNA Merchant API uses string values for several fields whose set of valid values
is not documented as closed: `format` (payment request), `status` (payment info response),
and `qrType`.

For `format`, the Postman collection shows `"0"`, `"1"`, `"2"`, and `"5"` — but the gap
between 2 and 5 strongly suggests undocumented values exist or will be added.

For `status`, the collection shows `"PENDING"` and `"APPROVED"`, but real-world payment
APIs routinely add statuses (`"EXPIRED"`, `"DECLINED"`, `"PROCESSING"`, etc.).

## Decision

- **`QrResponseFormat`** and **`QrType`** are implemented as `static class` with `const string`
  members rather than C# `enum` types.
- **`PaymentInfoResponse.Status`** is typed as `string`, not an enum.
- **`PaymentResponse.Status`** is typed as `string`.

## Consequences

✅ Consumer code will not break when the DEUNA API adds new values.  
✅ The JSON contract is exact — no serialization attribute wrangling needed.  
⚠️ Consumers cannot exhaustively switch over values without compiler warnings — this is
   intentional. Use `if`/`else if` or pattern matching with a default/fallthrough arm.

## Alternatives Considered

- **Closed enum + `Unknown` sentinel**: Would still silently map novel values to `Unknown`,
  hiding information rather than surfacing it.
- **`[JsonConverter]` with custom enum that falls back to `Unknown`**: More complex; the
  string approach is simpler and more honest about the open-world assumption.
