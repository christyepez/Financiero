# Accounting Core Architecture

## Contextos Sprint 1

| Contexto | Ownership | Dependencias permitidas |
|---|---|---|
| ChartOfAccounts | Cuentas, códigos, jerarquía, tipo, vigencia y movimiento | AccountingConfiguration, Audit/Outbox ports |
| FiscalPeriods | Años, períodos y transiciones Open/Close/Reopen | Security permission, Audit/Outbox ports |
| JournalEntries | Asientos, líneas, contabilización, void y reverso | ChartOfAccounts y FiscalPeriods por interfaces de aplicación |
| AccountingCore | Orquestación de invariantes y contratos públicos | Los tres contextos; sin ownership de datos duplicado |
| AccountingConfiguration | Snapshot efectivo de moneda, códigos, numeración y reglas | Configuration API mediante adaptador/cache |

En Sprint 1 pueden desplegarse como un modular monolith con un único host y esquema lógico `financial`, preservando módulos y dependencias. No se comparten bases con Portal.

## Modelo de dominio

- `Account`: Id, TenantId, `AccountCode`, Name, `AccountType` (Asset/Liability/Equity/Income/Expense), ParentAccountId, `AccountLevel`, IsMovementAccount, Status (Active/Inactive), timestamps/version.
- `FiscalYear`: Id, TenantId, Code, StartDate, EndDate, Status.
- `FiscalPeriod`: Id, FiscalYearId, Code, StartDate, EndDate, `PeriodStatus` (Draft/Open/Closed), closed/reopened metadata.
- `JournalEntry`: Id, TenantId, FiscalPeriodId, `EntryNumber?`, PostingDate, Description, Reference, `EntrySource` (Manual/Import/System/Reversal), `EntryStatus` (Draft/Posted/Reversed/Voided), ReversalOfId, totals, version.
- `JournalEntryLine`: Id, JournalEntryId, LineNumber, AccountId, Description, Debit, Credit.
- `AccountingSequence`: TenantId, FiscalYearId, EntryType, NextValue, concurrency token.
- `AccountingConfiguration`: tenant, BaseCurrency, AccountCodePattern, MaxCodeLength, MaxLevels, EntryNumberPrefix, EntryNumberPadding, ClosingRules, RoundingTolerance fixed at zero for posting.

Value objects validate account code, money, date ranges, entry number and debit/credit. Domain objects contain no Portal entities.

## Invariantes

1. Un asiento contabilizado tiene al menos dos líneas y `ΣDebit = ΣCredit > 0` a cuatro decimales.
2. Cada línea tiene exactamente un lado positivo; cero/negativos y ambos lados están prohibidos.
3. PostingDate pertenece a un período Open del tenant.
4. Cada cuenta usada está Active y `IsMovementAccount=true`; cuentas padre no reciben movimiento.
5. Draft es editable; Posted/Reversed/Voided son inmutables.
6. Void conserva Draft; Posted solo se revierte con asiento compensatorio en período Open.
7. Código de cuenta y rangos de períodos son únicos/no solapados por tenant.
8. EntryNumber se asigna al posting de forma atómica y única; optimistic concurrency evita doble posting/cierre.
9. Moneda base única; no FX en Sprint 1.
10. Toda mutación crítica registra actor, correlationId y evento Outbox auditable.

## Contratos mínimos

| Módulo | Commands | Queries |
|---|---|---|
| Accounts | CreateAccount, UpdateAccount, ActivateAccount, DeactivateAccount | GetAccount, SearchAccounts |
| Periods | CreateFiscalYear, CreateFiscalPeriod, OpenFiscalPeriod, CloseFiscalPeriod, ReopenFiscalPeriod | GetFiscalPeriod |
| Entries | CreateJournalEntry, AddJournalEntryLine, PostJournalEntry, ReverseJournalEntry, VoidJournalEntry | GetJournalEntry, SearchJournalEntries |

Todos incluyen tenant derivado de identidad, expectedVersion en mutaciones, actor/correlationId técnicos y errores estables. `VoidJournalEntry` se admite únicamente para Draft.

## Endpoints propuestos

- `POST/PUT/GET /api/financial/accounts`, `GET /api/financial/accounts/{id}`.
- `POST /api/financial/accounts/{id}/activate|deactivate`.
- `POST /api/financial/fiscal-years`, `POST /api/financial/fiscal-periods`.
- `GET /api/financial/fiscal-periods/{id}`; `POST /{id}/open|close|reopen`.
- `POST/GET /api/financial/journal-entries`, `GET /api/financial/journal-entries/{id}`.
- `POST /api/financial/journal-entries/{id}/lines|post|reverse|void`.
- `GET /health/live`, `GET /health/ready` anónimos; restantes con JWT y permiso específico.

## Estado P1 implementado

`ChartOfAccounts` queda implementado en código con `Account`, endpoints `/api/financial/accounts`, búsqueda paginada, árbol de cuentas y persistencia EF Core en `financial.accounts`. Los eventos Audit/Outbox se emiten por puertos existentes hacia Portal/adaptadores dev.

Permisos específicos P1: `financial.chartofaccounts.read`, `create`, `update`, `activate`, `deactivate`, `archive` y `manage`.

`FiscalPeriods` queda implementado con `FiscalYear` y `FiscalPeriod`, endpoints `/api/financial/fiscal-years` y `/api/financial/fiscal-periods`, búsqueda paginada, periodo abierto por fecha y persistencia EF Core en `financial.fiscal_years` y `financial.fiscal_periods`.

Permisos específicos P2: `financial.fiscalyears.*` y `financial.fiscalperiods.*`.

`JournalEntries` queda implementado con `JournalEntry` y `JournalEntryLine`, endpoints `/api/financial/journal-entries`, operaciones Draft/update/líneas/post/reverse/void, búsqueda paginada, búsqueda por número y persistencia EF Core en `financial.journal_entries`, `financial.journal_entry_lines` y `financial.accounting_sequences`.

Permisos específicos P3: `financial.journalentries.read`, `create`, `update`, `post`, `reverse`, `void` y `manage`.

## Seguridad runtime P4

Los endpoints aplican policies por permiso específico. La API acepta permisos desde claims JWT `permission`, `permissions`, `scope`, `roles` o rol estándar. En Development, `X-Dev-Permissions` permite smoke tests sin crear login local; en Production se ignora.

Financiero no es source of truth de identidad ni permisos. Portal Security conserva ownership; Financiero solo adapta autorización en runtime.

## Datos lógicos

Tablas `financial.accounts`, `fiscal_years`, `fiscal_periods`, `journal_entries`, `journal_entry_lines`, `accounting_sequences`, `accounting_configurations` y `outbox_messages`. Índices únicos implementados: cuenta `(tenant, code)`, año `(tenant, year)`, período `(tenant, fiscalYear, number)`, asiento `(tenant, entryNumber)` y secuencia `(tenant, sequenceKey)`.

La secuencia de asientos usa prefijo/padding desde Configuration con defaults `JE` y `6`, scope tenant/año y unicidad `(TenantId, SequenceKey)`.

P5 endurece la reserva de números con transacción serializable y locks SQL `UPDLOCK,HOLDLOCK`. Readiness valida las tablas core antes de reportar `/health/ready` saludable.

## Portal

- REUSE: Gateway, health, logging, correlationId.
- EXTEND Security: recursos `financial.accounts`, `financial.periods`, `financial.journal-entries`, `financial.configuration`; acciones read/manage/post/reverse/reopen. Roles sugeridos FinancialAdmin, Accountant, FinancialViewer, FiscalManager, asignados en Portal, no definidos como identidad local.
- EXTEND Menu: Financiero → Plan de cuentas, Períodos fiscales, Asientos, Configuración.
- EXTEND Configuration: código/longitud/niveles, moneda base, cierre, numeración y tolerancia (cero en Sprint 1).
- ADAPT Audit: AccountCreated/Updated, FiscalPeriodOpened/Closed/Reopened, JournalEntryCreated/Posted/Reversed/Voided.
- ADAPT Notification: período cerrado, asiento contabilizado y fallo relevante; no es requisito para confirmar transacción.
- ADAPT/EXTEND Outbox: `AccountChangedV1`, `FiscalPeriodClosedV1`, `AccountingEntryPostedV1`; Inbox no tiene consumidor externo en Sprint 1.
- Workers: solo contratos/eventos; ningún worker productivo en Sprint 1.

BLOCKED/diferido: Billing, Tax, ElectronicDocuments, SRI productivo, firma, RIDE, ATS, Reporting, Content/File, Integration productiva y Angular Shell.
