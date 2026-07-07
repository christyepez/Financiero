using Financiero.Domain;
using Xunit;

namespace Financiero.Domain.Tests;

public sealed class AccountTests
{
    [Fact]
    public void Creates_valid_account()
    {
        var account = Account.Create("tenant", " 1.1.01 ", "Caja", AccountType.Asset, 1, null, true, DateTimeOffset.UtcNow);
        Assert.Equal("1.1.01", account.Code);
        Assert.Equal(AccountStatus.Draft, account.Status);
        Assert.True(account.IsMovementAccount);
    }

    [Fact]
    public void Rejects_empty_name() =>
        Assert.Throws<FinancialDomainException>(() => Account.Create("tenant", "1", "", AccountType.Asset, 1, null, true, DateTimeOffset.UtcNow));

    [Fact]
    public void Rejects_invalid_level() =>
        Assert.Throws<FinancialDomainException>(() => Account.Create("tenant", "1", "Caja", AccountType.Asset, 0, null, true, DateTimeOffset.UtcNow));

    [Fact]
    public void Activates_and_deactivates_account()
    {
        var account = Account.Create("tenant", "1", "Caja", AccountType.Asset, 1, null, true, DateTimeOffset.UtcNow);
        account.Activate(DateTimeOffset.UtcNow);
        Assert.Equal(AccountStatus.Active, account.Status);
        account.Deactivate(DateTimeOffset.UtcNow);
        Assert.Equal(AccountStatus.Inactive, account.Status);
    }
}
