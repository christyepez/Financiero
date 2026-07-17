# ADR-026 - Controlled Productization Readiness

## Status

Accepted.

## Decision

Financiero exposes read-only productization readiness for purchases and voided documents. The readiness endpoints report gates and blockers, but never mutate state or enable productive tax flows.

## Consequences

- `ApprovedFoundation` does not enable production.
- Portal Content/File remains the future owner of evidence files.
- Portal Notification remains the future owner of delivery.
- Productive activation, official tax flows, SRI submission, evidence upload and notification send stay disabled.

## Security boundary

No login, roles, token storage, certificates, XML storage, evidence storage, notification engine or SQL Server is added.
