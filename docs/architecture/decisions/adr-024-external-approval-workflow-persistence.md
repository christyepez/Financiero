# ADR-024 - External Approval Workflow Persistence Foundation

## Status

Accepted.

## Context

El workflow de aprobaciones externas era read-only/advisory. Sprint 7 P2 requiere persistir metadata controlada sin duplicar Portal Content/File, Notification, Audit ni Security.

## Decision

- Crear persistencia foundation para solicitudes, requisitos, referencias de evidencia, decisiones y timeline.
- Guardar solo metadata sanitizada.
- Reusar permisos existentes `financial.electronicdocuments.read/manage`.
- Reusar Portal Audit/Outbox adapters.
- Mantener Content/File y Notification como integraciones futuras.
- Bloquear producción fiscal aunque exista `ApprovedFoundation`.

## Consequences

- Hay trazabilidad local foundation para preparar revisiones externas.
- No existe storage documental propio.
- No existe aprobación productiva automática.
- Portal sigue siendo owner de Security, Content/File, Notification y Audit.
