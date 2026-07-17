# Financial Risk Register

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
