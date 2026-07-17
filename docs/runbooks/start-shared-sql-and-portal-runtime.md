# Start Shared SQL and Portal Runtime

## Purpose

Bring the external dependencies required for Financiero E2E validation online without duplicating PortalCorporativo capabilities inside Financiero.

## Required before Financiero

1. Shared SQL Server owned by Portal/shared infrastructure.
2. PortalCorporativo Gateway.
3. PortalCorporativo Shell.
4. Optional observability such as Seq.

Do not start or add a Financiero-owned SQL Server, Gateway, Shell, Identity, Menu, Configuration, Audit, Outbox, Content/File or Notification service.

## Expected local endpoints

| Dependency | Expected value |
|---|---|
| Shared SQL host | `host.docker.internal` |
| Shared SQL port | `21433` |
| Financiero database | `FinancieroDb` |
| Portal Gateway | `http://localhost:8082` |
| Portal Gateway health | `/health` |
| Portal Shell | `http://localhost:4201` or environment-specific URL |
| Financiero API | `http://localhost:8083` |

## Startup sequence

1. Start shared SQL from Portal/shared infrastructure.
2. Validate SQL TCP:

   ```powershell
   Test-NetConnection host.docker.internal -Port 21433
   ```

3. Start Portal Gateway and Shell from PortalCorporativo.
4. Validate Portal Gateway:

   ```powershell
   Invoke-WebRequest -UseBasicParsing http://localhost:8082/health
   ```

5. Validate Portal Shell in browser or with HTTP check when available.
6. Start Financiero API only after SQL is reachable:

   ```powershell
   docker compose up -d --build financial-api
   ```

7. Validate Financiero:

   ```powershell
   Invoke-WebRequest -UseBasicParsing http://localhost:8083/health
   Invoke-WebRequest -UseBasicParsing http://localhost:8083/health/live
   Invoke-WebRequest -UseBasicParsing http://localhost:8083/health/ready
   ```

8. Run preflight:

   ```powershell
   .\tools\validate-portal-financiero-e2e.ps1 -OutputMarkdown -VerboseDiagnostics -SuggestFixes
   ```

## Environment overrides

Use script parameters, local environment variables or untracked `.env` files. Do not commit `.env` or real secrets.

```powershell
.\tools\validate-portal-financiero-e2e.ps1 `
  -SqlHost host.docker.internal `
  -SqlPort 21433 `
  -PortalBaseUrl http://localhost:8082 `
  -PortalShellBaseUrl http://localhost:4201 `
  -FinancialBaseUrl http://localhost:8083 `
  -OutputMarkdown `
  -VerboseDiagnostics `
  -SuggestFixes
```

## PASS evidence

- SQL DNS resolves.
- SQL TCP succeeds.
- Portal Gateway health returns HTTP 2xx.
- Portal Shell is reachable.
- Financiero health/live/ready return HTTP 2xx.
- `/api/financial/portal-integration/readiness` is reachable and sanitized.
- Preflight exits `0`.

Sprint 9 P3 observed blocker: Portal Gateway base responded but `/health` returned `HTTP 404`. Confirm the correct Portal health path or gateway route before marking Portal PASS.

## BLOCKED_DEPENDENCY evidence

- `HOST_NOT_RESOLVED`.
- `HOST_RESOLVES_BUT_PORT_CLOSED`.
- `HTTP_ENDPOINT_UNREACHABLE`.
- `HTTP_STATUS_UNEXPECTED`.
- Service skipped intentionally during scoped validation.

## Troubleshooting

- If SQL DNS resolves but TCP is closed, start shared SQL or correct the port mapping.
- If Portal Gateway is unreachable, start PortalCorporativo or override `-PortalBaseUrl`.
- If Portal Shell is unreachable, start Shell or override `-PortalShellBaseUrl`.
- If Financiero API is down while SQL is down, fix SQL first.
- If readiness is blocked after services are up, inspect sanitized readiness blockers.

## Security guardrails

No SRI Production; No official ATS; No legal-final RIDE; No productive XAdES.

Financiero remains not production-ready until real E2E PASS evidence exists.

Never commit passwords, tokens, full connection strings, certificates, real XML, SRI responses, personal data, screenshots with secrets or generated artifacts.
# Sprint 9 P5 closure

Use this runbook as part of Sprint 10 External Infra Remediation. Sprint 9 final status is `BLOCKED_DEPENDENCY`; do not mark PASS until SQL/Gateway/Shell evidence is returned.

Control tokens: Portal Gateway; shared SQL; not production-ready; No SRI Production; No official ATS; No legal-final RIDE; No productive XAdES.

# Sprint 9 P4 external intervention

Use `docs/runbooks/infra-sql-common-intervention-package.md` and `docs/runbooks/portal-runtime-intervention-package.md` before attempting PASS. Confirm shared SQL port `21433`, Portal Gateway health route and Portal Shell route. Run preflight with explicit `-PortalGatewayHealthPath`, `-PortalShellHealthPath` and `-FinancialApiHealthPath`.

Control tokens: BLOCKED_DEPENDENCY; Portal Gateway; shared SQL; not production-ready; No SRI Production; No official ATS; No legal-final RIDE; No productive XAdES.
