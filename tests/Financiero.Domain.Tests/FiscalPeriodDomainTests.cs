using Financiero.Domain;
using Xunit;

namespace Financiero.Domain.Tests;

public sealed class FiscalPeriodDomainTests
{
    [Fact]
    public void Creates_valid_fiscal_year()
    {
        var year = FiscalYear.Create("default", 2026, new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31), DateTimeOffset.UtcNow);
        Assert.Equal(FiscalYearStatus.Draft, year.Status);
    }

    [Fact]
    public void Rejects_invalid_dates() =>
        Assert.Throws<FinancialDomainException>(() => FiscalPeriod.Create("default", Guid.NewGuid(), 1, new DateOnly(2026, 2, 1), new DateOnly(2026, 1, 1), DateTimeOffset.UtcNow));

    [Fact]
    public void Locked_period_cannot_be_reopened()
    {
        var period = FiscalPeriod.Create("default", Guid.NewGuid(), 1, new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31), DateTimeOffset.UtcNow);
        period.Lock(DateTimeOffset.UtcNow);
        Assert.Throws<FinancialDomainException>(() => period.Reopen(DateTimeOffset.UtcNow));
    }
}
