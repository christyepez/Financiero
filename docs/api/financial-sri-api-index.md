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
| POST | `/{id}/store-ride` | generate | Dev/Test | Medio | Storage id/hash | implemented Sprint 4 P1 |
| GET | `/{id}/ride-preview` | read | Dev/Test | Medio | HTML sanitizado | implemented Sprint 3 P3 |
| GET | `/{id}/ride-legal-readiness` | read | Dev/Test | Medio | Issues/disclaimer sanitizados | implemented Sprint 4 P3 |
| GET | `/api/financial/tax-reporting/summary` | read | Dev/Test | Medio | Totales agregados | implemented Sprint 3 P3 |
| GET | `/api/financial/tax-reporting/documents` | read | Dev/Test | Medio | Datos enmascarados | implemented Sprint 3 P3 |
| GET | `/api/financial/tax-reporting/tax-totals` | read | Dev/Test | Medio | Totales impuestos | implemented Sprint 3 P3 |
| GET | `/api/financial/tax-reporting/withholding-totals` | read | Dev/Test | Medio | Totales retenciones | implemented Sprint 3 P3 |
| GET | `/api/financial/tax-reporting/export` | read | Dev/Test | Medio | JSON/CSV en memoria | implemented Sprint 3 P4 |
| POST | `/api/financial/tax-reporting/export/store` | manage | Dev/Test | Medio | Storage id/hash | implemented Sprint 4 P1 |
| GET | `/api/financial/tax-reporting/ats-readiness` | read | Dev/Test | Medio | Evaluación interna no oficial | implemented Sprint 3 P4 |
| GET | `/api/financial/tax-reporting/ats-official-design` | read | Dev/Test | Medio | Diseño no oficial | implemented Sprint 4 P3 |
| GET | `/api/financial/tax-legal-review/ride-gaps` | read | Dev/Test | Medio | Gaps/disclaimer no oficial | implemented Sprint 4 P4 |
| GET | `/api/financial/tax-legal-review/ats-gaps` | read | Dev/Test | Medio | Gaps/disclaimer no oficial | implemented Sprint 4 P4 |
| GET | `/api/financial/tax-legal-review/approval-checklist` | read | Dev/Test | Medio | Checklist advisory | implemented Sprint 4 P4 |
| GET | `/api/financial/tax-reporting/action-queue` | read | Dev/Test | Medio | Pendientes por acción | implemented Sprint 3 P4 |
| GET | `/api/financial/tax-reporting/monthly-summary` | read | Dev/Test | Medio | Resumen mensual | implemented Sprint 3 P4 |
| GET | `/{id}/ride-metadata` | read | Dev/Test | Bajo | Hash/id | implemented |
| GET | `/{id}/storage-metadata` | read | Dev/Test | Bajo | Hash/id | implemented |
| GET | `/{id}/integration-status` | read | Dev/Test | Bajo | Sanitizado | implemented |
| GET | `/sri/readiness` | manage | Dev/Test | Bajo | No secretos/XML | dry-run |
| GET | `/content-file/readiness` | manage | Dev/Test | Bajo | URL enmascarada | implemented Sprint 4 P1 |
| GET | `/sri/connectivity-probe` | manage | Test manual | Medio | URLs enmascaradas | dry-run |
| GET | `/health/sri` | anonymous | Dev/Test | Bajo | No secretos/XML | implemented |
| GET | `/health/content-file` | anonymous | Dev/Test | Bajo | No secretos/XML | implemented Sprint 4 P1 |

Production SRI, XAdES real, RIDE legal final, ATS oficial y Content/File upload/payload real permanecen bloqueados.

Sprint 4 P2 prepara el cliente HTTP Content/File detrás de configuración. No agrega endpoints públicos nuevos.

Sprint 4 P3 agrega endpoints read-only para RIDE legal readiness y ATS official design foundation. No generan documentos legales finales.

Sprint 4 P4 agrega endpoints read-only de gaps y checklist de aprobación. No aprueba uso oficial, no genera ATS oficial y no persiste evidencia real.

Sprint 4 queda cerrado como readiness no productivo: Content/File readiness, store RIDE/export metadata, RIDE legal readiness, ATS official design, tax/legal gaps y approval checklist. Los permisos usan `financial.electronicdocuments.read/manage` según sensibilidad; las respuestas deben permanecer sanitizadas.

Sprint 5 P1 agrega foundation de compras y documentos anulados:

- `/api/financial/purchases` y subrutas de líneas, impuestos, validación y consulta.
- `/api/financial/voided-documents` para registro y consulta.
- Impacto ATS: readiness/design usa conteos foundation y reduce gaps de compras/anulados cuando existen registros del periodo.
- No genera ATS oficial, no almacena XML real y no duplica Audit/Outbox/Security del Portal.

Sprint 5 P2 agrega mapping/readiness ATS read-only:

- `/api/financial/purchases/{id}/ats-mapping`.
- `/api/financial/voided-documents/{id}/ats-mapping`.
- `/api/financial/tax-reporting/ats-section-readiness?period=YYYY-MM`.
- `/api/financial/tax-reporting/support-document-mappings`.
- Todos usan `financial.electronicdocuments.read`, respuesta sanitizada y disclaimer de no ATS oficial.

Sprint 5 P3 agrega catálogos foundation read-only:

- `/api/financial/tax-catalogs`.
- `/purchase-document-types`, `/support-document-types`, `/voided-document-types`, `/purchase-tax-codes`, `/supplier-identification-types`.
- Versión `2026-07-sprint-5-p3-foundation`; no oficiales finales y sujetos a revisión tributaria.

Sprint 5 P4 agrega ATS XML foundation gated:

- `/api/financial/tax-reporting/ats-xml/readiness`.
- `/api/financial/tax-reporting/ats-xml/generate-preview`.
- Bloqueado por defecto, sin persistencia, sin envío SRI y sin afirmación de cumplimiento oficial.

Sprint 5 P5 agrega external approval workflow foundation:

- `/api/financial/external-approvals`.
- `/api/financial/external-approvals/{scope}`.
- `/api/financial/external-approvals/readiness?scope=all`.
- Es read-only/advisory y no habilita producción.

Sprint 3 P2 endurece validaciones de catálogo foundation, periodo fiscal, documento relacionado, totales y retenciones antes de generar XML.
- External approval requests foundation: `/api/financial/external-approval-requests`.
- External approval Portal integration readiness: `/api/financial/external-approval-requests/integration-readiness`.
- Purchase productization readiness: `/api/financial/purchases/productization-readiness`.
- Voided document productization readiness: `/api/financial/voided-documents/productization-readiness`.

Sprint 7 P5 does not add mutating API capabilities. It closes the sprint with QA evidence, capability matrix and roadmap. All Sprint 7 endpoints remain foundation/readiness only and must not be interpreted as SRI Production, official ATS, legal-final RIDE, productive XAdES, upload or notification-send enablement.

Sprint 8 P1 agrega `GET /api/financial/portal-integration/readiness`, protegido con `financial.electronicdocuments.read`. Es read-only, no expone secretos/tokens/claims completas, reporta capacidades Portal esperadas, blockers productivos y evidencia faltante para E2E.

Sprint 8 P2 fortalece la respuesta con servicios esperados, requisitos de SQL común y contract drift indicators. El endpoint sigue siendo read-only y no activa producción tributaria.

Sprint 8 P3 agrega clasificación explícita de readiness: `Ready`, `BlockedDependency`, `NotConfigured`, `FoundationOnly`, `ProductionBlocked`. La clasificación no oculta fallas de SQL común ni Portal runtime.

Sprint 8 P4 no agrega endpoints. Endurece la UX consumidora del workflow de aprobaciones externas:

- `ApprovedFoundation` significa aprobación foundation, no autorización productiva.
- Las referencias de evidencia son Portal-owned metadata-only; Financiero no almacena archivos.
- Los notification intents son foundation/no-send; Portal Notification mantiene ownership.
- Producción requiere Portal runtime, aprobación legal/tributaria y aprobación security.

Sprint 8 P5 no agrega endpoints. Cierra evidencia final como `BLOCKED_DEPENDENCY` mientras SQL común y Portal Gateway/Shell no estén disponibles. Los contratos API siguen read-only/foundation y not production-ready.

Sprint 9 P1 no agrega endpoints. Reejecuta E2E real y mantiene `BLOCKED_DEPENDENCY` hasta que SQL común, Portal Gateway/Shell y Financiero API estén disponibles. No cambia permisos ni activa producción.
