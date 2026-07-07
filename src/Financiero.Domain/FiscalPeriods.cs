namespace Financiero.Domain;

public enum FiscalYearStatus { Draft, Open, Closed, Archived }
public enum FiscalPeriodStatus { Draft, Open, Closed, Locked, Archived }

public sealed class FiscalYear
{
    private FiscalYear() { }
    private FiscalYear(Guid id, string tenantId, int year, DateOnly startDate, DateOnly endDate, DateTimeOffset now)
    {
        if (year < 1) throw new FinancialDomainException("fiscal_year.year.invalid", "Fiscal year must be valid.");
        ValidateDates(startDate, endDate);
        Id = id;
        TenantId = Required(tenantId, nameof(TenantId));
        Year = year;
        StartDate = startDate;
        EndDate = endDate;
        Status = FiscalYearStatus.Draft;
        CreatedAtUtc = now;
        UpdatedAtUtc = now;
    }

    public Guid Id { get; private set; }
    public string TenantId { get; private set; } = FinancialTenant.Default;
    public int Year { get; private set; }
    public DateOnly StartDate { get; private set; }
    public DateOnly EndDate { get; private set; }
    public FiscalYearStatus Status { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public static FiscalYear Create(string tenantId, int year, DateOnly startDate, DateOnly endDate, DateTimeOffset now) =>
        new(Guid.NewGuid(), tenantId, year, startDate, endDate, now);

    public void Update(int year, DateOnly startDate, DateOnly endDate, DateTimeOffset now)
    {
        EnsureNotArchived();
        if (year < 1) throw new FinancialDomainException("fiscal_year.year.invalid", "Fiscal year must be valid.");
        ValidateDates(startDate, endDate);
        Year = year;
        StartDate = startDate;
        EndDate = endDate;
        UpdatedAtUtc = now;
    }

    public void Open(DateTimeOffset now) { EnsureNotArchived(); Status = FiscalYearStatus.Open; UpdatedAtUtc = now; }
    public void Close(DateTimeOffset now) { EnsureNotArchived(); Status = FiscalYearStatus.Closed; UpdatedAtUtc = now; }
    public void Archive(DateTimeOffset now)
    {
        if (Status == FiscalYearStatus.Open) throw new FinancialDomainException("fiscal_year.archive.open", "Open fiscal years cannot be archived.");
        Status = FiscalYearStatus.Archived;
        UpdatedAtUtc = now;
    }
    private void EnsureNotArchived()
    {
        if (Status == FiscalYearStatus.Archived) throw new FinancialDomainException("fiscal_year.archived", "Archived fiscal years cannot be modified.");
    }
    internal static void ValidateDates(DateOnly startDate, DateOnly endDate)
    {
        if (startDate > endDate) throw new FinancialDomainException("date_range.invalid", "StartDate must be before or equal to EndDate.");
    }
    internal static string Required(string value, string name) => string.IsNullOrWhiteSpace(value) ? throw new FinancialDomainException($"{name.ToLowerInvariant()}.required", $"{name} is required.") : value.Trim();
}

public sealed class FiscalPeriod
{
    private FiscalPeriod() { }
    private FiscalPeriod(Guid id, string tenantId, Guid fiscalYearId, int periodNumber, DateOnly startDate, DateOnly endDate, DateTimeOffset now)
    {
        if (periodNumber < 1) throw new FinancialDomainException("fiscal_period.number.invalid", "Period number must be greater than zero.");
        FiscalYear.ValidateDates(startDate, endDate);
        Id = id;
        TenantId = FiscalYear.Required(tenantId, nameof(TenantId));
        FiscalYearId = fiscalYearId;
        PeriodNumber = periodNumber;
        StartDate = startDate;
        EndDate = endDate;
        Status = FiscalPeriodStatus.Draft;
        CreatedAtUtc = now;
        UpdatedAtUtc = now;
    }

    public Guid Id { get; private set; }
    public string TenantId { get; private set; } = FinancialTenant.Default;
    public Guid FiscalYearId { get; private set; }
    public int PeriodNumber { get; private set; }
    public DateOnly StartDate { get; private set; }
    public DateOnly EndDate { get; private set; }
    public FiscalPeriodStatus Status { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public static FiscalPeriod Create(string tenantId, Guid fiscalYearId, int periodNumber, DateOnly startDate, DateOnly endDate, DateTimeOffset now) =>
        new(Guid.NewGuid(), tenantId, fiscalYearId, periodNumber, startDate, endDate, now);

    public void Update(int periodNumber, DateOnly startDate, DateOnly endDate, DateTimeOffset now)
    {
        EnsureNotArchived();
        if (Status == FiscalPeriodStatus.Locked) throw new FinancialDomainException("fiscal_period.locked", "Locked fiscal periods cannot be modified.");
        if (periodNumber < 1) throw new FinancialDomainException("fiscal_period.number.invalid", "Period number must be greater than zero.");
        FiscalYear.ValidateDates(startDate, endDate);
        PeriodNumber = periodNumber;
        StartDate = startDate;
        EndDate = endDate;
        UpdatedAtUtc = now;
    }

    public void Open(DateTimeOffset now) { EnsureNotArchived(); Status = FiscalPeriodStatus.Open; UpdatedAtUtc = now; }
    public void Close(DateTimeOffset now) { EnsureNotArchived(); Status = FiscalPeriodStatus.Closed; UpdatedAtUtc = now; }
    public void Lock(DateTimeOffset now) { EnsureNotArchived(); Status = FiscalPeriodStatus.Locked; UpdatedAtUtc = now; }
    public void Reopen(DateTimeOffset now)
    {
        EnsureNotArchived();
        if (Status == FiscalPeriodStatus.Locked) throw new FinancialDomainException("fiscal_period.reopen.locked", "Locked fiscal periods cannot be reopened.");
        Status = FiscalPeriodStatus.Open;
        UpdatedAtUtc = now;
    }
    public void Archive(DateTimeOffset now)
    {
        if (Status == FiscalPeriodStatus.Open) throw new FinancialDomainException("fiscal_period.archive.open", "Open fiscal periods cannot be archived.");
        Status = FiscalPeriodStatus.Archived;
        UpdatedAtUtc = now;
    }
    public bool Overlaps(DateOnly start, DateOnly end) => StartDate <= end && start <= EndDate;
    private void EnsureNotArchived()
    {
        if (Status == FiscalPeriodStatus.Archived) throw new FinancialDomainException("fiscal_period.archived", "Archived fiscal periods cannot be modified.");
    }
}
