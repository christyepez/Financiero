# Sprint 7 Readiness Notes

## P4 controlled readiness

Adds read-only readiness endpoints for purchases and voided documents:

- `GET /api/financial/purchases/productization-readiness`
- `GET /api/financial/purchases/{id}/productization-readiness`
- `GET /api/financial/voided-documents/productization-readiness`
- `GET /api/financial/voided-documents/{id}/productization-readiness`

The endpoints are foundation-only and require `financial.electronicdocuments.read`.

## Validation status

Readiness reports blockers for production SRI, SRI Test real, official ATS, legal RIDE, productive XAdES, evidence upload and notification send.
