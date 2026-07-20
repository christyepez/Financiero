# Financial Sprint 11 Decision Matrix

Date: 2026-07-20  
Input: Sprint 10 closes as `BLOCKED_DEPENDENCY`.  
Production state: not production-ready.

| Option | When to choose | Dependencies | Risk | Effort | Recommendation |
|---|---|---|---|---|---|
| A. External infra remediation outside Financiero repo | Sprint 10 closed `BLOCKED_DEPENDENCY` and owners still have pending evidence | SQL Common, DBA, Portal Gateway, Portal Shell, Portal Contract, Security/Menu, Observability | Low risk for Financiero architecture; high dependency on external owners | Medium | Recommended |
| B. PASS capture sprint | Owners deliver accepted evidence and preflight can return 0 | Accepted shared SQL, `FinancieroDb`, Portal Gateway, Shell, PortalShellContext/Menu/permissions/correlation id | Low if evidence is real and sanitized | Low to medium | Conditional |
| C. Controlled synthetic productization | Executive approval explicitly accepts no real E2E PASS | Written executive approval, no-production guardrails, mock-only scope | High; can create false readiness | Medium | Not recommended |
| D. Pause productization and return to backlog | No owner, no SLA or no external remediation path | Product Owner and Architecture Governance decision | Low delivery risk, high schedule impact | Low | Acceptable fallback |

## Objective criteria

- PASS capture requires accepted evidence and preflight exit 0.
- Synthetic continuation requires explicit executive decision and must remain non-production.
- Productization remains blocked without real SQL/Portal evidence.
- No duplicated Portal or SQL capabilities may be created in Financiero.

## Final recommendation

Sprint 11 should use Option A: external infra remediation outside Financiero repo. If owners deliver evidence before Sprint 11 starts, switch to Option B and capture PASS with sanitized evidence.

No SRI Production; No official ATS; No legal-final RIDE; No productive XAdES.
