# Financial Sprint 11 Decision Matrix

## Sprint 11 P3 update

P3 result: `BLOCKED_DEPENDENCY` with `NoResponse` / `EvidencePending` and preflight `SCRIPT_EXIT=2`. Option A remains active: keep external escalation and do not productize. Option B PASS capture is NOT_READY and can start only if evidence is accepted and preflight returns `SCRIPT_EXIT=0`. If future evidence is rejected, mark `REJECTED_EVIDENCE`; if a real Financiero application defect is proven, create a fix sprint and mark FAIL.

No SRI Production; No official ATS; No legal-final RIDE; No productive XAdES.

## Sprint 11 P2 update

P2 result: `BLOCKED_DEPENDENCY` with `NoResponse` / `EvidencePending`. Option A remains active. Do not productize. Continue external follow-up. Switch to Option B PASS capture only if evidence is accepted and preflight returns `SCRIPT_EXIT=0`. If evidence is rejected, keep Option A with correction. If a real application FAIL is proven, create a Financiero fix sprint.

No SRI Production; No official ATS; No legal-final RIDE; No productive XAdES.

## Sprint 11 P1 update

P1 confirms Option A as the active recommendation: external infra remediation outside Financiero repo. Switch to Option B PASS capture only when external owners provide accepted evidence and preflight can return `SCRIPT_EXIT=0`. Do not continue productization if owners remain undefined or evidence cannot be mapped to gates.

Artifacts:

- `docs/coordination/financial-sprint-11-p1-external-remediation-plan.md`.
- `docs/coordination/financial-sprint-11-p1-external-repo-handoff-checklist.md`.
- `docs/qa/financial-sprint-11-p1-return-to-pass-criteria.md`.

No SRI Production; No official ATS; No legal-final RIDE; No productive XAdES.

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
