# Financial E2E PASS Checklist

Use this checklist to move from `BLOCKED_DEPENDENCY` to real E2E `PASS`.

This checklist requires shared SQL and Portal Gateway/Shell evidence. Financiero remains not production-ready until every item passes.

## External dependencies

- [ ] Shared SQL runtime is active outside Financiero.
- [ ] TCP `host.docker.internal:21433` is open, or an approved override is provided.
- [ ] Logical database `FinancieroDb` is available.
- [ ] Portal Gateway is active.
- [ ] Portal Gateway health returns HTTP 2xx.
- [ ] Portal Shell is active.
- [ ] Portal Shell exposes a real `PortalShellContext`.

## Portal context

- [ ] `contractVersion=1.0`.
- [ ] `source=portal`.
- [ ] User and tenant metadata are synthetic or safe.
- [ ] Permissions include expected `financial.electronicdocuments.read`.
- [ ] Menu routes match Financiero allow-list.
- [ ] Feature flags keep production actions disabled.
- [ ] Correlation id is present.

## Financiero runtime

- [ ] `docker compose config` passes.
- [ ] No Financiero SQL Server container exists.
- [ ] `docker compose up -d --build financial-api` starts after SQL is reachable.
- [ ] `GET /health` returns HTTP 2xx.
- [ ] `GET /health/live` returns HTTP 2xx.
- [ ] `GET /health/ready` returns HTTP 2xx.
- [ ] `GET /health/sri` returns HTTP 2xx.
- [ ] `GET /health/content-file` returns HTTP 2xx.
- [ ] `GET /api/financial/portal-integration/readiness` returns sanitized readiness data.

## Angular shell

- [ ] Financiero Angular builds.
- [ ] Angular is reachable.
- [ ] Portal Shell can navigate to Financiero.
- [ ] No token storage is used.
- [ ] No upload/download evidence controls are enabled.
- [ ] No notification send control is enabled.

## Preflight PASS

Run:

```powershell
.\tools\validate-portal-financiero-e2e.ps1 -OutputMarkdown -VerboseDiagnostics -SuggestFixes
```

PASS requires:

- [ ] Script exits `0`.
- [ ] All required checks are `PASS`.
- [ ] No `BLOCKED_DEPENDENCY`.
- [ ] No `FAIL`.
- [ ] Evidence is sanitized and committed only as text.

## No-production guardrails

- [ ] No SRI Production.
- [ ] No SRI Test real send.
- [ ] No official ATS.
- [ ] No legal-final RIDE.
- [ ] No productive XAdES.
- [ ] No real certificates.
- [ ] No real taxpayer XML.
- [ ] No real SRI responses.
- [ ] No secrets or full connection strings.

Financiero remains not production-ready until this checklist passes and governance approves the next scope.
