# Financial Risk Register

## Sprint 11 P1 external remediation risks

| Risk | Impact | Mitigation | State |
|---|---|---|---|
| Remediation outside repo loses visibility | PASS capture may reopen without traceability | Use handoff checklist and return-to-PASS criteria | EvidencePending |
| Evidence outside standard | Evidence cannot be accepted | Require templates and QA acceptance review | BLOCKED_DEPENDENCY |
| External fix without Financiero coordination | Runtime may drift from expected preflight | Re-run preflight and document evidence outside repo | BLOCKED_DEPENDENCY |
| Gateway/Shell partially remediated | Portal-integrated UX still blocked | Require all Portal gates, not partial PASS | EvidencePending |
| SQL enabled without `FinancieroDb` | DB gate remains blocked | Require TCP and logical DB evidence | EvidencePending |
| Premature return to PASS capture | False readiness | P2 only after accepted evidence and `SCRIPT_EXIT=0` | BLOCKED_DEPENDENCY |

No SRI Production; No official ATS; No legal-final RIDE; No productive XAdES.

## Sprint 10 P5 closure risks

| Risk | Impact | Mitigation | Mitigation owner | State |
|---|---|---|---|---|
| External block closes without remediation | Productization remains stopped | Sprint 11 Option A external remediation | Product Owner + external owners | BLOCKED_DEPENDENCY |
| Bypass pressure after closure | Architecture and security drift | No duplicated SQL/Portal capabilities; verifier enforcement | Architecture Governance | BLOCKED_DEPENDENCY |
| Owner remains undefined | No unlock path | Require named SQL/Portal owners before PASS capture | Product Owner | EvidencePending |
| Late evidence appears after closure | Reopen decision may be unclear | Use Sprint 11 Option B only after accepted evidence and preflight 0 | QA Lead | EvidencePending |
| Synthetic productization is requested | False readiness | Require explicit executive approval and keep non-production | Product Owner | High risk |

No SRI Production; No official ATS; No legal-final RIDE; No productive XAdES. Financiero remains not production-ready.

## Sprint 10 P4 executive block risks

Executive follow-up confirms `NoResponse` / `EvidencePending`: shared SQL and Portal Gateway remain not production-ready until accepted evidence arrives.

| Risk | Impact | Mitigation | State |
|---|---|---|---|
| SLA unresolved after follow-up | Productization remains blocked | Executive block decision and P5 controlled closure | BLOCKED_DEPENDENCY |
| Productization pressure without E2E PASS | False readiness and operational exposure | Explicit no-PASS/no-production guardrails | BLOCKED_DEPENDENCY |
| Late evidence arrives after closure | PASS capture may need re-open decision | Require sanitized intake and rerun preflight before acceptance | EvidencePending |
| External remediation stays outside Financiero | Ownership gap remains unresolved | Keep backlog and executive decision traceable | EvidencePending |
| Team bypasses Portal/Infra with local duplicates | Architecture drift and security risk | No SQL/Gateway/Shell/Auth propios; enforce verifiers | BLOCKED_DEPENDENCY |
| Traceability is lost across P1-P4 | Governance cannot audit why PASS was blocked | Maintain PR history, intake, matrix, log and executive decision | EvidencePending |

No SRI Production; No official ATS; No legal-final RIDE; No productive XAdES. Financiero remains not production-ready.

## Sprint 10 P3 external escalation risks

The unresolved runtime path explicitly includes shared SQL and Portal Gateway evidence before any PASS capture.

| Risk | Impact | Mitigation | State |
|---|---|---|---|
| External owner does not respond | E2E PASS and productization remain blocked | SLA, escalation matrix and Product Owner decision gate | EvidencePending |
| Owner is not assigned or accountable | Dependency cannot be remediated | Escalate to Architecture Governance and require named owner | EvidencePending |
| SLA expires | Sprint 10 cannot close with runtime PASS | Execute Sprint 10 P4 follow-up or resolve outside Financiero repo | BLOCKED_DEPENDENCY |
| Remediation is partial | False confidence or unstable PASS | Accept only gate-mapped, sanitized evidence | EvidencePending |
| Evidence is not usable | PASS cannot be accepted | Use templates and reject secrets/private data | EvidencePending |
| Team continues with mocks without PASS | Productization risk | Allow mocks only as development mode; do not claim production readiness | BLOCKED_DEPENDENCY |
| Traceability is lost | Cannot audit dependency decisions | Maintain intake, backlog, remediation log and PR history | EvidencePending |

No SRI Production; No official ATS; No legal-final RIDE; No productive XAdES. Financiero remains not production-ready.

## Sprint 10 P2 evidence review risks

| Risk | Status | Impact | Mitigation |
|---|---|---|---|
| Evidence not received | Active | Gates 1-8 cannot pass. | Escalate using owner intake and templates. |
| Evidence invalid or incomplete | Watch | Could delay acceptance after owners respond. | Validate against templates before accepting. |
| Evidence contains secrets | Watch | Security incident risk. | Reject evidence with tokens, passwords, full connection strings, certificates, XML reales or personal data. |
| Dependency confirmed out of scope by owner | Watch | Requires governance decision. | Mark Blocked/OutOfScope only with formal owner response. |
| Partial PASS | Active | Can be mistaken for E2E readiness. | Require all gates accepted and preflight exit `0`. |
| Prolonged external block | Active | Productization remains blocked. | Escalate owner assignment and target dates. |

Control tokens: BLOCKED_DEPENDENCY; Portal Gateway; shared SQL; not production-ready; No SRI Production; No official ATS; No legal-final RIDE; No productive XAdES.

| Risk | Impact | Status | Mitigation |
|---|---|---|---|
| Shared SQL unavailable | Blocks API readiness and full E2E PASS. | Open | Start one shared SQL container and keep logical databases separate. |
| Portal Gateway unavailable | Blocks Portal-integrated runtime validation. | Open | Start PortalCorporativo Gateway and capture health evidence. |
| PortalShellContext drift | Menu/auth/config may diverge from Portal contract. | Open | Keep static verifier and add runtime drift evidence in Sprint 9. |
| Misinterpretation of ApprovedFoundation | Users may assume production approval. | Mitigated | P4 UX states it does not enable production. |
| Accidental production activation | SRI/ATS/RIDE/XAdES could be enabled too early. | Open | Feature flags remain false and verifiers check no-production tokens. |
| Secret exposure | Tokens/passwords/certificates could leak. | Open | No `.env`, certs, token storage or sensitive querystrings committed. |
| Real certificates/XML committed | Legal/security breach. | Open | Static scans and repo policy reject certificates/XML evidence. |
| Portal capability duplication | Financiero could become a parallel platform. | Open | Reuse Portal Security/Menu/Config/Audit/Outbox/Content/File/Notification. |
| Missing legal/tax approval | Productive flows could lack compliance signoff. | Open | Maintain external approval checklist and no-production guardrails. |
| Health false positive | Dependency failures could be hidden. | Mitigated | P3 classification separates PASS, BLOCKED_DEPENDENCY and FAIL. |
| Missing final E2E PASS evidence | Sprint closure could overstate readiness. | Open | P5 final evidence marks current state as BLOCKED_DEPENDENCY. |

This register must be reviewed before any Sprint 9 productization decision.
# Sprint 9 P5 external infrastructure closure risks

## Sprint 10 P1 remediation intake risks

| Risk | Status | Impact | Mitigation |
|---|---|---|---|
| Owner missing or unresponsive | Active | Blocks all external remediation gates. | Assign SQL, Gateway, Shell and Contract owners in P1 intake. |
| Invalid evidence | Active | Creates false confidence. | Require templates and acceptance gates. |
| Evidence includes secrets | Active | Security incident risk. | Reject evidence containing passwords, tokens, full connection strings, certificates, XML reales or personal data. |
| Gateway health fixed but Shell remains absent | Active | Partial remediation cannot produce E2E PASS. | Gate 4 and Gate 5 remain required. |
| SQL TCP opens but `FinancieroDb` unavailable | Active | API readiness still blocked. | Gate 2 requires logical DB proof. |
| Preflight partial PASS misread as complete PASS | Active | False PASS and governance breach. | Final gate requires exit code `0` and all gates accepted. |
| Productive activation before gates | Active | Production and tax compliance risk. | Keep no-production guardrails and release approval blocked. |

Control tokens: BLOCKED_DEPENDENCY; Portal Gateway; shared SQL; not production-ready; No SRI Production; No official ATS; No legal-final RIDE; No productive XAdES.

| Risk | Status | Impact | Mitigation |
|---|---|---|---|
| Shared SQL port `21433` remains closed | Active | Blocks real E2E PASS. | Sprint 10 Option A: Infra remediation with sanitized TCP evidence. |
| Portal Gateway health route returns 404/unconfirmed | Active | Blocks Gateway readiness evidence. | Portal owner must confirm `/health`, `/health/live`, `/health/ready`, `/api/health` or another documented route. |
| Portal Shell has no live evidence | Active | Blocks PortalShellContext validation. | Portal Shell owner must provide URL, context, menu, permissions and correlation id evidence. |
| No accountable external owner | Active | Delays remediation. | Use external dependency backlog with owner/target date fields. |
| Productization decision without E2E PASS | Active | Governance and quality risk. | Keep productization blocked until preflight exits `0`. |
| Documentation fatigue without runtime correction | Active | Creates perceived progress without unblocking. | Sprint 10 should prioritize runtime remediation over more paperwork. |
| False PASS | Active | Misrepresents readiness. | Require sanitized evidence and explicit PASS criteria. |
| Improper infrastructure duplication | Active | Violates Portal reuse architecture. | Do not create SQL Server, Gateway, Shell, Auth, Menu, Configuration, Audit, Outbox, Notification or Content/File in Financiero. |

Control tokens: BLOCKED_DEPENDENCY; Portal Gateway; shared SQL; not production-ready; No SRI Production; No official ATS; No legal-final RIDE; No productive XAdES.
