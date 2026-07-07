# Financiero

Dominio de contabilidad y cumplimiento tributario/SRI, consumidor de PortalCorporativo.

Estado: P0 Bootstrap técnico completado. La solución .NET 8, Clean Architecture, base lógica `FinancieroDb`, health, JWT, logging/correlationId, adaptadores Portal dev y pruebas están preparados. P1 Chart of Accounts aún no inicia.

Documentos principales:

- `codex/PROJECT_CONTEXT.md`
- `codex/PORTAL_INTEGRATION_CONTRACTS.md`
- `docs/architecture/accounting-core-architecture.md`
- `docs/architecture/decisions/adr-001-financial-portal-integration.md`
- `docs/architecture/decisions/adr-002-accounting-core.md`
- `docs/coordination/financial-sprint-01-accounting-core.md`

No duplicar capacidades Portal ni acceder a sus bases. En local se reutiliza el único SQL Server de PortalCorporativo y Financiero mantiene su propia base lógica `FinancieroDb`. El código actual es solo infraestructura; no contiene entidades contables, SRI, facturación o frontend.

Ejecución y variables: `docs/coordination/financial-local-development.md`.
