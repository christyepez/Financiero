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
    private readonly List<ElectronicDocumentReference> _references = [];
    private readonly List<ElectronicDocumentDebitNoteReason> _debitNoteReasons = [];
    private readonly List<ElectronicDocumentWithholdingTax> _withholdingTaxes = [];

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
    public string? UnsignedXmlStorageId { get; private set; }
    public string? SignedXmlStorageId { get; private set; }
    public string? AuthorizationXmlStorageId { get; private set; }
    public string? RidePdfStorageId { get; private set; }
    public string? RidePdfHash { get; private set; }
    public string? StorageProvider { get; private set; }
    public string? SignatureProvider { get; private set; }
    public string? SignatureDigest { get; private set; }
    public string? SriAuthorizationNumber { get; private set; }
    public DateTimeOffset? SriAuthorizationDate { get; private set; }
    public string? SriResponseCode { get; private set; }
    public string? SriResponseMessage { get; private set; }
    public string? LastSriStatus { get; private set; }
    public string? LastSriMessage { get; private set; }
    public string? LastErrorCode { get; private set; }
    public string? LastErrorMessage { get; private set; }
    public int SriReceptionAttempts { get; private set; }
    public int SriAuthorizationAttempts { get; private set; }
    public string? LastIntegrationCorrelationId { get; private set; }
    public Guid? RelatedJournalEntryId { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }
    public DateTimeOffset? GeneratedAtUtc { get; private set; }
    public DateTimeOffset? SignedAtUtc { get; private set; }
    public DateTimeOffset? SentAtUtc { get; private set; }
    public DateTimeOffset? AuthorizedAtUtc { get; private set; }
    public DateTimeOffset? RejectedAtUtc { get; private set; }
    public DateTimeOffset? RideGeneratedAtUtc { get; private set; }
    public IReadOnlyCollection<ElectronicDocumentLine> Lines => _lines.OrderBy(x => x.LineNumber).ToArray();
    public IReadOnlyCollection<ElectronicDocumentTax> Taxes => _taxes.ToArray();
    public IReadOnlyCollection<ElectronicDocumentReference> References => _references.ToArray();
    public IReadOnlyCollection<ElectronicDocumentDebitNoteReason> DebitNoteReasons => _debitNoteReasons.ToArray();
    public IReadOnlyCollection<ElectronicDocumentWithholdingTax> WithholdingTaxes => _withholdingTaxes.ToArray();

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

    public static ElectronicDocument CreateCreditNote(
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
        string relatedDocumentTypeCode,
        string relatedDocumentNumber,
        DateOnly relatedDocumentIssueDate,
        string reason,
        DateTimeOffset now)
    {
        var document = new ElectronicDocument(Guid.NewGuid(), tenantId, ElectronicDocumentType.CreditNote, environment, emissionType, establishmentCode, emissionPointCode, issueDate,
            customerIdentificationType, customerIdentification, customerName, currency, now);
        document.AddRelatedDocument(relatedDocumentTypeCode, relatedDocumentNumber, relatedDocumentIssueDate, reason, now);
        return document;
    }

    public static ElectronicDocument CreateDebitNote(
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
        string relatedDocumentTypeCode,
        string relatedDocumentNumber,
        DateOnly relatedDocumentIssueDate,
        DateTimeOffset now)
    {
        var document = new ElectronicDocument(Guid.NewGuid(), tenantId, ElectronicDocumentType.DebitNote, environment, emissionType, establishmentCode, emissionPointCode, issueDate,
            customerIdentificationType, customerIdentification, customerName, currency, now);
        document.AddRelatedDocument(relatedDocumentTypeCode, relatedDocumentNumber, relatedDocumentIssueDate, "Documento relacionado", now);
        return document;
    }

    public static ElectronicDocument CreateWithholding(
        string tenantId,
        SriEnvironment environment,
        SriEmissionType emissionType,
        string establishmentCode,
        string emissionPointCode,
        DateOnly issueDate,
        string subjectIdentificationType,
        string subjectIdentification,
        string subjectName,
        string currency,
        string fiscalPeriod,
        string supportDocumentTypeCode,
        string supportDocumentNumber,
        DateOnly supportDocumentIssueDate,
        DateTimeOffset now)
    {
        var document = new ElectronicDocument(Guid.NewGuid(), tenantId, ElectronicDocumentType.Withholding, environment, emissionType, establishmentCode, emissionPointCode, issueDate,
            subjectIdentificationType, subjectIdentification, subjectName, currency, now);
        document.AddRelatedDocument(supportDocumentTypeCode, supportDocumentNumber, supportDocumentIssueDate, fiscalPeriod, now);
        return document;
    }

    public ElectronicDocumentReference AddRelatedDocument(string documentTypeCode, string number, DateOnly issueDate, string reasonOrPeriod, DateTimeOffset now)
    {
        EnsureEditable();
        if (DocumentType == ElectronicDocumentType.Invoice) throw new FinancialDomainException("electronic_document.reference.unsupported", "Invoice does not require related fiscal document in this foundation.");
        if (_references.Count > 0) throw new FinancialDomainException("electronic_document.reference.duplicate", "Only one related fiscal document is supported in this foundation.");
        var reference = ElectronicDocumentReference.Create(Guid.NewGuid(), TenantId, Id, documentTypeCode, number, issueDate, reasonOrPeriod, now);
        _references.Add(reference);
        UpdatedAtUtc = now;
        return reference;
    }

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

    public ElectronicDocumentDebitNoteReason AddDebitNoteReason(string reason, decimal amount, DateTimeOffset now)
    {
        EnsureEditable();
        if (DocumentType != ElectronicDocumentType.DebitNote) throw new FinancialDomainException("electronic_document.debit_note_reason.unsupported", "Debit note reason can only be added to debit notes.");
        var item = ElectronicDocumentDebitNoteReason.Create(Guid.NewGuid(), TenantId, Id, reason, amount, now);
        _debitNoteReasons.Add(item);
        SubtotalWithoutTaxes = FinancialPrecision.Normalize(_debitNoteReasons.Sum(x => x.Amount));
        TotalAmount = FinancialPrecision.Normalize(SubtotalWithoutTaxes + TotalTaxes);
        UpdatedAtUtc = now;
        return item;
    }

    public ElectronicDocumentWithholdingTax AddWithholdingTax(string taxCode, string withholdingCode, decimal taxBase, decimal withholdingPercentage, decimal withheldAmount, string supportDocumentNumber, DateOnly supportDocumentIssueDate, string fiscalPeriod, DateTimeOffset now)
    {
        EnsureEditable();
        if (DocumentType != ElectronicDocumentType.Withholding) throw new FinancialDomainException("electronic_document.withholding_tax.unsupported", "Withholding tax can only be added to withholding documents.");
        var item = ElectronicDocumentWithholdingTax.Create(Guid.NewGuid(), TenantId, Id, taxCode, withholdingCode, taxBase, withholdingPercentage, withheldAmount, supportDocumentNumber, supportDocumentIssueDate, fiscalPeriod, now);
        _withholdingTaxes.Add(item);
        SubtotalWithoutTaxes = FinancialPrecision.Normalize(_withholdingTaxes.Sum(x => x.TaxBase));
        TotalTaxes = FinancialPrecision.Normalize(_withholdingTaxes.Sum(x => x.WithheldAmount));
        TotalAmount = TotalTaxes;
        UpdatedAtUtc = now;
        return item;
    }

    public void Generate(string sequential, SriAccessKey accessKey, string unsignedXml, DateTimeOffset now)
    {
        EnsureEditable();
        ValidateBeforeGenerate();
        Sequential = ValidateFixedDigits(sequential, 9, "electronic_document.sequential.invalid", "Sequential must have 9 digits.");
        AccessKey = accessKey.Value;
        XmlContentHash = Sha256(unsignedXml);
        Status = ElectronicDocumentStatus.Generated;
        GeneratedAtUtc = now;
        UpdatedAtUtc = now;
    }

    private void ValidateBeforeGenerate()
    {
        if (DocumentType == ElectronicDocumentType.Invoice && _lines.Count == 0) throw new FinancialDomainException("electronic_document.invoice.lines.required", "Invoice requires at least one line.");
        if (DocumentType == ElectronicDocumentType.CreditNote)
        {
            if (_references.Count == 0) throw new FinancialDomainException("electronic_document.credit_note.reference.required", "Credit note requires related document.");
            if (string.IsNullOrWhiteSpace(_references[0].ReasonOrPeriod)) throw new FinancialDomainException("electronic_document.credit_note.reason.required", "Credit note reason is required.");
            if (_lines.Count == 0) throw new FinancialDomainException("electronic_document.credit_note.lines.required", "Credit note requires at least one line.");
        }
        if (DocumentType == ElectronicDocumentType.DebitNote)
        {
            if (_references.Count == 0) throw new FinancialDomainException("electronic_document.debit_note.reference.required", "Debit note requires related document.");
            if (_debitNoteReasons.Count == 0) throw new FinancialDomainException("electronic_document.debit_note.reason.required", "Debit note requires at least one reason.");
            if (_debitNoteReasons.Any(x => x.Amount <= 0)) throw new FinancialDomainException("electronic_document.debit_note.amount.invalid", "Debit note adjustment amount must be greater than zero.");
        }
        if (DocumentType == ElectronicDocumentType.Withholding)
        {
            if (_references.Count == 0) throw new FinancialDomainException("electronic_document.withholding.support.required", "Withholding requires support document and fiscal period.");
            if (_withholdingTaxes.Count == 0) throw new FinancialDomainException("electronic_document.withholding.tax.required", "Withholding requires at least one withheld tax.");
        }
    }

    public void RegisterUnsignedXmlStorage(string storageId, DateTimeOffset now)
    {
        UnsignedXmlStorageId = Required(storageId, nameof(UnsignedXmlStorageId));
        UpdatedAtUtc = now;
    }

    public void MarkSigned(string signedXml, string provider, string signatureDigest, DateTimeOffset now)
    {
        if (Status != ElectronicDocumentStatus.Generated) throw new FinancialDomainException("electronic_document.sign.status", "Only generated documents can be signed.");
        SignedXmlContentHash = Sha256(signedXml);
        SignatureProvider = Required(provider, nameof(SignatureProvider));
        SignatureDigest = Required(signatureDigest, nameof(SignatureDigest));
        Status = ElectronicDocumentStatus.Signed;
        SignedAtUtc = now;
        UpdatedAtUtc = now;
    }

    public void RegisterSignedXmlStorage(string storageId, DateTimeOffset now)
    {
        SignedXmlStorageId = Required(storageId, nameof(SignedXmlStorageId));
        UpdatedAtUtc = now;
    }

    public void MarkSent(string responseCode, string responseMessage, DateTimeOffset now)
    {
        if (Status != ElectronicDocumentStatus.Signed) throw new FinancialDomainException("electronic_document.send.status", "Only signed documents can be sent.");
        SriResponseCode = responseCode;
        SriResponseMessage = responseMessage;
        LastSriStatus = "Sent";
        LastSriMessage = responseMessage;
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
        LastSriStatus = "Authorized";
        LastSriMessage = responseMessage.Trim();
        Status = ElectronicDocumentStatus.Authorized;
        AuthorizedAtUtc = now;
        UpdatedAtUtc = now;
    }

    public void MarkRejected(string responseCode, string responseMessage, DateTimeOffset now)
    {
        SriResponseCode = Required(responseCode, nameof(SriResponseCode));
        SriResponseMessage = Required(responseMessage, nameof(SriResponseMessage));
        LastSriStatus = "Rejected";
        LastSriMessage = SriResponseMessage;
        Status = ElectronicDocumentStatus.Rejected;
        RejectedAtUtc = now;
        UpdatedAtUtc = now;
    }

    public void RegisterAuthorizationXmlStorage(string storageId, DateTimeOffset now)
    {
        AuthorizationXmlStorageId = Required(storageId, nameof(AuthorizationXmlStorageId));
        UpdatedAtUtc = now;
    }

    public void RegisterRidePdfStorage(string storageId, string hash, string provider, string correlationId, DateTimeOffset now)
    {
        RidePdfStorageId = Required(storageId, nameof(RidePdfStorageId));
        RidePdfHash = Required(hash, nameof(RidePdfHash));
        StorageProvider = Required(provider, nameof(StorageProvider));
        LastIntegrationCorrelationId = Required(correlationId, nameof(LastIntegrationCorrelationId));
        RideGeneratedAtUtc = now;
        UpdatedAtUtc = now;
    }

    public void RegisterSriReceptionAttempt(string correlationId, DateTimeOffset now)
    {
        SriReceptionAttempts++;
        LastIntegrationCorrelationId = Required(correlationId, nameof(LastIntegrationCorrelationId));
        UpdatedAtUtc = now;
    }

    public void RegisterSriAuthorizationAttempt(string correlationId, DateTimeOffset now)
    {
        SriAuthorizationAttempts++;
        LastIntegrationCorrelationId = Required(correlationId, nameof(LastIntegrationCorrelationId));
        UpdatedAtUtc = now;
    }

    public void MarkError(string code, string message, DateTimeOffset now)
    {
        LastErrorCode = Required(code, nameof(LastErrorCode));
        LastErrorMessage = Required(message, nameof(LastErrorMessage));
        Status = ElectronicDocumentStatus.Error;
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

public sealed class ElectronicDocumentReference
{
    private ElectronicDocumentReference() { }
    private ElectronicDocumentReference(Guid id, string tenantId, Guid electronicDocumentId, string documentTypeCode, string number, DateOnly issueDate, string reasonOrPeriod, DateTimeOffset now)
    {
        Id = id;
        TenantId = Required(tenantId, nameof(TenantId));
        ElectronicDocumentId = electronicDocumentId;
        DocumentTypeCode = ElectronicDocument.ValidateFixedDigits(documentTypeCode, 2, "electronic_document.reference.document_type.invalid", "Related document type must have 2 digits.");
        Number = Required(number, nameof(Number));
        IssueDate = issueDate;
        ReasonOrPeriod = Required(reasonOrPeriod, nameof(ReasonOrPeriod));
        CreatedAtUtc = now;
    }

    public Guid Id { get; private set; }
    public string TenantId { get; private set; } = FinancialTenant.Default;
    public Guid ElectronicDocumentId { get; private set; }
    public string DocumentTypeCode { get; private set; } = "";
    public string Number { get; private set; } = "";
    public DateOnly IssueDate { get; private set; }
    public string ReasonOrPeriod { get; private set; } = "";
    public DateTimeOffset CreatedAtUtc { get; private set; }

    public static ElectronicDocumentReference Create(Guid id, string tenantId, Guid electronicDocumentId, string documentTypeCode, string number, DateOnly issueDate, string reasonOrPeriod, DateTimeOffset now) =>
        new(id, tenantId, electronicDocumentId, documentTypeCode, number, issueDate, reasonOrPeriod, now);
    private static string Required(string value, string name) => string.IsNullOrWhiteSpace(value) ? throw new FinancialDomainException($"{name.ToLowerInvariant()}.required", $"{name} is required.") : value.Trim();
}

public sealed class ElectronicDocumentDebitNoteReason
{
    private ElectronicDocumentDebitNoteReason() { }
    private ElectronicDocumentDebitNoteReason(Guid id, string tenantId, Guid electronicDocumentId, string reason, decimal amount, DateTimeOffset now)
    {
        Id = id;
        TenantId = Required(tenantId, nameof(TenantId));
        ElectronicDocumentId = electronicDocumentId;
        Reason = Required(reason, nameof(Reason));
        Amount = amount > 0 ? FinancialPrecision.Normalize(amount) : throw new FinancialDomainException("electronic_document.debit_note.amount.invalid", "Debit note amount must be greater than zero.");
        CreatedAtUtc = now;
    }

    public Guid Id { get; private set; }
    public string TenantId { get; private set; } = FinancialTenant.Default;
    public Guid ElectronicDocumentId { get; private set; }
    public string Reason { get; private set; } = "";
    public decimal Amount { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }

    public static ElectronicDocumentDebitNoteReason Create(Guid id, string tenantId, Guid electronicDocumentId, string reason, decimal amount, DateTimeOffset now) =>
        new(id, tenantId, electronicDocumentId, reason, amount, now);
    private static string Required(string value, string name) => string.IsNullOrWhiteSpace(value) ? throw new FinancialDomainException($"{name.ToLowerInvariant()}.required", $"{name} is required.") : value.Trim();
}

public sealed class ElectronicDocumentWithholdingTax
{
    private ElectronicDocumentWithholdingTax() { }
    private ElectronicDocumentWithholdingTax(Guid id, string tenantId, Guid electronicDocumentId, string taxCode, string withholdingCode, decimal taxBase, decimal withholdingPercentage, decimal withheldAmount, string supportDocumentNumber, DateOnly supportDocumentIssueDate, string fiscalPeriod, DateTimeOffset now)
    {
        Id = id;
        TenantId = Required(tenantId, nameof(TenantId));
        ElectronicDocumentId = electronicDocumentId;
        TaxCode = Required(taxCode, nameof(TaxCode));
        WithholdingCode = Required(withholdingCode, nameof(WithholdingCode));
        TaxBase = taxBase >= 0 ? FinancialPrecision.Normalize(taxBase) : throw new FinancialDomainException("electronic_document.withholding.base.invalid", "Withholding tax base cannot be negative.");
        WithholdingPercentage = withholdingPercentage >= 0 ? FinancialPrecision.Normalize(withholdingPercentage) : throw new FinancialDomainException("electronic_document.withholding.percentage.invalid", "Withholding percentage cannot be negative.");
        WithheldAmount = withheldAmount >= 0 ? FinancialPrecision.Normalize(withheldAmount) : throw new FinancialDomainException("electronic_document.withholding.amount.invalid", "Withheld amount cannot be negative.");
        SupportDocumentNumber = Required(supportDocumentNumber, nameof(SupportDocumentNumber));
        SupportDocumentIssueDate = supportDocumentIssueDate;
        FiscalPeriod = Required(fiscalPeriod, nameof(FiscalPeriod));
        CreatedAtUtc = now;
    }

    public Guid Id { get; private set; }
    public string TenantId { get; private set; } = FinancialTenant.Default;
    public Guid ElectronicDocumentId { get; private set; }
    public string TaxCode { get; private set; } = "";
    public string WithholdingCode { get; private set; } = "";
    public decimal TaxBase { get; private set; }
    public decimal WithholdingPercentage { get; private set; }
    public decimal WithheldAmount { get; private set; }
    public string SupportDocumentNumber { get; private set; } = "";
    public DateOnly SupportDocumentIssueDate { get; private set; }
    public string FiscalPeriod { get; private set; } = "";
    public DateTimeOffset CreatedAtUtc { get; private set; }

    public static ElectronicDocumentWithholdingTax Create(Guid id, string tenantId, Guid electronicDocumentId, string taxCode, string withholdingCode, decimal taxBase, decimal withholdingPercentage, decimal withheldAmount, string supportDocumentNumber, DateOnly supportDocumentIssueDate, string fiscalPeriod, DateTimeOffset now) =>
        new(id, tenantId, electronicDocumentId, taxCode, withholdingCode, taxBase, withholdingPercentage, withheldAmount, supportDocumentNumber, supportDocumentIssueDate, fiscalPeriod, now);
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
