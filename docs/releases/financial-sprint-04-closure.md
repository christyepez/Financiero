# Financial Sprint 4 Closure

Sprint 4 objective: close the fiscal/reporting readiness layer for electronic documents without enabling production SRI, real certificates, real XML payloads or own document storage.

## Included packages

- P1 Content/File storage readiness: metadata/hash registration, `store-ride`, export storage readiness and `/health/content-file`.
- P2 Content/File HTTP upload contract: dry-run HTTP contract, validation, token placeholder and safe defaults.
- P3 RIDE legal / ATS design foundation: RIDE templates/readiness and ATS official design foundation with disclaimers.
- P4 Tax/legal review gaps: read-only gaps, checklist and evidence templates.

## Implemented capabilities

- Portal Content/File readiness without owning storage.
- RIDE foundation and legal-readiness checks for factura, nota de crédito, nota de débito and retención.
- ATS readiness and ATS official-design foundation.
- Tax/legal gap model and external review templates.
- Security permissions, audit and SQL-common model preserved.

## Explicitly non-productive

- No final legal RIDE.
- No official ATS XML.
- No SRI production or real SRI test send.
- No XAdES productive signing.
- No real certificates, secrets, taxpayer XML, SRI sensitive responses or payload storage.

## Blockers and decisions

- External Ecuador tax/legal review is required before RIDE final or official ATS.
- Official SRI schemas/catalogs must be approved before ATS XML generation.
- Certificate custody and security production gate must be approved before XAdES real signing.
- Portal Content/File real payload upload remains gated by Portal contract approval.

## QA evidence

Last reported local test run: 203 tests passed. `dotnet restore`, `dotnet build`, `dotnet test`, `DOTNET_ROLL_FORWARD=Major dotnet test`, `docker compose config`, SQL validation and secret/cert/XML scans were used across Sprint 4 packages.

Docker runtime remained blocked by external MCR timeout resolving `mcr.microsoft.com/dotnet/aspnet:8.0`; health and smoke are not declared OK when Docker did not start.

## Current states

- SQL common: aligned. Financiero uses logical `FinancieroDb`; no SQL Server container owned by Financiero.
- Security: no real secrets/certificates; production gates required.
- SRI production: blocked.
- Portal Content/File: ready for contract/dry-run; real payloads remain gated.
- RIDE: foundation/readiness only.
- ATS: design/readiness only.

## Sprint 5 recommendation

Default to Sprint 5 Option A: purchases and voided documents foundation, unless external gates approve XAdES/SRI Test, Portal Content/File real upload, final RIDE or ATS official XML work.
