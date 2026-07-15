namespace Financiero.Domain;

public enum PurchaseTaxDocumentStatus { Draft, Validated, Archived, Voided, RejectedFoundation }
public enum VoidedTaxDocumentStatus { Draft, RegisteredFoundation, Archived }

public sealed class PurchaseTaxDocument
{
    private readonly List<PurchaseTaxDocumentLine> _lines = [];
    private readonly List<PurchaseTax> _taxes = [];
    private readonly List<PurchaseSupportDocumentReference> _references = [];
    private PurchaseTaxDocument() { TenantId = ""; SupplierIdentificationType = ""; SupplierIdentification = ""; SupplierName = ""; Establishment = ""; EmissionPoint = ""; Sequential = ""; DocumentType = ""; FiscalPeriod = ""; SupportDocumentType = ""; Currency = ""; }
    private PurchaseTaxDocument(string tenantId, string supplierIdentificationType, string supplierIdentification, string supplierName, string establishment, string emissionPoint, string sequential, string documentType, DateOnly issueDate, DateOnly registrationDate, string fiscalPeriod, string supportDocumentType, decimal subtotal, decimal taxTotal, decimal total, string currency, DateTimeOffset now)
    {
        Id = Guid.NewGuid();
        TenantId = Required(tenantId, "purchase.tenant.required");
        SupplierIdentificationType = supplierIdentificationType.Trim();
        SupplierIdentification = Required(supplierIdentification, "purchase.supplier_identification.required");
        SupplierName = Required(supplierName, "purchase.supplier_name.required");
        Establishment = establishment.Trim();
        EmissionPoint = emissionPoint.Trim();
        Sequential = sequential.Trim();
        DocumentType = documentType.Trim();
        IssueDate = issueDate;
        RegistrationDate = registrationDate;
        FiscalPeriod = fiscalPeriod.Trim();
        SupportDocumentType = supportDocumentType.Trim();
        Subtotal = FinancialPrecision.Normalize(subtotal);
        TaxTotal = FinancialPrecision.Normalize(taxTotal);
        Total = FinancialPrecision.Normalize(total);
        Currency = string.IsNullOrWhiteSpace(currency) ? "USD" : currency.Trim().ToUpperInvariant();
        Status = PurchaseTaxDocumentStatus.Draft;
        CreatedAtUtc = UpdatedAtUtc = now;
        ValidateFoundationRules();
    }

    public Guid Id { get; private set; }
    public string TenantId { get; private set; }
    public string SupplierIdentificationType { get; private set; }
    public string SupplierIdentification { get; private set; }
    public string SupplierName { get; private set; }
    public string Establishment { get; private set; }
    public string EmissionPoint { get; private set; }
    public string Sequential { get; private set; }
    public string DocumentType { get; private set; }
    public string? AuthorizationNumber { get; private set; }
    public string? AccessKey { get; private set; }
    public DateOnly IssueDate { get; private set; }
    public DateOnly RegistrationDate { get; private set; }
    public string FiscalPeriod { get; private set; }
    public string SupportDocumentType { get; private set; }
    public decimal Subtotal { get; private set; }
    public decimal TaxTotal { get; private set; }
    public decimal Total { get; private set; }
    public string Currency { get; private set; }
    public PurchaseTaxDocumentStatus Status { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }
    public IReadOnlyCollection<PurchaseTaxDocumentLine> Lines => _lines.AsReadOnly();
    public IReadOnlyCollection<PurchaseTax> Taxes => _taxes.AsReadOnly();
    public IReadOnlyCollection<PurchaseSupportDocumentReference> References => _references.AsReadOnly();

    public static PurchaseTaxDocument Create(string tenantId, string supplierIdentificationType, string supplierIdentification, string supplierName, string establishment, string emissionPoint, string sequential, string documentType, DateOnly issueDate, DateOnly registrationDate, string fiscalPeriod, string supportDocumentType, decimal subtotal, decimal taxTotal, decimal total, string currency, string? authorizationNumber, string? accessKey, DateTimeOffset now)
    {
        var document = new PurchaseTaxDocument(tenantId, supplierIdentificationType, supplierIdentification, supplierName, establishment, emissionPoint, sequential, documentType, issueDate, registrationDate, fiscalPeriod, supportDocumentType, subtotal, taxTotal, total, currency, now);
        document.AuthorizationNumber = Clean(authorizationNumber);
        document.AccessKey = Clean(accessKey);
        if (!string.IsNullOrWhiteSpace(document.AccessKey) && document.AccessKey.Length != 49) throw new FinancialDomainException("purchase.access_key.invalid", "Purchase access key must have 49 digits when provided.");
        return document;
    }

    public PurchaseTaxDocumentLine AddLine(string productCode, string description, decimal quantity, decimal unitPrice, decimal discount, DateTimeOffset now)
    {
        var lineNumber = _lines.Count + 1;
        var subtotal = FinancialPrecision.Normalize(quantity * unitPrice - discount);
        if (subtotal < 0) throw new FinancialDomainException("purchase.line.total.invalid", "Purchase line subtotal cannot be negative.");
        var line = new PurchaseTaxDocumentLine(Id, TenantId, lineNumber, Required(productCode, "purchase.line.product_code.required"), Required(description, "purchase.line.description.required"), quantity, unitPrice, discount, subtotal);
        _lines.Add(line);
        UpdatedAtUtc = now;
        return line;
    }

    public PurchaseTax AddTax(string taxCode, string taxPercentageCode, decimal taxBase, decimal taxRate, decimal taxAmount, DateTimeOffset now)
    {
        if (!SriCatalogService.IsTaxCodeAllowed(taxCode)) throw new FinancialDomainException("purchase.tax.code.invalid", "Purchase tax code is not allowed by foundation catalog.");
        var tax = new PurchaseTax(Id, TenantId, Required(taxCode, "purchase.tax.code.required"), Required(taxPercentageCode, "purchase.tax.percentage.required"), taxBase, taxRate, taxAmount);
        _taxes.Add(tax);
        UpdatedAtUtc = now;
        return tax;
    }

    public PurchaseSupportDocumentReference AddReference(string documentTypeCode, string number, DateOnly issueDate, string reason, DateTimeOffset now)
    {
        var reference = new PurchaseSupportDocumentReference(Id, TenantId, Required(documentTypeCode, "purchase.reference.type.required"), Required(number, "purchase.reference.number.required"), issueDate, Required(reason, "purchase.reference.reason.required"));
        _references.Add(reference);
        UpdatedAtUtc = now;
        return reference;
    }

    public void ValidateFoundation(DateTimeOffset now)
    {
        ValidateFoundationRules();
        PurchaseTaxCalculationValidator.Validate(this);
        Status = PurchaseTaxDocumentStatus.Validated;
        UpdatedAtUtc = now;
    }

    public void Archive(DateTimeOffset now)
    {
        Status = PurchaseTaxDocumentStatus.Archived;
        UpdatedAtUtc = now;
    }

    private void ValidateFoundationRules()
    {
        if (!SriCatalogService.IsIdentificationTypeAllowed(SupplierIdentificationType)) throw new FinancialDomainException("purchase.supplier_identification_type.invalid", "Supplier identification type is not allowed by foundation catalog.");
        if (!SriCatalogService.IsDocumentTypeAllowed(DocumentType)) throw new FinancialDomainException("purchase.document_type.invalid", "Purchase document type is not allowed by foundation catalog.");
        if (!SriCatalogService.IsSupportDocumentTypeAllowed(SupportDocumentType)) throw new FinancialDomainException("purchase.support_document_type.invalid", "Purchase support document type is not allowed by foundation catalog.");
        if (!PurchaseTaxDocumentValidator.IsFiscalPeriod(FiscalPeriod)) throw new FinancialDomainException("purchase.fiscal_period.invalid", "Purchase fiscal period must use YYYY-MM.");
        if (!PurchaseTaxDocumentValidator.IsDocumentNumber(Establishment, EmissionPoint, Sequential)) throw new FinancialDomainException("purchase.document_number.invalid", "Purchase document number must use ###-###-######### parts.");
        if (IssueDate > DateOnly.FromDateTime(DateTime.UtcNow.Date)) throw new FinancialDomainException("purchase.issue_date.future", "Purchase issue date cannot be in the future.");
        if (Subtotal < 0 || TaxTotal < 0 || Total < 0) throw new FinancialDomainException("purchase.totals.negative", "Purchase totals cannot be negative.");
    }

    private static string Required(string value, string code) => string.IsNullOrWhiteSpace(value) ? throw new FinancialDomainException(code, code) : value.Trim();
    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

public sealed class PurchaseTaxDocumentLine
{
    private PurchaseTaxDocumentLine() { TenantId = ""; ProductCode = ""; Description = ""; }
    internal PurchaseTaxDocumentLine(Guid purchaseTaxDocumentId, string tenantId, int lineNumber, string productCode, string description, decimal quantity, decimal unitPrice, decimal discount, decimal subtotal)
    {
        Id = Guid.NewGuid(); PurchaseTaxDocumentId = purchaseTaxDocumentId; TenantId = tenantId; LineNumber = lineNumber; ProductCode = productCode; Description = description;
        Quantity = quantity; UnitPrice = unitPrice; Discount = discount; Subtotal = subtotal;
    }
    public Guid Id { get; private set; }
    public Guid PurchaseTaxDocumentId { get; private set; }
    public string TenantId { get; private set; }
    public int LineNumber { get; private set; }
    public string ProductCode { get; private set; }
    public string Description { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal Discount { get; private set; }
    public decimal Subtotal { get; private set; }
}

public sealed class PurchaseTax
{
    private PurchaseTax() { TenantId = ""; TaxCode = ""; TaxPercentageCode = ""; }
    internal PurchaseTax(Guid purchaseTaxDocumentId, string tenantId, string taxCode, string taxPercentageCode, decimal taxBase, decimal taxRate, decimal taxAmount)
    {
        Id = Guid.NewGuid(); PurchaseTaxDocumentId = purchaseTaxDocumentId; TenantId = tenantId; TaxCode = taxCode; TaxPercentageCode = taxPercentageCode;
        TaxBase = FinancialPrecision.Normalize(taxBase); TaxRate = FinancialPrecision.Normalize(taxRate); TaxAmount = FinancialPrecision.Normalize(taxAmount);
    }
    public Guid Id { get; private set; }
    public Guid PurchaseTaxDocumentId { get; private set; }
    public string TenantId { get; private set; }
    public string TaxCode { get; private set; }
    public string TaxPercentageCode { get; private set; }
    public decimal TaxBase { get; private set; }
    public decimal TaxRate { get; private set; }
    public decimal TaxAmount { get; private set; }
}

public sealed class PurchaseSupportDocumentReference
{
    private PurchaseSupportDocumentReference() { TenantId = ""; DocumentTypeCode = ""; Number = ""; Reason = ""; }
    internal PurchaseSupportDocumentReference(Guid purchaseTaxDocumentId, string tenantId, string documentTypeCode, string number, DateOnly issueDate, string reason)
    {
        Id = Guid.NewGuid(); PurchaseTaxDocumentId = purchaseTaxDocumentId; TenantId = tenantId; DocumentTypeCode = documentTypeCode; Number = number; IssueDate = issueDate; Reason = reason;
    }
    public Guid Id { get; private set; }
    public Guid PurchaseTaxDocumentId { get; private set; }
    public string TenantId { get; private set; }
    public string DocumentTypeCode { get; private set; }
    public string Number { get; private set; }
    public DateOnly IssueDate { get; private set; }
    public string Reason { get; private set; }
}

public sealed class VoidedTaxDocument
{
    private VoidedTaxDocument() { TenantId = ""; DocumentType = ""; Establishment = ""; EmissionPoint = ""; Sequential = ""; FiscalPeriod = ""; Reason = ""; }
    private VoidedTaxDocument(string tenantId, string documentType, string establishment, string emissionPoint, string sequential, DateOnly issueDate, DateOnly voidDate, string fiscalPeriod, string reason, Guid? sourceDocumentId, string? authorizationNumber, string? accessKey, DateTimeOffset now)
    {
        Id = Guid.NewGuid(); TenantId = Required(tenantId, "voided.tenant.required"); DocumentType = Required(documentType, "voided.document_type.required");
        Establishment = Required(establishment, "voided.establishment.required"); EmissionPoint = Required(emissionPoint, "voided.emission_point.required"); Sequential = Required(sequential, "voided.sequential.required");
        IssueDate = issueDate; VoidDate = voidDate; FiscalPeriod = Required(fiscalPeriod, "voided.fiscal_period.required"); Reason = Required(reason, "voided.reason.required");
        SourceDocumentId = sourceDocumentId; AuthorizationNumber = Clean(authorizationNumber); AccessKey = Clean(accessKey); Status = VoidedTaxDocumentStatus.Draft; CreatedAtUtc = now;
        ValidateFoundationRules();
    }
    public Guid Id { get; private set; }
    public string TenantId { get; private set; }
    public string DocumentType { get; private set; }
    public string Establishment { get; private set; }
    public string EmissionPoint { get; private set; }
    public string Sequential { get; private set; }
    public string? AccessKey { get; private set; }
    public string? AuthorizationNumber { get; private set; }
    public DateOnly IssueDate { get; private set; }
    public DateOnly VoidDate { get; private set; }
    public string FiscalPeriod { get; private set; }
    public string Reason { get; private set; }
    public Guid? SourceDocumentId { get; private set; }
    public VoidedTaxDocumentStatus Status { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public string DocumentNumber => $"{Establishment}-{EmissionPoint}-{Sequential}";

    public static VoidedTaxDocument RegisterFoundation(string tenantId, string documentType, string establishment, string emissionPoint, string sequential, DateOnly issueDate, DateOnly voidDate, string fiscalPeriod, string reason, Guid? sourceDocumentId, string? authorizationNumber, string? accessKey, DateTimeOffset now)
    {
        var document = new VoidedTaxDocument(tenantId, documentType, establishment, emissionPoint, sequential, issueDate, voidDate, fiscalPeriod, reason, sourceDocumentId, authorizationNumber, accessKey, now);
        document.Status = VoidedTaxDocumentStatus.RegisteredFoundation;
        return document;
    }
    private void ValidateFoundationRules()
    {
        if (!SriCatalogService.IsDocumentTypeAllowed(DocumentType)) throw new FinancialDomainException("voided.document_type.invalid", "Voided document type is not allowed by foundation catalog.");
        if (!PurchaseTaxDocumentValidator.IsFiscalPeriod(FiscalPeriod)) throw new FinancialDomainException("voided.fiscal_period.invalid", "Voided fiscal period must use YYYY-MM.");
        if (!PurchaseTaxDocumentValidator.IsDocumentNumber(Establishment, EmissionPoint, Sequential)) throw new FinancialDomainException("voided.document_number.invalid", "Voided document number must use ###-###-######### parts.");
        if (VoidDate > DateOnly.FromDateTime(DateTime.UtcNow.Date)) throw new FinancialDomainException("voided.date.future", "Void date cannot be in the future.");
        if (VoidDate < IssueDate) throw new FinancialDomainException("voided.date.before_issue", "Void date cannot be before issue date.");
        if (!string.IsNullOrWhiteSpace(AccessKey) && AccessKey.Length != 49) throw new FinancialDomainException("voided.access_key.invalid", "Voided access key must have 49 digits when provided.");
    }
    private static string Required(string value, string code) => string.IsNullOrWhiteSpace(value) ? throw new FinancialDomainException(code, code) : value.Trim();
    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

public static class PurchaseTaxDocumentValidator
{
    public static bool IsFiscalPeriod(string value) => System.Text.RegularExpressions.Regex.IsMatch(value ?? "", "^\\d{4}-\\d{2}$");
    public static bool IsDocumentNumber(string establishment, string emissionPoint, string sequential) =>
        System.Text.RegularExpressions.Regex.IsMatch(establishment ?? "", "^\\d{3}$") && System.Text.RegularExpressions.Regex.IsMatch(emissionPoint ?? "", "^\\d{3}$") && System.Text.RegularExpressions.Regex.IsMatch(sequential ?? "", "^\\d{9}$");
}

public static class PurchaseTaxCalculationValidator
{
    public static void Validate(PurchaseTaxDocument document)
    {
        const decimal tolerance = 0.01m;
        var lineSubtotal = document.Lines.Sum(x => x.Subtotal);
        if (document.Lines.Count > 0 && Math.Abs(lineSubtotal - document.Subtotal) > tolerance) throw new FinancialDomainException("purchase.lines.subtotal_mismatch", "Purchase line subtotal does not match document subtotal.");
        var taxTotal = document.Taxes.Sum(x => x.TaxAmount);
        if (document.Taxes.Count > 0 && Math.Abs(taxTotal - document.TaxTotal) > tolerance) throw new FinancialDomainException("purchase.taxes.total_mismatch", "Purchase tax total does not match document tax total.");
        if (Math.Abs((document.Subtotal + document.TaxTotal) - document.Total) > tolerance) throw new FinancialDomainException("purchase.total.mismatch", "Purchase total must equal subtotal plus tax total within foundation tolerance.");
    }
}
