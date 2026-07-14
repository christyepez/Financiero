# ADR-011 - Portal Content/File HTTP Upload Contract

## Estado

Aprobado para Sprint 4 P2.

## Contexto

Sprint 4 P1 dejó readiness de Content/File. P2 necesita preparar el contrato HTTP sin asumir que Portal ya expone endpoint productivo estable.

## Decisión

Financiero incorpora un cliente HTTP productivo-ready detrás de `IPortalContentFileClient`. El adapter trabaja en `dryRun=true` por defecto y solo realiza POST cuando se configure explícitamente `dryRun=false`.

Headers esperados:

- `Authorization: Bearer <token>` si existe.
- `X-Tenant-Id`.
- `X-Correlation-Id`.
- `X-Source-System: Financiero`.
- `Content-Type: application/json`.

## Reglas

- Base URL y upload path se validan antes de enviar.
- Token real no se inventa ni se versiona.
- Production sin token y `authRequired=true` falla si `dryRun=false`.
- `sendPayloads=false` excluye `payloadBase64`.
- Production con payloads requiere aprobación explícita.

## Consecuencias

- No hay storage documental propio.
- No hay migración.
- Tests usan `HttpMessageHandler` falso; no llaman Portal real.
- Upload real queda pendiente de contrato estable de Portal.
