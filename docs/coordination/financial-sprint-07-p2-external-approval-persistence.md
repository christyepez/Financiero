# Sprint 7 P2 - External Approval Workflow Persistence Foundation

## Objetivo

Persistir solicitudes foundation de aprobación externa con metadata, estados y referencias sanitizadas de evidencia, sin almacenar archivos ni habilitar producción SRI/ATS/RIDE/XAdES.

## Implementado

- Entidad `ExternalApprovalRequest` y agregados dependientes.
- Migración idempotente `013_external_approval_workflow_foundation.sql`.
- Servicios command/query.
- Endpoints `/api/financial/external-approval-requests`.
- Audit/Outbox foundation mediante adapters Portal existentes.
- UI Angular foundation con comandos gated.

## Qué se guarda

- Scope, estado, título, período opcional.
- Requisitos foundation.
- Referencias metadata: provider, referenceId, displayName, hash opcional, contentType opcional.
- Decisiones foundation y timeline.

## Qué no se guarda

- Archivos.
- XML.
- Certificados.
- Base64.
- URLs con tokens/querystring sensible.
- Evidencia real o PII completa.

## Límites

`ApprovedFoundation` no equivale a aprobación legal/tributaria real y no habilita producción.
