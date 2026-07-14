# Financial Sprint 4 Architecture Snapshot

## Boundaries

Financiero owns financial/tax domain behavior. PortalCorporativo owns cross-cutting Security, Audit, Content/File, configuration and platform concerns.

## Portal reuse

- Security permissions reused through runtime authorization.
- Audit reused for readiness/gap queries and document operations.
- Content/File is the target for document ownership; Financiero stores metadata/hash only.
- Shared SQL Server is reused; Financiero keeps logical database `FinancieroDb`.

## Components created or extended

- Content/File storage readiness and HTTP contract.
- RIDE template/readiness foundation.
- ATS readiness and official design foundation.
- Tax/legal gap analysis and approval checklist.
- Review templates and release documentation.

## Flows

- RIDE readiness: document metadata -> sanitized RIDE model -> readiness issues/disclaimer.
- ATS design: period query -> internal summaries/design sections -> unsupported/gap reasons.
- Tax/legal gaps: read-only services -> gap/checklist DTOs -> Portal Audit.
- Content/File readiness: configuration -> dry-run/metadata/hash -> no owned storage.
- Content/File HTTP contract: optional HTTP adapter behind dry-run and payload gates.

## Security, audit and configuration

All sensitive production paths remain gated. No real certificate, XML payload, token or SRI production URL is stored. Read-only endpoints require existing permissions. Auditable queries use Portal Audit.

## Explicit exclusions

No own storage, no SQL Server of its own, no production SRI, no official ATS, no final legal RIDE and no frontend implementation.
