using Financiero.Application;
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

    private static ExternalApprovalReadinessService Service() => new(new RecordingAudit());
}
