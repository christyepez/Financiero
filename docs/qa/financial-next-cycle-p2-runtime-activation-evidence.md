# Next Cycle P2 - Financial Runtime Activation Evidence

Date: 2026-07-20  
Environment: local Docker Desktop + shared Portal/SQL contract  
Final state: `BLOCKED_DEPENDENCY`  
Preflight: `SCRIPT_EXIT=2`

## Commands executed

- `git remote -v`
- `git checkout main`
- `git fetch origin`
- `git pull origin main`
- `git rev-parse HEAD`
- `git status --short`
- `git checkout -b financiero-next-cycle-p2-runtime-activation-e2e-unblock`
- `docker compose config`
- `Test-NetConnection host.docker.internal -Port 21433`
- `Invoke-WebRequest -UseBasicParsing http://localhost:8082/health`
- `Invoke-WebRequest -UseBasicParsing http://localhost:8080/health`
- `Invoke-WebRequest -UseBasicParsing http://localhost:8083/health`
- `pwsh -NoProfile -ExecutionPolicy Bypass -File tools/validate-portal-financiero-e2e.ps1 ...`
- `dotnet restore Financiero.sln`
- `dotnet build Financiero.sln --no-restore`
- `DOTNET_ROLL_FORWARD=Major dotnet test Financiero.sln --no-build`
- `pnpm install --frozen-lockfile`
- `pnpm run build`
- `pnpm test`
- `node tools/verify-portal-e2e-contract.mjs`
- `docker compose up -d --build`

## Runtime results

| Check | Result | Evidence |
|---|---|---|
| SQL Common TCP | `BLOCKED_DEPENDENCY` | `host.docker.internal` resolves to `172.20.20.141`, but TCP `21433` is closed. |
| `FinancieroDb` | `BLOCKED_DEPENDENCY` | Cannot validate logical DB until SQL TCP is open and DBA evidence is provided. |
| Portal Gateway health | `BLOCKED_DEPENDENCY` | `http://localhost:8082/health` returned HTTP 404. |
| Portal Shell health | `BLOCKED_DEPENDENCY` | `PortalShellBaseUrl` was not provided; owner evidence required. |
| PortalShellContext/Menu/permissions/correlation | `BLOCKED_DEPENDENCY` | No live Portal evidence accepted. |
| Financiero API health | `PASS` | After `docker compose up -d --build`, `/health`, `/health/live`, `/health/ready`, `/health/sri`, `/health/content-file`, and `/api/financial/portal-integration/readiness` returned HTTP 200. |
| Angular shell/frontend | `PASS` | `pnpm run build`, `pnpm test`, and `node tools/verify-portal-e2e-contract.mjs` passed with Codex Node runtime on PATH. |
| Docker compose | `PASS` | Compose is valid and defines only `financial-api`; no Financiero SQL Server container. |
| Preflight | `BLOCKED_DEPENDENCY` | Final preflight: PASS=12, BLOCKED_DEPENDENCY=2, FAIL=0, `SCRIPT_EXIT=2`. |

## Root cause

The current blocker is not a demonstrated Financiero code defect. Financiero builds, tests, starts in Docker, and exposes its local health/readiness endpoints. Real E2E remains blocked because the shared SQL TCP endpoint is closed and Portal Gateway/Shell live evidence is not accepted.

## Final classification

`BLOCKED_DEPENDENCY`

## Next technical action

External owners must start/repair shared SQL on `host.docker.internal:21433`, confirm `FinancieroDb`, expose a valid Portal Gateway health endpoint, provide Portal Shell/PortalShellContext/Menu/permissions/correlation evidence, then rerun the preflight until `SCRIPT_EXIT=0`.

No SQL Server propio, Gateway propio, Shell propio, login/Auth propio, token storage, SRI production, official ATS, legal-final RIDE, productive XAdES, real certificates, real XML, or personal data were introduced.
