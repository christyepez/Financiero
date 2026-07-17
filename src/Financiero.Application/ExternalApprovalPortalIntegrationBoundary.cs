using Financiero.Domain;

namespace Financiero.Application;

public enum ExternalApprovalContentFilePurpose { ExternalApprovalEvidence, TaxLegalReview, SriTestEvidence, OperationalRunbook, PortalReadiness }
public enum PortalContentFileLinkPolicy { ReferenceOnly, PortalOwned, NoDirectLink }

public sealed record PortalContentFileEvidenceReference(
    string Provider,
    string ReferenceId,
    string DisplayName,
    string? ContentType,
    string? Hash,
    long? SizeBytes,
    DateTimeOffset CreatedAtUtc,
    string? CreatedByDisplayName,
    string? Purpose,
    string? RetentionHint,
    PortalContentFileLinkPolicy LinkPolicy);

public sealed record PortalContentFileReadiness(
    string Status,
    bool IsPortalOwned,
    bool AllowsUpload,
    bool AllowsDownload,
    bool AllowsSensitiveLinks,
    IReadOnlyCollection<string> MissingPortalDependencies,
    IReadOnlyCollection<string> Blockers,
    string Disclaimer);

public sealed class PortalContentFileReferenceValidator
{
    public PortalContentFileEvidenceReference Validate(PortalContentFileEvidenceReference reference)
    {
        Guard(reference.Provider, reference.ReferenceId, reference.DisplayName, reference.ContentType, reference.Hash, reference.CreatedByDisplayName, reference.Purpose, reference.RetentionHint);
        if (reference.SizeBytes is < 0 or > 10_000_000) throw new FinancialApplicationException("portal_content_file.size.invalid", "Evidence reference size metadata is invalid.");
        if (LooksLikeLocalPath(reference.ReferenceId)) throw new FinancialApplicationException("portal_content_file.local_path.rejected", "Local file paths are not allowed as evidence references.");
        if (LooksLikeSensitiveUrl(reference.ReferenceId)) throw new FinancialApplicationException("portal_content_file.url.rejected", "Tokenized or querystring URLs are not allowed as evidence references.");
        if (reference.LinkPolicy != PortalContentFileLinkPolicy.ReferenceOnly && reference.LinkPolicy != PortalContentFileLinkPolicy.PortalOwned && reference.LinkPolicy != PortalContentFileLinkPolicy.NoDirectLink)
            throw new FinancialApplicationException("portal_content_file.link_policy.invalid", "Evidence link policy is invalid.");
        return reference;
    }

    private static void Guard(params string?[] values)
    {
        foreach (var value in values.Where(x => !string.IsNullOrWhiteSpace(x))!)
        {
            var text = value!;
            ExternalApprovalRequest.GuardNoUnsafePayload(text);
            if (text.Contains("data:", StringComparison.OrdinalIgnoreCase) || text.Contains("application/octet-stream", StringComparison.OrdinalIgnoreCase))
                throw new FinancialApplicationException("portal_content_file.payload.rejected", "Embedded or binary evidence payloads are not allowed.");
            if (text.Contains("claveAcceso>", StringComparison.OrdinalIgnoreCase) || text.Contains("factura>", StringComparison.OrdinalIgnoreCase) || text.Contains("ats>", StringComparison.OrdinalIgnoreCase))
                throw new FinancialApplicationException("portal_content_file.xml_payload.rejected", "SRI/XML payloads are not allowed in evidence metadata.");
        }
    }

    private static bool LooksLikeSensitiveUrl(string value) =>
        (value.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || value.StartsWith("https://", StringComparison.OrdinalIgnoreCase)) &&
        (value.Contains('?', StringComparison.Ordinal) || value.Contains("token", StringComparison.OrdinalIgnoreCase) || value.Contains("sig=", StringComparison.OrdinalIgnoreCase));

    private static bool LooksLikeLocalPath(string value) =>
        value.StartsWith(@"C:\", StringComparison.OrdinalIgnoreCase) ||
        value.StartsWith(@"\\", StringComparison.OrdinalIgnoreCase) ||
        value.StartsWith("/", StringComparison.OrdinalIgnoreCase) ||
        value.Contains(@"\..\", StringComparison.OrdinalIgnoreCase);
}

public enum PortalNotificationPurpose { ExternalApprovalSubmitted, ExternalApprovalReviewStarted, ExternalApprovalDecisionRecorded, ExternalApprovalCancelled }
public sealed record PortalNotificationTemplateKey(string Value);
public sealed record PortalNotificationRecipientHint(string RoleOrGroup, string? MaskedRecipient = null);
public sealed record PortalNotificationRequest(PortalNotificationPurpose Purpose, PortalNotificationTemplateKey TemplateKey, IReadOnlyCollection<PortalNotificationRecipientHint> RecipientHints, IReadOnlyDictionary<string, string> Metadata, bool SendDisabled);
public sealed record PortalNotificationIntent(Guid Id, PortalNotificationPurpose Purpose, string TemplateKey, string Status, bool IsFoundationOnly, bool SendDisabled, DateTimeOffset CreatedAtUtc, string Disclaimer);
public sealed record PortalNotificationReadiness(string Status, bool IsPortalOwned, bool AllowsSend, IReadOnlyCollection<string> MissingPortalDependencies, IReadOnlyCollection<string> Blockers, string Disclaimer);

public sealed record ExternalApprovalIntegrationReadiness(
    PortalContentFileReadiness ContentFile,
    PortalNotificationReadiness Notification,
    string AuditOutboxStatus,
    IReadOnlyCollection<string> FeatureFlags,
    IReadOnlyCollection<string> MissingPortalDependencies,
    IReadOnlyCollection<string> Blockers,
    string RecommendedNextAction,
    string Disclaimer);

public static class ExternalApprovalPortalIntegrationBoundary
{
    public const string Disclaimer = "Foundation boundary only. Financiero stores metadata references and notification intents only; Portal owns Content/File and Notification.";

    public static PortalContentFileReadiness ContentFileReadiness() => new(
        "FoundationContractOnly",
        IsPortalOwned: true,
        AllowsUpload: false,
        AllowsDownload: false,
        AllowsSensitiveLinks: false,
        ["Portal Content/File production contract", "Portal evidence retention policy", "Portal secure file link policy"],
        ["No upload from Financiero", "No file download from Financiero", "No sensitive links or token URLs"],
        Disclaimer);

    public static PortalNotificationReadiness NotificationReadiness() => new(
        "FoundationIntentOnly",
        IsPortalOwned: true,
        AllowsSend: false,
        ["Portal Notification templates", "Portal recipient resolution", "Portal delivery provider"],
        ["Notification send is disabled", "No SMTP or Teams connector in Financiero", "No full recipient addresses in Financiero"],
        Disclaimer);

    public static ExternalApprovalIntegrationReadiness Readiness() => new(
        ContentFileReadiness(),
        NotificationReadiness(),
        "Audit/Outbox foundation events only; payload contains ids, scope, status and no evidence content.",
        ["allowPortalContentFileEvidenceReferences=false", "allowPortalNotificationIntents=false", "allowNotificationSend=false", "allowEvidenceUpload=false"],
        ["Portal Content/File", "Portal Notification"],
        ["Production evidence storage is blocked", "Notification delivery is blocked", "External approval does not enable tax/SRI production"],
        "Keep storing references only until Portal Content/File and Notification production contracts are implemented.",
        Disclaimer);

    public static PortalNotificationIntent BuildNotificationIntent(PortalNotificationPurpose purpose, ExternalApprovalRequest request) => new(
        Guid.NewGuid(),
        purpose,
        purpose.ToString(),
        "PreparedNotSent",
        IsFoundationOnly: true,
        SendDisabled: true,
        DateTimeOffset.UtcNow,
        "Notification intent only; Portal Notification owns future delivery.");
}
