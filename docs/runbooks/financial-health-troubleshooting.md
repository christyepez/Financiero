# Financial Health Troubleshooting

## Health states

| State | Meaning |
|---|---|
| App build OK | Code compiles locally. |
| Container start OK | Image builds and container process starts. |
| Dependency unavailable | External dependency such as shared SQL or Portal is unreachable. |
| App failure | Application throws without an external dependency explanation. |
| Readiness failure | App is running but not ready because required checks fail. |

Do not convert dependency failures into false-positive health.

Sprint 8 P5 final evidence uses `BLOCKED_DEPENDENCY` when shared SQL or Portal Gateway/Shell are unavailable. This is the correct closure state and must not be hidden as PASS.

## Endpoints

- `/health`: overall health.
- `/health/live`: process liveness.
- `/health/ready`: readiness, including DB dependency.
- `/health/sri`: SRI mock/test health.
- `/health/content-file`: Portal Content/File boundary health.
- `/api/financial/portal-integration/readiness`: read-only Portal E2E readiness.

## Common cases

### SQL TCP unavailable

Run:

```powershell
Test-NetConnection host.docker.internal -Port 21433
```

If blocked, fix shared SQL before diagnosing Financiero code.

Sprint 9 P1 note: if SQL TCP is blocked, do not start adding fallback databases or a Financiero SQL container. Record `BLOCKED_DEPENDENCY` and start the shared SQL runtime.

### `host.docker.internal` does not resolve

Validate DNS resolution and Docker Desktop networking. Record only host/port status, no credentials.

### Portal Gateway is off

Run:

```powershell
tools/validate-portal-financiero-e2e.ps1 -SkipApiHealthChecks -OutputMarkdown
```

If Portal checks are blocked, start PortalCorporativo Gateway/Shell.

Expected PASS evidence includes HTTP 2xx from Portal Gateway health and a Portal Shell context source of `portal`. Expected BLOCKED_DEPENDENCY evidence includes connection refused or timeout for Gateway/Shell URLs.

### Sprint 9 P2 diagnostic codes

- `HOST_NOT_RESOLVED`: fix host alias or override host parameter.
- `HOST_RESOLVES_BUT_PORT_CLOSED`: start service or fix port/firewall.
- `HTTP_ENDPOINT_UNREACHABLE`: start the HTTP service or override URL.
- `HTTP_STATUS_UNEXPECTED`: inspect service health path and dependencies.
- `SERVICE_SKIPPED`: rerun without skip flags for full E2E evidence.

Sprint 9 P3 observed `HTTP_STATUS_UNEXPECTED` for Portal Gateway `/health` with HTTP 404. This means something responded, but not as the expected health endpoint. Confirm route/path/port with Portal owner.

### Missing permissions

For local development only, use `X-Dev-Permissions` with synthetic permissions. Production must receive delegated permissions from Portal.

### Dangerous feature flags

Keep these false:

- `allowProductiveActivation`
- `allowOfficialTaxFlows`
- `allowSriSubmission`
- `allowAtsOfficialActions`
- `allowEvidenceUpload`
- `allowNotificationSend`

### CORS or delegated context

Validate PortalShellContext contract version, source, permissions, menu and correlation id. Do not store tokens in local/session storage or querystrings.

## Evidence to capture

- Command executed.
- PASS / BLOCKED_DEPENDENCY / FAIL.
- Sanitized URL host/port.
- Timestamp.
- Short blocker.

Never capture secrets, connection strings, tokens, certificates, XML or real taxpayer data.
# Sprint 9 P4 health route configuration

Preflight now accepts `-PortalGatewayHealthPath`, `-PortalShellHealthPath` and `-FinancialApiHealthPath`. HTTP 404 on a health route is classified as `HTTP_STATUS_UNEXPECTED`; owner must confirm whether `/health`, `/health/live`, `/health/ready`, `/api/health` or another documented route is correct.

Control tokens: BLOCKED_DEPENDENCY; Portal Gateway; shared SQL; not production-ready; No SRI Production; No official ATS; No legal-final RIDE; No productive XAdES.
