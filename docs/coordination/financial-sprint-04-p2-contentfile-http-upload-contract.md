# Sprint 4 P2 - Portal Content/File HTTP Upload Adapter Contract

## Resultado

P2 prepara un adapter HTTP productivo-ready hacia Portal Content/File sin activar upload real por defecto. Financiero sigue siendo consumidor; Portal conserva ownership de archivos, retención, descarga y lifecycle documental.

## Implementado

- `IPortalContentFileClient`.
- `PortalContentFileHttpClient`.
- `PortalContentFileHttpOptions`.
- `PortalContentFileHttpRequest`.
- `PortalContentFileHttpResponse`.
- `PortalContentFileContractValidator`.
- `PortalContentFileErrorMapper`.
- `IPortalContentFileTokenProvider`.
- `ConfigurationPortalContentFileTokenProvider`.
- `DisabledPortalContentFileTokenProvider`.

## Modo operativo

- `Development`: usa `dev://...`.
- `PortalContentFile` + `dryRun=true`: construye/valida contrato y retorna `portal-dryrun://...`.
- `PortalContentFile` + `dryRun=false`: usa HTTP POST hacia `portalBaseUrl + uploadPath`.

## Seguridad

- `sendPayloads=false` por defecto.
- `authToken` vacío por defecto.
- `authRequired=false` por defecto.
- Production bloquea payloads salvo aprobación explícita.
- Tokens, payloads, XML y querystrings se redactan en errores.

## Fuera de alcance

- Endpoint productivo confirmado por Portal.
- Token real.
- Storage propio.
- RIDE legal final.
- ATS oficial.
- SRI Production.

## Validación

Tests cubren dry-run, headers, fake HTTP success/failure, token requerido, URL inválida, payload gating y sanitización.
