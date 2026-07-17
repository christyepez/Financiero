# Sprint 9 Decision Matrix

## Decision criteria

- Remove `BLOCKED_DEPENDENCY` before expanding product scope.
- Keep Portal capabilities Portal-owned.
- Use synthetic data only.
- Preserve no-production guardrails.
- Prefer work that increases verifiable E2E evidence.

## Options

| Option | Description | Dependencies | Risk | Effort | Recommendation |
|---|---|---|---|---|---|
| A. Stabilize real E2E infrastructure | Bring up shared SQL, Portal Gateway/Shell, health PASS and preflight PASS. | PortalCorporativo runtime, shared SQL. | Medium; infra coordination. | Medium | Recommended first if `BLOCKED_DEPENDENCY` remains. |
| B. External approval operations UX | Filters, search, history, reports and audit views without productivo. | Existing approval workflow. | Low; could distract from E2E. | Medium | Second, after E2E PASS. |
| C. Controlled productization with synthetic data | Purchases/voided QA scenarios, no SRI real, no ATS official, no certs. | E2E baseline and QA env. | Medium-high; easy to overread as production. | High | Defer until E2E PASS. |
| D. Portal Content/File real contract | Real Portal metadata references, no storage propio. | Portal Content/File contract. | Medium; contract drift. | Medium | Good after Option A. |
| E. Portal Notification real contract | Portal-owned controlled delivery, no SMTP/Teams propio. | Portal Notification contract. | Medium; privacy/consent. | Medium | Good after Option A/D. |

## Final recommendation

Sprint 9 should select Option A: stabilize real E2E infrastructure. Once PASS evidence exists, choose D or E to reduce Portal boundary risk, then B/C for broader operational UX and controlled synthetic QA.

Control tokens: No SRI Production; No official ATS; No legal-final RIDE; No productive XAdES. Financiero remains not production-ready until shared SQL and Portal Gateway/Shell have PASS evidence.

## Sprint 9 P1 execution note

Option A was started. The first real execution remains `BLOCKED_DEPENDENCY`: shared SQL TCP on `host.docker.internal:21433`, Portal Gateway `localhost:8082` and Portal Shell live evidence are unavailable. Continue Option A before selecting B, C, D or E.
