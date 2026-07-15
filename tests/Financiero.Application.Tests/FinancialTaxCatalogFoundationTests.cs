using Financiero.Application;
using Financiero.Domain;
using Xunit;

namespace Financiero.Application.Tests;

public sealed class FinancialTaxCatalogFoundationTests
{
    private static readonly PortalCallContext Context = new("default", "corr-tax-catalog", null);

    [Fact]
    public async Task Catalog_service_returns_version_and_foundation_items()
    {
        var all = await Catalog().GetAllAsync(Context, default);

        Assert.Equal("2026-07-sprint-5-p3-foundation", all.Version);
        Assert.Contains(all.PurchaseDocumentTypes, x => x.Code == "01" && x.IsFoundationOnly);
        Assert.Contains(all.SupportDocumentTypes, x => x.Code == "01" && x.Name.Contains("Bienes"));
        Assert.Contains(all.SupportDocumentTypes, x => x.Code == "02" && x.Name.Contains("Servicios"));
        Assert.Contains(all.VoidedDocumentTypes, x => x.Code == "01");
        Assert.Contains(all.PurchaseTaxCodes, x => x.Code == "2");
        Assert.Contains(all.SupplierIdentificationTypes, x => x.Code == "04");
        Assert.Contains("not an official", all.Disclaimer, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Purchase_validation_reports_foundation_and_tax_review()
    {
        var purchase = PurchaseTaxDocument.Create("default", "04", "0999999999001", "Proveedor", "001", "001", "000000001", "01", new(2026, 1, 1), new(2026, 1, 2), "2026-01", "01", 100, 12, 112, "USD", null, null, DateTimeOffset.UtcNow);
        purchase.AddLine("SKU", "Compra", 1, 100, 0, DateTimeOffset.UtcNow);
        purchase.AddTax("2", "2", 100, 12, 12, DateTimeOffset.UtcNow);

        var result = Catalog().ValidatePurchase(purchase);

        Assert.Equal("2026-07-sprint-5-p3-foundation", result.CatalogVersion);
        Assert.Contains(result.Issues, x => x.Code == "purchase.catalog.foundation_only");
        Assert.Contains(result.Issues, x => x.Code == "purchase.catalog.tax_review.required");
    }

    [Fact]
    public void Purchase_domain_rejects_unsupported_document_type()
    {
        var ex = Assert.Throws<FinancialDomainException>(() =>
            PurchaseTaxDocument.Create("default", "04", "0999999999001", "Proveedor", "001", "001", "000000001", "99", new(2026, 1, 1), new(2026, 1, 2), "2026-01", "01", 100, 12, 112, "USD", null, null, DateTimeOffset.UtcNow));

        Assert.Equal("purchase.document_type.invalid", ex.Code);
    }

    [Fact]
    public void Voided_domain_rejects_unsupported_document_type()
    {
        var ex = Assert.Throws<FinancialDomainException>(() =>
            VoidedTaxDocument.RegisterFoundation("default", "99", "001", "001", "000000001", new(2026, 1, 1), new(2026, 1, 2), "2026-01", "Synthetic", null, null, null, DateTimeOffset.UtcNow));

        Assert.Equal("voided.document_type.invalid", ex.Code);
    }

    [Fact]
    public async Task Ats_readiness_and_mapping_include_catalog_version_and_issues()
    {
        var purchases = new InMemoryPurchaseTaxDocumentRepository();
        var purchase = PurchaseTaxDocument.Create("default", "04", "0999999999001", "Proveedor", "001", "001", "000000001", "01", new(2026, 3, 1), new(2026, 3, 2), "2026-03", "01", 100, 12, 112, "USD", null, null, DateTimeOffset.UtcNow);
        purchase.AddLine("SKU", "Compra", 1, 100, 0, DateTimeOffset.UtcNow);
        purchase.AddTax("2", "2", 100, 12, 12, DateTimeOffset.UtcNow);
        await purchases.AddAsync(purchase, default);
        var voided = new InMemoryVoidedTaxDocumentRepository();
        var catalog = Catalog();
        var mapping = new AtsSupportMappingService(purchases, voided, new RecordingAudit(), catalog);

        var mapped = await mapping.MapPurchaseAsync(purchase.Id, Context, default);
        var export = new TaxExportService(new TaxReportingService(new InMemoryElectronicDocumentRepository(), new RecordingAudit()), new InMemoryElectronicDocumentRepository(), new DevelopmentElectronicDocumentStorageClient(), new RecordingAudit(), purchases, voided, mapping);
        var readiness = await export.EvaluateAtsReadinessAsync(new("2026-03"), Context, default);

        Assert.Equal("2026-07-sprint-5-p3-foundation", mapped.Mapping.CatalogVersion);
        Assert.Contains(mapped.Mapping.Issues, x => x.Code == "purchase.catalog.tax_review.required");
        Assert.Equal("2026-07-sprint-5-p3-foundation", readiness.CatalogVersion);
        Assert.NotNull(readiness.Sections);
    }

    private static FinancialTaxCatalogService Catalog() => new(new FoundationFinancialTaxCatalogProvider(), new RecordingAudit());
}
