namespace Financiero.Application;

public sealed record PortalCallContext(string TenantId, string CorrelationId, string? BearerToken = null);
public sealed record AuditRecord(string Action, string Resource, string ResourceId, object? Metadata = null);
public sealed record NotificationRequest(string TemplateCode, IReadOnlyCollection<string> Recipients, IReadOnlyDictionary<string,string> Variables, string IdempotencyKey);
public sealed record OutboxEnvelope(Guid EventId, string EventType, int Version, DateTimeOffset OccurredAtUtc, string CorrelationId, string PayloadJson);

public interface IPortalAuditClient { Task RecordAsync(AuditRecord record, PortalCallContext context, CancellationToken ct); }
public interface IPortalNotificationClient { Task RequestAsync(NotificationRequest request, PortalCallContext context, CancellationToken ct); }
public interface IPortalOutboxClient { Task EnqueueAsync(OutboxEnvelope message, PortalCallContext context, CancellationToken ct); }
public interface IPortalConfigurationClient { Task<string?> GetAsync(string key, PortalCallContext context, CancellationToken ct); }
public interface IPortalSecurityClient { Task<bool> HasPermissionAsync(string permission, PortalCallContext context, CancellationToken ct); }
public interface IPortalMenuRegistrationClient { Task RegisterModuleAsync(string moduleCode, PortalCallContext context, CancellationToken ct); }

public interface IFinancialConfigurationReader
{
    Task<bool> GetBoolAsync(string key, bool defaultValue, PortalCallContext context, CancellationToken ct);
    Task<int> GetIntAsync(string key, int defaultValue, PortalCallContext context, CancellationToken ct);
    Task<string> GetStringAsync(string key, string defaultValue, PortalCallContext context, CancellationToken ct);
}

public sealed class PortalOptions
{
    public const string SectionName = "Portal";
    public Uri? GatewayBaseUrl { get; set; }
    public Uri? SecurityBaseUrl { get; set; }
    public Uri? ConfigurationBaseUrl { get; set; }
    public Uri? MenuBaseUrl { get; set; }
    public Uri? AuditBaseUrl { get; set; }
    public Uri? OutboxBaseUrl { get; set; }
    public int TimeoutSeconds { get; set; } = 10;
    public bool UseDevelopmentAdapters { get; set; } = true;
    public bool UsePortalAudit { get; set; }
    public bool UsePortalNotification { get; set; }
    public bool UsePortalOutbox { get; set; }
    public bool UsePortalConfiguration { get; set; }
}

public sealed class FinancialPlatformOptions
{
    public const string SectionName = "FinancialPlatform";
    public string TenantId { get; set; } = "default";
    public string CorrelationHeader { get; set; } = "X-Correlation-ID";
}
