using Financiero.Domain;
using Xunit;

namespace Financiero.Domain.Tests;

public sealed class ExternalApprovalWorkflowDomainTests
{
    [Fact]
    public void Submitted_requires_requirements()
    {
        var request = ExternalApprovalRequest.Create("default", ExternalApprovalRequestScope.ATS, "ATS foundation", "2026-07", "Reviewer", [], DateTimeOffset.UtcNow);

        var ex = Assert.Throws<FinancialDomainException>(() => request.Submit(DateTimeOffset.UtcNow, "Reviewer"));

        Assert.Equal("external_approval.requirements.required", ex.Code);
    }

    [Fact]
    public void Approved_foundation_does_not_enable_production()
    {
        var request = NewRequest();
        request.Submit(DateTimeOffset.UtcNow, "Reviewer");
        request.StartReview(DateTimeOffset.UtcNow, "Reviewer");

        request.RecordDecision(ExternalApprovalDecisionKind.ApprovedFoundation, "Foundation approval only.", "Reviewer", DateTimeOffset.UtcNow);

        Assert.Equal(ExternalApprovalRequestStatus.ApprovedFoundation, request.Status);
        Assert.True(request.DoesNotEnableProduction);
        Assert.All(request.Decisions, x => Assert.True(x.DoesNotEnableProduction));
    }

    [Fact]
    public void Rejected_foundation_requires_reason()
    {
        var request = NewRequest();
        request.Submit(DateTimeOffset.UtcNow, "Reviewer");

        var ex = Assert.Throws<FinancialDomainException>(() => request.RecordDecision(ExternalApprovalDecisionKind.RejectedFoundation, "", "Reviewer", DateTimeOffset.UtcNow));

        Assert.Equal("external_approval.decision.reason.required", ex.Code);
    }

    [Theory]
    [InlineData("<xml />")]
    [InlineData("data:application/pdf;base64,AAAA")]
    [InlineData("https://evidence.example.test/file?token=secret")]
    public void Evidence_reference_rejects_unsafe_payloads(string reference)
    {
        var request = NewRequest();

        var ex = Assert.Throws<FinancialDomainException>(() => request.AddEvidenceReference("PortalContentFile", reference, "Evidence", null, "text/plain", "Reviewer", DateTimeOffset.UtcNow));

        Assert.StartsWith("external_approval.payload.", ex.Code);
    }

    [Fact]
    public void Cancel_closes_draft_request()
    {
        var request = NewRequest();

        request.Cancel("No longer needed foundation.", DateTimeOffset.UtcNow, "Reviewer");

        Assert.Equal(ExternalApprovalRequestStatus.Cancelled, request.Status);
    }

    private static ExternalApprovalRequest NewRequest() =>
        ExternalApprovalRequest.Create("default", ExternalApprovalRequestScope.ATS, "ATS foundation", "2026-07", "Reviewer", ["External evidence package"], DateTimeOffset.UtcNow);
}
