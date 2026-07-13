# Sprint 2 SRI Readiness Closure

## Objetivo

Cerrar Sprint 2 como readiness técnico para facturación electrónica Ecuador/SRI en Financiero, sin habilitar producción, sin certificados reales y sin duplicar capacidades transversales del Portal.

## Implementado P1-P5

- P1: foundation de `ElectronicDocument`, factura electrónica, líneas, impuestos, secuencias, clave de acceso SRI con módulo 11, XML base, permisos runtime y clientes SRI mock/dev.
- P2: estrategia XAdES, contrato SRI Test/mock, validación XML, storage XML/PDF delegado por puerto, metadata de firma/storage.
- P3: foundation XAdES, SOAP Test dry-run, Portal Content/File contract, RIDE/PDF Development.
- P4: Secret Store port, certificate provider por referencia, readiness SRI, sanitización, integration status.
- P5: wiring realista de Key Vault, probe manual SRI Test, observabilidad sanitizada y contrato Content/File estabilizado.

## Capacidades listas

- Flujo mock/dev: draft invoice, line, XML, validate, sign dev, send mock, authorize mock, RIDE placeholder, storage metadata.
- Endpoints SRI readiness e integration status.
- Contratos para firma, certificados, secret store, SRI reception/authorization, Content/File y observabilidad.
- Configuración segura por defecto: Production bloqueado, dry-run activo y payloads deshabilitados.

## Capacidades bloqueadas

- Firma XAdES real.
- Certificados reales y secretos reales.
- SRI Test real send.
- SRI producción.
- Portal Content/File productivo con payload real.
- RIDE tributario final.

## Estado de seguridad

No hay certificados, secretos, XML reales, datos personales reales, tokens ni URLs privadas sensibles en el repositorio. `X-Dev-Permissions` sigue limitado a Development. Production ignora headers dev.

## Estado producción

Producción SRI NO está habilitada. `financial.sri.allowProduction=false` permanece como gate obligatorio.

## Docker / health / smoke

`docker compose config` fue validado en P3-P5. `docker compose up -d --build`, health y smoke quedaron bloqueados por timeout externo de `mcr.microsoft.com/dotnet/aspnet:8.0`; no se declaran OK.

## Estado SRI Test

SRI Test real requiere validación manual fuera del repositorio con credenciales no productivas, certificado de pruebas y configuración externa.

## Estado Secret Store

Puerto y readiness están listos. Azure Key Vault sigue como placeholder implementable sin SDK/credenciales reales en repo.

## Estado XAdES

XAdES real NO está habilitado. Existe ruta segura Secret Store -> Certificate Provider -> XAdES Service, pero la firma criptográfica real queda diferida.

## Estado Portal Content/File

El contrato consumidor está estabilizado con metadata/hash y payload opcional. Productivo requiere contrato final del Portal y `sendPayloads=true` aprobado.

## Recomendación Sprint 3

Si no hay credenciales/certificados SRI Test listos, avanzar con NC/ND/Retenciones Foundation. Si ya existen Key Vault/certificado/credenciales no productivas, priorizar Real Key Vault + XAdES + SRI Test Controlled Send.
