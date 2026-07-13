# SRI Test Manual Validation Runbook

## Objetivo

Validar configuración y conectividad hacia SRI Test sin enviar documentos reales ni activar producción.

## Configuración segura inicial

- `financial.sri.integration.mode=Test`
- `financial.sri.test.dryRun=true`
- `financial.sri.test.allowConnectivityProbe=false`
- `financial.sri.test.allowDocumentSend=false`
- `financial.sri.allowProduction=false`
- `financial.sri.logPayloads=false`
- `financial.sri.maskPayloads=true`

## Pasos

1. Configurar URLs públicas de SRI Test mediante variables externas.
2. Ejecutar `/health/sri`.
3. Ejecutar `/api/financial/electronic-documents/sri/readiness` con permiso `financial.electronicdocuments.manage`.
4. Ejecutar `/api/financial/electronic-documents/sri/connectivity-probe`.
5. Revisar logs sanitizados: no debe aparecer XML, clave de acceso completa, RUC real, identificación completa, secreto ni certificado.

## Para habilitar probe controlado

Activar únicamente en ambiente no productivo:

- `financial.sri.test.allowConnectivityProbe=true`
- mantener `financial.sri.test.allowDocumentSend=false`

## Rollback

Volver a:

- `financial.sri.integration.mode=Development`
- `financial.sri.test.dryRun=true`
- `financial.sri.test.allowConnectivityProbe=false`
- `financial.sri.test.allowDocumentSend=false`
