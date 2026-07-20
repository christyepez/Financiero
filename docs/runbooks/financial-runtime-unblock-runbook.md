# Financial Runtime Unblock Runbook

Purpose: activate real Financiero runtime against shared Portal/SQL dependencies without duplicating platform infrastructure.

## Expected ports and endpoints

| Component | Expected endpoint |
|---|---|
| Shared SQL | `host.docker.internal:21433` |
| Financiero API | `http://localhost:8083` |
| Financiero health | `/health`, `/health/live`, `/health/ready` |
| Portal Gateway | `http://localhost:8082/health` or owner-confirmed route |
| Portal Shell | owner-confirmed `PortalShellBaseUrl` + `/health` |

## Step-by-step

1. Confirm branch is based on GitHub `main`.
2. Validate Compose:
   - `docker compose config`
3. Confirm Financiero does not define SQL Server:
   - search for `mcr.microsoft.com/mssql`, `1433:1433`, or SQL container names in `docker-compose.yml`.
4. Validate SQL common TCP:
   - `Test-NetConnection host.docker.internal -Port 21433`
5. Start Financiero only after confirming no SQL propio is added:
   - `docker compose up -d --build`
6. Validate Financiero API:
   - `Invoke-WebRequest -UseBasicParsing http://localhost:8083/health`
   - `Invoke-WebRequest -UseBasicParsing http://localhost:8083/health/ready`
7. Validate Portal Gateway:
   - `Invoke-WebRequest -UseBasicParsing http://localhost:8082/health`
8. Validate Portal Shell and PortalShellContext using owner-provided URL/evidence.
9. Run preflight:
   - `pwsh -NoProfile -ExecutionPolicy Bypass -File tools/validate-portal-financiero-e2e.ps1 -OutputMarkdown -VerboseDiagnostics -SuggestFixes -PortalGatewayHealthPath /health -PortalShellHealthPath /health -FinancialApiHealthPath /health -EvidenceOutputPath <TEMP_PATH_OUTSIDE_REPO> -AcceptanceGateReport <TEMP_PATH_OUTSIDE_REPO>`

## P3 owner handoff artifacts

- SQL owner package: `docs/coordination/financial-next-cycle-p3-sql-owner-remediation-package.md`.
- Portal owner package: `docs/coordination/financial-next-cycle-p3-portal-owner-remediation-package.md`.
- Owner handoff message: `docs/coordination/financial-next-cycle-p3-owner-handoff-message.md`.
- Accepted evidence checklist: `docs/qa/financial-next-cycle-p3-accepted-evidence-checklist.md`.

Use these artifacts before rerunning PASS capture. They define owners, SLA, expected commands and evidence acceptance criteria.

## P4 pause state

P4 did not receive accepted SQL/Portal owner evidence and the preflight returned `SCRIPT_EXIT=2`. Productization is paused. Use this runbook only to revalidate after owners remediate; do not use it to justify SQL/Gateway/Shell/Auth/Menu duplication in Financiero.

## Interpreting exit codes

| Exit code | Meaning | Action |
|---|---|---|
| `0` | `PASS_E2E_REAL` | Capture sanitized evidence and open closure path. |
| `1` | `FAIL` | Fix proven Financiero defect or document out-of-scope failure. |
| `2` | `BLOCKED_DEPENDENCY` | Do not productize; request external remediation/evidence. |

## If SQL TCP is closed

- Do not add SQL Server to Financiero.
- Start the shared SQL runtime owned by Portal/Infra.
- Confirm port `21433` is exposed to host.
- Confirm `FinancieroDb` exists as a logical DB in shared SQL.
- Re-run preflight.

## If Gateway health returns 404

- Do not create a Gateway in Financiero.
- Ask Portal Gateway owner to confirm the route and port.
- If `/health` is not the route, document owner-approved alternative and pass it to the preflight.

## If Shell/Context evidence is missing

- Do not create a Portal Shell clone in Financiero.
- Require live Portal Shell URL, PortalShellContext, menu, permissions and correlation evidence from Portal owners.

## If Financiero API fails

- Check `docker compose ps`.
- Check container logs locally without committing them.
- Run `dotnet build` and `dotnet test`.
- Fix only a proven Financiero defect; do not add platform duplicates.

## Do not do

- Do not create SQL Server propio.
- Do not create Gateway propio.
- Do not create Shell propio.
- Do not create login/Auth/Identity propio.
- Do not use secrets reales.
- Do not store tokens.
- Do not activate SRI Test real or production.
- Do not claim PASS with mocks or partial evidence.
