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
        await db.Accounts.AnyAsync(x => x.ParentAccountId == id && x.TenantId == tenantId && x.Status != AccountStatus.Archived, ct);
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

        var connectionString = configuration.GetConnectionString("FinancialDb");
        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            services.AddDbContext<FinancialDbContext>(x => x.UseSqlServer(connectionString));
            services.AddScoped<IAccountRepository, EfAccountRepository>();
            services.AddHealthChecks().AddCheck<FinancialSqlHealthCheck>("financial-sql", tags: ["ready"]);
            if (configuration.GetValue<bool>("Database:Initialize")) services.AddHostedService<FinancialDatabaseInitializer>();
        }
        else
        {
            services.AddScoped<IAccountRepository, UnconfiguredAccountRepository>();
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

internal sealed class FinancialDatabaseInitializer(IServiceProvider services) : IHostedService
{
    public async Task StartAsync(CancellationToken ct)
    {
        await using var scope = services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<FinancialDbContext>().Database.EnsureCreatedAsync(ct);
    }
    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
}
