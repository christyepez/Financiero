# Financial Sprint 5 QA Evidence

## Automated checks

- `git diff --check`.
- `dotnet restore Financiero.sln`.
- `dotnet build Financiero.sln --no-restore`.
- `DOTNET_ROLL_FORWARD=Major dotnet test Financiero.sln --no-build`.
- `docker compose config`.
- `docker compose up -d --build financial-api`.
- Health: `/health`, `/health/live`, `/health/ready`, `/health/sri`, `/health/content-file`.
- `scripts/smoke/financial-sri-smoke.ps1`.

## Evidence constraints

Evidence is synthetic/local. No real SRI responses, real XML, certificates, secrets or personal data are stored.
