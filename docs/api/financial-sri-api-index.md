# Financial SRI API Index

| Método | Ruta | Permiso | Modo | Riesgo | Datos sensibles | Estado |
|---|---|---|---|---|---|---|
| POST | `/api/financial/electronic-documents/invoices` | create | Dev/Test | Medio | Cliente sintético | implemented |
| POST | `/{id}/lines` | update | Dev/Test | Medio | No XML | implemented |
| POST | `/{id}/generate-xml` | generate | Dev/Test | Alto | XML generado | implemented |
| POST | `/{id}/validate-xml` | generate | Dev/Test | Medio | XML interno | implemented |
| POST | `/{id}/sign` | sign | Dev/mock | Alto | Firma metadata | mock/dev |
| POST | `/{id}/send` | send | Mock/dry-run | Alto | SRI metadata | mock/dev |
| POST | `/{id}/authorize` | authorize | Mock/dry-run | Alto | SRI metadata | mock/dev |
| POST | `/{id}/generate-ride` | generate | Dev | Medio | PDF placeholder | implemented |
| GET | `/{id}/ride-metadata` | read | Dev/Test | Bajo | Hash/id | implemented |
| GET | `/{id}/storage-metadata` | read | Dev/Test | Bajo | Hash/id | implemented |
| GET | `/{id}/integration-status` | read | Dev/Test | Bajo | Sanitizado | implemented |
| GET | `/sri/readiness` | manage | Dev/Test | Bajo | No secretos/XML | dry-run |
| GET | `/sri/connectivity-probe` | manage | Test manual | Medio | URLs enmascaradas | dry-run |
| GET | `/health/sri` | anonymous | Dev/Test | Bajo | No secretos/XML | implemented |

Production SRI, XAdES real y Content/File payload real permanecen bloqueados.
