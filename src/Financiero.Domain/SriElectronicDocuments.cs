using System.Security.Cryptography;
using System.Text;

namespace Financiero.Domain;

public enum ElectronicDocumentStatus { Draft, Generated, SignedPending, Signed, Sent, Authorized, Rejected, Cancelled, Error }
public enum ElectronicDocumentType { Invoice, CreditNote, DebitNote, Withholding, ReferralGuide }
public enum SriEnvironment { Test = 1, Production = 2 }
public enum SriEmissionType { Normal = 1, Contingency = 2 }

public sealed class ElectronicDocument
{
    private readonly List<ElectronicDocumentLine> _lines = [];
    private readonly List<ElectronicDocumentTax> _taxes = [];

    private ElectronicDocument() { }

    private ElectronicDocument(
        Guid id,
        string tenantId,
        ElectronicDocumentType documentType,
        SriEnvironment environment,
        SriEmissionType emissionType,
        string establishmentCode,
        string emissionPointCode,
        DateOnly issueDate,
        string customerIdentificationType,
        string customerIdentification,
        string customerName,
        string currency,
        DateTimeOffset now)
    {
        Id = id;
        TenantId = Required(tenantId, nameof(TenantId));
        DocumentType = documentType;
        Environment = environment;
        EmissionType = emissionType;
        Status = ElectronicDocumentStatus.Draft;
        EstablishmentCode = ValidateFixedDigits(establishmentCode, 3, "electronic_document.establishment.invalid", "Establishment code must have 3 digits.");
        EmissionPointCode = ValidateFixedDigits(emissionPointCode, 3, "electronic_document.emission_point.invalid", "Emission point code must have 3 digits.");
        IssueDate = issueDate;
        CustomerIdentificationType = Required(customerIdentificationType, nameof(CustomerIdentificationType));
        CustomerIdentification = Required(customerIdentification, nameof(CustomerIdentification));
        CustomerName = Required(customerName, nameof(CustomerName));
        Currency = Required(currency, nameof(Currency));
        CreatedAtUtc = now;
        UpdatedAtUtc = now;
    }

    public Guid Id { get; private set; }
    public string TenantId { get; private set; } = FinancialTenant.Default;
    public ElectronicDocumentType DocumentType { get; private set; }
    public SriEnvironment Environment { get; private set; }
    public SriEmissionType EmissionType { get; private set; }
    public ElectronicDocumentStatus Status { get; private set; }
    public string EstablishmentCode { get; private set; } = "";
    public string EmissionPointCode { get; private set; } = "";
    public string? Sequential { get; private set; }
    public string? AccessKey { get; private set; }
    public DateOnly IssueDate { get; private set; }
    public string CustomerIdentificationType { get; private set; } = "";
    public string CustomerIdentification { get; private set; } = "";
    public string CustomerName { get; private set; } = "";
    public string Currency { get; private set; } = "USD";
    public decimal SubtotalWithoutTaxes { get; private set; }
    public decimal TotalDiscount { get; private set; }
    public decimal TotalTaxes { get; private set; }
    public decimal TotalAmount { get; private set; }
    public string? XmlContentHash { get; private set; }
    public string? SignedXmlContentHash { get; private set; }
    public string? SriAuthorizationNumber { get; private set; }
    public DateTimeOffset? SriAuthorizationDate { get; private set; }
    public string? SriResponseCode { get; private set; }
    public string? SriResponseMessage { get; private set; }
    public Guid? RelatedJournalEntryId { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }
    public DateTimeOffset? GeneratedAtUtc { get; private set; }
    public DateTimeOffset? SignedAtUtc { get; private set; }
    public DateTimeOffset? SentAtUtc { get; private set; }
    public DateTimeOffset? AuthorizedAtUtc { get; private set; }
    public DateTimeOffset? RejectedAtUtc { get; private set; }
    public IReadOnlyCollection<ElectronicDocumentLine> Lines => _lines.OrderBy(x => x.LineNumber).ToArray();
    public IReadOnlyCollection<ElectronicDocumentTax> Taxes => _taxes.ToArray();

    public static ElectronicDocument CreateInvoice(
        string tenantId,
        SriEnvironment environment,
        SriEmissionType emissionType,
        string establishmentCode,
        string emissionPointCode,
        DateOnly issueDate,
        string customerIdentificationType,
        string customerIdentification,
        string customerName,
        string currency,
        DateTimeOffset now) =>
        new(Guid.NewGuid(), tenantId, ElectronicDocumentType.Invoice, environment, emissionType, establishmentCode, emissionPointCode, issueDate,
            customerIdentificationType, customerIdentification, customerName, currency, now);

    public ElectronicDocumentLine AddLine(string productCode, string description, decimal quantity, decimal unitPrice, decimal discount, DateTimeOffset now)
    {
        EnsureEditable();
        var line = ElectronicDocumentLine.Create(Guid.NewGuid(), Id, TenantId, NextLineNumber(), productCode, description, quantity, unitPrice, discount, now);
        _lines.Add(line);
        RecalculateTotals(now);
        return line;
    }

    public void AddTax(Guid? lineId, string taxCode, string taxPercentageCode, decimal taxRate, decimal taxBase, decimal taxAmount, DateTimeOffset now)
    {
        EnsureEditable();
        _taxes.Add(ElectronicDocumentTax.Create(Guid.NewGuid(), TenantId, Id, lineId, taxCode, taxPercentageCode, taxRate, taxBase, taxAmount, now));
        RecalculateTotals(now);
    }

    public void Generate(string sequential, SriAccessKey accessKey, string unsignedXml, DateTimeOffset now)
    {
        EnsureEditable();
        if (DocumentType == ElectronicDocumentType.Invoice && _lines.Count == 0) throw new FinancialDomainException("electronic_document.invoice.lines.required", "Invoice requires at least one line.");
        Sequential = ValidateFixedDigits(sequential, 9, "electronic_document.sequential.invalid", "Sequential must have 9 digits.");
        AccessKey = accessKey.Value;
        XmlContentHash = Sha256(unsignedXml);
        Status = ElectronicDocumentStatus.Generated;
        GeneratedAtUtc = now;
        UpdatedAtUtc = now;
    }

    public void MarkSigned(string signedXml, DateTimeOffset now)
    {
        if (Status != ElectronicDocumentStatus.Generated) throw new FinancialDomainException("electronic_document.sign.status", "Only generated documents can be signed.");
        SignedXmlContentHash = Sha256(signedXml);
        Status = ElectronicDocumentStatus.Signed;
        SignedAtUtc = now;
        UpdatedAtUtc = now;
    }

    public void MarkSent(string responseCode, string responseMessage, DateTimeOffset now)
    {
        if (Status != ElectronicDocumentStatus.Signed) throw new FinancialDomainException("electronic_document.send.status", "Only signed documents can be sent.");
        SriResponseCode = responseCode;
        SriResponseMessage = responseMessage;
        Status = ElectronicDocumentStatus.Sent;
        SentAtUtc = now;
        UpdatedAtUtc = now;
    }

    public void MarkAuthorized(string authorizationNumber, DateTimeOffset authorizationDate, string responseCode, string responseMessage, DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(responseCode)) throw new FinancialDomainException("electronic_document.authorization.response.required", "SRI response is required.");
        SriAuthorizationNumber = Required(authorizationNumber, nameof(SriAuthorizationNumber));
        SriAuthorizationDate = authorizationDate;
        SriResponseCode = responseCode.Trim();
        SriResponseMessage = responseMessage.Trim();
        Status = ElectronicDocumentStatus.Authorized;
        AuthorizedAtUtc = now;
        UpdatedAtUtc = now;
    }

    public void MarkRejected(string responseCode, string responseMessage, DateTimeOffset now)
    {
        SriResponseCode = Required(responseCode, nameof(SriResponseCode));
        SriResponseMessage = Required(responseMessage, nameof(SriResponseMessage));
        Status = ElectronicDocumentStatus.Rejected;
        RejectedAtUtc = now;
        UpdatedAtUtc = now;
    }

    public void EnsureCanSend()
    {
        if (Status != ElectronicDocumentStatus.Signed) throw new FinancialDomainException("electronic_document.send.status", "Only signed documents can be sent.");
    }

    public void EnsureCanSign()
    {
        if (Status != ElectronicDocumentStatus.Generated) throw new FinancialDomainException("electronic_document.sign.status", "Only generated documents can be signed.");
    }

    private void EnsureEditable()
    {
        if (Status == ElectronicDocumentStatus.Authorized) throw new FinancialDomainException("electronic_document.authorized.immutable", "Authorized documents cannot be modified.");
        if (Status != ElectronicDocumentStatus.Draft) throw new FinancialDomainException("electronic_document.not_draft", "Only draft documents can be modified.");
    }

    private int NextLineNumber() => _lines.Count == 0 ? 1 : _lines.Max(x => x.LineNumber) + 1;

    private void RecalculateTotals(DateTimeOffset now)
    {
        SubtotalWithoutTaxes = FinancialPrecision.Normalize(_lines.Sum(x => x.Subtotal));
        TotalDiscount = FinancialPrecision.Normalize(_lines.Sum(x => x.Discount));
        TotalTaxes = FinancialPrecision.Normalize(_taxes.Sum(x => x.TaxAmount));
        TotalAmount = FinancialPrecision.Normalize(SubtotalWithoutTaxes + TotalTaxes);
        if (TotalAmount < 0) throw new FinancialDomainException("electronic_document.total.invalid", "TotalAmount must be greater than or equal to zero.");
        UpdatedAtUtc = now;
    }

    private static string Required(string value, string name) => string.IsNullOrWhiteSpace(value) ? throw new FinancialDomainException($"{name.ToLowerInvariant()}.required", $"{name} is required.") : value.Trim();
    internal static string ValidateFixedDigits(string value, int length, string code, string message)
    {
        value = Required(value, nameof(value));
        return value.Length == length && value.All(char.IsDigit) ? value : throw new FinancialDomainException(code, message);
    }
    private static string Sha256(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
}

public sealed class ElectronicDocumentLine
{
    private ElectronicDocumentLine() { }
    private ElectronicDocumentLine(Guid id, Guid electronicDocumentId, string tenantId, int lineNumber, string productCode, string description, decimal quantity, decimal unitPrice, decimal discount, DateTimeOffset now)
    {
        Id = id;
        ElectronicDocumentId = electronicDocumentId;
        TenantId = tenantId;
        LineNumber = lineNumber;
        ProductCode = Required(productCode, nameof(ProductCode));
        Description = Required(description, nameof(Description));
        Quantity = quantity > 0 ? FinancialPrecision.Normalize(quantity) : throw new FinancialDomainException("electronic_document.line.quantity.invalid", "Quantity must be greater than zero.");
        UnitPrice = unitPrice >= 0 ? FinancialPrecision.Normalize(unitPrice) : throw new FinancialDomainException("electronic_document.line.unit_price.invalid", "Unit price cannot be negative.");
        Discount = discount >= 0 ? FinancialPrecision.Normalize(discount) : throw new FinancialDomainException("electronic_document.line.discount.invalid", "Discount cannot be negative.");
        Subtotal = FinancialPrecision.Normalize((Quantity * UnitPrice) - Discount);
        Total = Subtotal;
        CreatedAtUtc = now;
        UpdatedAtUtc = now;
    }

    public Guid Id { get; private set; }
    public string TenantId { get; private set; } = FinancialTenant.Default;
    public Guid ElectronicDocumentId { get; private set; }
    public int LineNumber { get; private set; }
    public string ProductCode { get; private set; } = "";
    public string Description { get; private set; } = "";
    public decimal Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal Discount { get; private set; }
    public decimal Subtotal { get; private set; }
    public decimal Total { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public static ElectronicDocumentLine Create(Guid id, Guid electronicDocumentId, string tenantId, int lineNumber, string productCode, string description, decimal quantity, decimal unitPrice, decimal discount, DateTimeOffset now) =>
        new(id, electronicDocumentId, tenantId, lineNumber, productCode, description, quantity, unitPrice, discount, now);
    private static string Required(string value, string name) => string.IsNullOrWhiteSpace(value) ? throw new FinancialDomainException($"{name.ToLowerInvariant()}.required", $"{name} is required.") : value.Trim();
}

public sealed class ElectronicDocumentTax
{
    private ElectronicDocumentTax() { }
    private ElectronicDocumentTax(Guid id, string tenantId, Guid electronicDocumentId, Guid? lineId, string taxCode, string taxPercentageCode, decimal taxRate, decimal taxBase, decimal taxAmount, DateTimeOffset now)
    {
        Id = id;
        TenantId = tenantId;
        ElectronicDocumentId = electronicDocumentId;
        LineId = lineId;
        TaxCode = Required(taxCode, nameof(TaxCode));
        TaxPercentageCode = Required(taxPercentageCode, nameof(TaxPercentageCode));
        TaxRate = taxRate < 0 ? throw new FinancialDomainException("electronic_document.tax.rate.invalid", "Tax rate cannot be negative.") : FinancialPrecision.Normalize(taxRate);
        TaxBase = taxBase < 0 ? throw new FinancialDomainException("electronic_document.tax.base.invalid", "Tax base cannot be negative.") : FinancialPrecision.Normalize(taxBase);
        TaxAmount = taxAmount < 0 ? throw new FinancialDomainException("electronic_document.tax.amount.invalid", "Tax amount cannot be negative.") : FinancialPrecision.Normalize(taxAmount);
        CreatedAtUtc = now;
    }

    public Guid Id { get; private set; }
    public string TenantId { get; private set; } = FinancialTenant.Default;
    public Guid ElectronicDocumentId { get; private set; }
    public Guid? LineId { get; private set; }
    public string TaxCode { get; private set; } = "";
    public string TaxPercentageCode { get; private set; } = "";
    public decimal TaxRate { get; private set; }
    public decimal TaxBase { get; private set; }
    public decimal TaxAmount { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }

    public static ElectronicDocumentTax Create(Guid id, string tenantId, Guid electronicDocumentId, Guid? lineId, string taxCode, string taxPercentageCode, decimal taxRate, decimal taxBase, decimal taxAmount, DateTimeOffset now) =>
        new(id, tenantId, electronicDocumentId, lineId, taxCode, taxPercentageCode, taxRate, taxBase, taxAmount, now);
    private static string Required(string value, string name) => string.IsNullOrWhiteSpace(value) ? throw new FinancialDomainException($"{name.ToLowerInvariant()}.required", $"{name} is required.") : value.Trim();
}

public sealed class SriDocumentSequence
{
    private SriDocumentSequence() { }
    public SriDocumentSequence(string tenantId, ElectronicDocumentType documentType, SriEnvironment environment, string establishmentCode, string emissionPointCode, long currentValue)
    {
        Id = Guid.NewGuid();
        TenantId = string.IsNullOrWhiteSpace(tenantId) ? throw new FinancialDomainException("sri_sequence.tenant.required", "TenantId is required.") : tenantId.Trim();
        DocumentType = documentType;
        Environment = environment;
        EstablishmentCode = ElectronicDocument.ValidateFixedDigits(establishmentCode, 3, "sri_sequence.establishment.invalid", "Establishment code must have 3 digits.");
        EmissionPointCode = ElectronicDocument.ValidateFixedDigits(emissionPointCode, 3, "sri_sequence.emission_point.invalid", "Emission point code must have 3 digits.");
        CurrentValue = currentValue < 0 ? throw new FinancialDomainException("sri_sequence.current.invalid", "Current value cannot be negative.") : currentValue;
        IsActive = true;
        CreatedAtUtc = DateTimeOffset.UtcNow;
        UpdatedAtUtc = CreatedAtUtc;
    }
    public Guid Id { get; private set; }
    public string TenantId { get; private set; } = FinancialTenant.Default;
    public ElectronicDocumentType DocumentType { get; private set; }
    public string EstablishmentCode { get; private set; } = "";
    public string EmissionPointCode { get; private set; } = "";
    public long CurrentValue { get; private set; }
    public SriEnvironment Environment { get; private set; }
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }
}

public sealed record SriAccessKey
{
    public SriAccessKey(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length != 49 || !value.All(char.IsDigit))
            throw new FinancialDomainException("sri_access_key.invalid", "SRI access key must have 49 digits.");
        Value = value;
    }
    public string Value { get; }
}

public sealed record SriAccessKeyInput(DateOnly IssueDate, string DocumentCode, string Ruc, SriEnvironment Environment, string EstablishmentCode, string EmissionPointCode, string Sequential, string NumericCode, SriEmissionType EmissionType);

public static class SriAccessKeyGenerator
{
    public static SriAccessKey Generate(SriAccessKeyInput input)
    {
        if (!IsValidRuc(input.Ruc)) throw new FinancialDomainException("sri_access_key.ruc.invalid", "Issuer RUC must have 13 digits.");
        var establishment = ElectronicDocument.ValidateFixedDigits(input.EstablishmentCode, 3, "sri_access_key.establishment.invalid", "Establishment code must have 3 digits.");
        var emissionPoint = ElectronicDocument.ValidateFixedDigits(input.EmissionPointCode, 3, "sri_access_key.emission_point.invalid", "Emission point code must have 3 digits.");
        var sequential = ElectronicDocument.ValidateFixedDigits(input.Sequential, 9, "sri_access_key.sequential.invalid", "Sequential must have 9 digits.");
        var documentCode = ElectronicDocument.ValidateFixedDigits(input.DocumentCode, 2, "sri_access_key.document_code.invalid", "Document code must have 2 digits.");
        var numericCode = ElectronicDocument.ValidateFixedDigits(input.NumericCode, 8, "sri_access_key.numeric_code.invalid", "Numeric code must have 8 digits.");
        var body = $"{input.IssueDate:ddMMyyyy}{documentCode}{input.Ruc}{(int)input.Environment}{establishment}{emissionPoint}{sequential}{numericCode}{(int)input.EmissionType}";
        return new SriAccessKey(body + CalculateModulo11CheckDigit(body));
    }

    public static int CalculateModulo11CheckDigit(string digits)
    {
        if (string.IsNullOrWhiteSpace(digits) || !digits.All(char.IsDigit)) throw new FinancialDomainException("sri_access_key.body.invalid", "Modulo 11 input must contain only digits.");
        var factor = 2;
        var sum = 0;
        for (var i = digits.Length - 1; i >= 0; i--)
        {
            sum += (digits[i] - '0') * factor;
            factor = factor == 7 ? 2 : factor + 1;
        }
        var digit = 11 - (sum % 11);
        return digit == 11 ? 0 : digit == 10 ? 1 : digit;
    }

    public static bool IsValidRuc(string ruc) => !string.IsNullOrWhiteSpace(ruc) && ruc.Length == 13 && ruc.All(char.IsDigit);
}

public sealed record SriCatalogItem(string Catalog, string Code, string Name, bool IsActive = true)
{
    public static IReadOnlyCollection<SriCatalogItem> Minimum =>
    [
        new("documentType", "01", "Factura"),
        new("documentType", "04", "Nota de crédito"),
        new("documentType", "05", "Nota de débito"),
        new("documentType", "06", "Guía de remisión"),
        new("documentType", "07", "Comprobante de retención"),
        new("identificationType", "04", "RUC"),
        new("identificationType", "05", "Cédula"),
        new("identificationType", "06", "Pasaporte"),
        new("identificationType", "07", "Consumidor final"),
        new("identificationType", "08", "Identificación del exterior"),
        new("tax", "2", "IVA"),
        new("ivaRate", "0", "0%"),
        new("ivaRate", "2", "12%"),
        new("ivaRate", "3", "14%"),
        new("ivaRate", "4", "15%"),
        new("ivaRate", "6", "No objeto de impuesto"),
        new("ivaRate", "7", "Exento de IVA")
    ];
}
