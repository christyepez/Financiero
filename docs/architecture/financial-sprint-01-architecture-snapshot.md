# Financial Sprint 1 Architecture Snapshot

## C4 textual — contenedores

```text
Usuario/Cliente
  -> PortalCorporativo Gateway
    -> Financiero API
      -> Financiero Application
        -> Financiero Domain
      -> Financiero Infrastructure
        -> SQL Server común / FinancieroDb
        -> Portal ports: Security, Configuration, Audit, Outbox, Notification
```

## C4 textual — componentes

```text
Financiero API
  - Accounts endpoints
  - Fiscal Years endpoints
  - Fiscal Periods endpoints
  - Journal Entries endpoints
  - Health endpoints

Application
  - ChartOfAccountsService
  - FiscalYearsService
  - FiscalPeriodsService
  - JournalEntriesService
  - Permission policies

Domain
  - Account
  - FiscalYear
  - FiscalPeriod
  - JournalEntry
  - JournalEntryLine

Infrastructure
  - EF Core repositories
  - FinancialDbContext
  - Migration runner
  - Audit/Outbox/Configuration adapters
  - SQL readiness health check
```

## PortalCorporativo vs Financiero

PortalCorporativo conserva ownership transversal. Financiero consume o adapta contratos, pero no copia ni reemplaza capabilities del Portal.

## Componentes reutilizados de Portal

- Gateway.
- Security model.
- Configuration port.
- Audit port.
- Outbox port.
- Health/logging/correlation.
- SQL Server común local.

## Componentes propios de Financiero

- Accounts.
- Fiscal Years.
- Fiscal Periods.
- Journal Entries.
- Accounting Sequences.
- Migration Runner.
- Smoke tests.

## Decisión de no duplicar Portal

Financiero no crea Security, Audit, Notification, Configuration, Gateway ni SQL Server propios. Solo define permisos, metadata y adaptadores necesarios para operar como consumidor.

## Base de datos

- SQL Server común: PortalCorporativo local.
- Base lógica separada: `FinancieroDb`.
- Schema propio: `financial`.

## Flujos principales

### Crear cuenta

Cliente llama endpoint de cuentas, Security valida permiso, Application aplica reglas de código/jerarquía, Infrastructure persiste en `financial.accounts` y emite Audit/Outbox por puertos.

### Abrir período

Cliente abre año/período fiscal con permiso específico. El dominio valida rangos y estado; la mutación queda auditada.

### Postear asiento

Cliente crea asiento y líneas. Al postear, Application valida período abierto, cuentas activas de movimiento y partida doble. Infrastructure reserva número con lock SQL y persiste estado Posted.

### Reversar asiento

Cliente solicita reverso de un asiento Posted. El dominio crea asiento compensatorio y marca el original como Reversed.

### Aplicar permisos

En Development se permite `X-Dev-Permissions`. En Production se usan claims JWT de Portal/IdP. Financiero no administra identidades.

### Ejecutar migraciones

Con `Database__RunMigrations=true`, el runner aplica scripts ordenados en `database/migrations/financial` y registra versiones en `financial.schema_versions`.
