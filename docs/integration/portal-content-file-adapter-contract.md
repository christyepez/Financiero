# Portal Content/File Adapter Contract

## Puerto consumidor

`IElectronicDocumentStorageClient` delega almacenamiento de:

- XML sin firmar.
- XML firmado.
- XML de autorización.
- RIDE/PDF.
- Preview HTML de RIDE.
- Exports fiscales JSON/CSV.
- Snapshot ATS readiness.

## Request esperado

`PortalContentFileRequest` contiene:

- `purpose`.
- `fileName`.
- `contentType`.
- `hash`.
- `size`.
- `container`.
- `tenantId`.
- `correlationId`.
- `metadata`.
- `includePayload`.
- `payloadBase64` opcional y redactado en logs.

## HTTP P2

Cuando `financial.sri.storage.provider=PortalContentFile`:

- `dryRun=true` valida el contrato y retorna `portal-dryrun://...`.
- `dryRun=false` ejecuta `POST {portalBaseUrl}{uploadPath}`.

Headers:

- `Authorization: Bearer <token>` si está disponible.
- `X-Tenant-Id`.
- `X-Correlation-Id`.
- `X-Source-System: Financiero`.
- `Content-Type: application/json`.

Respuesta esperada:

```json
{
  "storageId": "portal-content-file://...",
  "provider": "PortalContentFile",
  "storedAtUtc": "2026-01-01T00:00:00Z",
  "contractStatus": "Uploaded"
}
```

## Metadata

`PortalContentFileMetadata` incluye `sourceSystem=Financiero`, documento, tipo, clave de acceso enmascarada, periodo, política de retención y valores extensibles.

## Configuración

- `financial.sri.storage.provider=Development|PortalContentFile|Disabled`.
- `financial.sri.storage.portalBaseUrl`.
- `financial.sri.storage.uploadPath=/api/content-files`.
- `financial.sri.storage.container`.
- `financial.sri.storage.timeoutSeconds`.
- `financial.sri.storage.sendPayloads=false`.
- `financial.sri.storage.maskPayloads=true`.
- `financial.sri.storage.allowProductionContentFilePayload=false`.
- `financial.sri.storage.allowProductionPayload=false`.
- `financial.sri.storage.dryRun=true`.
- `financial.sri.storage.authRequired=false`.
- `financial.sri.storage.authToken=` vacío, no versionar valor real.
- `financial.sri.storage.retainXml=true`.
- `financial.sri.storage.retainPdf=false`.

## Política

Financiero no persiste archivos productivos. Portal Content/File debe ser el dueño de storage, retención, descarga, auditoría documental y lifecycle del archivo.
