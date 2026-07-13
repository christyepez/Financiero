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
        var ride = await service.GenerateRidePdfAsync(authorized.Id, Context, default);
        var byAccessKey = await service.GetByAccessKeyAsync(authorized.AccessKey!, Context, default);
        var rideMetadata = await service.GetRideMetadataAsync(authorized.Id, Context, default);

        Assert.Equal("000000001", generated.Sequential);
        Assert.Equal(49, generated.AccessKey!.Length);
        Assert.Equal("Signed", signed.Status);
        Assert.Equal("Sent", sent.Status);
        Assert.Equal("Authorized", authorized.Status);
        Assert.NotNull(ride.RidePdfStorageId);
        Assert.NotNull(rideMetadata.RidePdfHash);
        Assert.Equal(authorized.Id, byAccessKey.Id);
        Assert.NotNull(authorized.UnsignedXmlStorageId);
        Assert.NotNull(authorized.SignedXmlStorageId);
        Assert.NotNull(authorized.AuthorizationXmlStorageId);
        Assert.Equal("Development", authorized.SignatureProvider);
        Assert.NotNull(authorized.SignatureDigest);
        Assert.Contains("ElectronicDocumentXmlGenerated", audit.Actions);
        Assert.Contains("ElectronicDocumentAuthorized", outbox.EventTypes);
        Assert.Contains("ElectronicDocumentStorageRegistered", outbox.EventTypes);
        Assert.All(outbox.CorrelationIds, x => Assert.Equal(Context.CorrelationId, x));
    }

    [Fact]
    public async Task Runs_credit_note_foundation_flow()
    {
        var repo = new InMemoryElectronicDocumentRepository();
        var audit = new RecordingAudit();
        var outbox = new RecordingOutbox();
        var service = NewService(repo, audit, outbox);

        var draft = await service.CreateCreditNoteDraftAsync(new(new DateOnly(2026, 1, 2), "04", "0999999999001", "Cliente", "01", "001-001-000000001", new DateOnly(2026, 1, 1), "Devolución"), Context, default);
        await service.AddInvoiceLineAsync(draft.Id, new("SKU-NC", "Devolución", 1, 5, 0), Context, default);
        var generated = await service.GenerateCreditNoteXmlAsync(draft.Id, Context, default);

        Assert.Equal("CreditNote", generated.DocumentType);
        Assert.Equal("04", generated.AccessKey![8..10]);
        Assert.NotNull(generated.UnsignedXmlStorageId);
        Assert.Contains("CreditNoteCreated", audit.Actions);
        Assert.Contains("CreditNoteXmlGenerated", outbox.EventTypes);
    }

    [Fact]
    public async Task Runs_debit_note_foundation_flow()
    {
        var repo = new InMemoryElectronicDocumentRepository();
        var audit = new RecordingAudit();
        var outbox = new RecordingOutbox();
        var service = NewService(repo, audit, outbox);

        var draft = await service.CreateDebitNoteDraftAsync(new(new DateOnly(2026, 1, 2), "04", "0999999999001", "Cliente", "01", "001-001-000000001", new DateOnly(2026, 1, 1)), Context, default);
        await service.AddDebitNoteReasonAsync(draft.Id, new("Interés", 2.50m), Context, default);
        var generated = await service.GenerateDebitNoteXmlAsync(draft.Id, Context, default);

        Assert.Equal("DebitNote", generated.DocumentType);
        Assert.Equal("05", generated.AccessKey![8..10]);
        Assert.Equal(2.50m, generated.TotalAmount);
        Assert.Contains("DebitNoteReasonAdded", outbox.EventTypes);
        Assert.Contains("DebitNoteXmlGenerated", outbox.EventTypes);
    }

    [Fact]
    public async Task Runs_withholding_foundation_flow()
    {
        var repo = new InMemoryElectronicDocumentRepository();
        var audit = new RecordingAudit();
        var outbox = new RecordingOutbox();
        var service = NewService(repo, audit, outbox);

        var draft = await service.CreateWithholdingDraftAsync(new(new DateOnly(2026, 1, 2), "04", "0999999999001", "Proveedor", "01/2026", "01", "001-001-000000001", new DateOnly(2026, 1, 1)), Context, default);
        await service.AddWithholdingTaxAsync(draft.Id, new("1", "312", 100, 1.75m, 1.75m, "001-001-000000001", new DateOnly(2026, 1, 1), "01/2026"), Context, default);
        var generated = await service.GenerateWithholdingXmlAsync(draft.Id, Context, default);

        Assert.Equal("Withholding", generated.DocumentType);
        Assert.Equal("07", generated.AccessKey![8..10]);
        Assert.Equal(1.75m, generated.TotalAmount);
        Assert.Contains("WithholdingTaxAdded", outbox.EventTypes);
        Assert.Contains("WithholdingXmlGenerated", outbox.EventTypes);
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
        return new(repo, config, new ElectronicInvoiceXmlGenerator(), new DevelopmentElectronicSignatureService(), new DevelopmentSriReceptionClient(config),
            new DevelopmentSriAuthorizationClient(config), new ElectronicDocumentXmlValidator(), new DevelopmentElectronicDocumentStorageClient(), new DevelopmentRidePdfGenerator(), audit, outbox);
    }

    [Fact]
    public void Xml_validator_rejects_malformed_or_missing_access_key()
    {
        var validator = new ElectronicDocumentXmlValidator();
        Assert.False(validator.ValidateInvoiceXml("<factura>").IsValid);
        Assert.False(validator.ValidateInvoiceXml("<factura><infoTributaria></infoTributaria><infoFactura><totalSinImpuestos>1</totalSinImpuestos><importeTotal>1</importeTotal></infoFactura><detalles><detalle /></detalles></factura>").IsValid);
    }

    [Fact]
    public void Xml_validator_rejects_invalid_sri_structure()
    {
        var xml = "<factura><infoTributaria><ambiente>9</ambiente><tipoEmision>1</tipoEmision><ruc>123</ruc><claveAcceso>1234567890123456789012345678901234567890123456789</claveAcceso><codDoc>99</codDoc><estab>1</estab><ptoEmi>001</ptoEmi><secuencial>1</secuencial></infoTributaria><infoFactura><totalSinImpuestos>ABC</totalSinImpuestos><importeTotal>1</importeTotal></infoFactura><detalles><detalle /></detalles></factura>";
        var result = new ElectronicDocumentXmlValidator().ValidateInvoiceXml(xml);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.Contains("ambiente"));
        Assert.Contains(result.Errors, x => x.Contains("ruc"));
        Assert.Contains(result.Errors, x => x.Contains("codDoc"));
        Assert.Contains(result.Errors, x => x.Contains("secuencial"));
    }

    [Fact]
    public async Task Signature_placeholder_fails_with_clear_error()
    {
        var service = new DevelopmentElectronicSignatureService();
        await Assert.ThrowsAsync<FinancialApplicationException>(() => service.SignAsync("<factura />", new("default", SignatureProviderType.LocalCertificatePlaceholder, "Development", "", "", "", "", true), default));
    }

    [Fact]
    public async Task Development_signature_is_rejected_in_production()
    {
        var service = new DevelopmentElectronicSignatureService();
        await Assert.ThrowsAsync<FinancialApplicationException>(() => service.SignAsync("<factura />", new("default", SignatureProviderType.Development, "Production", "", "", "", "", true), default));
    }

    [Fact]
    public async Task Xades_signature_fails_without_secure_certificate()
    {
        var service = new XadesElectronicSignatureService(new KeyVaultCertificateProviderPlaceholder());
        var ex = await Assert.ThrowsAsync<FinancialApplicationException>(() => service.SignAsync("<factura />", new("default", SignatureProviderType.Xades, "Development", "", "", "", "", true, "KeyVault"), default));
        Assert.Equal("sri.certificate.keyvault.missing_configuration", ex.Code);
    }

    [Fact]
    public async Task Local_certificate_placeholder_is_rejected_in_production()
    {
        var result = await new LocalCertificateProviderPlaceholder().LoadAsync(new("default", SignatureProviderType.Xades, "Production", "", "", "", "", true, "LocalPlaceholder"), default);
        Assert.False(result.IsLoaded);
        Assert.Equal("sri.certificate.local.production_rejected", result.ErrorCode);
    }

    [Fact]
    public async Task Sri_soap_test_requires_configured_url_and_blocks_production()
    {
        var client = new SriSoapReceptionClient();
        var test = new SriClientContext("default", SriEnvironment.Test, "Test", "", "", 30, 3, 5);
        await Assert.ThrowsAsync<FinancialApplicationException>(() => client.SendAsync(new("123", "<xml />", test), default));
        var production = test with { Mode = "Production", AllowProduction = false, ReceptionUrl = "https://example.test/reception" };
        var ex = await Assert.ThrowsAsync<FinancialApplicationException>(() => client.SendAsync(new("123", "<xml />", production), default));
        Assert.Equal("sri.soap.production.disabled", ex.Code);
    }

    [Fact]
    public async Task Portal_content_file_storage_requires_base_url()
    {
        var client = new PortalContentFileStorageClient(new("PortalContentFile", "", "financial-electronic-documents", 30, true, false));
        var doc = ElectronicDocument.CreateInvoice("default", SriEnvironment.Test, SriEmissionType.Normal, "001", "001", new DateOnly(2026, 1, 1), "04", "0999999999001", "Cliente", "USD", DateTimeOffset.UtcNow);
        var ex = await Assert.ThrowsAsync<FinancialApplicationException>(() => client.SaveUnsignedXmlAsync(doc, "<factura />", Context, default));
        Assert.Equal("sri.storage.portal_base_url.required", ex.Code);
    }

    [Fact]
    public async Task Development_secret_store_is_blocked_in_production()
    {
        var result = await new DevelopmentSecretStoreClient("Production", allowDevelopmentSecrets: true)
            .GetSecretAsync(new("SRI_CERTIFICATE", SecretStoreProviderType.Development), Context, default);
        Assert.False(result.Success);
        Assert.Equal("secret_store.development.production_blocked", result.ErrorCode);
    }

    [Fact]
    public async Task Disabled_secret_store_fails_without_exposing_secret()
    {
        var result = await new DisabledSecretStoreClient().GetSecretAsync(new("VERY_SECRET_CERTIFICATE_NAME", SecretStoreProviderType.Disabled), Context, default);
        Assert.False(result.Success);
        Assert.Equal("secret_store.disabled", result.ErrorCode);
        Assert.DoesNotContain("VERY_SECRET_CERTIFICATE_NAME", result.ToString());
    }

    [Fact]
    public async Task Azure_key_vault_placeholder_requires_configuration()
    {
        var result = await new AzureKeyVaultSecretStoreClientPlaceholder(new("", false, false, false)).GetSecretAsync(new("SRI_CERT", SecretStoreProviderType.AzureKeyVault), Context, default);
        Assert.False(result.Success);
        Assert.Equal("secret_store.azure_keyvault.missing_configuration", result.ErrorCode);
    }

    [Fact]
    public void Azure_key_vault_readiness_masks_secret_name_and_validates_identity_options()
    {
        var readiness = AzureKeyVaultSecretStoreClientPlaceholder.CheckReadiness(new("kv-financial-test", false, true, false), new("SUPER_SECRET_CERT", SecretStoreProviderType.AzureKeyVault));
        Assert.Equal("Unhealthy", readiness.Status);
        Assert.Contains(readiness.Issues, x => x.Contains("Managed identity"));
        Assert.DoesNotContain("SUPER_SECRET_CERT", string.Join(";", readiness.Checks.Concat(readiness.Issues)));
    }

    [Fact]
    public async Task Secret_store_certificate_provider_uses_secret_reference()
    {
        var provider = new SecretStoreCertificateProvider(new DisabledSecretStoreClient());
        var result = await provider.LoadAsync(new("default", SignatureProviderType.Xades, "Development", "SRI_CERTIFICATE_SECRET", "vault", "", "", true, "Disabled"), default);
        Assert.False(result.IsLoaded);
        Assert.Equal("secret_store.disabled", result.ErrorCode);
        Assert.DoesNotContain("SRI_CERTIFICATE_SECRET", result.ToString());
    }

    [Fact]
    public async Task Sri_soap_test_dry_run_validates_urls_without_sending()
    {
        var client = new SriSoapReceptionClient();
        var context = new SriClientContext("default", SriEnvironment.Test, "Test", "https://example.test/reception", "https://example.test/auth", 30, 3, 5, false, false, true, true);
        var result = await client.SendAsync(new("123", "<factura><claveAcceso>secret</claveAcceso></factura>", context), default);
        Assert.Equal(SriResponseStatus.Received, result.Status);
        Assert.Contains("dry-run", result.Message);
    }

    [Fact]
    public async Task Sri_manual_connectivity_blocks_document_send_by_default()
    {
        var configuration = new StaticFinancialConfigurationReader(new Dictionary<string, string>
        {
            ["financial.sri.integration.mode"] = "Test",
            ["financial.sri.receptionUrl"] = "https://celcer.sri.gob.ec/reception",
            ["financial.sri.authorizationUrl"] = "https://celcer.sri.gob.ec/authorization",
            ["financial.sri.test.dryRun"] = "false",
            ["financial.sri.test.allowConnectivityProbe"] = "true",
            ["financial.sri.test.allowDocumentSend"] = "false"
        });
        var result = await new SriManualTestConnectivityService(configuration).CheckAsync(Context, default);
        Assert.Equal(SriConnectivityMode.TestSendDisabled, result.Mode);
        Assert.False(result.DocumentSendAllowed);
        Assert.DoesNotContain("celcer.sri.gob.ec/reception", result.ToString());
    }

    [Fact]
    public void Sanitizer_masks_sensitive_values_and_xml()
    {
        Assert.Equal("*********************************************6789", SriSensitiveDataSanitizer.MaskAccessKey("1234567890123456789012345678901234567890123456789"));
        Assert.Equal("******7890", SriSensitiveDataSanitizer.MaskCustomerIdentification("1234567890"));
        var masked = SriSensitiveDataSanitizer.MaskXmlPayload("<factura><claveAcceso>123</claveAcceso></factura>");
        Assert.Contains("redacted", masked);
        Assert.DoesNotContain("claveAcceso", masked);
    }

    [Fact]
    public void Observability_sanitizer_removes_sensitive_payloads()
    {
        var telemetry = SriIntegrationLogSanitizer.Sanitize(new SriObservabilityEvent("corr", "default", Guid.NewGuid(), "Invoice", "Error", "SRI", "Test", 12, 1, "1234567890123456789012345678901234567890123456789", null, "ERR", "<factura>secret</factura>"));
        Assert.EndsWith("6789", telemetry.AccessKeyMasked);
        Assert.DoesNotContain("<factura>", telemetry.SanitizedMessage);
    }

    [Fact]
    public void Portal_content_file_request_excludes_payload_by_default_and_contains_metadata_hash()
    {
        var doc = ElectronicDocument.CreateInvoice("default", SriEnvironment.Test, SriEmissionType.Normal, "001", "001", new DateOnly(2026, 1, 1), "04", "0999999999001", "Cliente", "USD", DateTimeOffset.UtcNow);
        var request = PortalContentFileStorageClient.BuildRequest(doc, System.Text.Encoding.UTF8.GetBytes("<xml />"), "unsigned-xml", "application/xml", Context, new("PortalContentFile", "https://portal.test", "financial", 30, true, false));
        Assert.False(request.IncludePayload);
        Assert.Null(request.PayloadBase64);
        Assert.Equal(request.Hash, request.Metadata["hash"]);
        Assert.Equal("unsigned-xml", request.Metadata["purpose"]);
    }

    [Fact]
    public async Task Sri_readiness_reports_mock_healthy_and_production_blocked()
    {
        var healthy = await new SriIntegrationReadinessService(new StaticFinancialConfigurationReader()).CheckAsync(Context, default);
        Assert.Equal("Healthy", healthy.Status);

        var blockedConfig = new StaticFinancialConfigurationReader(new Dictionary<string, string>
        {
            ["financial.sri.environment"] = "Production",
            ["financial.sri.integration.mode"] = "Production",
            ["financial.sri.allowProduction"] = "false"
        });
        var blocked = await new SriIntegrationReadinessService(blockedConfig).CheckAsync(Context, default);
        Assert.Equal("Unhealthy", blocked.Status);
    }

    [Fact]
    public async Task Sri_readiness_test_mode_is_degraded_and_sanitized()
    {
        var config = new StaticFinancialConfigurationReader(new Dictionary<string, string>
        {
            ["financial.sri.integration.mode"] = "Test",
            ["financial.sri.receptionUrl"] = "https://celcer.sri.gob.ec/reception",
            ["financial.sri.authorizationUrl"] = "https://celcer.sri.gob.ec/authorization",
            ["financial.sri.test.manualValidationRequired"] = "true"
        });
        var result = await new SriIntegrationReadinessService(config).CheckAsync(Context, default);
        Assert.Equal("Degraded", result.Status);
        Assert.DoesNotContain("celcer.sri.gob.ec", string.Join(";", result.Checks.Concat(result.Issues)));
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
