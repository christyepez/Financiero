# Sprint 7 QA Evidence

## Reported validation commands

- `git remote -v`
- `git checkout main`
- `git fetch origin`
- `git pull origin main`
- `git rev-parse HEAD`
- `git status`
- `git checkout -b financiero-sprint-7-p5-closure-qa-roadmap`
- `git diff --check`
- `dotnet restore Financiero.sln`
- `dotnet build Financiero.sln --no-restore`
- `DOTNET_ROLL_FORWARD=Major dotnet test Financiero.sln --no-build`
- `pnpm install --frozen-lockfile`
- `pnpm run build`
- `pnpm test`
- `docker compose config`

## P1-P4 validation baseline

Control tokens: No SRI Production; No official ATS; No legal-final RIDE; No productive XAdES.

| Area | Evidence | Result |
|---|---|---|
| Backend | Restore/build/tests reported through Sprint 7 packages. | Passing before P5 closure. |
| Frontend | Angular build and verifier reported through Sprint 7 packages. | Passing before P5 closure. |
| Docker | Compose configuration valid. | Runtime depends on shared SQL Server. |
| SQL | Financiero uses `FinancieroDb` on shared SQL Server. | No SQL container in Financiero. |
| Security | Verifiers reject token storage, certs and dangerous UI controls. | Foundation-safe. |

## Health checks expected

- `/health`
- `/health/live`
- `/health/ready`
- `/health/sri`
- `/health/content-file`

`/health/ready` and API runtime checks require the shared Portal SQL Server to be available. If `host.docker.internal,21433` is unavailable, runtime health is blocked by infrastructure, not by a Financiero SQL container.

## Negative evidence checklist

- [x] No SQL Server propio in Financiero Docker Compose.
- [x] No login propio.
- [x] No token storage in frontend source.
- [x] No committed certificate/key extensions.
- [x] No real XML or base64 evidence committed.
- [x] No SRI Production activation.
- [x] No official ATS.
- [x] No legal-final RIDE.
- [x] No real upload control.
- [x] No real notification send control.

## Recommended smoke tests

- Validate Portal Shell context with `source=portal` and contract `1.0`.
- List external approval requests and integration readiness.
- Query purchase and voided document productization readiness.
- Validate health endpoints with shared SQL running.
- Confirm dangerous flags remain false in production build.
