# Sprint 4 P1 - Portal Content/File Productive Adapter + Storage Readiness

## Resultado

P1 prepara a Financiero para almacenar RIDE, XML y exports fiscales mediante Portal Content/File sin crear storage documental propio. El modo local sigue usando `DevelopmentElectronicDocumentStorageClient` y devuelve identificadores `dev://...`.

## Alcance implementado

- Adapter `PortalContentFileStorageClient` con contrato de request, metadata, hash, tenant, correlation id y payload opcional.
- Readiness de Content/File por `/health/content-file`.
- Endpoint protegido `GET /api/financial/electronic-documents/content-file/readiness`.
- Endpoint `POST /api/financial/electronic-documents/{id}/store-ride`.
- Endpoint `POST /api/financial/tax-reporting/export/store`.
- Export storage readiness para JSON/CSV con metadata y hash.
- Smoke SRI ampliado para health, readiness, store-ride y export/store.

## Seguridad

- `sendPayloads=false` por defecto.
- `PayloadBase64` se redacta en `ToString()`.
- En Production se bloquea envío de payloads salvo `allowProductionContentFilePayload=true`.
- No se suben certificados, XML reales, `.env`, secretos ni llaves.

## Fuera de alcance

- Upload HTTP real a Portal Content/File.
- RIDE legal final.
- ATS oficial.
- Firma XAdES productiva.
- SRI Production.

## Siguiente paso

Ejecutar Sprint 4 P2 cuando Portal exponga contrato productivo estable de Content/File y exista validación fiscal/manual de retención y descarga.
