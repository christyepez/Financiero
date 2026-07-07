using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Xunit;
namespace Financiero.Api.Tests;
public sealed class HealthTests : IClassFixture<FinancialApiFactory>
{
    private readonly HttpClient _client;
    public HealthTests(FinancialApiFactory factory) => _client = factory.CreateClient();
    [Fact] public async Task Health_is_anonymous_and_healthy() => Assert.True((await _client.GetAsync("/health")).IsSuccessStatusCode);
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
