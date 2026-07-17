# Portal + Financiero Local E2E Runbook

## Goal

Validate PortalCorporativo and Financiero together with synthetic data and no production tax activation.

## 1. Start shared dependencies

- Start the single shared SQL Server for the environment.
- Confirm logical databases remain separate: `PortalCorporativoDb`, `FinancieroDb`.
- Start Seq if local log inspection is needed.

Do not start a Financiero-owned SQL Server.

## 2. Start PortalCorporativo

- Start Portal API/Gateway/Shell from its own repository.
- Confirm Portal owns Security/Auth, Menu, Configuration, Audit, Outbox, Content/File and Notification.
- Confirm Gateway URL is available locally.

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

Use development headers only in local development and never in production.

## 4. Start Financiero Angular

```powershell
cd frontend/financiero-web
pnpm install --frozen-lockfile
pnpm run build
pnpm test
node tools/verify-portal-e2e-contract.mjs
```

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

## 7. Validate correlation

- Send a synthetic correlation id from Portal/Gateway.
- Confirm Financiero responses include correlation id.
- Confirm logs include correlation id without sensitive payloads.

## 8. Validate non-production posture

- SRI Production remains blocked.
- SRI Test real send remains blocked.
- Official ATS remains blocked.
- Legal-final RIDE remains blocked.
- Productive XAdES remains blocked.
- Upload and notification send remain blocked.

## 9. Shutdown

```powershell
docker compose down
```

Stop Portal services according to the Portal runbook. Do not delete real data.
