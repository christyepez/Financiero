# Sprint 2 P5 - Real Secret Store, SRI Test Manual Connectivity y Observability

P5 prepara la ruta operativa para conectar Secret Store real y validar SRI Test manualmente, sin activar producción ni enviar documentos automáticamente.

## Entregado

- Opciones de Azure Key Vault: `useDefaultAzureCredential`, `requireManagedIdentity` y `failFastOnStartup`.
- Readiness de Secret Store sin exposición de secretos.
- Probe manual SRI Test por `/api/financial/electronic-documents/sri/connectivity-probe`.
- Gates `allowConnectivityProbe`, `allowDocumentSend`, `manualValidationRequired` y `useSyntheticDocumentOnly`.
- Observabilidad sanitizada para intentos de integración.
- Contrato Portal Content/File estabilizado: metadata/hash y payload opcional desactivado por defecto.

## Gates de seguridad

- `financial.sri.allowProduction=false`.
- `financial.sri.test.dryRun=true`.
- `financial.sri.test.allowDocumentSend=false`.
- `financial.sri.storage.sendPayloads=false`.
- `financial.secrets.provider=Disabled`.

## Resultado

El sistema queda listo para validación manual controlada, no para operación productiva.
