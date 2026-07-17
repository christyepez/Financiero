# Portal + Financiero Local E2E Runbook

## Goal

Validate PortalCorporativo and Financiero together with synthetic data and no production tax activation.

## 1. Start shared dependencies

- Start the single shared SQL Server for the environment.
- Confirm logical databases remain separate: `PortalCorporativoDb`, `FinancieroDb`.
- Start Seq if local log inspection is needed.

Do not start a Financiero-owned SQL Server.

Validate SQL before starting the API:

```powershell
Test-NetConnection host.docker.internal -Port 21433
docker compose config
```

If SQL is blocked, stop and follow `docs/runbooks/shared-sql-runtime-validation.md`.

## 2. Start PortalCorporativo

- Start Portal API/Gateway/Shell from its own repository.
- Confirm Portal owns Security/Auth, Menu, Configuration, Audit, Outbox, Content/File and Notification.
- Confirm Gateway URL is available locally.
- Confirm Portal Shell emits `PortalShellContext` with `contractVersion=1.0`, `source=portal`, permissions, menu, feature flags and correlation id.
- Confirm Gateway health returns HTTP 2xx before starting E2E evidence capture.

## 3. Start Financiero API

```powershell
dotnet restore Financiero.sln
dotnet build Financiero.sln
docker compose up -d --build financial-api
```

Validate:

- `GET /health`
- `GET /health/live`
- `GET /health/ready`
- `GET /health/sri`
- `GET /health/content-file`
- `GET /api/financial/portal-integration/readiness`

Or run the non-invasive script:

```powershell
tools/validate-portal-financiero-e2e.ps1
```

Useful variants:

```powershell
tools/validate-portal-financiero-e2e.ps1 -OutputMarkdown
tools/validate-portal-financiero-e2e.ps1 -SkipPortalChecks -OutputMarkdown
tools/validate-portal-financiero-e2e.ps1 -SkipApiHealthChecks -PortalShellBaseUrl http://localhost:4201 -OutputMarkdown
tools/validate-portal-financiero-e2e.ps1 -OutputMarkdown -VerboseDiagnostics -SuggestFixes
```

Exit codes: `0` PASS, `2` BLOCKED_DEPENDENCY, `1` FAIL.

Use development headers only in local development and never in production.

Sprint 8 P5 closure rule: do not mark E2E as PASS unless shared SQL, Portal Gateway/Shell, Financiero API health/readiness and Portal-integrated Angular evidence are all available. If SQL or Portal are unavailable, record `BLOCKED_DEPENDENCY`.

## 4. Start Financiero Angular

```powershell
cd frontend/financiero-web
pnpm install --frozen-lockfile
pnpm run build
pnpm test
node tools/verify-portal-e2e-contract.mjs
```

Validate `/external-approvals` with synthetic data only:

- `ApprovedFoundation no habilita producción` is visible.
- Evidence is described as Portal-owned metadata-only.
- Notification intent is described as prepared only/no-send.
- No upload/download control appears.
- No notification send/resend control appears.
- No productive approval, SRI real, official ATS, legal-final RIDE or productive XAdES control appears.

## 5. Inject synthetic Portal context

For development-only validation, provide a synthetic `window.__PORTAL_SHELL_CONTEXT__` with:

- `contractVersion: "1.0"`.
- `source: "portal"`.
- synthetic user display name.
- synthetic tenant display name.
- permissions needed for read-only screens.
- allowed menu routes.
- safe feature flags.

Do not include tokens, passwords, real emails, real taxpayer data, XML or certificates.

## 6. Validate menu and permissions

- Confirm routes outside the allow-list are not rendered.
- Confirm missing permission hides or blocks routes.
- Confirm `financial.electronicdocuments.read` is enough for readiness screens.
- Confirm `financial.electronicdocuments.manage` is only used for explicitly gated foundation commands.
- Confirm no permissions are invented or persisted by Financiero.

## 7. Validate correlation

- Send a synthetic correlation id from Portal/Gateway.
- Confirm Financiero responses include correlation id.
- Confirm logs include correlation id without sensitive payloads.

## 7.1 PASS/BLOCKED_DEPENDENCY evidence

PASS requires:

- Shared SQL TCP PASS.
- Portal Gateway health PASS.
- Portal Shell context PASS.
- Financiero health/live/ready PASS.
- `/api/financial/portal-integration/readiness` reachable and sanitized.

BLOCKED_DEPENDENCY applies when SQL, Gateway, Shell or API runtime is unavailable. Do not mark PASS from static build alone.

Sprint 9 P3 adds `docs/runbooks/financial-e2e-pass-checklist.md` and `docs/runbooks/financial-e2e-dependency-owner-handoff.md` for the operational handoff required before PASS.

## 8. Validate non-production posture

- SRI Production remains blocked.
- SRI Test real send remains blocked.
- Official ATS remains blocked.
- Legal-final RIDE remains blocked.
- Productive XAdES remains blocked.
- Upload and notification send remain blocked.
- External approval does not replace legal/tax/security approval.

## 9. Shutdown

```powershell
docker compose down
```

Stop Portal services according to the Portal runbook. Do not delete real data.
# Sprint 9 P4 local E2E intervention

Before local PASS, complete SQL and Portal intervention packages, then run preflight with explicit health route parameters. If SQL or Portal remains unavailable, record `BLOCKED_DEPENDENCY` in `docs/qa/financial-sprint-09-p4-infra-intervention-evidence.md`.

Control tokens: Portal Gateway; shared SQL; not production-ready; No SRI Production; No official ATS; No legal-final RIDE; No productive XAdES.
