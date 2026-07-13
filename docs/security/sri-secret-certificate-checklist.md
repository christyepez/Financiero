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
