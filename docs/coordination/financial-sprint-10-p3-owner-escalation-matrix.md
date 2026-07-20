# Sprint 10 P3 Owner Escalation Matrix

## Sprint 10 P4 follow-up update

Date: 2026-07-20. P4 follow-up found `NoResponse` / `EvidencePending` for SQL Common, `FinancieroDb`, Portal Gateway, Portal Shell, PortalShellContext, Menu/permissions and correlation id. The operational SLA is unresolved for P4, so the executive action is to keep `BLOCKED_DEPENDENCY`, block productization and prepare Sprint 10 P5 controlled closure if owners do not provide accepted evidence.

Executive follow-up confirms shared SQL and Portal Gateway remain not production-ready until accepted evidence arrives.

References:

- `docs/qa/financial-sprint-10-p4-remediation-followup-evidence.md`.
- `docs/coordination/financial-sprint-10-p4-executive-block-decision.md`.

No SRI Production; No official ATS; No legal-final RIDE; No productive XAdES. Financiero remains not production-ready.

Date: 2026-07-20  
State: `EvidencePending` / `BLOCKED_DEPENDENCY`  
Scope: external owner escalation only. No PASS, no production activation, no duplicated Portal or SQL capabilities.

## Escalation matrix

| Dependency | Required owner | Current state | Missing evidence | Impact | Severity | Request date | Target date | Recommended SLA | Escalation path | Decision if no response | Next action |
|---|---|---|---|---|---|---|---|---|---|---|---|
| SQL Common TCP | SQL Common / Infra Owner | NotReceived / EvidencePending | Sanitized TCP PASS for shared SQL on configured host/port | Blocks FinancieroDb, API readiness and E2E PASS | Critical | 2026-07-20 | 2026-07-22 | 2 business days | Infra lead -> Architecture Governance -> Product Owner | Keep `BLOCKED_DEPENDENCY`; do not create SQL Server propio | Send formal evidence request |
| SQL `FinancieroDb` | SQL Common / DBA Owner | NotReceived / EvidencePending | Sanitized database existence/connectivity proof for `FinancieroDb` | Blocks runtime validation and migrations evidence | Critical | 2026-07-20 | 2026-07-22 | 2 business days | DBA owner -> Infra lead -> Architecture Governance | Keep mocks/local docs only; no shared database reuse | Request DB-level sanitized evidence |
| Portal Gateway health route | Portal Gateway Owner | NotReceived / EvidencePending | HTTP 2xx health/readiness path and route/port confirmation | Blocks Gateway reuse and Portal integration readiness | Critical | 2026-07-20 | 2026-07-22 | 2 business days | Portal Gateway owner -> Portal Architect -> Product Owner | Do not create Gateway propio; schedule P4 follow-up | Confirm route and rerun preflight |
| Portal Shell health route | Portal Shell Owner | NotReceived / EvidencePending | Live Shell health URL/path and availability proof | Blocks Shell-integrated UX PASS | High | 2026-07-20 | 2026-07-23 | 3 business days | Shell owner -> Portal Architect -> Product Owner | Keep standalone development-only mode | Request sanitized Shell evidence |
| PortalShellContext live | Portal Contract Owner | NotReceived / EvidencePending | Live `PortalShellContext` contract sample without tokens or private URLs | Blocks delegated auth/menu/permissions integration PASS | High | 2026-07-20 | 2026-07-23 | 3 business days | Portal Contract owner -> Architecture Governance | Keep `PortalShellContext` unaccepted | Request contract proof |
| Menu/permissions live | Portal Security/Menu Owner | NotReceived / EvidencePending | Live menu/resources/permissions evidence for Financiero resources | Blocks end-user authorization acceptance | High | 2026-07-20 | 2026-07-23 | 3 business days | Security/Menu owner -> Portal Architect | Do not persist roles/permissions propios | Request read-only sanitized evidence |
| Correlation id live | Portal Observability Owner | NotReceived / EvidencePending | Cross-service correlation id propagation evidence | Blocks traceability acceptance | Medium | 2026-07-20 | 2026-07-24 | 4 business days | Observability owner -> DevOps lead | Keep traceability risk open | Request log snippet with sanitized IDs |
| E2E preflight PASS | Joint SQL + Portal + Financiero Owners | BLOCKED_DEPENDENCY | Exit code 0 with real external evidence accepted | Blocks productization and PASS capture | Critical | 2026-07-20 | 2026-07-24 | 4 business days | Product Owner -> Architecture Governance -> owner leads | Execute Sprint 10 P4 external remediation follow-up or resolve outside Financiero repo | Re-run only after owner evidence arrives |

## Acceptance policy

- Evidence must be sanitized and must not include passwords, tokens, connection strings, private URLs, certificates, XML reales, personal data or real SRI responses.
- `BLOCKED_DEPENDENCY` remains the only valid final state until owner evidence is accepted and the preflight exits 0.
- No SQL Server propio, Gateway propio, Portal Shell propio, login/Auth propio or token storage may be created in Financiero.

No SRI Production; No official ATS; No legal-final RIDE; No productive XAdES. Financiero remains not production-ready.
