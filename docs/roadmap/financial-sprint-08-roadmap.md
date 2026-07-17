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

## Sprint 8 P4 update

External Approval UX is hardened for functional QA. The screen now explains that `ApprovedFoundation` does not enable production, evidence references are Portal-owned metadata-only, notification intents are foundation/no-send, and production requires Portal plus legal/tax/security approval.

This closes the approval workflow readiness from a UX perspective, but it does not activate SRI Test real send, SRI Production, official ATS, legal-final RIDE, productive XAdES, evidence upload/download, Content/File storage or Notification send.

## Sprint 8 P5 closure

Sprint 8 closes with final result `BLOCKED_DEPENDENCY`. Repository validation, backend tests, frontend build/verifiers and compose config pass, but full E2E PASS is not claimed until shared SQL and Portal Gateway/Shell are live.

Sprint 9 recommendation: Option A from `docs/roadmap/financial-sprint-09-decision-matrix.md` - stabilize real E2E infrastructure first. Controlled productization backlog is tracked in `docs/roadmap/financial-controlled-productization-backlog.md`.

## Explicit exclusions

- No SRI Production.
- No SRI Test real send.
- No official ATS.
- No legal-final RIDE.
- No productive XAdES.
- No real certificates.
- No automatic activation.
