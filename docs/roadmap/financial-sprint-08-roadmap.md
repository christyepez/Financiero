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

1. Option A: Portal E2E validation.
2. Option D: Stable shared SQL and health.
3. Option C: Approval UX and Portal boundaries.
4. Option B: Controlled purchase QA with synthetic data.

## Explicit exclusions

- No SRI Production.
- No SRI Test real send.
- No official ATS.
- No legal-final RIDE.
- No productive XAdES.
- No real certificates.
- No automatic activation.
