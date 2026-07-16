# ATS XML Foundation Gated

Este preview existe para evaluación técnica interna. No es archivo ATS oficial y no certifica cumplimiento.

## Endpoints

- `GET /api/financial/tax-reporting/ats-xml/readiness?period=YYYY-MM`
- `POST /api/financial/tax-reporting/ats-xml/generate-preview`

`generate-preview` requiere:

- `acknowledgeFoundationOnly=true`
- `acknowledgeNoSriSubmission=true`
- `acknowledgeNoOfficialCompliance=true`

## Seguridad

El XML no se persiste, no se audita y no se loguea. Audit registra solo metadata: periodo, estado, conteos y hash si aplica.

## Pendientes oficiales

- XSD/layout oficial.
- Catálogos oficiales finales.
- Revisión tributaria/legal.
- Aprobación operacional antes de cualquier envío.

## Sprint 5 P5

El uso oficial queda condicionado por external approval gates: ATS official XML, security production gate, operational runbook y revisión tributaria/legal. El workflow expone estado foundation y no habilita envío.
