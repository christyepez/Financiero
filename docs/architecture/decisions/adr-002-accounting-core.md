# ADR-002: Accounting Core Foundation

- Estado: Aceptado
- Fecha: 2026-07-06

## Decisión

1. Sprint 1 usa una moneda base ISO 4217 configurable por tenant; no soporta asientos multimoneda ni conversión FX.
2. Importes se almacenan como `decimal(19,4)`. Cada línea tiene exactamente débito o crédito positivo, nunca ambos; al contabilizar los totales deben coincidir exactamente a cuatro decimales. No existe auto-balanceo ni tolerancia distinta de cero en Sprint 1.
3. Cuentas forman un árbol por tenant. Código normalizado único, máximo 32 caracteres y formato configurable; el nivel deriva del padre. Solo cuentas activas y de movimiento admiten líneas.
4. Años y períodos no se solapan dentro del tenant. Un período puede ser Draft, Open o Closed; reabrir Closed requiere `financial.periods.reopen`, motivo y auditoría. Puede haber varios períodos Open si sus fechas no se solapan.
5. El asiento inicia Draft, es editable solo en Draft y se contabiliza en un período Open según `PostingDate`. Posted es inmutable.
6. El número se asigna atómicamente al contabilizar, con secuencia monotónica por tenant, año fiscal y tipo; se exige unicidad, no ausencia absoluta de huecos.
7. Void solo aplica a Draft y conserva el registro. Un Posted se corrige mediante un nuevo asiento reverso enlazado, con líneas invertidas y fecha dentro de un período Open. El original pasa a Reversed al contabilizar el reverso.
8. Posting, reverso, cierre/reapertura y cambios de cuentas generan evento Outbox en la misma transacción. Audit se entrega mediante adaptador/evento; una indisponibilidad remota no revierte una transacción local ya confirmada.
9. Persistencia financiera es independiente; no comparte tablas ni entidades con PortalCorporativo.

## Consecuencias

Las invariantes son deterministas y auditables, con concurrencia requerida para secuencias y cierres. Multimoneda, aprobaciones, cierres automáticos y reglas tributarias se difieren.
