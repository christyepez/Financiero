# Financial Next Cycle P1 Operating Decision

Date: 2026-07-20  
Phase: Next Cycle P1  
Inherited result: `BLOCKED_DEPENDENCY`  
Current preflight: `SCRIPT_EXIT=2`  
PASS capture: closed  
PASS E2E real: NOT_READY  
Production state: not production-ready.

## Executive summary

Sprint 11 closed as an external SQL/Portal dependency block. Next Cycle P1 converts that closure into an operating decision: either continue external remediation with named owners and SLA, pause productization until owners exist, approve a clearly labeled synthetic/demo path, or reopen PASS capture only after accepted evidence and preflight `SCRIPT_EXIT=0`.

Financiero must not solve the block by creating SQL Server propio, Gateway propio, Portal Shell propio, login/Auth propio, token storage or duplicated Portal capabilities.

## Inherited state from Sprint 11

- Final result: `BLOCKED_DEPENDENCY`.
- PASS E2E real: NOT_READY.
- External evidence: `NoResponse` / `EvidencePending`.
- Preflight remains `SCRIPT_EXIT=2`.
- Gates 1-8 remain `BLOCKED_DEPENDENCY`.
- Gate 9 no-production guardrails remains PASS.

## Required decision

| Option | Criteria | Decision impact |
|---|---|---|
| Continue external remediation | Owners and SLA can be assigned for SQL/Portal dependencies | Recommended operating path |
| Pause productization | Owners or SLA remain unavailable | Stops productization pressure and avoids architecture bypass |
| Controlled synthetic/demo path | Explicit executive approval; clear non-PASS labeling | Allowed only as demo, not PASS real |
| Reopen PASS capture | Accepted external evidence and preflight `SCRIPT_EXIT=0` | Only route to PASS E2E real |

## Required owners

SQL Common / Infra Owner, DBA Owner, Portal Gateway Owner, Portal Shell Owner, Portal Contract Owner, Portal Security/Menu Owner and Portal Observability Owner.

## Minimum SLA

Critical SQL/Gateway owners should respond within 2 business days. Shell/PortalShellContext/Menu/correlation owners should respond within 3 business days. If SLA expires without owner evidence, Product Owner and Architecture Governance should pause productization or escalate externally.

## Recommendation

Continue external remediation only if owners/SLA are assigned. Otherwise pause productization. Do not reopen PASS capture and do not create duplicated platform capabilities in Financiero.

No SRI Production; No official ATS; No legal-final RIDE; No productive XAdES.
