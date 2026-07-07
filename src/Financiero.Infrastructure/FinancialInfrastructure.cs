using Financiero.Application;
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
    protected override void OnModelCreating(ModelBuilder modelBuilder) => modelBuilder.HasDefaultSchema("financial");
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

        var connectionString = configuration.GetConnectionString("FinancialDb");
        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            services.AddDbContext<FinancialDbContext>(x => x.UseSqlServer(connectionString));
            services.AddHealthChecks().AddCheck<FinancialSqlHealthCheck>("financial-sql", tags: ["ready"]);
            if (configuration.GetValue<bool>("Database:Initialize")) services.AddHostedService<FinancialDatabaseInitializer>();
        }
        return services;
    }
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
