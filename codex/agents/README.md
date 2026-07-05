# Codex Agents

Este directorio define los roles que Codex debe usar para ejecutar la implementación por agentes.

## Coordinator Agent

Coordina la ejecución, valida dependencias y aplica la estrategia Portal-First.

## Portal Capability Discovery Agent

Revisa `PortalCorporativo` y genera el inventario de capacidades reutilizables.

## Portal Reuse Architect Agent

Define si cada capacidad se maneja como REUSE, EXTEND, ADAPT, CREATE o BLOCKED.

## Security Extension Agent

Extiende permisos y roles del portal para el dominio financiero.

## Audit Reuse Agent

Define el adaptador para publicar eventos financieros hacia Audit API del portal.

## Notification Reuse Agent

Define el adaptador para publicar notificaciones financieras hacia Notification API del portal.

## UI Portal Integration Agent

Integra menús, rutas, grids y acciones financieras dentro del Portal Angular.

## Accounting Domain Agent

Implementa plan de cuentas, periodos, asientos, reversos y cierre contable.

## Billing Domain Agent

Implementa facturas, notas de crédito, notas de débito y ciclo de facturación.

## Tax Ecuador Agent

Implementa IVA, retenciones, catálogos SRI, ATS y reportes tributarios.

## Electronic Documents Agent

Implementa XML, validación XSD, clave de acceso, firma, RIDE y archivo documental.

## SRI Integration Agent

Implementa recepción, autorización, anulación, reproceso y contingencia SRI.

## Transactional Bus Agent

Revisa si el portal tiene outbox/workers reutilizables y define la extensión para eventos financieros.

## Integration Adapters Agent

Implementa conectores ERP/CRM: SAP, Dynamics, Oracle, QuickBooks, Salesforce y genéricos.

## Database Agent

Crea solo tablas propias del dominio financiero, tributario y SRI.

## DevOps Agent

Extiende Docker Compose, CI/CD y secretos siguiendo patrones del portal.

## QA Agent

Valida comportamiento funcional y no duplicidad de capacidades del portal.

## Documentation Agent

Mantiene documentos, ADRs, contratos, matriz de reutilización y backlog.

## Reporte obligatorio

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
