using System.Globalization;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Xml.Linq;
using Financiero.Domain;

namespace Financiero.Application;

public sealed record CreateInvoiceRequest(DateOnly IssueDate, string CustomerIdentificationType, string CustomerIdentification, string CustomerName, string Currency = "USD", string? EstablishmentCode = null, string? EmissionPointCode = null);
public sealed record AddElectronicDocumentLineRequest(string ProductCode, string Description, decimal Quantity, decimal UnitPrice, decimal Discount = 0);
public sealed record SearchElectronicDocumentsRequest(string? Status = null, string? AccessKey = null, int Page = 1, int PageSize = 20);
public sealed record ElectronicDocumentLineDto(Guid Id, int LineNumber, string ProductCode, string Description, decimal Quantity, decimal UnitPrice, decimal Discount, decimal Subtotal, decimal Total);
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
    IReadOnlyCollection<ElectronicDocumentLineDto> Lines);

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
public sealed record SignatureContext(string TenantId, string Provider, string? CertificateSecretName, string? KeyVaultName);
public sealed record SriClientContext(string TenantId, SriEnvironment Environment, string Mode, string? ReceptionUrl, string? AuthorizationUrl, int TimeoutSeconds);
public sealed record SriReceptionResult(string Status, string Code, string Message);
public sealed record SriAuthorizationResult(bool Authorized, string Code, string Message, string? AuthorizationNumber, DateTimeOffset? AuthorizationDate);

public interface IElectronicDocumentXmlGenerator
{
    string GenerateInvoiceXml(ElectronicDocument document, IssuerSriOptions issuer);
}

public interface IElectronicSignatureService
{
    Task<string> SignAsync(string unsignedXml, SignatureContext context, CancellationToken ct);
}

public interface ISriReceptionClient
{
    Task<SriReceptionResult> SendAsync(string signedXml, SriClientContext context, CancellationToken ct);
}

public interface ISriAuthorizationClient
{
    Task<SriAuthorizationResult> AuthorizeAsync(string accessKey, SriClientContext context, CancellationToken ct);
}

public sealed class ElectronicInvoiceXmlGenerator : IElectronicDocumentXmlGenerator
{
    public string GenerateInvoiceXml(ElectronicDocument document, IssuerSriOptions issuer)
    {
        if (document.DocumentType != ElectronicDocumentType.Invoice) throw new FinancialApplicationException("sri.xml.document_type.unsupported", "Only invoice XML is supported in P1.");
        if (document.AccessKey is null || document.Sequential is null) throw new FinancialApplicationException("sri.xml.access_key.required", "Access key and sequential are required.");
        var infoTributaria = new XElement("infoTributaria",
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

    private static string Format(decimal value) => FinancialPrecision.Normalize(value).ToString("0.00##", CultureInfo.InvariantCulture);
}

public sealed class DevelopmentElectronicSignatureService : IElectronicSignatureService
{
    public Task<string> SignAsync(string unsignedXml, SignatureContext context, CancellationToken ct) =>
        Task.FromResult(unsignedXml.Replace("</factura>", "<firmaSimulada proveedor=\"Development\" />" + "</factura>", StringComparison.Ordinal));
}

public sealed class DevelopmentSriReceptionClient(IFinancialConfigurationReader configuration) : ISriReceptionClient
{
    public async Task<SriReceptionResult> SendAsync(string signedXml, SriClientContext context, CancellationToken ct)
    {
        var mode = await configuration.GetStringAsync("financial.sri.mock.receptionStatus", "Received", new PortalCallContext(context.TenantId, "sri-reception-dev"), ct);
        return string.Equals(mode, "Rejected", StringComparison.OrdinalIgnoreCase)
            ? new("Rejected", "DEV-REJECTED", "Development SRI reception rejected the document.")
            : new("Received", "DEV-RECEIVED", "Development SRI reception accepted the document.");
    }
}

public sealed class DevelopmentSriAuthorizationClient(IFinancialConfigurationReader configuration) : ISriAuthorizationClient
{
    public async Task<SriAuthorizationResult> AuthorizeAsync(string accessKey, SriClientContext context, CancellationToken ct)
    {
        var mode = await configuration.GetStringAsync("financial.sri.mock.authorizationStatus", "Authorized", new PortalCallContext(context.TenantId, "sri-authorization-dev"), ct);
        return string.Equals(mode, "Rejected", StringComparison.OrdinalIgnoreCase)
            ? new(false, "DEV-REJECTED", "Development SRI authorization rejected the document.", null, null)
            : new(true, "DEV-AUTHORIZED", "Development SRI authorization approved the document.", accessKey, DateTimeOffset.UtcNow);
    }
}

public sealed class ElectronicDocumentsService(
    IElectronicDocumentRepository documents,
    IFinancialConfigurationReader configuration,
    IElectronicDocumentXmlGenerator xmlGenerator,
    IElectronicSignatureService signature,
    ISriReceptionClient reception,
    ISriAuthorizationClient authorization,
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

    public async Task<ElectronicDocumentDto> AddInvoiceLineAsync(Guid id, AddElectronicDocumentLineRequest request, PortalCallContext context, CancellationToken ct)
    {
        var document = await GetRequiredAsync(id, context.TenantId, ct);
        var line = document.AddLine(request.ProductCode, request.Description, request.Quantity, request.UnitPrice, request.Discount, DateTimeOffset.UtcNow);
        await documents.AddLineAsync(line, ct);
        await documents.SaveChangesAsync(ct);
        await AuditOnlyAsync("ElectronicDocumentLineAdded", document, context, ct);
        return ToDto(document);
    }

    public async Task<ElectronicDocumentDto> GenerateInvoiceXmlAsync(Guid id, PortalCallContext context, CancellationToken ct)
    {
        var document = await GetRequiredAsync(id, context.TenantId, ct);
        var issuer = await GetIssuerAsync(context, ct);
        var sequential = await documents.GetNextSequentialAsync(context.TenantId, document.DocumentType, document.Environment, document.EstablishmentCode, document.EmissionPointCode, ct);
        if (await documents.SequenceDocumentExistsAsync(context.TenantId, document.DocumentType, document.Environment, document.EstablishmentCode, document.EmissionPointCode, sequential, document.Id, ct))
            throw new FinancialApplicationException("sri.sequence.duplicate", "SRI sequence already exists for this document scope.");
        var numericCode = await configuration.GetStringAsync("financial.sri.accessKey.numericCode", DeterministicNumericCode(document.Id), context, ct);
        var accessKey = SriAccessKeyGenerator.Generate(new(document.IssueDate, SriDocumentCodes.ToCode(document.DocumentType), issuer.Ruc, document.Environment, document.EstablishmentCode, document.EmissionPointCode, sequential, numericCode, document.EmissionType));
        var xml = GenerateUnsignedInvoiceXml(document, issuer, sequential, accessKey.Value);
        document.Generate(sequential, accessKey, xml, DateTimeOffset.UtcNow);
        await documents.SaveChangesAsync(ct);
        await AuditOutboxAsync("ElectronicDocumentXmlGenerated", "ElectronicDocumentXmlGenerated", document, context, ct);
        return ToDto(document);
    }

    public async Task<ElectronicDocumentDto> SignElectronicDocumentAsync(Guid id, PortalCallContext context, CancellationToken ct)
    {
        var document = await GetRequiredAsync(id, context.TenantId, ct);
        document.EnsureCanSign();
        var issuer = await GetIssuerAsync(context, ct);
        var xml = xmlGenerator.GenerateInvoiceXml(document, issuer);
        var signed = await signature.SignAsync(xml, await GetSignatureContextAsync(context, ct), ct);
        document.MarkSigned(signed, DateTimeOffset.UtcNow);
        await documents.SaveChangesAsync(ct);
        await AuditOutboxAsync("ElectronicDocumentSigned", "ElectronicDocumentSigned", document, context, ct);
        return ToDto(document);
    }

    public async Task<ElectronicDocumentDto> SendElectronicDocumentAsync(Guid id, PortalCallContext context, CancellationToken ct)
    {
        var document = await GetRequiredAsync(id, context.TenantId, ct);
        document.EnsureCanSend();
        var issuer = await GetIssuerAsync(context, ct);
        var signedXml = await signature.SignAsync(xmlGenerator.GenerateInvoiceXml(document, issuer), await GetSignatureContextAsync(context, ct), ct);
        var result = await reception.SendAsync(signedXml, await GetSriClientContextAsync(document.Environment, context, ct), ct);
        if (string.Equals(result.Status, "Rejected", StringComparison.OrdinalIgnoreCase)) document.MarkRejected(result.Code, result.Message, DateTimeOffset.UtcNow);
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
        var result = await authorization.AuthorizeAsync(document.AccessKey, await GetSriClientContextAsync(document.Environment, context, ct), ct);
        if (result.Authorized) document.MarkAuthorized(result.AuthorizationNumber ?? document.AccessKey, result.AuthorizationDate ?? DateTimeOffset.UtcNow, result.Code, result.Message, DateTimeOffset.UtcNow);
        else document.MarkRejected(result.Code, result.Message, DateTimeOffset.UtcNow);
        await documents.SaveChangesAsync(ct);
        await AuditOutboxAsync(result.Authorized ? "ElectronicDocumentAuthorized" : "ElectronicDocumentRejected", result.Authorized ? "ElectronicDocumentAuthorized" : "ElectronicDocumentRejected", document, context, ct);
        return ToDto(document);
    }

    public async Task<ElectronicDocumentDto> GetByIdAsync(Guid id, PortalCallContext context, CancellationToken ct) => ToDto(await GetRequiredAsync(id, context.TenantId, ct));
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
            await configuration.GetStringAsync("financial.sri.signature.provider", "Development", context, ct),
            await configuration.GetStringAsync("financial.sri.signature.certificateSecretName", "", context, ct),
            await configuration.GetStringAsync("financial.sri.signature.keyVaultName", "", context, ct));
    private async Task<SriClientContext> GetSriClientContextAsync(SriEnvironment environment, PortalCallContext context, CancellationToken ct) =>
        new(context.TenantId, environment,
            await configuration.GetStringAsync("financial.sri.integration.mode", "Development", context, ct),
            await configuration.GetStringAsync("financial.sri.receptionUrl", "", context, ct),
            await configuration.GetStringAsync("financial.sri.authorizationUrl", "", context, ct),
            await configuration.GetIntAsync("financial.sri.timeoutSeconds", 30, context, ct));
    private async Task AuditOnlyAsync(string auditAction, ElectronicDocument document, PortalCallContext context, CancellationToken ct) =>
        await audit.RecordAsync(new(auditAction, "financial.electronic-document", document.Id.ToString(), new { document.Id, document.AccessKey, document.Status }), context, ct);
    private async Task AuditOutboxAsync(string auditAction, string outboxType, ElectronicDocument document, PortalCallContext context, CancellationToken ct)
    {
        await audit.RecordAsync(new(auditAction, "financial.electronic-document", document.Id.ToString(), new { document.Id, document.AccessKey, document.Status }), context, ct);
        await outbox.EnqueueAsync(new(Guid.NewGuid(), outboxType, 1, DateTimeOffset.UtcNow, context.CorrelationId,
            JsonSerializer.Serialize(new { document.Id, document.TenantId, document.AccessKey, Status = document.Status.ToString() })), context, ct);
    }
    private static string DeterministicNumericCode(Guid id) => Math.Abs(BitConverter.ToInt32(id.ToByteArray(), 0)).ToString("00000000", CultureInfo.InvariantCulture)[..8];
    private static string GenerateUnsignedInvoiceXml(ElectronicDocument document, IssuerSriOptions issuer, string sequential, string accessKey)
    {
        var infoTributaria = $"""
<infoTributaria><ambiente>{(int)document.Environment}</ambiente><tipoEmision>{(int)document.EmissionType}</tipoEmision><razonSocial>{Xml(issuer.LegalName)}</razonSocial><nombreComercial>{Xml(issuer.TradeName ?? issuer.LegalName)}</nombreComercial><ruc>{issuer.Ruc}</ruc><claveAcceso>{accessKey}</claveAcceso><codDoc>{SriDocumentCodes.ToCode(document.DocumentType)}</codDoc><estab>{document.EstablishmentCode}</estab><ptoEmi>{document.EmissionPointCode}</ptoEmi><secuencial>{sequential}</secuencial><dirMatriz>{Xml(issuer.Address)}</dirMatriz></infoTributaria>
""";
        var detalles = string.Concat(document.Lines.Select(line => $"<detalle><codigoPrincipal>{Xml(line.ProductCode)}</codigoPrincipal><descripcion>{Xml(line.Description)}</descripcion><cantidad>{line.Quantity:0.00##}</cantidad><precioUnitario>{line.UnitPrice:0.00##}</precioUnitario><descuento>{line.Discount:0.00##}</descuento><precioTotalSinImpuesto>{line.Subtotal:0.00##}</precioTotalSinImpuesto></detalle>"));
        return $"""<?xml version="1.0" encoding="UTF-8"?><factura id="comprobante" version="1.0.0">{infoTributaria}<infoFactura><fechaEmision>{document.IssueDate:dd/MM/yyyy}</fechaEmision><dirEstablecimiento>{Xml(issuer.Address)}</dirEstablecimiento><obligadoContabilidad>{(issuer.AccountingRequired ? "SI" : "NO")}</obligadoContabilidad><tipoIdentificacionComprador>{document.CustomerIdentificationType}</tipoIdentificacionComprador><razonSocialComprador>{Xml(document.CustomerName)}</razonSocialComprador><identificacionComprador>{document.CustomerIdentification}</identificacionComprador><totalSinImpuestos>{document.SubtotalWithoutTaxes:0.00##}</totalSinImpuestos><totalDescuento>{document.TotalDiscount:0.00##}</totalDescuento><propina>0.00</propina><importeTotal>{document.TotalAmount:0.00##}</importeTotal><moneda>{document.Currency}</moneda></infoFactura><detalles>{detalles}</detalles></factura>""";
    }
    private static string Xml(string value) => HtmlEncoder.Default.Encode(value);
    public static ElectronicDocumentDto ToDto(ElectronicDocument document) => new(document.Id, document.TenantId, document.DocumentType.ToString(), document.Environment.ToString(), document.EmissionType.ToString(), document.Status.ToString(),
        document.EstablishmentCode, document.EmissionPointCode, document.Sequential, document.AccessKey, document.IssueDate, document.CustomerIdentificationType, document.CustomerIdentification, document.CustomerName, document.Currency,
        document.SubtotalWithoutTaxes, document.TotalDiscount, document.TotalTaxes, document.TotalAmount, document.SriAuthorizationNumber, document.SriAuthorizationDate, document.SriResponseCode, document.SriResponseMessage,
        document.Lines.Select(x => new ElectronicDocumentLineDto(x.Id, x.LineNumber, x.ProductCode, x.Description, x.Quantity, x.UnitPrice, x.Discount, x.Subtotal, x.Total)).ToArray());
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
        "financial.sri.issuer.ruc", "financial.sri.issuer.legalName", "financial.sri.issuer.tradeName", "financial.sri.issuer.address",
        "financial.sri.issuer.specialTaxpayerNumber", "financial.sri.issuer.accountingRequired", "financial.sri.environment",
        "financial.sri.integration.mode", "financial.sri.emissionType", "financial.sri.defaultEstablishmentCode",
        "financial.sri.defaultEmissionPointCode", "financial.sri.signature.provider", "financial.sri.signature.certificateSecretName",
        "financial.sri.signature.keyVaultName", "financial.sri.receptionUrl", "financial.sri.authorizationUrl",
        "financial.sri.timeoutSeconds", "financial.sri.xml.version.invoice", "financial.sri.xml.includeOptionalFields"
    ];
}
