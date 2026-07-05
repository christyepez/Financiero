# Contexto del proyecto Financiero

## Propósito

Implementar el módulo financiero, contable, tributario Ecuador y SRI como extensión de `PortalCorporativo`.

## Repositorios relacionados

```text
PortalCorporativo: https://github.com/christyepez/PortalCorporativo
Financiero: https://github.com/christyepez/Financiero
CodexCommonAgents: https://github.com/christyepez/CodexCommonAgents
```

## Estrategia

La estrategia es Portal-First:

1. Revisar capacidades del portal.
2. Clasificar cada necesidad como REUSE, EXTEND, ADAPT, CREATE o BLOCKED.
3. Reutilizar capacidades transversales del portal.
4. Crear solo componentes propios del dominio financiero/SRI.
5. Registrar decisiones en la matriz de reutilización.

## Capacidades del portal a revisar primero

- Security API.
- Menu API.
- Configuration API.
- Catalog API.
- Audit API.
- Notification API.
- Content/File API.
- Reporting API.
- Integration API.
- API Gateway.
- SQL Outbox.
- Workers.
- Portal Angular Shell.

## Dominio propio permitido

- Plan de cuentas.
- Periodos fiscales.
- Asientos contables.
- Motor de contabilización.
- Documentos electrónicos.
- Facturación electrónica.
- Retenciones.
- Notas de crédito.
- Notas de débito.
- Liquidaciones de compra.
- Guías de remisión.
- Catálogos tributarios SRI.
- Generación XML.
- Firma electrónica XML.
- Envío y autorización SRI.
- RIDE.
- ATS.
- Reportes fiscales.

## Reglas de bloqueo

Codex no debe avanzar si:

- No existe clasificación REUSE/EXTEND/ADAPT/CREATE/BLOCKED.
- Se intenta duplicar login, usuarios, roles, menús, auditoría o notificaciones.
- Se intenta guardar secretos en código.
- Se intenta quemar catálogos SRI o porcentajes tributarios.
- Se intenta llamar SRI directamente desde controladores.

## Salida obligatoria

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
Tests Added:
Risks:
Next Step:
```
