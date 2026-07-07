# Accounting Sequence Concurrency Strategy

## P5 result

`EfJournalEntryRepository.GetNextEntryNumberAsync` now obtains numbers through SQL Server using:

- `IsolationLevel.Serializable`.
- `UPDLOCK, HOLDLOCK` on `financial.accounting_sequences`.
- Unique index `(TenantId, SequenceKey)`.
- Unique index `(TenantId, EntryNumber)` on `financial.journal_entries`.

This prevents two concurrent callers from reading the same `NextValue` for the same tenant/year/prefix sequence.

## Remaining risk

The sequence reservation and final journal posting are not yet one single application-level transaction spanning all changes. This is intentionally gap-tolerant: a failed post may consume a number. P5 accepts gaps, not duplicates.

## P6/P7 recommendation

If stress tests show contention, evaluate:

- rowversion on `financial.accounting_sequences`;
- retry on duplicate key exceptions;
- `sp_getapplock` per `{TenantId}:{SequenceKey}`;
- explicit unit-of-work transaction around sequence + journal post.
