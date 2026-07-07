using System.Text.Json;
using Financiero.Contracts;
using Financiero.Domain;

namespace Financiero.Application;

public interface IJournalEntryRepository
{
    Task AddAsync(JournalEntry entry, CancellationToken ct);
    Task AddLineAsync(JournalEntryLine line, CancellationToken ct);
    Task<JournalEntry?> GetByIdAsync(Guid id, string tenantId, CancellationToken ct);
    Task<JournalEntry?> GetByNumberAsync(string entryNumber, string tenantId, CancellationToken ct);
    Task<(IReadOnlyCollection<JournalEntry> Items, long Total)> SearchAsync(string tenantId, JournalEntryStatus? status, DateOnly? from, DateOnly? to, string? search, int page, int pageSize, CancellationToken ct);
    Task<string> GetNextEntryNumberAsync(string tenantId, int year, string prefix, int padding, CancellationToken ct);
    Task<bool> HasPostedEntriesForAccountAsync(Guid accountId, string tenantId, CancellationToken ct);
    Task<bool> HasDraftEntriesInPeriodAsync(Guid fiscalPeriodId, string tenantId, CancellationToken ct);
    Task<bool> HasDraftEntriesInDateRangeAsync(string tenantId, DateOnly startDate, DateOnly endDate, CancellationToken ct);
    Task<bool> HasPostedEntriesInPeriodAsync(Guid fiscalPeriodId, string tenantId, CancellationToken ct);
    Task<bool> HasPostedEntriesInFiscalYearAsync(Guid fiscalYearId, string tenantId, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}

public sealed class JournalEntriesService(
    IJournalEntryRepository entries,
    IAccountRepository accounts,
    IFiscalRepository fiscal,
    IFinancialConfigurationReader configuration,
    IPortalAuditClient audit,
    IPortalOutboxClient outbox)
{
    public async Task<JournalEntryDto> CreateAsync(CreateJournalEntryRequest request, PortalCallContext context, CancellationToken ct)
    {
        var entry = JournalEntry.Create(context.TenantId, request.PostingDate, ParseSource(request.Source), request.Reference, request.Description, DateTimeOffset.UtcNow);
        foreach (var line in request.Lines ?? [])
        {
            entry.AddLine(line.AccountId, line.Description, line.Debit, line.Credit, DateTimeOffset.UtcNow);
        }
        await entries.AddAsync(entry, ct);
        await entries.SaveChangesAsync(ct);
        await AuditOutboxAsync("JournalEntryCreated", "JournalEntryCreated", entry, context, ct);
        return ToDto(entry);
    }

    public async Task<JournalEntryDto> UpdateAsync(Guid id, UpdateJournalEntryRequest request, PortalCallContext context, CancellationToken ct)
    {
        var entry = await GetRequiredAsync(id, context.TenantId, ct);
        entry.Update(request.PostingDate, ParseSource(request.Source), request.Reference, request.Description, DateTimeOffset.UtcNow);
        await entries.SaveChangesAsync(ct);
        await AuditOutboxAsync("JournalEntryUpdated", "JournalEntryUpdated", entry, context, ct);
        return ToDto(entry);
    }

    public async Task<JournalEntryDto> AddLineAsync(Guid id, CreateJournalEntryLineRequest request, PortalCallContext context, CancellationToken ct)
    {
        var entry = await GetRequiredAsync(id, context.TenantId, ct);
        var line = entry.AddLine(request.AccountId, request.Description, request.Debit, request.Credit, DateTimeOffset.UtcNow);
        await entries.AddLineAsync(line, ct);
        await entries.SaveChangesAsync(ct);
        await AuditOnlyAsync("JournalEntryLineAdded", entry, context, ct);
        return ToDto(entry);
    }

    public async Task<JournalEntryDto> UpdateLineAsync(Guid id, Guid lineId, UpdateJournalEntryLineRequest request, PortalCallContext context, CancellationToken ct)
    {
        var entry = await GetRequiredAsync(id, context.TenantId, ct);
        entry.UpdateLine(lineId, request.AccountId, request.Description, request.Debit, request.Credit, DateTimeOffset.UtcNow);
        await entries.SaveChangesAsync(ct);
        await AuditOnlyAsync("JournalEntryLineUpdated", entry, context, ct);
        return ToDto(entry);
    }

    public async Task<JournalEntryDto> RemoveLineAsync(Guid id, Guid lineId, PortalCallContext context, CancellationToken ct)
    {
        var entry = await GetRequiredAsync(id, context.TenantId, ct);
        entry.RemoveLine(lineId, DateTimeOffset.UtcNow);
        await entries.SaveChangesAsync(ct);
        await AuditOnlyAsync("JournalEntryLineRemoved", entry, context, ct);
        return ToDto(entry);
    }

    public async Task<JournalEntryDto> PostAsync(Guid id, PortalCallContext context, CancellationToken ct)
    {
        var entry = await GetRequiredAsync(id, context.TenantId, ct);
        var period = await fiscal.GetOpenPeriodByDateAsync(context.TenantId, entry.PostingDate, ct)
            ?? throw new FinancialApplicationException("journal_entry.period.open.not_found", "Posting date must belong to an open fiscal period.");
        await ValidateLinesAsync(entry, context.TenantId, ct);
        var prefix = await configuration.GetStringAsync("financial.journalEntries.numbering.prefix", "JE", context, ct);
        var padding = await configuration.GetIntAsync("financial.journalEntries.numbering.padding", 6, context, ct);
        var number = await entries.GetNextEntryNumberAsync(context.TenantId, entry.PostingDate.Year, prefix, padding, ct);
        entry.Post(number, period.Id, DateTimeOffset.UtcNow);
        await entries.SaveChangesAsync(ct);
        await AuditOutboxAsync("JournalEntryPosted", "JournalEntryPosted", entry, context, ct);
        return ToDto(entry);
    }

    public async Task<JournalEntryDto> ReverseAsync(Guid id, ReverseJournalEntryRequest request, PortalCallContext context, CancellationToken ct)
    {
        var original = await GetRequiredAsync(id, context.TenantId, ct);
        var reversal = original.CreateReversalDraft(request.Reason, DateTimeOffset.UtcNow);
        await ValidateLinesAsync(reversal, context.TenantId, ct);
        var period = await fiscal.GetOpenPeriodByDateAsync(context.TenantId, reversal.PostingDate, ct)
            ?? throw new FinancialApplicationException("journal_entry.period.open.not_found", "Reversal posting date must belong to an open fiscal period.");
        var prefix = await configuration.GetStringAsync("financial.journalEntries.numbering.prefix", "JE", context, ct);
        var padding = await configuration.GetIntAsync("financial.journalEntries.numbering.padding", 6, context, ct);
        var number = await entries.GetNextEntryNumberAsync(context.TenantId, reversal.PostingDate.Year, prefix, padding, ct);
        reversal.Post(number, period.Id, DateTimeOffset.UtcNow);
        await entries.AddAsync(reversal, ct);
        original.MarkReversed(reversal.Id, request.Reason, DateTimeOffset.UtcNow);
        await entries.SaveChangesAsync(ct);
        await AuditOutboxAsync("JournalEntryReversed", "JournalEntryReversed", original, context, ct);
        return ToDto(reversal);
    }

    public async Task<JournalEntryDto> VoidAsync(Guid id, VoidJournalEntryRequest request, PortalCallContext context, CancellationToken ct)
    {
        var entry = await GetRequiredAsync(id, context.TenantId, ct);
        var allowVoidDraft = await configuration.GetBoolAsync("financial.journalEntries.allowVoidDraft", true, context, ct);
        if (!allowVoidDraft) throw new FinancialApplicationException("journal_entry.void.disabled", "Voiding draft journal entries is disabled by configuration.");
        entry.Void(request.Reason, DateTimeOffset.UtcNow);
        await entries.SaveChangesAsync(ct);
        await AuditOutboxAsync("JournalEntryVoided", "JournalEntryVoided", entry, context, ct);
        return ToDto(entry);
    }

    public async Task<JournalEntryDto> GetByIdAsync(Guid id, PortalCallContext context, CancellationToken ct) => ToDto(await GetRequiredAsync(id, context.TenantId, ct));
    public async Task<JournalEntryDto> GetByNumberAsync(string entryNumber, PortalCallContext context, CancellationToken ct) =>
        ToDto(await entries.GetByNumberAsync(entryNumber.Trim(), context.TenantId, ct) ?? throw new FinancialApplicationException("journal_entry.not_found", "Journal entry was not found."));

    public async Task<PageResponse<JournalEntryDto>> SearchAsync(SearchJournalEntriesRequest request, PortalCallContext context, CancellationToken ct)
    {
        JournalEntryStatus? status = string.IsNullOrWhiteSpace(request.Status) ? null : ParseStatus(request.Status);
        var page = Math.Max(1, request.Page);
        var size = Math.Clamp(request.PageSize, 1, 100);
        var (items, total) = await entries.SearchAsync(context.TenantId, status, request.From, request.To, request.Search, page, size, ct);
        return new(items.Select(ToDto).ToArray(), page, size, total);
    }

    private async Task ValidateLinesAsync(JournalEntry entry, string tenantId, CancellationToken ct)
    {
        entry.ValidatePostable();
        foreach (var line in entry.Lines)
        {
            var account = await accounts.GetByIdAsync(line.AccountId, tenantId, ct)
                ?? throw new FinancialApplicationException("journal_entry.account.not_found", "Journal entry account was not found.");
            if (account.TenantId != tenantId) throw new FinancialApplicationException("journal_entry.account.tenant", "Journal entry account belongs to a different tenant.");
            if (account.Status != AccountStatus.Active) throw new FinancialApplicationException("journal_entry.account.inactive", "Journal entry account must be active.");
            if (!account.IsMovementAccount) throw new FinancialApplicationException("journal_entry.account.summary", "Journal entry account must be a movement account.");
        }
    }

    private async Task<JournalEntry> GetRequiredAsync(Guid id, string tenantId, CancellationToken ct) =>
        await entries.GetByIdAsync(id, tenantId, ct) ?? throw new FinancialApplicationException("journal_entry.not_found", "Journal entry was not found.");

    private async Task AuditOnlyAsync(string auditAction, JournalEntry entry, PortalCallContext context, CancellationToken ct) =>
        await audit.RecordAsync(new(auditAction, "financial.journal-entry", entry.Id.ToString(), new { entry.Id, entry.EntryNumber, entry.Status }), context, ct);

    private async Task AuditOutboxAsync(string auditAction, string outboxType, JournalEntry entry, PortalCallContext context, CancellationToken ct)
    {
        var payload = JsonSerializer.Serialize(new { entry.Id, entry.TenantId, entry.EntryNumber, Status = entry.Status.ToString(), entry.PostingDate });
        await audit.RecordAsync(new(auditAction, "financial.journal-entry", entry.Id.ToString(), new { entry.Id, entry.EntryNumber, entry.Status, entry.PostingDate }), context, ct);
        await outbox.EnqueueAsync(new(Guid.NewGuid(), outboxType, 1, DateTimeOffset.UtcNow, context.CorrelationId, payload), context, ct);
    }

    public static JournalEntryDto ToDto(JournalEntry entry) => new(
        entry.Id, entry.TenantId, entry.FiscalPeriodId, entry.EntryNumber, entry.PostingDate, entry.Reference, entry.Description,
        entry.Status.ToString(), entry.Source.ToString(), entry.ReversalOfJournalEntryId, entry.ReversedByJournalEntryId,
        entry.TotalDebit, entry.TotalCredit, entry.Lines.Select(x => new JournalEntryLineDto(x.Id, x.AccountId, x.LineNumber, x.Description, x.Debit, x.Credit)).ToArray(),
        entry.CreatedAtUtc, entry.UpdatedAtUtc);

    private static JournalEntrySource ParseSource(string value) => Enum.TryParse<JournalEntrySource>(value, true, out var source) ? source : throw new FinancialApplicationException("journal_entry.source.invalid", "Journal entry source is invalid.");
    private static JournalEntryStatus ParseStatus(string value) => Enum.TryParse<JournalEntryStatus>(value, true, out var status) ? status : throw new FinancialApplicationException("journal_entry.status.invalid", "Journal entry status is invalid.");
}

public static class JournalEntriesPortalMetadata
{
    public static readonly string[] Permissions =
    [
        "financial.journalentries.read", "financial.journalentries.create", "financial.journalentries.update",
        "financial.journalentries.post", "financial.journalentries.reverse", "financial.journalentries.void", "financial.journalentries.manage"
    ];

    public static readonly string[] ConfigurationKeys =
    [
        "financial.journalEntries.numbering.scope", "financial.journalEntries.numbering.prefix", "financial.journalEntries.numbering.padding",
        "financial.journalEntries.allowDraftWithoutLines", "financial.journalEntries.allowVoidDraft", "financial.journalEntries.reverseCreatesNewEntry",
        "financial.journalEntries.decimalPrecision", "financial.journalEntries.roundingMode"
    ];
}
