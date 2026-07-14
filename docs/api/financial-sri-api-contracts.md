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
| GET | `/api/financial/tax-reporting/summary` | `financial.electronicdocuments.read` | Implementado Sprint 3 P3 |
| GET | `/api/financial/tax-reporting/documents` | `financial.electronicdocuments.read` | Implementado Sprint 3 P3 |
| GET | `/api/financial/tax-reporting/tax-totals` | `financial.electronicdocuments.read` | Implementado Sprint 3 P3 |
| GET | `/api/financial/tax-reporting/withholding-totals` | `financial.electronicdocuments.read` | Implementado Sprint 3 P3 |
| GET | `/api/financial/tax-reporting/export` | `financial.electronicdocuments.read` | Implementado Sprint 3 P4; JSON/CSV foundation |
| POST | `/api/financial/tax-reporting/export/store` | `financial.electronicdocuments.manage` | Implementado Sprint 4 P1; storage metadata |
| GET | `/api/financial/tax-reporting/ats-readiness` | `financial.electronicdocuments.read` | Implementado Sprint 3 P4; no ATS oficial |
| GET | `/api/financial/tax-reporting/action-queue` | `financial.electronicdocuments.read` | Implementado Sprint 3 P4 |
| GET | `/api/financial/tax-reporting/monthly-summary` | `financial.electronicdocuments.read` | Implementado Sprint 3 P4 |
| GET | `/api/financial/electronic-documents/{id}/integration-status` | `financial.electronicdocuments.read` | Implementado P4 |
| GET | `/api/financial/electronic-documents/sri/readiness` | `financial.electronicdocuments.manage` | Implementado P4 |
| GET | `/api/financial/electronic-documents/content-file/readiness` | `financial.electronicdocuments.manage` | Implementado Sprint 4 P1 |
| GET | `/api/financial/electronic-documents/sri/connectivity-probe` | `financial.electronicdocuments.manage` | Implementado P5 |
| GET | `/api/financial/electronic-documents/by-access-key/{accessKey}` | `financial.electronicdocuments.read` | Implementado P1 |

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
