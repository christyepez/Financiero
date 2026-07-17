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

## Sprint 4 P3

- RIDE legal readiness y ATS official design no deben exponer XML ni secretos.
- RIDE foundation debe mostrar disclaimer de no legal final.
- ATS official design debe mostrar disclaimer de no ATS oficial.

## Sprint 4 P4

- Tax/legal review gaps y approval checklist son read-only y usan `financial.electronicdocuments.read`.
- El checklist no aprueba producción ni habilita mutaciones.
- Security production gate debe aprobar exposición de clave completa, custodia XAdES, Content/File productivo, redacción de logs y rollback antes de cualquier uso oficial.

## Sprint 4 closure

- Sin certificados reales en repositorio.
- Sin secretos reales, token real Portal, passwords ni connection strings privadas.
- Sin XML reales, datos personales reales ni respuestas reales del SRI.
- Payloads Content/File bloqueados por defecto; `dryRun=true` y `sendPayloads=false` son el estándar seguro.
- SRI Production y payload productivo siguen bloqueados hasta security production gate, certificate custody approval, evidencia SRI Test y aprobación operacional.

## Sprint 5 P1

Compras y anulados foundation no agregan secretos, certificados ni payloads XML. Los identificadores sensibles se devuelven enmascarados y el smoke usa datos sintéticos. Mantener bloqueado cualquier uso de datos reales, XML ATS oficial o evidencia productiva hasta aprobación externa.
- No descargar catálogos/XSD oficiales desde runtime.

## Sprint 5 P2

Mapping/readiness ATS es read-only y sanitizado. No debe exponer XML, access key completa, autorizaciones completas, identificación completa, secretos, certificados ni payloads. No se permite persistir evidencias ATS oficiales ni habilitar generación oficial desde estos endpoints.

## Sprint 5 P3

Catálogos foundation no contienen datos personales, XML, secretos, certificados, tokens ni URLs privadas. Las respuestas deben incluir disclaimer de no oficial final y no deben habilitar SRI producción, SRI Test real ni ATS oficial.

## Sprint 5 P4

ATS XML foundation preview está bloqueado por defecto. XML no se persiste, no se sube a Content/File, no se audita y no se loguea. El preview requiere permisos manage, acknowledgements explícitos y configuración local controlada.

## Sprint 5 P5

External approval workflow es read-only/advisory. No almacena evidencias reales, certificados, XML, tokens ni datos personales. No activa configuración productiva ni envíos SRI.
# Sprint 7 P2 approval evidence boundary

- No adjuntar certificados en requests de aprobación.
- No guardar `.p12`, `.pfx`, `.pem`, `.key`, `.cer` ni `.crt`.
- No pegar XML, base64 ni respuestas reales SRI.
- Evidence references deben apuntar a metadata sanitizada y futura integración Portal Content/File.
