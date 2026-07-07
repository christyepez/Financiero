using Financiero.Application;
using Financiero.Contracts;
using Financiero.Domain;
using Xunit;

namespace Financiero.Application.Tests;

public sealed class FiscalPeriodsServiceTests
{
    private static PortalCallContext Context => new("default", "corr-fiscal");

    [Fact]
    public async Task Creates_fiscal_year_and_invokes_audit_outbox()
    {
        var audit = new RecordingAudit();
        var outbox = new RecordingOutbox();
        var service = new FiscalPeriodsService(new InMemoryFiscalRepository(), new InMemoryJournalEntryRepository(), new StaticFinancialConfigurationReader(), audit, outbox);

        var year = await service.CreateYearAsync(new(2026, new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31)), Context, default);

        Assert.Equal(2026, year.Year);
        Assert.Contains("FiscalYearCreated", audit.Actions);
        Assert.Contains("FiscalYearCreated", outbox.EventTypes);
    }

    [Fact]
    public async Task Rejects_duplicate_year()
    {
        var service = NewService(new InMemoryFiscalRepository());
        await service.CreateYearAsync(new(2026, new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31)), Context, default);
        await Assert.ThrowsAsync<FinancialApplicationException>(() => service.CreateYearAsync(new(2026, new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31)), Context, default));
    }

    [Fact]
    public async Task Opens_and_closes_year_without_open_periods()
    {
        var service = NewService(new InMemoryFiscalRepository());
        var year = await service.CreateYearAsync(new(2026, new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31)), Context, default);
        await service.OpenYearAsync(year.Id, Context, default);
        var closed = await service.CloseYearAsync(year.Id, Context, default);
        Assert.Equal("Closed", closed.Status);
    }

    [Fact]
    public async Task Rejects_close_year_with_open_periods_and_archive_open_year()
    {
        var service = NewService(new InMemoryFiscalRepository());
        var year = await service.CreateYearAsync(new(2026, new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31)), Context, default);
        await service.OpenYearAsync(year.Id, Context, default);
        var period = await service.CreatePeriodAsync(new(year.Id, 1, new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31)), Context, default);
        await service.OpenPeriodAsync(period.Id, Context, default);

        await Assert.ThrowsAsync<FinancialApplicationException>(() => service.CloseYearAsync(year.Id, Context, default));
        await Assert.ThrowsAsync<FinancialDomainException>(() => service.ArchiveYearAsync(year.Id, Context, default));
    }

    [Fact]
    public async Task Creates_period_and_rejects_duplicate_or_overlap()
    {
        var service = NewService(new InMemoryFiscalRepository());
        var year = await service.CreateYearAsync(new(2026, new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31)), Context, default);
        await service.CreatePeriodAsync(new(year.Id, 1, new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31)), Context, default);

        await Assert.ThrowsAsync<FinancialApplicationException>(() => service.CreatePeriodAsync(new(year.Id, 1, new DateOnly(2026, 2, 1), new DateOnly(2026, 2, 28)), Context, default));
        await Assert.ThrowsAsync<FinancialApplicationException>(() => service.CreatePeriodAsync(new(year.Id, 2, new DateOnly(2026, 1, 15), new DateOnly(2026, 2, 15)), Context, default));
    }

    [Fact]
    public async Task Opens_period_only_when_year_is_open()
    {
        var service = NewService(new InMemoryFiscalRepository());
        var year = await service.CreateYearAsync(new(2026, new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31)), Context, default);
        var period = await service.CreatePeriodAsync(new(year.Id, 1, new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31)), Context, default);

        await Assert.ThrowsAsync<FinancialApplicationException>(() => service.OpenPeriodAsync(period.Id, Context, default));
        await service.OpenYearAsync(year.Id, Context, default);
        var opened = await service.OpenPeriodAsync(period.Id, Context, default);
        Assert.Equal("Open", opened.Status);
    }

    [Fact]
    public async Task Closes_reopens_locks_and_rejects_locked_reopen()
    {
        var service = NewService(new InMemoryFiscalRepository());
        var year = await service.CreateYearAsync(new(2026, new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31)), Context, default);
        await service.OpenYearAsync(year.Id, Context, default);
        var period = await service.CreatePeriodAsync(new(year.Id, 1, new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31)), Context, default);
        await service.OpenPeriodAsync(period.Id, Context, default);
        await service.ClosePeriodAsync(period.Id, Context, default);
        var reopened = await service.ReopenPeriodAsync(period.Id, Context, default);
        Assert.Equal("Open", reopened.Status);
        await service.LockPeriodAsync(period.Id, Context, default);
        await Assert.ThrowsAsync<FinancialDomainException>(() => service.ReopenPeriodAsync(period.Id, Context, default));
    }

    [Fact]
    public async Task Finds_open_period_by_date_and_searches_paginated()
    {
        var service = NewService(new InMemoryFiscalRepository());
        var year = await service.CreateYearAsync(new(2026, new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31)), Context, default);
        await service.OpenYearAsync(year.Id, Context, default);
        var p1 = await service.CreatePeriodAsync(new(year.Id, 1, new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31)), Context, default);
        await service.CreatePeriodAsync(new(year.Id, 2, new DateOnly(2026, 2, 1), new DateOnly(2026, 2, 28)), Context, default);
        await service.OpenPeriodAsync(p1.Id, Context, default);

        var open = await service.GetOpenPeriodByDateAsync(new DateOnly(2026, 1, 15), Context, default);
        var page = await service.SearchPeriodsAsync(new SearchFiscalPeriodsRequest(Page: 1, PageSize: 1), Context, default);
        Assert.Equal(p1.Id, open.Id);
        Assert.Single(page.Items);
        Assert.Equal(2, page.Total);
    }

    [Fact]
    public async Task Rejects_close_or_lock_period_with_draft_journal_entries_when_config_enabled()
    {
        var repo = new InMemoryFiscalRepository();
        var entries = new InMemoryJournalEntryRepository();
        var service = NewService(repo, entries, new StaticFinancialConfigurationReader(new Dictionary<string, string>
        {
            ["financial.fiscalPeriods.closeRequiresNoDraftEntries"] = "true"
        }));
        var year = await service.CreateYearAsync(new(2026, new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31)), Context, default);
        await service.OpenYearAsync(year.Id, Context, default);
        var period = await service.CreatePeriodAsync(new(year.Id, 1, new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31)), Context, default);
        await service.OpenPeriodAsync(period.Id, Context, default);
        entries.Track(JournalEntry.Create("default", new DateOnly(2026, 1, 15), JournalEntrySource.Manual, null, "draft", DateTimeOffset.UtcNow));

        await Assert.ThrowsAsync<FinancialApplicationException>(() => service.ClosePeriodAsync(period.Id, Context, default));
        await Assert.ThrowsAsync<FinancialApplicationException>(() => service.LockPeriodAsync(period.Id, Context, default));
    }

    private static FiscalPeriodsService NewService(InMemoryFiscalRepository repo, InMemoryJournalEntryRepository? entries = null, IFinancialConfigurationReader? configuration = null) =>
        new(repo, entries ?? new InMemoryJournalEntryRepository(), configuration ?? new StaticFinancialConfigurationReader(), new RecordingAudit(), new RecordingOutbox());
}

internal sealed class InMemoryFiscalRepository : IFiscalRepository
{
    private readonly List<FiscalYear> _years = [];
    private readonly List<FiscalPeriod> _periods = [];
    public Task AddYearAsync(FiscalYear year, CancellationToken ct) { _years.Add(year); return Task.CompletedTask; }
    public Task AddPeriodAsync(FiscalPeriod period, CancellationToken ct) { _periods.Add(period); return Task.CompletedTask; }
    public Task<FiscalYear?> GetYearByIdAsync(Guid id, string tenantId, CancellationToken ct) => Task.FromResult(_years.FirstOrDefault(x => x.Id == id && x.TenantId == tenantId));
    public Task<FiscalPeriod?> GetPeriodByIdAsync(Guid id, string tenantId, CancellationToken ct) => Task.FromResult(_periods.FirstOrDefault(x => x.Id == id && x.TenantId == tenantId));
    public Task<bool> YearExistsAsync(int year, string tenantId, Guid? excludingId, CancellationToken ct) => Task.FromResult(_years.Any(x => x.Year == year && x.TenantId == tenantId && (!excludingId.HasValue || x.Id != excludingId)));
    public Task<bool> PeriodNumberExistsAsync(Guid fiscalYearId, string tenantId, int periodNumber, Guid? excludingId, CancellationToken ct) => Task.FromResult(_periods.Any(x => x.FiscalYearId == fiscalYearId && x.TenantId == tenantId && x.PeriodNumber == periodNumber && (!excludingId.HasValue || x.Id != excludingId)));
    public Task<bool> PeriodOverlapsAsync(Guid fiscalYearId, string tenantId, DateOnly startDate, DateOnly endDate, Guid? excludingId, CancellationToken ct) => Task.FromResult(_periods.Any(x => x.FiscalYearId == fiscalYearId && x.TenantId == tenantId && x.Status != FiscalPeriodStatus.Archived && (!excludingId.HasValue || x.Id != excludingId) && x.Overlaps(startDate, endDate)));
    public Task<bool> HasOpenPeriodsAsync(Guid fiscalYearId, string tenantId, CancellationToken ct) => Task.FromResult(_periods.Any(x => x.FiscalYearId == fiscalYearId && x.TenantId == tenantId && x.Status == FiscalPeriodStatus.Open));
    public Task<(IReadOnlyCollection<FiscalYear> Items, long Total)> SearchYearsAsync(string tenantId, int? year, FiscalYearStatus? status, int page, int pageSize, CancellationToken ct)
    {
        var query = _years.Where(x => x.TenantId == tenantId);
        if (year.HasValue) query = query.Where(x => x.Year == year);
        if (status.HasValue) query = query.Where(x => x.Status == status);
        var list = query.OrderByDescending(x => x.Year).ToArray();
        return Task.FromResult(((IReadOnlyCollection<FiscalYear>)list.Skip((page - 1) * pageSize).Take(pageSize).ToArray(), (long)list.Length));
    }
    public Task<(IReadOnlyCollection<FiscalPeriod> Items, long Total)> SearchPeriodsAsync(string tenantId, Guid? fiscalYearId, FiscalPeriodStatus? status, int page, int pageSize, CancellationToken ct)
    {
        var query = _periods.Where(x => x.TenantId == tenantId);
        if (fiscalYearId.HasValue) query = query.Where(x => x.FiscalYearId == fiscalYearId);
        if (status.HasValue) query = query.Where(x => x.Status == status);
        var list = query.OrderBy(x => x.StartDate).ToArray();
        return Task.FromResult(((IReadOnlyCollection<FiscalPeriod>)list.Skip((page - 1) * pageSize).Take(pageSize).ToArray(), (long)list.Length));
    }
    public Task<FiscalPeriod?> GetOpenPeriodByDateAsync(string tenantId, DateOnly date, CancellationToken ct) => Task.FromResult(_periods.FirstOrDefault(x => x.TenantId == tenantId && x.Status == FiscalPeriodStatus.Open && x.StartDate <= date && date <= x.EndDate));
    public Task SaveChangesAsync(CancellationToken ct) => Task.CompletedTask;
}
