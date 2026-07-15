# ADR-014 - Purchases and Voided Documents Foundation for ATS Readiness

## Decisión

Crear en Financiero una foundation propia para compras tributarias y documentos anulados, integrada con ATS readiness, sin implementar ATS oficial ni duplicar capacidades transversales del Portal.

## Contexto

Sprint 4 cerró readiness RIDE/ATS con gaps explícitos para compras y anulados. Para reducir esos gaps sin activar producción, Financiero necesita persistir datos fiscales propios de compras/anulados en su base lógica, manteniendo PortalCorporativo como dueño de Security, Audit, Outbox, Configuration, Notification, Gateway y Content/File.

## Consecuencias

- Se agrega migración `012_purchases_voided_documents_foundation.sql` en schema `financial`.
- ATS readiness y ATS official design pueden reportar conteos foundation de compras/anulados.
- Los gaps `ats.purchases.module_missing` y `ats.voided_documents.model_missing` se mantienen cuando no existen datos del periodo.
- La generación XML ATS oficial queda bloqueada hasta revisión tributaria/legal y catálogos oficiales versionados.

## Restricciones

- No XML real en base de datos.
- No secretos/certificados.
- No SQL Server propio.
- No auditoría ni outbox propios.
- No frontend en este paquete.
