# Contratos de integración con PortalCorporativo

| Capacidad | Clasificación | Contrato de consumo | Estado PROD local |
|---|---|---|---|
| Security | REUSE/EXTEND | JWT firmado, recursos y claims `permission` | ACTIVO |
| Menu | EXTEND | Módulo, rutas, acciones y permission code | DISPONIBLE |
| Configuration | EXTEND | Claves con scope global/tenant/module/user | ACTIVO |
| Audit | ADAPT | API append-only con `portal.audit.write` | ACTIVO |
| Notification | ADAPT | Plantilla + destinatarios + idempotencyKey | ACTIVO |
| Outbox/Inbox | ADAPT/EXTEND | Envelope versionado; idempotencia | ACTIVO |
| Gateway | REUSE | Entrada única HTTP `/api/financial/**` | ACTIVO |
| Workers | EXTEND | Procesos financieros específicos sobre contratos comunes | DISPONIBLE |
| Health/logging/correlationId | REUSE | Convenciones Portal | ACTIVO |
| Content/File | REUSE/ADAPT | Portal Content API `/api/content/**` | ACTIVO para PROD local |
| Reporting | REUSE/ADAPT | Portal Reporting API `/api/reporting/**` | DISPONIBLE |
| Integration | ADAPT | Portal Integration API + Outbox/Inbox | ACTIVO local; transporte externo real bloqueado |
| Angular Shell | REUSE/EXTEND | Portal Shell registra módulo Financiero | ACTIVO en Portal |
| IdP productivo externo | DEFERRED | OIDC/OAuth2 | No requerido para PROD local; JWT local común |

## Matriz vigente

- REUSE: Gateway, Security, health, logging, correlationId, Content/File, Reporting y Shell.
- EXTEND: recursos/permisos Security, Menu, Configuration y Workers cuando aplique.
- ADAPT: Audit, Notification, Outbox/Inbox e Integration.
- CREATE: plan de cuentas, períodos, asientos, detalles, motor contable, SRI y catálogos exclusivamente financieros.
- DEFERRED EXTERNAL: envío SRI real, certificados productivos, proveedores cloud/Internet y secretos productivos.

## PROD local

`docker-compose.portal-prod-local.yml` conecta `financial-api` a `portal-local-network`, no publica el API al host y usa `api-gateway:8080` como frontera Portal. Comparte issuer/audience/signing key JWT con Portal y mantiene SRI real deshabilitado.
