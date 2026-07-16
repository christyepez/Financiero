using Financiero.Application;
using Financiero.Domain;
using Xunit;

namespace Financiero.Application.Tests;

public sealed class AtsXmlFoundationTests
{
    private static readonly PortalCallContext Context = new("default", "corr-ats-xml", null);

    [Fact]
    public async Task Readiness_is_disabled_by_default()
    {
        var (_, validator) = Services(new StaticFinancialConfigurationReader());

        var result = await validator.CheckAsync("2026-03", "Development", Context, default);

        Assert.False(result.CanGenerateFoundation);
        Assert.Equal(AtsXmlGenerationStatus.DisabledByConfiguration, result.Status);
        Assert.Contains(result.BlockedReasons, x => x.Code == "ats.xml.disabled");
    }

    [Fact]
    public async Task Readiness_is_blocked_in_production()
    {
        var (_, validator) = Services(Config(("financial.sri.atsXmlFoundation.enabled", "true"), ("financial.sri.atsXmlFoundation.allowXmlPreview", "true")));

        var result = await validator.CheckAsync("2026-03", "Production", Context, default);

        Assert.False(result.CanGenerateFoundation);
        Assert.Contains(result.BlockedReasons, x => x.Code == "ats.xml.production.blocked");
    }

    [Fact]
    public async Task Missing_acknowledgements_block_generation()
    {
        var (generator, _) = Services(Config(("financial.sri.atsXmlFoundation.enabled", "true")));

        var ex = await Assert.ThrowsAsync<FinancialApplicationException>(() => generator.GeneratePreviewAsync(new("2026-03", true, false, true, true), "Development", Context, default));

        Assert.Equal("ats.xml.acknowledgement.required", ex.Code);
    }

    [Fact]
    public async Task Generate_preview_disabled_returns_blocked_without_xml()
    {
        var (generator, _) = Services(new StaticFinancialConfigurationReader());

        var result = await generator.GeneratePreviewAsync(Acknowledged(includeXml: true), "Development", Context, default);

        Assert.Equal(AtsXmlGenerationStatus.BlockedByReadiness, result.Status);
        Assert.Null(result.XmlContent);
        Assert.Null(result.XmlHash);
    }

    [Fact]
    public async Task Generate_preview_without_xml_gate_returns_metadata_only()
    {
        var (generator, _) = Services(Config(("financial.sri.atsXmlFoundation.enabled", "true"), ("financial.sri.atsXmlFoundation.allowXmlPreview", "false")));

        var result = await generator.GeneratePreviewAsync(Acknowledged(includeXml: true), "Development", Context, default);

        Assert.Null(result.XmlContent);
        Assert.Null(result.XmlHash);
        Assert.False(result.IsOfficial);
        Assert.True(result.IsFoundationOnly);
        Assert.Equal("2026-07-sprint-5-p3-foundation", result.CatalogVersion);
    }

    [Fact]
    public async Task Generate_preview_with_gate_returns_foundation_xml_and_hash()
    {
        var (generator, _) = Services(Config(("financial.sri.atsXmlFoundation.enabled", "true"), ("financial.sri.atsXmlFoundation.allowXmlPreview", "true")));

        var result = await generator.GeneratePreviewAsync(Acknowledged(includeXml: true), "Development", Context, default);

        Assert.Equal(AtsXmlGenerationStatus.GeneratedFoundation, result.Status);
        Assert.Contains("atsFoundationPreview", result.XmlContent);
        Assert.Contains("foundationOnly=\"true\"", result.XmlContent);
        Assert.NotNull(result.XmlHash);
        Assert.DoesNotContain("0999999999001", result.XmlContent);
        Assert.Contains("not official", result.Disclaimer, StringComparison.OrdinalIgnoreCase);
    }

    private static AtsXmlGenerationRequest Acknowledged(bool includeXml) => new("2026-03", includeXml, true, true, true);

    private static (IAtsXmlFoundationGenerator Generator, IAtsXmlReadinessValidator Validator) Services(IFinancialConfigurationReader config)
    {
        var purchases = new InMemoryPurchaseTaxDocumentRepository();
        var purchase = PurchaseTaxDocument.Create("default", "04", "0999999999001", "Proveedor", "001", "001", "000000001", "01", new(2026, 3, 1), new(2026, 3, 2), "2026-03", "01", 100, 12, 112, "USD", null, null, DateTimeOffset.UtcNow);
        purchase.AddLine("SKU", "Compra", 1, 100, 0, DateTimeOffset.UtcNow);
        purchase.AddTax("2", "2", 100, 12, 12, DateTimeOffset.UtcNow);
        purchases.AddAsync(purchase, default).GetAwaiter().GetResult();
        var voided = new InMemoryVoidedTaxDocumentRepository();
        var electronic = new InMemoryElectronicDocumentRepository();
        var audit = new RecordingAudit();
        var catalog = new FinancialTaxCatalogService(new FoundationFinancialTaxCatalogProvider(), audit);
        var mapping = new AtsSupportMappingService(purchases, voided, audit, catalog);
        var validator = new AtsXmlReadinessValidator(config, mapping, catalog, audit);
        var generator = new AtsXmlFoundationGenerator(config, purchases, voided, electronic, validator, catalog, audit);
        return (generator, validator);
    }

    private static StaticFinancialConfigurationReader Config(params (string Key, string Value)[] values) =>
        new(values.ToDictionary(x => x.Key, x => x.Value));
}
