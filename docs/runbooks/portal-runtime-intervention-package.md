# Portal Runtime Intervention Package

## Sprint 11 P4 Portal follow-up note

No accepted Portal evidence was received in P4. Portal Gateway, Portal Shell, PortalShellContext, Menu/permissions and correlation id remain `NoResponse` / `EvidencePending` / `BLOCKED_DEPENDENCY`; preflight returned `SCRIPT_EXIT=2` and Portal Gateway `/health` returned HTTP 404. Do not create Gateway propio, Shell propio or Auth/Login propio in Financiero.

No SRI Production; No official ATS; No legal-final RIDE; No productive XAdES.

## Sprint 11 P3 Portal escalation note

No accepted Portal evidence was received in P3. Portal Gateway, Portal Shell, PortalShellContext, Menu/permissions and correlation id remain `NoResponse` / `EvidencePending` / `BLOCKED_DEPENDENCY`; preflight returned `SCRIPT_EXIT=2` and Portal Gateway `/health` returned HTTP 404. Do not create Gateway propio, Shell propio or Auth/Login propio in Financiero.

No SRI Production; No official ATS; No legal-final RIDE; No productive XAdES.

## Sprint 11 P2 Portal follow-up note

No accepted Portal evidence was received in P2. Portal Gateway, Portal Shell, PortalShellContext, Menu/permissions and correlation id remain `EvidencePending` / `BLOCKED_DEPENDENCY`.

No Gateway propio, Portal Shell propio or Auth/Login propio. No SRI Production; No official ATS; No legal-final RIDE; No productive XAdES.

## Sprint 11 P1 Portal external remediation

Portal remediation must happen outside the Financiero repo. Required return evidence: Portal Gateway health, Portal Shell health, PortalShellContext live, Menu/permissions live and correlation id live evidence.

No Gateway propio, Portal Shell propio or Auth/Login propio. No SRI Production; No official ATS; No legal-final RIDE; No productive XAdES.

## Sprint 10 P5 Portal closure note

Sprint 10 closes as `BLOCKED_DEPENDENCY` with Portal Gateway, Portal Shell, PortalShellContext, Menu/permissions and correlation id evidence still pending. Sprint 11 Option A should remediate this outside the Financiero repo.

No Gateway propio, Portal Shell propio or Auth/Login propio. No SRI Production; No official ATS; No legal-final RIDE; No productive XAdES.

## Sprint 10 P4 Portal follow-up note

P4 follow-up confirms `NoResponse` / `EvidencePending` for Portal Gateway, Portal Shell, PortalShellContext, Menu/permissions and correlation id. Productization remains blocked; do not create Gateway propio, Shell propio or Auth/Login propio in Financiero. Reopen PASS capture only with accepted Portal owner evidence.

No SRI Production; No official ATS; No legal-final RIDE; No productive XAdES. Financiero remains not production-ready.

## Sprint 10 P3 escalation note

Portal Gateway, Portal Shell, PortalShellContext, Menu/permissions and correlation id evidence remain `EvidencePending` / `BLOCKED_DEPENDENCY`. The required action is owner-provided sanitized evidence using the P3 formal request and escalation matrix; Financiero must not create Gateway propio, Shell propio, login/Auth propio or duplicated Portal services.

No SRI Production; No official ATS; No legal-final RIDE; No productive XAdES. Financiero remains not production-ready.

Sprint 10 P2 review: Portal Gateway, Portal Shell and Portal Contract evidence was NotReceived, so Gate 3, Gate 4 and Gate 5 remain `BLOCKED_DEPENDENCY`.

Sprint 10 P1 requires Portal owners to use the Gateway, Shell and Contract evidence templates under `docs/qa/templates` and pass Gate 3, Gate 4 and Gate 5.

Sprint 9 P5 closure keeps this intervention as a Sprint 10 prerequisite. Portal Gateway and Portal Shell evidence are required before E2E PASS.

Sprint 9 P4 prepares the Portal Gateway and Portal Shell intervention needed before Financiero can claim real E2E `PASS`.

## Observed problem

- Portal Gateway base URL responds, but `/health` returned HTTP 404.
- Portal Shell live evidence is not available.
- Financiero must not create a duplicate Gateway or Shell.

## Gateway health routes to confirm

Portal owner must confirm one documented route:

- `/health`
- `/health/live`
- `/health/ready`
- `/api/health`
- another Portal-documented route

If a non-default path is used, run the preflight with `-PortalGatewayHealthPath`.

## Runtime URLs to confirm

- Portal Gateway base URL, expected local default: `http://localhost:8082`.
- Portal Shell base URL, expected local default: `http://localhost:4201` or Portal-provided URL.
- Portal Shell health path, expected local default: `/health`.

## PortalShellContext contract evidence

Portal owner must provide sanitized evidence for:

- `contractVersion=1.0`.
- `source=portal`.
- delegated permissions including `financial.electronicdocuments.read`.
- menu routes compatible with Financiero allow-list.
- safe feature flags with productive actions disabled.
- `correlationId` propagation.

## Evidence expected for PASS

- Gateway health returns HTTP 2xx at the agreed route.
- Shell health/base route returns HTTP 2xx.
- PortalShellContext is available and sanitized.
- Financiero preflight exits `0` after shared SQL is available.

## Do not do

- Do not create Gateway inside Financiero.
- Do not create Shell inside Financiero.
- Do not duplicate Security, Menu, Configuration, Audit, Outbox, Notification or Content/File.
- Do not expose or commit tokens.
- Do not enable SRI Production, official ATS, legal-final RIDE or productive XAdES.

Control tokens: BLOCKED_DEPENDENCY; Portal Gateway; shared SQL; not production-ready; No SRI Production; No official ATS; No legal-final RIDE; No productive XAdES.
