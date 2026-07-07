using Financiero.Application;
using Financiero.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;
namespace Financiero.Application.Tests;
public sealed class PortalAdapterTests
{
    [Fact] public async Task Development_adapters_do_not_call_external_services()
    {
        var adapter = new DevelopmentPortalAdapters(Options.Create(new PortalOptions()), NullLogger<DevelopmentPortalAdapters>.Instance, new ConfigurationBuilder().Build());
        var context = new PortalCallContext("default", "corr-1");
        await adapter.RecordAsync(new("created", "financial.test", "1"), context, default);
        await adapter.RequestAsync(new("test", ["dev@example.test"], new Dictionary<string,string>(), "key"), context, default);
        await adapter.EnqueueAsync(new(Guid.NewGuid(), "TestV1", 1, DateTimeOffset.UtcNow, "corr-1", "{}"), context, default);
        Assert.Equal("corr-1", adapter.LastCorrelationId);
    }
    [Fact] public async Task Development_security_is_fail_closed()
    {
        var adapter = new DevelopmentPortalAdapters(Options.Create(new PortalOptions()), NullLogger<DevelopmentPortalAdapters>.Instance, new ConfigurationBuilder().Build());
        Assert.False(await adapter.HasPermissionAsync("financial.test", new("default", "c"), default));
    }
    [Fact] public void Portal_options_bind_feature_flags()
    {
        var values = new Dictionary<string,string?> { ["Portal:GatewayBaseUrl"]="http://portal:8080", ["Portal:UsePortalAudit"]="true", ["FinancialPlatform:TenantId"]="default" };
        var config = new ConfigurationBuilder().AddInMemoryCollection(values).Build();
        var portal = config.GetSection(PortalOptions.SectionName).Get<PortalOptions>();
        var platform = config.GetSection(FinancialPlatformOptions.SectionName).Get<FinancialPlatformOptions>();
        Assert.True(portal!.UsePortalAudit); Assert.Equal(new Uri("http://portal:8080"), portal.GatewayBaseUrl); Assert.Equal("default", platform!.TenantId);
    }
}
