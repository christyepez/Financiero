# Sprint 3 P3 - RIDE and Tax Reporting Foundation

Status: implemented as foundation.

Scope delivered:

- RIDE development generation per document type: invoice, credit note, debit note and withholding.
- Sanitized RIDE preview endpoint without XML payloads, certificate material or full access keys/customer identifiers.
- Tax reporting foundation for summary, document list, tax totals and withholding totals.
- Audit-only events for preview/reporting queries; no custom Audit/Outbox implementation.
- Smoke flow extended for RIDE per type and reporting summary.

Out of scope:

- Final legal RIDE layout.
- Official ATS generation.
- Production SRI calls.
- Real XAdES/certificate loading.
- Portal Content/File payload upload.
- Angular frontend.

Portal reuse:

- Security: reused `financial.electronicdocuments.read` and `generate`.
- Audit: reused Portal audit client.
- Outbox: unchanged for document lifecycle events.
- Storage: reused existing Development/Portal Content/File placeholder strategy.
- SQL: no SQL Server container added; no migration required for P3.

Acceptance evidence:

- `dotnet build Financiero.sln` passes.
- `DOTNET_ROLL_FORWARD=Major dotnet test Financiero.sln` passes: 152 tests.
- Direct `dotnet test` without roll-forward remains blocked by missing .NET 8 runtime on the workstation.
