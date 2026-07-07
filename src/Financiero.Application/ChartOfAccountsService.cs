using System.Text.Json;
using Financiero.Contracts;
using Financiero.Domain;

namespace Financiero.Application;

public sealed class FinancialApplicationException(string code, string message) : InvalidOperationException(message)
{
    public string Code { get; } = code;
}

public interface IAccountRepository
{
    Task AddAsync(Account account, CancellationToken ct);
    Task<Account?> GetByIdAsync(Guid id, string tenantId, CancellationToken ct);
    Task<Account?> GetByCodeAsync(string code, string tenantId, CancellationToken ct);
    Task<bool> ExistsByCodeAsync(string code, string tenantId, Guid? excludingId, CancellationToken ct);
    Task<bool> HasChildrenAsync(Guid id, string tenantId, CancellationToken ct);
    Task<(IReadOnlyCollection<Account> Items, long Total)> SearchAsync(string tenantId, string? search, AccountType? type, AccountStatus? status, int page, int pageSize, CancellationToken ct);
    Task<IReadOnlyCollection<Account>> GetTreeAccountsAsync(string tenantId, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}

public sealed class ChartOfAccountsService(IAccountRepository accounts, IPortalAuditClient audit, IPortalOutboxClient outbox)
{
    public async Task<AccountDto> CreateAsync(CreateAccountRequest request, PortalCallContext context, CancellationToken ct)
    {
        var type = ParseType(request.Type);
        var code = Account.NormalizeCode(request.Code);
        if (await accounts.ExistsByCodeAsync(code, context.TenantId, null, ct)) throw new FinancialApplicationException("account.code.duplicate", "Account code already exists for tenant.");

        var parent = await ValidateParentAsync(request.ParentAccountId, request.Level, context.TenantId, ct);
        var account = Account.Create(context.TenantId, code, request.Name, type, request.Level, request.ParentAccountId, request.IsMovementAccount, DateTimeOffset.UtcNow);
        if (parent is not null) parent.MarkAsSummaryAccount(DateTimeOffset.UtcNow);

        await accounts.AddAsync(account, ct);
        await accounts.SaveChangesAsync(ct);
        await AuditAndOutboxAsync("AccountCreated", "FinancialAccountCreated", account, context, ct);
        return ToDto(account);
    }

    public async Task<AccountDto> UpdateAsync(Guid id, UpdateAccountRequest request, PortalCallContext context, CancellationToken ct)
    {
        var account = await GetRequiredAsync(id, context.TenantId, ct);
        var type = ParseType(request.Type);
        var code = Account.NormalizeCode(request.Code);
        if (await accounts.ExistsByCodeAsync(code, context.TenantId, id, ct)) throw new FinancialApplicationException("account.code.duplicate", "Account code already exists for tenant.");
        var parent = await ValidateParentAsync(request.ParentAccountId, request.Level, context.TenantId, ct);
        if (request.ParentAccountId == id) throw new FinancialApplicationException("account.parent.self", "Parent account cannot be the same account.");
        if (await accounts.HasChildrenAsync(id, context.TenantId, ct) && request.IsMovementAccount) throw new FinancialApplicationException("account.movement.has_children", "Accounts with children cannot be movement accounts.");

        account.Update(code, request.Name, type, request.Level, request.ParentAccountId, request.IsMovementAccount, DateTimeOffset.UtcNow);
        parent?.MarkAsSummaryAccount(DateTimeOffset.UtcNow);
        await accounts.SaveChangesAsync(ct);
        await AuditAndOutboxAsync("AccountUpdated", "FinancialAccountUpdated", account, context, ct);
        return ToDto(account);
    }

    public async Task<AccountDto> ActivateAsync(Guid id, PortalCallContext context, CancellationToken ct)
    {
        var account = await GetRequiredAsync(id, context.TenantId, ct);
        if (account.ParentAccountId is not null)
        {
            var parent = await accounts.GetByIdAsync(account.ParentAccountId.Value, context.TenantId, ct);
            if (parent?.Status != AccountStatus.Active) throw new FinancialApplicationException("account.parent.inactive", "Cannot activate account with inactive parent.");
        }
        account.Activate(DateTimeOffset.UtcNow);
        await accounts.SaveChangesAsync(ct);
        await AuditAndOutboxAsync("AccountActivated", "FinancialAccountStatusChanged", account, context, ct);
        return ToDto(account);
    }

    public async Task<AccountDto> DeactivateAsync(Guid id, PortalCallContext context, CancellationToken ct)
    {
        var account = await GetRequiredAsync(id, context.TenantId, ct);
        account.Deactivate(DateTimeOffset.UtcNow);
        await accounts.SaveChangesAsync(ct);
        await AuditAndOutboxAsync("AccountDeactivated", "FinancialAccountStatusChanged", account, context, ct);
        return ToDto(account);
    }

    public async Task<AccountDto> ArchiveAsync(Guid id, PortalCallContext context, CancellationToken ct)
    {
        var account = await GetRequiredAsync(id, context.TenantId, ct);
        account.Archive(DateTimeOffset.UtcNow);
        await accounts.SaveChangesAsync(ct);
        await AuditAndOutboxAsync("AccountArchived", "FinancialAccountStatusChanged", account, context, ct);
        return ToDto(account);
    }

    public async Task<AccountDto> GetByIdAsync(Guid id, PortalCallContext context, CancellationToken ct) => ToDto(await GetRequiredAsync(id, context.TenantId, ct));
    public async Task<AccountDto> GetByCodeAsync(string code, PortalCallContext context, CancellationToken ct) =>
        ToDto(await accounts.GetByCodeAsync(Account.NormalizeCode(code), context.TenantId, ct) ?? throw new FinancialApplicationException("account.not_found", "Account was not found."));

    public async Task<PageResponse<AccountDto>> SearchAsync(SearchAccountsRequest request, PortalCallContext context, CancellationToken ct)
    {
        AccountType? type = string.IsNullOrWhiteSpace(request.Type) ? null : ParseType(request.Type);
        AccountStatus? status = string.IsNullOrWhiteSpace(request.Status) ? null : ParseStatus(request.Status);
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var (items, total) = await accounts.SearchAsync(context.TenantId, request.Search, type, status, page, pageSize, ct);
        return new PageResponse<AccountDto>(items.Select(ToDto).ToArray(), page, pageSize, total);
    }

    public async Task<IReadOnlyCollection<AccountTreeNode>> GetTreeAsync(PortalCallContext context, CancellationToken ct)
    {
        var all = await accounts.GetTreeAccountsAsync(context.TenantId, ct);
        return BuildTree(all, null);
    }

    private async Task<Account?> ValidateParentAsync(Guid? parentId, int level, string tenantId, CancellationToken ct)
    {
        if (parentId is null)
        {
            if (level > 1) throw new FinancialApplicationException("account.parent.required", "Parent account is required for level greater than one.");
            return null;
        }

        var parent = await accounts.GetByIdAsync(parentId.Value, tenantId, ct) ?? throw new FinancialApplicationException("account.parent.not_found", "Parent account does not exist.");
        if (parent.Status != AccountStatus.Active) throw new FinancialApplicationException("account.parent.inactive", "Parent account must be active.");
        if (level != parent.Level + 1) throw new FinancialApplicationException("account.level.parent", "Account level must be parent level plus one.");
        return parent;
    }

    private async Task<Account> GetRequiredAsync(Guid id, string tenantId, CancellationToken ct) =>
        await accounts.GetByIdAsync(id, tenantId, ct) ?? throw new FinancialApplicationException("account.not_found", "Account was not found.");

    private async Task AuditAndOutboxAsync(string auditAction, string outboxType, Account account, PortalCallContext context, CancellationToken ct)
    {
        var payload = JsonSerializer.Serialize(new { account.Id, account.TenantId, account.Code, Status = account.Status.ToString() });
        await audit.RecordAsync(new AuditRecord(auditAction, "financial.account", account.Id.ToString(), new { account.Code, account.Status }), context, ct);
        await outbox.EnqueueAsync(new OutboxEnvelope(Guid.NewGuid(), outboxType, 1, DateTimeOffset.UtcNow, context.CorrelationId, payload), context, ct);
    }

    public static AccountDto ToDto(Account account) => new(account.Id, account.TenantId, account.Code, account.Name, account.Type.ToString(), account.Level,
        account.ParentAccountId, account.IsMovementAccount, account.Status.ToString(), account.CreatedAtUtc, account.UpdatedAtUtc);

    private static AccountType ParseType(string value) => Enum.TryParse<AccountType>(value, true, out var type) ? type : throw new FinancialApplicationException("account.type.invalid", "Account type is invalid.");
    private static AccountStatus ParseStatus(string value) => Enum.TryParse<AccountStatus>(value, true, out var status) ? status : throw new FinancialApplicationException("account.status.invalid", "Account status is invalid.");

    private static IReadOnlyCollection<AccountTreeNode> BuildTree(IReadOnlyCollection<Account> accounts, Guid? parentId) =>
        accounts.Where(x => x.ParentAccountId == parentId).OrderBy(x => x.Code)
            .Select(x => new AccountTreeNode(ToDto(x), BuildTree(accounts, x.Id))).ToArray();
}
