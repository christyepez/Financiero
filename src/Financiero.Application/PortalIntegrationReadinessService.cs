namespace Financiero.Application;

public sealed record PortalIntegrationCapability(string Name, string Owner, string Status, string ReuseClassification, bool Required);
public sealed record PortalIntegrationReadinessResult(
    string Status,
    bool IsReadyForProduction,
    string CurrentEnvironmentMode,
    IReadOnlyCollection<PortalIntegrationCapability> RequiredCapabilities,
    IReadOnlyCollection<string> MissingCapabilities,
    IReadOnlyCollection<string> ExpectedPermissions,
    IReadOnlyCollection<string> ExpectedMenuRoutes,
    IReadOnlyCollection<string> ExpectedFeatureFlags,
    IReadOnlyCollection<string> ProductionBlockers,
    IReadOnlyCollection<string> Warnings,
    string CorrelationId,
    string Disclaimer);

public sealed class PortalIntegrationReadinessService
{
    public Task<PortalIntegrationReadinessResult> CheckAsync(PortalCallContext context, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var capabilities = new[]
        {
            new PortalIntegrationCapability("Security/Auth", "PortalCorporativo", "Required", "REUSE", true),
            new PortalIntegrationCapability("Menu", "PortalCorporativo", "Required", "EXTEND", true),
            new PortalIntegrationCapability("Configuration", "PortalCorporativo", "Required", "EXTEND", true),
            new PortalIntegrationCapability("Audit", "PortalCorporativo", "Required", "ADAPT", true),
            new PortalIntegrationCapability("Outbox", "PortalCorporativo", "Required", "ADAPT", true),
            new PortalIntegrationCapability("Content/File", "PortalCorporativo", "BoundaryReady", "ADAPT", true),
            new PortalIntegrationCapability("Notification", "PortalCorporativo", "BoundaryReady", "ADAPT", true),
            new PortalIntegrationCapability("Portal Shell Context", "PortalCorporativo", "ContractReady", "EXTEND", true)
        };

        return Task.FromResult(new PortalIntegrationReadinessResult(
            "BlockedFoundation",
            false,
            "DevelopmentStandaloneUntilPortalContextValidated",
            capabilities,
            ["Real PortalShellContext E2E evidence", "Gateway route evidence", "Shared SQL runtime evidence", "Portal Security/Menu/Configuration runtime evidence"],
            ["financial.electronicdocuments.read", "financial.electronicdocuments.manage"],
            ["/dashboard", "/sri-readiness", "/ats-readiness", "/external-approvals", "/tax-catalogs", "/purchases", "/voided-documents"],
            ["allowProductizationReadiness=true", "allowProductiveActivation=false", "allowOfficialTaxFlows=false", "allowSriSubmission=false", "allowAtsOfficialActions=false", "allowEvidenceUpload=false", "allowNotificationSend=false"],
            ["SRI Production blocked", "SRI Test real send blocked", "Official ATS blocked", "Legal-final RIDE blocked", "Productive XAdES blocked", "Evidence upload blocked", "Notification send blocked"],
            ["Standalone mode is development-only.", "Delegated auth must remain in memory.", "No tokens, secrets, claims dump or tenant-sensitive raw data are returned."],
            context.CorrelationId,
            "Portal E2E readiness is informational and read-only. It does not enable production tax flows or duplicate Portal-owned capabilities."));
    }
}
