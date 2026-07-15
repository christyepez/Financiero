using Financiero.Domain;

namespace Financiero.Application;

public enum SupportDocumentMappingStatus { MappedFoundation, MissingRequiredData, UnsupportedFoundation, RequiresTaxReview }
public sealed record SupportDocumentMappingRule(string DocumentType, string SupportDocumentType, string AtsSection, string AtsFieldGroup, bool RequiresAuthorization, bool RequiresAccessKey, bool RequiresSupplierIdentification, bool RequiresTaxBreakdown, bool RequiresRetentionLink, bool IsFoundationOnly, bool RequiresTaxReview);
public sealed record SupportDocumentMappingIssue(string Code, string Message, string Severity, string Field);
public sealed record SupportDocumentMappingResult(string DocumentType, string SupportDocumentType, string AtsSection, string AtsFieldGroup, SupportDocumentMappingStatus Status, IReadOnlyCollection<SupportDocumentMappingIssue> Issues, SupportDocumentMappingRule? Rule, string CatalogVersion, string Disclaimer);
public sealed record AtsFieldReadiness(string Field, string Status, string Source, string Notes);
public sealed record AtsSectionReadiness(string Section, int MappedCount, int MissingCount, int UnsupportedCount, IReadOnlyCollection<string> RequiredFields, IReadOnlyCollection<string> MissingFields, IReadOnlyCollection<string> Warnings, string Disclaimer);
public sealed record AtsPurchaseMappingResult(Guid Id, string DocumentNumber, string SupplierIdentificationMasked, string? AccessKeyMasked, string? AuthorizationNumberMasked, SupportDocumentMappingResult Mapping, IReadOnlyCollection<AtsFieldReadiness> Fields, string Disclaimer);
public sealed record AtsVoidedMappingResult(Guid Id, string DocumentNumber, string? AccessKeyMasked, string? AuthorizationNumberMasked, SupportDocumentMappingResult Mapping, IReadOnlyCollection<AtsFieldReadiness> Fields, string Disclaimer);
public sealed record AtsSectionReadinessResult(string Period, string CatalogVersion, IReadOnlyCollection<AtsSectionReadiness> Sections, IReadOnlyCollection<SupportDocumentMappingIssue> Issues, string Disclaimer);

public sealed class AtsSupportMappingService(IPurchaseTaxDocumentRepository purchases, IVoidedTaxDocumentRepository voided, IPortalAuditClient audit, FinancialTaxCatalogService? catalog = null)
{
    private const string Disclaimer = "ATS support document mapping foundation only. It is not official ATS XML and does not certify tax compliance.";
    private string CatalogVersion => catalog?.Version.Version ?? FoundationFinancialTaxCatalogProvider.CatalogVersion;
    private static readonly SupportDocumentMappingRule[] Rules =
    [
        new("01", "01", "Purchases", "PurchaseInvoice", true, false, true, true, false, true, true),
        new("04", "04", "Purchases", "PurchaseCreditNote", true, false, true, true, false, true, true),
        new("05", "05", "Purchases", "PurchaseDebitNote", true, false, true, true, false, true, true),
        new("01", "04", "Purchases", "LiquidationPurchasePlaceholder", true, true, true, true, false, true, true),
        new("07", "WithholdingSupport", "Withholdings", "WithholdingSupportRelation", true, false, true, true, true, true, true),
        new("Voided", "Voided", "VoidedDocuments", "VoidedSupportDocument", false, false, false, false, false, true, true)
    ];

    public async Task<IReadOnlyCollection<SupportDocumentMappingRule>> GetMappingsAsync(PortalCallContext context, CancellationToken ct)
    {
        await audit.RecordAsync(new("SupportDocumentMappingsQueried", "financial.ats-support-mapping", context.TenantId, new { Count = Rules.Length }), context, ct);
        return Rules;
    }

    public async Task<AtsPurchaseMappingResult> MapPurchaseAsync(Guid id, PortalCallContext context, CancellationToken ct)
    {
        var document = await purchases.GetByIdAsync(id, context.TenantId, ct) ?? throw new FinancialApplicationException("purchase.not_found", "Purchase tax document was not found.");
        var result = MapPurchase(document, catalog);
        await audit.RecordAsync(new("PurchaseAtsMappingQueried", "financial.ats-support-mapping", context.TenantId, new { id, result.Mapping.Status, IssueCount = result.Mapping.Issues.Count }), context, ct);
        return result;
    }

    public async Task<AtsVoidedMappingResult> MapVoidedAsync(Guid id, PortalCallContext context, CancellationToken ct)
    {
        var document = await voided.GetByIdAsync(id, context.TenantId, ct) ?? throw new FinancialApplicationException("voided.not_found", "Voided tax document was not found.");
        var result = MapVoided(document, catalog);
        await audit.RecordAsync(new("VoidedAtsMappingQueried", "financial.ats-support-mapping", context.TenantId, new { id, result.Mapping.Status, IssueCount = result.Mapping.Issues.Count }), context, ct);
        return result;
    }

    public async Task<AtsSectionReadinessResult> GetSectionReadinessAsync(string period, PortalCallContext context, CancellationToken ct)
    {
        var purchaseItems = await purchases.GetByPeriodAsync(context.TenantId, period, ct);
        var voidedItems = await voided.GetByPeriodAsync(context.TenantId, period, ct);
        var purchaseMappings = purchaseItems.Select(x => MapPurchase(x, catalog)).ToArray();
        var voidedMappings = voidedItems.Select(x => MapVoided(x, catalog)).ToArray();
        var issues = purchaseMappings.SelectMany(x => x.Mapping.Issues).Concat(voidedMappings.SelectMany(x => x.Mapping.Issues)).ToArray();
        var sections = new[]
        {
            Section("Informant", 1, 0, 0, ["issuer.ruc", "issuer.legalName"], [], ["Issuer configuration is reused from SRI foundation and still requires external review."]),
            Section("Sales", 0, 0, 0, ["sales.documents", "sales.taxes"], [], ["Sales mapping remains based on electronic document foundation."]),
            Section("Purchases", purchaseMappings.Count(x => x.Mapping.Status is SupportDocumentMappingStatus.MappedFoundation or SupportDocumentMappingStatus.RequiresTaxReview), purchaseMappings.Count(x => x.Mapping.Status == SupportDocumentMappingStatus.MissingRequiredData), purchaseMappings.Count(x => x.Mapping.Status == SupportDocumentMappingStatus.UnsupportedFoundation), ["supplierIdentification", "supportDocumentType", "documentNumber", "taxBreakdown"], purchaseMappings.SelectMany(x => x.Fields).Where(x => x.Status == "MissingRequiredData").Select(x => x.Field).Distinct().ToArray(), purchaseMappings.SelectMany(x => x.Mapping.Issues).Select(x => x.Message).Distinct().ToArray()),
            Section("Withholdings", 0, 0, 0, ["withholding.supportRelation"], [], ["Withholding support relation remains foundation-only and requires tax review."]),
            Section("VoidedDocuments", voidedMappings.Count(x => x.Mapping.Status is SupportDocumentMappingStatus.MappedFoundation or SupportDocumentMappingStatus.RequiresTaxReview), voidedMappings.Count(x => x.Mapping.Status == SupportDocumentMappingStatus.MissingRequiredData), voidedMappings.Count(x => x.Mapping.Status == SupportDocumentMappingStatus.UnsupportedFoundation), ["documentNumber", "voidDate", "reason", "fiscalPeriod"], voidedMappings.SelectMany(x => x.Fields).Where(x => x.Status == "MissingRequiredData").Select(x => x.Field).Distinct().ToArray(), voidedMappings.SelectMany(x => x.Mapping.Issues).Select(x => x.Message).Distinct().ToArray()),
            Section("Establishments", purchaseItems.Select(x => x.Establishment).Concat(voidedItems.Select(x => x.Establishment)).Distinct().Count(), 0, 0, ["establishment", "emissionPoint"], [], ["Establishment aggregation is foundation-only."]),
            Section("TaxSummary", purchaseItems.Count, purchaseMappings.Count(x => x.Fields.Any(f => f.Field == "taxBreakdown" && f.Status == "MissingRequiredData")), 0, ["taxBase", "taxAmount"], [], ["Tax summary requires official ATS schema/catalog review."])
        };
        await audit.RecordAsync(new("AtsSectionReadinessQueried", "financial.ats-support-mapping", context.TenantId, new { period, PurchaseCount = purchaseItems.Count, VoidedCount = voidedItems.Count, IssueCount = issues.Length }), context, ct);
        return new(period, CatalogVersion, sections, issues, Disclaimer);
    }

    public static AtsPurchaseMappingResult MapPurchase(PurchaseTaxDocument document, FinancialTaxCatalogService? catalog = null)
    {
        var rule = Rules.FirstOrDefault(x => x.DocumentType == document.DocumentType && x.SupportDocumentType == document.SupportDocumentType);
        var issues = new List<SupportDocumentMappingIssue>();
        if (rule is null) issues.Add(Issue("support.mapping.unsupported", "Purchase support document mapping is not supported by foundation catalog.", "Warning", "supportDocumentType"));
        AddCatalogIssues(issues, catalog?.ValidatePurchase(document));
        var effectiveRule = rule ?? new(document.DocumentType, document.SupportDocumentType, "Purchases", "UnsupportedFoundation", false, false, true, true, false, true, true);
        AddRequiredIssues(issues, effectiveRule, document.SupplierIdentification, document.AuthorizationNumber, document.AccessKey, document.Taxes.Count, "purchase");
        try { PurchaseTaxCalculationValidator.Validate(document); }
        catch (FinancialDomainException ex) { issues.Add(Issue("purchase.mapping.tax_breakdown.invalid", ex.Message, "Warning", "taxBreakdown")); }
        if (effectiveRule.RequiresTaxReview) issues.Add(Issue("support.mapping.tax_review.required", "Support document mapping is foundation-only and requires Ecuador tax review.", "Info", "supportDocumentType"));
        var status = Status(effectiveRule, issues);
        return new(document.Id, $"{document.Establishment}-{document.EmissionPoint}-{document.Sequential}", SriSensitiveDataSanitizer.MaskCustomerIdentification(document.SupplierIdentification) ?? "", SriSensitiveDataSanitizer.MaskAccessKey(document.AccessKey), SriSensitiveDataSanitizer.MaskAccessKey(document.AuthorizationNumber), new(document.DocumentType, document.SupportDocumentType, effectiveRule.AtsSection, effectiveRule.AtsFieldGroup, status, issues, rule, catalog?.Version.Version ?? FoundationFinancialTaxCatalogProvider.CatalogVersion, Disclaimer), PurchaseFields(document, effectiveRule, issues), Disclaimer);
    }

    public static AtsVoidedMappingResult MapVoided(VoidedTaxDocument document, FinancialTaxCatalogService? catalog = null)
    {
        var rule = Rules.First(x => x.DocumentType == "Voided");
        var issues = new List<SupportDocumentMappingIssue>();
        AddCatalogIssues(issues, catalog?.ValidateVoided(document));
        if (!PurchaseTaxDocumentValidator.IsFiscalPeriod(document.FiscalPeriod)) issues.Add(Issue("voided.mapping.period.invalid", "Voided fiscal period must use YYYY-MM.", "Error", "fiscalPeriod"));
        if (string.IsNullOrWhiteSpace(document.Reason)) issues.Add(Issue("voided.mapping.reason.required", "Voided document reason is required.", "Error", "reason"));
        if (!PurchaseTaxDocumentValidator.IsDocumentNumber(document.Establishment, document.EmissionPoint, document.Sequential)) issues.Add(Issue("voided.mapping.document_number.invalid", "Voided document number must use ###-###-######### parts.", "Error", "documentNumber"));
        issues.Add(Issue("voided.mapping.tax_review.required", "Voided document mapping is foundation-only and requires Ecuador tax review.", "Info", "voidedDocument"));
        var status = Status(rule, issues);
        return new(document.Id, document.DocumentNumber, SriSensitiveDataSanitizer.MaskAccessKey(document.AccessKey), SriSensitiveDataSanitizer.MaskAccessKey(document.AuthorizationNumber), new(document.DocumentType, "Voided", rule.AtsSection, rule.AtsFieldGroup, status, issues, rule, catalog?.Version.Version ?? FoundationFinancialTaxCatalogProvider.CatalogVersion, Disclaimer), VoidedFields(document, issues), Disclaimer);
    }

    private static void AddCatalogIssues(List<SupportDocumentMappingIssue> issues, FinancialTaxCatalogValidationResult? validation)
    {
        if (validation is null) return;
        issues.AddRange(validation.Issues.Select(x => Issue(x.Code, x.Message, x.Severity, x.Field)));
    }

    private static void AddRequiredIssues(List<SupportDocumentMappingIssue> issues, SupportDocumentMappingRule rule, string? supplierIdentification, string? authorizationNumber, string? accessKey, int taxCount, string prefix)
    {
        if (rule.RequiresSupplierIdentification && string.IsNullOrWhiteSpace(supplierIdentification)) issues.Add(Issue($"{prefix}.mapping.supplier_identification.required", "Supplier identification is required by support document mapping.", "Error", "supplierIdentification"));
        if (rule.RequiresAuthorization && string.IsNullOrWhiteSpace(authorizationNumber)) issues.Add(Issue($"{prefix}.mapping.authorization.required", "Authorization number is required by support document mapping foundation.", "Warning", "authorizationNumber"));
        if (rule.RequiresAccessKey && string.IsNullOrWhiteSpace(accessKey)) issues.Add(Issue($"{prefix}.mapping.access_key.required", "Access key is required by support document mapping foundation.", "Warning", "accessKey"));
        if (rule.RequiresTaxBreakdown && taxCount == 0) issues.Add(Issue($"{prefix}.mapping.tax_breakdown.required", "Tax breakdown is required by support document mapping foundation.", "Error", "taxBreakdown"));
    }

    private static SupportDocumentMappingStatus Status(SupportDocumentMappingRule rule, IReadOnlyCollection<SupportDocumentMappingIssue> issues) =>
        issues.Any(x => x.Severity == "Error") ? SupportDocumentMappingStatus.MissingRequiredData :
        rule.AtsFieldGroup == "UnsupportedFoundation" ? SupportDocumentMappingStatus.UnsupportedFoundation :
        rule.RequiresTaxReview || issues.Count > 0 ? SupportDocumentMappingStatus.RequiresTaxReview :
        SupportDocumentMappingStatus.MappedFoundation;

    private static IReadOnlyCollection<AtsFieldReadiness> PurchaseFields(PurchaseTaxDocument d, SupportDocumentMappingRule r, IReadOnlyCollection<SupportDocumentMappingIssue> issues) =>
    [
        Field("supplierIdentification", !string.IsNullOrWhiteSpace(d.SupplierIdentification), "PurchaseTaxDocument.SupplierIdentification", "Masked in API responses."),
        Field("authorizationNumber", !r.RequiresAuthorization || !string.IsNullOrWhiteSpace(d.AuthorizationNumber), "PurchaseTaxDocument.AuthorizationNumber", "Masked in API responses."),
        Field("accessKey", !r.RequiresAccessKey || !string.IsNullOrWhiteSpace(d.AccessKey), "PurchaseTaxDocument.AccessKey", "Masked in API responses."),
        Field("supportDocumentType", true, "PurchaseTaxDocument.SupportDocumentType", "Foundation catalog only."),
        Field("taxBreakdown", d.Taxes.Count > 0 && !issues.Any(x => x.Field == "taxBreakdown" && x.Severity == "Error"), "PurchaseTax", "Official ATS tax mapping pending review.")
    ];

    private static IReadOnlyCollection<AtsFieldReadiness> VoidedFields(VoidedTaxDocument d, IReadOnlyCollection<SupportDocumentMappingIssue> issues) =>
    [
        Field("documentNumber", PurchaseTaxDocumentValidator.IsDocumentNumber(d.Establishment, d.EmissionPoint, d.Sequential), "VoidedTaxDocument.DocumentNumber", "Foundation document number."),
        Field("fiscalPeriod", PurchaseTaxDocumentValidator.IsFiscalPeriod(d.FiscalPeriod), "VoidedTaxDocument.FiscalPeriod", "ATS period foundation."),
        Field("reason", !string.IsNullOrWhiteSpace(d.Reason), "VoidedTaxDocument.Reason", "Reason is required for foundation mapping."),
        Field("accessKey", true, "VoidedTaxDocument.AccessKey", "Optional and masked if present.")
    ];

    private static AtsFieldReadiness Field(string field, bool ready, string source, string notes) => new(field, ready ? "MappedFoundation" : "MissingRequiredData", source, notes);
    private static SupportDocumentMappingIssue Issue(string code, string message, string severity, string field) => new(code, message, severity, field);
    private static AtsSectionReadiness Section(string section, int mapped, int missing, int unsupported, IReadOnlyCollection<string> required, IReadOnlyCollection<string> missingFields, IReadOnlyCollection<string> warnings) => new(section, mapped, missing, unsupported, required, missingFields, warnings, Disclaimer);
}
