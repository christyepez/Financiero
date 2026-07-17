using Financiero.Application;
using Financiero.Domain;
using Xunit;

namespace Financiero.Application.Tests;

public sealed class ExternalApprovalWorkflowTests
{
    private static readonly PortalCallContext Context = new("default", "corr-approval", null);

    [Fact]
    public async Task External_approvals_return_all_scopes()
    {
        var result = await Service().GetAllAsync(Context, default);

        Assert.Contains(result, x => x.Scope == ExternalApprovalScope.AtsOfficialXml);
        Assert.Contains(result, x => x.Scope == ExternalApprovalScope.RideLegalFinal);
        Assert.Contains(result, x => x.Scope == ExternalApprovalScope.XadesSignature);
        Assert.All(result, x => Assert.True(x.IsFoundationOnly));
        Assert.All(result, x => Assert.True(x.DoesNotEnableProduction));
    }

    [Theory]
    [InlineData("ats", ExternalApprovalScope.AtsOfficialXml, "Official ATS")]
    [InlineData("ride", ExternalApprovalScope.RideLegalFinal, "RIDE")]
    [InlineData("xades", ExternalApprovalScope.XadesSignature, "Certificate custody")]
    [InlineData("sri-test", ExternalApprovalScope.SriTestSubmission, "SRI Test")]
    [InlineData("portal-content-file", ExternalApprovalScope.PortalContentFileProduction, "Content/File")]
    public async Task Scope_readiness_returns_required_evidence(string scope, ExternalApprovalScope expected, string expectedText)
    {
        var result = await Service().CheckAsync(scope, Context, default);

        Assert.Equal(expected, result.Gates.Single().Scope);
        Assert.Contains(result.RequiredEvidence, x => x.Contains(expectedText, StringComparison.OrdinalIgnoreCase));
        Assert.True(result.DecisionMetadata.DoesNotEnableProduction);
        Assert.Contains("does not enable production", result.Disclaimer, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Security_and_runbook_remain_required_for_all_readiness()
    {
        var result = await Service().CheckAsync("all", Context, default);

        Assert.Contains(result.Gates, x => x.Scope == ExternalApprovalScope.SecurityProductionGate);
        Assert.Contains(result.Gates, x => x.Scope == ExternalApprovalScope.OperationalRunbook);
        Assert.Contains(result.BlockingRisks, x => x.Contains("blocked for production", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Invalid_scope_fails_clear()
    {
        var ex = await Assert.ThrowsAsync<FinancialApplicationException>(() => Service().GetAsync("invalid", Context, default));

        Assert.Equal("external_approval.scope.invalid", ex.Code);
    }

    [Fact]
    public void Content_file_reference_validator_accepts_safe_metadata_only()
    {
        var reference = new PortalContentFileReferenceValidator().Validate(new(
            "PortalContentFile",
            "portal-content-file-ref-123",
            "Evidence reference only",
            "application/pdf",
            "ABC123",
            128,
            DateTimeOffset.UtcNow,
            "Reviewer",
            ExternalApprovalContentFilePurpose.ExternalApprovalEvidence.ToString(),
            "retain-by-portal-policy",
            PortalContentFileLinkPolicy.ReferenceOnly));

        Assert.Equal("portal-content-file-ref-123", reference.ReferenceId);
        Assert.Equal(PortalContentFileLinkPolicy.ReferenceOnly, reference.LinkPolicy);
    }

    [Theory]
    [InlineData("base64:QUJD")]
    [InlineData("<factura />")]
    [InlineData("BEGIN CERTIFICATE")]
    [InlineData("PRIVATE KEY")]
    [InlineData("https://portal.test/files/1?token=secret")]
    [InlineData("C:\\temp\\evidence.pdf")]
    public void Content_file_reference_validator_rejects_unsafe_metadata(string referenceId)
    {
        var ex = Assert.ThrowsAny<Exception>(() => new PortalContentFileReferenceValidator().Validate(new(
            "PortalContentFile",
            referenceId,
            "Evidence reference only",
            "application/pdf",
            null,
            128,
            DateTimeOffset.UtcNow,
            "Reviewer",
            ExternalApprovalContentFilePurpose.ExternalApprovalEvidence.ToString(),
            null,
            PortalContentFileLinkPolicy.ReferenceOnly)));

        Assert.True(ex is FinancialApplicationException or FinancialDomainException);
    }

    [Fact]
    public void Notification_intent_is_metadata_only_and_send_disabled()
    {
        var request = ExternalApprovalRequest.Create("default", ExternalApprovalRequestScope.CONTENT_FILE, "Portal boundary", null, "Reviewer", ["Portal evidence reference"], DateTimeOffset.UtcNow);

        var intent = ExternalApprovalPortalIntegrationBoundary.BuildNotificationIntent(PortalNotificationPurpose.ExternalApprovalSubmitted, request);

        Assert.True(intent.SendDisabled);
        Assert.True(intent.IsFoundationOnly);
        Assert.Equal("PreparedNotSent", intent.Status);
    }

    [Fact]
    public void Integration_readiness_returns_foundation_blockers()
    {
        var readiness = ExternalApprovalPortalIntegrationBoundary.Readiness();

        Assert.False(readiness.ContentFile.AllowsUpload);
        Assert.False(readiness.Notification.AllowsSend);
        Assert.Contains(readiness.Blockers, x => x.Contains("blocked", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(readiness.FeatureFlags, x => x.Contains("allowEvidenceUpload=false", StringComparison.OrdinalIgnoreCase));
    }

    private static ExternalApprovalReadinessService Service() => new(new RecordingAudit());
}
