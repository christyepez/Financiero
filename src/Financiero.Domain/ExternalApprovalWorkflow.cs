namespace Financiero.Domain;

public enum ExternalApprovalRequestScope { RIDE, ATS, XADES, SRI_TEST, SRI_PRODUCTION, CONTENT_FILE, CERTIFICATE_CUSTODY, SECURITY_GATE, TAX_LEGAL_REVIEW, RUNBOOK, PORTAL_SHELL }
public enum ExternalApprovalRequestStatus { Draft, Submitted, InReview, ApprovedFoundation, RejectedFoundation, Blocked, Superseded, Cancelled }
public enum ExternalApprovalDecisionKind { ApprovedFoundation, RejectedFoundation, Blocked }

public sealed class ExternalApprovalRequest
{
    private readonly List<ExternalApprovalRequirementItem> _requirements = [];
    private readonly List<ExternalApprovalEvidenceReference> _evidenceReferences = [];
    private readonly List<ExternalApprovalDecision> _decisions = [];
    private readonly List<ExternalApprovalTimelineEntry> _timeline = [];
    private ExternalApprovalRequest() { TenantId = ""; Title = ""; CreatedByDisplayName = ""; }
    private ExternalApprovalRequest(string tenantId, ExternalApprovalRequestScope scope, string title, string? fiscalPeriod, string createdByDisplayName, DateTimeOffset now)
    {
        Id = Guid.NewGuid();
        TenantId = Required(tenantId, "external_approval.tenant.required");
        Scope = scope;
        Title = Safe(title, "external_approval.title.required");
        FiscalPeriod = Clean(fiscalPeriod);
        CreatedByDisplayName = Safe(createdByDisplayName, "external_approval.created_by.required");
        Status = ExternalApprovalRequestStatus.Draft;
        CreatedAtUtc = UpdatedAtUtc = now;
        AddTimeline("Created", "External approval request foundation created.", now, CreatedByDisplayName);
    }

    public Guid Id { get; private set; }
    public string TenantId { get; private set; }
    public ExternalApprovalRequestScope Scope { get; private set; }
    public ExternalApprovalRequestStatus Status { get; private set; }
    public string Title { get; private set; }
    public string? FiscalPeriod { get; private set; }
    public string CreatedByDisplayName { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }
    public IReadOnlyCollection<ExternalApprovalRequirementItem> Requirements => _requirements.AsReadOnly();
    public IReadOnlyCollection<ExternalApprovalEvidenceReference> EvidenceReferences => _evidenceReferences.AsReadOnly();
    public IReadOnlyCollection<ExternalApprovalDecision> Decisions => _decisions.AsReadOnly();
    public IReadOnlyCollection<ExternalApprovalTimelineEntry> Timeline => _timeline.AsReadOnly();
    public bool DoesNotEnableProduction => true;

    public static ExternalApprovalRequest Create(string tenantId, ExternalApprovalRequestScope scope, string title, string? fiscalPeriod, string createdByDisplayName, IEnumerable<string> requirements, DateTimeOffset now)
    {
        var request = new ExternalApprovalRequest(tenantId, scope, title, fiscalPeriod, createdByDisplayName, now);
        foreach (var requirement in requirements.Where(x => !string.IsNullOrWhiteSpace(x))) request.AddRequirement(requirement, true, true, now);
        return request;
    }

    public void Submit(DateTimeOffset now, string actor)
    {
        if (Status != ExternalApprovalRequestStatus.Draft) throw new FinancialDomainException("external_approval.state.invalid", "Only draft approval requests can be submitted.");
        if (_requirements.Count == 0) throw new FinancialDomainException("external_approval.requirements.required", "Submitted approval request requires at least one requirement.");
        Status = ExternalApprovalRequestStatus.Submitted; Updated(now); AddTimeline("Submitted", "Foundation request submitted for external review.", now, actor);
    }

    public void StartReview(DateTimeOffset now, string actor)
    {
        if (Status is not (ExternalApprovalRequestStatus.Submitted or ExternalApprovalRequestStatus.InReview)) throw new FinancialDomainException("external_approval.state.invalid", "Only submitted approval requests can enter review.");
        Status = ExternalApprovalRequestStatus.InReview; Updated(now); AddTimeline("InReview", "Foundation request review started.", now, actor);
    }

    public ExternalApprovalEvidenceReference AddEvidenceReference(string provider, string referenceId, string displayName, string? hash, string? contentType, string? createdByDisplayName, DateTimeOffset now)
    {
        if (Status is ExternalApprovalRequestStatus.Cancelled or ExternalApprovalRequestStatus.Superseded) throw new FinancialDomainException("external_approval.state.invalid", "Cannot add evidence to a closed request.");
        GuardNoUnsafePayload(provider, referenceId, displayName, hash, contentType, createdByDisplayName);
        var reference = new ExternalApprovalEvidenceReference(Id, TenantId, Safe(provider, "external_approval.evidence.provider.required"), Safe(referenceId, "external_approval.evidence.reference.required"), Safe(displayName, "external_approval.evidence.name.required"), Clean(hash), Clean(contentType), now, Clean(createdByDisplayName));
        _evidenceReferences.Add(reference); Updated(now); AddTimeline("EvidenceReferenceAdded", "Sanitized evidence metadata reference added; no file stored.", now, createdByDisplayName ?? "foundation-user");
        return reference;
    }

    public ExternalApprovalDecision RecordDecision(ExternalApprovalDecisionKind decisionKind, string reason, string decidedByDisplayName, DateTimeOffset now)
    {
        if (Status is not (ExternalApprovalRequestStatus.Submitted or ExternalApprovalRequestStatus.InReview)) throw new FinancialDomainException("external_approval.state.invalid", "Decision requires submitted or in-review status.");
        if (decisionKind == ExternalApprovalDecisionKind.RejectedFoundation && string.IsNullOrWhiteSpace(reason)) throw new FinancialDomainException("external_approval.decision.reason.required", "Rejected foundation decision requires reason.");
        GuardNoUnsafePayload(reason, decidedByDisplayName);
        var decision = new ExternalApprovalDecision(Id, TenantId, decisionKind, Safe(reason, "external_approval.decision.reason.required"), Safe(decidedByDisplayName, "external_approval.decision.by.required"), now, true);
        _decisions.Add(decision);
        Status = decisionKind switch
        {
            ExternalApprovalDecisionKind.ApprovedFoundation => ExternalApprovalRequestStatus.ApprovedFoundation,
            ExternalApprovalDecisionKind.RejectedFoundation => ExternalApprovalRequestStatus.RejectedFoundation,
            _ => ExternalApprovalRequestStatus.Blocked
        };
        Updated(now); AddTimeline("DecisionRecorded", $"{decisionKind} recorded. Does not enable production.", now, decidedByDisplayName);
        return decision;
    }

    public void Cancel(string reason, DateTimeOffset now, string actor)
    {
        if (Status is ExternalApprovalRequestStatus.ApprovedFoundation or ExternalApprovalRequestStatus.RejectedFoundation) throw new FinancialDomainException("external_approval.state.invalid", "Cannot cancel a decided request.");
        GuardNoUnsafePayload(reason);
        Status = ExternalApprovalRequestStatus.Cancelled; Updated(now); AddTimeline("Cancelled", Safe(reason, "external_approval.cancel.reason.required"), now, actor);
    }

    public void AddRequirement(string description, bool requiresEvidence, bool requiresHumanReview, DateTimeOffset now)
    {
        GuardNoUnsafePayload(description);
        _requirements.Add(new ExternalApprovalRequirementItem(Id, TenantId, Safe(description, "external_approval.requirement.required"), requiresEvidence, requiresHumanReview, now));
        Updated(now);
    }
    private void AddTimeline(string action, string message, DateTimeOffset now, string actor) => _timeline.Add(new ExternalApprovalTimelineEntry(Id, TenantId, Safe(action, "external_approval.timeline.action.required"), Safe(message, "external_approval.timeline.message.required"), now, Clean(actor)));
    private void Updated(DateTimeOffset now) => UpdatedAtUtc = now;
    private static string Required(string value, string code) => string.IsNullOrWhiteSpace(value) ? throw new FinancialDomainException(code, code) : value.Trim();
    private static string Safe(string value, string code) { var clean = Required(value, code); GuardNoUnsafePayload(clean); return clean.Length > 512 ? clean[..512] : clean; }
    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    public static void GuardNoUnsafePayload(params string?[] values)
    {
        foreach (var value in values.Where(x => !string.IsNullOrWhiteSpace(x))!)
        {
            var text = value!;
            if (text.Contains("<", StringComparison.Ordinal) || text.Contains(">", StringComparison.Ordinal)) throw new FinancialDomainException("external_approval.payload.xml_rejected", "XML or HTML payload is not allowed in approval metadata.");
            if (text.Contains("base64", StringComparison.OrdinalIgnoreCase) || text.Length > 2048) throw new FinancialDomainException("external_approval.payload.base64_rejected", "Base64 or large payload is not allowed in approval metadata.");
            if (text.Contains("BEGIN CERTIFICATE", StringComparison.OrdinalIgnoreCase) || text.Contains("PRIVATE KEY", StringComparison.OrdinalIgnoreCase) || text.EndsWith(".p12", StringComparison.OrdinalIgnoreCase) || text.EndsWith(".pfx", StringComparison.OrdinalIgnoreCase)) throw new FinancialDomainException("external_approval.payload.certificate_rejected", "Certificate material is not allowed in approval metadata.");
            if ((text.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || text.StartsWith("https://", StringComparison.OrdinalIgnoreCase)) && (text.Contains('?', StringComparison.Ordinal) || text.Contains("token", StringComparison.OrdinalIgnoreCase))) throw new FinancialDomainException("external_approval.payload.url_rejected", "URLs with query strings or tokens are not allowed.");
        }
    }
}

public sealed class ExternalApprovalRequirementItem
{
    private ExternalApprovalRequirementItem() { TenantId = ""; Description = ""; }
    internal ExternalApprovalRequirementItem(Guid requestId, string tenantId, string description, bool requiresEvidence, bool requiresHumanReview, DateTimeOffset createdAtUtc)
    { Id = Guid.NewGuid(); ExternalApprovalRequestId = requestId; TenantId = tenantId; Description = description; RequiresEvidence = requiresEvidence; RequiresHumanReview = requiresHumanReview; CreatedAtUtc = createdAtUtc; }
    public Guid Id { get; private set; }
    public Guid ExternalApprovalRequestId { get; private set; }
    public string TenantId { get; private set; }
    public string Description { get; private set; }
    public bool RequiresEvidence { get; private set; }
    public bool RequiresHumanReview { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
}

public sealed class ExternalApprovalEvidenceReference
{
    private ExternalApprovalEvidenceReference() { TenantId = ""; Provider = ""; ReferenceId = ""; DisplayName = ""; }
    internal ExternalApprovalEvidenceReference(Guid requestId, string tenantId, string provider, string referenceId, string displayName, string? hash, string? contentType, DateTimeOffset createdAtUtc, string? createdByDisplayName)
    { Id = Guid.NewGuid(); ExternalApprovalRequestId = requestId; TenantId = tenantId; Provider = provider; ReferenceId = referenceId; DisplayName = displayName; Hash = hash; ContentType = contentType; CreatedAtUtc = createdAtUtc; CreatedByDisplayName = createdByDisplayName; }
    public Guid Id { get; private set; }
    public Guid ExternalApprovalRequestId { get; private set; }
    public string TenantId { get; private set; }
    public string Provider { get; private set; }
    public string ReferenceId { get; private set; }
    public string DisplayName { get; private set; }
    public string? Hash { get; private set; }
    public string? ContentType { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public string? CreatedByDisplayName { get; private set; }
}

public sealed class ExternalApprovalDecision
{
    private ExternalApprovalDecision() { TenantId = ""; Reason = ""; DecidedByDisplayName = ""; }
    internal ExternalApprovalDecision(Guid requestId, string tenantId, ExternalApprovalDecisionKind decisionKind, string reason, string decidedByDisplayName, DateTimeOffset decidedAtUtc, bool doesNotEnableProduction)
    { Id = Guid.NewGuid(); ExternalApprovalRequestId = requestId; TenantId = tenantId; DecisionKind = decisionKind; Reason = reason; DecidedByDisplayName = decidedByDisplayName; DecidedAtUtc = decidedAtUtc; DoesNotEnableProduction = doesNotEnableProduction; }
    public Guid Id { get; private set; }
    public Guid ExternalApprovalRequestId { get; private set; }
    public string TenantId { get; private set; }
    public ExternalApprovalDecisionKind DecisionKind { get; private set; }
    public string Reason { get; private set; }
    public string DecidedByDisplayName { get; private set; }
    public DateTimeOffset DecidedAtUtc { get; private set; }
    public bool DoesNotEnableProduction { get; private set; }
}

public sealed class ExternalApprovalTimelineEntry
{
    private ExternalApprovalTimelineEntry() { TenantId = ""; Action = ""; Message = ""; }
    internal ExternalApprovalTimelineEntry(Guid requestId, string tenantId, string action, string message, DateTimeOffset createdAtUtc, string? actorDisplayName)
    { Id = Guid.NewGuid(); ExternalApprovalRequestId = requestId; TenantId = tenantId; Action = action; Message = message; CreatedAtUtc = createdAtUtc; ActorDisplayName = actorDisplayName; }
    public Guid Id { get; private set; }
    public Guid ExternalApprovalRequestId { get; private set; }
    public string TenantId { get; private set; }
    public string Action { get; private set; }
    public string Message { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public string? ActorDisplayName { get; private set; }
}
