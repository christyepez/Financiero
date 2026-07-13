# SRI Test Manual Validation Runbook

## Objetivo

Validar configuración y conectividad hacia SRI Test sin enviar documentos reales ni activar producción.

Este runbook es manual y no productivo. Su propósito es recolectar evidencia controlada para decidir Sprint 3; no autoriza envío a SRI producción, carga de certificados reales al repositorio ni uso de datos reales de contribuyentes.

## Prerrequisitos

- Rama validada desde `main` de GitHub.
- Ambiente no productivo.
- Credenciales SRI Test custodiadas fuera del repositorio.
- Certificado de pruebas custodiado fuera del repositorio, si se valida firma real en Sprint 3.
- Secret Store o Key Vault configurado mediante variables externas.
- Logs con sanitización activa.
- Permiso runtime `financial.electronicdocuments.manage` para readiness/probe.
- Permisos específicos para operaciones por documento:
  - `financial.electronicdocuments.generate`
  - `financial.electronicdocuments.sign`
  - `financial.electronicdocuments.send`
  - `financial.electronicdocuments.authorize`

## Configuración segura inicial

- `financial.sri.integration.mode=Test`
- `financial.sri.test.dryRun=true`
- `financial.sri.test.allowConnectivityProbe=false`
- `financial.sri.test.allowDocumentSend=false`
- `financial.sri.allowProduction=false`
- `financial.sri.logPayloads=false`
- `financial.sri.maskPayloads=true`
- `financial.sri.signature.provider=External` o `LocalCertificatePlaceholder` solo para validar bloqueo seguro; no habilitar certificado real sin aprobación.
- `financial.sri.signature.certificateSecretName` definido fuera del repositorio si aplica.
- `financial.sri.signature.keyVaultName` definido fuera del repositorio si aplica.

## Pasos

1. Configurar URLs públicas de SRI Test mediante variables externas.
2. Confirmar que no existe SQL Server propio de Financiero; debe usarse el SQL común del ambiente local.
3. Ejecutar `/health/sri`.
4. Ejecutar `/api/financial/electronic-documents/sri/readiness` con permiso `financial.electronicdocuments.manage`.
5. Ejecutar `/api/financial/electronic-documents/sri/connectivity-probe`.
6. Generar un documento sintético no real en ambiente local/test.
7. Generar XML y validar XML básico.
8. Intentar firma según provider configurado:
   - `Development` solo en ambiente Development.
   - `LocalCertificatePlaceholder` debe fallar explícitamente hasta lectura segura real.
   - `External` debe documentar puerto externo y fallar claro si no está disponible.
9. Enviar únicamente si `allowDocumentSend=true`, ambiente no productivo y aprobación manual existen.
10. Consultar estado y registrar evidencia sin payload sensible.
11. Revisar logs sanitizados: no debe aparecer XML completo, clave de acceso completa, RUC real, identificación completa, secreto ni certificado.

## Evidencia esperada

- Commit/branch usados.
- Configuración efectiva sin secretos.
- Respuesta `/health/sri`.
- Respuesta readiness.
- Resultado connectivity probe.
- Resultado de validación XML.
- Resultado de firma o bloqueo seguro.
- Resultado de envío/autorización si fue aprobado.
- Logs sanitizados.
- Confirmación de no certificados ni `.env` en Git.

## Validación de respuesta SRI Test

- `Received` o `Processing`: conexión válida, autorización pendiente.
- `Authorized`: flujo Test completo; revisar que el XML corresponda a datos sintéticos.
- `Returned` o `Rejected`: capturar mensajes normalizados y revisar corrección funcional.
- `Error`: capturar código genérico y correlationId; no pegar payload completo en documentación.

## Criterios Go / No-Go

Go para Sprint 3 opción B solo si:

- Existe custodia segura de certificado fuera del repositorio.
- Secret Store/Key Vault resuelve referencias sin exponer valores.
- SRI Test responde desde ambiente no productivo.
- Logs y errores están sanitizados.
- Operaciones requieren permisos runtime.
- No existe envío a Production.

No-Go si:

- Se requiere subir certificado, clave o `.env`.
- Se necesita desactivar sanitización.
- El provider Development aparece en ambiente Production.
- SRI Test requiere datos reales.
- Portal Content/File productivo aún no tiene contrato disponible para almacenamiento final.

## Para habilitar probe controlado

Activar únicamente en ambiente no productivo:

- `financial.sri.test.allowConnectivityProbe=true`
- mantener `financial.sri.test.allowDocumentSend=false`

## Qué no hacer

- No usar URLs de SRI producción.
- No hacer commit de `.env`, certificados, llaves, XML reales ni PDFs reales.
- No guardar certificados en `appsettings*.json`.
- No registrar XML completo ni claveAcceso completa en logs.
- No crear storage documental propio.
- No crear SQL Server propio.

## Rollback

Volver a:

- `financial.sri.integration.mode=Development`
- `financial.sri.test.dryRun=true`
- `financial.sri.test.allowConnectivityProbe=false`
- `financial.sri.test.allowDocumentSend=false`
