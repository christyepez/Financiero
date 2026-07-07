# Sprint 1 P3 — Journal Entries Foundation

## Resultado

P3 implementa la base de asientos contables sin duplicar PortalCorporativo ni crear SQL Server propio. Usa `FinancieroDb` en el SQL común local y mantiene integración vía puertos Portal existentes.

## Incluido

- Dominio: `JournalEntry`, `JournalEntryLine`, estados Draft/Posted/Reversed/Voided y fuentes Manual/OpeningBalance/Adjustment/Integration/System.
- Reglas: partida doble exacta, mínimo dos líneas para post, una sola columna débito/crédito positiva por línea, Posted inmutable, Void solo Draft, Reverse por asiento compensatorio.
- Aplicación: create/update, líneas add/update/remove, post, reverse, void, get by id, get by number y search paginado.
- Integración: validación contra cuentas P1 y periodos P2; Audit/Outbox por adaptadores Portal dev.
- Persistencia: `financial.journal_entries`, `financial.journal_entry_lines`, `financial.accounting_sequences`.
- API: `/api/financial/journal-entries`.
- Metadata Portal: permisos `financial.journalentries.*`, menú `Asientos contables`, configuración de numeración/precisión/reverso.

## No incluido

- Frontend/Angular Shell.
- Facturación, SRI, XML, firma, RIDE, ATS.
- Workers productivos.
- Bloqueo final de desactivar/archivar cuentas usadas en Posted; queda interfaz preparada para P4/P5.

## Validación

- `dotnet build Financiero.sln`
- `$env:DOTNET_ROLL_FORWARD='Major'; dotnet test Financiero.sln`

Resultado esperado actual: 47 pruebas superadas.

## Riesgos

- Secuencia tenant/año es foundation y gap-tolerant; requiere prueba de concurrencia SQL en P5.
- Audit/Outbox son adaptadores dev; integración productiva depende de Portal Sprint 2/P4.
- La regla de no cerrar periodo con Draft entries debe conectarse en P4/P5 si se activa por configuración.
