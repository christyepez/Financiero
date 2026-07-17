# Financial SRI API Contracts

## Electronic Documents

| Método | Ruta | Permiso | Estado |
|---|---|---|---|
| POST | `/api/financial/electronic-documents/invoices` | `financial.electronicdocuments.create` | Implementado P1 |
| POST | `/api/financial/electronic-documents/credit-notes` | `financial.electronicdocuments.create` | Implementado Sprint 3 P1 |
| POST | `/api/financial/electronic-documents/debit-notes` | `financial.electronicdocuments.create` | Implementado Sprint 3 P1 |
| POST | `/api/financial/electronic-documents/withholdings` | `financial.electronicdocuments.create` | Implementado Sprint 3 P1 |
| POST | `/api/financial/electronic-documents/{id}/lines` | `financial.electronicdocuments.update` | Implementado P1 |
| POST | `/api/financial/electronic-documents/{id}/credit-note-lines` | `financial.electronicdocuments.update` | Implementado Sprint 3 P1 |
| POST | `/api/financial/electronic-documents/{id}/debit-note-reasons` | `financial.electronicdocuments.update` | Implementado Sprint 3 P1 |
| POST | `/api/financial/electronic-documents/{id}/withholding-taxes` | `financial.electronicdocuments.update` | Implementado Sprint 3 P1 |
| POST | `/api/financial/electronic-documents/{id}/generate-xml` | `financial.electronicdocuments.generate` | Implementado P1 |
| POST | `/api/financial/electronic-documents/{id}/generate-credit-note-xml` | `financial.electronicdocuments.generate` | Implementado Sprint 3 P1 |
| POST | `/api/financial/electronic-documents/{id}/generate-debit-note-xml` | `financial.electronicdocuments.generate` | Implementado Sprint 3 P1 |
| POST | `/api/financial/electronic-documents/{id}/generate-withholding-xml` | `financial.electronicdocuments.generate` | Implementado Sprint 3 P1 |
| POST | `/api/financial/electronic-documents/{id}/sign` | `financial.electronicdocuments.sign` | Implementado dev |
| POST | `/api/financial/electronic-documents/{id}/send` | `financial.electronicdocuments.send` | Implementado dev/mock |
| POST | `/api/financial/electronic-documents/{id}/authorize` | `financial.electronicdocuments.authorize` | Implementado dev/mock |
| POST | `/api/financial/electronic-documents/{id}/validate-xml` | `financial.electronicdocuments.generate` | Implementado P2 |
| GET | `/api/financial/electronic-documents` | `financial.electronicdocuments.read` | Implementado P1 |
| GET | `/api/financial/electronic-documents/{id}` | `financial.electronicdocuments.read` | Implementado P1 |
| GET | `/api/financial/electronic-documents/{id}/status` | `financial.electronicdocuments.read` | Implementado P2 |
| GET | `/api/financial/electronic-documents/{id}/storage-metadata` | `financial.electronicdocuments.read` | Implementado P2 |
| POST | `/api/financial/electronic-documents/{id}/generate-ride` | `financial.electronicdocuments.generate` | Implementado P3 |
| POST | `/api/financial/electronic-documents/{id}/store-ride` | `financial.electronicdocuments.generate` | Implementado Sprint 4 P1; registra metadata Content/File |
| GET | `/api/financial/electronic-documents/{id}/ride-metadata` | `financial.electronicdocuments.read` | Implementado P3 |
| GET | `/api/financial/electronic-documents/{id}/ride-preview` | `financial.electronicdocuments.read` | Implementado Sprint 3 P3; HTML sanitizado |
| GET | `/api/financial/electronic-documents/{id}/ride-legal-readiness` | `financial.electronicdocuments.read` | Implementado Sprint 4 P3; foundation no legal final |
| GET | `/api/financial/tax-reporting/summary` | `financial.electronicdocuments.read` | Implementado Sprint 3 P3 |
| GET | `/api/financial/tax-reporting/documents` | `financial.electronicdocuments.read` | Implementado Sprint 3 P3 |
| GET | `/api/financial/tax-reporting/tax-totals` | `financial.electronicdocuments.read` | Implementado Sprint 3 P3 |
| GET | `/api/financial/tax-reporting/withholding-totals` | `financial.electronicdocuments.read` | Implementado Sprint 3 P3 |
| GET | `/api/financial/tax-reporting/export` | `financial.electronicdocuments.read` | Implementado Sprint 3 P4; JSON/CSV foundation |
| POST | `/api/financial/tax-reporting/export/store` | `financial.electronicdocuments.manage` | Implementado Sprint 4 P1; storage metadata |
| GET | `/api/financial/tax-reporting/ats-readiness` | `financial.electronicdocuments.read` | Implementado Sprint 3 P4; no ATS oficial |
| GET | `/api/financial/tax-reporting/ats-official-design` | `financial.electronicdocuments.read` | Implementado Sprint 4 P3; diseño foundation no oficial |
| GET | `/api/financial/tax-legal-review/ride-gaps` | `financial.electronicdocuments.read` | Implementado Sprint 4 P4; gaps RIDE no oficial |
| GET | `/api/financial/tax-legal-review/ats-gaps` | `financial.electronicdocuments.read` | Implementado Sprint 4 P4; gaps ATS no oficial |
| GET | `/api/financial/tax-legal-review/approval-checklist` | `financial.electronicdocuments.read` | Implementado Sprint 4 P4; checklist advisory sin aprobación productiva |
| GET | `/api/financial/tax-reporting/action-queue` | `financial.electronicdocuments.read` | Implementado Sprint 3 P4 |
| GET | `/api/financial/tax-reporting/monthly-summary` | `financial.electronicdocuments.read` | Implementado Sprint 3 P4 |
| GET | `/api/financial/electronic-documents/{id}/integration-status` | `financial.electronicdocuments.read` | Implementado P4 |
| GET | `/api/financial/electronic-documents/sri/readiness` | `financial.electronicdocuments.manage` | Implementado P4 |
| GET | `/api/financial/electronic-documents/content-file/readiness` | `financial.electronicdocuments.manage` | Implementado Sprint 4 P1 |

Content/File P2 no agrega endpoints públicos nuevos. Fortalece `store-ride`, `export/store` y readiness para soportar `PortalContentFile` en `dryRun=true` o HTTP real controlado por configuración.
| GET | `/api/financial/electronic-documents/sri/connectivity-probe` | `financial.electronicdocuments.manage` | Implementado P5 |
| GET | `/api/financial/electronic-documents/by-access-key/{accessKey}` | `financial.electronicdocuments.read` | Implementado P1 |

Sprint 4 P4 agrega endpoints read-only para gestión de gaps tributarios/legales. No generan RIDE legal final, XML ATS oficial, evidencia real ni aprobaciones mutables.

## Purchases and voided documents foundation

| Método | Ruta | Permiso | Estado |
|---|---|---|---|
| POST | `/api/financial/purchases` | `financial.electronicdocuments.manage` | Implementado Sprint 5 P1 |
| POST | `/api/financial/purchases/{id}/lines` | `financial.electronicdocuments.manage` | Implementado Sprint 5 P1 |
| POST | `/api/financial/purchases/{id}/taxes` | `financial.electronicdocuments.manage` | Implementado Sprint 5 P1 |
| POST | `/api/financial/purchases/{id}/validate` | `financial.electronicdocuments.manage` | Implementado Sprint 5 P1 |
| GET | `/api/financial/purchases?period=YYYY-MM` | `financial.electronicdocuments.read` | Implementado Sprint 5 P1 |
| GET | `/api/financial/purchases/{id}` | `financial.electronicdocuments.read` | Implementado Sprint 5 P1 |
| GET | `/api/financial/purchases/{id}/ats-mapping` | `financial.electronicdocuments.read` | Implementado Sprint 5 P2; read-only sanitizado |
| POST | `/api/financial/voided-documents` | `financial.electronicdocuments.manage` | Implementado Sprint 5 P1 |
| GET | `/api/financial/voided-documents?period=YYYY-MM` | `financial.electronicdocuments.read` | Implementado Sprint 5 P1 |
| GET | `/api/financial/voided-documents/{id}` | `financial.electronicdocuments.read` | Implementado Sprint 5 P1 |
| GET | `/api/financial/voided-documents/{id}/ats-mapping` | `financial.electronicdocuments.read` | Implementado Sprint 5 P2; read-only sanitizado |
| GET | `/api/financial/tax-reporting/ats-section-readiness?period=YYYY-MM` | `financial.electronicdocuments.read` | Implementado Sprint 5 P2; no ATS oficial |
| GET | `/api/financial/tax-reporting/support-document-mappings` | `financial.electronicdocuments.read` | Implementado Sprint 5 P2; catálogo foundation |

Las respuestas enmascaran identificación de proveedor, clave de acceso y autorización. Estos endpoints reducen gaps ATS, pero no generan XML ATS oficial.

## Tax catalogs foundation

| Método | Ruta | Permiso | Estado |
|---|---|---|---|
| GET | `/api/financial/tax-catalogs` | `financial.electronicdocuments.read` | Implementado Sprint 5 P3; versionado foundation |
| GET | `/api/financial/tax-catalogs/purchase-document-types` | `financial.electronicdocuments.read` | Implementado Sprint 5 P3 |
| GET | `/api/financial/tax-catalogs/support-document-types` | `financial.electronicdocuments.read` | Implementado Sprint 5 P3 |
| GET | `/api/financial/tax-catalogs/voided-document-types` | `financial.electronicdocuments.read` | Implementado Sprint 5 P3 |
| GET | `/api/financial/tax-catalogs/purchase-tax-codes` | `financial.electronicdocuments.read` | Implementado Sprint 5 P3 |
| GET | `/api/financial/tax-catalogs/supplier-identification-types` | `financial.electronicdocuments.read` | Implementado Sprint 5 P3 |

Versión: `2026-07-sprint-5-p3-foundation`. Todos los items son foundation-only, requieren revisión tributaria y no son catálogos oficiales finales.

## ATS XML foundation gated

| Método | Ruta | Permiso | Estado |
|---|---|---|---|
| GET | `/api/financial/tax-reporting/ats-xml/readiness?period=YYYY-MM` | `financial.electronicdocuments.read` | Implementado Sprint 5 P4; bloqueado por defecto |
| POST | `/api/financial/tax-reporting/ats-xml/generate-preview` | `financial.electronicdocuments.manage` | Implementado Sprint 5 P4; requiere acknowledgements |

El preview no se persiste, no se envía al SRI y solo puede devolver XML si `financial.sri.atsXmlFoundation.enabled=true` y `allowXmlPreview=true`.

## External approval workflow foundation

| Método | Ruta | Permiso | Estado |
|---|---|---|---|
| GET | `/api/financial/external-approvals` | `financial.electronicdocuments.read` | Implementado Sprint 5 P5; read-only |
| GET | `/api/financial/external-approvals/{scope}` | `financial.electronicdocuments.read` | Implementado Sprint 5 P5 |
| GET | `/api/financial/external-approvals/readiness?scope=all` | `financial.electronicdocuments.read` | Implementado Sprint 5 P5 |

El workflow es advisory/foundation. No habilita producción, no persiste evidencia y no cambia configuración.

## Sprint 4 endpoint summary

- Content/File readiness: `GET /api/financial/electronic-documents/content-file/readiness`, permiso `financial.electronicdocuments.manage`, no secretos/XML.
- Store RIDE/export: `POST /store-ride` y `POST /tax-reporting/export/store`, metadata/hash; no storage propio.
- RIDE legal readiness: read-only, permiso `financial.electronicdocuments.read`, no RIDE legal final.
- ATS official design: read-only, permiso `financial.electronicdocuments.read`, no ATS oficial.
- Tax/legal review gaps and checklist: read-only, permiso `financial.electronicdocuments.read`, no aprobaciones productivas.
- Todos los endpoints Sprint 4 deben mantener sanitización de XML, certificados, access keys completas, tokens y datos reales.

## Create invoice draft

```json
{
  "issueDate": "2026-01-15",
  "customerIdentificationType": "04",
  "customerIdentification": "0999999999001",
  "customerName": "Cliente",
  "currency": "USD",
  "establishmentCode": "001",
  "emissionPointCode": "001"
}
```

## Add line

```json
{
  "productCode": "SKU-1",
  "description": "Servicio",
  "quantity": 1,
  "unitPrice": 10,
  "discount": 0
}
```

## Create credit note draft

```json
{
  "issueDate": "2026-01-15",
  "customerIdentificationType": "04",
  "customerIdentification": "0999999999001",
  "customerName": "Cliente",
  "modifiedDocumentTypeCode": "01",
  "modifiedDocumentNumber": "001-001-000000001",
  "modifiedDocumentDate": "2026-01-10",
  "reason": "Devolución parcial",
  "currency": "USD",
  "establishmentCode": "001",
  "emissionPointCode": "001"
}
```

## Create debit note draft

```json
{
  "issueDate": "2026-01-15",
  "customerIdentificationType": "04",
  "customerIdentification": "0999999999001",
  "customerName": "Cliente",
  "modifiedDocumentTypeCode": "01",
  "modifiedDocumentNumber": "001-001-000000001",
  "modifiedDocumentDate": "2026-01-10",
  "currency": "USD",
  "establishmentCode": "001",
  "emissionPointCode": "001"
}
```

## Add debit note reason

```json
{
  "reason": "Intereses por mora",
  "amount": 5.25
}
```

## Create withholding draft

```json
{
  "issueDate": "2026-01-15",
  "subjectIdentificationType": "04",
  "subjectIdentification": "0999999999001",
  "subjectName": "Proveedor",
  "fiscalPeriod": "01/2026",
  "supportDocumentTypeCode": "01",
  "supportDocumentNumber": "001-001-000000001",
  "supportDocumentDate": "2026-01-10",
  "currency": "USD",
  "establishmentCode": "001",
  "emissionPointCode": "001"
}
```

## Add withholding tax

```json
{
  "taxCode": "2",
  "withholdingCode": "332",
  "baseAmount": 100,
  "percentage": 1.75
}
```

## Nota

Los endpoints `sign`, `send` y `authorize` son dev/mock en P1/P2/P3. No usan certificados reales ni SRI productivo.

Sprint 3 P2 agrega hardening funcional:

- documentos relacionados/sustento deben usar formato `###-###-#########`;
- NC y ND requieren total mayor a cero;
- retenciones requieren periodo fiscal `MM/YYYY` o `YYYY-MM`;
- taxCode/withholdingCode deben existir en catálogo foundation;
- valor retenido debe coincidir con `base * porcentaje / 100` dentro de tolerancia documentada.
# External approval requests foundation

Base: `/api/financial/external-approval-requests`

- `GET /` requiere `financial.electronicdocuments.read`.
- `GET /{id}` requiere `financial.electronicdocuments.read`.
- `GET /readiness?scope=all` requiere `financial.electronicdocuments.read`.
- `GET /integration-readiness` requiere `financial.electronicdocuments.read`.
- `POST /` requiere `financial.electronicdocuments.manage`.
- `POST /{id}/submit` requiere `financial.electronicdocuments.manage`.
- `POST /{id}/start-review` requiere `financial.electronicdocuments.manage`.
- `POST /{id}/evidence-references` requiere `financial.electronicdocuments.manage`.
- `POST /{id}/decision` requiere `financial.electronicdocuments.manage`.
- `POST /{id}/cancel` requiere `financial.electronicdocuments.manage`.

Solo acepta metadata foundation. No acepta XML, base64, certificados, archivos, rutas locales ni URLs con tokens/querystring sensible. `ApprovedFoundation` no habilita producción.

Sprint 7 P3 agrega boundary con Portal Content/File y Portal Notification: evidencia como metadata/reference-only y notification intents foundation sin envío real desde Financiero.

# Controlled productization readiness

- `GET /api/financial/purchases/productization-readiness` requiere `financial.electronicdocuments.read`.
- `GET /api/financial/purchases/{id}/productization-readiness` requiere `financial.electronicdocuments.read`.
- `GET /api/financial/voided-documents/productization-readiness` requiere `financial.electronicdocuments.read`.
- `GET /api/financial/voided-documents/{id}/productization-readiness` requiere `financial.electronicdocuments.read`.

Los endpoints son read-only. No mutan estado, no generan XML, no envían SRI, no suben evidencia, no envían notificaciones y no activan producción.

# Sprint 7 closure API posture

Sprint 7 P5 no agrega endpoints funcionales nuevos. Documenta el estado de P1-P4 y conserva los contratos existentes como foundation/no productivo. La activación de SRI Test real, SRI Production, ATS oficial, RIDE legal final, XAdES productivo, upload real y notification send sigue bloqueada.

# Portal integration readiness

| Método | Ruta | Permiso | Estado |
|---|---|---|---|
| GET | `/api/financial/portal-integration/readiness` | `financial.electronicdocuments.read` | Implementado Sprint 8 P1; read-only |

Devuelve capacidades Portal requeridas, permisos esperados, rutas de menú esperadas, feature flags seguros, blockers productivos, warnings sanitizados y correlation id. No devuelve tokens, secretos, claims completas ni datos sensibles.

Sprint 8 P2 agrega a la respuesta servicios esperados, requisitos de SQL común y posibles indicadores de drift del contrato PortalShellContext. No cambia permisos ni agrega mutaciones.

Sprint 8 P3 agrega `readinessClassification`. Valores esperados: `Ready`, `BlockedDependency`, `NotConfigured`, `FoundationOnly`, `ProductionBlocked`.
