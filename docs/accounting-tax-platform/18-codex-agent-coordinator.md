# 18 - Codex Agent Coordinator

## Portal-First Accounting, Billing, Tax Ecuador and SRI Integration

## Objetivo

Coordinar la implementación del módulo financiero, contable, tributario Ecuador y SRI como extensión del repositorio `PortalCorporativo`.

Repositorio base reutilizable:

```text
https://github.com/christyepez/PortalCorporativo
```

## Regla central

Antes de crear cualquier componente, Codex debe validar si el portal ya tiene una capacidad equivalente.

Clasificación obligatoria:

```text
REUSE   = usar componente del portal.
EXTEND  = extender configuración, permisos, catálogos o menús del portal.
ADAPT   = crear adaptador hacia una API o servicio del portal.
CREATE  = crear componente propio del dominio financiero/SRI.
BLOCKED = no crear hasta revisar el portal.
```

## Componentes del portal que se deben priorizar

```text
Security API
Menu API
Integration API
Reporting API
Audit API
Notification API
Content API
Catalog API
Configuration API
API Gateway
Docker Compose
SQL Outbox
Workers
Portal Angular
```

## Agentes

```text
Coordinator Agent
Portal Capability Discovery Agent
Portal Reuse Architect Agent
Security Extension Agent
Audit Reuse Agent
Notification Reuse Agent
UI Portal Integration Agent
Accounting Domain Agent
Billing Domain Agent
Tax Ecuador Agent
Electronic Documents Agent
SRI Integration Agent
Transactional Bus Agent
Integration Adapters Agent
Database Agent
DevOps Agent
QA Agent
Documentation Agent
```

## Sprint 0 obligatorio

No se debe implementar dominio financiero hasta completar Sprint 0.

Entregables de Sprint 0:

```text
docs/portal-reuse/portal-capability-inventory.md
docs/portal-reuse/portal-reuse-matrix.md
docs/portal-reuse/do-not-duplicate-list.md
docs/portal-reuse/portal-extension-plan.md
```

## Matriz base

| Capacidad | Decisión inicial | Acción |
|---|---|---|
| Login | REUSE | Usar Security API |
| Usuarios | REUSE | No crear usuarios financieros |
| Roles | EXTEND | Agregar roles financieros |
| Permisos | EXTEND | Agregar permisos contables/SRI |
| Menús | EXTEND | Registrar menús financieros |
| Auditoría | ADAPT | Publicar eventos hacia Audit API |
| Notificaciones | ADAPT | Publicar eventos hacia Notification API |
| Reportes | EXTEND | Usar Reporting API |
| Catálogos | EXTEND | Usar Catalog API cuando aplique |
| Configuración | EXTEND | Usar Configuration API |
| Integraciones | EXTEND/ADAPT | Usar Integration API |
| Contenido | ADAPT | Usar Content API para XML/RIDE si aplica |
| API Gateway | REUSE | Registrar APIs financieras |
| Outbox/Workers | EXTEND | Reutilizar si es extensible |
| Plan de cuentas | CREATE | Dominio financiero |
| Asientos contables | CREATE | Dominio financiero |
| Facturación | CREATE | Dominio financiero |
| Retenciones | CREATE | Dominio tributario Ecuador |
| XML SRI | CREATE | Dominio SRI |
| Firma electrónica | CREATE | Dominio SRI |
| RIDE | CREATE | Documentos electrónicos |
| ATS | CREATE | Dominio tributario Ecuador |

## Orden de trabajo

```text
1. Revisar capacidades existentes del portal.
2. Generar inventario real.
3. Actualizar matriz de reutilización.
4. Definir contratos con APIs del portal.
5. Crear adaptadores para Audit, Notification, Security, Menu, Configuration y Content.
6. Implementar módulos financieros propios.
7. Integrar UI dentro de Portal Angular.
8. Validar que no exista duplicidad.
```

## Reglas de bloqueo

Codex no debe continuar si:

```text
No existe clasificación REUSE/EXTEND/ADAPT/CREATE/BLOCKED.
Se intenta duplicar login, usuarios, menús, auditoría o notificaciones.
Se intenta guardar secretos en código.
Se intenta hardcodear catálogos SRI o porcentajes tributarios.
Se intenta llamar SRI directamente desde controladores.
```

## Prompt maestro

```text
Actúa como Coordinator Agent del repositorio Financiero.
Aplica estrategia Portal-First.
Primero ejecuta Sprint 0 y genera los documentos de reutilización.
No implementes código financiero hasta completar Sprint 0.
Reutiliza componentes del PortalCorporativo siempre que sea posible.
```
