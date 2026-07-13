# ADR-005 - SRI Secure Integration Readiness

## Estado

Aprobado para Sprint 2 P4.

## Decisión

La integración real con certificados, SRI Test y Portal Content/File se habilitará solo detrás de puertos seguros, configuración externa y gates explícitos.

- Secretos: `ISecretStoreClient`.
- Certificados: `SecretStoreCertificateProvider`.
- SRI Test: dry-run por defecto; Production bloqueado.
- Portal Content/File: contrato productivo-ready sin storage local propio.
- Sanitización: XML, claves de acceso, identificaciones y errores se enmascaran.

## Consecuencias

P4 permite validar configuración y contratos sin exponer datos tributarios ni activar producción.
