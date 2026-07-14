# ADR-009 - Tax Exports and ATS Readiness Foundation

Date: 2026-07-14

Decision: add internal tax export and ATS readiness foundation without producing official fiscal files.

Rationale:

- Consumers need safe downloadable summaries before official ATS implementation.
- ATS readiness should detect missing data and tax-review needs without claiming regulatory compliance.
- Portal remains owner of cross-cutting capabilities; Financiero only creates domain reporting/export logic.

Accepted design:

- `TaxExportService` reuses `TaxReportingService` and existing `ElectronicDocument` persistence.
- Exports are generated in memory as JSON or CSV and include metadata/hash.
- ATS readiness returns summaries, issues and an explicit non-official disclaimer.
- Advanced reporting exposes action queue and monthly summaries.
- Endpoints reuse `financial.electronicdocuments.read`.
- Audit captures metadata only: period, format, row count, hash and status.

Consequences:

- No migration 012 is required.
- Exported files are foundation/dev/internal, not official tax artifacts.
- Official ATS, legal RIDE and production Content/File upload remain future work.
