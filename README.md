# Financiero

Dominio de contabilidad y cumplimiento tributario/SRI, consumidor de PortalCorporativo.

Estado: P4 Portal Adapters / Runtime Security Integration / Accounting Hardening implementado sobre el bootstrap técnico. La solución .NET 8, Clean Architecture, base lógica `FinancieroDb`, health, JWT, logging/correlationId, autorización runtime por permisos, adaptadores Portal dev, Plan de Cuentas, Años/Periodos fiscales, Asientos contables y pruebas están preparados.

Documentos principales:

- `codex/PROJECT_CONTEXT.md`
- `codex/PORTAL_INTEGRATION_CONTRACTS.md`
- `docs/architecture/accounting-core-architecture.md`
- `docs/architecture/decisions/adr-001-financial-portal-integration.md`
- `docs/architecture/decisions/adr-002-accounting-core.md`
- `docs/coordination/financial-sprint-01-accounting-core.md`
- `docs/coordination/financial-sprint-01-p1-chart-of-accounts.md`
- `docs/coordination/financial-sprint-01-p2-fiscal-periods.md`
- `docs/coordination/financial-sprint-01-p3-journal-entries.md`
- `docs/coordination/financial-sprint-01-p4-portal-adapters-hardening.md`
- `docs/security/runtime-permission-strategy.md`
- `docs/architecture/database-migration-strategy.md`

No duplicar capacidades Portal ni acceder a sus bases. En local se reutiliza el único SQL Server de PortalCorporativo y Financiero mantiene su propia base lógica `FinancieroDb`. El código actual contiene Plan de Cuentas, Fiscal Periods y Journal Entries; no contiene SRI, facturación o frontend.

Ejecución y variables: `docs/coordination/financial-local-development.md`.
