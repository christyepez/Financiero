using System.Globalization;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Xml.Linq;
using System.Security.Cryptography;
using Financiero.Domain;

namespace Financiero.Application;

public sealed record CreateInvoiceRequest(DateOnly IssueDate, string CustomerIdentificationType, string CustomerIdentification, string CustomerName, string Currency = "USD", string? EstablishmentCode = null, string? EmissionPointCode = null);
public sealed record CreateCreditNoteRequest(DateOnly IssueDate, string CustomerIdentificationType, string CustomerIdentification, string CustomerName, string RelatedDocumentTypeCode, string RelatedDocumentNumber, DateOnly RelatedDocumentIssueDate, string Reason, string Currency = "USD", string? EstablishmentCode = null, string? EmissionPointCode = null);
public sealed record CreateDebitNoteRequest(DateOnly IssueDate, string CustomerIdentificationType, string CustomerIdentification, string CustomerName, string RelatedDocumentTypeCode, string RelatedDocumentNumber, DateOnly RelatedDocumentIssueDate, string Currency = "USD", string? EstablishmentCode = null, string? EmissionPointCode = null);
public sealed record CreateWithholdingRequest(DateOnly IssueDate, string SubjectIdentificationType, string SubjectIdentification, string SubjectName, string FiscalPeriod, string SupportDocumentTypeCode, string SupportDocumentNumber, DateOnly SupportDocumentIssueDate, string Currency = "USD", string? EstablishmentCode = null, string? EmissionPointCode = null);
public sealed record AddElectronicDocumentLineRequest(string ProductCode, string Description, decimal Quantity, decimal UnitPrice, decimal Discount = 0);
public sealed record AddDebitNoteReasonRequest(string Reason, decimal Amount);
public sealed record AddWithholdingTaxRequest(string TaxCode, string WithholdingCode, decimal TaxBase, decimal WithholdingPercentage, decimal WithheldAmount, string SupportDocumentNumber, DateOnly SupportDocumentIssueDate, string FiscalPeriod);
public sealed record SearchElectronicDocumentsRequest(string? Status = null, string? AccessKey = null, int Page = 1, int PageSize = 20);
public sealed record ElectronicDocumentLineDto(Guid Id, int LineNumber, string ProductCode, string Description, decimal Quantity, decimal UnitPrice, decimal Discount, decimal Subtotal, decimal Total);
public sealed record ElectronicDocumentReferenceDto(string DocumentTypeCode, string Number, DateOnly IssueDate, string ReasonOrPeriod);
public sealed record ElectronicDocumentDebitNoteReasonDto(string Reason, decimal Amount);
public sealed record ElectronicDocumentWithholdingTaxDto(string TaxCode, string WithholdingCode, decimal TaxBase, decimal WithholdingPercentage, decimal WithheldAmount, string SupportDocumentNumber, DateOnly SupportDocumentIssueDate, string FiscalPeriod);
public sealed record ElectronicDocumentDto(
    Guid Id,
    string TenantId,
    string DocumentType,
    string Environment,
    string EmissionType,
    string Status,
    string EstablishmentCode,
    string EmissionPointCode,
    string? Sequential,
    string? AccessKey,
    DateOnly IssueDate,
    string CustomerIdentificationType,
    string CustomerIdentification,
    string CustomerName,
    string Currency,
    decimal SubtotalWithoutTaxes,
    decimal TotalDiscount,
    decimal TotalTaxes,
    decimal TotalAmount,
    string? SriAuthorizationNumber,
    DateTimeOffset? SriAuthorizationDate,
    string? SriResponseCode,
    string? SriResponseMessage,
    string? UnsignedXmlStorageId,
    string? SignedXmlStorageId,
    string? AuthorizationXmlStorageId,
    string? RidePdfStorageId,
    string? SignatureProvider,
    string? SignatureDigest,
    string? LastSriStatus,
    string? LastSriMessage,
    IReadOnlyCollection<ElectronicDocumentLineDto> Lines,
    IReadOnlyCollection<ElectronicDocumentReferenceDto>? References = null,
    IReadOnlyCollection<ElectronicDocumentDebitNoteReasonDto>? DebitNoteReasons = null,
    IReadOnlyCollection<ElectronicDocumentWithholdingTaxDto>? WithholdingTaxes = null);

public interface IElectronicDocumentRepository
{
    Task AddAsync(ElectronicDocument document, CancellationToken ct);
    Task AddLineAsync(ElectronicDocumentLine line, CancellationToken ct);
    Task<ElectronicDocument?> GetByIdAsync(Guid id, string tenantId, CancellationToken ct);
    Task<ElectronicDocument?> GetByAccessKeyAsync(string accessKey, string tenantId, CancellationToken ct);
    Task<(IReadOnlyCollection<ElectronicDocument> Items, long Total)> SearchAsync(string tenantId, ElectronicDocumentStatus? status, string? accessKey, int page, int pageSize, CancellationToken ct);
    Task<string> GetNextSequentialAsync(string tenantId, ElectronicDocumentType documentType, SriEnvironment environment, string establishmentCode, string emissionPointCode, CancellationToken ct);
    Task<bool> SequenceDocumentExistsAsync(string tenantId, ElectronicDocumentType documentType, SriEnvironment environment, string establishmentCode, string emissionPointCode, string sequential, Guid? excludingId, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}

public sealed record IssuerSriOptions(string Ruc, string LegalName, string? TradeName, string Address, bool AccountingRequired);
public enum SignatureProviderType { Development, Disabled, External, LocalCertificatePlaceholder, Xades }
public sealed record SignatureContext(string TenantId, SignatureProviderType Provider, string EnvironmentName, string? CertificateSecretName, string? KeyVaultName, string? LocalCertificatePath, string? LocalCertificatePasswordSecretName, bool RequireTrustedCertificate, string? CertificateSource = null, string? TimestampPolicy = null, bool RequireOcsp = false);
public sealed record SignatureResult(string SignedXml, string Provider, string? CertificateAlias, DateTimeOffset SignedAtUtc, string SignatureDigest, string SignatureMode);
public sealed record XadesSignatureOptions(string Provider, string CertificateSource, string? CertificateSecretName, string? KeyVaultName, string? TimestampPolicy, bool RequireOcsp, bool RequireTrustedCertificate);
public sealed record CertificateDescriptor(string Alias, string Source, bool HasPrivateKey, bool IsTrusted, DateTimeOffset? ExpiresAtUtc)
{
    public override string ToString() => $"CertificateDescriptor(Alias={SecretMaskingHelper.Mask(Alias)}, Source={Source}, HasPrivateKey={HasPrivateKey}, IsTrusted={IsTrusted}, ExpiresAtUtc={ExpiresAtUtc:O})";
}
public sealed record CertificateLoadResult(bool IsLoaded, CertificateDescriptor? Descriptor, string? ErrorCode, string? ErrorMessage);
public enum SecretStoreProviderType { Development, AzureKeyVault, External, Disabled }
public sealed record SecretReference(string Name, SecretStoreProviderType Provider, string? VaultName = null)
{
    public override string ToString() => $"SecretReference(Name={SecretMaskingHelper.Mask(Name)}, Provider={Provider}, VaultName={SecretMaskingHelper.Mask(VaultName)})";
}
public sealed record SecretValue(byte[] Value, string ContentType)
{
    public override string ToString() => $"SecretValue(ContentType={ContentType}, Length={Value.Length}, Value=**REDACTED**)";
}
public sealed record SecretMetadata(string Alias, string Source, DateTimeOffset? ExpiresAtUtc)
{
    public override string ToString() => $"SecretMetadata(Alias={SecretMaskingHelper.Mask(Alias)}, Source={Source}, ExpiresAtUtc={ExpiresAtUtc:O})";
}
public sealed record SecretStoreResult(bool Success, SecretValue? Value, SecretMetadata? Metadata, string? ErrorCode, string? ErrorMessage)
{
    public override string ToString() => $"SecretStoreResult(Success={Success}, Metadata={Metadata}, ErrorCode={ErrorCode}, ErrorMessage={SecretMaskingHelper.Mask(ErrorMessage)})";
}
public sealed record AzureKeyVaultSecretStoreOptions(string? KeyVaultName, bool UseDefaultAzureCredential, bool RequireManagedIdentity, bool FailFastOnStartup);
public sealed record SecretStoreReadinessResult(string Status, string Provider, IReadOnlyCollection<string> Checks, IReadOnlyCollection<string> Issues);
public enum SriResponseStatus { Received, Returned, Processing, Authorized, Rejected, NotFound, Error }
public enum SriConnectivityMode { Mock, TestDryRun, TestConnectivityProbe, TestSendDisabled, ProductionBlocked, ManualRequired }
public sealed record SriClientContext(string TenantId, SriEnvironment Environment, string Mode, string? ReceptionUrl, string? AuthorizationUrl, int TimeoutSeconds, int MaxRetries, int RetryDelaySeconds, bool AllowProduction = false, bool LogPayloads = false, bool DryRun = true, bool MaskPayloads = true);
public sealed record SriSoapOptions(string Mode, bool AllowProduction, bool DryRun, string? ReceptionUrl, string? AuthorizationUrl, int TimeoutSeconds, int MaxRetries, int RetryDelaySeconds, bool LogPayloads, bool MaskPayloads);
public sealed record SriConnectivityProbeResult(SriConnectivityMode Mode, string Status, string SanitizedMessage, string? ReceptionEndpointMasked, string? AuthorizationEndpointMasked, bool DocumentSendAllowed);
public sealed record SriObservabilityEvent(string CorrelationId, string TenantId, Guid? DocumentId, string? DocumentType, string Status, string Provider, string Mode, long DurationMs, int AttemptNumber, string? AccessKeyMasked, string? Hash, string? ErrorCode, string? SanitizedMessage);
public sealed record IntegrationAttemptTelemetry(string CorrelationId, string Provider, string Mode, long DurationMs, int AttemptNumber, string Status, string? SanitizedMessage);
public sealed record SriMessage(string Code, string Message, string? Type = null);
public sealed record SriReceptionRequest(string AccessKey, string SignedXml, SriClientContext Context);
public sealed record SriReceptionResponse(SriResponseStatus Status, string Code, string Message, IReadOnlyCollection<SriMessage> Messages);
public sealed record SriAuthorizationRequest(string AccessKey, SriClientContext Context);
public sealed record SriAuthorizationResponse(SriResponseStatus Status, string Code, string Message, string? AuthorizationNumber, DateTimeOffset? AuthorizationDate, string? AuthorizationXml, IReadOnlyCollection<SriMessage> Messages);
public sealed record XmlValidationResult(bool IsValid, IReadOnlyCollection<string> Errors, IReadOnlyCollection<string> Warnings);
public interface ISriCatalogProvider
{
    string Version { get; }
    IReadOnlyCollection<SriCatalogItem> GetCatalogItems();
    bool Contains(string catalog, string code);
}
public sealed class DevelopmentSriCatalogProvider : ISriCatalogProvider
{
    public string Version => SriCatalogService.FoundationVersion;
    public IReadOnlyCollection<SriCatalogItem> GetCatalogItems() => SriCatalogService.FoundationCatalogs;
    public bool Contains(string catalog, string code) => SriCatalogService.Contains(catalog, code);
}
public sealed record StoredDocumentFile(string StorageId, string Hash, string Provider, DateTimeOffset StoredAtUtc, string ContentType, string Purpose);
public sealed record PortalContentFileOptions(string Provider, string? PortalBaseUrl, string Container, int TimeoutSeconds, bool RetainXml, bool RetainPdf, bool SendPayloads = false, bool MaskPayloads = true);
public sealed record PortalContentFileRequest(string Purpose, string FileName, string ContentType, string Hash, string Container, string CorrelationId, string TenantId, IReadOnlyDictionary<string, string> Metadata, bool IncludePayload, string? PayloadBase64);
public sealed record PortalContentFileResponse(string StorageId, string Provider, DateTimeOffset StoredAtUtc);
public sealed record ElectronicDocumentStorageMetadataDto(string? UnsignedXmlStorageId, string? SignedXmlStorageId, string? AuthorizationXmlStorageId, string? RidePdfStorageId, string? XmlContentHash, string? SignedXmlContentHash, string? SignatureDigest, string? RidePdfHash = null, string? StorageProvider = null);
public sealed record InvoiceRideModel(string IssuerRuc, string IssuerName, string DocumentNumber, string AccessKey, DateOnly IssueDate, string CustomerName, string CustomerIdentification, IReadOnlyCollection<ElectronicDocumentLineDto> Lines, decimal Subtotal, decimal Taxes, decimal Total, string? AuthorizationNumber, DateTimeOffset? AuthorizationDate, string Environment, string Status);
public sealed record RidePdfGenerationResult(byte[] PdfBytes, string Html, string Hash, DateTimeOffset GeneratedAtUtc, string ContentType);
public sealed record RideMetadataDto(string? RidePdfStorageId, string? RidePdfHash, DateTimeOffset? RideGeneratedAtUtc, string? StorageProvider);

public interface IElectronicDocumentXmlGenerator
{
    string GenerateXml(ElectronicDocument document, IssuerSriOptions issuer);
    string GenerateInvoiceXml(ElectronicDocument document, IssuerSriOptions issuer);
    string GenerateCreditNoteXml(ElectronicDocument document, IssuerSriOptions issuer);
    string GenerateDebitNoteXml(ElectronicDocument document, IssuerSriOptions issuer);
    string GenerateWithholdingXml(ElectronicDocument document, IssuerSriOptions issuer);
}

public interface IElectronicSignatureService
{
    Task<SignatureResult> SignAsync(string unsignedXml, SignatureContext context, CancellationToken ct);
}

public interface ICertificateProvider
{
    Task<CertificateLoadResult> LoadAsync(SignatureContext context, CancellationToken ct);
}

public interface ISecretStoreClient
{
    Task<SecretStoreResult> GetSecretAsync(SecretReference reference, PortalCallContext context, CancellationToken ct);
}

public interface ISriReceptionClient
{
    Task<SriReceptionResponse> SendAsync(SriReceptionRequest request, CancellationToken ct);
}

public interface ISriAuthorizationClient
{
    Task<SriAuthorizationResponse> AuthorizeAsync(SriAuthorizationRequest request, CancellationToken ct);
}

public interface IElectronicDocumentXmlValidator
{
    XmlValidationResult ValidateXml(ElectronicDocumentType type, string xml);
    XmlValidationResult ValidateInvoiceXml(string xml);
    XmlValidationResult ValidateCreditNoteXml(string xml);
    XmlValidationResult ValidateDebitNoteXml(string xml);
    XmlValidationResult ValidateWithholdingXml(string xml);
}

public interface IXsdSchemaValidator
{
    XmlValidationResult Validate(string xml, string schemaName);
}

public interface IElectronicDocumentStorageClient
{
    Task<StoredDocumentFile> SaveUnsignedXmlAsync(ElectronicDocument document, string xml, PortalCallContext context, CancellationToken ct);
    Task<StoredDocumentFile> SaveSignedXmlAsync(ElectronicDocument document, string xml, PortalCallContext context, CancellationToken ct);
    Task<StoredDocumentFile> SaveAuthorizationXmlAsync(ElectronicDocument document, string xml, PortalCallContext context, CancellationToken ct);
    Task<StoredDocumentFile> SaveRidePdfAsync(ElectronicDocument document, byte[] pdf, PortalCallContext context, CancellationToken ct);
}

public interface IRidePdfGenerator
{
    Task<RidePdfGenerationResult> GenerateInvoiceRideAsync(InvoiceRideModel model, CancellationToken ct);
}

public sealed class ElectronicInvoiceXmlGenerator : IElectronicDocumentXmlGenerator
{
    public string GenerateXml(ElectronicDocument document, IssuerSriOptions issuer) => document.DocumentType switch
    {
        ElectronicDocumentType.Invoice => GenerateInvoiceXml(document, issuer),
        ElectronicDocumentType.CreditNote => GenerateCreditNoteXml(document, issuer),
        ElectronicDocumentType.DebitNote => GenerateDebitNoteXml(document, issuer),
        ElectronicDocumentType.Withholding => GenerateWithholdingXml(document, issuer),
        _ => throw new FinancialApplicationException("sri.xml.document_type.unsupported", "Unsupported SRI document type.")
    };

    public string GenerateInvoiceXml(ElectronicDocument document, IssuerSriOptions issuer)
    {
        if (document.AccessKey is null || document.Sequential is null) throw new FinancialApplicationException("sri.xml.access_key.required", "Access key and sequential are required.");
        var infoTributaria = InfoTributaria(document, issuer);

        var detalles = new XElement("detalles", document.Lines.Select(line => new XElement("detalle",
            new XElement("codigoPrincipal", line.ProductCode),
            new XElement("descripcion", line.Description),
            new XElement("cantidad", Format(line.Quantity)),
            new XElement("precioUnitario", Format(line.UnitPrice)),
            new XElement("descuento", Format(line.Discount)),
            new XElement("precioTotalSinImpuesto", Format(line.Subtotal)))));

        var infoFactura = new XElement("infoFactura",
            new XElement("fechaEmision", document.IssueDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)),
            new XElement("dirEstablecimiento", issuer.Address),
            new XElement("obligadoContabilidad", issuer.AccountingRequired ? "SI" : "NO"),
            new XElement("tipoIdentificacionComprador", document.CustomerIdentificationType),
            new XElement("razonSocialComprador", document.CustomerName),
            new XElement("identificacionComprador", document.CustomerIdentification),
            new XElement("totalSinImpuestos", Format(document.SubtotalWithoutTaxes)),
            new XElement("totalDescuento", Format(document.TotalDiscount)),
            new XElement("propina", "0.00"),
            new XElement("importeTotal", Format(document.TotalAmount)),
            new XElement("moneda", document.Currency));

        var invoice = new XElement("factura",
            new XAttribute("id", "comprobante"),
            new XAttribute("version", "1.0.0"),
            infoTributaria,
            infoFactura,
            detalles);
        return new XDocument(new XDeclaration("1.0", "UTF-8", null), invoice).ToString(SaveOptions.DisableFormatting);
    }

    public string GenerateCreditNoteXml(ElectronicDocument document, IssuerSriOptions issuer)
    {
        if (document.AccessKey is null || document.Sequential is null) throw new FinancialApplicationException("sri.xml.access_key.required", "Access key and sequential are required.");
        var related = document.References.FirstOrDefault() ?? throw new FinancialApplicationException("sri.credit_note.reference.required", "Credit note related document is required.");
        var detalles = new XElement("detalles", document.Lines.Select(line => new XElement("detalle",
            new XElement("codigoInterno", line.ProductCode),
            new XElement("descripcion", line.Description),
            new XElement("cantidad", Format(line.Quantity)),
            new XElement("precioUnitario", Format(line.UnitPrice)),
            new XElement("descuento", Format(line.Discount)),
            new XElement("precioTotalSinImpuesto", Format(line.Subtotal)))));
        var infoNotaCredito = new XElement("infoNotaCredito",
            new XElement("fechaEmision", document.IssueDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)),
            new XElement("dirEstablecimiento", issuer.Address),
            new XElement("tipoIdentificacionComprador", document.CustomerIdentificationType),
            new XElement("razonSocialComprador", document.CustomerName),
            new XElement("identificacionComprador", document.CustomerIdentification),
            new XElement("codDocModificado", related.DocumentTypeCode),
            new XElement("numDocModificado", related.Number),
            new XElement("fechaEmisionDocSustento", related.IssueDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)),
            new XElement("totalSinImpuestos", Format(document.SubtotalWithoutTaxes)),
            new XElement("valorModificacion", Format(document.TotalAmount)),
            new XElement("moneda", document.Currency),
            new XElement("motivo", related.ReasonOrPeriod));
        var xml = new XElement("notaCredito", new XAttribute("id", "comprobante"), new XAttribute("version", "1.0.0"), InfoTributaria(document, issuer), infoNotaCredito, detalles);
        return new XDocument(new XDeclaration("1.0", "UTF-8", null), xml).ToString(SaveOptions.DisableFormatting);
    }

    public string GenerateDebitNoteXml(ElectronicDocument document, IssuerSriOptions issuer)
    {
        if (document.AccessKey is null || document.Sequential is null) throw new FinancialApplicationException("sri.xml.access_key.required", "Access key and sequential are required.");
        var related = document.References.FirstOrDefault() ?? throw new FinancialApplicationException("sri.debit_note.reference.required", "Debit note related document is required.");
        var motivos = new XElement("motivos", document.DebitNoteReasons.Select(x => new XElement("motivo",
            new XElement("razon", x.Reason),
            new XElement("valor", Format(x.Amount)))));
        var impuestos = new XElement("impuestos", document.Taxes.Select(x => new XElement("impuesto",
            new XElement("codigo", x.TaxCode),
            new XElement("codigoPorcentaje", x.TaxPercentageCode),
            new XElement("tarifa", Format(x.TaxRate)),
            new XElement("baseImponible", Format(x.TaxBase)),
            new XElement("valor", Format(x.TaxAmount)))));
        var infoNotaDebito = new XElement("infoNotaDebito",
            new XElement("fechaEmision", document.IssueDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)),
            new XElement("dirEstablecimiento", issuer.Address),
            new XElement("tipoIdentificacionComprador", document.CustomerIdentificationType),
            new XElement("razonSocialComprador", document.CustomerName),
            new XElement("identificacionComprador", document.CustomerIdentification),
            new XElement("codDocModificado", related.DocumentTypeCode),
            new XElement("numDocModificado", related.Number),
            new XElement("fechaEmisionDocSustento", related.IssueDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)),
            new XElement("totalSinImpuestos", Format(document.SubtotalWithoutTaxes)),
            new XElement("valorTotal", Format(document.TotalAmount)));
        var xml = new XElement("notaDebito", new XAttribute("id", "comprobante"), new XAttribute("version", "1.0.0"), InfoTributaria(document, issuer), infoNotaDebito, motivos, impuestos);
        return new XDocument(new XDeclaration("1.0", "UTF-8", null), xml).ToString(SaveOptions.DisableFormatting);
    }

    public string GenerateWithholdingXml(ElectronicDocument document, IssuerSriOptions issuer)
    {
        if (document.AccessKey is null || document.Sequential is null) throw new FinancialApplicationException("sri.xml.access_key.required", "Access key and sequential are required.");
        var related = document.References.FirstOrDefault() ?? throw new FinancialApplicationException("sri.withholding.support.required", "Withholding support document is required.");
        var impuestos = new XElement("impuestos", document.WithholdingTaxes.Select(x => new XElement("impuesto",
            new XElement("codigo", x.TaxCode),
            new XElement("codigoRetencion", x.WithholdingCode),
            new XElement("baseImponible", Format(x.TaxBase)),
            new XElement("porcentajeRetener", Format(x.WithholdingPercentage)),
            new XElement("valorRetenido", Format(x.WithheldAmount)),
            new XElement("codDocSustento", related.DocumentTypeCode),
            new XElement("numDocSustento", x.SupportDocumentNumber),
            new XElement("fechaEmisionDocSustento", x.SupportDocumentIssueDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)))));
        var infoCompRetencion = new XElement("infoCompRetencion",
            new XElement("fechaEmision", document.IssueDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)),
            new XElement("dirEstablecimiento", issuer.Address),
            new XElement("obligadoContabilidad", issuer.AccountingRequired ? "SI" : "NO"),
            new XElement("tipoIdentificacionSujetoRetenido", document.CustomerIdentificationType),
            new XElement("razonSocialSujetoRetenido", document.CustomerName),
            new XElement("identificacionSujetoRetenido", document.CustomerIdentification),
            new XElement("periodoFiscal", related.ReasonOrPeriod));
        var docsSustento = new XElement("docsSustento", new XElement("docSustento",
            new XElement("codSustento", related.DocumentTypeCode),
            new XElement("codDocSustento", related.DocumentTypeCode),
            new XElement("numDocSustento", related.Number),
            new XElement("fechaEmisionDocSustento", related.IssueDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture))));
        var xml = new XElement("comprobanteRetencion", new XAttribute("id", "comprobante"), new XAttribute("version", "1.0.0"), InfoTributaria(document, issuer), infoCompRetencion, impuestos, docsSustento);
        return new XDocument(new XDeclaration("1.0", "UTF-8", null), xml).ToString(SaveOptions.DisableFormatting);
    }

    private static XElement InfoTributaria(ElectronicDocument document, IssuerSriOptions issuer) => new("infoTributaria",
        new XElement("ambiente", ((int)document.Environment).ToString(CultureInfo.InvariantCulture)),
        new XElement("tipoEmision", ((int)document.EmissionType).ToString(CultureInfo.InvariantCulture)),
        new XElement("razonSocial", issuer.LegalName),
        new XElement("nombreComercial", issuer.TradeName ?? issuer.LegalName),
        new XElement("ruc", issuer.Ruc),
        new XElement("claveAcceso", document.AccessKey),
        new XElement("codDoc", SriDocumentCodes.ToCode(document.DocumentType)),
        new XElement("estab", document.EstablishmentCode),
        new XElement("ptoEmi", document.EmissionPointCode),
        new XElement("secuencial", document.Sequential),
        new XElement("dirMatriz", issuer.Address));

    private static string Format(decimal value) => FinancialPrecision.Normalize(value).ToString("0.00##", CultureInfo.InvariantCulture);
}

public sealed class DevelopmentElectronicSignatureService : IElectronicSignatureService
{
    public Task<SignatureResult> SignAsync(string unsignedXml, SignatureContext context, CancellationToken ct)
    {
        if (context.Provider == SignatureProviderType.Disabled) throw new FinancialApplicationException("sri.signature.disabled", "Electronic signature provider is disabled.");
        if (context.Provider == SignatureProviderType.Development && string.Equals(context.EnvironmentName, "Production", StringComparison.OrdinalIgnoreCase))
            throw new FinancialApplicationException("sri.signature.development.production", "Development signature provider is not allowed in Production.");
        if (context.Provider == SignatureProviderType.LocalCertificatePlaceholder)
            throw new FinancialApplicationException("sri.signature.local_certificate.not_implemented", "Local certificate signature is not implemented. Use secure secret storage before enabling it.");
        if (context.Provider == SignatureProviderType.External)
            throw new FinancialApplicationException("sri.signature.external.not_configured", "External signature provider port is defined but no production adapter is configured.");
        if (context.Provider == SignatureProviderType.Xades)
            throw new FinancialApplicationException("sri.signature.xades.adapter_required", "XAdES provider requires the XadesElectronicSignatureService and a secure certificate provider.");
        var signed = unsignedXml.Contains("</factura>", StringComparison.Ordinal)
            ? unsignedXml.Replace("</factura>", "<firmaSimulada proveedor=\"Development\" /></factura>", StringComparison.Ordinal)
            : unsignedXml.Contains("</notaCredito>", StringComparison.Ordinal)
                ? unsignedXml.Replace("</notaCredito>", "<firmaSimulada proveedor=\"Development\" /></notaCredito>", StringComparison.Ordinal)
                : unsignedXml.Contains("</notaDebito>", StringComparison.Ordinal)
                    ? unsignedXml.Replace("</notaDebito>", "<firmaSimulada proveedor=\"Development\" /></notaDebito>", StringComparison.Ordinal)
                    : unsignedXml.Replace("</comprobanteRetencion>", "<firmaSimulada proveedor=\"Development\" /></comprobanteRetencion>", StringComparison.Ordinal);
        var digest = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(signed)));
        return Task.FromResult(new SignatureResult(signed, context.Provider.ToString(), context.CertificateSecretName, DateTimeOffset.UtcNow, digest, "DevelopmentSimulation"));
    }
}

public sealed class DevelopmentCertificateProvider : ICertificateProvider
{
    public Task<CertificateLoadResult> LoadAsync(SignatureContext context, CancellationToken ct)
    {
        if (string.Equals(context.EnvironmentName, "Production", StringComparison.OrdinalIgnoreCase))
            return Task.FromResult(new CertificateLoadResult(false, null, "sri.certificate.development.production", "Development certificate provider is not allowed in Production."));
        return Task.FromResult(new CertificateLoadResult(true, new("development-certificate", "Development", true, false, null), null, null));
    }
}

public sealed class DevelopmentSecretStoreClient(string environmentName = "Development", bool allowDevelopmentSecrets = false) : ISecretStoreClient
{
    public Task<SecretStoreResult> GetSecretAsync(SecretReference reference, PortalCallContext context, CancellationToken ct)
    {
        if (!string.Equals(environmentName, "Development", StringComparison.OrdinalIgnoreCase))
            return Task.FromResult(new SecretStoreResult(false, null, null, "secret_store.development.production_blocked", "Development secret store is not allowed outside Development."));
        if (!allowDevelopmentSecrets)
            return Task.FromResult(new SecretStoreResult(false, null, null, "secret_store.development.disabled", "Development secret reads require explicit configuration."));
        var value = Environment.GetEnvironmentVariable(reference.Name);
        if (string.IsNullOrEmpty(value))
            return Task.FromResult(new SecretStoreResult(false, null, null, "secret_store.secret.not_found", "Configured secret reference was not found."));
        return Task.FromResult(new SecretStoreResult(true, new(Encoding.UTF8.GetBytes(value), "text/plain"), new(reference.Name, "DevelopmentEnvironment", null), null, null));
    }
}

public sealed class AzureKeyVaultSecretStoreClientPlaceholder(AzureKeyVaultSecretStoreOptions options) : ISecretStoreClient
{
    public Task<SecretStoreResult> GetSecretAsync(SecretReference reference, PortalCallContext context, CancellationToken ct)
    {
        var readiness = CheckReadiness(options, reference);
        if (readiness.Status == "Unhealthy")
            return Task.FromResult(new SecretStoreResult(false, null, null, "secret_store.azure_keyvault.missing_configuration", string.Join("; ", readiness.Issues)));
        return Task.FromResult(new SecretStoreResult(false, null, new(SecretMaskingHelper.Mask(reference.Name) ?? "secret", "AzureKeyVault", null), "secret_store.azure_keyvault.placeholder", "Azure Key Vault wiring is ready, but SDK credential execution is disabled until explicit operational validation."));
    }

    public static SecretStoreReadinessResult CheckReadiness(AzureKeyVaultSecretStoreOptions options, SecretReference? reference = null)
    {
        var checks = new List<string> { "provider=AzureKeyVault", $"useDefaultAzureCredential={options.UseDefaultAzureCredential}", $"requireManagedIdentity={options.RequireManagedIdentity}", $"failFastOnStartup={options.FailFastOnStartup}" };
        var issues = new List<string>();
        if (string.IsNullOrWhiteSpace(options.KeyVaultName)) issues.Add("Key Vault name is required.");
        if (reference is not null && string.IsNullOrWhiteSpace(reference.Name)) issues.Add("Secret reference name is required.");
        if (options.RequireManagedIdentity && !options.UseDefaultAzureCredential) issues.Add("Managed identity requires DefaultAzureCredential or an external credential chain.");
        return new(issues.Count == 0 ? "Degraded" : "Unhealthy", "AzureKeyVault", checks, issues);
    }
}

public sealed class ExternalSecretStoreClientPlaceholder : ISecretStoreClient
{
    public Task<SecretStoreResult> GetSecretAsync(SecretReference reference, PortalCallContext context, CancellationToken ct) =>
        Task.FromResult(new SecretStoreResult(false, null, null, "secret_store.external.not_configured", "External secret store adapter is not configured."));
}

public sealed class DisabledSecretStoreClient : ISecretStoreClient
{
    public Task<SecretStoreResult> GetSecretAsync(SecretReference reference, PortalCallContext context, CancellationToken ct) =>
        Task.FromResult(new SecretStoreResult(false, null, null, "secret_store.disabled", "Secret store is disabled."));
}

public sealed class ConfiguredSecretStoreClient(IFinancialConfigurationReader configuration) : ISecretStoreClient
{
    public async Task<SecretStoreResult> GetSecretAsync(SecretReference reference, PortalCallContext context, CancellationToken ct)
    {
        var providerText = await configuration.GetStringAsync("financial.secrets.provider", "Disabled", context, ct);
        var environment = await configuration.GetStringAsync("ASPNETCORE_ENVIRONMENT", "Development", context, ct);
        var keyVault = await configuration.GetStringAsync("financial.secrets.keyVaultName", reference.VaultName ?? "", context, ct);
        var useDefaultAzureCredential = await configuration.GetBoolAsync("financial.secrets.useDefaultAzureCredential", false, context, ct);
        var requireManagedIdentity = await configuration.GetBoolAsync("financial.secrets.requireManagedIdentity", false, context, ct);
        var failFastOnStartup = await configuration.GetBoolAsync("financial.secrets.failFastOnStartup", false, context, ct);
        var allowDev = await configuration.GetBoolAsync("financial.secrets.allowDevelopmentSecrets", false, context, ct);
        var provider = Enum.TryParse<SecretStoreProviderType>(providerText, true, out var parsed) ? parsed : SecretStoreProviderType.Disabled;
        ISecretStoreClient client = provider switch
        {
            SecretStoreProviderType.Development => new DevelopmentSecretStoreClient(environment, allowDev),
            SecretStoreProviderType.AzureKeyVault => new AzureKeyVaultSecretStoreClientPlaceholder(new(keyVault, useDefaultAzureCredential, requireManagedIdentity, failFastOnStartup)),
            SecretStoreProviderType.External => new ExternalSecretStoreClientPlaceholder(),
            _ => new DisabledSecretStoreClient()
        };
        return await client.GetSecretAsync(reference, context, ct);
    }
}

public sealed class SecretStoreCertificateProvider(ISecretStoreClient secretStore) : ICertificateProvider
{
    public async Task<CertificateLoadResult> LoadAsync(SignatureContext context, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(context.CertificateSecretName))
            return new(false, null, "sri.certificate.secret_reference.required", "Certificate secret reference is required.");
        var provider = Enum.TryParse<SecretStoreProviderType>(context.CertificateSource, true, out var parsed) ? parsed : SecretStoreProviderType.Disabled;
        var secret = await secretStore.GetSecretAsync(new(context.CertificateSecretName, provider, context.KeyVaultName), new(context.TenantId, "certificate-provider"), ct);
        if (!secret.Success || secret.Value is null)
            return new(false, null, secret.ErrorCode ?? "sri.certificate.secret_unavailable", secret.ErrorMessage ?? "Certificate secret is unavailable.");
        return new(false, new(SecretMaskingHelper.Mask(context.CertificateSecretName) ?? "certificate", provider.ToString(), true, false, null), "sri.certificate.xades.not_enabled", "Certificate bytes were resolved in-memory, but real XAdES signing remains disabled in P4.");
    }
}

public sealed class KeyVaultCertificateProviderPlaceholder : ICertificateProvider
{
    public Task<CertificateLoadResult> LoadAsync(SignatureContext context, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(context.KeyVaultName) || string.IsNullOrWhiteSpace(context.CertificateSecretName))
            return Task.FromResult(new CertificateLoadResult(false, null, "sri.certificate.keyvault.missing_configuration", "Key Vault name and certificate secret name are required. No secret value is read or stored by this placeholder."));
        return Task.FromResult(new CertificateLoadResult(false, new(context.CertificateSecretName, "KeyVault", false, false, null), "sri.certificate.keyvault.placeholder", "Key Vault certificate loading is a documented placeholder in P3 and must be implemented through a secure secret store."));
    }
}

public sealed class LocalCertificateProviderPlaceholder : ICertificateProvider
{
    public Task<CertificateLoadResult> LoadAsync(SignatureContext context, CancellationToken ct)
    {
        if (string.Equals(context.EnvironmentName, "Production", StringComparison.OrdinalIgnoreCase))
            return Task.FromResult(new CertificateLoadResult(false, null, "sri.certificate.local.production_rejected", "Local certificate files are rejected in Production. Use Key Vault or an external signature service."));
        return Task.FromResult(new CertificateLoadResult(false, null, "sri.certificate.local.placeholder", "Local certificate loading is intentionally disabled. Do not commit .p12, .pfx, .key, .cer, .crt or .pem files."));
    }
}

public sealed class XadesElectronicSignatureService(ICertificateProvider certificateProvider) : IElectronicSignatureService
{
    public async Task<SignatureResult> SignAsync(string unsignedXml, SignatureContext context, CancellationToken ct)
    {
        if (context.Provider != SignatureProviderType.Xades)
            throw new FinancialApplicationException("sri.signature.xades.provider_required", "XAdES signature service requires provider Xades.");
        var certificate = await certificateProvider.LoadAsync(context, ct);
        if (!certificate.IsLoaded || certificate.Descriptor is null)
            throw new FinancialApplicationException(certificate.ErrorCode ?? "sri.certificate.unavailable", certificate.ErrorMessage ?? "A secure certificate is required for XAdES signing.");
        if (context.RequireTrustedCertificate && !certificate.Descriptor.IsTrusted)
            throw new FinancialApplicationException("sri.certificate.untrusted", "A trusted certificate is required for XAdES signing.");
        var digest = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(unsignedXml)));
        throw new FinancialApplicationException("sri.signature.xades.not_implemented", $"XAdES signing contract is ready, but productive signing is disabled. Unsigned digest: {digest}.");
    }
}

public sealed class DevelopmentSriReceptionClient(IFinancialConfigurationReader configuration) : ISriReceptionClient
{
    public async Task<SriReceptionResponse> SendAsync(SriReceptionRequest request, CancellationToken ct)
    {
        if (!string.Equals(request.Context.Mode, "Mock", StringComparison.OrdinalIgnoreCase) && !string.Equals(request.Context.Mode, "Development", StringComparison.OrdinalIgnoreCase))
            throw new FinancialApplicationException("sri.reception.real.not_configured", "SRI real/test reception client is not configured in P2.");
        var mode = await configuration.GetStringAsync("financial.sri.mock.receptionStatus", "Received", new PortalCallContext(request.Context.TenantId, "sri-reception-dev"), ct);
        return string.Equals(mode, "Rejected", StringComparison.OrdinalIgnoreCase)
            ? new(SriResponseStatus.Returned, "DEV-RETURNED", "Development SRI reception returned the document.", [new("DEV-RETURNED", "Mock returned")])
            : new(SriResponseStatus.Received, "DEV-RECEIVED", "Development SRI reception accepted the document.", []);
    }
}

public sealed class DevelopmentSriAuthorizationClient(IFinancialConfigurationReader configuration) : ISriAuthorizationClient
{
    public async Task<SriAuthorizationResponse> AuthorizeAsync(SriAuthorizationRequest request, CancellationToken ct)
    {
        if (!string.Equals(request.Context.Mode, "Mock", StringComparison.OrdinalIgnoreCase) && !string.Equals(request.Context.Mode, "Development", StringComparison.OrdinalIgnoreCase))
            throw new FinancialApplicationException("sri.authorization.real.not_configured", "SRI real/test authorization client is not configured in P2.");
        var mode = await configuration.GetStringAsync("financial.sri.mock.authorizationStatus", "Authorized", new PortalCallContext(request.Context.TenantId, "sri-authorization-dev"), ct);
        return string.Equals(mode, "Rejected", StringComparison.OrdinalIgnoreCase)
            ? new(SriResponseStatus.Rejected, "DEV-REJECTED", "Development SRI authorization rejected the document.", null, null, null, [new("DEV-REJECTED", "Mock rejected")])
            : new(SriResponseStatus.Authorized, "DEV-AUTHORIZED", "Development SRI authorization approved the document.", request.AccessKey, DateTimeOffset.UtcNow, $"<autorizacion><numeroAutorizacion>{request.AccessKey}</numeroAutorizacion></autorizacion>", []);
    }
}

public sealed class SriSoapClientPlaceholder : ISriReceptionClient, ISriAuthorizationClient
{
    public Task<SriReceptionResponse> SendAsync(SriReceptionRequest request, CancellationToken ct) =>
        Task.FromException<SriReceptionResponse>(new FinancialApplicationException("sri.soap.placeholder", "SRI SOAP test/production client is a placeholder and is not enabled."));
    public Task<SriAuthorizationResponse> AuthorizeAsync(SriAuthorizationRequest request, CancellationToken ct) =>
        Task.FromException<SriAuthorizationResponse>(new FinancialApplicationException("sri.soap.placeholder", "SRI SOAP test/production client is a placeholder and is not enabled."));
}

public sealed class SriSoapReceptionClient : ISriReceptionClient
{
    public Task<SriReceptionResponse> SendAsync(SriReceptionRequest request, CancellationToken ct)
    {
        Validate(request.Context, request.Context.ReceptionUrl, "sri.soap.reception.url_required");
        if (request.Context.DryRun)
            return Task.FromResult(new SriReceptionResponse(SriResponseStatus.Received, "SRI-TEST-DRY-RUN", "SRI Test dry-run validated reception configuration without sending XML.", []));
        return Task.FromException<SriReceptionResponse>(new FinancialApplicationException("sri.soap.reception.not_enabled", "SRI SOAP test reception contract is prepared but the real HTTP adapter is disabled in P3."));
    }

    internal static void Validate(SriClientContext context, string? url, string missingUrlCode)
    {
        if (string.Equals(context.Mode, "Production", StringComparison.OrdinalIgnoreCase) && !context.AllowProduction)
            throw new FinancialApplicationException("sri.soap.production.disabled", "SRI Production integration is disabled by configuration.");
        if (string.IsNullOrWhiteSpace(url)) throw new FinancialApplicationException(missingUrlCode, "SRI SOAP URL must be configured for Test/Production mode.");
        if (!Uri.TryCreate(url, UriKind.Absolute, out _)) throw new FinancialApplicationException("sri.soap.url.invalid", "SRI SOAP URL must be absolute and come from configuration.");
        if (context.LogPayloads && (context.Environment == SriEnvironment.Production || !context.MaskPayloads)) throw new FinancialApplicationException("sri.soap.payload_logging.disabled", "Full XML payload logging is disabled to avoid leaking tax data.");
    }
}

public sealed class SriSoapAuthorizationClient : ISriAuthorizationClient
{
    public Task<SriAuthorizationResponse> AuthorizeAsync(SriAuthorizationRequest request, CancellationToken ct)
    {
        SriSoapReceptionClient.Validate(request.Context, request.Context.AuthorizationUrl, "sri.soap.authorization.url_required");
        if (request.Context.DryRun)
            return Task.FromResult(new SriAuthorizationResponse(SriResponseStatus.Processing, "SRI-TEST-DRY-RUN", "SRI Test dry-run validated authorization configuration without calling SRI.", null, null, null, []));
        return Task.FromException<SriAuthorizationResponse>(new FinancialApplicationException("sri.soap.authorization.not_enabled", "SRI SOAP test authorization contract is prepared but the real HTTP adapter is disabled in P3."));
    }
}

public static class SriSoapResponseParser
{
    public static SriResponseStatus ParseStatus(string? value) => (value ?? "").Trim().ToUpperInvariant() switch
    {
        "RECIBIDA" or "RECEIVED" => SriResponseStatus.Received,
        "DEVUELTA" or "RETURNED" => SriResponseStatus.Returned,
        "EN PROCESO" or "PROCESSING" => SriResponseStatus.Processing,
        "AUTORIZADO" or "AUTHORIZED" => SriResponseStatus.Authorized,
        "NO AUTORIZADO" or "RECHAZADO" or "REJECTED" => SriResponseStatus.Rejected,
        "" => SriResponseStatus.NotFound,
        _ => SriResponseStatus.Error
    };
}

public sealed class ElectronicDocumentXmlValidator : IElectronicDocumentXmlValidator
{
    public XmlValidationResult ValidateXml(ElectronicDocumentType type, string xml) => type switch
    {
        ElectronicDocumentType.Invoice => ValidateInvoiceXml(xml),
        ElectronicDocumentType.CreditNote => ValidateCreditNoteXml(xml),
        ElectronicDocumentType.DebitNote => ValidateDebitNoteXml(xml),
        ElectronicDocumentType.Withholding => ValidateWithholdingXml(xml),
        _ => new(false, ["Unsupported SRI document type."], [])
    };

    public XmlValidationResult ValidateInvoiceXml(string xml)
    {
        var result = ValidateBase(xml, "factura", "01");
        if (!result.IsValid) return result;
        var errors = result.Errors.ToList();
        var root = XDocument.Parse(xml).Root!;
        var infoFactura = root.Element("infoFactura");
        if (infoFactura is null) errors.Add("infoFactura is required.");
        var detalles = root.Element("detalles");
        if (detalles is null || !detalles.Elements("detalle").Any()) errors.Add("detalles with at least one detalle is required.");
        ValidateDecimal(infoFactura, "totalSinImpuestos", errors);
        ValidateDecimal(infoFactura, "importeTotal", errors);
        return new(errors.Count == 0, errors, result.Warnings);
    }

    public XmlValidationResult ValidateCreditNoteXml(string xml)
    {
        var result = ValidateBase(xml, "notaCredito", "04");
        if (!result.IsValid) return result;
        var errors = result.Errors.ToList();
        var root = XDocument.Parse(xml).Root!;
        var info = root.Element("infoNotaCredito");
        if (info is null) errors.Add("infoNotaCredito is required.");
        var detalles = root.Element("detalles");
        if (detalles is null || !detalles.Elements("detalle").Any()) errors.Add("detalles with at least one detalle is required.");
        Require(info, "codDocModificado", errors);
        Require(info, "numDocModificado", errors);
        Require(info, "fechaEmisionDocSustento", errors);
        Require(info, "motivo", errors);
        ValidateDecimal(info, "totalSinImpuestos", errors);
        ValidateDecimal(info, "valorModificacion", errors);
        ValidatePositiveDecimal(info, "valorModificacion", errors);
        ValidateRelatedDocumentType(info, "codDocModificado", errors);
        ValidateDocumentNumber(info, "numDocModificado", errors);
        ValidateNonEmptyMaxLength(info, "motivo", 300, errors);
        return new(errors.Count == 0, errors, result.Warnings);
    }

    public XmlValidationResult ValidateDebitNoteXml(string xml)
    {
        var result = ValidateBase(xml, "notaDebito", "05");
        if (!result.IsValid) return result;
        var errors = result.Errors.ToList();
        var root = XDocument.Parse(xml).Root!;
        var info = root.Element("infoNotaDebito");
        if (info is null) errors.Add("infoNotaDebito is required.");
        var motivos = root.Element("motivos");
        if (motivos is null || !motivos.Elements("motivo").Any()) errors.Add("motivos with at least one motivo is required.");
        Require(info, "codDocModificado", errors);
        Require(info, "numDocModificado", errors);
        Require(info, "fechaEmisionDocSustento", errors);
        ValidateDecimal(info, "totalSinImpuestos", errors);
        ValidateDecimal(info, "valorTotal", errors);
        ValidatePositiveDecimal(info, "valorTotal", errors);
        ValidateRelatedDocumentType(info, "codDocModificado", errors);
        ValidateDocumentNumber(info, "numDocModificado", errors);
        foreach (var motivo in motivos?.Elements("motivo") ?? [])
        {
            ValidateNonEmptyMaxLength(motivo, "razon", 300, errors);
            ValidatePositiveDecimal(motivo, "valor", errors);
        }
        return new(errors.Count == 0, errors, result.Warnings);
    }

    public XmlValidationResult ValidateWithholdingXml(string xml)
    {
        var result = ValidateBase(xml, "comprobanteRetencion", "07");
        if (!result.IsValid) return result;
        var errors = result.Errors.ToList();
        var root = XDocument.Parse(xml).Root!;
        var info = root.Element("infoCompRetencion");
        if (info is null) errors.Add("infoCompRetencion is required.");
        var impuestos = root.Element("impuestos");
        if (impuestos is null || !impuestos.Elements("impuesto").Any()) errors.Add("impuestos with at least one impuesto is required.");
        Require(info, "tipoIdentificacionSujetoRetenido", errors);
        Require(info, "identificacionSujetoRetenido", errors);
        Require(info, "periodoFiscal", errors);
        ValidateFiscalPeriod(info, "periodoFiscal", errors);
        foreach (var impuesto in impuestos?.Elements("impuesto") ?? [])
        {
            ValidateTaxCode(impuesto, "codigo", errors);
            ValidateWithholdingCode(impuesto, "codigoRetencion", errors);
            ValidatePositiveDecimal(impuesto, "baseImponible", errors);
            ValidatePositiveDecimal(impuesto, "porcentajeRetener", errors);
            ValidateDecimal(impuesto, "valorRetenido", errors);
            ValidateRelatedDocumentType(impuesto, "codDocSustento", errors);
            ValidateDocumentNumber(impuesto, "numDocSustento", errors);
        }
        return new(errors.Count == 0, errors, result.Warnings);
    }

    private static XmlValidationResult ValidateBase(string xml, string expectedRoot, string expectedCodDoc)
    {
        var errors = new List<string>();
        try
        {
            var document = XDocument.Parse(xml);
            var root = document.Root;
            if (root?.Name.LocalName != expectedRoot) errors.Add($"Root element must be {expectedRoot}.");
            var infoTributaria = root?.Element("infoTributaria");
            if (infoTributaria is null) errors.Add("infoTributaria is required.");
            var accessKey = infoTributaria?.Element("claveAcceso")?.Value;
            if (string.IsNullOrWhiteSpace(accessKey) || accessKey.Length != 49 || !accessKey.All(char.IsDigit)) errors.Add("claveAcceso must have 49 digits.");
            if (accessKey?.Length >= 10 && accessKey[8..10] != expectedCodDoc) errors.Add("claveAcceso document code does not match codDoc.");
            if (accessKey?.Length >= 23 && infoTributaria?.Element("ruc")?.Value is { } xmlRuc && accessKey[10..23] != xmlRuc) errors.Add("claveAcceso RUC does not match infoTributaria/ruc.");
            if (accessKey?.Length >= 24 && infoTributaria?.Element("ambiente")?.Value is { } xmlAmbiente && accessKey[23..24] != xmlAmbiente) errors.Add("claveAcceso ambiente does not match infoTributaria/ambiente.");
            ValidateCode(infoTributaria, "ambiente", ["1", "2"], "ambiente must be 1 or 2.", errors);
            ValidateCode(infoTributaria, "tipoEmision", ["1", "2"], "tipoEmision must be 1 or 2.", errors);
            ValidateCode(infoTributaria, "codDoc", [expectedCodDoc], $"codDoc must be {expectedCodDoc}.", errors);
            ValidateFixed(infoTributaria, "ruc", 13, "ruc must have 13 digits.", errors);
            ValidateFixed(infoTributaria, "estab", 3, "estab must have 3 digits.", errors);
            ValidateFixed(infoTributaria, "ptoEmi", 3, "ptoEmi must have 3 digits.", errors);
            ValidateFixed(infoTributaria, "secuencial", 9, "secuencial must have 9 digits.", errors);
            if (!string.IsNullOrWhiteSpace(accessKey) && accessKey.Length == 49)
            {
                if (infoTributaria?.Element("codDoc")?.Value is { } codDoc && accessKey[8..10] != codDoc) errors.Add("claveAcceso codDoc does not match infoTributaria.");
                if (infoTributaria?.Element("ruc")?.Value is { } ruc && accessKey[10..23] != ruc) errors.Add("claveAcceso ruc does not match infoTributaria.");
                if (infoTributaria?.Element("ambiente")?.Value is { } ambiente && accessKey[23].ToString() != ambiente) errors.Add("claveAcceso ambiente does not match infoTributaria.");
                if (infoTributaria?.Element("estab")?.Value is { } estab && infoTributaria.Element("ptoEmi")?.Value is { } ptoEmi && accessKey[24..30] != estab + ptoEmi) errors.Add("claveAcceso serie does not match infoTributaria.");
                if (infoTributaria?.Element("secuencial")?.Value is { } secuencial && accessKey[30..39] != secuencial) errors.Add("claveAcceso secuencial does not match infoTributaria.");
            }
        }
        catch (Exception ex) when (ex is System.Xml.XmlException or InvalidOperationException)
        {
            errors.Add("XML is not well formed.");
        }
        return new(errors.Count == 0, errors, []);
    }

    private static void Require(XElement? parent, string name, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(parent?.Element(name)?.Value)) errors.Add($"{name} is required.");
    }

    private static void ValidateCode(XElement? parent, string name, string[] allowed, string message, List<string> errors)
    {
        var value = parent?.Element(name)?.Value;
        if (string.IsNullOrWhiteSpace(value) || !allowed.Contains(value)) errors.Add(message);
    }

    private static void ValidateFixed(XElement? parent, string name, int length, string message, List<string> errors)
    {
        var value = parent?.Element(name)?.Value;
        if (string.IsNullOrWhiteSpace(value) || value.Length != length || !value.All(char.IsDigit)) errors.Add(message);
    }

    private static void ValidateDecimal(XElement? parent, string name, List<string> errors)
    {
        var value = parent?.Element(name)?.Value;
        if (string.IsNullOrWhiteSpace(value)) { errors.Add($"{name} is required."); return; }
        if (!decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out _)) errors.Add($"{name} must be numeric.");
    }
    private static void ValidatePositiveDecimal(XElement? parent, string name, List<string> errors)
    {
        var value = parent?.Element(name)?.Value;
        if (string.IsNullOrWhiteSpace(value)) { errors.Add($"{name} is required."); return; }
        if (!decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var number) || number <= 0) errors.Add($"{name} must be greater than zero.");
    }
    private static void ValidateRelatedDocumentType(XElement? parent, string name, List<string> errors)
    {
        var value = parent?.Element(name)?.Value;
        if (string.IsNullOrWhiteSpace(value) || !SriCatalogService.IsSupportDocumentTypeAllowed(value)) errors.Add($"{name} is not allowed by foundation catalog.");
    }
    private static void ValidateTaxCode(XElement? parent, string name, List<string> errors)
    {
        var value = parent?.Element(name)?.Value;
        if (string.IsNullOrWhiteSpace(value) || !SriCatalogService.IsTaxCodeAllowed(value)) errors.Add($"{name} is not allowed by foundation catalog.");
    }
    private static void ValidateWithholdingCode(XElement? parent, string name, List<string> errors)
    {
        var value = parent?.Element(name)?.Value;
        if (string.IsNullOrWhiteSpace(value) || !SriCatalogService.IsWithholdingCodeAllowed(value)) errors.Add($"{name} is not allowed by foundation catalog.");
    }
    private static void ValidateDocumentNumber(XElement? parent, string name, List<string> errors)
    {
        try { SriTaxRuleValidator.ValidateDocumentNumber(parent?.Element(name)?.Value ?? ""); }
        catch (FinancialDomainException ex) { errors.Add($"{name}: {ex.Message}"); }
    }
    private static void ValidateFiscalPeriod(XElement? parent, string name, List<string> errors)
    {
        try { SriTaxRuleValidator.ValidateFiscalPeriod(parent?.Element(name)?.Value ?? ""); }
        catch (FinancialDomainException ex) { errors.Add($"{name}: {ex.Message}"); }
    }
    private static void ValidateNonEmptyMaxLength(XElement? parent, string name, int maxLength, List<string> errors)
    {
        var value = parent?.Element(name)?.Value;
        if (string.IsNullOrWhiteSpace(value)) errors.Add($"{name} is required.");
        else if (value.Length > maxLength) errors.Add($"{name} must be {maxLength} characters or fewer.");
    }
}

public sealed class XsdSchemaValidatorPlaceholder : IXsdSchemaValidator
{
    public XmlValidationResult Validate(string xml, string schemaName) =>
        new(false, [$"XSD schema validation mode is not enabled because schema '{schemaName}' is not configured in P3."], []);
}

public sealed class DevelopmentElectronicDocumentStorageClient : IElectronicDocumentStorageClient
{
    public Task<StoredDocumentFile> SaveUnsignedXmlAsync(ElectronicDocument document, string xml, PortalCallContext context, CancellationToken ct) => SaveAsync("unsigned-xml", xml, "application/xml");
    public Task<StoredDocumentFile> SaveSignedXmlAsync(ElectronicDocument document, string xml, PortalCallContext context, CancellationToken ct) => SaveAsync("signed-xml", xml, "application/xml");
    public Task<StoredDocumentFile> SaveAuthorizationXmlAsync(ElectronicDocument document, string xml, PortalCallContext context, CancellationToken ct) => SaveAsync("authorization-xml", xml, "application/xml");
    public Task<StoredDocumentFile> SaveRidePdfAsync(ElectronicDocument document, byte[] pdf, PortalCallContext context, CancellationToken ct) =>
        Task.FromResult(new StoredDocumentFile($"dev://ride-pdf/{Guid.NewGuid():N}", Convert.ToHexString(SHA256.HashData(pdf)), "Development", DateTimeOffset.UtcNow, "application/pdf", "ride-pdf"));
    private static Task<StoredDocumentFile> SaveAsync(string purpose, string content, string contentType) =>
        Task.FromResult(new StoredDocumentFile($"dev://{purpose}/{Guid.NewGuid():N}", Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(content))), "Development", DateTimeOffset.UtcNow, contentType, purpose));
}

public sealed class PortalContentFileStorageClient(PortalContentFileOptions options) : IElectronicDocumentStorageClient
{
    public static PortalContentFileRequest BuildRequest(ElectronicDocument document, byte[] content, string purpose, string contentType, PortalCallContext context, PortalContentFileOptions options)
    {
        var hash = Convert.ToHexString(SHA256.HashData(content));
        return new(purpose, $"{document.AccessKey ?? document.Id.ToString("N")}-{purpose}", contentType, hash, options.Container, context.CorrelationId, context.TenantId,
            new Dictionary<string, string>
            {
                ["documentId"] = document.Id.ToString(),
                ["documentType"] = document.DocumentType.ToString(),
                ["status"] = document.Status.ToString(),
                ["accessKeyMasked"] = SriSensitiveDataSanitizer.MaskAccessKey(document.AccessKey) ?? "",
                ["hash"] = hash,
                ["purpose"] = purpose
            },
            options.SendPayloads,
            options.SendPayloads ? Convert.ToBase64String(content) : null);
    }

    public Task<StoredDocumentFile> SaveUnsignedXmlAsync(ElectronicDocument document, string xml, PortalCallContext context, CancellationToken ct) => SaveAsync(document, Encoding.UTF8.GetBytes(xml), "unsigned-xml", "application/xml", context);
    public Task<StoredDocumentFile> SaveSignedXmlAsync(ElectronicDocument document, string xml, PortalCallContext context, CancellationToken ct) => SaveAsync(document, Encoding.UTF8.GetBytes(xml), "signed-xml", "application/xml", context);
    public Task<StoredDocumentFile> SaveAuthorizationXmlAsync(ElectronicDocument document, string xml, PortalCallContext context, CancellationToken ct) => SaveAsync(document, Encoding.UTF8.GetBytes(xml), "authorization-xml", "application/xml", context);
    public Task<StoredDocumentFile> SaveRidePdfAsync(ElectronicDocument document, byte[] pdf, PortalCallContext context, CancellationToken ct) => SaveAsync(document, pdf, "ride-pdf", "application/pdf", context);

    private Task<StoredDocumentFile> SaveAsync(ElectronicDocument document, byte[] content, string purpose, string contentType, PortalCallContext context)
    {
        if (!string.Equals(options.Provider, "PortalContentFile", StringComparison.OrdinalIgnoreCase))
            throw new FinancialApplicationException("sri.storage.provider.invalid", "PortalContentFileStorageClient requires provider PortalContentFile.");
        if (string.IsNullOrWhiteSpace(options.PortalBaseUrl))
            throw new FinancialApplicationException("sri.storage.portal_base_url.required", "Portal Content/File base URL is required when storage provider is PortalContentFile.");
        var request = BuildRequest(document, content, purpose, contentType, context, options);
        var storageId = $"portal-content-file://{options.Container}/{document.TenantId}/{document.Id}/{purpose}";
        return Task.FromResult(new StoredDocumentFile(storageId, request.Hash, "PortalContentFile", DateTimeOffset.UtcNow, contentType, purpose));
    }
}

public sealed class ConfiguredElectronicDocumentStorageClient(IFinancialConfigurationReader configuration) : IElectronicDocumentStorageClient
{
    private readonly DevelopmentElectronicDocumentStorageClient _development = new();
    public async Task<StoredDocumentFile> SaveUnsignedXmlAsync(ElectronicDocument document, string xml, PortalCallContext context, CancellationToken ct) => await ResolveAsync(context, ct).SaveUnsignedXmlAsync(document, xml, context, ct);
    public async Task<StoredDocumentFile> SaveSignedXmlAsync(ElectronicDocument document, string xml, PortalCallContext context, CancellationToken ct) => await ResolveAsync(context, ct).SaveSignedXmlAsync(document, xml, context, ct);
    public async Task<StoredDocumentFile> SaveAuthorizationXmlAsync(ElectronicDocument document, string xml, PortalCallContext context, CancellationToken ct) => await ResolveAsync(context, ct).SaveAuthorizationXmlAsync(document, xml, context, ct);
    public async Task<StoredDocumentFile> SaveRidePdfAsync(ElectronicDocument document, byte[] pdf, PortalCallContext context, CancellationToken ct) => await ResolveAsync(context, ct).SaveRidePdfAsync(document, pdf, context, ct);

    private IElectronicDocumentStorageClient ResolveAsync(PortalCallContext context, CancellationToken ct)
    {
        var provider = configuration.GetStringAsync("financial.sri.storage.provider", "Development", context, ct).GetAwaiter().GetResult();
        if (string.Equals(provider, "Development", StringComparison.OrdinalIgnoreCase)) return _development;
        if (string.Equals(provider, "Disabled", StringComparison.OrdinalIgnoreCase)) throw new FinancialApplicationException("sri.storage.disabled", "Electronic document storage is disabled.");
        if (string.Equals(provider, "PortalContentFile", StringComparison.OrdinalIgnoreCase))
        {
            var options = new PortalContentFileOptions(provider,
                configuration.GetStringAsync("financial.sri.storage.portalBaseUrl", "", context, ct).GetAwaiter().GetResult(),
                configuration.GetStringAsync("financial.sri.storage.container", "financial-electronic-documents", context, ct).GetAwaiter().GetResult(),
                configuration.GetIntAsync("financial.sri.storage.timeoutSeconds", 30, context, ct).GetAwaiter().GetResult(),
                configuration.GetBoolAsync("financial.sri.storage.retainXml", true, context, ct).GetAwaiter().GetResult(),
                configuration.GetBoolAsync("financial.sri.storage.retainPdf", false, context, ct).GetAwaiter().GetResult(),
                configuration.GetBoolAsync("financial.sri.storage.sendPayloads", false, context, ct).GetAwaiter().GetResult(),
                configuration.GetBoolAsync("financial.sri.storage.maskPayloads", true, context, ct).GetAwaiter().GetResult());
            return new PortalContentFileStorageClient(options);
        }
        throw new FinancialApplicationException("sri.storage.provider.unsupported", $"Unsupported electronic document storage provider '{provider}'.");
    }
}

public sealed class DevelopmentRidePdfGenerator : IRidePdfGenerator
{
    public Task<RidePdfGenerationResult> GenerateInvoiceRideAsync(InvoiceRideModel model, CancellationToken ct)
    {
        var html = $"""
<html><body><h1>RIDE Factura</h1><p>Emisor: {HtmlEncoder.Default.Encode(model.IssuerName)} - {model.IssuerRuc}</p><p>Documento: {model.DocumentNumber}</p><p>Clave acceso: {model.AccessKey}</p><p>Cliente: {HtmlEncoder.Default.Encode(model.CustomerName)} - {model.CustomerIdentification}</p><p>Fecha: {model.IssueDate:yyyy-MM-dd}</p><p>Subtotal: {model.Subtotal:0.00}</p><p>Impuestos: {model.Taxes:0.00}</p><p>Total: {model.Total:0.00}</p><p>Autorizacion: {model.AuthorizationNumber ?? "PENDIENTE"}</p><p>Ambiente: {model.Environment}</p><p>Estado: {model.Status}</p></body></html>
""";
        var bytes = Encoding.UTF8.GetBytes("%PDF-DEV-RIDE-PLACEHOLDER\n" + html);
        return Task.FromResult(new RidePdfGenerationResult(bytes, html, Convert.ToHexString(SHA256.HashData(bytes)), DateTimeOffset.UtcNow, "application/pdf"));
    }
}

public static class SecretMaskingHelper
{
    public static string? Mask(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return value;
        var trimmed = value.Trim();
        return trimmed.Length <= 4 ? "****" : $"{trimmed[..2]}***{trimmed[^2..]}";
    }
}

public static class SriSensitiveDataSanitizer
{
    public static string? MaskAccessKey(string? accessKey) => MaskDigits(accessKey, 4);
    public static string? MaskCustomerIdentification(string? identification) => MaskDigits(identification, 4);
    public static string? MaskUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return url;
        return Uri.TryCreate(url, UriKind.Absolute, out var uri) ? $"{uri.Scheme}://{uri.Host}/***" : SecretMaskingHelper.Mask(url);
    }
    public static string MaskXmlPayload(string? xml) => string.IsNullOrWhiteSpace(xml) ? "" : $"<xml redacted=\"true\" sha256=\"{Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(xml)))[..16]}\" />";
    public static string SanitizeMessage(string? message)
    {
        if (string.IsNullOrWhiteSpace(message)) return "";
        var sanitized = message.Replace("<", "[").Replace(">", "]");
        return sanitized.Length > 256 ? sanitized[..256] + "..." : sanitized;
    }

    private static string? MaskDigits(string? value, int visible)
    {
        if (string.IsNullOrWhiteSpace(value)) return value;
        var trimmed = value.Trim();
        if (trimmed.Length <= visible) return "****";
        return new string('*', Math.Max(0, trimmed.Length - visible)) + trimmed[^visible..];
    }
}

public static class SriIntegrationLogSanitizer
{
    public static SriObservabilityEvent Sanitize(SriObservabilityEvent value) =>
        value with
        {
            AccessKeyMasked = SriSensitiveDataSanitizer.MaskAccessKey(value.AccessKeyMasked),
            SanitizedMessage = SriSensitiveDataSanitizer.SanitizeMessage(value.SanitizedMessage)
        };

    public static IntegrationAttemptTelemetry Sanitize(IntegrationAttemptTelemetry value) =>
        value with { SanitizedMessage = SriSensitiveDataSanitizer.SanitizeMessage(value.SanitizedMessage) };
}

public sealed record FinancialCorrelationContext(string TenantId, string CorrelationId);

public sealed class SriManualTestConnectivityService(IFinancialConfigurationReader configuration)
{
    public async Task<SriConnectivityProbeResult> CheckAsync(PortalCallContext context, CancellationToken ct)
    {
        var mode = await configuration.GetStringAsync("financial.sri.integration.mode", "Development", context, ct);
        var allowProduction = await configuration.GetBoolAsync("financial.sri.allowProduction", false, context, ct);
        var dryRun = await configuration.GetBoolAsync("financial.sri.test.dryRun", true, context, ct);
        var allowProbe = await configuration.GetBoolAsync("financial.sri.test.allowConnectivityProbe", false, context, ct);
        var allowDocumentSend = await configuration.GetBoolAsync("financial.sri.test.allowDocumentSend", false, context, ct);
        var receptionUrl = await configuration.GetStringAsync("financial.sri.receptionUrl", "", context, ct);
        var authorizationUrl = await configuration.GetStringAsync("financial.sri.authorizationUrl", "", context, ct);

        if (string.Equals(mode, "Production", StringComparison.OrdinalIgnoreCase) && !allowProduction)
            return new(SriConnectivityMode.ProductionBlocked, "Blocked", "SRI Production connectivity is blocked.", SriSensitiveDataSanitizer.MaskUrl(receptionUrl), SriSensitiveDataSanitizer.MaskUrl(authorizationUrl), false);
        if (!string.Equals(mode, "Test", StringComparison.OrdinalIgnoreCase))
            return new(SriConnectivityMode.Mock, "Healthy", "Mock/Development mode does not require SRI Test connectivity.", null, null, false);
        if (string.IsNullOrWhiteSpace(receptionUrl) || string.IsNullOrWhiteSpace(authorizationUrl))
            return new(SriConnectivityMode.ManualRequired, "Degraded", "SRI Test URLs must be configured before manual validation.", SriSensitiveDataSanitizer.MaskUrl(receptionUrl), SriSensitiveDataSanitizer.MaskUrl(authorizationUrl), false);
        if (dryRun)
            return new(SriConnectivityMode.TestDryRun, "Degraded", "SRI Test dry-run is active; no document is sent.", SriSensitiveDataSanitizer.MaskUrl(receptionUrl), SriSensitiveDataSanitizer.MaskUrl(authorizationUrl), false);
        if (!allowProbe)
            return new(SriConnectivityMode.ManualRequired, "Degraded", "Connectivity probe requires explicit approval.", SriSensitiveDataSanitizer.MaskUrl(receptionUrl), SriSensitiveDataSanitizer.MaskUrl(authorizationUrl), false);
        return allowDocumentSend
            ? new(SriConnectivityMode.TestConnectivityProbe, "Degraded", "Connectivity probe is enabled; document send remains a manual operation.", SriSensitiveDataSanitizer.MaskUrl(receptionUrl), SriSensitiveDataSanitizer.MaskUrl(authorizationUrl), true)
            : new(SriConnectivityMode.TestSendDisabled, "Degraded", "Connectivity probe may run, but document send is disabled.", SriSensitiveDataSanitizer.MaskUrl(receptionUrl), SriSensitiveDataSanitizer.MaskUrl(authorizationUrl), false);
    }
}

public sealed record SriReadinessResult(string Status, IReadOnlyCollection<string> Checks, IReadOnlyCollection<string> Issues);

public sealed class SriIntegrationReadinessService(IFinancialConfigurationReader configuration)
{
    public async Task<SriReadinessResult> CheckAsync(PortalCallContext context, CancellationToken ct)
    {
        var checks = new List<string>();
        var issues = new List<string>();
        var environmentName = await configuration.GetStringAsync("ASPNETCORE_ENVIRONMENT", "Development", context, ct);
        var sriEnvironment = await configuration.GetStringAsync("financial.sri.environment", "Test", context, ct);
        var mode = await configuration.GetStringAsync("financial.sri.integration.mode", "Development", context, ct);
        var allowProduction = await configuration.GetBoolAsync("financial.sri.allowProduction", false, context, ct);
        var secretProvider = await configuration.GetStringAsync("financial.secrets.provider", "Disabled", context, ct);
        var signatureProvider = await configuration.GetStringAsync("financial.sri.signature.provider", "Development", context, ct);
        var storageProvider = await configuration.GetStringAsync("financial.sri.storage.provider", "Development", context, ct);
        var receptionUrl = await configuration.GetStringAsync("financial.sri.receptionUrl", "", context, ct);
        var authorizationUrl = await configuration.GetStringAsync("financial.sri.authorizationUrl", "", context, ct);
        var useDefaultAzureCredential = await configuration.GetBoolAsync("financial.secrets.useDefaultAzureCredential", false, context, ct);
        var requireManagedIdentity = await configuration.GetBoolAsync("financial.secrets.requireManagedIdentity", false, context, ct);
        var failFastOnStartup = await configuration.GetBoolAsync("financial.secrets.failFastOnStartup", false, context, ct);
        var keyVaultName = await configuration.GetStringAsync("financial.secrets.keyVaultName", "", context, ct);
        var dryRun = await configuration.GetBoolAsync("financial.sri.test.dryRun", true, context, ct);
        var allowProbe = await configuration.GetBoolAsync("financial.sri.test.allowConnectivityProbe", false, context, ct);
        var allowDocumentSend = await configuration.GetBoolAsync("financial.sri.test.allowDocumentSend", false, context, ct);
        var manualRequired = await configuration.GetBoolAsync("financial.sri.test.manualValidationRequired", true, context, ct);
        var logPayloads = await configuration.GetBoolAsync("financial.sri.logPayloads", false, context, ct);
        var maskPayloads = await configuration.GetBoolAsync("financial.sri.maskPayloads", true, context, ct);
        var localCertificatePath = await configuration.GetStringAsync("financial.sri.signature.localCertificatePath", "", context, ct);

        checks.Add($"mode={mode}");
        checks.Add($"secretStore={secretProvider}");
        checks.Add($"signature={signatureProvider}");
        checks.Add($"storage={storageProvider}");
        checks.Add($"sriEnvironment={sriEnvironment}");
        checks.Add($"dryRun={dryRun}");
        checks.Add($"manualValidationRequired={manualRequired}");
        checks.Add($"connectivityProbe={allowProbe}");
        checks.Add($"documentSend={allowDocumentSend}");

        if (string.Equals(sriEnvironment, "Production", StringComparison.OrdinalIgnoreCase) && !allowProduction) issues.Add("SRI Production is blocked because allowProduction=false.");
        if (string.Equals(mode, "Production", StringComparison.OrdinalIgnoreCase) && !allowProduction) issues.Add("Integration mode Production is blocked.");
        if (string.Equals(mode, "Test", StringComparison.OrdinalIgnoreCase) && (string.IsNullOrWhiteSpace(receptionUrl) || string.IsNullOrWhiteSpace(authorizationUrl))) issues.Add("SRI Test mode requires receptionUrl and authorizationUrl.");
        if (string.Equals(mode, "Test", StringComparison.OrdinalIgnoreCase) && manualRequired) issues.Add("SRI Test manual validation is required before enabling real send.");
        if (string.Equals(storageProvider, "PortalContentFile", StringComparison.OrdinalIgnoreCase) && string.IsNullOrWhiteSpace(await configuration.GetStringAsync("financial.sri.storage.portalBaseUrl", "", context, ct))) issues.Add("PortalContentFile storage requires portalBaseUrl.");
        if (string.Equals(environmentName, "Production", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(localCertificatePath)) issues.Add("Local certificate path is not allowed in Production.");
        if (string.Equals(secretProvider, "Development", StringComparison.OrdinalIgnoreCase) && string.Equals(environmentName, "Production", StringComparison.OrdinalIgnoreCase)) issues.Add("Development secret store is not allowed in Production.");
        if (string.Equals(secretProvider, "AzureKeyVault", StringComparison.OrdinalIgnoreCase))
        {
            var secretReadiness = AzureKeyVaultSecretStoreClientPlaceholder.CheckReadiness(new(keyVaultName, useDefaultAzureCredential, requireManagedIdentity, failFastOnStartup));
            checks.AddRange(secretReadiness.Checks.Select(x => $"secret.{x}"));
            issues.AddRange(secretReadiness.Issues.Select(x => $"SecretStore: {x}"));
        }
        if (logPayloads && !maskPayloads) issues.Add("Payload logging without masking is not allowed.");
        if (string.Equals(environmentName, "Production", StringComparison.OrdinalIgnoreCase) && logPayloads) issues.Add("Payload logging is not allowed in Production.");

        var status = issues.Count == 0 ? "Healthy" : string.Equals(mode, "Test", StringComparison.OrdinalIgnoreCase) ? "Degraded" : "Unhealthy";
        return new(status, checks, issues);
    }
}

public sealed class ElectronicDocumentsService(
    IElectronicDocumentRepository documents,
    IFinancialConfigurationReader configuration,
    IElectronicDocumentXmlGenerator xmlGenerator,
    IElectronicSignatureService signature,
    ISriReceptionClient reception,
    ISriAuthorizationClient authorization,
    IElectronicDocumentXmlValidator xmlValidator,
    IElectronicDocumentStorageClient storage,
    IRidePdfGenerator ridePdfGenerator,
    IPortalAuditClient audit,
    IPortalOutboxClient outbox)
{
    public async Task<ElectronicDocumentDto> CreateInvoiceDraftAsync(CreateInvoiceRequest request, PortalCallContext context, CancellationToken ct)
    {
        var environment = await GetEnvironmentAsync(context, ct);
        var emissionType = await GetEmissionTypeAsync(context, ct);
        var establishment = request.EstablishmentCode ?? await configuration.GetStringAsync("financial.sri.defaultEstablishmentCode", "001", context, ct);
        var emissionPoint = request.EmissionPointCode ?? await configuration.GetStringAsync("financial.sri.defaultEmissionPointCode", "001", context, ct);
        var document = ElectronicDocument.CreateInvoice(context.TenantId, environment, emissionType, establishment, emissionPoint, request.IssueDate,
            request.CustomerIdentificationType, request.CustomerIdentification, request.CustomerName, request.Currency, DateTimeOffset.UtcNow);
        await documents.AddAsync(document, ct);
        await documents.SaveChangesAsync(ct);
        await AuditOutboxAsync("ElectronicDocumentCreated", "ElectronicDocumentCreated", document, context, ct);
        return ToDto(document);
    }

    public async Task<ElectronicDocumentDto> CreateCreditNoteDraftAsync(CreateCreditNoteRequest request, PortalCallContext context, CancellationToken ct)
    {
        var environment = await GetEnvironmentAsync(context, ct);
        var emissionType = await GetEmissionTypeAsync(context, ct);
        var establishment = request.EstablishmentCode ?? await configuration.GetStringAsync("financial.sri.defaultEstablishmentCode", "001", context, ct);
        var emissionPoint = request.EmissionPointCode ?? await configuration.GetStringAsync("financial.sri.defaultEmissionPointCode", "001", context, ct);
        var document = ElectronicDocument.CreateCreditNote(context.TenantId, environment, emissionType, establishment, emissionPoint, request.IssueDate,
            request.CustomerIdentificationType, request.CustomerIdentification, request.CustomerName, request.Currency, request.RelatedDocumentTypeCode,
            request.RelatedDocumentNumber, request.RelatedDocumentIssueDate, request.Reason, DateTimeOffset.UtcNow);
        await documents.AddAsync(document, ct);
        await documents.SaveChangesAsync(ct);
        await AuditOutboxAsync("CreditNoteCreated", "CreditNoteCreated", document, context, ct);
        return ToDto(document);
    }

    public async Task<ElectronicDocumentDto> CreateDebitNoteDraftAsync(CreateDebitNoteRequest request, PortalCallContext context, CancellationToken ct)
    {
        var environment = await GetEnvironmentAsync(context, ct);
        var emissionType = await GetEmissionTypeAsync(context, ct);
        var establishment = request.EstablishmentCode ?? await configuration.GetStringAsync("financial.sri.defaultEstablishmentCode", "001", context, ct);
        var emissionPoint = request.EmissionPointCode ?? await configuration.GetStringAsync("financial.sri.defaultEmissionPointCode", "001", context, ct);
        var document = ElectronicDocument.CreateDebitNote(context.TenantId, environment, emissionType, establishment, emissionPoint, request.IssueDate,
            request.CustomerIdentificationType, request.CustomerIdentification, request.CustomerName, request.Currency, request.RelatedDocumentTypeCode,
            request.RelatedDocumentNumber, request.RelatedDocumentIssueDate, DateTimeOffset.UtcNow);
        await documents.AddAsync(document, ct);
        await documents.SaveChangesAsync(ct);
        await AuditOutboxAsync("DebitNoteCreated", "DebitNoteCreated", document, context, ct);
        return ToDto(document);
    }

    public async Task<ElectronicDocumentDto> CreateWithholdingDraftAsync(CreateWithholdingRequest request, PortalCallContext context, CancellationToken ct)
    {
        var environment = await GetEnvironmentAsync(context, ct);
        var emissionType = await GetEmissionTypeAsync(context, ct);
        var establishment = request.EstablishmentCode ?? await configuration.GetStringAsync("financial.sri.defaultEstablishmentCode", "001", context, ct);
        var emissionPoint = request.EmissionPointCode ?? await configuration.GetStringAsync("financial.sri.defaultEmissionPointCode", "001", context, ct);
        var document = ElectronicDocument.CreateWithholding(context.TenantId, environment, emissionType, establishment, emissionPoint, request.IssueDate,
            request.SubjectIdentificationType, request.SubjectIdentification, request.SubjectName, request.Currency, request.FiscalPeriod,
            request.SupportDocumentTypeCode, request.SupportDocumentNumber, request.SupportDocumentIssueDate, DateTimeOffset.UtcNow);
        await documents.AddAsync(document, ct);
        await documents.SaveChangesAsync(ct);
        await AuditOutboxAsync("WithholdingCreated", "WithholdingCreated", document, context, ct);
        return ToDto(document);
    }

    public async Task<ElectronicDocumentDto> AddInvoiceLineAsync(Guid id, AddElectronicDocumentLineRequest request, PortalCallContext context, CancellationToken ct)
    {
        var document = await GetRequiredAsync(id, context.TenantId, ct);
        var line = document.AddLine(request.ProductCode, request.Description, request.Quantity, request.UnitPrice, request.Discount, DateTimeOffset.UtcNow);
        await documents.AddLineAsync(line, ct);
        await documents.SaveChangesAsync(ct);
        await AuditOnlyAsync("ElectronicDocumentLineAdded", document, context, ct);
        return ToDto(document);
    }

    public async Task<ElectronicDocumentDto> AddDebitNoteReasonAsync(Guid id, AddDebitNoteReasonRequest request, PortalCallContext context, CancellationToken ct)
    {
        var document = await GetRequiredAsync(id, context.TenantId, ct);
        document.AddDebitNoteReason(request.Reason, request.Amount, DateTimeOffset.UtcNow);
        await documents.SaveChangesAsync(ct);
        await AuditOutboxAsync("DebitNoteReasonAdded", "DebitNoteReasonAdded", document, context, ct);
        return ToDto(document);
    }

    public async Task<ElectronicDocumentDto> AddWithholdingTaxAsync(Guid id, AddWithholdingTaxRequest request, PortalCallContext context, CancellationToken ct)
    {
        var document = await GetRequiredAsync(id, context.TenantId, ct);
        document.AddWithholdingTax(request.TaxCode, request.WithholdingCode, request.TaxBase, request.WithholdingPercentage, request.WithheldAmount,
            request.SupportDocumentNumber, request.SupportDocumentIssueDate, request.FiscalPeriod, DateTimeOffset.UtcNow);
        await documents.SaveChangesAsync(ct);
        await AuditOutboxAsync("WithholdingTaxAdded", "WithholdingTaxAdded", document, context, ct);
        return ToDto(document);
    }

    public Task<ElectronicDocumentDto> GenerateInvoiceXmlAsync(Guid id, PortalCallContext context, CancellationToken ct) => GenerateDocumentXmlAsync(id, ElectronicDocumentType.Invoice, "ElectronicDocumentXmlGenerated", context, ct);
    public Task<ElectronicDocumentDto> GenerateCreditNoteXmlAsync(Guid id, PortalCallContext context, CancellationToken ct) => GenerateDocumentXmlAsync(id, ElectronicDocumentType.CreditNote, "CreditNoteXmlGenerated", context, ct);
    public Task<ElectronicDocumentDto> GenerateDebitNoteXmlAsync(Guid id, PortalCallContext context, CancellationToken ct) => GenerateDocumentXmlAsync(id, ElectronicDocumentType.DebitNote, "DebitNoteXmlGenerated", context, ct);
    public Task<ElectronicDocumentDto> GenerateWithholdingXmlAsync(Guid id, PortalCallContext context, CancellationToken ct) => GenerateDocumentXmlAsync(id, ElectronicDocumentType.Withholding, "WithholdingXmlGenerated", context, ct);

    private async Task<ElectronicDocumentDto> GenerateDocumentXmlAsync(Guid id, ElectronicDocumentType expectedType, string generatedEvent, PortalCallContext context, CancellationToken ct)
    {
        var document = await GetRequiredAsync(id, context.TenantId, ct);
        if (document.DocumentType != expectedType) throw new FinancialApplicationException("electronic_document.type.invalid", $"Document must be {expectedType}.");
        var issuer = await GetIssuerAsync(context, ct);
        SriTaxRuleValidator.ValidateBeforeGenerate(document, DateOnly.FromDateTime(DateTimeOffset.UtcNow.UtcDateTime));
        await AuditOutboxAsync($"{expectedType}RulesValidated", $"{expectedType}RulesValidated", document, context, ct);
        var sequential = await documents.GetNextSequentialAsync(context.TenantId, document.DocumentType, document.Environment, document.EstablishmentCode, document.EmissionPointCode, ct);
        if (await documents.SequenceDocumentExistsAsync(context.TenantId, document.DocumentType, document.Environment, document.EstablishmentCode, document.EmissionPointCode, sequential, document.Id, ct))
            throw new FinancialApplicationException("sri.sequence.duplicate", "SRI sequence already exists for this document scope.");
        var numericCode = await configuration.GetStringAsync("financial.sri.accessKey.numericCode", DeterministicNumericCode(document.Id), context, ct);
        var accessKey = SriAccessKeyGenerator.Generate(new(document.IssueDate, SriDocumentCodes.ToCode(document.DocumentType), issuer.Ruc, document.Environment, document.EstablishmentCode, document.EmissionPointCode, sequential, numericCode, document.EmissionType));
        var xml = GenerateUnsignedXml(document, issuer, sequential, accessKey.Value);
        var validation = xmlValidator.ValidateXml(document.DocumentType, xml);
        if (!validation.IsValid) throw new FinancialApplicationException("sri.xml.validation.failed", string.Join("; ", validation.Errors));
        document.Generate(sequential, accessKey, xml, DateTimeOffset.UtcNow);
        var stored = await storage.SaveUnsignedXmlAsync(document, xml, context, ct);
        document.RegisterUnsignedXmlStorage(stored.StorageId, DateTimeOffset.UtcNow);
        await documents.SaveChangesAsync(ct);
        await AuditOutboxAsync("ElectronicDocumentXmlValidated", "ElectronicDocumentXmlValidated", document, context, ct);
        await AuditOutboxAsync("ElectronicDocumentStorageRegistered", "ElectronicDocumentStorageRegistered", document, context, ct);
        await AuditOutboxAsync(generatedEvent, generatedEvent, document, context, ct);
        return ToDto(document);
    }

    public async Task<XmlValidationResult> ValidateInvoiceXmlAsync(Guid id, PortalCallContext context, CancellationToken ct)
    {
        var document = await GetRequiredAsync(id, context.TenantId, ct);
        var xml = xmlGenerator.GenerateXml(document, await GetIssuerAsync(context, ct));
        var result = xmlValidator.ValidateXml(document.DocumentType, xml);
        if (result.IsValid) await AuditOutboxAsync("ElectronicDocumentXmlValidated", "ElectronicDocumentXmlValidated", document, context, ct);
        return result;
    }

    public async Task<ElectronicDocumentDto> SignElectronicDocumentAsync(Guid id, PortalCallContext context, CancellationToken ct)
    {
        var document = await GetRequiredAsync(id, context.TenantId, ct);
        document.EnsureCanSign();
        var issuer = await GetIssuerAsync(context, ct);
        var xml = xmlGenerator.GenerateXml(document, issuer);
        var validation = xmlValidator.ValidateXml(document.DocumentType, xml);
        if (!validation.IsValid) throw new FinancialApplicationException("sri.xml.validation.failed", string.Join("; ", validation.Errors));
        SignatureResult signed;
        try
        {
            signed = await signature.SignAsync(xml, await GetSignatureContextAsync(context, ct), ct);
        }
        catch (FinancialApplicationException ex)
        {
            document.MarkError(ex.Code, ex.Message, DateTimeOffset.UtcNow);
            await documents.SaveChangesAsync(ct);
            await AuditOutboxAsync("ElectronicDocumentSignatureFailed", "ElectronicDocumentSignatureFailed", document, context, ct);
            throw;
        }
        document.MarkSigned(signed.SignedXml, signed.Provider, signed.SignatureDigest, signed.SignedAtUtc);
        var stored = await storage.SaveSignedXmlAsync(document, signed.SignedXml, context, ct);
        document.RegisterSignedXmlStorage(stored.StorageId, DateTimeOffset.UtcNow);
        await documents.SaveChangesAsync(ct);
        await AuditOutboxAsync("ElectronicDocumentStorageRegistered", "ElectronicDocumentStorageRegistered", document, context, ct);
        await AuditOutboxAsync("ElectronicDocumentSigned", "ElectronicDocumentSigned", document, context, ct);
        return ToDto(document);
    }

    public async Task<ElectronicDocumentDto> SendElectronicDocumentAsync(Guid id, PortalCallContext context, CancellationToken ct)
    {
        var document = await GetRequiredAsync(id, context.TenantId, ct);
        document.EnsureCanSend();
        var issuer = await GetIssuerAsync(context, ct);
        var signedXml = (await signature.SignAsync(xmlGenerator.GenerateXml(document, issuer), await GetSignatureContextAsync(context, ct), ct)).SignedXml;
        SriReceptionResponse result;
        try
        {
            document.RegisterSriReceptionAttempt(context.CorrelationId, DateTimeOffset.UtcNow);
            result = await reception.SendAsync(new(document.AccessKey!, signedXml, await GetSriClientContextAsync(document.Environment, context, ct)), ct);
        }
        catch (FinancialApplicationException ex)
        {
            document.MarkError(ex.Code, ex.Message, DateTimeOffset.UtcNow);
            await documents.SaveChangesAsync(ct);
            await AuditOutboxAsync("ElectronicDocumentSriSendFailed", "ElectronicDocumentSriSendFailed", document, context, ct);
            throw;
        }
        if (result.Status is SriResponseStatus.Returned or SriResponseStatus.Rejected or SriResponseStatus.Error) document.MarkRejected(result.Code, result.Message, DateTimeOffset.UtcNow);
        else document.MarkSent(result.Code, result.Message, DateTimeOffset.UtcNow);
        await documents.SaveChangesAsync(ct);
        await AuditOutboxAsync(document.Status == ElectronicDocumentStatus.Rejected ? "ElectronicDocumentRejected" : "ElectronicDocumentSent",
            document.Status == ElectronicDocumentStatus.Rejected ? "ElectronicDocumentRejected" : "ElectronicDocumentSent", document, context, ct);
        return ToDto(document);
    }

    public async Task<ElectronicDocumentDto> AuthorizeElectronicDocumentAsync(Guid id, PortalCallContext context, CancellationToken ct)
    {
        var document = await GetRequiredAsync(id, context.TenantId, ct);
        if (document.AccessKey is null) throw new FinancialApplicationException("sri.authorization.access_key.required", "Access key is required.");
        document.RegisterSriAuthorizationAttempt(context.CorrelationId, DateTimeOffset.UtcNow);
        var result = await authorization.AuthorizeAsync(new(document.AccessKey, await GetSriClientContextAsync(document.Environment, context, ct)), ct);
        if (result.Status == SriResponseStatus.Authorized)
        {
            document.MarkAuthorized(result.AuthorizationNumber ?? document.AccessKey, result.AuthorizationDate ?? DateTimeOffset.UtcNow, result.Code, result.Message, DateTimeOffset.UtcNow);
            if (!string.IsNullOrWhiteSpace(result.AuthorizationXml))
            {
                var stored = await storage.SaveAuthorizationXmlAsync(document, result.AuthorizationXml, context, ct);
                document.RegisterAuthorizationXmlStorage(stored.StorageId, DateTimeOffset.UtcNow);
                await AuditOutboxAsync("ElectronicDocumentStorageRegistered", "ElectronicDocumentStorageRegistered", document, context, ct);
            }
        }
        else document.MarkRejected(result.Code, result.Message, DateTimeOffset.UtcNow);
        await documents.SaveChangesAsync(ct);
        var authorized = result.Status == SriResponseStatus.Authorized;
        await AuditOutboxAsync(authorized ? "ElectronicDocumentAuthorized" : "ElectronicDocumentRejected", authorized ? "ElectronicDocumentAuthorized" : "ElectronicDocumentRejected", document, context, ct);
        return ToDto(document);
    }

    public async Task<ElectronicDocumentDto> GetByIdAsync(Guid id, PortalCallContext context, CancellationToken ct) => ToDto(await GetRequiredAsync(id, context.TenantId, ct));
    public async Task<ElectronicDocumentDto> GenerateRidePdfAsync(Guid id, PortalCallContext context, CancellationToken ct)
    {
        var document = await GetRequiredAsync(id, context.TenantId, ct);
        if (string.IsNullOrWhiteSpace(document.AccessKey) || string.IsNullOrWhiteSpace(document.Sequential))
            throw new FinancialApplicationException("sri.ride.access_key.required", "Generated XML/access key is required before generating RIDE.");
        var issuer = await GetIssuerAsync(context, ct);
        var model = new InvoiceRideModel(issuer.Ruc, issuer.LegalName, $"{document.EstablishmentCode}-{document.EmissionPointCode}-{document.Sequential}", document.AccessKey, document.IssueDate,
            document.CustomerName, document.CustomerIdentification,
            document.Lines.Select(x => new ElectronicDocumentLineDto(x.Id, x.LineNumber, x.ProductCode, x.Description, x.Quantity, x.UnitPrice, x.Discount, x.Subtotal, x.Total)).ToArray(),
            document.SubtotalWithoutTaxes, document.TotalTaxes, document.TotalAmount, document.SriAuthorizationNumber, document.SriAuthorizationDate, document.Environment.ToString(), document.Status.ToString());
        var ride = await ridePdfGenerator.GenerateInvoiceRideAsync(model, ct);
        var stored = await storage.SaveRidePdfAsync(document, ride.PdfBytes, context, ct);
        document.RegisterRidePdfStorage(stored.StorageId, ride.Hash, stored.Provider, context.CorrelationId, DateTimeOffset.UtcNow);
        await documents.SaveChangesAsync(ct);
        await AuditOutboxAsync("ElectronicDocumentRideGenerated", "ElectronicDocumentRideGenerated", document, context, ct);
        await AuditOutboxAsync("ElectronicDocumentStorageRegistered", "ElectronicDocumentStorageRegistered", document, context, ct);
        return ToDto(document);
    }

    public async Task<RideMetadataDto> GetRideMetadataAsync(Guid id, PortalCallContext context, CancellationToken ct)
    {
        var document = await GetRequiredAsync(id, context.TenantId, ct);
        return new(document.RidePdfStorageId, document.RidePdfHash, document.RideGeneratedAtUtc, document.StorageProvider);
    }

    public async Task<object> GetIntegrationStatusAsync(Guid id, PortalCallContext context, CancellationToken ct)
    {
        var document = await GetRequiredAsync(id, context.TenantId, ct);
        return new
        {
            document.Id,
            AccessKey = SriSensitiveDataSanitizer.MaskAccessKey(document.AccessKey),
            CustomerIdentification = SriSensitiveDataSanitizer.MaskCustomerIdentification(document.CustomerIdentification),
            Status = document.Status.ToString(),
            document.LastSriStatus,
            LastSriMessage = SriSensitiveDataSanitizer.SanitizeMessage(document.LastSriMessage),
            document.SriReceptionAttempts,
            document.SriAuthorizationAttempts,
            document.LastIntegrationCorrelationId,
            document.StorageProvider,
            document.RideGeneratedAtUtc
        };
    }

    public async Task<object> GetStatusAsync(Guid id, PortalCallContext context, CancellationToken ct)
    {
        var document = await GetRequiredAsync(id, context.TenantId, ct);
        return new { document.Id, Status = document.Status.ToString(), document.AccessKey, document.LastSriStatus, document.LastSriMessage, document.SriResponseCode, document.SriResponseMessage, document.LastErrorCode, document.LastErrorMessage };
    }
    public async Task<ElectronicDocumentStorageMetadataDto> GetStorageMetadataAsync(Guid id, PortalCallContext context, CancellationToken ct)
    {
        var document = await GetRequiredAsync(id, context.TenantId, ct);
        return new(document.UnsignedXmlStorageId, document.SignedXmlStorageId, document.AuthorizationXmlStorageId, document.RidePdfStorageId, document.XmlContentHash, document.SignedXmlContentHash, document.SignatureDigest, document.RidePdfHash, document.StorageProvider);
    }
    public async Task<ElectronicDocumentDto> GetByAccessKeyAsync(string accessKey, PortalCallContext context, CancellationToken ct) =>
        ToDto(await documents.GetByAccessKeyAsync(accessKey.Trim(), context.TenantId, ct) ?? throw new FinancialApplicationException("electronic_document.not_found", "Electronic document was not found."));
    public async Task<object> SearchAsync(SearchElectronicDocumentsRequest request, PortalCallContext context, CancellationToken ct)
    {
        ElectronicDocumentStatus? status = string.IsNullOrWhiteSpace(request.Status) ? null : Enum.Parse<ElectronicDocumentStatus>(request.Status, true);
        var page = Math.Max(1, request.Page);
        var size = Math.Clamp(request.PageSize, 1, 100);
        var (items, total) = await documents.SearchAsync(context.TenantId, status, request.AccessKey, page, size, ct);
        return new { items = items.Select(ToDto).ToArray(), page, pageSize = size, total };
    }

    private async Task<ElectronicDocument> GetRequiredAsync(Guid id, string tenantId, CancellationToken ct) =>
        await documents.GetByIdAsync(id, tenantId, ct) ?? throw new FinancialApplicationException("electronic_document.not_found", "Electronic document was not found.");
    private async Task<IssuerSriOptions> GetIssuerAsync(PortalCallContext context, CancellationToken ct) =>
        new(
            await configuration.GetStringAsync("financial.sri.issuer.ruc", "9999999999999", context, ct),
            await configuration.GetStringAsync("financial.sri.issuer.legalName", "EMISOR DESARROLLO", context, ct),
            await configuration.GetStringAsync("financial.sri.issuer.tradeName", "EMISOR DESARROLLO", context, ct),
            await configuration.GetStringAsync("financial.sri.issuer.address", "DIRECCION DESARROLLO", context, ct),
            await configuration.GetBoolAsync("financial.sri.issuer.accountingRequired", true, context, ct));
    private async Task<SriEnvironment> GetEnvironmentAsync(PortalCallContext context, CancellationToken ct) =>
        Enum.TryParse<SriEnvironment>(await configuration.GetStringAsync("financial.sri.environment", "Test", context, ct), true, out var value) ? value : SriEnvironment.Test;
    private async Task<SriEmissionType> GetEmissionTypeAsync(PortalCallContext context, CancellationToken ct) =>
        Enum.TryParse<SriEmissionType>(await configuration.GetStringAsync("financial.sri.emissionType", "Normal", context, ct), true, out var value) ? value : SriEmissionType.Normal;
    private async Task<SignatureContext> GetSignatureContextAsync(PortalCallContext context, CancellationToken ct) =>
        new(context.TenantId,
            Enum.TryParse<SignatureProviderType>(await configuration.GetStringAsync("financial.sri.signature.provider", "Development", context, ct), true, out var provider) ? provider : SignatureProviderType.Disabled,
            await configuration.GetStringAsync("ASPNETCORE_ENVIRONMENT", "Development", context, ct),
            await configuration.GetStringAsync("financial.sri.signature.certificateSecretName", "", context, ct),
            await configuration.GetStringAsync("financial.sri.signature.keyVaultName", "", context, ct),
            await configuration.GetStringAsync("financial.sri.signature.localCertificatePath", "", context, ct),
            await configuration.GetStringAsync("financial.sri.signature.localCertificatePasswordSecretName", "", context, ct),
            await configuration.GetBoolAsync("financial.sri.signature.requireTrustedCertificate", true, context, ct),
            await configuration.GetStringAsync("financial.sri.signature.certificateSource", "SecretStore", context, ct),
            await configuration.GetStringAsync("financial.sri.signature.timestampPolicy", "", context, ct),
            await configuration.GetBoolAsync("financial.sri.signature.requireOcsp", false, context, ct));
    private async Task<SriClientContext> GetSriClientContextAsync(SriEnvironment environment, PortalCallContext context, CancellationToken ct) =>
        new(context.TenantId, environment,
            await configuration.GetStringAsync("financial.sri.integration.mode", "Development", context, ct),
            await configuration.GetStringAsync("financial.sri.receptionUrl", "", context, ct),
            await configuration.GetStringAsync("financial.sri.authorizationUrl", "", context, ct),
            await configuration.GetIntAsync("financial.sri.timeoutSeconds", 30, context, ct),
            await configuration.GetIntAsync("financial.sri.maxRetries", 3, context, ct),
            await configuration.GetIntAsync("financial.sri.retryDelaySeconds", 5, context, ct),
            await configuration.GetBoolAsync("financial.sri.allowProduction", false, context, ct),
            await configuration.GetBoolAsync("financial.sri.logPayloads", false, context, ct),
            await configuration.GetBoolAsync("financial.sri.test.dryRun", true, context, ct),
            await configuration.GetBoolAsync("financial.sri.maskPayloads", true, context, ct));
    private async Task AuditOnlyAsync(string auditAction, ElectronicDocument document, PortalCallContext context, CancellationToken ct) =>
        await audit.RecordAsync(new(auditAction, "financial.electronic-document", document.Id.ToString(), new { document.Id, document.AccessKey, document.Status }), context, ct);
    private async Task AuditOutboxAsync(string auditAction, string outboxType, ElectronicDocument document, PortalCallContext context, CancellationToken ct)
    {
        await audit.RecordAsync(new(auditAction, "financial.electronic-document", document.Id.ToString(), new { document.Id, document.AccessKey, document.Status }), context, ct);
        await outbox.EnqueueAsync(new(Guid.NewGuid(), outboxType, 1, DateTimeOffset.UtcNow, context.CorrelationId,
            JsonSerializer.Serialize(new { document.Id, document.TenantId, document.AccessKey, Status = document.Status.ToString() })), context, ct);
    }
    private static string DeterministicNumericCode(Guid id) => Math.Abs(BitConverter.ToInt32(id.ToByteArray(), 0)).ToString("00000000", CultureInfo.InvariantCulture)[..8];
    private static string GenerateUnsignedXml(ElectronicDocument document, IssuerSriOptions issuer, string sequential, string accessKey) => document.DocumentType switch
    {
        ElectronicDocumentType.Invoice => GenerateUnsignedInvoiceXml(document, issuer, sequential, accessKey),
        ElectronicDocumentType.CreditNote => GenerateUnsignedCreditNoteXml(document, issuer, sequential, accessKey),
        ElectronicDocumentType.DebitNote => GenerateUnsignedDebitNoteXml(document, issuer, sequential, accessKey),
        ElectronicDocumentType.Withholding => GenerateUnsignedWithholdingXml(document, issuer, sequential, accessKey),
        _ => throw new FinancialApplicationException("sri.xml.document_type.unsupported", "Unsupported SRI document type.")
    };

    private static string UnsignedInfoTributaria(ElectronicDocument document, IssuerSriOptions issuer, string sequential, string accessKey) => $"""
<infoTributaria><ambiente>{(int)document.Environment}</ambiente><tipoEmision>{(int)document.EmissionType}</tipoEmision><razonSocial>{Xml(issuer.LegalName)}</razonSocial><nombreComercial>{Xml(issuer.TradeName ?? issuer.LegalName)}</nombreComercial><ruc>{issuer.Ruc}</ruc><claveAcceso>{accessKey}</claveAcceso><codDoc>{SriDocumentCodes.ToCode(document.DocumentType)}</codDoc><estab>{document.EstablishmentCode}</estab><ptoEmi>{document.EmissionPointCode}</ptoEmi><secuencial>{sequential}</secuencial><dirMatriz>{Xml(issuer.Address)}</dirMatriz></infoTributaria>
""";

    private static string GenerateUnsignedInvoiceXml(ElectronicDocument document, IssuerSriOptions issuer, string sequential, string accessKey)
    {
        var infoTributaria = UnsignedInfoTributaria(document, issuer, sequential, accessKey);
        var detalles = string.Concat(document.Lines.Select(line => $"<detalle><codigoPrincipal>{Xml(line.ProductCode)}</codigoPrincipal><descripcion>{Xml(line.Description)}</descripcion><cantidad>{line.Quantity:0.00##}</cantidad><precioUnitario>{line.UnitPrice:0.00##}</precioUnitario><descuento>{line.Discount:0.00##}</descuento><precioTotalSinImpuesto>{line.Subtotal:0.00##}</precioTotalSinImpuesto></detalle>"));
        return $"""<?xml version="1.0" encoding="UTF-8"?><factura id="comprobante" version="1.0.0">{infoTributaria}<infoFactura><fechaEmision>{document.IssueDate:dd/MM/yyyy}</fechaEmision><dirEstablecimiento>{Xml(issuer.Address)}</dirEstablecimiento><obligadoContabilidad>{(issuer.AccountingRequired ? "SI" : "NO")}</obligadoContabilidad><tipoIdentificacionComprador>{document.CustomerIdentificationType}</tipoIdentificacionComprador><razonSocialComprador>{Xml(document.CustomerName)}</razonSocialComprador><identificacionComprador>{document.CustomerIdentification}</identificacionComprador><totalSinImpuestos>{document.SubtotalWithoutTaxes:0.00##}</totalSinImpuestos><totalDescuento>{document.TotalDiscount:0.00##}</totalDescuento><propina>0.00</propina><importeTotal>{document.TotalAmount:0.00##}</importeTotal><moneda>{document.Currency}</moneda></infoFactura><detalles>{detalles}</detalles></factura>""";
    }

    private static string GenerateUnsignedCreditNoteXml(ElectronicDocument document, IssuerSriOptions issuer, string sequential, string accessKey)
    {
        var related = document.References.FirstOrDefault() ?? throw new FinancialApplicationException("sri.credit_note.reference.required", "Credit note related document is required.");
        var detalles = string.Concat(document.Lines.Select(line => $"<detalle><codigoInterno>{Xml(line.ProductCode)}</codigoInterno><descripcion>{Xml(line.Description)}</descripcion><cantidad>{line.Quantity:0.00##}</cantidad><precioUnitario>{line.UnitPrice:0.00##}</precioUnitario><descuento>{line.Discount:0.00##}</descuento><precioTotalSinImpuesto>{line.Subtotal:0.00##}</precioTotalSinImpuesto></detalle>"));
        return $"""<?xml version="1.0" encoding="UTF-8"?><notaCredito id="comprobante" version="1.0.0">{UnsignedInfoTributaria(document, issuer, sequential, accessKey)}<infoNotaCredito><fechaEmision>{document.IssueDate:dd/MM/yyyy}</fechaEmision><dirEstablecimiento>{Xml(issuer.Address)}</dirEstablecimiento><tipoIdentificacionComprador>{document.CustomerIdentificationType}</tipoIdentificacionComprador><razonSocialComprador>{Xml(document.CustomerName)}</razonSocialComprador><identificacionComprador>{document.CustomerIdentification}</identificacionComprador><codDocModificado>{related.DocumentTypeCode}</codDocModificado><numDocModificado>{Xml(related.Number)}</numDocModificado><fechaEmisionDocSustento>{related.IssueDate:dd/MM/yyyy}</fechaEmisionDocSustento><totalSinImpuestos>{document.SubtotalWithoutTaxes:0.00##}</totalSinImpuestos><valorModificacion>{document.TotalAmount:0.00##}</valorModificacion><moneda>{document.Currency}</moneda><motivo>{Xml(related.ReasonOrPeriod)}</motivo></infoNotaCredito><detalles>{detalles}</detalles></notaCredito>""";
    }

    private static string GenerateUnsignedDebitNoteXml(ElectronicDocument document, IssuerSriOptions issuer, string sequential, string accessKey)
    {
        var related = document.References.FirstOrDefault() ?? throw new FinancialApplicationException("sri.debit_note.reference.required", "Debit note related document is required.");
        var motivos = string.Concat(document.DebitNoteReasons.Select(x => $"<motivo><razon>{Xml(x.Reason)}</razon><valor>{x.Amount:0.00##}</valor></motivo>"));
        var impuestos = string.Concat(document.Taxes.Select(x => $"<impuesto><codigo>{x.TaxCode}</codigo><codigoPorcentaje>{x.TaxPercentageCode}</codigoPorcentaje><tarifa>{x.TaxRate:0.00##}</tarifa><baseImponible>{x.TaxBase:0.00##}</baseImponible><valor>{x.TaxAmount:0.00##}</valor></impuesto>"));
        return $"""<?xml version="1.0" encoding="UTF-8"?><notaDebito id="comprobante" version="1.0.0">{UnsignedInfoTributaria(document, issuer, sequential, accessKey)}<infoNotaDebito><fechaEmision>{document.IssueDate:dd/MM/yyyy}</fechaEmision><dirEstablecimiento>{Xml(issuer.Address)}</dirEstablecimiento><tipoIdentificacionComprador>{document.CustomerIdentificationType}</tipoIdentificacionComprador><razonSocialComprador>{Xml(document.CustomerName)}</razonSocialComprador><identificacionComprador>{document.CustomerIdentification}</identificacionComprador><codDocModificado>{related.DocumentTypeCode}</codDocModificado><numDocModificado>{Xml(related.Number)}</numDocModificado><fechaEmisionDocSustento>{related.IssueDate:dd/MM/yyyy}</fechaEmisionDocSustento><totalSinImpuestos>{document.SubtotalWithoutTaxes:0.00##}</totalSinImpuestos><valorTotal>{document.TotalAmount:0.00##}</valorTotal></infoNotaDebito><motivos>{motivos}</motivos><impuestos>{impuestos}</impuestos></notaDebito>""";
    }

    private static string GenerateUnsignedWithholdingXml(ElectronicDocument document, IssuerSriOptions issuer, string sequential, string accessKey)
    {
        var related = document.References.FirstOrDefault() ?? throw new FinancialApplicationException("sri.withholding.support.required", "Withholding support document is required.");
        var impuestos = string.Concat(document.WithholdingTaxes.Select(x => $"<impuesto><codigo>{Xml(x.TaxCode)}</codigo><codigoRetencion>{Xml(x.WithholdingCode)}</codigoRetencion><baseImponible>{x.TaxBase:0.00##}</baseImponible><porcentajeRetener>{x.WithholdingPercentage:0.00##}</porcentajeRetener><valorRetenido>{x.WithheldAmount:0.00##}</valorRetenido><codDocSustento>{related.DocumentTypeCode}</codDocSustento><numDocSustento>{Xml(x.SupportDocumentNumber)}</numDocSustento><fechaEmisionDocSustento>{x.SupportDocumentIssueDate:dd/MM/yyyy}</fechaEmisionDocSustento></impuesto>"));
        return $"""<?xml version="1.0" encoding="UTF-8"?><comprobanteRetencion id="comprobante" version="1.0.0">{UnsignedInfoTributaria(document, issuer, sequential, accessKey)}<infoCompRetencion><fechaEmision>{document.IssueDate:dd/MM/yyyy}</fechaEmision><dirEstablecimiento>{Xml(issuer.Address)}</dirEstablecimiento><obligadoContabilidad>{(issuer.AccountingRequired ? "SI" : "NO")}</obligadoContabilidad><tipoIdentificacionSujetoRetenido>{document.CustomerIdentificationType}</tipoIdentificacionSujetoRetenido><razonSocialSujetoRetenido>{Xml(document.CustomerName)}</razonSocialSujetoRetenido><identificacionSujetoRetenido>{document.CustomerIdentification}</identificacionSujetoRetenido><periodoFiscal>{Xml(related.ReasonOrPeriod)}</periodoFiscal></infoCompRetencion><impuestos>{impuestos}</impuestos><docsSustento><docSustento><codSustento>{related.DocumentTypeCode}</codSustento><codDocSustento>{related.DocumentTypeCode}</codDocSustento><numDocSustento>{Xml(related.Number)}</numDocSustento><fechaEmisionDocSustento>{related.IssueDate:dd/MM/yyyy}</fechaEmisionDocSustento></docSustento></docsSustento></comprobanteRetencion>""";
    }
    private static string Xml(string value) => HtmlEncoder.Default.Encode(value);
    public static ElectronicDocumentDto ToDto(ElectronicDocument document) => new(document.Id, document.TenantId, document.DocumentType.ToString(), document.Environment.ToString(), document.EmissionType.ToString(), document.Status.ToString(),
        document.EstablishmentCode, document.EmissionPointCode, document.Sequential, document.AccessKey, document.IssueDate, document.CustomerIdentificationType, document.CustomerIdentification, document.CustomerName, document.Currency,
        document.SubtotalWithoutTaxes, document.TotalDiscount, document.TotalTaxes, document.TotalAmount, document.SriAuthorizationNumber, document.SriAuthorizationDate, document.SriResponseCode, document.SriResponseMessage,
        document.UnsignedXmlStorageId, document.SignedXmlStorageId, document.AuthorizationXmlStorageId, document.RidePdfStorageId, document.SignatureProvider, document.SignatureDigest, document.LastSriStatus, document.LastSriMessage,
        document.Lines.Select(x => new ElectronicDocumentLineDto(x.Id, x.LineNumber, x.ProductCode, x.Description, x.Quantity, x.UnitPrice, x.Discount, x.Subtotal, x.Total)).ToArray(),
        document.References.Select(x => new ElectronicDocumentReferenceDto(x.DocumentTypeCode, x.Number, x.IssueDate, x.ReasonOrPeriod)).ToArray(),
        document.DebitNoteReasons.Select(x => new ElectronicDocumentDebitNoteReasonDto(x.Reason, x.Amount)).ToArray(),
        document.WithholdingTaxes.Select(x => new ElectronicDocumentWithholdingTaxDto(x.TaxCode, x.WithholdingCode, x.TaxBase, x.WithholdingPercentage, x.WithheldAmount, x.SupportDocumentNumber, x.SupportDocumentIssueDate, x.FiscalPeriod)).ToArray());
}

public static class SriDocumentCodes
{
    public static string ToCode(ElectronicDocumentType type) => type switch
    {
        ElectronicDocumentType.Invoice => "01",
        ElectronicDocumentType.CreditNote => "04",
        ElectronicDocumentType.DebitNote => "05",
        ElectronicDocumentType.ReferralGuide => "06",
        ElectronicDocumentType.Withholding => "07",
        _ => throw new FinancialApplicationException("sri.document_type.unsupported", "Unsupported SRI document type.")
    };
}

public static class SriPortalMetadata
{
    public static readonly string[] Permissions =
    [
        "financial.electronicdocuments.read", "financial.electronicdocuments.create", "financial.electronicdocuments.update",
        "financial.electronicdocuments.generate", "financial.electronicdocuments.sign", "financial.electronicdocuments.send",
        "financial.electronicdocuments.authorize", "financial.electronicdocuments.cancel", "financial.electronicdocuments.manage"
    ];
    public static readonly string[] ConfigurationKeys =
    [
        "financial.secrets.provider", "financial.secrets.keyVaultName", "financial.secrets.useDefaultAzureCredential", "financial.secrets.requireManagedIdentity",
        "financial.secrets.allowDevelopmentSecrets", "financial.secrets.maskSecretsInLogs", "financial.secrets.failFastOnStartup",
        "financial.sri.issuer.ruc", "financial.sri.issuer.legalName", "financial.sri.issuer.tradeName", "financial.sri.issuer.address",
        "financial.sri.issuer.specialTaxpayerNumber", "financial.sri.issuer.accountingRequired", "financial.sri.environment",
        "financial.sri.integration.mode", "financial.sri.emissionType", "financial.sri.defaultEstablishmentCode",
        "financial.sri.defaultEmissionPointCode", "financial.sri.signature.provider", "financial.sri.signature.certificateSecretName",
        "financial.sri.signature.certificatePasswordSecretName",
        "financial.sri.signature.keyVaultName", "financial.sri.signature.certificateSource", "financial.sri.signature.timestampPolicy",
        "financial.sri.signature.requireOcsp", "financial.sri.signature.localCertificatePath", "financial.sri.signature.localCertificatePasswordSecretName",
        "financial.sri.signature.requireTrustedCertificate", "financial.sri.receptionUrl", "financial.sri.authorizationUrl",
        "financial.sri.allowProduction", "financial.sri.logPayloads", "financial.sri.maskPayloads", "financial.sri.test.dryRun",
        "financial.sri.test.allowConnectivityProbe", "financial.sri.test.allowDocumentSend", "financial.sri.test.manualValidationRequired",
        "financial.sri.test.useSyntheticDocumentOnly", "financial.sri.test.syntheticIssuerRuc", "financial.sri.test.syntheticCustomerIdentification",
        "financial.sri.test.maxManualAttempts", "financial.sri.timeoutSeconds", "financial.sri.maxRetries", "financial.sri.retryDelaySeconds",
        "financial.sri.xml.version.invoice", "financial.sri.xml.includeOptionalFields", "financial.sri.xml.validation.mode",
        "financial.sri.xml.validation.failOnWarning", "financial.sri.storage.provider", "financial.sri.storage.portalBaseUrl",
        "financial.sri.storage.container", "financial.sri.storage.timeoutSeconds", "financial.sri.storage.sendPayloads", "financial.sri.storage.maskPayloads", "financial.sri.storage.retainXml", "financial.sri.storage.retainPdf"
    ];
}
