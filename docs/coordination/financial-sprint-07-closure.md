# Sprint 7 Closure - Portal Integration Readiness

## Executive summary

Sprint 7 closes Financiero as a Portal-integrated foundation, not as a production tax platform. P1-P4 hardened the Portal Shell contract, persisted external approval requests as foundation metadata, prepared Content/File and Notification boundaries, and exposed controlled productization readiness for purchases and voided documents.

Production SRI, SRI Test real send, official ATS, legal-final RIDE, productive XAdES, certificate custody, evidence upload and notification delivery remain blocked.

Control tokens: No SRI Production; No official ATS; No legal-final RIDE; No productive XAdES.

## Scope delivered

| Package | Delivered | Production status |
|---|---|---|
| P1 Real Portal Shell Contract | Contract version, source, capabilities, warnings, allow-listed menu and in-memory delegated auth. | Blocked until real Portal context is validated end-to-end. |
| P2 External Approval Persistence | Foundation approval requests, sanitized references and decisions. | Does not approve production. |
| P3 Content/File and Notification Boundary | Readiness and safe reference/intents contracts. | No upload/download/send. |
| P4 Productization Readiness | Read-only gates and blockers for purchases and voided documents. | Reports blockers only. |

## Implemented

- Portal-owned capabilities are consumed by contract/boundary, not duplicated.
- Runtime permissions remain delegated to Portal Security.
- UI keeps dangerous flags false and displays foundation/no-production disclaimers.
- Readiness endpoints are read-only and protected by existing permissions.
- External approval metadata is sanitized and reference-only.

## Foundation-only

- Purchases and voided documents.
- ATS mapping/readiness and ATS XML preview gated.
- RIDE/PDF foundation.
- External approval workflow.
- Productization readiness.
- Content/File and Notification integration boundaries.

## Blocked

- SRI Production and SRI Test real send.
- Official ATS.
- Legal-final RIDE.
- Productive XAdES.
- Real certificate custody.
- Real evidence upload/download through Portal Content/File.
- Real notification delivery through Portal Notification.
- Automatic productization activation.

## Dependencies

- Portal Security/Auth, Menu, Configuration, Audit, Outbox, Content/File and Notification remain Portal-owned.
- Local runtime depends on the shared SQL Server environment and logical database `FinancieroDb`.
- Legal/tax review is required before official tax outputs.

## Risks

- False confidence if foundation readiness is interpreted as production readiness.
- Integration drift if Portal Shell, Content/File or Notification contracts change.
- Health checks fail when shared SQL Server is not running.
- Real certificates, XML or taxpayer data must stay outside the repository.

## Sprint 8 recommendation

Prioritize Option A: end-to-end validation with real PortalCorporativo context, Gateway, Security/Menu/Configuration and shared SQL. Keep tax production flows blocked.

## No-production checklist

- [x] No login propio.
- [x] No roles propios.
- [x] No token storage.
- [x] No SRI Production.
- [x] No SRI Test real send.
- [x] No official ATS.
- [x] No legal-final RIDE.
- [x] No productive XAdES.
- [x] No certificates or real secrets.
- [x] No real XML/taxpayer evidence.
- [x] No SQL Server propio.
