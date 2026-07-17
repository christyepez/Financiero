# Sprint 8 Roadmap

## Goal

Move from foundation readiness to controlled integration evidence without enabling production tax flows.

Control tokens: No SRI Production; No official ATS; No legal-final RIDE; No productive XAdES.

## Option A - Real PortalCorporativo end-to-end validation

- Use real PortalShellContext.
- Validate Security/Menu/Configuration context.
- Validate Gateway routing.
- Validate shared SQL runtime.
- Confirm delegated auth without token storage.
- Use `/api/financial/portal-integration/readiness`, `portal-e2e-validation-checklist.md` and the local E2E runbook as the validation frame.

Recommended first because it reduces platform integration risk for every future package.

## Option B - Controlled purchase productization QA

- Use synthetic data only.
- Exercise purchase readiness and approval gates.
- Keep SRI Production, SRI Test real send, official ATS and real certificates blocked.

## Option C - External Approval UX hardening

- Improve review UX.
- Add audit/reporting views.
- Validate Content/File reference contract with Portal when available.

## Option D - Stable local QA infrastructure

- Provide repeatable shared SQL environment.
- Add integration docker/devcontainer guidance.
- Make health checks repeatable across machines.

## Prioritized route

1. Option A: Portal E2E validation with Sprint 8 P1 checklist/runbook/evidence template.
2. Option D: Stable shared SQL and health.
3. Option C: Approval UX and Portal boundaries.
4. Option B: Controlled purchase QA with synthetic data.

## Sprint 8 P2 update

The first executable validation produced partial validation: Financiero compose is valid and no SQL Server propio exists, but shared SQL and Portal Gateway/Shell were not reachable in the local environment. The next priority is to make shared SQL and Portal runtime available, then rerun `tools/validate-portal-financiero-e2e.ps1` and capture PASS evidence.

Sprint 8 P3 adds QA preflight stabilization. The next execution should treat exit code `2` as blocked dependency and reserve `1` for repository/script/application failures.

## Explicit exclusions

- No SRI Production.
- No SRI Test real send.
- No official ATS.
- No legal-final RIDE.
- No productive XAdES.
- No real certificates.
- No automatic activation.
