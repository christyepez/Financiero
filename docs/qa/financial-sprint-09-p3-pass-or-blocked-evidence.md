# Sprint 9 P3 - PASS or BLOCKED Evidence

## Scope

Attempt to capture real PASS evidence for Financiero E2E infrastructure. If dependencies remain unavailable or misrouted, capture actionable `BLOCKED_DEPENDENCY` evidence without creating infrastructure inside Financiero.

## Base

- Base main commit: `0705062e8df2663242a7ce8266104b727bccfa72`.
- Branch: `financiero-sprint-9-p3-external-infra-pass-evidence`.
- Execution date: 2026-07-17.
- Data policy: sanitized textual evidence only.

## Commands executed

```powershell
git remote -v
git checkout main
git fetch origin
git pull origin main
git rev-parse HEAD
git status --short
git checkout -b financiero-sprint-9-p3-external-infra-pass-evidence
pwsh -NoProfile -ExecutionPolicy Bypass -Command "& .\tools\validate-portal-financiero-e2e.ps1 -OutputMarkdown -VerboseDiagnostics -SuggestFixes; Write-Output ('SCRIPT_EXIT=' + $LASTEXITCODE)"
docker compose config
dotnet restore Financiero.sln
dotnet build Financiero.sln --no-restore
$env:DOTNET_ROLL_FORWARD='Major'; dotnet test Financiero.sln --no-build
pnpm install --frozen-lockfile
node node_modules/@angular/cli/bin/ng build
node tools/verify-frontend.mjs
node tools/verify-portal-e2e-contract.mjs
```

Validation summary:

- `git diff --check`: pass with LF/CRLF warnings only.
- `dotnet restore Financiero.sln`: pass.
- `dotnet build Financiero.sln --no-restore`: pass.
- `DOTNET_ROLL_FORWARD=Major dotnet test Financiero.sln --no-build`: 322 tests passed.
- `pnpm install --frozen-lockfile`: already up to date.
- Angular build via bundled Node: pass.
- `node tools/verify-frontend.mjs`: pass.
- `node tools/verify-portal-e2e-contract.mjs`: pass.
- `docker compose config`: pass.
- SQL container scan: only defensive script/test/docs references; no Financiero SQL Server container.
- Sensitive pattern scan: only defensive docs/checks and existing XML generator; no new secrets or certificates.

## Preflight result

Final preflight result: `BLOCKED_DEPENDENCY`.

Observed script exit: `SCRIPT_EXIT=2`.

| Check | Status | Code | Evidence | Corrective action |
|---|---|---|---|---|
| PowerShell | PASS | PASS | 7.6.3 | None. |
| Shared SQL host resolution | PASS | PASS | `host.docker.internal -> 172.20.20.141` | None. |
| Docker compose config | PASS | PASS | Financiero compose is valid. | None. |
| Shared SQL TCP | BLOCKED_DEPENDENCY | HOST_RESOLVES_BUT_PORT_CLOSED | `host.docker.internal:21433` | Start shared SQL, validate port mapping `21433`, firewall and host exposure. |
| No Financiero SQL container | PASS | PASS | Financiero does not define SQL Server. | None; keep this architecture. |
| Financiero API health | BLOCKED_DEPENDENCY | HTTP_ENDPOINT_UNREACHABLE | `localhost:8083` refused connection | Start API only after shared SQL is reachable. |
| Portal Gateway health | BLOCKED_DEPENDENCY | HTTP_STATUS_UNEXPECTED | `HTTP 404` for `/health` at expected Gateway base | Validate Gateway health path, route or port. |
| Portal Shell | BLOCKED_DEPENDENCY | Not available | No Shell URL validated in this run | Start Portal Shell and provide URL. |

## SQL status

- Host: `host.docker.internal`.
- Port: `21433`.
- Diagnosis: `HOST_RESOLVES_BUT_PORT_CLOSED`.
- Result: `BLOCKED_DEPENDENCY`.

Action required by Portal/Infra:

1. Start the shared SQL runtime.
2. Expose SQL on host port `21433` or provide the correct override.
3. Confirm logical database `FinancieroDb`.
4. Return sanitized TCP evidence.

## Portal Gateway status

- URL: `http://localhost:8082/health`.
- Diagnosis: `HTTP_STATUS_UNEXPECTED`.
- Result: `BLOCKED_DEPENDENCY`.

Action required by Portal/Infra:

1. Confirm whether `8082` is the Gateway port.
2. Confirm the correct health path.
3. Return HTTP 2xx evidence or updated URL/path.

## Portal Shell status

- URL: not confirmed.
- Diagnosis: Shell live evidence unavailable.
- Result: `BLOCKED_DEPENDENCY`.

Action required by Portal/Infra:

1. Start Portal Shell.
2. Provide Shell URL.
3. Validate `PortalShellContext` with `source=portal`, menu, permissions, feature flags and correlation id.

## Financiero API status

- Compose config: PASS.
- Container/API status: not started because shared SQL is unavailable.
- Health endpoints: not reachable while API is not running.

Starting Financiero without SQL would not produce PASS; it would produce dependency failures.

## Final result

`BLOCKED_DEPENDENCY`.

This is not a Financiero application failure. PASS requires shared SQL TCP, Portal Gateway health, Portal Shell runtime, Financiero API health/readiness and Portal integration readiness to succeed.

## No-production guardrails

No SRI Production; No official ATS; No legal-final RIDE; No productive XAdES.

Financiero remains not production-ready. No secrets, certificates, real XML, SRI responses, personal data, token storage, upload/download or notification send were added.
