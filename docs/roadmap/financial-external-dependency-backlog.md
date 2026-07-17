# Financial External Dependency Backlog

## Purpose

Track external dependencies required before Financiero E2E PASS and productization.

| Item | Owner | Target date | Acceptance criteria | Evidence required | Status |
|---|---|---|---|---|---|
| Shared SQL runtime | Infra owner TBD | TBD | TCP `host.docker.internal:21433` open. | Sanitized TCP and SQL connectivity evidence. | `BLOCKED_DEPENDENCY` |
| `FinancieroDb` logical database | Infra/DBA owner TBD | TBD | Separate logical DB exists or migrations can initialize it. | Sanitized DB existence proof. | `BLOCKED_DEPENDENCY` |
| Portal Gateway health route | Portal Gateway owner TBD | TBD | Owner-confirmed route returns HTTP 2xx. | URL/path/status with no tokens. | `BLOCKED_DEPENDENCY` |
| Portal Shell runtime | Portal Shell owner TBD | TBD | Shell URL reachable. | Sanitized HTTP/browser evidence. | `BLOCKED_DEPENDENCY` |
| PortalShellContext | Portal owner TBD | TBD | `contractVersion=1.0`, source portal, menu, permissions, feature flags, correlation id. | Sanitized context sample. | `BLOCKED_DEPENDENCY` |
| Menu/permissions alignment | Portal Security/Menu owner TBD | TBD | Financial routes and permissions align with allow-list. | Sanitized menu/permission matrix. | `BLOCKED_DEPENDENCY` |
| Correlation id propagation | Portal/Infra owner TBD | TBD | Correlation id reaches Financiero. | Sanitized trace/header evidence. | `BLOCKED_DEPENDENCY` |
| Shared runtime evidence | Infra/Portal owner TBD | TBD | SQL + Gateway + Shell evidence returned together. | Evidence bundle. | `BLOCKED_DEPENDENCY` |
| E2E PASS evidence | QA owner TBD | TBD | Preflight exits `0` and smoke passes. | Sanitized final report. | `BLOCKED_DEPENDENCY` |

## Guardrails

- No SQL Server propio.
- No Gateway/Shell propios.
- No token storage.
- No upload/download evidence.
- No notification send.
- No production tax activation.

Control tokens: BLOCKED_DEPENDENCY; Portal Gateway; shared SQL; not production-ready; No SRI Production; No official ATS; No legal-final RIDE; No productive XAdES.
