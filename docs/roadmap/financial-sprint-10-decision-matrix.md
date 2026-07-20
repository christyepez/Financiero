# Financial Sprint 10 Decision Matrix

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
