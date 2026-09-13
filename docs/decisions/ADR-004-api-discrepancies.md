# ADR 004: API Discrepancies and Guard Clauses

## Status
Accepted

## Context
The initial implementation of the DEUNA Merchant API SDK was based on an exported Postman collection. Later, official PDF documentation was provided by DEUNA. 
There were several discrepancies between the two sets of documentation:
1. `PaymentRequest.Format` was documented as taking 0-2 in Postman, but the PDF specified 1-5 (where 1=DeeplinkOnly, 5=NumericCodeOnly).
2. `PaymentInfoRequest` showed `idType="0"` in Postman, but the PDF lists 0, 1, 2 for TransactionId, InternalTransactionReference, and TransferNumber, respectively, each with specific maximum lengths.
3. Postman omitted `ExpiredTime` for `PaymentRequest`, while the PDF indicated it was applicable for `Dynamic` QR codes and strictly disallowed for `Static`.
4. `RefundRequest.TransferNumber` was undocumented in length constraints in Postman, while the PDF required it to be `<= 12` characters.
5. The date field in `PaymentInfoResponse` was inconsistently formatted across the environments, which the PDF noted but did not resolve to a strict ISO8601 shape.

## Decision
1. **Official PDF Prevails**: We consider the DEUNA official PDF documentation as the ultimate source of truth.
2. **Client-side Guards**: We implemented strict guard clauses using `Ardalis.GuardClauses` to throw `DeunaValidationException` *before* hitting the network when the payload violates these documented constraints (e.g. `ExpiredTime` with `QrType.Static`, length limit on `TransferNumber`).
3. **Enum Corrections**: `QrResponseFormat` was updated to reflect 1-5 values.
4. **Resilience in Date Parsing**: Due to inconsistent date string formatting from the API, we continue to rely on the robust fallback parsing in `TryGetParsedDate()`, but added a length check (`<= 23`) based on the official PDF.

## Consequences
- **Positive**: The SDK strictly enforces DEUNA's rules, minimizing confusing HTTP 400 errors for developers and saving unnecessary network requests.
- **Negative**: If the DEUNA API evolves to allow longer IDs or new formats without notifying SDK maintainers, the strict guard clauses will prematurely reject valid requests. Developer feedback will be needed to loosen constraints in the future.
