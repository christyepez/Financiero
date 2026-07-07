using Financiero.Application;
using Financiero.Contracts;
using Financiero.Domain;
using Xunit;

namespace Financiero.Application.Tests;

public sealed class JournalEntriesServiceTests
{
    private static PortalCallContext Context => new("default", "corr-journal");

    [Fact]
    public async Task Rejects_post_without_open_period()
    {
        var harness = await NewHarnessAsync(openPeriod: false);
        var entry = await CreateBalancedDraftAsync(harness);
        await Assert.ThrowsAsync<FinancialApplicationException>(() => harness.Service.PostAsync(entry.Id, Context, default));
    }

    [Fact]
    public async Task Rejects_missing_inactive_or_summary_account()
    {
        var repo = new InMemoryJournalEntryRepository();
        var accounts = new InMemoryAccountRepository();
        var fiscal = new InMemoryFiscalRepository();
        await OpenPeriodAsync(fiscal);
        var service = new JournalEntriesService(repo, accounts, fiscal, new StaticFinancialConfigurationReader(), new RecordingAudit(), new RecordingOutbox());
        var missing = await service.CreateAsync(new(new DateOnly(2026, 1, 15), "Manual", null, "Missing", [new(Guid.NewGuid(), null, 10, 0), new(Guid.NewGuid(), null, 0, 10)]), Context, default);
        await Assert.ThrowsAsync<FinancialApplicationException>(() => service.PostAsync(missing.Id, Context, default));

        var inactive = await AddAccountAsync(accounts, "1", active: false, movement: true);
        var active = await AddAccountAsync(accounts, "2", active: true, movement: true);
        var inactiveEntry = await service.CreateAsync(new(new DateOnly(2026, 1, 15), "Manual", null, "Inactive", [new(inactive.Id, null, 10, 0), new(active.Id, null, 0, 10)]), Context, default);
        await Assert.ThrowsAsync<FinancialApplicationException>(() => service.PostAsync(inactiveEntry.Id, Context, default));

        var summary = await AddAccountAsync(accounts, "3", active: true, movement: false);
        var summaryEntry = await service.CreateAsync(new(new DateOnly(2026, 1, 15), "Manual", null, "Summary", [new(summary.Id, null, 10, 0), new(active.Id, null, 0, 10)]), Context, default);
        await Assert.ThrowsAsync<FinancialApplicationException>(() => service.PostAsync(summaryEntry.Id, Context, default));
    }

    [Fact]
    public async Task Posts_successfully_assigns_number_period_audit_and_outbox()
    {
        var audit = new RecordingAudit();
        var outbox = new RecordingOutbox();
        var harness = await NewHarnessAsync(audit: audit, outbox: outbox);
        var entry = await CreateBalancedDraftAsync(harness);

        var posted = await harness.Service.PostAsync(entry.Id, Context, default);

        Assert.Equal("Posted", posted.Status);
        Assert.NotNull(posted.FiscalPeriodId);
        Assert.Equal("JE-2026-000001", posted.EntryNumber);
        Assert.Contains("JournalEntryPosted", audit.Actions);
        Assert.Contains("JournalEntryPosted", outbox.EventTypes);
    }

    [Fact]
    public async Task Reverse_creates_posted_reversal_and_events()
    {
        var audit = new RecordingAudit();
        var outbox = new RecordingOutbox();
        var harness = await NewHarnessAsync(audit: audit, outbox: outbox);
        var entry = await CreateBalancedDraftAsync(harness);
        var posted = await harness.Service.PostAsync(entry.Id, Context, default);

        var reversal = await harness.Service.ReverseAsync(posted.Id, new("wrong amount"), Context, default);
        var original = await harness.Service.GetByIdAsync(posted.Id, Context, default);

        Assert.Equal("Posted", reversal.Status);
        Assert.Equal(posted.Id, reversal.ReversalOfJournalEntryId);
        Assert.Equal(reversal.Id, original.ReversedByJournalEntryId);
        Assert.Contains("JournalEntryReversed", audit.Actions);
        Assert.Contains("JournalEntryReversed", outbox.EventTypes);
    }

    [Fact]
    public async Task Void_only_draft_and_search_by_number_are_supported()
    {
        var harness = await NewHarnessAsync();
        var draft = await CreateBalancedDraftAsync(harness);
        var voided = await harness.Service.VoidAsync(draft.Id, new("duplicate"), Context, default);
        Assert.Equal("Voided", voided.Status);

        var entry = await CreateBalancedDraftAsync(harness);
        var posted = await harness.Service.PostAsync(entry.Id, Context, default);
        var byNumber = await harness.Service.GetByNumberAsync(posted.EntryNumber!, Context, default);
        var page = await harness.Service.SearchAsync(new SearchJournalEntriesRequest(Status: "Posted", Page: 1, PageSize: 1), Context, default);

        Assert.Equal(posted.Id, byNumber.Id);
        Assert.Single(page.Items);
        await Assert.ThrowsAsync<FinancialDomainException>(() => harness.Service.VoidAsync(posted.Id, new("no"), Context, default));
    }

    [Fact]
    public async Task Void_respects_runtime_configuration()
    {
        var harness = await NewHarnessAsync(configuration: new StaticFinancialConfigurationReader(new Dictionary<string, string>
        {
            ["financial.journalEntries.allowVoidDraft"] = "false"
        }));
        var draft = await CreateBalancedDraftAsync(harness);
        await Assert.ThrowsAsync<FinancialApplicationException>(() => harness.Service.VoidAsync(draft.Id, new("blocked"), Context, default));
    }

    [Fact]
    public async Task Post_uses_configured_numbering_format()
    {
        var harness = await NewHarnessAsync(configuration: new StaticFinancialConfigurationReader(new Dictionary<string, string>
        {
            ["financial.journalEntries.numbering.prefix"] = "FIN",
            ["financial.journalEntries.numbering.padding"] = "4"
        }));
        var draft = await CreateBalancedDraftAsync(harness);
        var posted = await harness.Service.PostAsync(draft.Id, Context, default);
        Assert.Equal("FIN-2026-0001", posted.EntryNumber);
    }

    [Fact]
    public async Task Line_crud_keeps_draft_editable()
    {
        var harness = await NewHarnessAsync();
        var entry = await harness.Service.CreateAsync(new(new DateOnly(2026, 1, 15), "Manual", "REF", "Draft"), Context, default);
        var firstAccount = harness.DebitAccountId;
        var secondAccount = harness.CreditAccountId;

        entry = await harness.Service.AddLineAsync(entry.Id, new(firstAccount, "d", 5, 0), Context, default);
        var lineId = entry.Lines.Single().Id;
        entry = await harness.Service.UpdateLineAsync(entry.Id, lineId, new(firstAccount, "d2", 10, 0), Context, default);
        entry = await harness.Service.AddLineAsync(entry.Id, new(secondAccount, "c", 0, 10), Context, default);
        entry = await harness.Service.RemoveLineAsync(entry.Id, lineId, Context, default);

        Assert.Single(entry.Lines);
        Assert.Equal(1, entry.Lines.Single().LineNumber);
    }

    private sealed record TestHarness(JournalEntriesService Service, Guid DebitAccountId, Guid CreditAccountId);

    private static async Task<TestHarness> NewHarnessAsync(bool openPeriod = true, RecordingAudit? audit = null, RecordingOutbox? outbox = null, IFinancialConfigurationReader? configuration = null)
    {
        var accounts = new InMemoryAccountRepository();
        var debit = await AddAccountAsync(accounts, "1");
        var credit = await AddAccountAsync(accounts, "2");
        var fiscal = new InMemoryFiscalRepository();
        if (openPeriod) await OpenPeriodAsync(fiscal);
        return new(new JournalEntriesService(new InMemoryJournalEntryRepository(), accounts, fiscal, configuration ?? new StaticFinancialConfigurationReader(), audit ?? new RecordingAudit(), outbox ?? new RecordingOutbox()), debit.Id, credit.Id);
    }

    private static async Task<JournalEntryDto> CreateBalancedDraftAsync(TestHarness harness)
    {
        return await harness.Service.CreateAsync(new(new DateOnly(2026, 1, 15), "Manual", "REF", "Entry", [new(harness.DebitAccountId, null, 10, 0), new(harness.CreditAccountId, null, 0, 10)]), Context, default);
    }

    private static async Task<Account> AddAccountAsync(InMemoryAccountRepository repo, string code, bool active = true, bool movement = true)
    {
        var account = Account.Create("default", code, $"Cuenta {code}", AccountType.Asset, 1, null, movement, DateTimeOffset.UtcNow);
        if (active) account.Activate(DateTimeOffset.UtcNow);
        await repo.AddAsync(account, default);
        return account;
    }

    private static async Task OpenPeriodAsync(InMemoryFiscalRepository repo)
    {
        var year = FiscalYear.Create("default", 2026, new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31), DateTimeOffset.UtcNow);
        year.Open(DateTimeOffset.UtcNow);
        var period = FiscalPeriod.Create("default", year.Id, 1, new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31), DateTimeOffset.UtcNow);
        period.Open(DateTimeOffset.UtcNow);
        await repo.AddYearAsync(year, default);
        await repo.AddPeriodAsync(period, default);
    }
}

internal sealed class InMemoryJournalEntryRepository : IJournalEntryRepository
{
    private readonly List<JournalEntry> _entries = [];
    private readonly Dictionary<string, long> _sequences = [];
    public Task AddAsync(JournalEntry entry, CancellationToken ct) { _entries.Add(entry); return Task.CompletedTask; }
    public void Track(JournalEntry entry) => _entries.Add(entry);
    public Task<JournalEntry?> GetByIdAsync(Guid id, string tenantId, CancellationToken ct) => Task.FromResult(_entries.FirstOrDefault(x => x.Id == id && x.TenantId == tenantId));
    public Task<JournalEntry?> GetByNumberAsync(string entryNumber, string tenantId, CancellationToken ct) => Task.FromResult(_entries.FirstOrDefault(x => x.EntryNumber == entryNumber && x.TenantId == tenantId));
    public Task<(IReadOnlyCollection<JournalEntry> Items, long Total)> SearchAsync(string tenantId, JournalEntryStatus? status, DateOnly? from, DateOnly? to, string? search, int page, int pageSize, CancellationToken ct)
    {
        var query = _entries.Where(x => x.TenantId == tenantId);
        if (status.HasValue) query = query.Where(x => x.Status == status);
        if (from.HasValue) query = query.Where(x => x.PostingDate >= from.Value);
        if (to.HasValue) query = query.Where(x => x.PostingDate <= to.Value);
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(x => x.Description.Contains(search) || x.EntryNumber == search || x.Reference == search);
        var list = query.OrderByDescending(x => x.PostingDate).ToArray();
        return Task.FromResult(((IReadOnlyCollection<JournalEntry>)list.Skip((page - 1) * pageSize).Take(pageSize).ToArray(), (long)list.Length));
    }
    public Task<string> GetNextEntryNumberAsync(string tenantId, int year, string prefix, int padding, CancellationToken ct)
    {
        prefix = string.IsNullOrWhiteSpace(prefix) ? "JE" : prefix.Trim().ToUpperInvariant();
        padding = Math.Clamp(padding, 1, 12);
        var key = $"{tenantId}-{year}";
        _sequences[key] = _sequences.TryGetValue(key, out var next) ? next : 1;
        var value = _sequences[key]++;
        return Task.FromResult($"{prefix}-{year}-{value.ToString().PadLeft(padding, '0')}");
    }
    public Task<bool> HasPostedEntriesForAccountAsync(Guid accountId, string tenantId, CancellationToken ct) => Task.FromResult(_entries.Any(x => x.TenantId == tenantId && x.Status == JournalEntryStatus.Posted && x.Lines.Any(l => l.AccountId == accountId)));
    public Task<bool> HasDraftEntriesInPeriodAsync(Guid fiscalPeriodId, string tenantId, CancellationToken ct) => Task.FromResult(_entries.Any(x => x.TenantId == tenantId && x.FiscalPeriodId == fiscalPeriodId && x.Status == JournalEntryStatus.Draft));
    public Task<bool> HasDraftEntriesInDateRangeAsync(string tenantId, DateOnly startDate, DateOnly endDate, CancellationToken ct) => Task.FromResult(_entries.Any(x => x.TenantId == tenantId && x.Status == JournalEntryStatus.Draft && x.PostingDate >= startDate && x.PostingDate <= endDate));
    public Task<bool> HasPostedEntriesInPeriodAsync(Guid fiscalPeriodId, string tenantId, CancellationToken ct) => Task.FromResult(_entries.Any(x => x.TenantId == tenantId && x.FiscalPeriodId == fiscalPeriodId && x.Status is JournalEntryStatus.Posted or JournalEntryStatus.Reversed));
    public Task<bool> HasPostedEntriesInFiscalYearAsync(Guid fiscalYearId, string tenantId, CancellationToken ct) => Task.FromResult(false);
    public Task SaveChangesAsync(CancellationToken ct) => Task.CompletedTask;
}
