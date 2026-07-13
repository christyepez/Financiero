# ADR-008 - RIDE and Tax Reporting Foundation

Date: 2026-07-13

Decision: implement a development-safe RIDE and tax reporting foundation inside Financiero without duplicating Portal capabilities.

Rationale:

- RIDE generation needs document-type-specific models before a final legal layout.
- Reporting must summarize existing electronic documents without generating official ATS files yet.
- Sensitive SRI data must be masked in previews and reporting outputs.
- Portal remains owner of Security, Audit, Outbox, Notification, Gateway and future Content/File production storage.

Accepted design:

- RIDE models are generated from `ElectronicDocument` through an application mapper.
- `IRidePdfGenerator` exposes one method per supported document type.
- Development generator returns placeholder PDF bytes plus sanitized HTML.
- Tax reporting uses existing electronic document persistence and does not add tables in this sprint.
- Reporting endpoints reuse `financial.electronicdocuments.read`.
- RIDE generation continues to use `financial.electronicdocuments.generate`.

Consequences:

- No migration 012 is required for P3.
- Reports are foundation summaries, not official fiscal declarations.
- The final legal RIDE and ATS remain Sprint 4/backlog items.
