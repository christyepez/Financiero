# Financial Sprint 2 Backlog Readiness

## Opciones

### Opción A — Sprint 2: SRI & Electronic Invoicing Foundation

Construir la base de documentos tributarios electrónicos y preparación de integración SRI.

### Opción B — Sprint 2: Portal HTTP Adapters Productive Integration

Reemplazar adaptadores de desarrollo por clientes HTTP productivos hacia Portal Security, Configuration, Audit, Notification y Outbox.

### Opción C — Sprint 2: Financial Reporting / Trial Balance / General Ledger Query

Construir consultas financieras de lectura: mayor general, balance de comprobación y reportes base.

## Recomendación principal

Priorizar Opción A: Sprint 2 — SRI & Electronic Invoicing Foundation.

## Prerrequisitos para SRI

- Certificado/firma electrónica.
- Ambientes SRI.
- Catálogos SRI.
- RUC/empresa emisora.
- Secuencia de documentos tributarios.
- XML schema.
- Firma XAdES o estrategia de firma.
- Autorización SRI.
- Contingencia.
- Almacenamiento de XML/PDF.
- Integración con Notification.
- Integración con Audit/Outbox.
- Integración con Journal Entries para contabilización.

## Alcance sugerido Sprint 2

- Modelo base de documentos tributarios.
- Catálogos SRI mínimos.
- Secuencias tributarias.
- Generación XML inicial sin envío productivo.
- Puertos para firma/autorización.
- Eventos auditables y outbox.

## No hacer en Sprint 2 sin decisión previa

- Custodiar secretos/certificados sin estrategia aprobada.
- Implementar almacenamiento documental duplicando Content/File Portal.
- Enviar a SRI productivo sin ambiente y certificados confirmados.
- Construir frontend propio fuera de Angular Shell.

## Deudas controladas arrastradas

- Portal HTTP adapters productivos.
- Herramienta formal de migraciones.
- Load/concurrency test extendido.
- Frontend Angular.
- Reporting financiero.
