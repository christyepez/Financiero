# Financial Sprint 11 P1 External Repo / Owner Handoff Checklist

## Sprint 11 P2 follow-up update

Date: 2026-07-20. No accepted external owner evidence was received. All checklist items remain `EvidencePending` / `BLOCKED_DEPENDENCY`; owner response is `NoResponse`. The unlock condition remains accepted external evidence plus preflight `SCRIPT_EXIT=0`.

Next action: continue external owner follow-up and use `docs/qa/financial-sprint-11-p2-return-to-pass-review.md` before reopening PASS capture.

No SRI Production; No official ATS; No legal-final RIDE; No productive XAdES.

Date: 2026-07-20  
State: `BLOCKED_DEPENDENCY`  
Purpose: handoff to Infra/Portal owners outside the Financiero repository.

Return to PASS requires shared SQL, Portal Gateway and `SCRIPT_EXIT=0`; until then the system remains not production-ready.

This external handoff must be completed outside the Financiero repo before PASS capture reopens.

| Item | Owner | Expected evidence | Target date | Status | Acceptance criteria |
|---|---|---|---|---|---|
| SQL Common remediation | SQL Common / Infra Owner | Sanitized TCP PASS for shared SQL host/port | Pending owner date | EvidencePending | Host resolves, TCP open, no secrets exposed |
| `FinancieroDb` availability | SQL Common / DBA Owner | Sanitized logical DB availability proof | Pending owner date | EvidencePending | Separate `FinancieroDb`, no shared domain DB |
| Portal Gateway health | Portal Gateway Owner | HTTP 2xx health/readiness route proof | Pending owner date | EvidencePending | Route/port documented and accepted |
| Portal Shell health | Portal Shell Owner | HTTP 2xx Shell health proof | Pending owner date | EvidencePending | Live Shell URL/path confirmed |
| PortalShellContext live | Portal Contract Owner | Sanitized context sample | Pending owner date | EvidencePending | No tokens/private URLs; contract version accepted |
| Menu/permissions live | Portal Security/Menu Owner | Read-only resources/permissions proof | Pending owner date | EvidencePending | Portal-owned permissions, no Financiero role storage |
| Correlation id live | Portal Observability Owner | Sanitized cross-service trace proof | Pending owner date | EvidencePending | Correlation id visible across Portal/Financiero path |
| Preflight return path | Joint owners + Financiero QA | Preflight output generated outside repo | Pending owner date | BLOCKED_DEPENDENCY | Exit code 0 only after all gates accepted |

## Evidence templates

- `docs/qa/templates/sql-common-evidence-template.md`.
- `docs/qa/templates/portal-gateway-evidence-template.md`.
- `docs/qa/templates/portal-shell-evidence-template.md`.
- `docs/qa/templates/portal-contract-evidence-template.md`.
- `docs/qa/financial-sprint-11-p1-return-to-pass-criteria.md`.

## Handoff rules

- Do not send passwords, tokens, private connection strings or private URLs.
- Do not send real certificates, XML reales, personal data or real SRI responses.
- Do not request SQL Server propio, Gateway propio, Shell propio or Auth/Login propio in Financiero.

No SRI Production; No official ATS; No legal-final RIDE; No productive XAdES. Financiero remains not production-ready.
