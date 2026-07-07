using Financiero.Application;
using Financiero.Contracts;
using Financiero.Domain;
using Xunit;

namespace Financiero.Application.Tests;

public sealed class ChartOfAccountsServiceTests
{
    private static PortalCallContext Context => new("default", "corr-test");

    [Fact]
    public async Task Creates_account_and_invokes_audit_and_outbox()
    {
        var audit = new RecordingAudit();
        var outbox = new RecordingOutbox();
        var service = new ChartOfAccountsService(new InMemoryAccountRepository(), audit, outbox);

        var account = await service.CreateAsync(new("1", "Activos", "Asset", 1, null), Context, default);

        Assert.Equal("1", account.Code);
        Assert.Equal("AccountCreated", audit.Actions.Single());
        Assert.Equal("FinancialAccountCreated", outbox.EventTypes.Single());
    }

    [Fact]
    public async Task Rejects_duplicate_code()
    {
        var repo = new InMemoryAccountRepository();
        var service = NewService(repo);
        await service.CreateAsync(new("1", "Activos", "Asset", 1, null), Context, default);

        await Assert.ThrowsAsync<FinancialApplicationException>(() => service.CreateAsync(new("1", "Caja", "Asset", 1, null), Context, default));
    }

    [Fact]
    public async Task Rejects_missing_parent_for_level_greater_than_one()
    {
        var service = NewService(new InMemoryAccountRepository());
        await Assert.ThrowsAsync<FinancialApplicationException>(() => service.CreateAsync(new("1.1", "Caja", "Asset", 2, null), Context, default));
    }

    [Fact]
    public async Task Rejects_nonexistent_parent()
    {
        var service = NewService(new InMemoryAccountRepository());
        await Assert.ThrowsAsync<FinancialApplicationException>(() => service.CreateAsync(new("1.1", "Caja", "Asset", 2, Guid.NewGuid()), Context, default));
    }

    [Fact]
    public async Task Rejects_inactive_parent()
    {
        var repo = new InMemoryAccountRepository();
        var service = NewService(repo);
        var parent = await service.CreateAsync(new("1", "Activos", "Asset", 1, null), Context, default);

        await Assert.ThrowsAsync<FinancialApplicationException>(() => service.CreateAsync(new("1.1", "Caja", "Asset", 2, parent.Id), Context, default));
    }

    [Fact]
    public async Task Creates_child_under_active_parent_and_returns_tree()
    {
        var repo = new InMemoryAccountRepository();
        var service = NewService(repo);
        var parent = await service.CreateAsync(new("1", "Activos", "Asset", 1, null), Context, default);
        await service.ActivateAsync(parent.Id, Context, default);

        await service.CreateAsync(new("1.1", "Caja", "Asset", 2, parent.Id), Context, default);
        var tree = await service.GetTreeAsync(Context, default);

        Assert.Single(tree);
        Assert.Single(tree.Single().Children);
    }

    [Fact]
    public async Task Rejects_deactivate_or_archive_parent_with_active_children()
    {
        var repo = new InMemoryAccountRepository();
        var service = NewService(repo);
        var parent = await service.CreateAsync(new("1", "Activos", "Asset", 1, null), Context, default);
        await service.ActivateAsync(parent.Id, Context, default);
        var child = await service.CreateAsync(new("1.1", "Caja", "Asset", 2, parent.Id), Context, default);
        await service.ActivateAsync(child.Id, Context, default);

        await Assert.ThrowsAsync<FinancialApplicationException>(() => service.DeactivateAsync(parent.Id, Context, default));
        await Assert.ThrowsAsync<FinancialApplicationException>(() => service.ArchiveAsync(parent.Id, Context, default));
    }

    [Fact]
    public async Task Search_is_paginated()
    {
        var service = NewService(new InMemoryAccountRepository());
        await service.CreateAsync(new("1", "Activos", "Asset", 1, null), Context, default);
        await service.CreateAsync(new("2", "Pasivos", "Liability", 1, null), Context, default);

        var page = await service.SearchAsync(new SearchAccountsRequest(Page: 1, PageSize: 1), Context, default);

        Assert.Single(page.Items);
        Assert.Equal(2, page.Total);
    }

    [Fact]
    public async Task Compose_does_not_define_financial_sql_server()
    {
        var compose = await File.ReadAllTextAsync(FindRepositoryFile("docker-compose.yml"));
        Assert.DoesNotContain("financial-sql", compose, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("mcr.microsoft.com/mssql", compose, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Database=FinancieroDb", compose, StringComparison.OrdinalIgnoreCase);
    }

    private static ChartOfAccountsService NewService(InMemoryAccountRepository repo) => new(repo, new RecordingAudit(), new RecordingOutbox());

    private static string FindRepositoryFile(string fileName)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, fileName);
            if (File.Exists(candidate)) return candidate;
            directory = directory.Parent;
        }
        throw new FileNotFoundException(fileName);
    }
}

internal sealed class RecordingAudit : IPortalAuditClient
{
    public List<string> Actions { get; } = [];
    public Task RecordAsync(AuditRecord record, PortalCallContext context, CancellationToken ct)
    {
        Actions.Add(record.Action);
        return Task.CompletedTask;
    }
}

internal sealed class RecordingOutbox : IPortalOutboxClient
{
    public List<string> EventTypes { get; } = [];
    public Task EnqueueAsync(OutboxEnvelope message, PortalCallContext context, CancellationToken ct)
    {
        EventTypes.Add(message.EventType);
        return Task.CompletedTask;
    }
}

internal sealed class InMemoryAccountRepository : IAccountRepository
{
    private readonly List<Account> _accounts = [];
    public Task AddAsync(Account account, CancellationToken ct) { _accounts.Add(account); return Task.CompletedTask; }
    public Task<Account?> GetByIdAsync(Guid id, string tenantId, CancellationToken ct) => Task.FromResult(_accounts.FirstOrDefault(x => x.Id == id && x.TenantId == tenantId));
    public Task<Account?> GetByCodeAsync(string code, string tenantId, CancellationToken ct) => Task.FromResult(_accounts.FirstOrDefault(x => x.Code == code && x.TenantId == tenantId));
    public Task<bool> ExistsByCodeAsync(string code, string tenantId, Guid? excludingId, CancellationToken ct) =>
        Task.FromResult(_accounts.Any(x => x.Code == code && x.TenantId == tenantId && (!excludingId.HasValue || x.Id != excludingId)));
    public Task<bool> HasChildrenAsync(Guid id, string tenantId, CancellationToken ct) =>
        Task.FromResult(_accounts.Any(x => x.ParentAccountId == id && x.TenantId == tenantId && x.Status == AccountStatus.Active));
    public Task<(IReadOnlyCollection<Account> Items, long Total)> SearchAsync(string tenantId, string? search, AccountType? type, AccountStatus? status, int page, int pageSize, CancellationToken ct)
    {
        var query = _accounts.Where(x => x.TenantId == tenantId);
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(x => x.Code.Contains(search) || x.Name.Contains(search));
        if (type.HasValue) query = query.Where(x => x.Type == type);
        if (status.HasValue) query = query.Where(x => x.Status == status);
        var materialized = query.OrderBy(x => x.Code).ToArray();
        return Task.FromResult(((IReadOnlyCollection<Account>)materialized.Skip((page - 1) * pageSize).Take(pageSize).ToArray(), (long)materialized.Length));
    }
    public Task<IReadOnlyCollection<Account>> GetTreeAccountsAsync(string tenantId, CancellationToken ct) =>
        Task.FromResult((IReadOnlyCollection<Account>)_accounts.Where(x => x.TenantId == tenantId).OrderBy(x => x.Code).ToArray());
    public Task SaveChangesAsync(CancellationToken ct) => Task.CompletedTask;
}
