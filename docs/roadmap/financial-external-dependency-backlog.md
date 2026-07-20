# Financial External Dependency Backlog

## Sprint 11 P1 external remediation backlog update

Sprint 11 P1 confirms remediation belongs outside the Financiero repo. The backlog remains active for shared SQL, `FinancieroDb`, Portal Gateway, Portal Shell, PortalShellContext, Menu/permissions, correlation id and preflight exit code 0.

- Current state: `BLOCKED_DEPENDENCY`.
- Owner external: SQL Common / Infra, SQL Common / DBA, Portal Gateway, Portal Shell, Portal Contract, Security/Menu and Observability.
- Return condition: accepted owner evidence plus `SCRIPT_EXIT=0`.
- Action next: execute external repo/owner handoff checklist.

No SRI Production; No official ATS; No legal-final RIDE; No productive XAdES. Financiero remains not production-ready.

## Sprint 10 P5 final backlog state

Sprint 10 closes as `BLOCKED_DEPENDENCY`. Pending dependencies remain shared SQL, `FinancieroDb`, Portal Gateway, Portal Shell, PortalShellContext, Menu/permissions, correlation id and preflight exit code 0.

- Owner pending: SQL Common / Infra, SQL Common / DBA, Portal Gateway, Portal Shell, Portal Contract, Security/Menu and Observability.
- SLA status: unresolved for Sprint 10 closure.
- Unlock condition: accepted owner evidence plus preflight exit 0.
- Sprint 11 decision: use `docs/roadmap/financial-sprint-11-decision-matrix.md`; recommended Option A is external infra remediation outside Financiero repo.

No SRI Production; No official ATS; No legal-final RIDE; No productive XAdES. Financiero remains not production-ready.

## Sprint 10 P4 executive block state

Date: 2026-07-20. P4 follow-up keeps shared SQL, `FinancieroDb`, Portal Gateway, Portal Shell, PortalShellContext, Menu/permissions and correlation id as `NoResponse` / `EvidencePending` / `BLOCKED_DEPENDENCY`.

Executive follow-up confirms shared SQL and Portal Gateway remain not production-ready until accepted evidence arrives.

- Executive action: block productization and escalate remediation outside the Financiero repository.
- New target: reopen PASS capture only after owner evidence is accepted and preflight exits 0.
- Closure path: prepare Sprint 10 P5 controlled closure if evidence remains unavailable.

No SRI Production; No official ATS; No legal-final RIDE; No productive XAdES. Financiero remains not production-ready.

## Sprint 10 P3 escalation state

Date: 2026-07-20. External SQL/Portal dependencies remain `EvidencePending` / `BLOCKED_DEPENDENCY`. P3 adds a formal escalation matrix, evidence request and remediation log instead of claiming PASS.

The unresolved runtime path explicitly includes shared SQL and Portal Gateway evidence before any PASS capture.

- SLA: 2 business days for SQL Common and Portal Gateway critical evidence; 3 business days for Portal Shell, PortalShellContext and menu/permissions; 4 business days for correlation id and joint E2E PASS.
- Escalation path: owner -> lead -> Architecture Governance -> Product Owner.
- Remediation log: `docs/qa/financial-sprint-10-p3-external-remediation-log.md`.
- Decision if unresolved: run Sprint 10 P4 as external remediation follow-up or stop productization until Portal/Infra is resolved outside Financiero.

No SRI Production; No official ATS; No legal-final RIDE; No productive XAdES. Financiero remains not production-ready.

## Sprint 10 P2 review update

No external SQL/Portal evidence was received. Current state remains `EvidencePending` or `Blocked` depending on whether the item is owner-provided or dependent on blocked runtime. Escalation paths remain active.

## Sprint 10 P1 intake update

Each dependency now requires an owner, target date, evidence artifact, acceptance gate, escalation path and current state. Evidence must use the sanitized templates under `docs/qa/templates`.

## Purpose

Track external dependencies required before Financiero E2E PASS and productization.

| Item | Owner | Target date | Evidence artifact | Acceptance gate | Escalation path | Current state after P1 | Acceptance criteria | Evidence required |
|---|---|---|---|---|---|---|---|---|
| Shared SQL runtime | SQL Common Owner TBD | TBD | `sql-common-evidence-template.md` | Gate 1 | Infra lead | EvidencePending | TCP `host.docker.internal:21433` open. | Sanitized TCP and SQL connectivity evidence. |
| `FinancieroDb` logical database | SQL Common/DBA Owner TBD | TBD | `sql-common-evidence-template.md` | Gate 2 | Infra/DBA lead | EvidencePending | Separate logical DB exists or migrations can initialize it. | Sanitized DB existence proof. |
| Portal Gateway health route | Portal Gateway Owner TBD | TBD | `portal-gateway-evidence-template.md` | Gate 3 | Portal lead | EvidencePending | Owner-confirmed route returns HTTP 2xx. | URL/path/status with no tokens. |
| Portal Shell runtime | Portal Shell Owner TBD | TBD | `portal-shell-evidence-template.md` | Gate 4 | Portal lead | EvidencePending | Shell URL reachable. | Sanitized HTTP/browser evidence. |
| PortalShellContext | Portal Contract Owner TBD | TBD | `portal-contract-evidence-template.md` | Gate 5 | Portal architecture lead | EvidencePending | `contractVersion=1.0`, source portal, menu, permissions, feature flags, correlation id. | Sanitized context sample. |
| Menu/permissions alignment | Portal Security/Menu Owner TBD | TBD | `portal-contract-evidence-template.md` | Gate 5 | Portal security lead | EvidencePending | Financial routes and permissions align with allow-list. | Sanitized menu/permission matrix. |
| Correlation id propagation | Portal/Infra Owner TBD | TBD | `portal-contract-evidence-template.md` | Gate 5 | Portal/Infra lead | EvidencePending | Correlation id reaches Financiero. | Sanitized trace/header evidence. |
| Financiero API health | Financiero Owner | TBD | Preflight evidence output | Gate 6 | Financiero lead | Blocked | API health passes after SQL available. | Sanitized health output. |
| Portal integration readiness | Financiero + Portal Owners | TBD | Preflight evidence output | Gate 7 | Joint owner review | Blocked | Readiness endpoint returns sanitized PASS. | Sanitized readiness output. |
| E2E PASS evidence | QA Owner TBD | TBD | Final QA evidence | Gate 8/Final | QA lead | Blocked | Preflight exits `0` and smoke passes. | Sanitized final report. |

## Guardrails

- No SQL Server propio.
- No Gateway/Shell propios.
- No token storage.
- No upload/download evidence.
- No notification send.
- No production tax activation.

Control tokens: BLOCKED_DEPENDENCY; Portal Gateway; shared SQL; not production-ready; No SRI Production; No official ATS; No legal-final RIDE; No productive XAdES.
