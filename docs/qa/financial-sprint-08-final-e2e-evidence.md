# Sprint 8 Final E2E Evidence

## Final result

Result: `BLOCKED_DEPENDENCY`.

This is not a Financiero application failure. Complete PASS requires shared SQL and PortalCorporativo Gateway/Shell to be running.

## Evidence summary

| Area | Result | Evidence |
|---|---|---|
| Repository sync | PASS | GitHub main contained Sprint 8 P4 commit `0f71d7ed8381f5c2953822dbe1f2697e88cecce9`. |
| Backend build/tests | PASS | Restore/build pass; test suite passes with `DOTNET_ROLL_FORWARD=Major`. |
| Frontend build/verifiers | PASS | Angular build, `verify-frontend.mjs` and `verify-portal-e2e-contract.mjs` pass. |
| Docker compose | PASS | `docker compose config` validates. |
| SQL común | BLOCKED_DEPENDENCY | `host.docker.internal:21433` unavailable in local validation. |
| Portal Gateway | BLOCKED_DEPENDENCY | `localhost:8082` unavailable in local validation. |
| Portal Shell | BLOCKED_DEPENDENCY | No live Portal Shell evidence captured. |
| Financiero API health | BLOCKED_DEPENDENCY | API cannot become healthy without shared SQL. |
| Readiness endpoint | BLOCKED_DEPENDENCY | Endpoint readiness depends on API runtime and Portal dependencies. |
| Financiero Angular | PASS static / PARTIAL runtime | Build and static verifiers pass; live Portal-integrated runtime requires Portal Shell. |
| External Approval UX | PASS static | P4 UX/verifiers confirm foundation-only warnings and no dangerous controls. |
| Feature flags | PASS | Productive flags remain disabled by default. |
| Security | PASS static | No token storage, login propio, upload/download, notification send or production tax activation added. |

## Preflight interpretation

The expected command is:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File tools/validate-portal-financiero-e2e.ps1 -OutputMarkdown
```

Exit code `2` is acceptable when SQL or Portal are unavailable and means `BLOCKED_DEPENDENCY`. Exit code `1` is reserved for real repository/script/application failure.

## Observed P5 validation

- `git diff --check`: pass with LF/CRLF warnings only.
- `dotnet restore Financiero.sln`: pass.
- `dotnet build Financiero.sln --no-restore`: pass.
- `DOTNET_ROLL_FORWARD=Major dotnet test Financiero.sln --no-build`: 322 tests passed.
- `pnpm install --frozen-lockfile`: already up to date.
- Angular build through bundled Node: pass.
- `node tools/verify-frontend.mjs`: pass.
- `node tools/verify-portal-e2e-contract.mjs`: pass.
- `docker compose config`: pass.
- `tools/validate-portal-financiero-e2e.ps1 -OutputMarkdown`: `SCRIPT_EXIT=2`, `BLOCKED_DEPENDENCY`.
- SQL container scan: only defensive checks/docs/tests; no Financiero SQL Server container definition.
- Sensitive pattern scan: only defensive checks/docs and existing XML generator; no new secret, certificate, token storage, upload/download or notification send behavior.

## Final classification

`BLOCKED_DEPENDENCY` until all are true:

- Shared SQL is reachable.
- Portal Gateway health is reachable.
- Portal Shell context is available.
- Financiero API health/readiness is reachable.
- Preflight returns PASS.

No health PASS, Portal PASS or production readiness is invented in this evidence.

Control tokens: No SRI Production; No official ATS; No legal-final RIDE; No productive XAdES. Financiero remains not production-ready.
