# Sprint 5 P4 - ATS XML Foundation Gated

Estado: implementado como preview foundation bloqueado por configuración segura. No es ATS oficial, no se envía al SRI y no se persiste XML.

## Alcance implementado

- Modelo de generación ATS XML foundation con metadata, secciones, hash y disclaimer.
- Readiness validator con gates de configuración, producción, periodo, schema oficial faltante y aprobación externa.
- Generador XML foundation en memoria.
- Endpoints protegidos para readiness y generate-preview.
- Smoke para validar bloqueo por defecto y ausencia de XML cuando preview está apagado.

## Gates

- `financial.sri.atsXmlFoundation.enabled=false` por defecto.
- `financial.sri.atsXmlFoundation.allowXmlPreview=false` por defecto.
- `financial.sri.atsXmlFoundation.maxDocuments=500`.
- `financial.sri.atsXmlFoundation.requireSyntheticData=true` por defecto.

## Restricciones

- No SRI Test/Production.
- No persistencia XML.
- No XML en audit/logs.
- No certificados, secretos ni datos reales.
- No Content/File upload.
