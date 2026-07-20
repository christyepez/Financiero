# Financial E2E PASS Checklist

Sprint 10 P2 executed gates and did not pass final acceptance. External evidence remains pending.

Sprint 10 P1 adds formal gates. PASS is allowed only after Gate 1 through Gate 9 and the final gate are accepted.

Sprint 9 P5 decision: this checklist is not complete. Sprint 10 must complete external infra remediation before PASS.

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

## Sprint 9 P4 intervention acceptance

- [ ] Owner SQL assigned and target date recorded.
- [ ] Owner Portal Gateway assigned and target date recorded.
- [ ] Owner Portal Shell assigned and target date recorded.
- [ ] Agreed SQL port is documented, default `21433`.
- [ ] Agreed Gateway health route is documented, default `/health`.
- [ ] Agreed Shell health route is documented, default `/health`.
- [ ] Infra returns sanitized shared SQL evidence.
- [ ] Portal returns sanitized Gateway/Shell evidence.
- [ ] Preflight is executed with explicit health route parameters.
- [ ] `docs/runbooks/infra-sql-common-intervention-package.md` is satisfied.
- [ ] `docs/runbooks/portal-runtime-intervention-package.md` is satisfied.
- [ ] `docs/qa/financial-sprint-09-p4-infra-intervention-evidence.md` records PASS or `BLOCKED_DEPENDENCY`.
