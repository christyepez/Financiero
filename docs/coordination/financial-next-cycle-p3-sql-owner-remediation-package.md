# Next Cycle P3 - SQL Owner Remediation Package

Date: 2026-07-20
Owner target: SQL Common / Infra + DBA
Current state: `BLOCKED_DEPENDENCY`

## SQL block summary

Financiero is not blocked by a demonstrated application defect. P2 and P3 preflight runs show the Financiero API is healthy locally, but real E2E cannot pass because the shared SQL endpoint is not reachable.

## Evidence from P2/P3

- `host.docker.internal` resolves to `172.20.20.141`.
- TCP `host.docker.internal:21433` is closed.
- `FinancieroDb` cannot be validated until shared SQL TCP is open.
- Financiero Compose is valid and defines no SQL Server container.
- Financiero API health/readiness returned HTTP 200 after Docker activation.

## Required SQL owner actions

1. Start or repair the shared SQL Server runtime used by PortalCorporativo, Financiero, CRM and future domains.
2. Expose SQL to the host on `host.docker.internal:21433`.
3. Confirm `FinancieroDb` exists as a separate logical database in the shared SQL Server.
4. Confirm Financiero does not require its own SQL Server container.
5. Provide sanitized evidence with date, owner and source.

## Expected ports and database

| Item | Expected value |
|---|---|
| SQL host | `host.docker.internal` |
| SQL TCP port | `21433` |
| Financiero logical DB | `FinancieroDb` |
| SQL ownership | Shared Portal/Infra runtime |
| Domain isolation | Separate logical databases, not separate SQL containers |

## Accepted evidence

- TCP connectivity PASS for `host.docker.internal:21433`.
- Sanitized SQL evidence proving `FinancieroDb` exists.
- Confirmation that no separate Financiero SQL Server container is required.
- Evidence must not include passwords, tokens, real connection strings, production URLs, personal data or screenshots with secrets.

## Validation commands

```powershell
Test-NetConnection host.docker.internal -Port 21433
docker compose config
pwsh -NoProfile -ExecutionPolicy Bypass -File tools/validate-portal-financiero-e2e.ps1 -OutputMarkdown -VerboseDiagnostics -SuggestFixes -PortalGatewayHealthPath /health -PortalShellHealthPath /health -FinancialApiHealthPath /health -EvidenceOutputPath <TEMP_PATH_OUTSIDE_REPO> -AcceptanceGateReport <TEMP_PATH_OUTSIDE_REPO>
```

DBA validation may use the team-approved SQL client, but evidence must be sanitized and must not expose secrets.

## Acceptance criteria

- Shared SQL TCP returns PASS.
- `FinancieroDb` is reachable in shared SQL.
- No SQL Server service is added to Financiero Compose.
- Preflight Gate 1 and Gate 2 pass.
- Final PASS capture is still withheld until Portal gates and `SCRIPT_EXIT=0` also pass.

## Suggested SLA

- Critical acknowledgement: same business day.
- Remediation target: 1 business day.
- Sanitized evidence delivery: immediately after remediation.

## Risks if unresolved

- PASS E2E real remains NOT_READY.
- Productization remains blocked.
- Teams may feel pressure to create a local SQL bypass, which is not allowed.

## What Financiero must not do

- Do not create SQL Server propio.
- Do not create a parallel database outside the shared SQL runtime.
- Do not modify Financiero Compose to include SQL.
- Do not commit connection strings, passwords, tokens, logs or screenshots with secrets.
