# Financial SRI API Index

| Método | Ruta | Permiso | Modo | Riesgo | Datos sensibles | Estado |
|---|---|---|---|---|---|---|
| POST | `/api/financial/electronic-documents/invoices` | create | Dev/Test | Medio | Cliente sintético | implemented |
| POST | `/api/financial/electronic-documents/credit-notes` | create | Dev/Test | Medio | Cliente sintético | implemented P3 P1 |
| POST | `/api/financial/electronic-documents/debit-notes` | create | Dev/Test | Medio | Cliente sintético | implemented P3 P1 |
| POST | `/api/financial/electronic-documents/withholdings` | create | Dev/Test | Medio | Sujeto retenido sintético | implemented P3 P1 |
| POST | `/{id}/lines` | update | Dev/Test | Medio | No XML | implemented |
| POST | `/{id}/credit-note-lines` | update | Dev/Test | Medio | No XML | implemented P3 P1 |
| POST | `/{id}/debit-note-reasons` | update | Dev/Test | Medio | No XML | implemented P3 P1 |
| POST | `/{id}/withholding-taxes` | update | Dev/Test | Medio | No XML | implemented P3 P1 |
| POST | `/{id}/generate-xml` | generate | Dev/Test | Alto | XML generado | implemented |
| POST | `/{id}/generate-credit-note-xml` | generate | Dev/Test | Alto | XML generado | implemented P3 P1 |
| POST | `/{id}/generate-debit-note-xml` | generate | Dev/Test | Alto | XML generado | implemented P3 P1 |
| POST | `/{id}/generate-withholding-xml` | generate | Dev/Test | Alto | XML generado | implemented P3 P1 |
| POST | `/{id}/validate-xml` | generate | Dev/Test | Medio | XML interno | implemented |
| POST | `/{id}/sign` | sign | Dev/mock | Alto | Firma metadata | mock/dev |
| POST | `/{id}/send` | send | Mock/dry-run | Alto | SRI metadata | mock/dev |
| POST | `/{id}/authorize` | authorize | Mock/dry-run | Alto | SRI metadata | mock/dev |
| POST | `/{id}/generate-ride` | generate | Dev | Medio | PDF placeholder | implemented |
| GET | `/{id}/ride-preview` | read | Dev/Test | Medio | HTML sanitizado | implemented Sprint 3 P3 |
| GET | `/api/financial/tax-reporting/summary` | read | Dev/Test | Medio | Totales agregados | implemented Sprint 3 P3 |
| GET | `/api/financial/tax-reporting/documents` | read | Dev/Test | Medio | Datos enmascarados | implemented Sprint 3 P3 |
| GET | `/api/financial/tax-reporting/tax-totals` | read | Dev/Test | Medio | Totales impuestos | implemented Sprint 3 P3 |
| GET | `/api/financial/tax-reporting/withholding-totals` | read | Dev/Test | Medio | Totales retenciones | implemented Sprint 3 P3 |
| GET | `/api/financial/tax-reporting/export` | read | Dev/Test | Medio | JSON/CSV en memoria | implemented Sprint 3 P4 |
| GET | `/api/financial/tax-reporting/ats-readiness` | read | Dev/Test | Medio | Evaluación interna no oficial | implemented Sprint 3 P4 |
| GET | `/api/financial/tax-reporting/action-queue` | read | Dev/Test | Medio | Pendientes por acción | implemented Sprint 3 P4 |
| GET | `/api/financial/tax-reporting/monthly-summary` | read | Dev/Test | Medio | Resumen mensual | implemented Sprint 3 P4 |
| GET | `/{id}/ride-metadata` | read | Dev/Test | Bajo | Hash/id | implemented |
| GET | `/{id}/storage-metadata` | read | Dev/Test | Bajo | Hash/id | implemented |
| GET | `/{id}/integration-status` | read | Dev/Test | Bajo | Sanitizado | implemented |
| GET | `/sri/readiness` | manage | Dev/Test | Bajo | No secretos/XML | dry-run |
| GET | `/sri/connectivity-probe` | manage | Test manual | Medio | URLs enmascaradas | dry-run |
| GET | `/health/sri` | anonymous | Dev/Test | Bajo | No secretos/XML | implemented |

Production SRI, XAdES real, RIDE final por tipo documental y Content/File payload real permanecen bloqueados.

Sprint 3 P2 endurece validaciones de catálogo foundation, periodo fiscal, documento relacionado, totales y retenciones antes de generar XML.
