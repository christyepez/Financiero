# Financial E2E Dependency Owner Handoff

Sprint 9 P5 closes with `BLOCKED_DEPENDENCY`. This handoff is now the required Sprint 10 entry artifact for Infra/Portal owners.

## Purpose

Hand off the external blockers needed to achieve Financiero E2E PASS without changing ownership boundaries.

## Portal/Infra team needs to provide

The first dependency is shared SQL; without shared SQL evidence Financiero must remain `BLOCKED_DEPENDENCY`.

| Dependency | Expected evidence |
|---|---|
| Shared SQL runtime | TCP success for host/port, no credentials. |
| SQL host/port | Confirm `host.docker.internal:21433` or provide override. |
| Logical database | Confirm `FinancieroDb` exists or can be initialized. |
| Portal Gateway | Base URL and health path returning HTTP 2xx. |
| Portal Shell | Shell URL reachable from local browser/HTTP check. |
| PortalShellContext | `contractVersion=1.0`, `source=portal`, menu, permissions, feature flags, correlation id. |

## Financiero team needs to provide

- Docker compose config evidence.
- API health/readiness evidence after SQL is reachable.
- Angular build/static verification evidence.
- Preflight output from `tools/validate-portal-financiero-e2e.ps1`.
- Sanitized final PASS or BLOCKED evidence.

## Must be lifted outside Financiero

- Shared SQL Server.
- Portal Gateway.
- Portal Shell.
- Portal Security/Auth/Menu/Configuration/Audit/Outbox/Content/File/Notification.

## Expected ports and URLs

- Shared SQL: `host.docker.internal:21433`.
- Portal Gateway: `http://localhost:8082`.
- Portal Gateway health path: `/health` unless Portal owner provides another path.
- Portal Shell: `http://localhost:4201` or Portal-provided URL.
- Financiero API: `http://localhost:8083`.

## Do not do

- Do not create SQL Server inside Financiero.
- Do not hardcode secrets.
- Do not commit `.env`, tokens, passwords or connection strings.
- Do not activate SRI Production, official ATS, legal-final RIDE or productive XAdES.
- Do not duplicate Portal Gateway/Shell/Auth/Menu/Configuration/Audit/Outbox/Content/File/Notification.
- Do not convert `BLOCKED_DEPENDENCY` into PASS without evidence.

## Return evidence format

Return sanitized text only:

- dependency name;
- host/port or URL without credentials;
- PASS/BLOCKED/FAIL status;
- timestamp;
- corrective action if blocked.

Financiero remains not production-ready until dependencies are available and preflight exits `0`.

Control tokens: No SRI Production; No official ATS; No legal-final RIDE; No productive XAdES.

## Sprint 9 P4 owner checklist

| Owner | Target date | Must return |
|---|---|---|
| SQL owner | TBD | Sanitized TCP evidence for `host.docker.internal:21433`, `FinancieroDb` availability and no Financiero SQL container. |
| Portal Gateway owner | TBD | Base URL, agreed health route, HTTP 2xx evidence and correlation id behavior. |
| Portal Shell owner | TBD | Shell URL, health route, PortalShellContext contract evidence, menu and permissions evidence. |

Accepted routes and ports:

- SQL port: `21433` unless Infra provides an approved override.
- Gateway health route: `/health` unless Portal documents another route.
- Shell health route: `/health` unless Portal documents another route.

Run the preflight with explicit route parameters:

```powershell
.\tools\validate-portal-financiero-e2e.ps1 -OutputMarkdown -VerboseDiagnostics -SuggestFixes -PortalGatewayHealthPath /health -PortalShellHealthPath /health -FinancialApiHealthPath /health
```
