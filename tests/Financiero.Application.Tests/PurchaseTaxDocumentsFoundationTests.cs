using Financiero.Application;
using Financiero.Domain;
using Xunit;

namespace Financiero.Application.Tests;

public sealed class PurchaseTaxDocumentsFoundationTests
{
    private static PortalCallContext Context => new("default", "corr-purchases");

    [Fact]
    public async Task Creates_valid_purchase_and_masks_supplier_identification()
    {
        var repo = new InMemoryPurchaseTaxDocumentRepository();
        var service = new PurchaseTaxDocumentService(repo, new RecordingAudit(), new RecordingOutbox());

        var created = await service.CreateAsync(ValidPurchase(), Context, default);
        await service.AddLineAsync(created.Id, new("SKU", "Compra foundation", 1, 100, 0), Context, default);
        await service.AddTaxAsync(created.Id, new("2", "2", 100, 12, 12), Context, default);
        var validated = await service.ValidateAsync(created.Id, Context, default);

        Assert.Equal("Validated", validated.Status);
        Assert.Equal(112, validated.Total);
        Assert.NotEqual("0999999999001", validated.SupplierIdentificationMasked);
        Assert.Contains("foundation only", validated.Disclaimer);
    }

    [Fact]
    public void Purchase_rejects_future_date_invalid_period_and_negative_totals()
    {
        Assert.Throws<FinancialDomainException>(() => PurchaseTaxDocument.Create("default", "04", "0999999999001", "Proveedor", "001", "001", "000000001", "01", DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)), DateOnly.FromDateTime(DateTime.UtcNow), "2026-01", "01", 0, 0, 0, "USD", null, null, DateTimeOffset.UtcNow));
        Assert.Throws<FinancialDomainException>(() => PurchaseTaxDocument.Create("default", "04", "0999999999001", "Proveedor", "001", "001", "000000001", "01", new(2026, 1, 1), new(2026, 1, 2), "01/2026", "01", 0, 0, 0, "USD", null, null, DateTimeOffset.UtcNow));
        Assert.Throws<FinancialDomainException>(() => PurchaseTaxDocument.Create("default", "04", "0999999999001", "Proveedor", "001", "001", "000000001", "01", new(2026, 1, 1), new(2026, 1, 2), "2026-01", "01", -1, 0, 0, "USD", null, null, DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Purchase_validation_checks_tax_catalog_and_total_tolerance()
    {
        var document = PurchaseTaxDocument.Create("default", "04", "0999999999001", "Proveedor", "001", "001", "000000001", "01", new(2026, 1, 1), new(2026, 1, 2), "2026-01", "01", 100, 12, 111, "USD", null, null, DateTimeOffset.UtcNow);
        document.AddLine("SKU", "Compra", 1, 100, 0, DateTimeOffset.UtcNow);
        Assert.Throws<FinancialDomainException>(() => document.AddTax("99", "2", 100, 12, 12, DateTimeOffset.UtcNow));
        document.AddTax("2", "2", 100, 12, 12, DateTimeOffset.UtcNow);
        Assert.Throws<FinancialDomainException>(() => document.ValidateFoundation(DateTimeOffset.UtcNow));
    }

    [Fact]
    public async Task Registers_voided_document_and_rejects_invalid_or_duplicate_voids()
    {
        var repo = new InMemoryVoidedTaxDocumentRepository();
        var service = new VoidedTaxDocumentService(repo, new RecordingAudit(), new RecordingOutbox());

        var registered = await service.RegisterAsync(new("01", "001", "001", "000000009", new(2026, 1, 1), new(2026, 1, 2), "2026-01", "Synthetic void foundation"), Context, default);

        Assert.Equal("RegisteredFoundation", registered.Status);
        Assert.Contains("foundation only", registered.Disclaimer);
        await Assert.ThrowsAsync<FinancialApplicationException>(() => service.RegisterAsync(new("01", "001", "001", "000000009", new(2026, 1, 1), new(2026, 1, 2), "2026-01", "Duplicate"), Context, default));
        Assert.Throws<FinancialDomainException>(() => VoidedTaxDocument.RegisterFoundation("default", "01", "001", "001", "000000010", new(2026, 1, 3), new(2026, 1, 2), "2026-01", "Before issue", null, null, null, DateTimeOffset.UtcNow));
        Assert.Throws<FinancialDomainException>(() => VoidedTaxDocument.RegisterFoundation("default", "01", "001", "001", "000000011", new(2026, 1, 1), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)), "2026-01", "Future void", null, null, null, DateTimeOffset.UtcNow));
    }

    [Fact]
    public async Task Ats_readiness_uses_purchase_and_voided_foundation_counts()
    {
        var electronicRepo = new InMemoryElectronicDocumentRepository();
        var purchases = new InMemoryPurchaseTaxDocumentRepository();
        var voided = new InMemoryVoidedTaxDocumentRepository();
        await purchases.AddAsync(PurchaseTaxDocument.Create("default", "04", "0999999999001", "Proveedor", "001", "001", "000000001", "01", new(2026, 3, 1), new(2026, 3, 2), "2026-03", "01", 100, 12, 112, "USD", null, null, DateTimeOffset.UtcNow), default);
        await voided.AddAsync(VoidedTaxDocument.RegisterFoundation("default", "01", "001", "001", "000000002", new(2026, 3, 1), new(2026, 3, 2), "2026-03", "Synthetic void", null, null, null, DateTimeOffset.UtcNow), default);
        var export = new TaxExportService(new TaxReportingService(electronicRepo, new RecordingAudit()), electronicRepo, new DevelopmentElectronicDocumentStorageClient(), new RecordingAudit(), purchases, voided);

        var readiness = await export.EvaluateAtsReadinessAsync(new("2026-03"), Context, default);
        var design = await export.EvaluateAtsOfficialDesignAsync(new("2026-03"), Context, default);
        var gaps = await new AtsOfficialGapAnalysisService(new RecordingAudit(), purchases, voided).AnalyzeAsync(new("2026-03"), Context, default);

        Assert.Equal(1, readiness.Purchases.Count);
        Assert.Equal(1, readiness.Voided!.Count);
        Assert.Contains(design.Sections, x => x.Code == "purchases" && x.Status == "ReadyFoundation");
        Assert.Contains(design.Sections, x => x.Code == "voided" && x.Status == "ReadyFoundation");
        Assert.DoesNotContain(gaps.Gaps, x => x.Code == "ats.purchases.module_missing");
        Assert.DoesNotContain(gaps.Gaps, x => x.Code == "ats.voided_documents.model_missing");
        Assert.Contains("not an official ATS", design.Disclaimer);
    }

    private static CreatePurchaseTaxDocumentRequest ValidPurchase() => new("04", "0999999999001", "Proveedor", "001", "001", "000000001", "01", new(2026, 1, 1), new(2026, 1, 2), "2026-01", "01", 100, 12, 112);
}

internal sealed class InMemoryPurchaseTaxDocumentRepository : IPurchaseTaxDocumentRepository
{
    private readonly List<PurchaseTaxDocument> _items = [];
    public Task AddAsync(PurchaseTaxDocument document, CancellationToken ct) { _items.Add(document); return Task.CompletedTask; }
    public Task AddLineAsync(PurchaseTaxDocumentLine line, CancellationToken ct) => Task.CompletedTask;
    public Task AddTaxAsync(PurchaseTax tax, CancellationToken ct) => Task.CompletedTask;
    public Task AddReferenceAsync(PurchaseSupportDocumentReference reference, CancellationToken ct) => Task.CompletedTask;
    public Task<PurchaseTaxDocument?> GetByIdAsync(Guid id, string tenantId, CancellationToken ct) => Task.FromResult(_items.FirstOrDefault(x => x.Id == id && x.TenantId == tenantId));
    public Task<IReadOnlyCollection<PurchaseTaxDocument>> GetByPeriodAsync(string tenantId, string period, CancellationToken ct) => Task.FromResult<IReadOnlyCollection<PurchaseTaxDocument>>(_items.Where(x => x.TenantId == tenantId && x.FiscalPeriod == period).ToArray());
    public Task<PurchaseTaxSummary> GetSummaryAsync(string tenantId, DateOnly from, DateOnly to, CancellationToken ct)
    {
        var items = _items.Where(x => x.TenantId == tenantId && x.IssueDate >= from && x.IssueDate <= to).ToArray();
        return Task.FromResult(new PurchaseTaxSummary(items.Length, items.Sum(x => x.Subtotal), items.Sum(x => x.TaxTotal), items.Sum(x => x.Total)));
    }
    public Task SaveChangesAsync(CancellationToken ct) => Task.CompletedTask;
}

internal sealed class InMemoryVoidedTaxDocumentRepository : IVoidedTaxDocumentRepository
{
    private readonly List<VoidedTaxDocument> _items = [];
    public Task AddAsync(VoidedTaxDocument document, CancellationToken ct) { _items.Add(document); return Task.CompletedTask; }
    public Task<VoidedTaxDocument?> GetByIdAsync(Guid id, string tenantId, CancellationToken ct) => Task.FromResult(_items.FirstOrDefault(x => x.Id == id && x.TenantId == tenantId));
    public Task<IReadOnlyCollection<VoidedTaxDocument>> GetByPeriodAsync(string tenantId, string period, CancellationToken ct) => Task.FromResult<IReadOnlyCollection<VoidedTaxDocument>>(_items.Where(x => x.TenantId == tenantId && x.FiscalPeriod == period).ToArray());
    public Task<int> CountByPeriodAsync(string tenantId, DateOnly from, DateOnly to, CancellationToken ct) => Task.FromResult(_items.Count(x => x.TenantId == tenantId && x.VoidDate >= from && x.VoidDate <= to));
    public Task<bool> ExistsDocumentNumberAsync(string tenantId, string documentType, string establishment, string emissionPoint, string sequential, Guid? excludingId, CancellationToken ct) => Task.FromResult(_items.Any(x => x.TenantId == tenantId && x.DocumentType == documentType && x.Establishment == establishment && x.EmissionPoint == emissionPoint && x.Sequential == sequential && (!excludingId.HasValue || x.Id != excludingId.Value)));
    public Task SaveChangesAsync(CancellationToken ct) => Task.CompletedTask;
}
