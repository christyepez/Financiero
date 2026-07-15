# SRI Electronic Invoicing Architecture

## Componentes

```text
Financiero API
  -> ElectronicDocumentsService
    -> ElectronicDocument domain
    -> SriAccessKeyGenerator
    -> IElectronicDocumentXmlGenerator
    -> IElectronicSignatureService
    -> IElectronicDocumentXmlValidator
    -> ISriReceptionClient / ISriAuthorizationClient
    -> IElectronicDocumentStorageClient
    -> IPortalAuditClient / IPortalOutboxClient
    -> IElectronicDocumentRepository
      -> FinancieroDb / schema financial
```

## Portal reuse

- Security: permisos `financial.electronicdocuments.*`.
- Configuration: claves `financial.sri.*`.
- Audit: eventos de documentos electrónicos por puerto.
- Outbox: eventos de integración por puerto.
- Gateway/health/logging/correlationId: reutilizados.
- SQL común: se usa `FinancieroDb`; no SQL Server propio.

## Secuencias

`financial.sri_document_sequences` reserva secuenciales por tenant, tipo, ambiente, establecimiento y punto de emisión. No se mezcla con `accounting_sequences`.

Sprint 3 P1 reutiliza la misma estrategia para:

- Factura, `codDoc=01`.
- Nota de Crédito, `codDoc=04`.
- Nota de Débito, `codDoc=05`.
- Comprobante de Retención, `codDoc=07`.

## Firma

P2 agrega providers Development, Disabled, External y LocalCertificatePlaceholder. Development se bloquea en Production; providers reales fallan explícitamente hasta tener adapter seguro.

## Clientes SRI

P2 normaliza request/response y estados SRI. No se invocan endpoints productivos ni SOAP real.

## Storage

P2 agrega puerto `IElectronicDocumentStorageClient`. El adapter development registra hashes/metadata; producción debe delegar XML/PDF a Portal Content/File.

P3 agrega foundation para XAdES, SOAP Test, Portal Content/File y RIDE. Todos los adapters productivos quedan deshabilitados por configuración segura; el flujo local continúa con Development/Mock.

P4 agrega Secret Store, sanitización y readiness SRI. Los adapters productivos siguen bloqueados por gates explícitos.

P5 fortalece Key Vault wiring, probe manual SRI Test y observabilidad sanitizada. Production y envío real permanecen bloqueados.

Sprint 4 P1 agrega readiness productivo de Portal Content/File: `PortalContentFileStorageClient`, `/health/content-file`, metadata estructurada, payload opcional apagado por defecto y bloqueo de payloads en Production sin aprobación explícita. No hay upload HTTP real todavía ni storage documental propio.

Sprint 4 P2 agrega `PortalContentFileHttpClient` con `HttpClientFactory`, contrato HTTP, validator, estrategia de token placeholder y `dryRun=true` por defecto. Las pruebas usan handler falso; no se llama Portal real.

Sprint 4 P3 separa templates RIDE foundation por tipo documental y agrega ATS official design foundation. Ambos son modelos de preparación y revisión; no son outputs legales finales ni ATS oficial.

Sprint 4 P4 agrega gestión calculada de gaps tributarios/legales para RIDE final y ATS oficial. Los resultados son read-only, auditados por Portal Audit y no crean evidencia persistida, aprobaciones mutables ni artefactos oficiales.

Sprint 5 P1 agrega foundation de compras tributarias y documentos anulados para readiness ATS. Son modelos del dominio Financiero, persistidos en `FinancieroDb`, auditados/outbox vía Portal y consultados por ATS readiness/design. No generan XML ATS oficial ni almacenan payloads reales.

## Relación futura con contabilidad

`ElectronicDocument.RelatedJournalEntryId` permite vincular contabilización futura sin acoplar el flujo SRI al posting contable en P1.

## Documentos tributarios Sprint 3 P1

`ElectronicDocument` se mantiene como agregado raíz. Las notas de crédito, notas de débito y retenciones agregan hijos persistidos para la información que no pertenece a factura:

- `electronic_document_references`: documento modificado o sustento.
- `electronic_document_debit_note_reasons`: motivos y valores de nota de débito.
- `electronic_document_withholding_taxes`: impuestos retenidos por periodo fiscal.

Los generadores y validadores XML son foundation. Antes de producción deben compararse contra XSD y reglas SRI vigentes.

## Catálogos y reglas Sprint 3 P2

P2 agrega catálogos internos foundation versionables y validadores tributarios:

- `SriCatalogService` y `DevelopmentSriCatalogProvider`.
- `SriTaxRuleValidator`.
- `TaxCalculationValidator`.
- `MoneyRoundingPolicy`.

La política de redondeo usa `decimal`, 2 posiciones, `AwayFromZero` y tolerancia `0.01`. No reemplaza un motor fiscal completo ni catálogos oficiales revisados.
## RIDE y reporting Sprint 3 P3

Sprint 3 P3 agrega modelos RIDE por tipo documental, generador Development sanitizado y endpoints de reporting tributario foundation. La generación PDF sigue siendo placeholder de desarrollo; el HTML de preview no expone XML, certificados, claves privadas, clave de acceso completa ni identificación completa del cliente/sujeto.

El reporting resume documentos electrónicos existentes por periodo, tipo, estado, impuestos y retenciones. No genera ATS oficial, no crea storage documental propio y no agrega migración porque consulta la persistencia existente de documentos electrónicos.

## Exportaciones y ATS readiness Sprint 3 P4

Sprint 3 P4 agrega exportaciones internas JSON/CSV, action queue, monthly summary y evaluación ATS readiness. ATS readiness no genera XML oficial ni certifica cumplimiento normativo; solo identifica faltantes de autorización/XML, revisión tributaria y agregados foundation.
