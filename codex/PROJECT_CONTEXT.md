# Contexto de Financiero

## Propósito

Financiero implementa contabilidad y cumplimiento tributario/SRI. Es un dominio independiente y consumidor de PortalCorporativo; CodexCommonAgents define su orquestación y clasificación Portal-first.

## Ownership del dominio

Financiero posee plan de cuentas, períodos fiscales, asientos y detalles, motor contable, facturación, notas, retenciones, liquidaciones, guías, documentos electrónicos, reglas tributarias, integración SRI y reporting fiscal. No posee capacidades corporativas transversales.

## Relación con PortalCorporativo

- REUSE: Gateway, health, logging y correlationId.
- EXTEND: recursos/permisos Security, Menu, Configuration y Workers cuando exista proceso financiero específico.
- ADAPT: Audit, Notification y contratos Outbox/Inbox.
- BLOCKED hasta Portal Sprint 2: Content/File, Reporting, Integration productiva, Angular Shell e IdP productivo.

## Adaptadores previstos

`FinancialAuditAdapter`, `FinancialNotificationAdapter`, Outbox por bounded context, Inbox para respuestas externas y, posteriormente, adaptadores Content/File, Reporting y SriIntegration.

## Límites

Cada bounded context financiero posee su modelo y almacenamiento. La comunicación externa ocurre mediante API, adaptador o evento versionado con correlationId. Sprint 1 solo cubre Contabilidad Core; excluye facturación, SRI productivo, firma, documentos, reporting y frontend.

## No duplicación

No crear login, usuarios, roles globales, policies, Gateway, motores de Menu/Configuration/Audit/Notification, archivos genéricos, reporting transversal, shell, health o logging. No consultar bases del portal, compartir tablas entre contextos, guardar secretos/certificados en código ni autorizar solo en frontend.
