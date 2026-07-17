# Portal E2E Validation Checklist

## Preconditions

- GitHub `main` is the source of truth.
- PortalCorporativo and Financiero are on reviewed branches or `main`.
- Shared SQL Server is running once per local environment.
- No real secrets, certificates, taxpayer XML, SRI responses or personal data are used.

## Required services

| Service | Expected local role | Notes |
|---|---|---|
| Shared SQL Server | Database host | One container only; Financiero uses logical database `FinancieroDb`. |
| PortalCorporativo Gateway/Shell | Owner of shell, auth, menu and configuration context | Must provide PortalShellContext. |
| Financiero API | Domain API | Runs behind Portal/Gateway or local port for validation. |
| Financiero Angular | Portal consumer UI | Uses Portal context; standalone is development only. |
| Seq | Logs/correlation | Optional but recommended. |
| Redis/MinIO | Portal-owned dependencies if enabled | Do not replace Portal services inside Financiero. |

## Variables expected

Use `.env.example` or local environment variables only. Do not commit `.env`.

- `FINANCIAL_API_PORT`
- `ConnectionStrings__FinancialDb`
- `Portal__GatewayBaseUrl`
- `Jwt__Issuer`
- `Jwt__Audience`
- `Jwt__Secret` only from secret store or local untracked env.

## URLs expected

- Portal Gateway/Shell: local Portal URL.
- Financiero API: `http://localhost:8083`.
- Financiero Angular: local Angular dev/build URL.
- Seq: local Seq URL if enabled.

No URL may contain tokens, passwords or private querystrings.

## PortalShellContext contract

Required fields:

- `contractVersion=1.0`.
- `source=portal` for real E2E.
- user and tenant metadata.
- permissions array.
- menu array.
- featureFlags.
- capabilities.
- correlationId.
- issuedAt/expiresAt.

## Minimum permissions

- `financial.electronicdocuments.read`.
- `financial.electronicdocuments.manage` only for foundation commands when explicitly testing gated commands.

Financiero must not invent or persist permissions.

## Minimum menu

- `/dashboard`
- `/sri-readiness`
- `/ats-readiness`
- `/external-approvals`
- `/tax-catalogs`
- `/purchases`
- `/voided-documents`

Routes must pass the allow-list and permission checks.

## Feature flags

Allowed read-only:

- `allowProductizationReadiness=true`.

Must remain false by default:

- `allowProductiveActivation`
- `allowOfficialTaxFlows`
- `allowSriSubmission`
- `allowAtsOfficialActions`
- `allowEvidenceUpload`
- `allowNotificationSend`
- mutation flags unless testing explicit foundation commands.

## Correlation id

- Portal sends correlation id.
- Financiero forwards/returns correlation id.
- Logs should include correlation id without secrets.

## Pass/fail criteria

Pass:

- Portal context is accepted.
- Standalone is blocked in production.
- Menu and permissions are filtered.
- Readiness endpoint returns blockers and no secrets.
- No production tax flow is enabled.
- External approval UX states clearly that `ApprovedFoundation` does not enable production.
- Evidence references are shown as Portal-owned metadata-only.
- Notification intents are shown as foundation/no-send.

Fail:

- Token appears in storage/querystring/logs.
- Unknown menu route is rendered.
- Productive flags are true.
- Financiero creates Security/Menu/Configuration/Notification/Content/File substitutes.
- External approval UX renders upload/download, notification send, productive approval or legal-final tax copy.

## Troubleshooting

- If health fails, verify shared SQL Server and `FinancieroDb`.
- If menu is empty, verify Portal permissions and route allow-list.
- If auth fails, verify delegated token is supplied by Portal and not by local storage.
- If readiness is blocked, review missing Portal capabilities and SQL runtime evidence.
- Use `tools/validate-portal-financiero-e2e.ps1` for a sanitized local pass/block/fail check.
