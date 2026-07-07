using System.Text.Json;
using Financiero.Contracts;
using Financiero.Domain;

namespace Financiero.Application;

public interface IFiscalRepository
{
    Task AddYearAsync(FiscalYear year, CancellationToken ct);
    Task AddPeriodAsync(FiscalPeriod period, CancellationToken ct);
    Task<FiscalYear?> GetYearByIdAsync(Guid id, string tenantId, CancellationToken ct);
    Task<FiscalPeriod?> GetPeriodByIdAsync(Guid id, string tenantId, CancellationToken ct);
    Task<bool> YearExistsAsync(int year, string tenantId, Guid? excludingId, CancellationToken ct);
    Task<bool> PeriodNumberExistsAsync(Guid fiscalYearId, string tenantId, int periodNumber, Guid? excludingId, CancellationToken ct);
    Task<bool> PeriodOverlapsAsync(Guid fiscalYearId, string tenantId, DateOnly startDate, DateOnly endDate, Guid? excludingId, CancellationToken ct);
    Task<bool> HasOpenPeriodsAsync(Guid fiscalYearId, string tenantId, CancellationToken ct);
    Task<(IReadOnlyCollection<FiscalYear> Items, long Total)> SearchYearsAsync(string tenantId, int? year, FiscalYearStatus? status, int page, int pageSize, CancellationToken ct);
    Task<(IReadOnlyCollection<FiscalPeriod> Items, long Total)> SearchPeriodsAsync(string tenantId, Guid? fiscalYearId, FiscalPeriodStatus? status, int page, int pageSize, CancellationToken ct);
    Task<FiscalPeriod?> GetOpenPeriodByDateAsync(string tenantId, DateOnly date, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}

public sealed class FiscalPeriodsService(IFiscalRepository fiscal, IPortalAuditClient audit, IPortalOutboxClient outbox)
{
    public async Task<FiscalYearDto> CreateYearAsync(CreateFiscalYearRequest request, PortalCallContext context, CancellationToken ct)
    {
        if (await fiscal.YearExistsAsync(request.Year, context.TenantId, null, ct)) throw new FinancialApplicationException("fiscal_year.duplicate", "Fiscal year already exists for tenant.");
        var year = FiscalYear.Create(context.TenantId, request.Year, request.StartDate, request.EndDate, DateTimeOffset.UtcNow);
        await fiscal.AddYearAsync(year, ct);
        await fiscal.SaveChangesAsync(ct);
        await AuditOutboxAsync("FiscalYearCreated", "FiscalYearCreated", year.Id, year, context, ct);
        return ToDto(year);
    }

    public async Task<FiscalYearDto> UpdateYearAsync(Guid id, UpdateFiscalYearRequest request, PortalCallContext context, CancellationToken ct)
    {
        var year = await GetYearAsync(id, context.TenantId, ct);
        if (await fiscal.YearExistsAsync(request.Year, context.TenantId, id, ct)) throw new FinancialApplicationException("fiscal_year.duplicate", "Fiscal year already exists for tenant.");
        year.Update(request.Year, request.StartDate, request.EndDate, DateTimeOffset.UtcNow);
        await fiscal.SaveChangesAsync(ct);
        await AuditOutboxAsync("FiscalYearUpdated", "FiscalYearUpdated", year.Id, year, context, ct);
        return ToDto(year);
    }

    public async Task<FiscalYearDto> OpenYearAsync(Guid id, PortalCallContext context, CancellationToken ct)
    {
        var year = await GetYearAsync(id, context.TenantId, ct);
        year.Open(DateTimeOffset.UtcNow);
        await fiscal.SaveChangesAsync(ct);
        await AuditOutboxAsync("FiscalYearOpened", "FiscalYearStatusChanged", year.Id, year, context, ct);
        return ToDto(year);
    }

    public async Task<FiscalYearDto> CloseYearAsync(Guid id, PortalCallContext context, CancellationToken ct)
    {
        var year = await GetYearAsync(id, context.TenantId, ct);
        if (await fiscal.HasOpenPeriodsAsync(id, context.TenantId, ct)) throw new FinancialApplicationException("fiscal_year.close.open_periods", "Fiscal year with open periods cannot be closed.");
        year.Close(DateTimeOffset.UtcNow);
        await fiscal.SaveChangesAsync(ct);
        await AuditOutboxAsync("FiscalYearClosed", "FiscalYearStatusChanged", year.Id, year, context, ct);
        return ToDto(year);
    }

    public async Task<FiscalYearDto> ArchiveYearAsync(Guid id, PortalCallContext context, CancellationToken ct)
    {
        var year = await GetYearAsync(id, context.TenantId, ct);
        year.Archive(DateTimeOffset.UtcNow);
        await fiscal.SaveChangesAsync(ct);
        await AuditOutboxAsync("FiscalYearArchived", "FiscalYearStatusChanged", year.Id, year, context, ct);
        return ToDto(year);
    }

    public async Task<FiscalPeriodDto> CreatePeriodAsync(CreateFiscalPeriodRequest request, PortalCallContext context, CancellationToken ct)
    {
        var year = await GetYearAsync(request.FiscalYearId, context.TenantId, ct);
        if (await fiscal.PeriodNumberExistsAsync(year.Id, context.TenantId, request.PeriodNumber, null, ct)) throw new FinancialApplicationException("fiscal_period.duplicate", "Fiscal period number already exists for fiscal year.");
        if (await fiscal.PeriodOverlapsAsync(year.Id, context.TenantId, request.StartDate, request.EndDate, null, ct)) throw new FinancialApplicationException("fiscal_period.overlap", "Fiscal period overlaps an existing period.");
        var period = FiscalPeriod.Create(context.TenantId, year.Id, request.PeriodNumber, request.StartDate, request.EndDate, DateTimeOffset.UtcNow);
        await fiscal.AddPeriodAsync(period, ct);
        await fiscal.SaveChangesAsync(ct);
        await AuditOutboxAsync("FiscalPeriodCreated", "FiscalPeriodCreated", period.Id, period, context, ct);
        return ToDto(period);
    }

    public async Task<FiscalPeriodDto> UpdatePeriodAsync(Guid id, UpdateFiscalPeriodRequest request, PortalCallContext context, CancellationToken ct)
    {
        var period = await GetPeriodAsync(id, context.TenantId, ct);
        if (await fiscal.PeriodNumberExistsAsync(period.FiscalYearId, context.TenantId, request.PeriodNumber, id, ct)) throw new FinancialApplicationException("fiscal_period.duplicate", "Fiscal period number already exists for fiscal year.");
        if (await fiscal.PeriodOverlapsAsync(period.FiscalYearId, context.TenantId, request.StartDate, request.EndDate, id, ct)) throw new FinancialApplicationException("fiscal_period.overlap", "Fiscal period overlaps an existing period.");
        period.Update(request.PeriodNumber, request.StartDate, request.EndDate, DateTimeOffset.UtcNow);
        await fiscal.SaveChangesAsync(ct);
        await AuditOutboxAsync("FiscalPeriodUpdated", "FiscalPeriodUpdated", period.Id, period, context, ct);
        return ToDto(period);
    }

    public async Task<FiscalPeriodDto> OpenPeriodAsync(Guid id, PortalCallContext context, CancellationToken ct)
    {
        var period = await GetPeriodAsync(id, context.TenantId, ct);
        var year = await GetYearAsync(period.FiscalYearId, context.TenantId, ct);
        if (year.Status != FiscalYearStatus.Open) throw new FinancialApplicationException("fiscal_period.open.year_not_open", "Fiscal year must be open to open a period.");
        period.Open(DateTimeOffset.UtcNow);
        await fiscal.SaveChangesAsync(ct);
        await AuditOutboxAsync("FiscalPeriodOpened", "FiscalPeriodStatusChanged", period.Id, period, context, ct);
        return ToDto(period);
    }

    public async Task<FiscalPeriodDto> ClosePeriodAsync(Guid id, PortalCallContext context, CancellationToken ct) => await ChangePeriodStatusAsync(id, context, "FiscalPeriodClosed", p => p.Close(DateTimeOffset.UtcNow), ct);
    public async Task<FiscalPeriodDto> LockPeriodAsync(Guid id, PortalCallContext context, CancellationToken ct) => await ChangePeriodStatusAsync(id, context, "FiscalPeriodLocked", p => p.Lock(DateTimeOffset.UtcNow), ct);

    public async Task<FiscalPeriodDto> ReopenPeriodAsync(Guid id, PortalCallContext context, CancellationToken ct)
    {
        var period = await GetPeriodAsync(id, context.TenantId, ct);
        var year = await GetYearAsync(period.FiscalYearId, context.TenantId, ct);
        if (year.Status == FiscalYearStatus.Closed) throw new FinancialApplicationException("fiscal_period.reopen.year_closed", "Closed fiscal years do not allow period reopening.");
        period.Reopen(DateTimeOffset.UtcNow);
        await fiscal.SaveChangesAsync(ct);
        await AuditOutboxAsync("FiscalPeriodReopened", "FiscalPeriodStatusChanged", period.Id, period, context, ct);
        return ToDto(period);
    }

    public async Task<FiscalPeriodDto> ArchivePeriodAsync(Guid id, PortalCallContext context, CancellationToken ct)
    {
        var period = await GetPeriodAsync(id, context.TenantId, ct);
        period.Archive(DateTimeOffset.UtcNow);
        await fiscal.SaveChangesAsync(ct);
        await AuditOutboxAsync("FiscalPeriodArchived", "FiscalPeriodStatusChanged", period.Id, period, context, ct);
        return ToDto(period);
    }

    public async Task<FiscalYearDto> GetYearByIdAsync(Guid id, PortalCallContext context, CancellationToken ct) => ToDto(await GetYearAsync(id, context.TenantId, ct));
    public async Task<FiscalPeriodDto> GetPeriodByIdAsync(Guid id, PortalCallContext context, CancellationToken ct) => ToDto(await GetPeriodAsync(id, context.TenantId, ct));

    public async Task<PageResponse<FiscalYearDto>> SearchYearsAsync(SearchFiscalYearsRequest request, PortalCallContext context, CancellationToken ct)
    {
        FiscalYearStatus? status = string.IsNullOrWhiteSpace(request.Status) ? null : ParseYearStatus(request.Status);
        var page = Math.Max(1, request.Page);
        var size = Math.Clamp(request.PageSize, 1, 100);
        var (items, total) = await fiscal.SearchYearsAsync(context.TenantId, request.Year, status, page, size, ct);
        return new(items.Select(ToDto).ToArray(), page, size, total);
    }

    public async Task<PageResponse<FiscalPeriodDto>> SearchPeriodsAsync(SearchFiscalPeriodsRequest request, PortalCallContext context, CancellationToken ct)
    {
        FiscalPeriodStatus? status = string.IsNullOrWhiteSpace(request.Status) ? null : ParsePeriodStatus(request.Status);
        var page = Math.Max(1, request.Page);
        var size = Math.Clamp(request.PageSize, 1, 100);
        var (items, total) = await fiscal.SearchPeriodsAsync(context.TenantId, request.FiscalYearId, status, page, size, ct);
        return new(items.Select(ToDto).ToArray(), page, size, total);
    }

    public async Task<FiscalPeriodDto> GetOpenPeriodByDateAsync(DateOnly date, PortalCallContext context, CancellationToken ct) =>
        ToDto(await fiscal.GetOpenPeriodByDateAsync(context.TenantId, date, ct) ?? throw new FinancialApplicationException("fiscal_period.open.not_found", "Open fiscal period was not found for date."));

    private async Task<FiscalPeriodDto> ChangePeriodStatusAsync(Guid id, PortalCallContext context, string auditAction, Action<FiscalPeriod> change, CancellationToken ct)
    {
        var period = await GetPeriodAsync(id, context.TenantId, ct);
        change(period);
        await fiscal.SaveChangesAsync(ct);
        await AuditOutboxAsync(auditAction, "FiscalPeriodStatusChanged", period.Id, period, context, ct);
        return ToDto(period);
    }

    private async Task<FiscalYear> GetYearAsync(Guid id, string tenantId, CancellationToken ct) =>
        await fiscal.GetYearByIdAsync(id, tenantId, ct) ?? throw new FinancialApplicationException("fiscal_year.not_found", "Fiscal year was not found.");
    private async Task<FiscalPeriod> GetPeriodAsync(Guid id, string tenantId, CancellationToken ct) =>
        await fiscal.GetPeriodByIdAsync(id, tenantId, ct) ?? throw new FinancialApplicationException("fiscal_period.not_found", "Fiscal period was not found.");
    private async Task AuditOutboxAsync(string auditAction, string outboxType, Guid id, object entity, PortalCallContext context, CancellationToken ct)
    {
        var payload = JsonSerializer.Serialize(entity);
        await audit.RecordAsync(new(auditAction, "financial.fiscal", id.ToString(), entity), context, ct);
        await outbox.EnqueueAsync(new(Guid.NewGuid(), outboxType, 1, DateTimeOffset.UtcNow, context.CorrelationId, payload), context, ct);
    }
    public static FiscalYearDto ToDto(FiscalYear year) => new(year.Id, year.TenantId, year.Year, year.StartDate, year.EndDate, year.Status.ToString(), year.CreatedAtUtc, year.UpdatedAtUtc);
    public static FiscalPeriodDto ToDto(FiscalPeriod period) => new(period.Id, period.TenantId, period.FiscalYearId, period.PeriodNumber, period.StartDate, period.EndDate, period.Status.ToString(), period.CreatedAtUtc, period.UpdatedAtUtc);
    private static FiscalYearStatus ParseYearStatus(string value) => Enum.TryParse<FiscalYearStatus>(value, true, out var status) ? status : throw new FinancialApplicationException("fiscal_year.status.invalid", "Fiscal year status is invalid.");
    private static FiscalPeriodStatus ParsePeriodStatus(string value) => Enum.TryParse<FiscalPeriodStatus>(value, true, out var status) ? status : throw new FinancialApplicationException("fiscal_period.status.invalid", "Fiscal period status is invalid.");
}

public static class FiscalPeriodsPortalMetadata
{
    public static readonly string[] Permissions =
    [
        "financial.fiscalyears.read", "financial.fiscalyears.create", "financial.fiscalyears.update", "financial.fiscalyears.open",
        "financial.fiscalyears.close", "financial.fiscalyears.archive", "financial.fiscalyears.manage",
        "financial.fiscalperiods.read", "financial.fiscalperiods.create", "financial.fiscalperiods.update", "financial.fiscalperiods.open",
        "financial.fiscalperiods.close", "financial.fiscalperiods.lock", "financial.fiscalperiods.reopen", "financial.fiscalperiods.archive", "financial.fiscalperiods.manage"
    ];
    public static readonly string[] ConfigurationKeys =
    [
        "financial.fiscalYear.startMonth", "financial.fiscalYear.defaultPeriodType", "financial.fiscalPeriods.defaultCount",
        "financial.fiscalPeriods.allowReopen", "financial.fiscalPeriods.allowLock", "financial.fiscalPeriods.closeRequiresNoDraftEntries"
    ];
}
