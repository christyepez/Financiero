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
