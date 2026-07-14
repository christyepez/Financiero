using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using System.Net;
using Xunit;
namespace Financiero.Api.Tests;
public sealed class HealthTests : IClassFixture<FinancialApiFactory>
{
    private readonly HttpClient _client;
    public HealthTests(FinancialApiFactory factory) => _client = factory.CreateClient();
    [Fact] public async Task Health_is_anonymous_and_healthy() => Assert.True((await _client.GetAsync("/health")).IsSuccessStatusCode);
    [Fact] public async Task Sri_health_is_anonymous() => Assert.False((await _client.GetAsync("/health/sri")).StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden);
    [Fact] public async Task Content_file_health_is_anonymous() => Assert.False((await _client.GetAsync("/health/content-file")).StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden);
    [Fact] public async Task Correlation_id_is_preserved()
    { using var request=new HttpRequestMessage(HttpMethod.Get,"/health");request.Headers.Add("X-Correlation-ID","api-test-correlation");var response=await _client.SendAsync(request);Assert.Equal("api-test-correlation",response.Headers.GetValues("X-Correlation-ID").Single()); }
}
public sealed class FinancialApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("Jwt:Secret", "Test_Secret_At_Least_32_Characters_Long!");
        builder.UseSetting("Jwt:Issuer", "portal-corporativo");
        builder.UseSetting("Jwt:Audience", "portal-corporativo-clients");
        builder.UseSetting("ConnectionStrings:FinancialDb", "");
    }
}

public sealed class RuntimeSecurityTests : IClassFixture<FinancialApiFactory>
{
    private readonly FinancialApiFactory _factory;
    public RuntimeSecurityTests(FinancialApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Read_endpoint_rejects_without_permission()
    {
        var response = await _factory.CreateClient().GetAsync("/api/financial/accounts");
        Assert.True(response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Development_header_allows_required_permission()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/financial/accounts");
        request.Headers.Add("X-Dev-Permissions", "financial.chartofaccounts.read");
        var response = await _factory.CreateClient().SendAsync(request);
        Assert.False(response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Development_header_requires_matching_permission()
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/financial/journal-entries/00000000-0000-0000-0000-000000000001/post");
        request.Headers.Add("X-Dev-Permissions", "financial.journalentries.read");
        var response = await _factory.CreateClient().SendAsync(request);
        Assert.True(response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden);
    }

    [Theory]
    [InlineData("POST", "/api/financial/electronic-documents/00000000-0000-0000-0000-000000000001/sign")]
    [InlineData("POST", "/api/financial/electronic-documents/00000000-0000-0000-0000-000000000001/send")]
    [InlineData("POST", "/api/financial/electronic-documents/00000000-0000-0000-0000-000000000001/authorize")]
    [InlineData("POST", "/api/financial/electronic-documents/00000000-0000-0000-0000-000000000001/generate-ride")]
    [InlineData("POST", "/api/financial/electronic-documents/00000000-0000-0000-0000-000000000001/store-ride")]
    [InlineData("GET", "/api/financial/electronic-documents/sri/readiness")]
    [InlineData("GET", "/api/financial/electronic-documents/content-file/readiness")]
    [InlineData("GET", "/api/financial/electronic-documents/sri/connectivity-probe")]
    [InlineData("POST", "/api/financial/electronic-documents/credit-notes")]
    [InlineData("POST", "/api/financial/electronic-documents/debit-notes")]
    [InlineData("POST", "/api/financial/electronic-documents/withholdings")]
    [InlineData("POST", "/api/financial/electronic-documents/00000000-0000-0000-0000-000000000001/generate-credit-note-xml")]
    [InlineData("POST", "/api/financial/electronic-documents/00000000-0000-0000-0000-000000000001/generate-debit-note-xml")]
    [InlineData("POST", "/api/financial/electronic-documents/00000000-0000-0000-0000-000000000001/generate-withholding-xml")]
    [InlineData("GET", "/api/financial/electronic-documents/00000000-0000-0000-0000-000000000001/ride-preview")]
    [InlineData("GET", "/api/financial/tax-reporting/summary")]
    [InlineData("GET", "/api/financial/tax-reporting/documents")]
    [InlineData("GET", "/api/financial/tax-reporting/tax-totals")]
    [InlineData("GET", "/api/financial/tax-reporting/withholding-totals")]
    [InlineData("GET", "/api/financial/tax-reporting/export")]
    [InlineData("POST", "/api/financial/tax-reporting/export/store")]
    [InlineData("GET", "/api/financial/tax-reporting/ats-readiness?period=2026-01")]
    [InlineData("GET", "/api/financial/tax-reporting/action-queue")]
    [InlineData("GET", "/api/financial/tax-reporting/monthly-summary")]
    public async Task Electronic_document_sensitive_actions_reject_without_permission(string method, string url)
    {
        var response = await _factory.CreateClient().SendAsync(new HttpRequestMessage(new HttpMethod(method), url));
        Assert.True(response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Development_header_is_ignored_in_production()
    {
        var client = _factory.WithWebHostBuilder(builder => builder.UseEnvironment("Production")).CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/financial/accounts");
        request.Headers.Add("X-Dev-Permissions", "financial.chartofaccounts.read");
        var response = await client.SendAsync(request);
        Assert.True(response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden);
    }

    [Theory]
    [InlineData("GET", "/api/financial/fiscal-periods", "financial.fiscalperiods.read")]
    [InlineData("POST", "/api/financial/fiscal-periods/00000000-0000-0000-0000-000000000001/open", "financial.fiscalperiods.open")]
    [InlineData("GET", "/api/financial/journal-entries", "financial.journalentries.read")]
    [InlineData("POST", "/api/financial/journal-entries/00000000-0000-0000-0000-000000000001/post", "financial.journalentries.post")]
    [InlineData("GET", "/api/financial/electronic-documents", "financial.electronicdocuments.read")]
    [InlineData("POST", "/api/financial/electronic-documents/invoices", "financial.electronicdocuments.create")]
    [InlineData("POST", "/api/financial/electronic-documents/credit-notes", "financial.electronicdocuments.create")]
    [InlineData("POST", "/api/financial/electronic-documents/debit-notes", "financial.electronicdocuments.create")]
    [InlineData("POST", "/api/financial/electronic-documents/withholdings", "financial.electronicdocuments.create")]
    [InlineData("POST", "/api/financial/electronic-documents/00000000-0000-0000-0000-000000000001/generate-ride", "financial.electronicdocuments.generate")]
    [InlineData("POST", "/api/financial/electronic-documents/00000000-0000-0000-0000-000000000001/store-ride", "financial.electronicdocuments.generate")]
    [InlineData("POST", "/api/financial/electronic-documents/00000000-0000-0000-0000-000000000001/generate-credit-note-xml", "financial.electronicdocuments.generate")]
    [InlineData("POST", "/api/financial/electronic-documents/00000000-0000-0000-0000-000000000001/generate-debit-note-xml", "financial.electronicdocuments.generate")]
    [InlineData("POST", "/api/financial/electronic-documents/00000000-0000-0000-0000-000000000001/generate-withholding-xml", "financial.electronicdocuments.generate")]
    [InlineData("GET", "/api/financial/electronic-documents/sri/readiness", "financial.electronicdocuments.manage")]
    [InlineData("GET", "/api/financial/electronic-documents/content-file/readiness", "financial.electronicdocuments.manage")]
    [InlineData("GET", "/api/financial/electronic-documents/sri/connectivity-probe", "financial.electronicdocuments.manage")]
    [InlineData("GET", "/api/financial/electronic-documents/00000000-0000-0000-0000-000000000001/integration-status", "financial.electronicdocuments.read")]
    [InlineData("GET", "/api/financial/electronic-documents/00000000-0000-0000-0000-000000000001/ride-preview", "financial.electronicdocuments.read")]
    [InlineData("GET", "/api/financial/tax-reporting/summary", "financial.electronicdocuments.read")]
    [InlineData("GET", "/api/financial/tax-reporting/documents", "financial.electronicdocuments.read")]
    [InlineData("GET", "/api/financial/tax-reporting/tax-totals", "financial.electronicdocuments.read")]
    [InlineData("GET", "/api/financial/tax-reporting/withholding-totals", "financial.electronicdocuments.read")]
    [InlineData("GET", "/api/financial/tax-reporting/export?format=Xml", "financial.electronicdocuments.read")]
    [InlineData("POST", "/api/financial/tax-reporting/export/store?format=Json", "financial.electronicdocuments.manage")]
    [InlineData("GET", "/api/financial/tax-reporting/ats-readiness?period=invalid", "financial.electronicdocuments.read")]
    [InlineData("GET", "/api/financial/tax-reporting/action-queue", "financial.electronicdocuments.read")]
    [InlineData("GET", "/api/financial/tax-reporting/monthly-summary", "financial.electronicdocuments.read")]
    public async Task Development_header_allows_endpoint_specific_permissions(string method, string url, string permission)
    {
        using var request = new HttpRequestMessage(new HttpMethod(method), url);
        request.Headers.Add("X-Dev-Permissions", permission);
        var response = await _factory.CreateClient().SendAsync(request);
        Assert.False(response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Tax_export_invalid_format_returns_bad_request_without_500()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/financial/tax-reporting/export?format=Xml");
        request.Headers.Add("X-Dev-Permissions", "financial.electronicdocuments.read");
        var response = await _factory.CreateClient().SendAsync(request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Ats_readiness_invalid_period_returns_bad_request_without_500()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/financial/tax-reporting/ats-readiness?period=invalid");
        request.Headers.Add("X-Dev-Permissions", "financial.electronicdocuments.read");
        var response = await _factory.CreateClient().SendAsync(request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Development_wildcard_permission_is_allowed_by_design()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/financial/journal-entries");
        request.Headers.Add("X-Dev-Permissions", "financial.*");
        var response = await _factory.CreateClient().SendAsync(request);
        Assert.False(response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden);
    }

}

public sealed class MigrationInventoryTests
{
    [Fact]
    public void Migration_011_tax_documents_foundation_is_idempotent()
    {
        var path = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..", "..", "..", "..", "..",
            "database",
            "migrations",
            "financial",
            "011_tax_documents_foundation.sql"));

        var sql = File.ReadAllText(path);

        Assert.Contains("IF OBJECT_ID('financial.electronic_document_references'", sql);
        Assert.Contains("IF OBJECT_ID('financial.electronic_document_debit_note_reasons'", sql);
        Assert.Contains("IF OBJECT_ID('financial.electronic_document_withholding_taxes'", sql);
        Assert.Contains("IF NOT EXISTS", sql);
    }
}
