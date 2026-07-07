namespace Financiero.Contracts;

public sealed record FiscalYearDto(Guid Id, string TenantId, int Year, DateOnly StartDate, DateOnly EndDate, string Status, DateTimeOffset CreatedAtUtc, DateTimeOffset UpdatedAtUtc);
public sealed record FiscalPeriodDto(Guid Id, string TenantId, Guid FiscalYearId, int PeriodNumber, DateOnly StartDate, DateOnly EndDate, string Status, DateTimeOffset CreatedAtUtc, DateTimeOffset UpdatedAtUtc);

public sealed record CreateFiscalYearRequest(int Year, DateOnly StartDate, DateOnly EndDate);
public sealed record UpdateFiscalYearRequest(int Year, DateOnly StartDate, DateOnly EndDate);
public sealed record SearchFiscalYearsRequest(int? Year = null, string? Status = null, int Page = 1, int PageSize = 20);

public sealed record CreateFiscalPeriodRequest(Guid FiscalYearId, int PeriodNumber, DateOnly StartDate, DateOnly EndDate);
public sealed record UpdateFiscalPeriodRequest(int PeriodNumber, DateOnly StartDate, DateOnly EndDate);
public sealed record SearchFiscalPeriodsRequest(Guid? FiscalYearId = null, string? Status = null, int Page = 1, int PageSize = 20);
