using Financiero.Domain;

namespace Financiero.Application;

public sealed record ProductizationGate(string Code, string Status, string Description, bool Required, bool IsFoundationOnly);
public sealed record ProductizationBlocker(string Code, string Message, string Severity);
public sealed record ProductizationRecommendation(string Code, string Message);
public sealed record ProductizationReadinessResult(
    string Scope,
    Guid? DocumentId,
    string Status,
    bool IsReadyForProduction,
    bool DoesNotEnableProduction,
    IReadOnlyCollection<ProductizationGate> Gates,
    IReadOnlyCollection<ProductizationBlocker> Blockers,
    IReadOnlyCollection<ProductizationRecommendation> Recommendations,
    object PortalBoundaries,
    IReadOnlyCollection<string> DangerousFeatureFlagsBlocked,
    string Disclaimer);

public sealed class PurchaseProductizationReadinessService(
    IPurchaseTaxDocumentRepository purchases,
    IExternalApprovalRequestRepository approvals,
    IPortalAuditClient audit)
{
    private const string Disclaimer = "Purchase productization readiness is foundation-only. It does not enable SRI production, official ATS, legal RIDE, XAdES production, evidence upload or notification delivery.";

    public async Task<ProductizationReadinessResult> CheckAsync(PortalCallContext context, CancellationToken ct) =>
        await BuildAsync(null, context, ct);

    public async Task<ProductizationReadinessResult> CheckAsync(Guid id, PortalCallContext context, CancellationToken ct)
    {
        var document = await purchases.GetByIdAsync(id, context.TenantId, ct) ?? throw new FinancialApplicationException("purchase.not_found", "Purchase tax document was not found.");
        return await BuildAsync(document, context, ct);
    }

    private async Task<ProductizationReadinessResult> BuildAsync(PurchaseTaxDocument? document, PortalCallContext context, CancellationToken ct)
    {
        var approved = await approvals.ListAsync(context.TenantId, null, ExternalApprovalRequestStatus.ApprovedFoundation, ct);
        var gates = BaseGates("purchase", approved.Count);
        var blockers = BaseBlockers("purchase", approved.Count).ToList();
        if (document is null) blockers.Add(new("purchase.document.required", "A concrete purchase document is required before future production readiness can be evaluated.", "Warning"));
        else
        {
            if (document.Lines.Count == 0) blockers.Add(new("purchase.lines.required", "Purchase requires at least one line before future productization.", "Error"));
            if (document.Taxes.Count == 0) blockers.Add(new("purchase.taxes.required", "Purchase requires tax details before future productization.", "Error"));
            if (document.Status != PurchaseTaxDocumentStatus.Validated) blockers.Add(new("purchase.validation.required", "Purchase must be validated in foundation before any future productization gate.", "Error"));
        }

        await audit.RecordAsync(new("PurchaseProductizationReadinessQueried", "financial.purchase-productization-readiness", context.TenantId, new { DocumentId = document?.Id, Blockers = blockers.Count, ApprovedFoundationCount = approved.Count, ReadOnly = true }), context, ct);
        return Result("purchase", document?.Id, gates, blockers, "Complete purchase validation, external foundation approvals and Portal boundary contracts before any future production activation.");
    }

    internal static IReadOnlyCollection<ProductizationGate> BaseGates(string scope, int approvedFoundationCount) =>
    [
        new($"{scope}.catalogs", "RequiredFoundation", "Tax catalogs must remain versioned and reviewed.", true, true),
        new($"{scope}.external_approval", approvedFoundationCount > 0 ? "ApprovedFoundationOnly" : "MissingFoundationApproval", "External approval workflow is foundation only and never enables production.", true, true),
        new($"{scope}.content_file", ExternalApprovalPortalIntegrationBoundary.ContentFileReadiness().Status, "Portal Content/File is required for future evidence ownership.", true, true),
        new($"{scope}.notification", ExternalApprovalPortalIntegrationBoundary.NotificationReadiness().Status, "Portal Notification is required for future notification delivery.", true, true),
        new($"{scope}.portal_shell", "RequiredFoundation", "Portal Shell context, permissions and feature flags must be provided by Portal.", true, true)
    ];

    internal static IReadOnlyCollection<ProductizationBlocker> BaseBlockers(string scope, int approvedFoundationCount)
    {
        var blockers = new List<ProductizationBlocker>
        {
            new($"{scope}.production.blocked", "Production tax flows remain blocked by design.", "Critical"),
            new($"{scope}.sri.blocked", "SRI Test/Production real submission is not enabled.", "Critical"),
            new($"{scope}.ats.blocked", "Official ATS generation is not enabled.", "Critical"),
            new($"{scope}.ride.blocked", "Legal/final RIDE generation is not enabled.", "Critical"),
            new($"{scope}.xades.blocked", "Productive XAdES signing and certificate custody are not enabled.", "Critical"),
            new($"{scope}.evidence_upload.blocked", "Evidence upload is blocked; only Portal-owned metadata references are allowed.", "Critical"),
            new($"{scope}.notification_send.blocked", "Notification send is blocked; only foundation intents are prepared.", "Critical")
        };
        if (approvedFoundationCount == 0) blockers.Add(new($"{scope}.external_approval.missing", "No ApprovedFoundation request exists. Even ApprovedFoundation would not enable production.", "Error"));
        return blockers;
    }

    internal static ProductizationReadinessResult Result(string scope, Guid? id, IReadOnlyCollection<ProductizationGate> gates, IReadOnlyCollection<ProductizationBlocker> blockers, string next) =>
        new(scope, id, "BlockedFoundation", false, true, gates, blockers,
            [new($"{scope}.next", next)],
            new { ContentFile = ExternalApprovalPortalIntegrationBoundary.ContentFileReadiness(), Notification = ExternalApprovalPortalIntegrationBoundary.NotificationReadiness(), AuditOutbox = "Read-only readiness audit only; no domain mutation." },
            ["allowProductiveActivation=false", "allowOfficialTaxFlows=false", "allowSriSubmission=false", "allowAtsOfficialActions=false", "allowEvidenceUpload=false", "allowNotificationSend=false"],
            Disclaimer);
}

public sealed class VoidedDocumentProductizationReadinessService(
    IVoidedTaxDocumentRepository voided,
    IExternalApprovalRequestRepository approvals,
    IPortalAuditClient audit)
{
    private const string Disclaimer = "Voided document productization readiness is foundation-only. It does not register official annulment, send SRI data, upload evidence or send notifications.";

    public async Task<ProductizationReadinessResult> CheckAsync(PortalCallContext context, CancellationToken ct) =>
        await BuildAsync(null, context, ct);

    public async Task<ProductizationReadinessResult> CheckAsync(Guid id, PortalCallContext context, CancellationToken ct)
    {
        var document = await voided.GetByIdAsync(id, context.TenantId, ct) ?? throw new FinancialApplicationException("voided.not_found", "Voided tax document was not found.");
        return await BuildAsync(document, context, ct);
    }

    private async Task<ProductizationReadinessResult> BuildAsync(VoidedTaxDocument? document, PortalCallContext context, CancellationToken ct)
    {
        var approved = await approvals.ListAsync(context.TenantId, null, ExternalApprovalRequestStatus.ApprovedFoundation, ct);
        var gates = PurchaseProductizationReadinessService.BaseGates("voided", approved.Count);
        var blockers = PurchaseProductizationReadinessService.BaseBlockers("voided", approved.Count).ToList();
        if (document is null) blockers.Add(new("voided.document.required", "A concrete voided document is required before future productization readiness can be evaluated.", "Warning"));
        else
        {
            if (string.IsNullOrWhiteSpace(document.Reason)) blockers.Add(new("voided.reason.required", "Voided document requires sanitized reason.", "Error"));
            if (document.Status != VoidedTaxDocumentStatus.RegisteredFoundation) blockers.Add(new("voided.foundation_registration.required", "Voided document must remain registered foundation before future gates.", "Error"));
            if (document.VoidDate < document.IssueDate) blockers.Add(new("voided.date.invalid", "Void date cannot be earlier than issue date.", "Error"));
        }

        await audit.RecordAsync(new("VoidedDocumentProductizationReadinessQueried", "financial.voided-productization-readiness", context.TenantId, new { DocumentId = document?.Id, Blockers = blockers.Count, ApprovedFoundationCount = approved.Count, ReadOnly = true }), context, ct);
        return PurchaseProductizationReadinessService.Result("voided", document?.Id, gates, blockers, "Complete voided document foundation checks, external approvals and Portal boundary contracts before any future official annulment flow.");
    }
}
