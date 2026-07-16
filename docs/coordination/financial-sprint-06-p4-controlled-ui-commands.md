# Sprint 6 P4 - Controlled UI Command Foundation

Estado: implementado como comandos UI foundation controlados.

## Alcance

- Crear compras foundation desde Angular solo si los feature flags y permisos lo permiten.
- Validar compras foundation desde Angular bajo las mismas reglas.
- Registrar documentos anulados foundation desde Angular bajo reglas controladas.
- Mantener modo read-only por defecto.

## Feature flags

Todos los comandos están apagados por defecto:

- `allowMutations=false`.
- `allowPurchaseCommands=false`.
- `allowVoidedDocumentCommands=false`.
- `allowAtsOfficialActions=false`.
- `allowSriSubmission=false`.
- `allowXmlPreviewUi=false`.

En production los comandos quedan deshabilitados por diseño foundation.

## Permisos

Las operaciones de comando requieren `financial.electronicdocuments.manage`. Las consultas mantienen `financial.electronicdocuments.read`.

## Seguridad

- No login propio.
- No token storage.
- No envío SRI.
- No ATS oficial.
- No XML preview.
- Resultados y errores son sanitizados.
- Identificación, accessKey y autorización se muestran enmascarados.

## Validación local sugerida

1. Ejecutar backend en `http://localhost:8083`.
2. Confirmar que la UI muestra modo read-only por defecto.
3. Habilitar flags solo en development mediante contexto Portal temporal.
4. Crear compra sintética foundation.
5. Validar compra foundation.
6. Registrar anulado sintético foundation.
7. Confirmar refresco de listas y ausencia de XML/SRI submission.
