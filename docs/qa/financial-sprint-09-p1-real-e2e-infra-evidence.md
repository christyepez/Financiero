# Sprint 9 P1 - Real E2E Infrastructure Evidence

## Base

- Repository: Financiero.
- Base main commit: `2f4a6670801e7d39df8eefdc474263a20ba99564`.
- Goal: validate real E2E infrastructure activation for shared SQL, Portal Gateway/Shell and Financiero runtime without creating infrastructure inside Financiero.

## Environment used

- Host OS: local Windows development machine.
- Financial API expected URL: `http://localhost:8083`.
- Portal Gateway expected URL: `http://localhost:8082`.
- Shared SQL expected host/port: `host.docker.internal:21433`.
- Data policy: synthetic/sanitized only.

No real secrets, certificates, taxpayer XML, SRI responses, personal data or private connection strings are recorded.

## Preflight result

Command:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -Command "& .\tools\validate-portal-financiero-e2e.ps1 -OutputMarkdown; Write-Output ('SCRIPT_EXIT=' + $LASTEXITCODE)"
```

Observed classification: `BLOCKED_DEPENDENCY`.

Observed exit code: `2`.

| Check | Status | Sanitized evidence |
|---|---|---|
| PowerShell | PASS | 7.6.3 |
| Shared SQL host resolution | PASS | `host.docker.internal -> 172.20.20.141` |
| Docker compose config | PASS | Financiero compose is valid. |
| Shared SQL TCP | BLOCKED_DEPENDENCY | `host.docker.internal:21433` unavailable. |
| No Financiero SQL container | PASS | Financiero does not define its own SQL Server container. |
| Financiero health | BLOCKED_DEPENDENCY | `localhost:8083` refused connection. |
| Financiero live | BLOCKED_DEPENDENCY | `localhost:8083` refused connection. |
| Financiero ready | BLOCKED_DEPENDENCY | `localhost:8083` refused connection. |
| Financiero SRI health | BLOCKED_DEPENDENCY | `localhost:8083` refused connection. |
| Financiero Content/File health | BLOCKED_DEPENDENCY | `localhost:8083` refused connection. |
| Financiero Portal readiness | BLOCKED_DEPENDENCY | `localhost:8083` refused connection. |
| Portal Gateway health | BLOCKED_DEPENDENCY | `localhost:8082` refused connection. |

## Verification commands executed

- `git remote -v`: origin points to `https://github.com/christyepez/Financiero.git`.
- `git checkout main`, `git fetch origin`, `git pull origin main`: main synchronized.
- `git rev-parse HEAD`: `2f4a6670801e7d39df8eefdc474263a20ba99564`.
- `git diff --check`: pass with LF/CRLF warnings only.
- `dotnet restore Financiero.sln`: pass.
- `dotnet build Financiero.sln --no-restore`: pass.
- `DOTNET_ROLL_FORWARD=Major dotnet test Financiero.sln --no-build`: 322 tests passed.
- `pnpm install --frozen-lockfile`: already up to date.
- Angular build via bundled Node: pass.
- `node tools/verify-frontend.mjs`: pass.
- `node tools/verify-portal-e2e-contract.mjs`: pass.
- `docker compose config`: pass.
- `tools/validate-portal-financiero-e2e.ps1 -OutputMarkdown`: `SCRIPT_EXIT=2`.

## SQL común

Result: `BLOCKED_DEPENDENCY`.

- Host resolution works.
- TCP connection to `host.docker.internal:21433` fails.
- `docker compose config` confirms Financiero still points to shared SQL and does not define SQL Server.
- `FinancieroDb` cannot be validated from the app while the shared SQL runtime is unavailable.

Required to reach PASS:

1. Start the shared SQL Server owned by Portal/shared infrastructure.
2. Confirm port `21433` is reachable from host and containers.
3. Confirm logical database `FinancieroDb` exists or can be initialized by existing migrations.
4. Do not add SQL Server to Financiero compose.

## Portal Gateway

Result: `BLOCKED_DEPENDENCY`.

- `localhost:8082/health` refused connection.
- No live Portal Gateway PASS evidence is captured.

Required to reach PASS:

1. Start PortalCorporativo Gateway.
2. Confirm health endpoint returns HTTP 2xx.
3. Confirm Gateway routes to Financiero without exposing tokens/querystrings.

## Portal Shell

Result: `BLOCKED_DEPENDENCY`.

- No live Portal Shell URL was available in this run.
- No real `PortalShellContext` runtime evidence is captured.

Required to reach PASS:

1. Start Portal Shell.
2. Provide `PortalShellContext` with `contractVersion=1.0`, `source=portal`, user/tenant metadata, permissions, menu, feature flags and correlation id.
3. Validate menu, permissions and feature flags with synthetic context only.

## Financiero API and health

Result: `BLOCKED_DEPENDENCY`.

Endpoints expected:

- `/health`
- `/health/live`
- `/health/ready`
- `/health/sri`
- `/health/content-file`

The API was not started because shared SQL is unavailable. Starting it without SQL would produce dependency failures and not a real PASS.

## Financiero Angular

Result: static PASS / runtime `BLOCKED_DEPENDENCY`.

- Angular build and static verifiers are expected to pass.
- Live Portal-integrated Angular evidence requires Portal Shell.

## Portal integration readiness endpoint

Expected endpoint:

- `/api/financial/portal-integration/readiness`

Result: `BLOCKED_DEPENDENCY` because the API runtime is unavailable while shared SQL is unavailable.

## Security and no-production guardrails

Validated posture:

- No SQL Server propio.
- No login/Auth/Identity propio.
- No token storage.
- No upload/download evidence.
- No notification send.
- No SRI Test real.
- No SRI Production.
- No official ATS.
- No legal-final RIDE.
- No productive XAdES.
- No real certificates/XML/data.

## Final result

Final result: `BLOCKED_DEPENDENCY`.

This is an infrastructure dependency block, not a Financiero application failure. Financiero remains not production-ready. Do not convert it to PASS until shared SQL, Portal Gateway, Portal Shell, Financiero API health/readiness and Portal-integrated Angular evidence are all available.
