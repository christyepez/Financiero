# Financial Sprint 10 Decision Matrix

## Sprint 10 P5 final decision

Sprint 10 closes as `BLOCKED_DEPENDENCY`. The final closure is documented in `docs/coordination/financial-sprint-10-closure.md`, with final evidence in `docs/qa/financial-sprint-10-final-evidence.md`. Sprint 11 options are documented in `docs/roadmap/financial-sprint-11-decision-matrix.md`.

Final decision: do not productize, do not claim PASS, keep external dependency backlog active outside Financiero, and reopen PASS capture only after accepted SQL/Portal evidence and preflight exit 0.

No SRI Production; No official ATS; No legal-final RIDE; No productive XAdES.

## Sprint 10 P4 decision update

P4 confirms `NoResponse` / `EvidencePending`. Without owner evidence, the objective decision is to continue P5 as closure of external block, not productize and keep the external dependency backlog outside Financiero until SQL/Portal owners deliver accepted evidence.

Executive follow-up confirms shared SQL and Portal Gateway remain not production-ready until accepted evidence arrives.

The executive block decision remains active while evidence is pending.

| Evidence state | P4 decision | P5 direction |
|---|---|---|
| No evidence received | Keep `BLOCKED_DEPENDENCY`; block productization | Controlled closure of external block |
| Evidence received and accepted | Re-run preflight; PASS only on exit 0 | PASS capture package |
| Evidence unsafe or partial | Mark `REJECTED_EVIDENCE`; request correction | Remediation follow-up |
| Application failure proven | Mark FAIL and fix in scope | Bugfix package |

No SRI Production; No official ATS; No legal-final RIDE; No productive XAdES.

## Sprint 10 P3 decision update

P3 keeps the decision at `EvidencePending` / `BLOCKED_DEPENDENCY`. The recommended path is not to productize, not to claim PASS, and not to create duplicated SQL/Portal capabilities in Financiero.

The unresolved runtime path explicitly includes shared SQL and Portal Gateway evidence before any PASS capture.

| Condition | Decision |
|---|---|
| Owners deliver sanitized evidence within SLA | Validate evidence and move to PASS capture only after preflight exit 0 |
| Owners do not respond | Execute Sprint 10 P4 external remediation follow-up or resolve Portal/Infra outside Financiero |
| Evidence is partial or unsafe | Reject intake, keep `BLOCKED_DEPENDENCY`, request corrected evidence |
| Team wants to continue with mocks | Allowed for development only; not production-ready and no PASS |

No SRI Production; No official ATS; No legal-final RIDE; No productive XAdES.

## Sprint 10 P2 gate result

Owner evidence review found no external evidence received. Option A remains active but blocked at evidence intake. Sprint 10 P2 result is `BLOCKED_DEPENDENCY`, with Gate 9 PASS and Gates 1-8 blocked.

## Sprint 10 P1 kickoff

Option A is active as owner evidence intake. P1 does not remediate runtime directly; it prepares acceptance gates and evidence templates so SQL Common Owner, Portal Gateway Owner, Portal Shell Owner and Portal Contract Owner can return valid proof.

## Decision context

Sprint 9 closes as `BLOCKED_DEPENDENCY`. Financiero is not production-ready because shared SQL, Portal Gateway health and Portal Shell evidence are still external blockers.

## Options

| Option | Description | Dependencies | Risk | Effort | Recommendation |
|---|---|---|---|---|---|
| A. External Infra Remediation Sprint | Correct shared SQL, confirm Gateway health route, lift Portal Shell and capture real PASS. | Infra + Portal owners. | Medium; depends on external teams. | Medium | Recommended. |
| B. Controlled productization with mocks/synthetic data | Continue foundation work using mocks while accepting no real E2E. | Governance acceptance. | High; can hide integration gaps. | Low/Medium | Not recommended unless explicitly approved. |
| C. Portal contract alignment | Confirm routes, PortalShellContext, menu, permissions and correlation id with PortalCorporativo. | Portal owner. | Medium. | Low/Medium | Pair with Option A. |
| D. QA automation hardening | Automate preflight reports and static no-production checks. | Local tooling only. | Low, but does not solve runtime. | Low | Useful after A or alongside A. |

## Decision criteria

- Real SQL connectivity exists.
- Portal Gateway health route returns HTTP 2xx.
- Portal Shell provides live PortalShellContext.
- No production guardrails are weakened.
- No Portal capabilities are duplicated in Financiero.

## Final recommendation

Choose Option A as Sprint 10 primary path, with Option C as required companion. Option D may continue only if it does not replace runtime remediation.

Control tokens: BLOCKED_DEPENDENCY; Portal Gateway; shared SQL; not production-ready; No SRI Production; No official ATS; No legal-final RIDE; No productive XAdES.
