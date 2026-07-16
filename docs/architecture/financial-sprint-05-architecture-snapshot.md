# Financial Sprint 5 Architecture Snapshot

```text
Financiero API
  -> Purchases / Voided foundation
  -> ATS support mapping
  -> Financial tax catalog foundation
  -> ATS XML foundation gated
  -> External approval readiness foundation
  -> Portal Security/Audit/Outbox contracts
  -> FinancieroDb on shared SQL Server
```

## Boundaries

- Financiero owns fiscal domain models.
- Portal owns Security, Audit, Outbox, Content/File and shared local platform services.
- Approval workflow is advisory/read-only and does not mutate configuration.

## Production gates

Official ATS, legal RIDE, XAdES, SRI Test/Production and Content/File payloads remain blocked until external approval.
