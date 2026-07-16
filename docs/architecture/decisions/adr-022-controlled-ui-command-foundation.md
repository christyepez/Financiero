# ADR-022 - Controlled UI Command Foundation

## Estado

Aprobado para Sprint 6 P4.

## Contexto

El frontend financiero ya consume datos reales sanitizados y tiene contrato foundation con Portal Shell. Se requiere habilitar comandos internos foundation sin convertir la UI en una herramienta productiva fiscal.

## Decisión

Agregar comandos UI controlados para compras y anulados:

- Deshabilitados por defecto mediante feature flags.
- Protegidos por permiso `financial.electronicdocuments.manage`.
- Disponibles solo como foundation no oficial.
- Sin envío SRI, ATS oficial, XML preview ni workflow productivo.

## Consecuencias

- El sistema puede probar flujos foundation desde Angular.
- El modo read-only sigue siendo el default seguro.
- No se agregan migraciones ni cambios backend.

## Restricciones

- No producción SRI.
- No SRI Test real.
- No certificados.
- No XML real.
- No token storage.
- No aprobaciones productivas.
