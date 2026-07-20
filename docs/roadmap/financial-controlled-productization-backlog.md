# Controlled Productization Backlog

## Sprint 11 P4 productization state

Productization remains blocked after the P4 external escalation follow-up. PASS E2E real is NOT_READY because evidence is `NoResponse` / `EvidencePending` and preflight returned `SCRIPT_EXIT=2`. Unlock requires accepted external evidence and preflight `SCRIPT_EXIT=0`.

No SRI Production; No official ATS; No legal-final RIDE; No productive XAdES.

## Sprint 11 P3 productization state

Productization remains blocked after the P3 external evidence gate decision. PASS capture is NOT_READY because evidence is `NoResponse` / `EvidencePending` and preflight returned `SCRIPT_EXIT=2`. Unlock requires accepted external evidence and preflight `SCRIPT_EXIT=0`.

No SRI Production; No official ATS; No legal-final RIDE; No productive XAdES.

## Sprint 11 P2 productization state

Productization remains blocked after external evidence follow-up. PASS capture is not open because evidence is `NoResponse` / `EvidencePending` and preflight remains blocked until `SCRIPT_EXIT=0`.

No SRI Production; No official ATS; No legal-final RIDE; No productive XAdES. Financiero remains not production-ready.

## Sprint 11 P1 productization state

Productization remains blocked. Sprint 11 P1 directs remediation to Infra/Portal outside the Financiero repo. Financiero must not create SQL Server propio, Gateway propio, Portal Shell propio, Auth/Login propio, token storage, upload/download evidence or notification send to bypass the block.

Unlock requires accepted external evidence and preflight `SCRIPT_EXIT=0`.

No SRI Production; No official ATS; No legal-final RIDE; No productive XAdES. Financiero remains not production-ready.

| Area | Title | Description | Type | Risk | Dependencies | Status | Owner | No-production guardrail |
|---|---|---|---|---|---|---|---|---|
| Infrastructure | Shared SQL PASS evidence | Start shared SQL and verify `FinancieroDb` without creating SQL propio. | QA/DevOps | High | Portal local compose | Ready | DevOps Agent | No new SQL container. |
| Infrastructure | Portal Gateway/Shell PASS | Start Portal Gateway/Shell and capture health/context evidence. | QA/Portal | High | PortalCorporativo | Ready | Portal Integration Agent | No fake Portal PASS. |
| Portal Integration | Contract drift monitor | Compare expected PortalShellContext against runtime context. | Architecture | Medium | Portal Shell | Proposed | Architecture Governance Agent | No local identity substitute. |
| External Approvals | Filters and search | Add foundation-only filtering/search over approval requests. | UX | Low | Existing API | Proposed | UX Agent | No productive approval copy. |
| External Approvals | History/report view | Show sanitized decision and status history. | UX/QA | Medium | Audit/Outbox contract | Proposed | QA Lead Agent | No raw payloads. |
| Purchases | Synthetic productization QA | Execute purchases/anulados scenarios with synthetic data only. | QA | Medium | E2E PASS | Blocked | Product Owner Agent | No SRI real/ATS official. |
| Voided Documents | Synthetic voided scenarios | Expand safe validation evidence for anulados. | QA | Medium | E2E PASS | Blocked | Backend Agent | No real taxpayer data. |
| Content/File | Real Portal metadata contract | Use Portal-owned Content/File references as metadata only. | Integration | Medium | Portal Content/File | Proposed | Portal Integration Agent | No storage propio/upload real from Financiero. |
| Notification | Portal notification contract | Prepare controlled delivery via Portal Notification. | Integration | Medium | Portal Notification | Proposed | Portal Integration Agent | No SMTP/Teams propio. |
| Security/Legal/Tax | Approval gate checklist | Formalize legal/tax/security manual approval checklist. | Governance | High | External reviewers | Proposed | Security + Legal/Tax Review | No production activation. |
| QA/Automation | E2E evidence automation | Archive sanitized PASS/PARTIAL/BLOCKED_DEPENDENCY evidence. | QA | Medium | Shared SQL + Portal | Proposed | QA Lead Agent | No secrets/XML/certs. |

Production tax flows remain blocked until explicit governance approval.

Control tokens: No SRI Production; No official ATS; No legal-final RIDE; No productive XAdES. Financiero remains not production-ready; shared SQL and Portal Gateway/Shell PASS evidence is required first.

Sprint 9 P1 status: infrastructure items remain first priority and `BLOCKED_DEPENDENCY` until shared SQL and Portal runtime are available.

Sprint 9 P2 status: dependency diagnostics improved; infrastructure items remain blocked until external services are started.

Sprint 9 P3 status: dependency owner handoff and PASS checklist added; controlled productization remains blocked until E2E PASS.
# Sprint 9 P5 productization decision

Controlled productization is not approved for production. Sprint 10 should remediate external infrastructure first. Mocks/synthetic flows may continue only as non-production foundation work and must not replace real E2E PASS evidence.

Control tokens: BLOCKED_DEPENDENCY; Portal Gateway; shared SQL; not production-ready; No SRI Production; No official ATS; No legal-final RIDE; No productive XAdES.

# Sprint 9 P4 backlog update

Productization remains blocked by external infrastructure. New P4 backlog items are owner handoff completion, shared SQL intervention evidence, Portal Gateway health route confirmation and Portal Shell live evidence. Financiero must not add SQL Server, Gateway, Shell, token storage, upload/download, notification send or production tax activation.

Control tokens: BLOCKED_DEPENDENCY; Portal Gateway; shared SQL; not production-ready; No SRI Production; No official ATS; No legal-final RIDE; No productive XAdES.
