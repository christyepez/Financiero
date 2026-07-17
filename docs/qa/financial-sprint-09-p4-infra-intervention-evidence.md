# Financial Sprint 9 P4 Infra Intervention Evidence

## Scope

Capture PASS or actionable `BLOCKED_DEPENDENCY` evidence for external SQL and Portal runtime intervention. This package does not modify Financiero runtime ownership.

## Base

- GitHub main source: `72278b6862d2a67580b63b1f052e85a1fe9cd5d0`.
- Branch: `financiero-sprint-9-p4-external-infra-intervention`.
- Execution date: `2026-07-17`.

## Preflight command

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File tools/validate-portal-financiero-e2e.ps1 -OutputMarkdown -VerboseDiagnostics -SuggestFixes -PortalGatewayHealthPath /health -PortalShellHealthPath /health -FinancialApiHealthPath /health
```

## Parameters used

- SQL host: `host.docker.internal`.
- SQL port: `21433`.
- Portal Gateway health path: `/health`.
- Portal Shell health path: `/health`.
- Financial API health path: `/health`.

## Current dependency result

| Area | Result | Evidence |
|---|---|---|
| Shared SQL | `BLOCKED_DEPENDENCY` | `host.docker.internal` resolves; TCP `21433` remains closed until Infra starts/exposes shared SQL. |
| Portal Gateway | `BLOCKED_DEPENDENCY` | `/health` previously returned HTTP 404; route must be confirmed or overridden. |
| Portal Shell | `BLOCKED_DEPENDENCY` | Live shell URL and PortalShellContext evidence pending from Portal owner. |
| Financial API | `BLOCKED_DEPENDENCY` | Runtime PASS must not be claimed while shared SQL is unavailable. |

## Health routes tested

- Financial API: `/health`.
- Portal Gateway: `/health`.
- Portal Shell: `/health` when `PortalShellBaseUrl` is provided.

## Final result

`BLOCKED_DEPENDENCY`.

This is intentionally not converted to PASS. External SQL and Portal runtime owners must return sanitized evidence before final E2E PASS.

## Validation summary

| Check | Result | Notes |
|---|---|---|
| `git diff --check` | PASS | Line-ending warnings only. |
| `dotnet restore Financiero.sln` | PASS | Projects already restored. |
| `dotnet build Financiero.sln --no-restore` | PASS | Build succeeded; transient copy warnings observed because tests were running in parallel. |
| `DOTNET_ROLL_FORWARD=Major dotnet test Financiero.sln --no-build` | PASS | 322 tests passed. |
| `pnpm install --frozen-lockfile` | PASS | Already up to date. |
| `pnpm run build` | BLOCKED_BY_LOCAL_PATH | Shell did not resolve `node`; direct bundled Node Angular build passed. |
| Direct Angular build with bundled Node | PASS | Build output generated locally and not committed. |
| `pnpm test` | BLOCKED_BY_LOCAL_PATH | Shell did not resolve `node`; direct bundled Node verifier passed. |
| `node tools/verify-frontend.mjs` with bundled Node | PASS | Frontend foundation checks passed. |
| `node tools/verify-portal-e2e-contract.mjs` with bundled Node | PASS | Portal E2E contract checks passed. |
| `docker compose config` | PASS | Compose remains valid and uses shared SQL host. |
| SQL Server own-container scan | PASS | Only defensive references and docs; no Financiero SQL Server service. |
| Sensitive pattern scan | PASS | No new secrets, certificates, XML reales, token storage, upload/download or notification send added. |

## Pending owner actions

- Infra: restore shared SQL on `host.docker.internal:21433`.
- Portal Gateway: confirm health route or expose `/health` with HTTP 2xx.
- Portal Shell: provide URL, health route and PortalShellContext evidence.

## Sanitization

No secrets, passwords, tokens, connection strings, certificates, real XML, taxpayer data or SRI responses are included.

Control tokens: BLOCKED_DEPENDENCY; Portal Gateway; shared SQL; not production-ready; No SRI Production; No official ATS; No legal-final RIDE; No productive XAdES.
