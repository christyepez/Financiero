# Sprint 1 P5 — Integrated QA / SQL Concurrency / Security Smoke / Migration Versioning

## Resultado

P5 agrega cierre QA para Accounting Core: flujo integrado, security smoke, migraciones versionadas, readiness de DB, script smoke y contratos API.

## Validaciones agregadas

- Flujo contable mínimo en `IntegratedAccountingFlowTests`.
- Security smoke ampliado para permisos por endpoint y wildcard dev.
- Audit/Outbox críticos validados en flujo integrado.
- Configuration QA para prefix/padding, allowVoidDraft y closeRequiresNoDraftEntries.
- Readiness valida conexión y tablas core.

## Migraciones

Se crea `database/migrations/financial` con scripts `001` a `006` y runner básico con `financial.schema_versions`. Docker habilita `Database__RunMigrations=true` sin eliminar compatibilidad con `Database__Initialize=true`.

## Concurrencia

La secuencia contable usa transacción serializable y `UPDLOCK,HOLDLOCK`. Se mantiene gap-tolerant; no se aceptan duplicados.

## Smoke local

Ejecutar:

```powershell
scripts/smoke/financial-smoke.ps1 -BaseUrl http://localhost:8083
```

El script usa `X-Dev-Permissions: financial.*` y crea datos únicos de prueba.

## Cierre

Sprint 1 queda listo para revisión de cierre/P6: QA final documental y decisión de avanzar a SRI/facturación sin duplicar Portal.
