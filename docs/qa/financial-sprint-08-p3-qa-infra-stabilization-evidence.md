# Sprint 8 P3 QA Infrastructure Stabilization Evidence

## Result classification

Expected statuses:

- `PASS`: all required checks pass.
- `BLOCKED_DEPENDENCY`: external dependency is unavailable.
- `FAIL`: repository/configuration/script issue.

## Script result

Command:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File tools/validate-portal-financiero-e2e.ps1 -OutputMarkdown
```

Current result: `BLOCKED_DEPENDENCY` is acceptable when shared SQL or Portal runtime is not running.

Observed local result:

| Check | Status | Detail |
|---|---|---|
| PowerShell | PASS | 7.6.3 |
| Shared SQL host resolution | PASS | `host.docker.internal` resolves. |
| Docker compose config | PASS | Financiero compose is valid. |
| Shared SQL TCP | BLOCKED_DEPENDENCY | `host.docker.internal:21433` unavailable. |
| No Financiero SQL container | PASS | Financiero does not define its own SQL Server container. |
| Financiero health/readiness | BLOCKED_DEPENDENCY | API not reachable because container exits on SQL dependency startup. |
| Portal Gateway health | BLOCKED_DEPENDENCY | Portal local health endpoint not reachable. |

Observed script exit classification: `2` / `BLOCKED_DEPENDENCY`.

## Shared SQL status

- Host resolution: to be captured by script.
- TCP `host.docker.internal:21433`: expected PASS when shared SQL is running.
- `FinancieroDb`: validate externally without printing connection strings.
- Financiero SQL container: must remain absent.

## Portal Gateway/Shell status

- Gateway health: expected PASS when PortalCorporativo is running.
- Shell base URL: optional explicit check via `-PortalShellBaseUrl`.

## Financiero API status

- Build/test PASS is required.
- Runtime health can be `BLOCKED_DEPENDENCY` when shared SQL is down.
- Do not treat SQL-down readiness as app failure.

## Readiness status

`/api/financial/portal-integration/readiness` classifies readiness as:

- `Ready`.
- `BlockedDependency`.
- `NotConfigured`.
- `FoundationOnly`.
- `ProductionBlocked`.

Current foundation response remains read-only and production blocked.

## Evidence expected for PASS

- Shared SQL TCP PASS.
- Portal Gateway health PASS.
- Financiero `/health/live` PASS.
- Financiero `/health/ready` PASS.
- Portal readiness endpoint PASS.
- No SQL Server propio.
- No production tax flags.

## Evidence accepted for BLOCKED_DEPENDENCY

- Compose config PASS.
- No SQL Server propio PASS.
- SQL or Portal checks BLOCKED_DEPENDENCY.
- Health blocked due to dependency.

## No-production evidence

- No SRI Test real.
- No SRI Production.
- No official ATS.
- No legal-final RIDE.
- No productive XAdES.
- No upload real.
- No notification send real.
