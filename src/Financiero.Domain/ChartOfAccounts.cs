namespace Financiero.Domain;

public enum AccountType { Asset, Liability, Equity, Income, Expense, Cost, Memo, Other }
public enum AccountStatus { Draft, Active, Inactive, Archived }

public sealed class FinancialDomainException(string code, string message) : InvalidOperationException(message)
{
    public string Code { get; } = code;
}

public sealed class Account
{
    private Account() { }

    private Account(Guid id, string tenantId, string code, string name, AccountType type, int level, Guid? parentAccountId, bool isMovementAccount, DateTimeOffset now)
    {
        Id = id;
        TenantId = Required(tenantId, nameof(TenantId));
        Code = NormalizeCode(code);
        Name = Required(name, nameof(Name));
        Type = type;
        Level = ValidateLevel(level);
        ParentAccountId = parentAccountId;
        IsMovementAccount = isMovementAccount;
        Status = AccountStatus.Draft;
        CreatedAtUtc = now;
        UpdatedAtUtc = now;
    }

    public Guid Id { get; private set; }
    public string TenantId { get; private set; } = FinancialTenant.Default;
    public string Code { get; private set; } = "";
    public string Name { get; private set; } = "";
    public AccountType Type { get; private set; }
    public int Level { get; private set; }
    public Guid? ParentAccountId { get; private set; }
    public bool IsMovementAccount { get; private set; }
    public AccountStatus Status { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public static Account Create(string tenantId, string code, string name, AccountType type, int level, Guid? parentAccountId, bool isMovementAccount, DateTimeOffset now) =>
        new(Guid.NewGuid(), tenantId, code, name, type, level, parentAccountId, isMovementAccount, now);

    public void Update(string code, string name, AccountType type, int level, Guid? parentAccountId, bool isMovementAccount, DateTimeOffset now)
    {
        EnsureNotArchived();
        if (parentAccountId == Id) throw new FinancialDomainException("account.parent.self", "Parent account cannot be the same account.");
        Code = NormalizeCode(code);
        Name = Required(name, nameof(Name));
        Type = type;
        Level = ValidateLevel(level);
        ParentAccountId = parentAccountId;
        IsMovementAccount = isMovementAccount;
        UpdatedAtUtc = now;
    }

    public void Activate(DateTimeOffset now)
    {
        EnsureNotArchived();
        Status = AccountStatus.Active;
        UpdatedAtUtc = now;
    }

    public void Deactivate(DateTimeOffset now)
    {
        EnsureNotArchived();
        Status = AccountStatus.Inactive;
        UpdatedAtUtc = now;
    }

    public void Archive(DateTimeOffset now)
    {
        Status = AccountStatus.Archived;
        UpdatedAtUtc = now;
    }

    public void MarkAsSummaryAccount(DateTimeOffset now)
    {
        if (!IsMovementAccount) return;
        IsMovementAccount = false;
        UpdatedAtUtc = now;
    }

    private void EnsureNotArchived()
    {
        if (Status == AccountStatus.Archived) throw new FinancialDomainException("account.archived", "Archived accounts cannot be modified.");
    }

    public static string NormalizeCode(string code) => Required(code, nameof(Code)).Trim().ToUpperInvariant();
    private static int ValidateLevel(int level) => level >= 1 ? level : throw new FinancialDomainException("account.level.invalid", "Account level must be greater than or equal to 1.");
    private static string Required(string value, string name) => string.IsNullOrWhiteSpace(value) ? throw new FinancialDomainException($"{name.ToLowerInvariant()}.required", $"{name} is required.") : value.Trim();
}

public sealed record AccountCreated(Guid AccountId, string TenantId, string Code, string CorrelationId)
    : DomainEvent(Guid.NewGuid(), DateTimeOffset.UtcNow, CorrelationId);
public sealed record AccountUpdated(Guid AccountId, string TenantId, string Code, string CorrelationId)
    : DomainEvent(Guid.NewGuid(), DateTimeOffset.UtcNow, CorrelationId);
public sealed record AccountStatusChanged(Guid AccountId, string TenantId, string Code, AccountStatus Status, string CorrelationId)
    : DomainEvent(Guid.NewGuid(), DateTimeOffset.UtcNow, CorrelationId);
