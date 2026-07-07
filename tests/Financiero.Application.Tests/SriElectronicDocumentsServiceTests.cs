using Financiero.Application;
using Financiero.Domain;
using Xunit;

namespace Financiero.Application.Tests;

public sealed class SriElectronicDocumentsServiceTests
{
    private static PortalCallContext Context => new("default", "corr-sri");

    [Fact]
    public async Task Runs_invoice_foundation_flow_with_audit_and_outbox()
    {
        var repo = new InMemoryElectronicDocumentRepository();
        var audit = new RecordingAudit();
        var outbox = new RecordingOutbox();
        var service = NewService(repo, audit, outbox);

        var draft = await service.CreateInvoiceDraftAsync(new(new DateOnly(2026, 1, 1), "04", "0999999999001", "Cliente"), Context, default);
        var withLine = await service.AddInvoiceLineAsync(draft.Id, new("SKU-1", "Servicio", 2, 5, 0), Context, default);
        var generated = await service.GenerateInvoiceXmlAsync(withLine.Id, Context, default);
        var signed = await service.SignElectronicDocumentAsync(generated.Id, Context, default);
        var sent = await service.SendElectronicDocumentAsync(signed.Id, Context, default);
        var authorized = await service.AuthorizeElectronicDocumentAsync(sent.Id, Context, default);
        var byAccessKey = await service.GetByAccessKeyAsync(authorized.AccessKey!, Context, default);

        Assert.Equal("000000001", generated.Sequential);
        Assert.Equal(49, generated.AccessKey!.Length);
        Assert.Equal("Signed", signed.Status);
        Assert.Equal("Sent", sent.Status);
        Assert.Equal("Authorized", authorized.Status);
        Assert.Equal(authorized.Id, byAccessKey.Id);
        Assert.Contains("ElectronicDocumentXmlGenerated", audit.Actions);
        Assert.Contains("ElectronicDocumentAuthorized", outbox.EventTypes);
        Assert.All(outbox.CorrelationIds, x => Assert.Equal(Context.CorrelationId, x));
    }

    [Fact]
    public async Task Rejected_sri_mock_marks_document_rejected()
    {
        var service = NewService(new InMemoryElectronicDocumentRepository(), new RecordingAudit(), new RecordingOutbox(), new StaticFinancialConfigurationReader(new Dictionary<string, string> { ["financial.sri.mock.authorizationStatus"] = "Rejected" }));
        var draft = await service.CreateInvoiceDraftAsync(new(new DateOnly(2026, 1, 1), "04", "0999999999001", "Cliente"), Context, default);
        await service.AddInvoiceLineAsync(draft.Id, new("SKU", "Item", 1, 1, 0), Context, default);
        await service.GenerateInvoiceXmlAsync(draft.Id, Context, default);
        await service.SignElectronicDocumentAsync(draft.Id, Context, default);
        await service.SendElectronicDocumentAsync(draft.Id, Context, default);

        var rejected = await service.AuthorizeElectronicDocumentAsync(draft.Id, Context, default);

        Assert.Equal("Rejected", rejected.Status);
    }

    [Fact]
    public void Xml_generator_emits_minimum_sri_structure()
    {
        var document = ElectronicDocument.CreateInvoice("default", SriEnvironment.Test, SriEmissionType.Normal, "001", "001", new DateOnly(2026, 1, 1), "04", "0999999999001", "Cliente", "USD", DateTimeOffset.UtcNow);
        document.AddLine("SKU", "Item", 1, 10, 0, DateTimeOffset.UtcNow);
        var key = SriAccessKeyGenerator.Generate(new(new DateOnly(2026, 1, 1), "01", "0999999999001", SriEnvironment.Test, "001", "001", "000000001", "12345678", SriEmissionType.Normal));
        document.Generate("000000001", key, "<factura />", DateTimeOffset.UtcNow);

        var xml = new ElectronicInvoiceXmlGenerator().GenerateInvoiceXml(document, new("0999999999001", "EMISOR", "EMISOR", "DIR", true));

        Assert.Contains("infoTributaria", xml);
        Assert.Contains("infoFactura", xml);
        Assert.Contains("detalles", xml);
        Assert.Contains("claveAcceso", xml);
        Assert.Contains("0999999999001", xml);
        Assert.Contains("000000001", xml);
    }

    private static ElectronicDocumentsService NewService(InMemoryElectronicDocumentRepository repo, RecordingAudit audit, RecordingOutbox outbox, IFinancialConfigurationReader? configuration = null)
    {
        var config = configuration ?? new StaticFinancialConfigurationReader(new Dictionary<string, string>
        {
            ["financial.sri.issuer.ruc"] = "0999999999001",
            ["financial.sri.issuer.legalName"] = "EMISOR TEST",
            ["financial.sri.issuer.tradeName"] = "EMISOR TEST",
            ["financial.sri.issuer.address"] = "DIR TEST",
            ["financial.sri.defaultEstablishmentCode"] = "001",
            ["financial.sri.defaultEmissionPointCode"] = "001",
            ["financial.sri.accessKey.numericCode"] = "12345678"
        });
        return new(repo, config, new ElectronicInvoiceXmlGenerator(), new DevelopmentElectronicSignatureService(), new DevelopmentSriReceptionClient(config), new DevelopmentSriAuthorizationClient(config), audit, outbox);
    }
}

internal sealed class InMemoryElectronicDocumentRepository : IElectronicDocumentRepository
{
    private readonly List<ElectronicDocument> _documents = [];
    private readonly Dictionary<string, long> _sequences = [];
    public Task AddAsync(ElectronicDocument document, CancellationToken ct) { _documents.Add(document); return Task.CompletedTask; }
    public Task AddLineAsync(ElectronicDocumentLine line, CancellationToken ct) => Task.CompletedTask;
    public Task<ElectronicDocument?> GetByIdAsync(Guid id, string tenantId, CancellationToken ct) => Task.FromResult(_documents.FirstOrDefault(x => x.Id == id && x.TenantId == tenantId));
    public Task<ElectronicDocument?> GetByAccessKeyAsync(string accessKey, string tenantId, CancellationToken ct) => Task.FromResult(_documents.FirstOrDefault(x => x.AccessKey == accessKey && x.TenantId == tenantId));
    public Task<(IReadOnlyCollection<ElectronicDocument> Items, long Total)> SearchAsync(string tenantId, ElectronicDocumentStatus? status, string? accessKey, int page, int pageSize, CancellationToken ct)
    {
        var query = _documents.Where(x => x.TenantId == tenantId);
        if (status.HasValue) query = query.Where(x => x.Status == status);
        if (!string.IsNullOrWhiteSpace(accessKey)) query = query.Where(x => x.AccessKey == accessKey);
        var items = query.ToArray();
        return Task.FromResult(((IReadOnlyCollection<ElectronicDocument>)items.Skip((page - 1) * pageSize).Take(pageSize).ToArray(), (long)items.Length));
    }
    public Task<string> GetNextSequentialAsync(string tenantId, ElectronicDocumentType documentType, SriEnvironment environment, string establishmentCode, string emissionPointCode, CancellationToken ct)
    {
        var key = $"{tenantId}:{documentType}:{environment}:{establishmentCode}:{emissionPointCode}";
        _sequences[key] = _sequences.GetValueOrDefault(key) + 1;
        return Task.FromResult(_sequences[key].ToString("000000000"));
    }
    public Task<bool> SequenceDocumentExistsAsync(string tenantId, ElectronicDocumentType documentType, SriEnvironment environment, string establishmentCode, string emissionPointCode, string sequential, Guid? excludingId, CancellationToken ct) =>
        Task.FromResult(_documents.Any(x => x.TenantId == tenantId && x.DocumentType == documentType && x.Environment == environment && x.EstablishmentCode == establishmentCode && x.EmissionPointCode == emissionPointCode && x.Sequential == sequential && (!excludingId.HasValue || x.Id != excludingId)));
    public Task SaveChangesAsync(CancellationToken ct) => Task.CompletedTask;
}

internal sealed class StaticFinancialConfigurationReader(IReadOnlyDictionary<string, string>? values = null) : IFinancialConfigurationReader
{
    private readonly IReadOnlyDictionary<string, string> _values = values ?? new Dictionary<string, string>();
    public Task<bool> GetBoolAsync(string key, bool defaultValue, PortalCallContext context, CancellationToken ct) => Task.FromResult(_values.TryGetValue(key, out var value) ? bool.Parse(value) : defaultValue);
    public Task<int> GetIntAsync(string key, int defaultValue, PortalCallContext context, CancellationToken ct) => Task.FromResult(_values.TryGetValue(key, out var value) ? int.Parse(value) : defaultValue);
    public Task<string> GetStringAsync(string key, string defaultValue, PortalCallContext context, CancellationToken ct) => Task.FromResult(_values.TryGetValue(key, out var value) ? value : defaultValue);
}
