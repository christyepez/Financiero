# Financial Sprint 11 P1 External Remediation Plan

## Sprint 11 P5 closure update

Sprint 11 closes as `BLOCKED_DEPENDENCY`. P5 preflight returned `SCRIPT_EXIT=2`; external evidence remains `NoResponse` / `EvidencePending`; PASS E2E real is NOT_READY. Continue remediation outside Financiero or pause productization if owners/SLA remain unavailable.

No SRI Production; No official ATS; No legal-final RIDE; No productive XAdES.

## Sprint 11 P4 follow-up update

Sprint 11 P4 confirms `NoResponse` / `EvidencePending` for external SQL/Portal evidence. Preflight returned `SCRIPT_EXIT=2`, so the current decision remains `BLOCKED_DEPENDENCY`; remediation must continue outside the Financiero repo. PASS E2E real is NOT_READY until external evidence is accepted and preflight returns `SCRIPT_EXIT=0`.

References:

- `docs/qa/financial-sprint-11-p4-execution-evidence.md`.
- `docs/coordination/financial-sprint-11-p4-external-escalation-followup.md`.

No SRI Production; No official ATS; No legal-final RIDE; No productive XAdES.

## Sprint 11 P3 escalation update

Sprint 11 P3 confirms `NoResponse` / `EvidencePending` for external SQL/Portal evidence. Preflight returned `SCRIPT_EXIT=2`, so the current decision remains `BLOCKED_DEPENDENCY`; remediation must continue outside the Financiero repo. PASS capture is NOT_READY until external evidence is accepted and preflight returns `SCRIPT_EXIT=0`.

References:

- `docs/qa/financial-sprint-11-p3-gate-decision-evidence.md`.
- `docs/coordination/financial-sprint-11-p3-external-escalation.md`.

No SRI Production; No official ATS; No legal-final RIDE; No productive XAdES.

## Sprint 11 P2 follow-up update

Sprint 11 P2 found no accepted external Infra/Portal evidence. Owner response is `NoResponse` and the evidence state remains `EvidencePending`. The current decision remains `BLOCKED_DEPENDENCY`; remediation must continue outside the Financiero repo. PASS capture is not allowed until external evidence is accepted and preflight returns `SCRIPT_EXIT=0`.

References:

- `docs/qa/financial-sprint-11-p2-external-evidence-followup.md`.
- `docs/qa/financial-sprint-11-p2-return-to-pass-review.md`.

No SRI Production; No official ATS; No legal-final RIDE; No productive XAdES.

Date: 2026-07-20  
Phase: Sprint 11 P1  
State: `BLOCKED_DEPENDENCY`  
Recommended option: external infra remediation outside Financiero repo.

## Executive summary

Sprint 10 closed as `BLOCKED_DEPENDENCY`, not PASS. Sprint 11 P1 starts the operational plan for SQL/Portal remediation outside the Financiero repository. Financiero must not solve the block by creating its own SQL Server, Gateway, Portal Shell, Auth/Login, token storage or duplicated Portal capabilities.

Return to PASS requires shared SQL, Portal Gateway and `SCRIPT_EXIT=0`; until then the system remains not production-ready.

## Final Sprint 10 state

- Gates 1-8: `BLOCKED_DEPENDENCY`.
- Gate 9 no-production guardrails: PASS.
- Latest preflight: `SCRIPT_EXIT=2`.
- Productization: blocked.
- PASS capture: closed until accepted owner evidence and preflight `SCRIPT_EXIT=0`.

## External dependencies pending

- shared SQL TCP.
- `FinancieroDb` logical database availability.
- Portal Gateway health route.
- Portal Shell health route.
- PortalShellContext live contract.
- Portal-owned Menu/permissions.
- Correlation id propagation.
- Preflight `SCRIPT_EXIT=0`.

## What Infra must do

- Restore or expose the shared SQL endpoint expected by the local environment.
- Confirm the shared SQL host/port without exposing secrets.
- Confirm `FinancieroDb` exists as a separate logical database.
- Provide sanitized evidence using the handoff checklist and evidence templates.

## What Portal must do

- Confirm Portal Gateway health/readiness route and port.
- Confirm Portal Shell health route and live availability.
- Provide sanitized PortalShellContext evidence.
- Provide live Menu/permissions evidence for Financiero resources.
- Provide correlation id propagation evidence.

## What Financiero must do

- Keep the application code unchanged for this remediation package.
- Maintain preflight, templates, intake and verification gates.
- Receive evidence through sanitized docs only.
- Reopen PASS capture only when accepted evidence exists.

## Outside the Financiero repo

- Creating or fixing SQL Server runtime.
- Configuring Portal Gateway/Shell runtime.
- Changing Portal-owned Security/Menu/Configuration/Audit/Notification/Content/File.
- Managing real secrets, certificates, private URLs or production infrastructure.

## Evidence required to return

- Accepted shared SQL TCP evidence.
- Accepted `FinancieroDb` evidence.
- Accepted Portal Gateway health evidence.
- Accepted Portal Shell health evidence.
- Accepted PortalShellContext/Menu/permissions/correlation id evidence.
- Sanitized preflight evidence outside the repo with exit code 0.

## Acceptance criteria

- Evidence is sanitized and owner-provided.
- Evidence maps to a Sprint 10/Sprint 11 gate.
- Evidence contains no secrets, tokens, passwords, private connection strings, certificates, XML reales, personal data or real SRI responses.
- Preflight exits 0 before PASS capture is claimed.

## Risks

- External remediation occurs without Financiero coordination.
- Partial Portal/SQL remediation creates false readiness.
- Evidence is delivered outside the accepted templates.
- PASS capture reopens prematurely.

## Recommended decision

Continue Sprint 11 with external infra remediation outside Financiero. Switch to PASS capture only after owners provide accepted evidence and the preflight exits 0.

No SRI Production; No official ATS; No legal-final RIDE; No productive XAdES. Financiero remains not production-ready.
