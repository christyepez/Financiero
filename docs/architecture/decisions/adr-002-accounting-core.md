# ADR-002: Accounting Core Foundation

- Estado: Aceptado
- Fecha: 2026-07-06

## Decisión

1. Sprint 1 usa una moneda base ISO 4217 configurable por tenant; no soporta asientos multimoneda ni conversión FX.
2. Importes se almacenan como `decimal(19,4)`. Cada línea tiene exactamente débito o crédito positivo, nunca ambos; al contabilizar los totales deben coincidir exactamente a cuatro decimales. No existe auto-balanceo ni tolerancia distinta de cero en Sprint 1.
3. Cuentas forman un árbol por tenant. Código normalizado único, máximo 32 caracteres y formato configurable; el nivel deriva del padre. Solo cuentas activas y de movimiento admiten líneas.
4. Años y períodos no se solapan dentro del tenant. Un período puede ser Draft, Open o Closed; reabrir Closed requiere `financial.periods.reopen`, motivo y auditoría. Puede haber varios períodos Open si sus fechas no se solapan.
5. El asiento inicia Draft, es editable solo en Draft y se contabiliza en un período Open según `PostingDate`. Posted es inmutable.
6. El número se asigna atómicamente al contabilizar, con secuencia monotónica por tenant, año fiscal y tipo; se exige unicidad, no ausencia absoluta de huecos.
7. Void solo aplica a Draft y conserva el registro. Un Posted se corrige mediante un nuevo asiento reverso enlazado, con líneas invertidas y fecha dentro de un período Open. El original pasa a Reversed al contabilizar el reverso.
8. Posting, reverso, cierre/reapertura y cambios de cuentas generan evento Outbox en la misma transacción. Audit se entrega mediante adaptador/evento; una indisponibilidad remota no revierte una transacción local ya confirmada.
9. Persistencia financiera es independiente; no comparte tablas ni entidades con PortalCorporativo.

## Consecuencias

Las invariantes son deterministas y auditables, con concurrencia requerida para secuencias y cierres. Multimoneda, aprobaciones, cierres automáticos y reglas tributarias se difieren.

## Decisión P1 - Chart of Accounts

Se implementa primero `ChartOfAccounts` con tabla `financial.accounts` en `FinancieroDb`. Security/Menu/Configuration se extienden mediante metadata `financial.chartofaccounts.*`; Audit usa eventos `AccountCreated`, `AccountUpdated`, `AccountActivated`, `AccountDeactivated`, `AccountArchived`; Outbox usa `FinancialAccountCreated`, `FinancialAccountUpdated` y `FinancialAccountStatusChanged`.

No se crea SQL Server, login, auditoría ni motor de notificaciones propios.

## Decisión P2 - Fiscal Periods

Se implementan `FiscalYear` y `FiscalPeriod` como segundo paquete funcional. Un año fiscal es único por tenant/año; los periodos son únicos por tenant/año/número y no se solapan. Un periodo solo abre si su año fiscal está `Open`; un año fiscal no cierra si tiene periodos abiertos; periodos `Locked` no se reabren sin permiso/regla futura especial.

Se mantiene `FinancieroDb` como base lógica separada y se reutilizan Portal Security/Menu/Configuration/Audit/Outbox mediante metadata y puertos existentes.

## Decisión P3 - Journal Entries

Se implementan `JournalEntry` y `JournalEntryLine` como foundation de asientos contables. Draft admite edición y líneas; Posted requiere mínimo dos líneas, cuentas existentes/activas/de movimiento y periodo fiscal `Open`; Posted/Reversed/Voided son inmutables para edición. Void aplica solo a Draft. Reverse crea un nuevo asiento Posted con líneas invertidas y enlaza original/reverso.

La numeración usa `financial.accounting_sequences` con scope tenant/año y formato inicial `JE-{year}-{000000}`. Es gap-tolerant; la contención/concurrencia fina queda como riesgo para pruebas de integración SQL en P5. Audit y Outbox se adaptan con eventos `JournalEntryCreated`, `JournalEntryUpdated`, `JournalEntryPosted`, `JournalEntryReversed` y `JournalEntryVoided`.
