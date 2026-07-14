# ADR-013 - Tax/Legal Review Gap Readiness

Decision: keep Sprint 4 P4 as calculated readiness/gap analysis only.

Rationale:
- RIDE final and ATS official enablement require external Ecuador tax/legal validation.
- Evidence and approvals must not be stored until ownership, retention and audit rules are defined.
- The system must not imply official compliance before expert review.

Consequences:
- Endpoints are read-only and use `financial.electronicdocuments.read`.
- No migration, approval workflow or evidence upload is introduced.
- Disclaimers are mandatory in responses.
- Production enablement remains blocked until official catalogs, schemas, SRI evidence, security gate and operational runbook are approved.
