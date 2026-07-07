namespace Financiero.Contracts;

public sealed record AccountDto(Guid Id, string TenantId, string Code, string Name, string Type, int Level, Guid? ParentAccountId,
    bool IsMovementAccount, string Status, DateTimeOffset CreatedAtUtc, DateTimeOffset UpdatedAtUtc);

public sealed record AccountTreeNode(AccountDto Account, IReadOnlyCollection<AccountTreeNode> Children);

public sealed record CreateAccountRequest(string Code, string Name, string Type, int Level, Guid? ParentAccountId, bool IsMovementAccount = true);
public sealed record UpdateAccountRequest(string Code, string Name, string Type, int Level, Guid? ParentAccountId, bool IsMovementAccount);
public sealed record SearchAccountsRequest(string? Search = null, string? Type = null, string? Status = null, int Page = 1, int PageSize = 20);
