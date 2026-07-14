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
        Assert.Contains("CreditNoteRulesValidated", outbox.EventTypes);
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
        Assert.Contains("DebitNoteRulesValidated", outbox.EventTypes);
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
        Assert.Contains("WithholdingRulesValidated", outbox.EventTypes);
        Assert.Contains("WithholdingTaxAdded", outbox.EventTypes);
        Assert.Contains("WithholdingXmlGenerated", outbox.EventTypes);
    }

    [Fact]
    public async Task Generate_withholding_xml_fails_when_tax_rules_are_incomplete()
    {
        var service = NewService(new InMemoryElectronicDocumentRepository(), new RecordingAudit(), new RecordingOutbox());
        var draft = await service.CreateWithholdingDraftAsync(new(new DateOnly(2026, 1, 2), "04", "0999999999001", "Proveedor", "01/2026", "01", "001-001-000000001", new DateOnly(2026, 1, 1)), Context, default);

        var ex = await Assert.ThrowsAsync<FinancialApplicationException>(() => service.GenerateWithholdingXmlAsync(draft.Id, Context, default));

        Assert.Equal("sri.xml.validation.failed", ex.Code);
        Assert.Contains("impuestos", ex.Message);
    }

    [Fact]
    public void Development_catalog_provider_exposes_foundation_catalog_version()
    {
        var provider = new DevelopmentSriCatalogProvider();

        Assert.Equal(SriCatalogService.FoundationVersion, provider.Version);
        Assert.True(provider.Contains("documentType", "04"));
        Assert.True(provider.Contains("withholdingCode", "312"));
        Assert.False(provider.Contains("documentType", "99"));
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
        Assert.Equal(request.Hash, request.Metadata.Values["hash"]);
        Assert.Equal("unsigned-xml", request.Metadata.Values["purpose"]);
        Assert.DoesNotContain("<xml", request.ToString());
        Assert.DoesNotContain("PayloadBase64=", request.ToString().Replace("PayloadBase64=**REDACTED**", ""));
    }

    [Fact]
    public void Portal_content_file_request_can_include_payload_only_when_enabled()
    {
        var doc = ElectronicDocument.CreateInvoice("default", SriEnvironment.Test, SriEmissionType.Normal, "001", "001", new DateOnly(2026, 1, 1), "04", "0999999999001", "Cliente", "USD", DateTimeOffset.UtcNow);
        var request = PortalContentFileStorageClient.BuildRequest(doc, System.Text.Encoding.UTF8.GetBytes("<xml />"), "signed-xml", "application/xml", Context, new("PortalContentFile", "https://portal.test", "financial", 30, true, false, SendPayloads: true));

        Assert.True(request.IncludePayload);
        Assert.NotNull(request.PayloadBase64);
        Assert.DoesNotContain(request.PayloadBase64!, request.ToString());
        Assert.Contains("REDACTED", request.ToString());
    }

    [Fact]
    public async Task Portal_content_file_blocks_production_payload_without_explicit_approval()
    {
        var client = new PortalContentFileStorageClient(new("PortalContentFile", "https://portal.test", "financial-electronic-documents", 30, true, true, SendPayloads: true, EnvironmentName: "Production"));
        var doc = ElectronicDocument.CreateInvoice("default", SriEnvironment.Test, SriEmissionType.Normal, "001", "001", new DateOnly(2026, 1, 1), "04", "0999999999001", "Cliente", "USD", DateTimeOffset.UtcNow);

        var ex = await Assert.ThrowsAsync<FinancialApplicationException>(() => client.SaveSignedXmlAsync(doc, "<factura />", Context, default));

        Assert.Equal("sri.storage.payload.production_blocked", ex.Code);
    }

    [Fact]
    public async Task Portal_content_file_dry_run_returns_stable_storage_id_without_http_call()
    {
        var handler = new RecordingHttpHandler(new HttpResponseMessage(System.Net.HttpStatusCode.OK));
        var client = new PortalContentFileStorageClient(new("PortalContentFile", "https://portal.test", "financial", 30, true, false, DryRun: true), new PortalContentFileHttpClient(new HttpClient(handler)));
        var doc = ElectronicDocument.CreateInvoice("default", SriEnvironment.Test, SriEmissionType.Normal, "001", "001", new DateOnly(2026, 1, 1), "04", "0999999999001", "Cliente", "USD", DateTimeOffset.UtcNow);

        var stored = await client.SaveUnsignedXmlAsync(doc, "<factura />", Context, default);

        Assert.StartsWith("portal-dryrun://financial/default/", stored.StorageId);
        Assert.Equal(0, handler.Calls);
    }

    [Fact]
    public async Task Portal_content_file_real_http_success_maps_portal_response_and_headers()
    {
        var response = new HttpResponseMessage(System.Net.HttpStatusCode.Created)
        {
            Content = new StringContent("""{"storageId":"portal-content-file://files/123","provider":"PortalContentFile","storedAtUtc":"2026-01-01T00:00:00Z","contractStatus":"Uploaded"}""")
        };
        var handler = new RecordingHttpHandler(response);
        var client = new PortalContentFileStorageClient(new("PortalContentFile", "https://portal.test", "financial", 30, true, true, DryRun: false, AuthRequired: true), new PortalContentFileHttpClient(new HttpClient(handler)));
        var doc = ElectronicDocument.CreateInvoice("default", SriEnvironment.Test, SriEmissionType.Normal, "001", "001", new DateOnly(2026, 1, 1), "04", "0999999999001", "Cliente", "USD", DateTimeOffset.UtcNow);

        var stored = await client.SaveRidePdfAsync(doc, [1, 2, 3], Context with { BearerToken = "test-token" }, default);

        Assert.Equal("portal-content-file://files/123", stored.StorageId);
        Assert.Equal("Bearer", handler.LastRequest!.Headers.Authorization!.Scheme);
        Assert.Equal("default", handler.LastRequest.Headers.GetValues("X-Tenant-Id").Single());
        Assert.Equal("corr-sri", handler.LastRequest.Headers.GetValues("X-Correlation-Id").Single());
        Assert.Equal("Financiero", handler.LastRequest.Headers.GetValues("X-Source-System").Single());
        Assert.DoesNotContain("test-token", stored.ToString());
    }

    [Fact]
    public async Task Portal_content_file_missing_token_fails_when_real_http_requires_auth()
    {
        var client = new PortalContentFileStorageClient(new("PortalContentFile", "https://portal.test", "financial", 30, true, true, DryRun: false, AuthRequired: true), new PortalContentFileHttpClient(new HttpClient(new RecordingHttpHandler(new HttpResponseMessage(System.Net.HttpStatusCode.OK)))));
        var doc = ElectronicDocument.CreateInvoice("default", SriEnvironment.Test, SriEmissionType.Normal, "001", "001", new DateOnly(2026, 1, 1), "04", "0999999999001", "Cliente", "USD", DateTimeOffset.UtcNow);

        var ex = await Assert.ThrowsAsync<FinancialApplicationException>(() => client.SaveRidePdfAsync(doc, [1, 2, 3], Context, default));

        Assert.Equal("content_file.auth.token_required", ex.Code);
    }

    [Fact]
    public async Task Portal_content_file_invalid_url_and_external_error_are_safe()
    {
        var doc = ElectronicDocument.CreateInvoice("default", SriEnvironment.Test, SriEmissionType.Normal, "001", "001", new DateOnly(2026, 1, 1), "04", "0999999999001", "Cliente", "USD", DateTimeOffset.UtcNow);
        var invalid = new PortalContentFileStorageClient(new("PortalContentFile", "not-a-url", "financial", 30, true, false, DryRun: false), new PortalContentFileHttpClient(new HttpClient()));
        var invalidEx = await Assert.ThrowsAsync<FinancialApplicationException>(() => invalid.SaveUnsignedXmlAsync(doc, "<factura />", Context, default));
        Assert.Equal("content_file.base_url.invalid", invalidEx.Code);

        var failure = new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest)
        {
            Content = new StringContent("""{"error":"bad","token":"Bearer SECRET","payloadBase64":"QUJD","url":"https://portal.test/files?token=SECRET"}""")
        };
        var failed = new PortalContentFileStorageClient(new("PortalContentFile", "https://portal.test", "financial", 30, true, false, DryRun: false), new PortalContentFileHttpClient(new HttpClient(new RecordingHttpHandler(failure))));
        var failedEx = await Assert.ThrowsAsync<FinancialApplicationException>(() => failed.SaveUnsignedXmlAsync(doc, "<factura />", Context, default));
        Assert.Equal("content_file.http.failed", failedEx.Code);
        Assert.DoesNotContain("SECRET", failedEx.Message);
        Assert.DoesNotContain("QUJD", failedEx.Message);
    }

    [Fact]
    public void Portal_content_file_contract_validator_rejects_sensitive_metadata()
    {
        var request = new PortalContentFileRequest("unsigned-xml", "safe-file", "application/xml", "ABC", 1, "financial", "corr", "default",
            new("Financiero", null, null, null, null, null, new Dictionary<string, string> { ["xml"] = "<factura />", ["token"] = "Bearer SECRET" }),
            false, null);

        var ex = Assert.Throws<FinancialApplicationException>(() => PortalContentFileContractValidator.Validate(request));

        Assert.Equal("content_file.contract.invalid", ex.Code);
        Assert.DoesNotContain("SECRET", PortalContentFileErrorMapper.Sanitize(ex.Message));
    }

    [Fact]
    public async Task Content_file_readiness_reports_development_healthy_and_portal_missing_url_unhealthy()
    {
        var development = await new ContentFileReadinessService(new StaticFinancialConfigurationReader(), new RecordingAudit()).CheckAsync(Context, default);
        Assert.Equal("Healthy", development.Status);

        var portalConfig = new StaticFinancialConfigurationReader(new Dictionary<string, string>
        {
            ["financial.sri.storage.provider"] = "PortalContentFile"
        });
        var portal = await new ContentFileReadinessService(portalConfig, new RecordingAudit()).CheckAsync(Context, default);
        Assert.Equal("Unhealthy", portal.Status);
        Assert.Contains(portal.Issues, x => x.Contains("portalBaseUrl"));

        var dryRunConfig = new StaticFinancialConfigurationReader(new Dictionary<string, string>
        {
            ["financial.sri.storage.provider"] = "PortalContentFile",
            ["financial.sri.storage.portalBaseUrl"] = "https://portal.test",
            ["financial.sri.storage.dryRun"] = "true"
        });
        var dryRun = await new ContentFileReadinessService(dryRunConfig, new RecordingAudit()).CheckAsync(Context, default);
        Assert.Equal("Degraded", dryRun.Status);
        Assert.Contains(dryRun.Checks, x => x.Contains("dryRun=True"));
    }

    [Fact]
    public async Task Tax_export_store_registers_development_storage_metadata()
    {
        var repo = new InMemoryElectronicDocumentRepository();
        var service = NewService(repo, new RecordingAudit(), new RecordingOutbox());
        var draft = await service.CreateInvoiceDraftAsync(new(new DateOnly(2026, 2, 1), "05", "0102030405", "Cliente Export"), Context, default);
        await service.AddInvoiceLineAsync(draft.Id, new("SKU", "Servicio", 1, 10, 0), Context, default);
        await service.GenerateInvoiceXmlAsync(draft.Id, Context, default);
        var export = new TaxExportService(new TaxReportingService(repo, new RecordingAudit()), repo, new DevelopmentElectronicDocumentStorageClient(), new RecordingAudit());

        var stored = await export.ExportAsync(new(From: new DateOnly(2026, 2, 1), To: new DateOnly(2026, 2, 28), Format: "Json", Store: true), Context, default);

        Assert.NotNull(stored.StoredFile);
        Assert.StartsWith("dev://tax-export", stored.StoredFile.StorageId);
        Assert.Equal(stored.Metadata.Hash, stored.StoredFile.Hash);
        Assert.Equal("tax-export-json", stored.StoredFile.Purpose);
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

    [Fact]
    public async Task Ride_preview_is_sanitized_and_does_not_expose_full_access_key_or_identification()
    {
        var repo = new InMemoryElectronicDocumentRepository();
        var service = NewService(repo, new RecordingAudit(), new RecordingOutbox());
        var draft = await service.CreateInvoiceDraftAsync(new(new DateOnly(2026, 1, 1), "05", "0102030405", "Cliente"), Context, default);
        await service.AddInvoiceLineAsync(draft.Id, new("SKU", "Servicio", 1, 10, 0), Context, default);
        var generated = await service.GenerateInvoiceXmlAsync(draft.Id, Context, default);

        var preview = await service.GetRidePreviewAsync(generated.Id, Context, default);

        Assert.Contains("RIDE Factura", preview.Html);
        Assert.DoesNotContain(generated.AccessKey!, preview.Html);
        Assert.DoesNotContain("0102030405", preview.Html);
        Assert.Contains("****", preview.Html);
        Assert.NotEmpty(preview.Hash);
    }

    [Fact]
    public async Task Ride_generation_supports_credit_note_debit_note_and_withholding()
    {
        var repo = new InMemoryElectronicDocumentRepository();
        var service = NewService(repo, new RecordingAudit(), new RecordingOutbox());
        var creditNote = await service.CreateCreditNoteDraftAsync(new(new DateOnly(2026, 1, 2), "04", "0999999999001", "Cliente", "01", "001-001-000000001", new DateOnly(2026, 1, 1), "Devolución"), Context, default);
        await service.AddInvoiceLineAsync(creditNote.Id, new("NC", "Devolución", 1, 5, 0), Context, default);
        creditNote = await service.GenerateCreditNoteXmlAsync(creditNote.Id, Context, default);
        var debitNote = await service.CreateDebitNoteDraftAsync(new(new DateOnly(2026, 1, 3), "04", "0999999999001", "Cliente", "01", "001-001-000000001", new DateOnly(2026, 1, 1)), Context, default);
        await service.AddDebitNoteReasonAsync(debitNote.Id, new("Interés", 2.50m), Context, default);
        debitNote = await service.GenerateDebitNoteXmlAsync(debitNote.Id, Context, default);
        var withholding = await service.CreateWithholdingDraftAsync(new(new DateOnly(2026, 1, 4), "04", "0999999999001", "Proveedor", "01/2026", "01", "001-001-000000001", new DateOnly(2026, 1, 1)), Context, default);
        await service.AddWithholdingTaxAsync(withholding.Id, new("1", "312", 100, 1.75m, 1.75m, "001-001-000000001", new DateOnly(2026, 1, 1), "01/2026"), Context, default);
        withholding = await service.GenerateWithholdingXmlAsync(withholding.Id, Context, default);

        var creditRide = await service.GetRidePreviewAsync(creditNote.Id, Context, default);
        var debitRide = await service.GetRidePreviewAsync(debitNote.Id, Context, default);
        var withholdingRide = await service.GetRidePreviewAsync(withholding.Id, Context, default);

        Assert.Contains("Nota de cr", creditRide.Html);
        Assert.Contains("Nota de d", debitRide.Html);
        Assert.Contains("Comprobante de reten", withholdingRide.Html);
        Assert.DoesNotContain("<notaCredito", creditRide.Html);
        Assert.DoesNotContain("<notaDebito", debitRide.Html);
        Assert.DoesNotContain("<comprobanteRetencion", withholdingRide.Html);
    }

    [Fact]
    public async Task Tax_reporting_summarizes_documents_and_masks_sensitive_values()
    {
        var repo = new InMemoryElectronicDocumentRepository();
        var service = NewService(repo, new RecordingAudit(), new RecordingOutbox());
        var invoice = await service.CreateInvoiceDraftAsync(new(new DateOnly(2026, 1, 1), "04", "0999999999001", "Cliente"), Context, default);
        await service.AddInvoiceLineAsync(invoice.Id, new("SKU", "Servicio", 2, 10, 0), Context, default);
        invoice = await service.GenerateInvoiceXmlAsync(invoice.Id, Context, default);
        var withholding = await service.CreateWithholdingDraftAsync(new(new DateOnly(2026, 1, 2), "04", "0999999999001", "Proveedor", "01/2026", "01", "001-001-000000001", new DateOnly(2026, 1, 1)), Context, default);
        await service.AddWithholdingTaxAsync(withholding.Id, new("1", "312", 100, 1.75m, 1.75m, "001-001-000000001", new DateOnly(2026, 1, 1), "01/2026"), Context, default);
        await service.GenerateWithholdingXmlAsync(withholding.Id, Context, default);

        var report = await new TaxReportingService(repo, new RecordingAudit()).GetSummaryAsync(new(StartDate: new DateOnly(2026, 1, 1), EndDate: new DateOnly(2026, 1, 31)), Context, default);

        Assert.Equal(2, report.Totals.Count);
        Assert.Contains("Invoice", report.ByDocumentType.Keys);
        Assert.Contains(report.WithholdingTotals, x => x.WithholdingCode == "312");
        Assert.All(report.Documents, x => Assert.DoesNotContain("0999999999001", x.CustomerIdentificationMasked));
        Assert.All(report.Documents.Where(x => x.AccessKeyMasked is not null), x => Assert.DoesNotContain(invoice.AccessKey!, x.AccessKeyMasked));
    }

    [Fact]
    public async Task Tax_export_json_and_csv_are_sanitized_and_hashed()
    {
        var repo = new InMemoryElectronicDocumentRepository();
        var service = NewService(repo, new RecordingAudit(), new RecordingOutbox());
        var draft = await service.CreateInvoiceDraftAsync(new(new DateOnly(2026, 2, 1), "05", "0102030405", "Cliente Export"), Context, default);
        await service.AddInvoiceLineAsync(draft.Id, new("SKU", "Servicio", 1, 10, 0), Context, default);
        var generated = await service.GenerateInvoiceXmlAsync(draft.Id, Context, default);
        var export = new TaxExportService(new TaxReportingService(repo, new RecordingAudit()), repo, new DevelopmentElectronicDocumentStorageClient(), new RecordingAudit());

        var json = await export.ExportAsync(new(From: new DateOnly(2026, 2, 1), To: new DateOnly(2026, 2, 28), Format: "Json"), Context, default);
        var csv = await export.ExportAsync(new(From: new DateOnly(2026, 2, 1), To: new DateOnly(2026, 2, 28), Format: "Csv"), Context, default);
        var jsonText = System.Text.Encoding.UTF8.GetString(json.Content);
        var csvText = System.Text.Encoding.UTF8.GetString(csv.Content);

        Assert.Equal("application/json", json.Metadata.ContentType);
        Assert.Equal("text/csv", csv.Metadata.ContentType);
        Assert.NotEmpty(json.Metadata.Hash);
        Assert.DoesNotContain(generated.AccessKey!, jsonText + csvText);
        Assert.DoesNotContain("0102030405", jsonText + csvText);
        Assert.DoesNotContain("<factura", jsonText + csvText);
        Assert.DoesNotContain("PRIVATE KEY", jsonText + csvText);
    }

    [Fact]
    public async Task Tax_export_withholding_totals_and_action_queue_are_foundation_safe()
    {
        var repo = new InMemoryElectronicDocumentRepository();
        var service = NewService(repo, new RecordingAudit(), new RecordingOutbox());
        var withholding = await service.CreateWithholdingDraftAsync(new(new DateOnly(2026, 2, 2), "04", "0999999999001", "Proveedor", "02/2026", "01", "001-001-000000001", new DateOnly(2026, 2, 1)), Context, default);
        await service.AddWithholdingTaxAsync(withholding.Id, new("1", "312", 100, 1.75m, 1.75m, "001-001-000000001", new DateOnly(2026, 2, 1), "02/2026"), Context, default);
        await service.GenerateWithholdingXmlAsync(withholding.Id, Context, default);
        var export = new TaxExportService(new TaxReportingService(repo, new RecordingAudit()), repo, new DevelopmentElectronicDocumentStorageClient(), new RecordingAudit());

        var csv = await export.ExportAsync(new(From: new DateOnly(2026, 2, 1), To: new DateOnly(2026, 2, 28), Kind: "WithholdingTotals", Format: "Csv"), Context, default);
        var queue = await export.GetActionQueueAsync(new(StartDate: new DateOnly(2026, 2, 1), EndDate: new DateOnly(2026, 2, 28)), Context, default);

        Assert.Contains("withheldAmount", System.Text.Encoding.UTF8.GetString(csv.Content));
        Assert.Contains(queue, x => x.Action == "GeneratedNotSigned" && x.Count == 1);
        Assert.Contains(queue, x => x.Action == "MissingRide" && x.Count == 1);
    }

    [Fact]
    public async Task Ats_readiness_detects_missing_authorization_and_disclaims_official_compliance()
    {
        var repo = new InMemoryElectronicDocumentRepository();
        var service = NewService(repo, new RecordingAudit(), new RecordingOutbox());
        var draft = await service.CreateInvoiceDraftAsync(new(new DateOnly(2026, 3, 1), "04", "0999999999001", "Cliente ATS"), Context, default);
        await service.AddInvoiceLineAsync(draft.Id, new("SKU", "Servicio", 1, 10, 0), Context, default);
        await service.GenerateInvoiceXmlAsync(draft.Id, Context, default);
        var export = new TaxExportService(new TaxReportingService(repo, new RecordingAudit()), repo, new DevelopmentElectronicDocumentStorageClient(), new RecordingAudit());

        var readiness = await export.EvaluateAtsReadinessAsync(new("2026-03"), Context, default);

        Assert.Equal(AtsReadinessStatus.MissingData, readiness.Status);
        Assert.Contains(readiness.Issues, x => x.Code == "ats.document.not_authorized");
        Assert.Contains("not an official ATS", readiness.Disclaimer);
    }

    [Fact]
    public async Task Monthly_summary_groups_by_month_and_document_type()
    {
        var repo = new InMemoryElectronicDocumentRepository();
        var service = NewService(repo, new RecordingAudit(), new RecordingOutbox());
        var invoice = await service.CreateInvoiceDraftAsync(new(new DateOnly(2026, 4, 1), "04", "0999999999001", "Cliente Monthly"), Context, default);
        await service.AddInvoiceLineAsync(invoice.Id, new("SKU", "Servicio", 1, 10, 0), Context, default);
        await service.GenerateInvoiceXmlAsync(invoice.Id, Context, default);
        var export = new TaxExportService(new TaxReportingService(repo, new RecordingAudit()), repo, new DevelopmentElectronicDocumentStorageClient(), new RecordingAudit());

        var summary = await export.GetMonthlySummaryAsync(new(StartDate: new DateOnly(2026, 4, 1), EndDate: new DateOnly(2026, 4, 30)), Context, default);

        Assert.Contains(summary, x => x.Month == "2026-04" && x.DocumentType == "Invoice" && x.Count == 1);
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

internal sealed class RecordingHttpHandler(HttpResponseMessage response) : HttpMessageHandler
{
    public int Calls { get; private set; }
    public HttpRequestMessage? LastRequest { get; private set; }
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Calls++;
        LastRequest = request;
        return Task.FromResult(response);
    }
}
