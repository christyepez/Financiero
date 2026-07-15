using Financiero.Application;
using Financiero.Domain;
using Xunit;

namespace Financiero.Application.Tests;

public sealed class AtsSupportMappingFoundationTests
{
    private static readonly PortalCallContext Context = new("default", "corr-ats-map", null);

    [Theory]
    [InlineData("01", "01", "PurchaseInvoice")]
    [InlineData("04", "04", "PurchaseCreditNote")]
    [InlineData("05", "05", "PurchaseDebitNote")]
    public async Task Maps_purchase_support_documents_as_foundation(string documentType, string supportType, string fieldGroup)
    {
        var purchases = new InMemoryPurchaseTaxDocumentRepository();
        var purchase = Purchase(documentType, supportType, authorization: "1234567890");
        purchase.AddLine("SKU", "Compra", 1, 100, 0, DateTimeOffset.UtcNow);
        purchase.AddTax("2", "2", 100, 12, 12, DateTimeOffset.UtcNow);
        await purchases.AddAsync(purchase, default);
        var service = new AtsSupportMappingService(purchases, new InMemoryVoidedTaxDocumentRepository(), new RecordingAudit());

        var result = await service.MapPurchaseAsync(purchase.Id, Context, default);

        Assert.Equal("Purchases", result.Mapping.AtsSection);
        Assert.Equal(fieldGroup, result.Mapping.AtsFieldGroup);
        Assert.Equal(SupportDocumentMappingStatus.RequiresTaxReview, result.Mapping.Status);
        Assert.Contains(result.Mapping.Issues, x => x.Code == "support.mapping.tax_review.required");
        Assert.Contains("***", result.SupplierIdentificationMasked);
        Assert.DoesNotContain("0999999999001", System.Text.Json.JsonSerializer.Serialize(result));
        Assert.Contains("not official ATS", result.Disclaimer);
    }

    [Fact]
    public async Task Purchase_mapping_reports_required_access_key_and_authorization()
    {
        var purchases = new InMemoryPurchaseTaxDocumentRepository();
        var purchase = Purchase("01", "04", authorization: null, accessKey: null);
        purchase.AddLine("SKU", "Liquidación", 1, 100, 0, DateTimeOffset.UtcNow);
        await purchases.AddAsync(purchase, default);
        var service = new AtsSupportMappingService(purchases, new InMemoryVoidedTaxDocumentRepository(), new RecordingAudit());

        var result = await service.MapPurchaseAsync(purchase.Id, Context, default);

        Assert.Equal(SupportDocumentMappingStatus.MissingRequiredData, result.Mapping.Status);
        Assert.Contains(result.Mapping.Issues, x => x.Code == "purchase.mapping.access_key.required");
        Assert.Contains(result.Mapping.Issues, x => x.Code == "purchase.mapping.authorization.required");
        Assert.Contains(result.Mapping.Issues, x => x.Code == "purchase.mapping.tax_breakdown.required");
    }

    [Fact]
    public async Task Maps_voided_document_as_foundation()
    {
        var voided = new InMemoryVoidedTaxDocumentRepository();
        var document = VoidedTaxDocument.RegisterFoundation("default", "01", "001", "001", "000000009", new(2026, 3, 1), new(2026, 3, 2), "2026-03", "Synthetic void", null, "1234567890", "1503202601999999999999910010010000000091234567811", DateTimeOffset.UtcNow);
        await voided.AddAsync(document, default);
        var service = new AtsSupportMappingService(new InMemoryPurchaseTaxDocumentRepository(), voided, new RecordingAudit());

        var result = await service.MapVoidedAsync(document.Id, Context, default);

        Assert.Equal("VoidedDocuments", result.Mapping.AtsSection);
        Assert.Equal(SupportDocumentMappingStatus.RequiresTaxReview, result.Mapping.Status);
        Assert.Contains("***", result.AccessKeyMasked);
        Assert.DoesNotContain(document.AccessKey!, System.Text.Json.JsonSerializer.Serialize(result));
    }

    [Fact]
    public async Task Ats_section_readiness_includes_purchases_voided_and_disclaimer()
    {
        var purchases = new InMemoryPurchaseTaxDocumentRepository();
        var voided = new InMemoryVoidedTaxDocumentRepository();
        var purchase = Purchase("01", "01", authorization: "1234567890");
        purchase.AddLine("SKU", "Compra", 1, 100, 0, DateTimeOffset.UtcNow);
        purchase.AddTax("2", "2", 100, 12, 12, DateTimeOffset.UtcNow);
        await purchases.AddAsync(purchase, default);
        await voided.AddAsync(VoidedTaxDocument.RegisterFoundation("default", "01", "001", "001", "000000010", new(2026, 3, 1), new(2026, 3, 2), "2026-03", "Synthetic void", null, null, null, DateTimeOffset.UtcNow), default);
        var service = new AtsSupportMappingService(purchases, voided, new RecordingAudit());

        var readiness = await service.GetSectionReadinessAsync("2026-03", Context, default);

        Assert.Contains(readiness.Sections, x => x.Section == "Purchases" && x.MappedCount == 1);
        Assert.Contains(readiness.Sections, x => x.Section == "VoidedDocuments" && x.MappedCount == 1);
        Assert.Contains("not official ATS", readiness.Disclaimer);
    }

    [Fact]
    public async Task Ats_readiness_and_gaps_include_section_mapping_detail()
    {
        var purchases = new InMemoryPurchaseTaxDocumentRepository();
        var voided = new InMemoryVoidedTaxDocumentRepository();
        var purchase = Purchase("01", "04", authorization: null, accessKey: null);
        await purchases.AddAsync(purchase, default);
        var mapping = new AtsSupportMappingService(purchases, voided, new RecordingAudit());
        var electronicRepo = new InMemoryElectronicDocumentRepository();
        var export = new TaxExportService(new TaxReportingService(electronicRepo, new RecordingAudit()), electronicRepo, new DevelopmentElectronicDocumentStorageClient(), new RecordingAudit(), purchases, voided, mapping);

        var readiness = await export.EvaluateAtsReadinessAsync(new("2026-03"), Context, default);
        var gaps = await new AtsOfficialGapAnalysisService(new RecordingAudit(), purchases, voided, mapping).AnalyzeAsync(new("2026-03"), Context, default);

        Assert.NotNull(readiness.Sections);
        Assert.Contains(readiness.Issues, x => x.Code.Contains("ats.section.purchases"));
        Assert.Contains(gaps.Gaps, x => x.Code == "ats.support_mapping.section_issues");
    }

    [Fact]
    public async Task Lists_support_document_mapping_rules_as_foundation()
    {
        var service = new AtsSupportMappingService(new InMemoryPurchaseTaxDocumentRepository(), new InMemoryVoidedTaxDocumentRepository(), new RecordingAudit());

        var rules = await service.GetMappingsAsync(Context, default);

        Assert.Contains(rules, x => x.AtsFieldGroup == "PurchaseInvoice");
        Assert.All(rules, x => Assert.True(x.IsFoundationOnly));
    }

    private static PurchaseTaxDocument Purchase(string documentType, string supportType, string? authorization = "1234567890", string? accessKey = null) =>
        PurchaseTaxDocument.Create("default", "04", "0999999999001", "Proveedor", "001", "001", "000000001", documentType, new(2026, 3, 1), new(2026, 3, 2), "2026-03", supportType, 100, 12, 112, "USD", authorization, accessKey, DateTimeOffset.UtcNow);
}
