# AGENTS - Financiero

## Propósito

Este archivo define cómo Codex debe operar en este repositorio.

El objetivo es implementar el módulo financiero/contable/tributario/SRI como extensión de `PortalCorporativo`, reutilizando sus APIs y componentes transversales.

## Repositorios relacionados

```text
PortalCorporativo: https://github.com/christyepez/PortalCorporativo
Financiero: https://github.com/christyepez/Financiero
CodexCommonAgents: https://github.com/christyepez/CodexCommonAgents
```

## Lectura mínima

Codex debe leer primero:

1. `README.md`.
2. `codex/PROJECT_CONTEXT.md`.
3. `docs/accounting-tax-platform/18-codex-agent-coordinator.md`.
4. `PortalCorporativo/codex/REUSABLE_CAPABILITIES.md` si está disponible.
5. `CodexCommonAgents/AGENTS.md` y playbook aplicable cuando el repo común esté disponible.

No leer todo el repositorio si la tarea no lo requiere.

## Regla obligatoria

Antes de crear cualquier componente, Codex debe revisar si el portal ya tiene una capacidad equivalente.

Clasificación obligatoria por componente:

```text
REUSE   = usar directamente componente del portal.
EXTEND  = extender configuración/catálogos/permisos del portal.
ADAPT   = crear adaptador hacia una API o servicio del portal.
CREATE  = crear porque es dominio financiero/SRI y no existe en portal.
BLOCKED = no crear hasta revisar portal.
```

## Prohibido duplicar

No crear de nuevo:

- Login.
- Usuarios.
- Roles.
- Permisos globales.
- Motor de menús.
- Auditoría visual.
- Centro de notificaciones.
- Envío genérico de correos.
- Configuración visual.
- Catálogos generales.
- Gestión de contenido.
- Dashboard shell.
- API Gateway.
- Outbox genérico si el portal ya lo tiene implementado.

## Componentes propios permitidos

Solo se crean como dominio financiero:

- Plan de cuentas.
- Periodos fiscales.
- Asientos contables.
- Motor de contabilización.
- Facturas.
- Notas de crédito.
- Notas de débito.
- Retenciones.
- Liquidaciones de compra.
- Guías de remisión.
- Catálogos tributarios SRI.
- Motor tributario Ecuador.
- Generador XML SRI.
- Firma electrónica XML.
- Envío y autorización SRI.
- RIDE.
- ATS.
- Reportes fiscales.
- Adaptadores ERP/CRM.

## Agente coordinador

El agente coordinador debe usar:

```text
/docs/accounting-tax-platform/18-codex-agent-coordinator.md
```

## Salida esperada de cada agente

Cada agente debe reportar:

```text
Agent:
Task:
Portal Capability Checked:
Reuse Classification:
Portal Components Reused:
Portal Components Extended:
New Components Created:
Reason for New Components:
Files Created:
Files Modified:
Audit Impact:
Notification Impact:
Security Impact:
Menu Impact:
Storage Impact:
Tests Added:
Risks:
Next Step:
```
