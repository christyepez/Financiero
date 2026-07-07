using Financiero.Domain;
using Xunit;
namespace Financiero.Domain.Tests;
public sealed class FinancialPrecisionTests
{
    [Fact] public void Uses_approved_precision() { Assert.Equal(19, FinancialPrecision.Precision); Assert.Equal(4, FinancialPrecision.Scale); }
    [Fact] public void Normalizes_using_bankers_rounding() => Assert.Equal(10.1234m, FinancialPrecision.Normalize(10.12345m));
    [Fact] public void Default_tenant_matches_portal_foundation() => Assert.Equal("default", FinancialTenant.Default);
}
