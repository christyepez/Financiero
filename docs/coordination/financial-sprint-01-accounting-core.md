# Sprint 1 — Contabilidad Core

## Objetivo

Entregar un vertical slice contable verificable para plan de cuentas, períodos y asientos, reutilizando PortalCorporativo y excluyendo SRI/facturación/frontend.

## Decisiones cerradas

Moneda base única configurable; `decimal(19,4)`; balance exacto; cuentas padre sin movimiento; período Open obligatorio; Posted inmutable; Void solo Draft; reverso compensatorio; número atómico al posting y secuencia gap-tolerant. ADR-002 gobierna conflictos.

## Paquetes y agentes

| Orden | Paquete | Agente | Entrega | Criterios de aceptación |
|---:|---|---|---|---|
| P0 | Bootstrap Financiero | DevOps Agent | solución, módulos, Compose/SQL aislado, health/logging | Build limpio; sin dominio Portal duplicado |
| P1 | Chart of Accounts | Backend + Data Agent | Account/value objects, contratos, persistencia, API, tests | árbol/código únicos; cuenta movimiento/estado probados |
| P2 | Fiscal Periods | Backend + Data Agent | años/períodos, transiciones y concurrencia | no solapamiento; close/reopen autorizado/auditado |
| P3 | Journal Entries | Backend Agent | Draft/líneas/post/void/reverse | partida doble, período/cuenta y estados probados |
| P4 | Portal adapters | Security + Integration Agent | permisos/Menu/Configuration, Audit, Outbox, Notification contracts | no DB compartida; eventos/correlationId/idempotencia |
| P5 | QA integrado | QA Agent | unitarias, integración SQL/JWT, contrato y smoke | build/tests/health/autorización y casos negativos pasan |
| P6 | Documentación | Documentation Agent | runbook, contratos y estado reutilizable | docs reflejan implementación real |

Estado: P0 completado con solución .NET 8, proyectos Clean Architecture, SQL aislado, health/JWT/logging/correlationId, contratos Portal, adaptadores dev y pruebas. P1 Chart of Accounts, P2 Fiscal Periods y P3 Journal Entries están implementados como vertical slices funcionales.

## P1 Chart of Accounts

Incluye entidad `Account`, reglas de jerarquía/código/estado, endpoints `/api/financial/accounts`, persistencia `FinancieroDb.financial.accounts`, metadata Portal Security/Menu/Configuration y adaptación Audit/Outbox. No incluye frontend, asientos, SRI ni facturación.

## P2 Fiscal Periods

Incluye entidades `FiscalYear` y `FiscalPeriod`, reglas de unicidad, rangos de fechas, no solapamiento, apertura/cierre/reapertura/bloqueo/archivo, endpoints `/api/financial/fiscal-years` y `/api/financial/fiscal-periods`, persistencia `FinancieroDb.financial.fiscal_years` y `financial.fiscal_periods`, metadata Portal y adaptación Audit/Outbox.

Además endurece P1: no se permite desactivar ni archivar cuentas padre con hijos activos.

## P3 Journal Entries

Incluye entidades `JournalEntry` y `JournalEntryLine`, estados Draft/Posted/Reversed/Voided, fuentes Manual/OpeningBalance/Adjustment/Integration/System, reglas de partida doble, validación de cuentas activas de movimiento y periodo fiscal Open, endpoints `/api/financial/journal-entries`, persistencia `financial.journal_entries`, `financial.journal_entry_lines` y `financial.accounting_sequences`, metadata Portal Security/Menu/Configuration y adaptación Audit/Outbox.

La numeración queda como secuencia por tenant/año con formato `JE-{year}-{000000}`. La prevención de desactivar/archivar cuentas usadas en asientos Posted queda preparada por `IJournalEntryRepository.HasPostedEntriesForAccountAsync` y debe conectarse en P4/P5 con una política transversal de integridad contable.

## P4 Portal Adapters / Runtime Security / Hardening

Incluye autorización runtime por permisos `financial.*` en todos los endpoints de cuentas, años fiscales, periodos fiscales y asientos. En Development se admite `X-Dev-Permissions`; en Production se ignora. Los permisos siguen siendo ownership de Portal Security.

El hardening bloquea cambios peligrosos: cuentas usadas en Posted no cambian código, no se convierten en resumen, no se desactivan ni archivan; periodos no cierran/bloquean con Draft entries si la configuración lo exige; periodos/años con Posted entries no se archivan; Journal Entries respetan configuración de void y numeración.

## Permisos propuestos

- `financial.accounts.read|manage`
- `financial.periods.read|manage|reopen`
- `financial.journal-entries.read|manage|post|reverse`
- `financial.configuration.manage`

FinancialAdmin recibe todos; Accountant gestiona/postea/revierte asientos y lee cuentas/períodos; FinancialViewer solo read; FiscalManager gestiona/reabre períodos. La asignación definitiva aplica mínimo privilegio.

## Aceptación del sprint

- APIs protegidas, tenant derivado de identidad y health anónimo.
- Invariantes ADR-002 cubiertas por pruebas unitarias y concurrencia crítica por integración.
- SQL financiero independiente; Outbox atómico en posting/cierre/cambios.
- Audit y logs correlacionados sin datos sensibles.
- Menu/Configuration registrados mediante Portal; Notification solo desacoplada.
- Cero facturación, impuestos documentales, SRI, firma, archivos, reporting o Angular.

## Riesgos

Contención de secuencia, carreras cierre/posting, jerarquías profundas, reversos mal fechados, configuración mutable y discrepancias decimales. Mitigar con constraints, optimistic concurrency, snapshot de configuración y pruebas negativas.

## Primer paquete implementable

P1 Chart of Accounts después de P0. No depende de capacidades bloqueadas de Portal Sprint 2.
