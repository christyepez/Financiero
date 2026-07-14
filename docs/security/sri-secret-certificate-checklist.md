# SRI Secret and Certificate Checklist

## Permitido

- Referencias a secretos por nombre enmascarado.
- Certificados de prueba fuera del repositorio.
- Variables de entorno locales no versionadas.
- Key Vault o secret store externo.
- Logs con hashes y valores enmascarados.

## Prohibido

- Subir `.p12`, `.pfx`, `.key`, `.cer`, `.crt`, `.pem`.
- Subir XML reales.
- Subir tokens, passwords, connection strings reales o URLs privadas sensibles.
- Loguear certificado, secreto, XML, clave de acceso completa o identificación completa.
- Habilitar SRI producción en Sprint 2.

## Requiere aprobación

- `financial.sri.test.allowConnectivityProbe=true`.
- `financial.sri.test.allowDocumentSend=true`.
- `financial.sri.storage.sendPayloads=true`.
- `financial.sri.storage.allowProductionContentFilePayload=true`.
- `financial.sri.storage.authRequired=true`.
- `financial.sri.storage.dryRun=false`.
- Provider Azure Key Vault real.
- Firma XAdES real.

## Operación manual fuera del repo

- Carga/rotación de certificado.
- Configuración de credenciales no productivas.
- Validación SRI Test con documento sintético.
- Recolección de evidencias sanitizadas.

## Checklist

- Secretos: sin valores en repo, sin valores en logs, referencias enmascaradas.
- Certificados: almacenados fuera del repo, rotación documentada, acceso mínimo.
- Key Vault: identidad configurada, permisos mínimos, fail-fast definido por ambiente.
- Permisos: `financial.electronicdocuments.*` aplicados.
- Logs: XML y datos sensibles redacted.
- Incident response: revocar secreto, rotar certificado, invalidar tokens, revisar Audit/Outbox.
- Rollback: volver a Development/Mock, dry-run true y send false.
- Producción futura: requiere aprobación de Security, DevOps, QA y owner funcional.

## Sprint 3 P1

NC, ND y retenciones foundation no agregan nuevos secretos ni certificados. Los datos de smoke y tests deben seguir siendo sintéticos. La firma real, la carga de certificados y el envío SRI Test/Production continúan sujetos a aprobación manual y custodia fuera del repositorio.

## Sprint 3 P2

Catálogos y reglas tributarias foundation no agregan secretos, certificados ni XML reales. No descargar catálogos automáticamente desde runtime; cualquier catálogo oficial revisado debe entrar por proceso controlado, PR y evidencia normativa.
## Sprint 3 P3

- RIDE preview y reporting deben devolver datos enmascarados.
- No incluir XML, certificados, claves privadas ni secretos en HTML/reportes.
- No cargar archivos `.p12`, `.pfx`, `.key`, `.pem`, `.crt`, `.cer`.
- El RIDE final legal y ATS oficial requieren revisión manual antes de producción.

## Sprint 3 P4

- Exports JSON/CSV deben excluir XML, secretos, certificados y claves privadas.
- AccessKey e identificación se enmascaran por defecto.
- `includeSensitive` existe solo como flag interno y permanece false por defecto.
- ATS readiness debe indicar que no es ATS oficial.

## Sprint 4 P1

- Content/File usa `sendPayloads=false` por defecto.
- `PayloadBase64` se redacta en logs y `ToString()`.
- Production bloquea payloads salvo aprobación explícita.
- Storage documental propio sigue prohibido.

## Sprint 4 P2

- Token Content/File no se versiona; `.env.example` deja la variable vacía.
- Errores HTTP redactan token, querystring y payload.
- `dryRun=true` es el modo seguro por defecto.
- `dryRun=false` + `authRequired=true` exige token externo.
