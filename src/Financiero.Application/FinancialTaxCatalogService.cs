using Financiero.Domain;

namespace Financiero.Application;

public sealed record FinancialTaxCatalogVersion(string Version, string Disclaimer, bool IsFoundationOnly, bool RequiresTaxReview);
public sealed record FinancialTaxCatalogItem(string Code, string Name, string Description, bool AppliesToPurchase, bool AppliesToVoided, bool AppliesToAts, bool RequiresAuthorization, bool RequiresAccessKey, bool RequiresTaxBreakdown, bool RequiresSupportReference, bool IsFoundationOnly, bool RequiresTaxReview, DateOnly? EffectiveFrom, DateOnly? EffectiveTo, string Version);
public sealed record FinancialTaxCatalogResponse(string Version, string Disclaimer, IReadOnlyCollection<FinancialTaxCatalogItem> Items);
public sealed record FinancialTaxCatalogSummary(string Version, string Disclaimer, IReadOnlyCollection<FinancialTaxCatalogItem> PurchaseDocumentTypes, IReadOnlyCollection<FinancialTaxCatalogItem> SupportDocumentTypes, IReadOnlyCollection<FinancialTaxCatalogItem> VoidedDocumentTypes, IReadOnlyCollection<FinancialTaxCatalogItem> PurchaseTaxCodes, IReadOnlyCollection<FinancialTaxCatalogItem> SupplierIdentificationTypes);
public sealed record FinancialTaxCatalogIssue(string Code, string Message, string Severity, string Field, string? Value);
public sealed record FinancialTaxCatalogValidationResult(string CatalogVersion, IReadOnlyCollection<FinancialTaxCatalogIssue> Issues, string Disclaimer);

public interface IFinancialTaxCatalogProvider
{
    FinancialTaxCatalogVersion Version { get; }
    IReadOnlyCollection<FinancialTaxCatalogItem> PurchaseDocumentTypes { get; }
    IReadOnlyCollection<FinancialTaxCatalogItem> SupportDocumentTypes { get; }
    IReadOnlyCollection<FinancialTaxCatalogItem> VoidedDocumentTypes { get; }
    IReadOnlyCollection<FinancialTaxCatalogItem> PurchaseTaxCodes { get; }
    IReadOnlyCollection<FinancialTaxCatalogItem> SupplierIdentificationTypes { get; }
}

public sealed class FoundationFinancialTaxCatalogProvider : IFinancialTaxCatalogProvider
{
    public const string CatalogVersion = "2026-07-sprint-5-p3-foundation";
    private const string Disclaimer = "Foundation tax catalog only. It is not an official final SRI/ATS catalog and requires Ecuador tax review.";
    public FinancialTaxCatalogVersion Version { get; } = new(CatalogVersion, Disclaimer, true, true);

    public IReadOnlyCollection<FinancialTaxCatalogItem> PurchaseDocumentTypes { get; } =
    [
        Item("01", "Factura compra", "Purchase invoice foundation.", true, true, true, true, false, true, false),
        Item("04", "Nota de crédito compra", "Purchase credit note foundation.", true, true, true, true, false, true, true),
        Item("05", "Nota de débito compra", "Purchase debit note foundation.", true, true, true, true, false, true, true),
        Item("03", "Liquidación de compra placeholder", "Purchase liquidation placeholder pending review.", true, false, true, true, true, true, false),
        Item("07", "Comprobante de retención relacionado", "Related withholding support placeholder.", true, true, true, true, false, true, true)
    ];

    public IReadOnlyCollection<FinancialTaxCatalogItem> SupportDocumentTypes { get; } =
    [
        Item("01", "Bienes", "Goods support foundation.", true, false, true, true, false, true, false),
        Item("02", "Servicios", "Services support foundation.", true, false, true, true, false, true, false),
        Item("03", "Activos fijos", "Fixed assets support foundation.", true, false, true, true, false, true, false),
        Item("04", "Gastos reembolsables placeholder", "Reimbursable expenses placeholder pending review.", true, false, true, true, true, true, true),
        Item("05", "Importaciones placeholder", "Imports placeholder pending review.", true, false, true, true, true, true, true)
    ];

    public IReadOnlyCollection<FinancialTaxCatalogItem> VoidedDocumentTypes { get; } =
    [
        Item("01", "Factura anulable", "Voidable invoice foundation.", false, true, true, false, false, false, false),
        Item("04", "Nota de crédito anulable", "Voidable credit note foundation.", false, true, true, false, false, false, false),
        Item("05", "Nota de débito anulable", "Voidable debit note foundation.", false, true, true, false, false, false, false),
        Item("07", "Retención anulable", "Voidable withholding foundation.", false, true, true, false, false, false, false)
    ];

    public IReadOnlyCollection<FinancialTaxCatalogItem> PurchaseTaxCodes { get; } =
    [
        Item("2", "IVA foundation", "VAT foundation for purchase taxes.", true, false, true, false, false, true, false),
        Item("3", "ICE placeholder", "ICE placeholder pending review.", true, false, true, false, false, true, false),
        Item("5", "IRBPNR placeholder", "IRBPNR placeholder pending review.", true, false, true, false, false, true, false),
        Item("6", "Retenciones relacionadas placeholder", "Related withholding tax placeholder pending review.", true, false, true, false, false, true, true)
    ];

    public IReadOnlyCollection<FinancialTaxCatalogItem> SupplierIdentificationTypes { get; } =
    [
        Item("04", "RUC", "Supplier RUC foundation.", true, false, true, false, false, false, false),
        Item("05", "Cédula", "Supplier cedula foundation.", true, false, true, false, false, false, false),
        Item("06", "Pasaporte", "Supplier passport foundation.", true, false, true, false, false, false, false),
        Item("07", "Consumidor final placeholder", "Final consumer placeholder pending review.", true, false, true, false, false, false, false)
    ];

    private static FinancialTaxCatalogItem Item(string code, string name, string description, bool purchase, bool voided, bool ats, bool auth, bool accessKey, bool taxes, bool support) =>
        new(code, name, description, purchase, voided, ats, auth, accessKey, taxes, support, true, true, null, null, CatalogVersion);
}

public sealed class FinancialTaxCatalogService(IFinancialTaxCatalogProvider provider, IPortalAuditClient audit)
{
    public async Task<FinancialTaxCatalogSummary> GetAllAsync(PortalCallContext context, CancellationToken ct)
    {
        await audit.RecordAsync(new("TaxCatalogsQueried", "financial.tax-catalog", context.TenantId, new { provider.Version.Version }), context, ct);
        return new(provider.Version.Version, provider.Version.Disclaimer, provider.PurchaseDocumentTypes, provider.SupportDocumentTypes, provider.VoidedDocumentTypes, provider.PurchaseTaxCodes, provider.SupplierIdentificationTypes);
    }

    public async Task<FinancialTaxCatalogResponse> GetPurchaseDocumentTypesAsync(PortalCallContext context, CancellationToken ct) => await ResponseAsync(provider.PurchaseDocumentTypes, context, ct);
    public async Task<FinancialTaxCatalogResponse> GetSupportDocumentTypesAsync(PortalCallContext context, CancellationToken ct) => await ResponseAsync(provider.SupportDocumentTypes, context, ct);
    public async Task<FinancialTaxCatalogResponse> GetVoidedDocumentTypesAsync(PortalCallContext context, CancellationToken ct) => await ResponseAsync(provider.VoidedDocumentTypes, context, ct);
    public async Task<FinancialTaxCatalogResponse> GetPurchaseTaxCodesAsync(PortalCallContext context, CancellationToken ct) => await ResponseAsync(provider.PurchaseTaxCodes, context, ct);
    public async Task<FinancialTaxCatalogResponse> GetSupplierIdentificationTypesAsync(PortalCallContext context, CancellationToken ct) => await ResponseAsync(provider.SupplierIdentificationTypes, context, ct);

    public FinancialTaxCatalogValidationResult ValidatePurchase(PurchaseTaxDocument document)
    {
        var issues = new List<FinancialTaxCatalogIssue>();
        AddLookup(issues, provider.PurchaseDocumentTypes, document.DocumentType, "purchase.catalog.document_type.unsupported", "Unsupported purchase document type.", "documentType");
        AddLookup(issues, provider.SupportDocumentTypes, document.SupportDocumentType, "purchase.catalog.support_type.unsupported", "Unsupported purchase support document type.", "supportDocumentType");
        AddLookup(issues, provider.SupplierIdentificationTypes, document.SupplierIdentificationType, "purchase.catalog.supplier_identification.unsupported", "Unsupported supplier identification type.", "supplierIdentificationType");
        foreach (var tax in document.Taxes) AddLookup(issues, provider.PurchaseTaxCodes, tax.TaxCode, "purchase.catalog.tax_code.unsupported", "Unsupported purchase tax code.", "taxCode");
        AddRuleIssues(issues, provider.PurchaseDocumentTypes.FirstOrDefault(x => x.Code == document.DocumentType), document.AuthorizationNumber, document.AccessKey, document.Taxes.Count, "purchase");
        return new(provider.Version.Version, issues, provider.Version.Disclaimer);
    }

    public FinancialTaxCatalogValidationResult ValidateVoided(VoidedTaxDocument document)
    {
        var issues = new List<FinancialTaxCatalogIssue>();
        AddLookup(issues, provider.VoidedDocumentTypes, document.DocumentType, "voided.catalog.document_type.unsupported", "Unsupported voided document type.", "documentType");
        var item = provider.VoidedDocumentTypes.FirstOrDefault(x => x.Code == document.DocumentType);
        if (item is not null) AddFoundationWarnings(issues, item, "voided", "documentType");
        return new(provider.Version.Version, issues, provider.Version.Disclaimer);
    }

    public bool IsPurchaseDocumentTypeAllowed(string code) => provider.PurchaseDocumentTypes.Any(x => x.Code == code);
    public bool IsSupportDocumentTypeAllowed(string code) => provider.SupportDocumentTypes.Any(x => x.Code == code);
    public bool IsVoidedDocumentTypeAllowed(string code) => provider.VoidedDocumentTypes.Any(x => x.Code == code);
    public bool IsPurchaseTaxCodeAllowed(string code) => provider.PurchaseTaxCodes.Any(x => x.Code == code);
    public FinancialTaxCatalogItem? PurchaseRule(string documentType) => provider.PurchaseDocumentTypes.FirstOrDefault(x => x.Code == documentType);
    public FinancialTaxCatalogItem? SupportRule(string supportType) => provider.SupportDocumentTypes.FirstOrDefault(x => x.Code == supportType);
    public FinancialTaxCatalogVersion Version => provider.Version;

    private async Task<FinancialTaxCatalogResponse> ResponseAsync(IReadOnlyCollection<FinancialTaxCatalogItem> items, PortalCallContext context, CancellationToken ct)
    {
        await audit.RecordAsync(new("TaxCatalogsQueried", "financial.tax-catalog", context.TenantId, new { provider.Version.Version, Count = items.Count }), context, ct);
        return new(provider.Version.Version, provider.Version.Disclaimer, items);
    }

    private static void AddLookup(List<FinancialTaxCatalogIssue> issues, IReadOnlyCollection<FinancialTaxCatalogItem> items, string value, string code, string message, string field)
    {
        var item = items.FirstOrDefault(x => x.Code == value);
        if (item is null) { issues.Add(new(code, message, "Error", field, value)); return; }
        AddFoundationWarnings(issues, item, field.Contains("voided", StringComparison.OrdinalIgnoreCase) ? "voided" : "purchase", field);
    }

    private static void AddRuleIssues(List<FinancialTaxCatalogIssue> issues, FinancialTaxCatalogItem? item, string? authorization, string? accessKey, int taxCount, string prefix)
    {
        if (item is null) return;
        if (item.RequiresAuthorization && string.IsNullOrWhiteSpace(authorization)) issues.Add(new($"{prefix}.catalog.authorization.required", "Authorization is required by foundation catalog.", "Warning", "authorizationNumber", null));
        if (item.RequiresAccessKey && string.IsNullOrWhiteSpace(accessKey)) issues.Add(new($"{prefix}.catalog.access_key.required", "Access key is required by foundation catalog.", "Warning", "accessKey", null));
        if (item.RequiresTaxBreakdown && taxCount == 0) issues.Add(new($"{prefix}.catalog.tax_breakdown.required", "Tax breakdown is required by foundation catalog.", "Error", "taxBreakdown", null));
    }

    private static void AddFoundationWarnings(List<FinancialTaxCatalogIssue> issues, FinancialTaxCatalogItem item, string prefix, string field)
    {
        if (item.IsFoundationOnly) issues.Add(new($"{prefix}.catalog.foundation_only", "Catalog item is foundation-only and not official final.", "Info", field, item.Code));
        if (item.RequiresTaxReview) issues.Add(new($"{prefix}.catalog.tax_review.required", "Catalog item requires Ecuador tax review.", "Warning", field, item.Code));
    }
}
