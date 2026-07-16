using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;
using Financiero.Domain;

namespace Financiero.Application;

public enum AtsXmlGenerationStatus { DisabledByConfiguration, BlockedByReadiness, GeneratedFoundation, RequiresTaxReview, RequiresOfficialSchema, RequiresExternalApproval }
public sealed record AtsXmlGenerationRequest(string Period, bool IncludeXml, bool AcknowledgeFoundationOnly, bool AcknowledgeNoSriSubmission, bool AcknowledgeNoOfficialCompliance);
public sealed record AtsXmlGenerationIssue(string Code, string Message, string Severity, string Field);
public sealed record AtsXmlSectionPayload(string Section, int Count, IReadOnlyCollection<string> Warnings);
public sealed record AtsXmlGenerationReadiness(string Period, string TenantId, string CatalogVersion, AtsXmlGenerationStatus Status, bool CanGenerateFoundation, bool CanPreviewXml, IReadOnlyCollection<AtsXmlGenerationIssue> BlockedReasons, IReadOnlyCollection<AtsXmlGenerationIssue> Warnings, IReadOnlyCollection<string> RequiredExternalApprovals, string Disclaimer);
public sealed record AtsXmlGenerationResult(string Period, string TenantId, string CatalogVersion, DateTimeOffset GeneratedAtUtc, bool IsOfficial, bool IsFoundationOnly, bool RequiresTaxReview, bool RequiresOfficialSchemaValidation, AtsXmlGenerationStatus Status, IReadOnlyCollection<AtsXmlSectionPayload> Sections, string? XmlContent, string? XmlHash, string Disclaimer, AtsXmlGenerationReadiness Readiness);

public interface IAtsXmlReadinessValidator { Task<AtsXmlGenerationReadiness> CheckAsync(string period, string environment, PortalCallContext context, CancellationToken ct); }
public interface IAtsXmlFoundationGenerator { Task<AtsXmlGenerationResult> GeneratePreviewAsync(AtsXmlGenerationRequest request, string environment, PortalCallContext context, CancellationToken ct); }

public sealed class AtsXmlReadinessValidator(IFinancialConfigurationReader configuration, AtsSupportMappingService mapping, FinancialTaxCatalogService catalog, IPortalAuditClient audit) : IAtsXmlReadinessValidator
{
    public const string Disclaimer = "ATS XML foundation preview only. It is not official ATS XML, is not submitted to SRI and does not certify tax compliance.";

    public async Task<AtsXmlGenerationReadiness> CheckAsync(string period, string environment, PortalCallContext context, CancellationToken ct)
    {
        var issues = new List<AtsXmlGenerationIssue>();
        var warnings = new List<AtsXmlGenerationIssue>();
        if (!PurchaseTaxDocumentValidator.IsFiscalPeriod(period)) issues.Add(Issue("ats.xml.period.invalid", "ATS XML period must use YYYY-MM.", "Error", "period"));
        var enabled = await configuration.GetBoolAsync("financial.sri.atsXmlFoundation.enabled", false, context, ct);
        var allowPreview = await configuration.GetBoolAsync("financial.sri.atsXmlFoundation.allowXmlPreview", false, context, ct);
        var requireSynthetic = await configuration.GetBoolAsync("financial.sri.atsXmlFoundation.requireSyntheticData", true, context, ct);
        if (!enabled) issues.Add(Issue("ats.xml.disabled", "ATS XML foundation preview is disabled by configuration.", "Error", "configuration"));
        if (environment.Equals("Production", StringComparison.OrdinalIgnoreCase)) issues.Add(Issue("ats.xml.production.blocked", "ATS XML foundation preview is blocked in Production.", "Error", "environment"));
        warnings.Add(Issue("ats.xml.schema.official_missing", "Official SRI ATS schema/layout validation is not implemented.", "Warning", "schema"));
        warnings.Add(Issue("ats.xml.external_approval.required", "External Ecuador tax/legal approval is required before official use.", "Warning", "approval"));
        warnings.Add(Issue("ats.xml.catalog.foundation_only", "Catalog version is foundation-only and requires tax review.", "Warning", "catalogVersion"));
        if (requireSynthetic) warnings.Add(Issue("ats.xml.synthetic_data.required", "Only synthetic/foundation data is allowed for preview.", "Warning", "data"));
        if (issues.Count == 0)
        {
            var section = await mapping.GetSectionReadinessAsync(period, context, ct);
            warnings.AddRange(section.Issues.Select(x => Issue(x.Code, x.Message, x.Severity, x.Field)));
        }
        await audit.RecordAsync(new("AtsXmlReadinessQueried", "financial.ats-xml-foundation", context.TenantId, new { period, Enabled = enabled, AllowXmlPreview = allowPreview, IssueCount = issues.Count, WarningCount = warnings.Count }), context, ct);
        var status = issues.Count > 0 ? AtsXmlGenerationStatus.DisabledByConfiguration : AtsXmlGenerationStatus.RequiresOfficialSchema;
        return new(period, context.TenantId, catalog.Version.Version, status, issues.Count == 0, enabled && allowPreview && issues.Count == 0, issues, warnings, ["Official SRI ATS schema validation", "Ecuador tax/legal review", "Operational approval before SRI submission"], Disclaimer);
    }

    private static AtsXmlGenerationIssue Issue(string code, string message, string severity, string field) => new(code, message, severity, field);
}

public sealed class AtsXmlFoundationGenerator(IFinancialConfigurationReader configuration, IPurchaseTaxDocumentRepository purchases, IVoidedTaxDocumentRepository voided, IElectronicDocumentRepository electronicDocuments, IAtsXmlReadinessValidator readinessValidator, FinancialTaxCatalogService catalog, IPortalAuditClient audit) : IAtsXmlFoundationGenerator
{
    public async Task<AtsXmlGenerationResult> GeneratePreviewAsync(AtsXmlGenerationRequest request, string environment, PortalCallContext context, CancellationToken ct)
    {
        if (!request.AcknowledgeFoundationOnly || !request.AcknowledgeNoSriSubmission || !request.AcknowledgeNoOfficialCompliance)
            throw new FinancialApplicationException("ats.xml.acknowledgement.required", "Foundation-only, no SRI submission and no official compliance acknowledgements are required.");
        var readiness = await readinessValidator.CheckAsync(request.Period, environment, context, ct);
        if (!readiness.CanGenerateFoundation)
        {
            await audit.RecordAsync(new("AtsXmlFoundationPreviewBlocked", "financial.ats-xml-foundation", context.TenantId, new { request.Period, readiness.Status, IssueCount = readiness.BlockedReasons.Count }), context, ct);
            return Result(request.Period, context, readiness, [], null, null, AtsXmlGenerationStatus.BlockedByReadiness);
        }
        var allowPreview = await configuration.GetBoolAsync("financial.sri.atsXmlFoundation.allowXmlPreview", false, context, ct);
        var purchasesByPeriod = await purchases.GetByPeriodAsync(context.TenantId, request.Period, ct);
        var voidedByPeriod = await voided.GetByPeriodAsync(context.TenantId, request.Period, ct);
        var (from, to) = PeriodRange(request.Period);
        var (docs, _) = await electronicDocuments.SearchAsync(context.TenantId, null, null, 1, await configuration.GetIntAsync("financial.sri.atsXmlFoundation.maxDocuments", 500, context, ct), ct);
        var sales = docs.Where(x => x.IssueDate >= from && x.IssueDate <= to).ToArray();
        var sections = new[]
        {
            new AtsXmlSectionPayload("informante", 1, ["Sanitized foundation issuer placeholder."]),
            new AtsXmlSectionPayload("ventas", sales.Length, ["Sales section is foundation-only."]),
            new AtsXmlSectionPayload("compras", purchasesByPeriod.Count, ["Purchase section is foundation-only."]),
            new AtsXmlSectionPayload("retenciones", sales.Count(x => x.DocumentType == ElectronicDocumentType.Withholding), ["Withholding section is foundation-only."]),
            new AtsXmlSectionPayload("anulados", voidedByPeriod.Count, ["Voided section is foundation-only."]),
            new AtsXmlSectionPayload("establecimientos", purchasesByPeriod.Select(x => x.Establishment).Concat(voidedByPeriod.Select(x => x.Establishment)).Distinct().Count(), ["Establishment summary is foundation-only."]),
            new AtsXmlSectionPayload("resumenImpuestos", purchasesByPeriod.SelectMany(x => x.Taxes).Count(), ["Tax summary requires official schema review."])
        };
        string? xml = null;
        string? hash = null;
        if (request.IncludeXml && allowPreview)
        {
            xml = BuildXml(request.Period, context.TenantId, catalog.Version.Version, sections, purchasesByPeriod, voidedByPeriod);
            hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(xml))).ToLowerInvariant();
        }
        await audit.RecordAsync(new("AtsXmlFoundationPreviewGenerated", "financial.ats-xml-foundation", context.TenantId, new { request.Period, SectionCount = sections.Length, IncludesXml = xml is not null, XmlHash = hash }), context, ct);
        return Result(request.Period, context, readiness, sections, xml, hash, xml is null ? AtsXmlGenerationStatus.RequiresOfficialSchema : AtsXmlGenerationStatus.GeneratedFoundation);
    }

    private static AtsXmlGenerationResult Result(string period, PortalCallContext context, AtsXmlGenerationReadiness readiness, IReadOnlyCollection<AtsXmlSectionPayload> sections, string? xml, string? hash, AtsXmlGenerationStatus status) =>
        new(period, context.TenantId, readiness.CatalogVersion, DateTimeOffset.UtcNow, false, true, true, true, status, sections, xml, hash, AtsXmlReadinessValidator.Disclaimer, readiness);

    private static string BuildXml(string period, string tenantId, string catalogVersion, IReadOnlyCollection<AtsXmlSectionPayload> sections, IReadOnlyCollection<PurchaseTaxDocument> purchases, IReadOnlyCollection<VoidedTaxDocument> voided)
    {
        var root = new XElement("atsFoundationPreview",
            new XAttribute("period", period),
            new XAttribute("tenantId", tenantId),
            new XAttribute("catalogVersion", catalogVersion),
            new XAttribute("isOfficial", "false"),
            new XAttribute("foundationOnly", "true"),
            new XElement("disclaimer", AtsXmlReadinessValidator.Disclaimer),
            new XElement("informante", new XElement("identificacion", "SYNTHETIC"), new XElement("razonSocial", "FOUNDATION PREVIEW")),
            new XElement("ventas", sections.First(x => x.Section == "ventas").Count),
            new XElement("compras", purchases.Select(x => new XElement("compra", new XAttribute("tipoDocumento", x.DocumentType), new XAttribute("tipoSustento", x.SupportDocumentType), new XAttribute("numero", x.Establishment + "-" + x.EmissionPoint + "-" + x.Sequential), new XElement("total", x.Total.ToString("0.00"))))),
            new XElement("retenciones", sections.First(x => x.Section == "retenciones").Count),
            new XElement("anulados", voided.Select(x => new XElement("anulado", new XAttribute("tipoDocumento", x.DocumentType), new XAttribute("numero", x.DocumentNumber), new XElement("motivo", "REDACTED")))),
            new XElement("establecimientos", sections.First(x => x.Section == "establecimientos").Count),
            new XElement("resumenImpuestos", sections.First(x => x.Section == "resumenImpuestos").Count));
        return root.ToString(SaveOptions.DisableFormatting);
    }

    private static (DateOnly From, DateOnly To) PeriodRange(string period)
    {
        var parts = period.Split('-');
        var from = new DateOnly(int.Parse(parts[0]), int.Parse(parts[1]), 1);
        return (from, from.AddMonths(1).AddDays(-1));
    }
}
