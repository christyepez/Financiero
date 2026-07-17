# Controlled Productization Backlog

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
