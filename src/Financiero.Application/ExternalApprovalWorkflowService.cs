using Financiero.Domain;

namespace Financiero.Application;

public enum ExternalApprovalScope { AtsOfficialXml, RideLegalFinal, XadesSignature, SriTestSubmission, SriProductionSubmission, PortalContentFileProduction, CertificateCustody, SecurityProductionGate, OperationalRunbook }
public enum ExternalApprovalStatus { NotStarted, Required, InReview, ApprovedFoundation, Rejected, Blocked, NotApplicable }
public sealed record ExternalApprovalRequirement(string Code, string Description, bool RequiresEvidence, bool RequiresHumanReview);
public sealed record ExternalApprovalGate(ExternalApprovalScope Scope, ExternalApprovalStatus Status, bool IsFoundationOnly, bool IsProductionApproval, bool RequiresHumanReview, bool RequiresEvidence, bool DoesNotEnableProduction, IReadOnlyCollection<ExternalApprovalRequirement> Requirements, IReadOnlyCollection<string> BlockingRisks, string RecommendedNextAction, string Disclaimer);
public sealed record ExternalApprovalDecisionMetadata(ExternalApprovalScope Scope, ExternalApprovalStatus Status, DateTimeOffset EvaluatedAtUtc, string EvaluatedBy, bool DoesNotEnableProduction);
public sealed record ExternalApprovalReadinessResult(string Scope, IReadOnlyCollection<ExternalApprovalGate> Gates, IReadOnlyCollection<string> MissingRequirements, IReadOnlyCollection<string> RequiredEvidence, IReadOnlyCollection<string> BlockingRisks, string RecommendedNextAction, ExternalApprovalDecisionMetadata DecisionMetadata, string Disclaimer);

public interface IExternalApprovalReadinessService
{
    Task<IReadOnlyCollection<ExternalApprovalGate>> GetAllAsync(PortalCallContext context, CancellationToken ct);
    Task<ExternalApprovalGate> GetAsync(string scope, PortalCallContext context, CancellationToken ct);
    Task<ExternalApprovalReadinessResult> CheckAsync(string scope, PortalCallContext context, CancellationToken ct);
}

public sealed class ExternalApprovalReadinessService(IPortalAuditClient audit) : IExternalApprovalReadinessService
{
    private const string Disclaimer = "External approval workflow foundation only. It is advisory, read-only and does not enable production, SRI submission, official ATS XML or legal RIDE.";
    private static readonly ExternalApprovalGate[] Gates =
    [
        Gate(ExternalApprovalScope.AtsOfficialXml, "official-ats-schema", "Official ATS XSD/layout reviewed and approved.", "tax-review", "External Ecuador tax review for ATS XML."),
        Gate(ExternalApprovalScope.RideLegalFinal, "legal-ride-template", "Legal RIDE template reviewed.", "tax-legal-review", "Tax/legal approval for final RIDE output."),
        Gate(ExternalApprovalScope.XadesSignature, "certificate-custody", "Certificate custody approved outside the repository.", "xades-provider", "Secure XAdES provider approved."),
        Gate(ExternalApprovalScope.SriTestSubmission, "sri-test-credentials", "SRI Test credentials and synthetic issuer approved.", "test-evidence", "Manual SRI Test evidence collected."),
        Gate(ExternalApprovalScope.SriProductionSubmission, "production-authorization", "Production submission explicitly authorized.", "rollback-runbook", "Rollback and incident runbook approved."),
        Gate(ExternalApprovalScope.PortalContentFileProduction, "content-file-contract", "Portal Content/File production contract approved.", "payload-policy", "Payload, retention and masking policy approved."),
        Gate(ExternalApprovalScope.CertificateCustody, "custody-owner", "Certificate owner and rotation process assigned.", "secret-store", "Secret store access approved."),
        Gate(ExternalApprovalScope.SecurityProductionGate, "security-review", "Security review completed.", "audit-redaction", "Audit/log redaction approved."),
        Gate(ExternalApprovalScope.OperationalRunbook, "operations-runbook", "Operational runbook approved.", "support-owner", "Support owner and escalation path assigned.")
    ];

    public async Task<IReadOnlyCollection<ExternalApprovalGate>> GetAllAsync(PortalCallContext context, CancellationToken ct)
    {
        await audit.RecordAsync(new("ExternalApprovalsQueried", "financial.external-approval", context.TenantId, new { Count = Gates.Length }), context, ct);
        return Gates;
    }

    public async Task<ExternalApprovalGate> GetAsync(string scope, PortalCallContext context, CancellationToken ct)
    {
        var parsed = Parse(scope);
        var gate = Gates.First(x => x.Scope == parsed);
        await audit.RecordAsync(new("ExternalApprovalsQueried", "financial.external-approval", context.TenantId, new { Scope = parsed.ToString() }), context, ct);
        return gate;
    }

    public async Task<ExternalApprovalReadinessResult> CheckAsync(string scope, PortalCallContext context, CancellationToken ct)
    {
        var selected = scope.Equals("all", StringComparison.OrdinalIgnoreCase) ? Gates : [await GetAsync(scope, context, ct)];
        var missing = selected.SelectMany(x => x.Requirements).Select(x => x.Description).Distinct().ToArray();
        var evidence = selected.SelectMany(x => x.Requirements).Where(x => x.RequiresEvidence).Select(x => x.Description).Distinct().ToArray();
        var risks = selected.SelectMany(x => x.BlockingRisks).Distinct().ToArray();
        var next = scope.Equals("all", StringComparison.OrdinalIgnoreCase) ? "Complete external review package and approve each gate outside the repository before enabling any production capability." : selected.Single().RecommendedNextAction;
        await audit.RecordAsync(new("ExternalApprovalReadinessQueried", "financial.external-approval", context.TenantId, new { Scope = scope, GateCount = selected.Length, MissingCount = missing.Length, RiskCount = risks.Length }), context, ct);
        var metadata = new ExternalApprovalDecisionMetadata(scope.Equals("all", StringComparison.OrdinalIgnoreCase) ? ExternalApprovalScope.SecurityProductionGate : selected.First().Scope, ExternalApprovalStatus.Required, DateTimeOffset.UtcNow, "foundation-readiness-service", true);
        return new(scope, selected, missing, evidence, risks, next, metadata, Disclaimer);
    }

    private static ExternalApprovalScope Parse(string scope)
    {
        var normalized = scope.Trim().Replace("-", "", StringComparison.OrdinalIgnoreCase).Replace("_", "", StringComparison.OrdinalIgnoreCase);
        return normalized.ToLowerInvariant() switch
        {
            "ats" or "atsofficialxml" => ExternalApprovalScope.AtsOfficialXml,
            "ride" or "ridelegalfinal" => ExternalApprovalScope.RideLegalFinal,
            "xades" or "xadessignature" => ExternalApprovalScope.XadesSignature,
            "sritest" or "sritestsubmission" => ExternalApprovalScope.SriTestSubmission,
            "sriproduction" or "sriproductionsubmission" => ExternalApprovalScope.SriProductionSubmission,
            "portalcontentfile" or "portalcontentfileproduction" => ExternalApprovalScope.PortalContentFileProduction,
            "certificatecustody" => ExternalApprovalScope.CertificateCustody,
            "security" or "securityproductiongate" => ExternalApprovalScope.SecurityProductionGate,
            "runbook" or "operationalrunbook" => ExternalApprovalScope.OperationalRunbook,
            _ => throw new FinancialApplicationException("external_approval.scope.invalid", "External approval scope is invalid.")
        };
    }

    private static ExternalApprovalGate Gate(ExternalApprovalScope scope, string req1, string desc1, string req2, string desc2) =>
        new(scope, ExternalApprovalStatus.Required, true, false, true, true, true,
            [new(req1, desc1, true, true), new(req2, desc2, true, true)],
            [$"{scope} is blocked for production until external evidence and approval are completed."],
            $"Prepare evidence package and request external approval for {scope}.",
            Disclaimer);
}

public sealed record CreateExternalApprovalRequest(string Scope, string Title, string? FiscalPeriod, string CreatedByDisplayName, IReadOnlyCollection<ExternalApprovalRequirementDto>? Requirements);
public sealed record AddExternalApprovalEvidenceReferenceRequest(string Provider, string ReferenceId, string DisplayName, string? Hash, string? ContentType, string? CreatedByDisplayName, long? SizeBytes = null, string? Purpose = null, string? RetentionHint = null);
public sealed record RecordExternalApprovalDecisionRequest(string Decision, string Reason, string DecidedByDisplayName);
public sealed record CancelExternalApprovalRequest(string Reason, string ActorDisplayName);
public sealed record ExternalApprovalRequirementDto(string Description, bool RequiresEvidence, bool RequiresHumanReview);
public sealed record ExternalApprovalEvidenceReferenceDto(Guid Id, string Provider, string ReferenceId, string DisplayName, string? Hash, string? ContentType, DateTimeOffset CreatedAtUtc, string? CreatedByDisplayName);
public sealed record ExternalApprovalDecisionDto(Guid Id, string DecisionKind, string Reason, string DecidedByDisplayName, DateTimeOffset DecidedAtUtc, bool DoesNotEnableProduction);
public sealed record ExternalApprovalTimelineDto(string Action, string Message, DateTimeOffset CreatedAtUtc, string? ActorDisplayName);
public sealed record ExternalApprovalRequestDto(Guid Id, string Scope, string Status, string Title, string? FiscalPeriod, DateTimeOffset CreatedAtUtc, DateTimeOffset UpdatedAtUtc, bool DoesNotEnableProduction, IReadOnlyCollection<ExternalApprovalRequirementDto> Requirements, IReadOnlyCollection<ExternalApprovalEvidenceReferenceDto> EvidenceReferences, IReadOnlyCollection<ExternalApprovalDecisionDto> Decisions, IReadOnlyCollection<ExternalApprovalTimelineDto> Timeline, string Disclaimer);

public interface IExternalApprovalRequestRepository
{
    Task AddAsync(ExternalApprovalRequest request, CancellationToken ct);
    Task<ExternalApprovalRequest?> GetByIdAsync(Guid id, string tenantId, CancellationToken ct);
    Task<IReadOnlyCollection<ExternalApprovalRequest>> ListAsync(string tenantId, ExternalApprovalRequestScope? scope, ExternalApprovalRequestStatus? status, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}

public sealed class ExternalApprovalWorkflowCommandService(IExternalApprovalRequestRepository requests, IPortalAuditClient audit, IPortalOutboxClient outbox)
{
    private static readonly PortalContentFileReferenceValidator ContentFileValidator = new();

    public async Task<ExternalApprovalRequestDto> CreateRequestAsync(CreateExternalApprovalRequest request, PortalCallContext context, CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow;
        var entity = ExternalApprovalRequest.Create(context.TenantId, ParseScope(request.Scope), Sanitize(request.Title), CleanPeriod(request.FiscalPeriod), Sanitize(request.CreatedByDisplayName), [], now);
        foreach (var requirement in request.Requirements ?? [])
        {
            entity.AddRequirement(requirement.Description, requirement.RequiresEvidence, requirement.RequiresHumanReview, now);
        }
        await requests.AddAsync(entity, ct);
        await requests.SaveChangesAsync(ct);
        await AuditAsync("ExternalApprovalRequestCreated", entity, context, ct);
        await OutboxAsync("ExternalApprovalRequestCreated", entity, context, ct);
        return ToDto(entity);
    }

    public Task<ExternalApprovalRequestDto> SubmitRequestAsync(Guid id, PortalCallContext context, CancellationToken ct) => MutateAsync(id, context, "ExternalApprovalRequestSubmitted", (entity, now) => entity.Submit(now, "Portal delegated user"), ct);
    public Task<ExternalApprovalRequestDto> StartReviewAsync(Guid id, PortalCallContext context, CancellationToken ct) => MutateAsync(id, context, "ExternalApprovalReviewStarted", (entity, now) => entity.StartReview(now, "Portal delegated reviewer"), ct);

    public async Task<ExternalApprovalRequestDto> AddEvidenceReferenceAsync(Guid id, AddExternalApprovalEvidenceReferenceRequest request, PortalCallContext context, CancellationToken ct)
    {
        var entity = await GetRequiredAsync(id, context, ct);
        var portalReference = ContentFileValidator.Validate(new(
            Sanitize(request.Provider),
            Sanitize(request.ReferenceId),
            Sanitize(request.DisplayName),
            Clean(request.ContentType),
            Clean(request.Hash),
            request.SizeBytes,
            DateTimeOffset.UtcNow,
            Clean(request.CreatedByDisplayName),
            Clean(request.Purpose) ?? ExternalApprovalContentFilePurpose.ExternalApprovalEvidence.ToString(),
            Clean(request.RetentionHint),
            PortalContentFileLinkPolicy.ReferenceOnly));
        entity.AddEvidenceReference(Sanitize(request.Provider), Sanitize(request.ReferenceId), Sanitize(request.DisplayName), Clean(request.Hash), Clean(request.ContentType), Clean(request.CreatedByDisplayName), DateTimeOffset.UtcNow);
        await requests.SaveChangesAsync(ct);
        await AuditAsync("ExternalApprovalEvidenceReferenceAdded", entity, context, ct);
        await OutboxAsync("ExternalApprovalEvidenceReferenceAdded", entity, context, ct, new { portalReference.Purpose, portalReference.LinkPolicy, portalReference.SizeBytes, ReferenceOnly = true });
        return ToDto(entity);
    }

    public async Task<ExternalApprovalRequestDto> RecordDecisionAsync(Guid id, RecordExternalApprovalDecisionRequest request, PortalCallContext context, CancellationToken ct)
    {
        var entity = await GetRequiredAsync(id, context, ct);
        entity.RecordDecision(ParseDecision(request.Decision), Sanitize(request.Reason), Sanitize(request.DecidedByDisplayName), DateTimeOffset.UtcNow);
        await requests.SaveChangesAsync(ct);
        await AuditAsync("ExternalApprovalDecisionRecorded", entity, context, ct);
        await OutboxAsync("ExternalApprovalDecisionRecorded", entity, context, ct);
        await NotificationIntentAsync(PortalNotificationPurpose.ExternalApprovalDecisionRecorded, entity, context, ct);
        return ToDto(entity);
    }

    public async Task<ExternalApprovalRequestDto> CancelRequestAsync(Guid id, CancelExternalApprovalRequest request, PortalCallContext context, CancellationToken ct)
    {
        var entity = await GetRequiredAsync(id, context, ct);
        entity.Cancel(Sanitize(request.Reason), DateTimeOffset.UtcNow, Sanitize(request.ActorDisplayName));
        await requests.SaveChangesAsync(ct);
        await AuditAsync("ExternalApprovalRequestCancelled", entity, context, ct);
        await OutboxAsync("ExternalApprovalRequestCancelled", entity, context, ct);
        await NotificationIntentAsync(PortalNotificationPurpose.ExternalApprovalCancelled, entity, context, ct);
        return ToDto(entity);
    }

    private async Task<ExternalApprovalRequestDto> MutateAsync(Guid id, PortalCallContext context, string eventName, Action<ExternalApprovalRequest, DateTimeOffset> mutation, CancellationToken ct)
    {
        var entity = await GetRequiredAsync(id, context, ct);
        mutation(entity, DateTimeOffset.UtcNow);
        await requests.SaveChangesAsync(ct);
        await AuditAsync(eventName, entity, context, ct);
        await OutboxAsync(eventName, entity, context, ct);
        if (eventName == "ExternalApprovalRequestSubmitted") await NotificationIntentAsync(PortalNotificationPurpose.ExternalApprovalSubmitted, entity, context, ct);
        if (eventName == "ExternalApprovalReviewStarted") await NotificationIntentAsync(PortalNotificationPurpose.ExternalApprovalReviewStarted, entity, context, ct);
        return ToDto(entity);
    }

    private async Task<ExternalApprovalRequest> GetRequiredAsync(Guid id, PortalCallContext context, CancellationToken ct) =>
        await requests.GetByIdAsync(id, context.TenantId, ct) ?? throw new FinancialApplicationException("external_approval.not_found", "External approval request was not found.");
    private Task AuditAsync(string eventName, ExternalApprovalRequest entity, PortalCallContext context, CancellationToken ct) => audit.RecordAsync(new(eventName, "financial.external-approval-request", context.TenantId, new { entity.Id, Scope = entity.Scope.ToString(), Status = entity.Status.ToString(), entity.DoesNotEnableProduction }), context, ct);
    private Task OutboxAsync(string eventName, ExternalApprovalRequest entity, PortalCallContext context, CancellationToken ct, object? extra = null) => outbox.EnqueueAsync(new(Guid.NewGuid(), eventName, 1, DateTimeOffset.UtcNow, context.CorrelationId, System.Text.Json.JsonSerializer.Serialize(new { entity.Id, Scope = entity.Scope.ToString(), Status = entity.Status.ToString(), entity.DoesNotEnableProduction, Extra = extra })), context, ct);
    private Task NotificationIntentAsync(PortalNotificationPurpose purpose, ExternalApprovalRequest entity, PortalCallContext context, CancellationToken ct)
    {
        var intent = ExternalApprovalPortalIntegrationBoundary.BuildNotificationIntent(purpose, entity);
        return outbox.EnqueueAsync(new(Guid.NewGuid(), "ExternalApprovalNotificationIntentPrepared", 1, DateTimeOffset.UtcNow, context.CorrelationId, System.Text.Json.JsonSerializer.Serialize(new { entity.Id, Scope = entity.Scope.ToString(), Status = entity.Status.ToString(), intent.Purpose, intent.TemplateKey, intent.SendDisabled, intent.IsFoundationOnly })), context, ct);
    }
    private static string Sanitize(string value) { ExternalApprovalRequest.GuardNoUnsafePayload(value); return string.IsNullOrWhiteSpace(value) ? throw new FinancialApplicationException("external_approval.input.required", "External approval input is required.") : value.Trim(); }
    private static string? Clean(string? value) { if (string.IsNullOrWhiteSpace(value)) return null; ExternalApprovalRequest.GuardNoUnsafePayload(value); return value.Trim(); }
    private static string? CleanPeriod(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    internal static ExternalApprovalRequestScope ParseScope(string value) => Enum.TryParse<ExternalApprovalRequestScope>(value.Replace("-", "_", StringComparison.OrdinalIgnoreCase), true, out var scope) ? scope : throw new FinancialApplicationException("external_approval.scope.invalid", "External approval request scope is invalid.");
    private static ExternalApprovalDecisionKind ParseDecision(string value) => Enum.TryParse<ExternalApprovalDecisionKind>(value, true, out var decision) ? decision : throw new FinancialApplicationException("external_approval.decision.invalid", "External approval decision is invalid.");
    internal static ExternalApprovalRequestDto ToDto(ExternalApprovalRequest entity) => new(entity.Id, entity.Scope.ToString(), entity.Status.ToString(), entity.Title, entity.FiscalPeriod, entity.CreatedAtUtc, entity.UpdatedAtUtc, entity.DoesNotEnableProduction,
        entity.Requirements.Select(x => new ExternalApprovalRequirementDto(x.Description, x.RequiresEvidence, x.RequiresHumanReview)).ToArray(),
        entity.EvidenceReferences.Select(x => new ExternalApprovalEvidenceReferenceDto(x.Id, x.Provider, x.ReferenceId, x.DisplayName, x.Hash, x.ContentType, x.CreatedAtUtc, x.CreatedByDisplayName)).ToArray(),
        entity.Decisions.Select(x => new ExternalApprovalDecisionDto(x.Id, x.DecisionKind.ToString(), x.Reason, x.DecidedByDisplayName, x.DecidedAtUtc, x.DoesNotEnableProduction)).ToArray(),
        entity.Timeline.OrderBy(x => x.CreatedAtUtc).Select(x => new ExternalApprovalTimelineDto(x.Action, x.Message, x.CreatedAtUtc, x.ActorDisplayName)).ToArray(),
        "Foundation only. Does not enable SRI production, official ATS, legal RIDE, XAdES production or tax/legal approval.");
}

public sealed class ExternalApprovalWorkflowQueryService(IExternalApprovalRequestRepository requests, IExternalApprovalReadinessService readiness, IPortalAuditClient audit)
{
    public async Task<IReadOnlyCollection<ExternalApprovalRequestDto>> ListRequestsAsync(string? scope, string? status, PortalCallContext context, CancellationToken ct)
    {
        ExternalApprovalRequestScope? parsedScope = string.IsNullOrWhiteSpace(scope) ? null : ExternalApprovalWorkflowCommandService.ParseScope(scope);
        ExternalApprovalRequestStatus? parsedStatus = string.IsNullOrWhiteSpace(status) ? null : Enum.TryParse<ExternalApprovalRequestStatus>(status, true, out var s) ? s : throw new FinancialApplicationException("external_approval.status.invalid", "External approval request status is invalid.");
        var result = await requests.ListAsync(context.TenantId, parsedScope, parsedStatus, ct);
        await audit.RecordAsync(new("ExternalApprovalRequestsQueried", "financial.external-approval-request", context.TenantId, new { Count = result.Count }), context, ct);
        return result.Select(ExternalApprovalWorkflowCommandService.ToDto).ToArray();
    }

    public async Task<ExternalApprovalRequestDto> GetRequestAsync(Guid id, PortalCallContext context, CancellationToken ct)
    {
        var entity = await requests.GetByIdAsync(id, context.TenantId, ct) ?? throw new FinancialApplicationException("external_approval.not_found", "External approval request was not found.");
        await audit.RecordAsync(new("ExternalApprovalRequestQueried", "financial.external-approval-request", context.TenantId, new { entity.Id }), context, ct);
        return ExternalApprovalWorkflowCommandService.ToDto(entity);
    }

    public async Task<object> GetReadinessWithPersistedRequestsAsync(string scope, PortalCallContext context, CancellationToken ct)
    {
        var readinessResult = await readiness.CheckAsync(scope, context, ct);
        var persisted = await ListRequestsAsync(scope.Equals("all", StringComparison.OrdinalIgnoreCase) ? null : scope, null, context, ct);
        return new { readiness = readinessResult, persistedRequests = persisted, disclaimer = "Persisted approval requests are foundation metadata only and do not enable production." };
    }

    public async Task<ExternalApprovalIntegrationReadiness> GetIntegrationReadinessAsync(PortalCallContext context, CancellationToken ct)
    {
        var result = ExternalApprovalPortalIntegrationBoundary.Readiness();
        await audit.RecordAsync(new("ExternalApprovalIntegrationReadinessQueried", "financial.external-approval-request", context.TenantId, new { result.ContentFile.Status, NotificationStatus = result.Notification.Status, FoundationOnly = true }), context, ct);
        return result;
    }
}
