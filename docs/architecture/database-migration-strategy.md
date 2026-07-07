# Database Migration Strategy

## Estado actual

El entorno local usa `EnsureCreated` más SQL raw idempotente para crear el esquema `financial` en la base lógica `FinancieroDb`, reutilizando el SQL Server común de Portal.

## Decisión P4

No se introduce EF Migrations todavía para no romper Docker ni el flujo local. Se documenta transición obligatoria antes de producción.

## Secuencia versionada objetivo

1. `001_foundation.sql`: schema `financial`, health/base.
2. `002_chart_of_accounts.sql`: `financial.accounts`.
3. `003_fiscal_periods.sql`: `financial.fiscal_years`, `financial.fiscal_periods`.
4. `004_journal_entries.sql`: `financial.journal_entries`, `financial.journal_entry_lines`, `financial.accounting_sequences`.
5. `005_hardening_indexes.sql`: índices de integridad y concurrencia.

## Reglas

- No crear SQL Server por dominio.
- Mantener bases lógicas separadas: `PortalCorporativoDb`, `FinancieroDb`, `CrmDb`.
- No compartir tablas entre Portal y Financiero.
- Todo script debe ser idempotente o versionado con tabla de historial.

## Deuda P5

Evaluar EF Migrations vs scripts SQL versionados y agregar pruebas de concurrencia para `financial.accounting_sequences`.
