# Contratos de integración con PortalCorporativo

| Capacidad | Clasificación | Contrato de consumo | Regla |
|---|---|---|---|
| Security | REUSE/EXTEND | JWT firmado, recursos y claims `permission` | Registrar `financiero.*`; no duplicar identidad. |
| Menu | EXTEND | Módulo, rutas, acciones y permission code | UI no sustituye autorización backend. |
| Configuration | EXTEND | Claves con scope global/tenant/module/user | Sin secretos ni reglas contables. |
| Audit | ADAPT | API append-only con `portal.audit.write` | Allowlist, redacción y correlationId. |
| Notification | ADAPT | Plantilla + destinatarios + idempotencyKey | No crear motor de entrega/retry. |
| Outbox/Inbox | ADAPT/EXTEND | Envelope versionado; idempotencia | Outbox en DB financiera y misma transacción local. |
| Gateway | REUSE | Entrada única HTTP | Tokens nunca por URL. |
| Workers | EXTEND | Procesos financieros específicos sobre contratos comunes | Sin worker genérico duplicado. |
| Health/logging/correlationId | REUSE | Convenciones Portal | No registrar secretos, certificados o PII. |
| Content/File | BLOCKED | Contrato pendiente Portal Sprint 2 | XML/RIDE/adjuntos no se resuelven con repositorio genérico propio. |
| Reporting | BLOCKED | Contrato pendiente Portal Sprint 2 | Reporting transversal no se duplica. |
| Integration productiva | BLOCKED | Transporte/contrato pendiente | SRI productivo diferido. |
| Angular Shell | BLOCKED | Shell pendiente | Frontend final diferido. |
| IdP productivo | BLOCKED | OIDC/OAuth2 pendiente | Login productivo diferido. |

## Matriz final

- REUSE: Gateway, health, logging, correlationId.
- EXTEND: recursos/permisos Security, Menu, Configuration y Workers cuando aplique.
- ADAPT: Audit, Notification, Outbox/Inbox.
- CREATE: plan de cuentas, períodos, asientos, detalles, motor contable y catálogos exclusivamente financieros.
- BLOCKED: almacenamiento XML/RIDE, reporting fiscal transversal, integración SRI productiva, firma/custodia y frontend final.
