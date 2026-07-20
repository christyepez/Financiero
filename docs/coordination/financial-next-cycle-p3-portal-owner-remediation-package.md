# Next Cycle P3 - Portal Owner Remediation Package

Date: 2026-07-20
Owner target: Portal Gateway / Portal Shell / Portal Contract / Security-Menu / Observability
Current state: `BLOCKED_DEPENDENCY`

## Portal block summary

Financiero can start and expose local health/readiness endpoints, but real Portal-integrated E2E remains blocked because Portal runtime evidence is incomplete.

## Evidence from P2/P3

- Portal Gateway `/health` at `http://localhost:8082/health` returns HTTP 404.
- Portal Shell live health evidence is missing.
- PortalShellContext live evidence is missing.
- Menu/permissions live evidence is missing.
- CorrelationId live evidence across Portal-to-Financiero is missing.
- Financiero API `/health` and `/api/financial/portal-integration/readiness` return HTTP 200 after Docker activation.

## Required Portal owner actions

1. Confirm or fix the Portal Gateway health route.
2. Provide a live Portal Shell URL and health route.
3. Provide live PortalShellContext evidence for Financiero.
4. Provide live Menu/permissions evidence using Portal-owned Security/Menu capabilities.
5. Provide correlationId propagation evidence from Portal Gateway/Shell to Financiero API.
6. Provide sanitized evidence with date, owner and source.

## Expected endpoints and health routes

| Component | Expected endpoint |
|---|---|
| Portal Gateway | `http://localhost:8082/health` or owner-confirmed official health route |
| Portal Shell | owner-confirmed Shell base URL + `/health` or official health route |
| PortalShellContext | live context endpoint or sanitized runtime capture |
| Financiero readiness | `http://localhost:8083/api/financial/portal-integration/readiness` |

If Portal uses a non-standard health path, the owner must document the route and the preflight must be run with the owner-approved path.

## Accepted evidence

- Portal Gateway health HTTP 200 or official alternative route confirmed and passing.
- Portal Shell health HTTP 200 or official alternative route confirmed and passing.
- PortalShellContext live evidence for Financiero.
- Menu/permissions live evidence for expected Financiero resources.
- CorrelationId visible in sanitized request/response evidence.
- Evidence must not include tokens, passwords, user personal data, private URLs, production secrets or raw authorization headers.

## Validation commands

```powershell
Invoke-WebRequest -UseBasicParsing http://localhost:8082/health
Invoke-WebRequest -UseBasicParsing <PORTAL_SHELL_BASE_URL>/health
Invoke-WebRequest -UseBasicParsing http://localhost:8083/api/financial/portal-integration/readiness -Headers @{ "X-Dev-Permissions" = "financial.electronicdocuments.read"; "X-Correlation-ID" = "synthetic-e2e-correlation" }
pwsh -NoProfile -ExecutionPolicy Bypass -File tools/validate-portal-financiero-e2e.ps1 -OutputMarkdown -VerboseDiagnostics -SuggestFixes -PortalGatewayHealthPath /health -PortalShellBaseUrl <PORTAL_SHELL_BASE_URL> -PortalShellHealthPath /health -FinancialApiHealthPath /health -EvidenceOutputPath <TEMP_PATH_OUTSIDE_REPO> -AcceptanceGateReport <TEMP_PATH_OUTSIDE_REPO>
```

## Acceptance criteria

- Gateway gate passes.
- Shell gate passes.
- PortalShellContext/Menu/permissions/correlation evidence is accepted by QA.
- Preflight can return `SCRIPT_EXIT=0` only after SQL, Portal and Financiero gates all pass.

## Suggested SLA

- Critical acknowledgement: same business day.
- Health route confirmation: same business day.
- Runtime evidence delivery: 1 business day.
- Full Portal contract evidence: 2 business days.

## Risks if unresolved

- PASS E2E real remains NOT_READY.
- Productization remains blocked.
- UI/runtime integration may drift from Portal-owned capabilities.
- Teams may feel pressure to duplicate Gateway, Shell, Auth or Menu inside Financiero, which is not allowed.

## What Financiero must not do

- Do not create Gateway propio.
- Do not create Portal Shell propio.
- Do not create Auth/Menu/permissions propios.
- Do not store tokens or user identities.
- Do not claim Portal PASS from static frontend checks alone.
