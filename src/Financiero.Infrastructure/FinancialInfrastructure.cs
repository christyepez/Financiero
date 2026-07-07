using Financiero.Application;
using Financiero.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Hosting;

namespace Financiero.Infrastructure;

public sealed class FinancialDbContext(DbContextOptions<FinancialDbContext> options) : DbContext(options)
{
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<FiscalYear> FiscalYears => Set<FiscalYear>();
    public DbSet<FiscalPeriod> FiscalPeriods => Set<FiscalPeriod>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("financial");
        modelBuilder.Entity<Account>(entity =>
        {
            entity.ToTable("accounts");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.TenantId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Code).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Type).HasConversion<string>().HasMaxLength(32).IsRequired();
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
            entity.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.ParentAccountId });
        });
        modelBuilder.Entity<FiscalYear>(entity =>
        {
            entity.ToTable("fiscal_years");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.TenantId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
            entity.HasIndex(x => new { x.TenantId, x.Year }).IsUnique();
        });
        modelBuilder.Entity<FiscalPeriod>(entity =>
        {
            entity.ToTable("fiscal_periods");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.TenantId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
            entity.HasIndex(x => new { x.TenantId, x.FiscalYearId, x.PeriodNumber }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.FiscalYearId, x.StartDate, x.EndDate });
        });
    }
}

public sealed class EfAccountRepository(FinancialDbContext db) : IAccountRepository
{
    public async Task AddAsync(Account account, CancellationToken ct) => await db.Accounts.AddAsync(account, ct);
    public async Task<Account?> GetByIdAsync(Guid id, string tenantId, CancellationToken ct) =>
        await db.Accounts.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId, ct);
    public async Task<Account?> GetByCodeAsync(string code, string tenantId, CancellationToken ct) =>
        await db.Accounts.FirstOrDefaultAsync(x => x.Code == code && x.TenantId == tenantId, ct);
    public async Task<bool> ExistsByCodeAsync(string code, string tenantId, Guid? excludingId, CancellationToken ct) =>
        await db.Accounts.AnyAsync(x => x.Code == code && x.TenantId == tenantId && (!excludingId.HasValue || x.Id != excludingId), ct);
    public async Task<bool> HasChildrenAsync(Guid id, string tenantId, CancellationToken ct) =>
        await db.Accounts.AnyAsync(x => x.ParentAccountId == id && x.TenantId == tenantId && x.Status == AccountStatus.Active, ct);
    public async Task<(IReadOnlyCollection<Account> Items, long Total)> SearchAsync(string tenantId, string? search, AccountType? type, AccountStatus? status, int page, int pageSize, CancellationToken ct)
    {
        var query = db.Accounts.AsNoTracking().Where(x => x.TenantId == tenantId);
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(x => x.Code.Contains(search) || x.Name.Contains(search));
        if (type.HasValue) query = query.Where(x => x.Type == type);
        if (status.HasValue) query = query.Where(x => x.Status == status);
        var total = await query.LongCountAsync(ct);
        var items = await query.OrderBy(x => x.Code).Skip((page - 1) * pageSize).Take(pageSize).ToArrayAsync(ct);
        return (items, total);
    }
    public async Task<IReadOnlyCollection<Account>> GetTreeAccountsAsync(string tenantId, CancellationToken ct) =>
        await db.Accounts.AsNoTracking().Where(x => x.TenantId == tenantId && x.Status != AccountStatus.Archived).OrderBy(x => x.Code).ToArrayAsync(ct);
    public async Task SaveChangesAsync(CancellationToken ct) => await db.SaveChangesAsync(ct);
}

public sealed class EfFiscalRepository(FinancialDbContext db) : IFiscalRepository
{
    public async Task AddYearAsync(FiscalYear year, CancellationToken ct) => await db.FiscalYears.AddAsync(year, ct);
    public async Task AddPeriodAsync(FiscalPeriod period, CancellationToken ct) => await db.FiscalPeriods.AddAsync(period, ct);
    public async Task<FiscalYear?> GetYearByIdAsync(Guid id, string tenantId, CancellationToken ct) => await db.FiscalYears.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId, ct);
    public async Task<FiscalPeriod?> GetPeriodByIdAsync(Guid id, string tenantId, CancellationToken ct) => await db.FiscalPeriods.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId, ct);
    public async Task<bool> YearExistsAsync(int year, string tenantId, Guid? excludingId, CancellationToken ct) =>
        await db.FiscalYears.AnyAsync(x => x.Year == year && x.TenantId == tenantId && (!excludingId.HasValue || x.Id != excludingId), ct);
    public async Task<bool> PeriodNumberExistsAsync(Guid fiscalYearId, string tenantId, int periodNumber, Guid? excludingId, CancellationToken ct) =>
        await db.FiscalPeriods.AnyAsync(x => x.FiscalYearId == fiscalYearId && x.TenantId == tenantId && x.PeriodNumber == periodNumber && (!excludingId.HasValue || x.Id != excludingId), ct);
    public async Task<bool> PeriodOverlapsAsync(Guid fiscalYearId, string tenantId, DateOnly startDate, DateOnly endDate, Guid? excludingId, CancellationToken ct) =>
        await db.FiscalPeriods.AnyAsync(x => x.FiscalYearId == fiscalYearId && x.TenantId == tenantId && x.Status != FiscalPeriodStatus.Archived && (!excludingId.HasValue || x.Id != excludingId) && x.StartDate <= endDate && startDate <= x.EndDate, ct);
    public async Task<bool> HasOpenPeriodsAsync(Guid fiscalYearId, string tenantId, CancellationToken ct) =>
        await db.FiscalPeriods.AnyAsync(x => x.FiscalYearId == fiscalYearId && x.TenantId == tenantId && x.Status == FiscalPeriodStatus.Open, ct);
    public async Task<(IReadOnlyCollection<FiscalYear> Items, long Total)> SearchYearsAsync(string tenantId, int? year, FiscalYearStatus? status, int page, int pageSize, CancellationToken ct)
    {
        var query = db.FiscalYears.AsNoTracking().Where(x => x.TenantId == tenantId);
        if (year.HasValue) query = query.Where(x => x.Year == year);
        if (status.HasValue) query = query.Where(x => x.Status == status);
        var total = await query.LongCountAsync(ct);
        return (await query.OrderByDescending(x => x.Year).Skip((page - 1) * pageSize).Take(pageSize).ToArrayAsync(ct), total);
    }
    public async Task<(IReadOnlyCollection<FiscalPeriod> Items, long Total)> SearchPeriodsAsync(string tenantId, Guid? fiscalYearId, FiscalPeriodStatus? status, int page, int pageSize, CancellationToken ct)
    {
        var query = db.FiscalPeriods.AsNoTracking().Where(x => x.TenantId == tenantId);
        if (fiscalYearId.HasValue) query = query.Where(x => x.FiscalYearId == fiscalYearId);
        if (status.HasValue) query = query.Where(x => x.Status == status);
        var total = await query.LongCountAsync(ct);
        return (await query.OrderBy(x => x.StartDate).ThenBy(x => x.PeriodNumber).Skip((page - 1) * pageSize).Take(pageSize).ToArrayAsync(ct), total);
    }
    public async Task<FiscalPeriod?> GetOpenPeriodByDateAsync(string tenantId, DateOnly date, CancellationToken ct) =>
        await db.FiscalPeriods.AsNoTracking().FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Status == FiscalPeriodStatus.Open && x.StartDate <= date && date <= x.EndDate, ct);
    public async Task SaveChangesAsync(CancellationToken ct) => await db.SaveChangesAsync(ct);
}

public sealed class FinancialSqlHealthCheck(FinancialDbContext db) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken ct = default) =>
        await db.Database.CanConnectAsync(ct) ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy("Financial SQL is unavailable.");
}

public sealed class DevelopmentPortalAdapters(IOptions<PortalOptions> options, ILogger<DevelopmentPortalAdapters> logger) :
    IPortalAuditClient, IPortalNotificationClient, IPortalOutboxClient, IPortalConfigurationClient,
    IPortalSecurityClient, IPortalMenuRegistrationClient
{
    public string? LastCorrelationId { get; private set; }
    private void Trace(string capability, PortalCallContext context)
    {
        LastCorrelationId = context.CorrelationId;
        logger.LogInformation("Portal adapter {Capability} is in development mode; correlation {CorrelationId}; gateway {Gateway}", capability, context.CorrelationId, options.Value.GatewayBaseUrl);
    }
    public Task RecordAsync(AuditRecord record, PortalCallContext context, CancellationToken ct) { Trace("Audit", context); return Task.CompletedTask; }
    public Task RequestAsync(NotificationRequest request, PortalCallContext context, CancellationToken ct) { Trace("Notification", context); return Task.CompletedTask; }
    public Task EnqueueAsync(OutboxEnvelope message, PortalCallContext context, CancellationToken ct) { Trace("Outbox", context); return Task.CompletedTask; }
    public Task<string?> GetAsync(string key, PortalCallContext context, CancellationToken ct) { Trace("Configuration", context); return Task.FromResult<string?>(null); }
    public Task<bool> HasPermissionAsync(string permission, PortalCallContext context, CancellationToken ct) { Trace("Security", context); return Task.FromResult(false); }
    public Task RegisterModuleAsync(string moduleCode, PortalCallContext context, CancellationToken ct) { Trace("Menu", context); return Task.CompletedTask; }
}

public static class FinancialInfrastructureExtensions
{
    public static IServiceCollection AddFinancialInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<PortalOptions>(configuration.GetSection(PortalOptions.SectionName));
        services.Configure<FinancialPlatformOptions>(configuration.GetSection(FinancialPlatformOptions.SectionName));
        services.AddSingleton<DevelopmentPortalAdapters>();
        services.AddSingleton<IPortalAuditClient>(x => x.GetRequiredService<DevelopmentPortalAdapters>());
        services.AddSingleton<IPortalNotificationClient>(x => x.GetRequiredService<DevelopmentPortalAdapters>());
        services.AddSingleton<IPortalOutboxClient>(x => x.GetRequiredService<DevelopmentPortalAdapters>());
        services.AddSingleton<IPortalConfigurationClient>(x => x.GetRequiredService<DevelopmentPortalAdapters>());
        services.AddSingleton<IPortalSecurityClient>(x => x.GetRequiredService<DevelopmentPortalAdapters>());
        services.AddSingleton<IPortalMenuRegistrationClient>(x => x.GetRequiredService<DevelopmentPortalAdapters>());
        services.AddScoped<ChartOfAccountsService>();
        services.AddScoped<FiscalPeriodsService>();

        var connectionString = configuration.GetConnectionString("FinancialDb");
        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            services.AddDbContext<FinancialDbContext>(x => x.UseSqlServer(connectionString));
            services.AddScoped<IAccountRepository, EfAccountRepository>();
            services.AddScoped<IFiscalRepository, EfFiscalRepository>();
            services.AddHealthChecks().AddCheck<FinancialSqlHealthCheck>("financial-sql", tags: ["ready"]);
            if (configuration.GetValue<bool>("Database:Initialize")) services.AddHostedService<FinancialDatabaseInitializer>();
        }
        else
        {
            services.AddScoped<IAccountRepository, UnconfiguredAccountRepository>();
            services.AddScoped<IFiscalRepository, UnconfiguredFiscalRepository>();
        }
        return services;
    }
}

internal sealed class UnconfiguredAccountRepository : IAccountRepository
{
    private static InvalidOperationException Error => new("FinancialDb connection string is not configured.");
    public Task AddAsync(Account account, CancellationToken ct) => Task.FromException(Error);
    public Task<Account?> GetByIdAsync(Guid id, string tenantId, CancellationToken ct) => Task.FromException<Account?>(Error);
    public Task<Account?> GetByCodeAsync(string code, string tenantId, CancellationToken ct) => Task.FromException<Account?>(Error);
    public Task<bool> ExistsByCodeAsync(string code, string tenantId, Guid? excludingId, CancellationToken ct) => Task.FromException<bool>(Error);
    public Task<bool> HasChildrenAsync(Guid id, string tenantId, CancellationToken ct) => Task.FromException<bool>(Error);
    public Task<(IReadOnlyCollection<Account> Items, long Total)> SearchAsync(string tenantId, string? search, AccountType? type, AccountStatus? status, int page, int pageSize, CancellationToken ct) =>
        Task.FromException<(IReadOnlyCollection<Account>, long)>(Error);
    public Task<IReadOnlyCollection<Account>> GetTreeAccountsAsync(string tenantId, CancellationToken ct) => Task.FromException<IReadOnlyCollection<Account>>(Error);
    public Task SaveChangesAsync(CancellationToken ct) => Task.FromException(Error);
}

internal sealed class UnconfiguredFiscalRepository : IFiscalRepository
{
    private static InvalidOperationException Error => new("FinancialDb connection string is not configured.");
    public Task AddYearAsync(FiscalYear year, CancellationToken ct) => Task.FromException(Error);
    public Task AddPeriodAsync(FiscalPeriod period, CancellationToken ct) => Task.FromException(Error);
    public Task<FiscalYear?> GetYearByIdAsync(Guid id, string tenantId, CancellationToken ct) => Task.FromException<FiscalYear?>(Error);
    public Task<FiscalPeriod?> GetPeriodByIdAsync(Guid id, string tenantId, CancellationToken ct) => Task.FromException<FiscalPeriod?>(Error);
    public Task<bool> YearExistsAsync(int year, string tenantId, Guid? excludingId, CancellationToken ct) => Task.FromException<bool>(Error);
    public Task<bool> PeriodNumberExistsAsync(Guid fiscalYearId, string tenantId, int periodNumber, Guid? excludingId, CancellationToken ct) => Task.FromException<bool>(Error);
    public Task<bool> PeriodOverlapsAsync(Guid fiscalYearId, string tenantId, DateOnly startDate, DateOnly endDate, Guid? excludingId, CancellationToken ct) => Task.FromException<bool>(Error);
    public Task<bool> HasOpenPeriodsAsync(Guid fiscalYearId, string tenantId, CancellationToken ct) => Task.FromException<bool>(Error);
    public Task<(IReadOnlyCollection<FiscalYear> Items, long Total)> SearchYearsAsync(string tenantId, int? year, FiscalYearStatus? status, int page, int pageSize, CancellationToken ct) => Task.FromException<(IReadOnlyCollection<FiscalYear>, long)>(Error);
    public Task<(IReadOnlyCollection<FiscalPeriod> Items, long Total)> SearchPeriodsAsync(string tenantId, Guid? fiscalYearId, FiscalPeriodStatus? status, int page, int pageSize, CancellationToken ct) => Task.FromException<(IReadOnlyCollection<FiscalPeriod>, long)>(Error);
    public Task<FiscalPeriod?> GetOpenPeriodByDateAsync(string tenantId, DateOnly date, CancellationToken ct) => Task.FromException<FiscalPeriod?>(Error);
    public Task SaveChangesAsync(CancellationToken ct) => Task.FromException(Error);
}

internal sealed class FinancialDatabaseInitializer(IServiceProvider services) : IHostedService
{
    public async Task StartAsync(CancellationToken ct)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<FinancialDbContext>();
        await db.Database.EnsureCreatedAsync(ct);
        await db.Database.ExecuteSqlRawAsync("""
IF SCHEMA_ID('financial') IS NULL EXEC('CREATE SCHEMA financial');
IF OBJECT_ID('financial.accounts', 'U') IS NULL
BEGIN
    CREATE TABLE financial.accounts (
        Id uniqueidentifier NOT NULL PRIMARY KEY,
        TenantId nvarchar(64) NOT NULL,
        Code nvarchar(64) NOT NULL,
        Name nvarchar(256) NOT NULL,
        Type nvarchar(32) NOT NULL,
        Level int NOT NULL,
        ParentAccountId uniqueidentifier NULL,
        IsMovementAccount bit NOT NULL,
        Status nvarchar(32) NOT NULL,
        CreatedAtUtc datetimeoffset NOT NULL,
        UpdatedAtUtc datetimeoffset NOT NULL
    );
    CREATE UNIQUE INDEX IX_accounts_TenantId_Code ON financial.accounts(TenantId, Code);
    CREATE INDEX IX_accounts_TenantId_ParentAccountId ON financial.accounts(TenantId, ParentAccountId);
END;
IF OBJECT_ID('financial.fiscal_years', 'U') IS NULL
BEGIN
    CREATE TABLE financial.fiscal_years (
        Id uniqueidentifier NOT NULL PRIMARY KEY,
        TenantId nvarchar(64) NOT NULL,
        Year int NOT NULL,
        StartDate date NOT NULL,
        EndDate date NOT NULL,
        Status nvarchar(32) NOT NULL,
        CreatedAtUtc datetimeoffset NOT NULL,
        UpdatedAtUtc datetimeoffset NOT NULL
    );
    CREATE UNIQUE INDEX IX_fiscal_years_TenantId_Year ON financial.fiscal_years(TenantId, Year);
END;
IF OBJECT_ID('financial.fiscal_periods', 'U') IS NULL
BEGIN
    CREATE TABLE financial.fiscal_periods (
        Id uniqueidentifier NOT NULL PRIMARY KEY,
        TenantId nvarchar(64) NOT NULL,
        FiscalYearId uniqueidentifier NOT NULL,
        PeriodNumber int NOT NULL,
        StartDate date NOT NULL,
        EndDate date NOT NULL,
        Status nvarchar(32) NOT NULL,
        CreatedAtUtc datetimeoffset NOT NULL,
        UpdatedAtUtc datetimeoffset NOT NULL
    );
    CREATE UNIQUE INDEX IX_fiscal_periods_TenantId_FiscalYearId_PeriodNumber ON financial.fiscal_periods(TenantId, FiscalYearId, PeriodNumber);
    CREATE INDEX IX_fiscal_periods_TenantId_FiscalYearId_StartDate_EndDate ON financial.fiscal_periods(TenantId, FiscalYearId, StartDate, EndDate);
END;
""", ct);
    }
    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
}
