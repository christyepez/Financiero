# Financiero

Repositorio para coordinar e implementar el módulo financiero, contable, tributario Ecuador y SRI como extensión de `PortalCorporativo`.

## Enfoque

Este repositorio usa una estrategia **Portal-First** y **Reuse-First**.

Antes de crear cualquier componente, Codex debe revisar `PortalCorporativo` y clasificar la capacidad requerida como:

```text
REUSE   = usar directamente una capacidad existente del portal.
EXTEND  = extender configuración, permisos, menús o catálogos del portal.
ADAPT   = crear un adaptador hacia una API o servicio del portal.
CREATE  = crear un componente propio del dominio financiero, contable, tributario o SRI.
BLOCKED = no implementar hasta completar revisión del portal.
```

## Repos relacionados

```text
PortalCorporativo: https://github.com/christyepez/PortalCorporativo
Financiero: https://github.com/christyepez/Financiero
CodexCommonAgents: https://github.com/christyepez/CodexCommonAgents
```

## Capacidades del portal que deben revisarse primero

- Security API.
- Menu API.
- Integration API.
- Reporting API.
- Audit API.
- Notification API.
- Content API / File API.
- Catalog API.
- Configuration API.
- API Gateway.
- Portal Angular Shell.
- Docker Compose.
- SQL Outbox y Workers.

## No duplicar

Codex no debe crear de nuevo:

- Login.
- Usuarios.
- Roles globales.
- Permisos globales.
- Motor de menús.
- Auditoría transversal.
- Centro de notificaciones.
- API Gateway.
- Configuración visual.
- Catálogos generales.
- Gestión genérica de contenido o archivos.
- Envío genérico de correos.

## Componentes propios del dominio Financiero

El repositorio puede crear componentes propios para:

- Plan de cuentas.
- Periodos fiscales.
- Asientos contables.
- Motor de contabilización.
- Facturación electrónica.
- Notas de crédito.
- Notas de débito.
- Retenciones.
- Liquidaciones de compra.
- Guías de remisión.
- Catálogos tributarios Ecuador/SRI.
- Generación XML SRI.
- Firma electrónica XML.
- Envío y autorización SRI.
- RIDE.
- ATS.
- Reportes fiscales.
- Adaptadores ERP y CRM.

## Orden recomendado para Codex

Codex debe leer en este orden:

1. `AGENTS.md`.
2. `codex/PROJECT_CONTEXT.md` si existe.
3. `codex/INSTRUCTIONS.md`.
4. `docs/accounting-tax-platform/18-codex-agent-coordinator.md`.
5. `docs/portal-reuse/portal-reuse-matrix.md` si existe.
6. `docs/portal-reuse/do-not-duplicate-list.md` si existe.
7. `docs/sprints/sprint-00-portal-discovery.md` si existe.
8. `docs/backlog/implementation-backlog.md` si existe.
9. `CodexCommonAgents/registry/reusable-portal-apis.md` cuando esté disponible.

## Primer comando para Codex

```text
Lee AGENTS.md y codex/PROJECT_CONTEXT.md. Ejecuta únicamente Sprint 00 Portal Discovery. Primero revisa PortalCorporativo y genera la matriz real de reutilización antes de crear código financiero.
```

## Estado actual

El repositorio contiene documentación, agentes, contratos, sprints y backlog inicial para que Codex dispare la implementación por fases. La prioridad es completar Sprint 0 de descubrimiento de capacidades del portal antes de implementar dominio financiero.
