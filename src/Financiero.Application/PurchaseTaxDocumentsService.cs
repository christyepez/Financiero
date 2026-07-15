using Financiero.Domain;

namespace Financiero.Application;

public sealed record CreatePurchaseTaxDocumentRequest(string SupplierIdentificationType, string SupplierIdentification, string SupplierName, string Establishment, string EmissionPoint, string Sequential, string DocumentType, DateOnly IssueDate, DateOnly RegistrationDate, string FiscalPeriod, string SupportDocumentType, decimal Subtotal, decimal TaxTotal, decimal Total, string Currency = "USD", string? AuthorizationNumber = null, string? AccessKey = null);
public sealed record AddPurchaseLineRequest(string ProductCode, string Description, decimal Quantity, decimal UnitPrice, decimal Discount = 0);
public sealed record AddPurchaseTaxRequest(string TaxCode, string TaxPercentageCode, decimal TaxBase, decimal TaxRate, decimal TaxAmount);
public sealed record PurchaseTaxDocumentLineDto(Guid Id, int LineNumber, string ProductCode, string Description, decimal Quantity, decimal UnitPrice, decimal Discount, decimal Subtotal);
public sealed record PurchaseTaxDto(Guid Id, string TaxCode, string TaxPercentageCode, decimal TaxBase, decimal TaxRate, decimal TaxAmount);
public sealed record PurchaseTaxDocumentDto(Guid Id, string DocumentType, string DocumentNumber, string SupplierIdentificationType, string SupplierIdentificationMasked, string SupplierName, DateOnly IssueDate, DateOnly RegistrationDate, string FiscalPeriod, decimal Subtotal, decimal TaxTotal, decimal Total, string Currency, string Status, string? AccessKeyMasked, string? AuthorizationNumberMasked, IReadOnlyCollection<PurchaseTaxDocumentLineDto> Lines, IReadOnlyCollection<PurchaseTaxDto> Taxes, string Disclaimer);

public sealed record RegisterVoidedTaxDocumentRequest(string DocumentType, string Establishment, string EmissionPoint, string Sequential, DateOnly IssueDate, DateOnly VoidDate, string FiscalPeriod, string Reason, Guid? SourceDocumentId = null, string? AuthorizationNumber = null, string? AccessKey = null);
public sealed record VoidedTaxDocumentDto(Guid Id, string DocumentType, string DocumentNumber, DateOnly IssueDate, DateOnly VoidDate, string FiscalPeriod, string Reason, string Status, string? AccessKeyMasked, string? AuthorizationNumberMasked, string Disclaimer);

public interface IPurchaseTaxDocumentRepository
{
    Task AddAsync(PurchaseTaxDocument document, CancellationToken ct);
    Task AddLineAsync(PurchaseTaxDocumentLine line, CancellationToken ct);
    Task AddTaxAsync(PurchaseTax tax, CancellationToken ct);
    Task AddReferenceAsync(PurchaseSupportDocumentReference reference, CancellationToken ct);
    Task<PurchaseTaxDocument?> GetByIdAsync(Guid id, string tenantId, CancellationToken ct);
    Task<IReadOnlyCollection<PurchaseTaxDocument>> GetByPeriodAsync(string tenantId, string period, CancellationToken ct);
    Task<PurchaseTaxSummary> GetSummaryAsync(string tenantId, DateOnly from, DateOnly to, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}

public interface IVoidedTaxDocumentRepository
{
    Task AddAsync(VoidedTaxDocument document, CancellationToken ct);
    Task<VoidedTaxDocument?> GetByIdAsync(Guid id, string tenantId, CancellationToken ct);
    Task<IReadOnlyCollection<VoidedTaxDocument>> GetByPeriodAsync(string tenantId, string period, CancellationToken ct);
    Task<int> CountByPeriodAsync(string tenantId, DateOnly from, DateOnly to, CancellationToken ct);
    Task<bool> ExistsDocumentNumberAsync(string tenantId, string documentType, string establishment, string emissionPoint, string sequential, Guid? excludingId, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}

public sealed record PurchaseTaxSummary(int Count, decimal Subtotal, decimal TaxTotal, decimal Total);

public sealed class PurchaseTaxDocumentService(IPurchaseTaxDocumentRepository purchases, IPortalAuditClient audit, IPortalOutboxClient outbox)
{
    private const string Disclaimer = "Purchase tax document foundation only. It is not official ATS generation and does not certify tax compliance.";

    public async Task<PurchaseTaxDocumentDto> CreateAsync(CreatePurchaseTaxDocumentRequest request, PortalCallContext context, CancellationToken ct)
    {
        var document = PurchaseTaxDocument.Create(context.TenantId, request.SupplierIdentificationType, request.SupplierIdentification, request.SupplierName, request.Establishment, request.EmissionPoint, request.Sequential, request.DocumentType, request.IssueDate, request.RegistrationDate, request.FiscalPeriod, request.SupportDocumentType, request.Subtotal, request.TaxTotal, request.Total, request.Currency, request.AuthorizationNumber, request.AccessKey, DateTimeOffset.UtcNow);
        await purchases.AddAsync(document, ct);
        await purchases.SaveChangesAsync(ct);
        await AuditOutboxAsync("PurchaseTaxDocumentCreated", document, context, ct);
        return ToDto(document);
    }

    public async Task<PurchaseTaxDocumentDto> AddLineAsync(Guid id, AddPurchaseLineRequest request, PortalCallContext context, CancellationToken ct)
    {
        var document = await GetDocumentAsync(id, context, ct);
        var line = document.AddLine(request.ProductCode, request.Description, request.Quantity, request.UnitPrice, request.Discount, DateTimeOffset.UtcNow);
        await purchases.AddLineAsync(line, ct);
        await purchases.SaveChangesAsync(ct);
        return ToDto(document);
    }

    public async Task<PurchaseTaxDocumentDto> AddTaxAsync(Guid id, AddPurchaseTaxRequest request, PortalCallContext context, CancellationToken ct)
    {
        var document = await GetDocumentAsync(id, context, ct);
        var tax = document.AddTax(request.TaxCode, request.TaxPercentageCode, request.TaxBase, request.TaxRate, request.TaxAmount, DateTimeOffset.UtcNow);
        await purchases.AddTaxAsync(tax, ct);
        await purchases.SaveChangesAsync(ct);
        return ToDto(document);
    }

    public async Task<PurchaseTaxDocumentDto> ValidateAsync(Guid id, PortalCallContext context, CancellationToken ct)
    {
        var document = await GetDocumentAsync(id, context, ct);
        document.ValidateFoundation(DateTimeOffset.UtcNow);
        await purchases.SaveChangesAsync(ct);
        await AuditOutboxAsync("PurchaseTaxDocumentValidated", document, context, ct);
        return ToDto(document);
    }

    public async Task<PurchaseTaxDocumentDto> ArchiveAsync(Guid id, PortalCallContext context, CancellationToken ct)
    {
        var document = await GetDocumentAsync(id, context, ct);
        document.Archive(DateTimeOffset.UtcNow);
        await purchases.SaveChangesAsync(ct);
        await AuditOutboxAsync("PurchaseTaxDocumentArchived", document, context, ct);
        return ToDto(document);
    }

    public async Task<IReadOnlyCollection<PurchaseTaxDocumentDto>> GetByPeriodAsync(string period, PortalCallContext context, CancellationToken ct)
    {
        if (!PurchaseTaxDocumentValidator.IsFiscalPeriod(period)) throw new FinancialApplicationException("purchase.period.invalid", "Purchase period must use YYYY-MM.");
        return (await purchases.GetByPeriodAsync(context.TenantId, period, ct)).Select(ToDto).ToArray();
    }

    public async Task<PurchaseTaxDocumentDto> GetByIdAsync(Guid id, PortalCallContext context, CancellationToken ct) => ToDto(await GetDocumentAsync(id, context, ct));

    private async Task<PurchaseTaxDocument> GetDocumentAsync(Guid id, PortalCallContext context, CancellationToken ct) =>
        await purchases.GetByIdAsync(id, context.TenantId, ct) ?? throw new FinancialApplicationException("purchase.not_found", "Purchase tax document was not found.");

    private async Task AuditOutboxAsync(string action, PurchaseTaxDocument document, PortalCallContext context, CancellationToken ct)
    {
        var metadata = new { document.Id, document.DocumentType, DocumentNumber = $"{document.Establishment}-{document.EmissionPoint}-{document.Sequential}", document.FiscalPeriod, SupplierIdentification = SriSensitiveDataSanitizer.MaskCustomerIdentification(document.SupplierIdentification), AccessKey = SriSensitiveDataSanitizer.MaskAccessKey(document.AccessKey) };
        await audit.RecordAsync(new(action, "financial.purchase-tax-document", context.TenantId, metadata), context, ct);
        await outbox.EnqueueAsync(new(Guid.NewGuid(), action, 1, DateTimeOffset.UtcNow, context.CorrelationId, System.Text.Json.JsonSerializer.Serialize(metadata)), context, ct);
    }

    private static PurchaseTaxDocumentDto ToDto(PurchaseTaxDocument document) => new(document.Id, document.DocumentType, $"{document.Establishment}-{document.EmissionPoint}-{document.Sequential}", document.SupplierIdentificationType, SriSensitiveDataSanitizer.MaskCustomerIdentification(document.SupplierIdentification) ?? "", document.SupplierName, document.IssueDate, document.RegistrationDate, document.FiscalPeriod, document.Subtotal, document.TaxTotal, document.Total, document.Currency, document.Status.ToString(), SriSensitiveDataSanitizer.MaskAccessKey(document.AccessKey), SriSensitiveDataSanitizer.MaskAccessKey(document.AuthorizationNumber), document.Lines.Select(x => new PurchaseTaxDocumentLineDto(x.Id, x.LineNumber, x.ProductCode, x.Description, x.Quantity, x.UnitPrice, x.Discount, x.Subtotal)).ToArray(), document.Taxes.Select(x => new PurchaseTaxDto(x.Id, x.TaxCode, x.TaxPercentageCode, x.TaxBase, x.TaxRate, x.TaxAmount)).ToArray(), Disclaimer);
}

public sealed class VoidedTaxDocumentService(IVoidedTaxDocumentRepository voided, IPortalAuditClient audit, IPortalOutboxClient outbox)
{
    private const string Disclaimer = "Voided tax document foundation only. It is not an official SRI annulment and does not certify tax compliance.";

    public async Task<VoidedTaxDocumentDto> RegisterAsync(RegisterVoidedTaxDocumentRequest request, PortalCallContext context, CancellationToken ct)
    {
        if (await voided.ExistsDocumentNumberAsync(context.TenantId, request.DocumentType, request.Establishment, request.EmissionPoint, request.Sequential, null, ct)) throw new FinancialApplicationException("voided.duplicate", "Voided document already exists for this tenant and document number.");
        var document = VoidedTaxDocument.RegisterFoundation(context.TenantId, request.DocumentType, request.Establishment, request.EmissionPoint, request.Sequential, request.IssueDate, request.VoidDate, request.FiscalPeriod, request.Reason, request.SourceDocumentId, request.AuthorizationNumber, request.AccessKey, DateTimeOffset.UtcNow);
        await voided.AddAsync(document, ct);
        await voided.SaveChangesAsync(ct);
        var metadata = new { document.Id, document.DocumentType, document.DocumentNumber, document.FiscalPeriod, AccessKey = SriSensitiveDataSanitizer.MaskAccessKey(document.AccessKey) };
        await audit.RecordAsync(new("VoidedTaxDocumentRegistered", "financial.voided-tax-document", context.TenantId, metadata), context, ct);
        await outbox.EnqueueAsync(new(Guid.NewGuid(), "VoidedTaxDocumentRegistered", 1, DateTimeOffset.UtcNow, context.CorrelationId, System.Text.Json.JsonSerializer.Serialize(metadata)), context, ct);
        return ToDto(document);
    }

    public async Task<IReadOnlyCollection<VoidedTaxDocumentDto>> GetByPeriodAsync(string period, PortalCallContext context, CancellationToken ct)
    {
        if (!PurchaseTaxDocumentValidator.IsFiscalPeriod(period)) throw new FinancialApplicationException("voided.period.invalid", "Voided period must use YYYY-MM.");
        return (await voided.GetByPeriodAsync(context.TenantId, period, ct)).Select(ToDto).ToArray();
    }

    public async Task<VoidedTaxDocumentDto> GetByIdAsync(Guid id, PortalCallContext context, CancellationToken ct) =>
        ToDto(await voided.GetByIdAsync(id, context.TenantId, ct) ?? throw new FinancialApplicationException("voided.not_found", "Voided tax document was not found."));

    private static VoidedTaxDocumentDto ToDto(VoidedTaxDocument document) => new(document.Id, document.DocumentType, document.DocumentNumber, document.IssueDate, document.VoidDate, document.FiscalPeriod, document.Reason, document.Status.ToString(), SriSensitiveDataSanitizer.MaskAccessKey(document.AccessKey), SriSensitiveDataSanitizer.MaskAccessKey(document.AuthorizationNumber), Disclaimer);
}
