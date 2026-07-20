# Financial Next Cycle P1 Executive Decision Request

Date: 2026-07-20  
Requested by: Financiero Release / QA / Architecture Governance  
Current result: `BLOCKED_DEPENDENCY`  
External evidence: `NoResponse` / `EvidencePending`  
PASS capture: closed  
Preflight: `SCRIPT_EXIT=2`  
Production state: not production-ready.

## Request

Please decide whether the next cycle should continue external SQL/Portal remediation with named owners and SLA, pause Financiero productization, or approve a clearly labeled synthetic/demo path. PASS E2E real must remain closed until accepted external evidence exists and preflight returns `SCRIPT_EXIT=0`.

## What is blocked

- SQL Common TCP.
- `FinancieroDb` availability.
- Portal Gateway health.
- Portal Shell health.
- PortalShellContext live.
- Menu/permissions live.
- Correlation id live.
- Preflight `SCRIPT_EXIT=0`.

## Why Financiero must not duplicate infrastructure

Creating SQL Server propio, Gateway propio, Portal Shell propio, Auth/Login propio, token storage or duplicated Portal services would violate the shared-platform architecture and produce a false readiness signal. Financiero must consume Portal and common infrastructure rather than replacing them.

## Evidence needed

Sanitized owner-provided evidence for each blocked gate, with no secrets, tokens, private URLs, passwords, certificates, XML reales, personal data or real SRI responses.

## If no owner/SLA exists

Pause productization or escalate ownership. Do not continue producing PASS attempts without real remediation.

## Allowed / not allowed

Allowed: external remediation, sanitized evidence, non-production verification and explicitly approved synthetic/demo work.  
Not allowed: production activation, real SRI, official ATS, legal-final RIDE, productive XAdES, upload/download evidence, notification send or duplicated Portal/Infra capabilities.

## Suggested target date

Owner/SLA decision target: 2026-07-22 for critical SQL/Gateway owners and 2026-07-23 for Shell/Portal contract owners.

## Expected result

A named operating decision: continue external remediation, pause productization, approve synthetic/demo with guardrails, or authorize PASS capture only after accepted evidence and `SCRIPT_EXIT=0`.

No SRI Production; No official ATS; No legal-final RIDE; No productive XAdES.
