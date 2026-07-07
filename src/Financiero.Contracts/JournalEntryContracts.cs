namespace Financiero.Contracts;

public sealed record JournalEntryDto(
    Guid Id,
    string TenantId,
    Guid? FiscalPeriodId,
    string? EntryNumber,
    DateOnly PostingDate,
    string? Reference,
    string Description,
    string Status,
    string Source,
    Guid? ReversalOfJournalEntryId,
    Guid? ReversedByJournalEntryId,
    decimal TotalDebit,
    decimal TotalCredit,
    IReadOnlyCollection<JournalEntryLineDto> Lines,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);

public sealed record JournalEntryLineDto(Guid Id, Guid AccountId, int LineNumber, string? Description, decimal Debit, decimal Credit);
public sealed record CreateJournalEntryRequest(DateOnly PostingDate, string Source, string? Reference, string Description, IReadOnlyCollection<CreateJournalEntryLineRequest>? Lines = null);
public sealed record UpdateJournalEntryRequest(DateOnly PostingDate, string Source, string? Reference, string Description);
public sealed record CreateJournalEntryLineRequest(Guid AccountId, string? Description, decimal Debit, decimal Credit);
public sealed record UpdateJournalEntryLineRequest(Guid AccountId, string? Description, decimal Debit, decimal Credit);
public sealed record ReverseJournalEntryRequest(string Reason);
public sealed record VoidJournalEntryRequest(string Reason);
public sealed record SearchJournalEntriesRequest(string? Status = null, DateOnly? From = null, DateOnly? To = null, string? Search = null, int Page = 1, int PageSize = 20);
