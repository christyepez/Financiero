using Financiero.Application;
using Financiero.Domain;
using Xunit;

namespace Financiero.Application.Tests;

public sealed class ProductizationReadinessTests
{
    private static readonly PortalCallContext Context = new("default", "corr-productization", null);

    [Fact]
    public async Task Purchase_readiness_returns_blockers_by_default()
    {
        var result = await new PurchaseProductizationReadinessService(new FakePurchaseRepo(), new FakeApprovalRepo(), new RecordingAudit()).CheckAsync(Context, default);

        Assert.False(result.IsReadyForProduction);
        Assert.True(result.DoesNotEnableProduction);
        Assert.Contains(result.Blockers, x => x.Code == "purchase.external_approval.missing");
        Assert.Contains(result.DangerousFeatureFlagsBlocked, x => x.Contains("allowSriSubmission=false"));
    }

    [Fact]
    public async Task Voided_readiness_returns_blockers_by_default()
    {
        var result = await new VoidedDocumentProductizationReadinessService(new FakeVoidedRepo(), new FakeApprovalRepo(), new RecordingAudit()).CheckAsync(Context, default);

        Assert.False(result.IsReadyForProduction);
        Assert.Contains(result.Blockers, x => x.Code == "voided.external_approval.missing");
        Assert.Contains(result.Blockers, x => x.Message.Contains("Notification", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task ApprovedFoundation_does_not_enable_production()
    {
        var approval = ExternalApprovalRequest.Create("default", ExternalApprovalRequestScope.ATS, "Foundation approval", null, "Reviewer", ["Evidence"], DateTimeOffset.UtcNow);
        approval.Submit(DateTimeOffset.UtcNow, "Reviewer");
        approval.RecordDecision(ExternalApprovalDecisionKind.ApprovedFoundation, "Foundation only.", "Reviewer", DateTimeOffset.UtcNow);

        var result = await new PurchaseProductizationReadinessService(new FakePurchaseRepo(), new FakeApprovalRepo([approval]), new RecordingAudit()).CheckAsync(Context, default);

        Assert.False(result.IsReadyForProduction);
        Assert.Contains(result.Gates, x => x.Status == "ApprovedFoundationOnly");
        Assert.Contains(result.Blockers, x => x.Code == "purchase.production.blocked");
    }

    [Fact]
    public async Task Readiness_does_not_mutate_repositories()
    {
        var purchaseRepo = new FakePurchaseRepo();
        var voidedRepo = new FakeVoidedRepo();

        await new PurchaseProductizationReadinessService(purchaseRepo, new FakeApprovalRepo(), new RecordingAudit()).CheckAsync(Context, default);
        await new VoidedDocumentProductizationReadinessService(voidedRepo, new FakeApprovalRepo(), new RecordingAudit()).CheckAsync(Context, default);

        Assert.Equal(0, purchaseRepo.SaveCount);
        Assert.Equal(0, voidedRepo.SaveCount);
    }

    private sealed class FakeApprovalRepo(IReadOnlyCollection<ExternalApprovalRequest>? items = null) : IExternalApprovalRequestRepository
    {
        private readonly IReadOnlyCollection<ExternalApprovalRequest> _items = items ?? [];
        public Task AddAsync(ExternalApprovalRequest request, CancellationToken ct) => throw new NotSupportedException();
        public Task<ExternalApprovalRequest?> GetByIdAsync(Guid id, string tenantId, CancellationToken ct) => Task.FromResult(_items.FirstOrDefault(x => x.Id == id && x.TenantId == tenantId));
        public Task<IReadOnlyCollection<ExternalApprovalRequest>> ListAsync(string tenantId, ExternalApprovalRequestScope? scope, ExternalApprovalRequestStatus? status, CancellationToken ct) =>
            Task.FromResult<IReadOnlyCollection<ExternalApprovalRequest>>(_items.Where(x => x.TenantId == tenantId && (!scope.HasValue || x.Scope == scope) && (!status.HasValue || x.Status == status)).ToArray());
        public Task SaveChangesAsync(CancellationToken ct) => throw new NotSupportedException();
    }

    private sealed class FakePurchaseRepo : IPurchaseTaxDocumentRepository
    {
        public int SaveCount { get; private set; }
        public Task AddAsync(PurchaseTaxDocument document, CancellationToken ct) => throw new NotSupportedException();
        public Task AddLineAsync(PurchaseTaxDocumentLine line, CancellationToken ct) => throw new NotSupportedException();
        public Task AddTaxAsync(PurchaseTax tax, CancellationToken ct) => throw new NotSupportedException();
        public Task AddReferenceAsync(PurchaseSupportDocumentReference reference, CancellationToken ct) => throw new NotSupportedException();
        public Task<PurchaseTaxDocument?> GetByIdAsync(Guid id, string tenantId, CancellationToken ct) => Task.FromResult<PurchaseTaxDocument?>(null);
        public Task<IReadOnlyCollection<PurchaseTaxDocument>> GetByPeriodAsync(string tenantId, string period, CancellationToken ct) => Task.FromResult<IReadOnlyCollection<PurchaseTaxDocument>>([]);
        public Task<PurchaseTaxSummary> GetSummaryAsync(string tenantId, DateOnly from, DateOnly to, CancellationToken ct) => Task.FromResult(new PurchaseTaxSummary(0, 0, 0, 0));
        public Task SaveChangesAsync(CancellationToken ct) { SaveCount++; return Task.CompletedTask; }
    }

    private sealed class FakeVoidedRepo : IVoidedTaxDocumentRepository
    {
        public int SaveCount { get; private set; }
        public Task AddAsync(VoidedTaxDocument document, CancellationToken ct) => throw new NotSupportedException();
        public Task<VoidedTaxDocument?> GetByIdAsync(Guid id, string tenantId, CancellationToken ct) => Task.FromResult<VoidedTaxDocument?>(null);
        public Task<IReadOnlyCollection<VoidedTaxDocument>> GetByPeriodAsync(string tenantId, string period, CancellationToken ct) => Task.FromResult<IReadOnlyCollection<VoidedTaxDocument>>([]);
        public Task<int> CountByPeriodAsync(string tenantId, DateOnly from, DateOnly to, CancellationToken ct) => Task.FromResult(0);
        public Task<bool> ExistsDocumentNumberAsync(string tenantId, string documentType, string establishment, string emissionPoint, string sequential, Guid? excludingId, CancellationToken ct) => Task.FromResult(false);
        public Task SaveChangesAsync(CancellationToken ct) { SaveCount++; return Task.CompletedTask; }
    }
}
