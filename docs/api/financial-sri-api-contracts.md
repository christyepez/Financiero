# Financial SRI API Contracts

## Electronic Documents

| Método | Ruta | Permiso | Estado |
|---|---|---|---|
| POST | `/api/financial/electronic-documents/invoices` | `financial.electronicdocuments.create` | Implementado P1 |
| POST | `/api/financial/electronic-documents/{id}/lines` | `financial.electronicdocuments.update` | Implementado P1 |
| POST | `/api/financial/electronic-documents/{id}/generate-xml` | `financial.electronicdocuments.generate` | Implementado P1 |
| POST | `/api/financial/electronic-documents/{id}/sign` | `financial.electronicdocuments.sign` | Implementado dev |
| POST | `/api/financial/electronic-documents/{id}/send` | `financial.electronicdocuments.send` | Implementado dev/mock |
| POST | `/api/financial/electronic-documents/{id}/authorize` | `financial.electronicdocuments.authorize` | Implementado dev/mock |
| POST | `/api/financial/electronic-documents/{id}/validate-xml` | `financial.electronicdocuments.generate` | Implementado P2 |
| GET | `/api/financial/electronic-documents` | `financial.electronicdocuments.read` | Implementado P1 |
| GET | `/api/financial/electronic-documents/{id}` | `financial.electronicdocuments.read` | Implementado P1 |
| GET | `/api/financial/electronic-documents/{id}/status` | `financial.electronicdocuments.read` | Implementado P2 |
| GET | `/api/financial/electronic-documents/{id}/storage-metadata` | `financial.electronicdocuments.read` | Implementado P2 |
| POST | `/api/financial/electronic-documents/{id}/generate-ride` | `financial.electronicdocuments.generate` | Implementado P3 |
| GET | `/api/financial/electronic-documents/{id}/ride-metadata` | `financial.electronicdocuments.read` | Implementado P3 |
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

## Nota

Los endpoints `sign`, `send` y `authorize` son dev/mock en P1/P2. No usan certificados reales ni SRI productivo.
