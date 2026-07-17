# Sprint 9 P2 - Dependency Diagnostic Evidence

## Scope

Diagnose the real E2E blockers for Financiero without changing ownership boundaries. Financiero must keep using shared SQL and PortalCorporativo Gateway/Shell; it must not create SQL Server, Gateway, Shell, login, storage or notification substitutes.

## Base

- Base main commit: `b603f39d80fe47bee9fb3c1a6eb456b4afb80cf5`.
- Branch: `financiero-sprint-9-p2-fix-real-e2e-dependencies`.
- Data policy: sanitized textual evidence only.

## SQL dependency diagnostic

| Check | Result | Evidence | Interpretation |
|---|---|---|---|
| DNS `host.docker.internal` | PASS | resolves to `172.20.20.141` | Host alias is available. |
| TCP `host.docker.internal:21433` | BLOCKED_DEPENDENCY | `HOST_RESOLVES_BUT_PORT_CLOSED` | Shared SQL is not reachable on expected port. |
| Financiero SQL container | PASS | compose has no SQL Server service | Architecture remains aligned. |
| Docker compose config | PASS | Financiero compose is valid | No compose syntax blocker. |

Possible causes:

- Shared SQL runtime is stopped.
- Shared SQL uses a different host port.
- Local firewall blocks port `21433`.
- SQL is exposed only inside another Docker network.
- Credentials/configuration are local-only and not available to this validation.

Recommended actions:

1. Start the shared SQL runtime owned by Portal/shared infrastructure.
2. Confirm host port mapping to `21433`, or pass an override with `-SqlHost` and `-SqlPort`.
3. Confirm `FinancieroDb` is a logical database inside the shared SQL instance.
4. Do not add SQL Server to Financiero compose.

## Portal runtime diagnostic

| Check | Result | Evidence | Interpretation |
|---|---|---|---|
| Portal Gateway `localhost:8082/health` | BLOCKED_DEPENDENCY | `HTTP_ENDPOINT_UNREACHABLE` | Gateway appears stopped or on another port. |
| Portal Shell | BLOCKED_DEPENDENCY | no Shell URL available in this run | Shell live evidence cannot be captured. |
| Financiero API `localhost:8083` | BLOCKED_DEPENDENCY | `HTTP_ENDPOINT_UNREACHABLE` | API not running because dependencies are unavailable. |

Possible causes:

- PortalCorporativo is not started.
- Gateway or Shell ports differ from expected values.
- Portal Shell is outside the local machine/browser scope.
- CORS/context validation remains pending until Portal is up.
- Portal runtime is out of scope for the Financiero repository.

Recommended actions:

1. Start PortalCorporativo Gateway and Shell from the Portal repository.
2. Validate Gateway health with the configured `PortalHealthPath`.
3. Pass overrides if ports differ:
   - `-PortalBaseUrl http://localhost:<gateway-port>`
   - `-PortalShellBaseUrl http://localhost:<shell-port>`
4. Validate `PortalShellContext` with `source=portal`, menu, permissions, feature flags and correlation id.

## Preflight command

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -Command "& .\tools\validate-portal-financiero-e2e.ps1 -OutputMarkdown -VerboseDiagnostics -SuggestFixes; Write-Output ('SCRIPT_EXIT=' + $LASTEXITCODE)"
```

Observed result: `SCRIPT_EXIT=2`.

Final classification: `BLOCKED_DEPENDENCY`.

## Validation commands executed

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

## No-production guardrails

No SRI Production; No official ATS; No legal-final RIDE; No productive XAdES.

Financiero remains not production-ready. No secrets, certificates, XML, SRI real responses, taxpayer data, token storage, upload/download or notification send were added.
