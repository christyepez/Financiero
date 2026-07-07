# Database Migration Strategy

## Estado actual

El entorno local usa `EnsureCreated` más SQL raw idempotente para crear el esquema `financial` en la base lógica `FinancieroDb`, reutilizando el SQL Server común de Portal.

## Decisión P4/P5

No se introduce EF Migrations todavía para no romper Docker ni el flujo local. P5 agrega scripts SQL idempotentes versionados y runner básico con tabla `financial.schema_versions`.

## Secuencia versionada objetivo

1. `001_foundation.sql`: schema `financial`, health/base.
2. `002_chart_of_accounts.sql`: `financial.accounts`.
3. `003_fiscal_periods.sql`: `financial.fiscal_years`, `financial.fiscal_periods`.
4. `004_journal_entries.sql`: `financial.journal_entries`, `financial.journal_entry_lines`, `financial.accounting_sequences`.
5. `005_hardening_indexes.sql`: índices de integridad y concurrencia.

## Runner P5

`FinancialDatabaseMigrationRunner` busca scripts en `database/migrations/financial`, omite versiones aplicadas y registra cada script en `financial.schema_versions`.

Docker local habilita:

```yaml
Database__RunMigrations: "true"
```

## Reglas

- No crear SQL Server por dominio.
- Mantener bases lógicas separadas: `PortalCorporativoDb`, `FinancieroDb`, `CrmDb`.
- No compartir tablas entre Portal y Financiero.
- Todo script debe ser idempotente o versionado con tabla de historial.

## Deuda P5

Evaluar EF Migrations vs scripts SQL versionados y agregar pruebas de concurrencia para `financial.accounting_sequences`.
