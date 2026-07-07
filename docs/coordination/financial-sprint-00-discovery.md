# Sprint 0 — Financial Portal Discovery

## Resultado

PortalCorporativo permanece owner de plataforma; Financiero es owner de contabilidad y SRI. La clasificación Portal-first queda aprobada en `codex/PORTAL_INTEGRATION_CONTRACTS.md` y permite iniciar Contabilidad Core sin duplicaciones.

## Reutilización y extensión

- Reutilizar Gateway, health, logging y correlationId.
- Extender Security con recursos/permisos `financiero.*`, Menu con módulo financiero y Configuration con metadata no secreta.
- Extender Workers solo para procesos financieros concretos reutilizando convenciones comunes.

## Adaptadores requeridos

- Audit para altas/cambios, contabilización, reversos, cierres y aprobaciones.
- Notification para alertas; proveedor productivo diferido.
- Outbox local e Inbox para procesamiento idempotente.
- Futuros adaptadores Content/File, Reporting e Integration/SRI.

## Bloqueos

Content/File, Reporting, Integration productiva, Angular Shell e IdP dependen de Portal Sprint 2. Firma electrónica/custodia requiere ADR y threat model específicos.

## Riesgos y decisiones pendientes

Límites demasiado finos entre contextos, invariantes contables incompletas, períodos concurrentes, reversos no auditables, duplicados asíncronos, PII fiscal y custodia de certificados. Antes de Sprint 1 deben cerrarse moneda base, jerarquía del plan, estados de período/asiento, política de reverso, numeración e idempotencia.

## Primer sprint implementable

ChartOfAccounts + FiscalPeriods + JournalEntries: plan jerárquico, períodos, asiento borrador/contabilizado/reversado, partida doble, Audit adapter, Outbox local, permisos/Menu/Configuration y pruebas de dominio. Se excluyen facturación, impuestos documentales, SRI, firma, archivos, reportes y frontend.
