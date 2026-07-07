# Financial Database Inventory

## Base y schema

- SQL Server: contenedor común de PortalCorporativo.
- Base lógica: `FinancieroDb`.
- Schema: `financial`.

## Tablas

- `financial.accounts`: plan de cuentas.
- `financial.fiscal_years`: años fiscales.
- `financial.fiscal_periods`: períodos fiscales.
- `financial.journal_entries`: cabecera de asientos.
- `financial.journal_entry_lines`: líneas de asiento.
- `financial.accounting_sequences`: numeración contable por tenant/año/tipo.
- `financial.schema_versions`: historial de scripts aplicados.

## Índices principales

- Cuentas por tenant/código.
- Años fiscales por tenant/año.
- Períodos por año fiscal y rango.
- Asientos por tenant/número.
- Asientos por tenant/período/estado.
- Secuencias por tenant/año/tipo.

## Restricciones principales

- Unicidad de códigos contables por tenant.
- Unicidad de año fiscal por tenant.
- Unicidad de número de asiento por tenant.
- Relación de línea con asiento.
- Relación de asiento con período fiscal.
- Secuencia única para evitar duplicados bajo concurrencia.

## Relaciones principales

- `fiscal_periods` pertenece a `fiscal_years`.
- `journal_entries` referencia `fiscal_periods`.
- `journal_entry_lines` pertenece a `journal_entries`.
- `journal_entry_lines` referencia cuentas por id lógico.

## Migraciones versionadas

Scripts actuales:

- `001_schema_financial.sql`
- `002_chart_of_accounts.sql`
- `003_fiscal_periods.sql`
- `004_journal_entries.sql`
- `005_accounting_sequences.sql`
- `006_indexes_constraints_hardening.sql`

## Runner de migraciones

`FinancialDatabaseMigrationRunner` aplica scripts pendientes desde `database/migrations/financial` y registra versiones en `financial.schema_versions`.

## Health readiness

`/health/ready` valida conexión y existencia de tablas core antes de reportar estado saludable.

## Regla SQL común

Financiero no crea SQL Server propio. Cada dominio mantiene base lógica separada dentro del SQL Server común local.
