using Financiero.Application;
using Financiero.Domain;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Hosting;

namespace Financiero.Infrastructure;

public sealed class FinancialDbContext(DbContextOptions<FinancialDbContext> options) : DbContext(options)
{
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<FiscalYear> FiscalYears => Set<FiscalYear>();
    public DbSet<FiscalPeriod> FiscalPeriods => Set<FiscalPeriod>();
    public DbSet<JournalEntry> JournalEntries => Set<JournalEntry>();
    public DbSet<JournalEntryLine> JournalEntryLines => Set<JournalEntryLine>();
    public DbSet<AccountingSequence> AccountingSequences => Set<AccountingSequence>();
    public DbSet<ElectronicDocument> ElectronicDocuments => Set<ElectronicDocument>();
    public DbSet<ElectronicDocumentLine> ElectronicDocumentLines => Set<ElectronicDocumentLine>();
    public DbSet<ElectronicDocumentTax> ElectronicDocumentTaxes => Set<ElectronicDocumentTax>();
    public DbSet<SriDocumentSequence> SriDocumentSequences => Set<SriDocumentSequence>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("financial");
        modelBuilder.Entity<Account>(entity =>
        {
            entity.ToTable("accounts");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.TenantId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Code).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Type).HasConversion<string>().HasMaxLength(32).IsRequired();
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
            entity.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.ParentAccountId });
        });
        modelBuilder.Entity<FiscalYear>(entity =>
        {
            entity.ToTable("fiscal_years");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.TenantId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
            entity.HasIndex(x => new { x.TenantId, x.Year }).IsUnique();
        });
        modelBuilder.Entity<FiscalPeriod>(entity =>
        {
            entity.ToTable("fiscal_periods");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.TenantId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
            entity.HasIndex(x => new { x.TenantId, x.FiscalYearId, x.PeriodNumber }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.FiscalYearId, x.StartDate, x.EndDate });
        });
        modelBuilder.Entity<JournalEntry>(entity =>
        {
            entity.ToTable("journal_entries");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.TenantId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.EntryNumber).HasMaxLength(64);
            entity.Property(x => x.Reference).HasMaxLength(128);
            entity.Property(x => x.Description).HasMaxLength(512).IsRequired();
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
            entity.Property(x => x.Source).HasConversion<string>().HasMaxLength(32).IsRequired();
            entity.Property(x => x.Reason).HasMaxLength(512);
            entity.HasIndex(x => new { x.TenantId, x.EntryNumber }).IsUnique().HasFilter("[EntryNumber] IS NOT NULL");
            entity.HasIndex(x => new { x.TenantId, x.PostingDate, x.Status });
            entity.Navigation(x => x.Lines).UsePropertyAccessMode(PropertyAccessMode.Field);
            entity.HasMany(x => x.Lines).WithOne().HasForeignKey(x => x.JournalEntryId).OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<JournalEntryLine>(entity =>
        {
            entity.ToTable("journal_entry_lines");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.TenantId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(512);
            entity.Property(x => x.Debit).HasPrecision(FinancialPrecision.Precision, FinancialPrecision.Scale);
            entity.Property(x => x.Credit).HasPrecision(FinancialPrecision.Precision, FinancialPrecision.Scale);
            entity.HasIndex(x => new { x.TenantId, x.JournalEntryId, x.LineNumber }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.AccountId });
        });
        modelBuilder.Entity<AccountingSequence>(entity =>
        {
            entity.ToTable("accounting_sequences");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.TenantId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.SequenceKey).HasMaxLength(64).IsRequired();
            entity.HasIndex(x => new { x.TenantId, x.SequenceKey }).IsUnique();
        });
        modelBuilder.Entity<ElectronicDocument>(entity =>
        {
            entity.ToTable("electronic_documents");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.TenantId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.DocumentType).HasConversion<string>().HasMaxLength(32).IsRequired();
            entity.Property(x => x.Environment).HasConversion<string>().HasMaxLength(32).IsRequired();
            entity.Property(x => x.EmissionType).HasConversion<string>().HasMaxLength(32).IsRequired();
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
            entity.Property(x => x.EstablishmentCode).HasMaxLength(3).IsRequired();
            entity.Property(x => x.EmissionPointCode).HasMaxLength(3).IsRequired();
            entity.Property(x => x.Sequential).HasMaxLength(9);
            entity.Property(x => x.AccessKey).HasMaxLength(49);
            entity.Property(x => x.CustomerIdentificationType).HasMaxLength(2).IsRequired();
            entity.Property(x => x.CustomerIdentification).HasMaxLength(32).IsRequired();
            entity.Property(x => x.CustomerName).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Currency).HasMaxLength(8).IsRequired();
            entity.Property(x => x.SubtotalWithoutTaxes).HasPrecision(FinancialPrecision.Precision, FinancialPrecision.Scale);
            entity.Property(x => x.TotalDiscount).HasPrecision(FinancialPrecision.Precision, FinancialPrecision.Scale);
            entity.Property(x => x.TotalTaxes).HasPrecision(FinancialPrecision.Precision, FinancialPrecision.Scale);
            entity.Property(x => x.TotalAmount).HasPrecision(FinancialPrecision.Precision, FinancialPrecision.Scale);
            entity.Property(x => x.XmlContentHash).HasMaxLength(128);
            entity.Property(x => x.SignedXmlContentHash).HasMaxLength(128);
            entity.Property(x => x.UnsignedXmlStorageId).HasMaxLength(256);
            entity.Property(x => x.SignedXmlStorageId).HasMaxLength(256);
            entity.Property(x => x.AuthorizationXmlStorageId).HasMaxLength(256);
            entity.Property(x => x.RidePdfStorageId).HasMaxLength(256);
            entity.Property(x => x.RidePdfHash).HasMaxLength(128);
            entity.Property(x => x.StorageProvider).HasMaxLength(64);
            entity.Property(x => x.SignatureProvider).HasMaxLength(64);
            entity.Property(x => x.SignatureDigest).HasMaxLength(128);
            entity.Property(x => x.SriAuthorizationNumber).HasMaxLength(64);
            entity.Property(x => x.SriResponseCode).HasMaxLength(64);
            entity.Property(x => x.SriResponseMessage).HasMaxLength(1024);
            entity.Property(x => x.LastSriStatus).HasMaxLength(64);
            entity.Property(x => x.LastSriMessage).HasMaxLength(1024);
            entity.Property(x => x.LastErrorCode).HasMaxLength(128);
            entity.Property(x => x.LastErrorMessage).HasMaxLength(1024);
            entity.Property(x => x.LastIntegrationCorrelationId).HasMaxLength(128);
            entity.HasIndex(x => new { x.TenantId, x.AccessKey }).IsUnique().HasFilter("[AccessKey] IS NOT NULL");
            entity.HasIndex(x => new { x.TenantId, x.DocumentType, x.Environment, x.EstablishmentCode, x.EmissionPointCode, x.Sequential }).IsUnique().HasFilter("[Sequential] IS NOT NULL");
            entity.HasIndex(x => new { x.TenantId, x.Status, x.IssueDate });
            entity.Navigation(x => x.Lines).UsePropertyAccessMode(PropertyAccessMode.Field);
            entity.Navigation(x => x.Taxes).UsePropertyAccessMode(PropertyAccessMode.Field);
            entity.HasMany(x => x.Lines).WithOne().HasForeignKey(x => x.ElectronicDocumentId).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(x => x.Taxes).WithOne().HasForeignKey(x => x.ElectronicDocumentId).OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<ElectronicDocumentLine>(entity =>
        {
            entity.ToTable("electronic_document_lines");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.TenantId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.ProductCode).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(512).IsRequired();
            entity.Property(x => x.Quantity).HasPrecision(FinancialPrecision.Precision, FinancialPrecision.Scale);
            entity.Property(x => x.UnitPrice).HasPrecision(FinancialPrecision.Precision, FinancialPrecision.Scale);
            entity.Property(x => x.Discount).HasPrecision(FinancialPrecision.Precision, FinancialPrecision.Scale);
            entity.Property(x => x.Subtotal).HasPrecision(FinancialPrecision.Precision, FinancialPrecision.Scale);
            entity.Property(x => x.Total).HasPrecision(FinancialPrecision.Precision, FinancialPrecision.Scale);
            entity.HasIndex(x => new { x.TenantId, x.ElectronicDocumentId, x.LineNumber }).IsUnique();
        });
        modelBuilder.Entity<ElectronicDocumentTax>(entity =>
        {
            entity.ToTable("electronic_document_taxes");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.TenantId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.TaxCode).HasMaxLength(8).IsRequired();
            entity.Property(x => x.TaxPercentageCode).HasMaxLength(8).IsRequired();
            entity.Property(x => x.TaxRate).HasPrecision(FinancialPrecision.Precision, FinancialPrecision.Scale);
            entity.Property(x => x.TaxBase).HasPrecision(FinancialPrecision.Precision, FinancialPrecision.Scale);
            entity.Property(x => x.TaxAmount).HasPrecision(FinancialPrecision.Precision, FinancialPrecision.Scale);
            entity.HasIndex(x => new { x.TenantId, x.ElectronicDocumentId });
        });
        modelBuilder.Entity<SriDocumentSequence>(entity =>
        {
            entity.ToTable("sri_document_sequences");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.TenantId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.DocumentType).HasConversion<string>().HasMaxLength(32).IsRequired();
            entity.Property(x => x.Environment).HasConversion<string>().HasMaxLength(32).IsRequired();
            entity.Property(x => x.EstablishmentCode).HasMaxLength(3).IsRequired();
            entity.Property(x => x.EmissionPointCode).HasMaxLength(3).IsRequired();
            entity.HasIndex(x => new { x.TenantId, x.DocumentType, x.Environment, x.EstablishmentCode, x.EmissionPointCode }).IsUnique();
        });
    }
}

public sealed class AccountingSequence
{
    private AccountingSequence() { }
    public AccountingSequence(string tenantId, string sequenceKey, long nextValue)
    {
        Id = Guid.NewGuid();
        TenantId = tenantId;
        SequenceKey = sequenceKey;
        NextValue = nextValue;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }
    public Guid Id { get; private set; }
    public string TenantId { get; private set; } = FinancialTenant.Default;
    public string SequenceKey { get; private set; } = "";
    public long NextValue { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }
    public long TakeNext()
    {
        var value = NextValue;
        NextValue++;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
        return value;
    }
}

public sealed class EfAccountRepository(FinancialDbContext db) : IAccountRepository
{
    public async Task AddAsync(Account account, CancellationToken ct) => await db.Accounts.AddAsync(account, ct);
    public async Task<Account?> GetByIdAsync(Guid id, string tenantId, CancellationToken ct) =>
        await db.Accounts.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId, ct);
    public async Task<Account?> GetByCodeAsync(string code, string tenantId, CancellationToken ct) =>
        await db.Accounts.FirstOrDefaultAsync(x => x.Code == code && x.TenantId == tenantId, ct);
    public async Task<bool> ExistsByCodeAsync(string code, string tenantId, Guid? excludingId, CancellationToken ct) =>
        await db.Accounts.AnyAsync(x => x.Code == code && x.TenantId == tenantId && (!excludingId.HasValue || x.Id != excludingId), ct);
    public async Task<bool> HasChildrenAsync(Guid id, string tenantId, CancellationToken ct) =>
        await db.Accounts.AnyAsync(x => x.ParentAccountId == id && x.TenantId == tenantId && x.Status == AccountStatus.Active, ct);
    public async Task<(IReadOnlyCollection<Account> Items, long Total)> SearchAsync(string tenantId, string? search, AccountType? type, AccountStatus? status, int page, int pageSize, CancellationToken ct)
    {
        var query = db.Accounts.AsNoTracking().Where(x => x.TenantId == tenantId);
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(x => x.Code.Contains(search) || x.Name.Contains(search));
        if (type.HasValue) query = query.Where(x => x.Type == type);
        if (status.HasValue) query = query.Where(x => x.Status == status);
        var total = await query.LongCountAsync(ct);
        var items = await query.OrderBy(x => x.Code).Skip((page - 1) * pageSize).Take(pageSize).ToArrayAsync(ct);
        return (items, total);
    }
    public async Task<IReadOnlyCollection<Account>> GetTreeAccountsAsync(string tenantId, CancellationToken ct) =>
        await db.Accounts.AsNoTracking().Where(x => x.TenantId == tenantId && x.Status != AccountStatus.Archived).OrderBy(x => x.Code).ToArrayAsync(ct);
    public async Task SaveChangesAsync(CancellationToken ct) => await db.SaveChangesAsync(ct);
}

public sealed class EfFiscalRepository(FinancialDbContext db) : IFiscalRepository
{
    public async Task AddYearAsync(FiscalYear year, CancellationToken ct) => await db.FiscalYears.AddAsync(year, ct);
    public async Task AddPeriodAsync(FiscalPeriod period, CancellationToken ct) => await db.FiscalPeriods.AddAsync(period, ct);
    public async Task<FiscalYear?> GetYearByIdAsync(Guid id, string tenantId, CancellationToken ct) => await db.FiscalYears.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId, ct);
    public async Task<FiscalPeriod?> GetPeriodByIdAsync(Guid id, string tenantId, CancellationToken ct) => await db.FiscalPeriods.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId, ct);
    public async Task<bool> YearExistsAsync(int year, string tenantId, Guid? excludingId, CancellationToken ct) =>
        await db.FiscalYears.AnyAsync(x => x.Year == year && x.TenantId == tenantId && (!excludingId.HasValue || x.Id != excludingId), ct);
    public async Task<bool> PeriodNumberExistsAsync(Guid fiscalYearId, string tenantId, int periodNumber, Guid? excludingId, CancellationToken ct) =>
        await db.FiscalPeriods.AnyAsync(x => x.FiscalYearId == fiscalYearId && x.TenantId == tenantId && x.PeriodNumber == periodNumber && (!excludingId.HasValue || x.Id != excludingId), ct);
    public async Task<bool> PeriodOverlapsAsync(Guid fiscalYearId, string tenantId, DateOnly startDate, DateOnly endDate, Guid? excludingId, CancellationToken ct) =>
        await db.FiscalPeriods.AnyAsync(x => x.FiscalYearId == fiscalYearId && x.TenantId == tenantId && x.Status != FiscalPeriodStatus.Archived && (!excludingId.HasValue || x.Id != excludingId) && x.StartDate <= endDate && startDate <= x.EndDate, ct);
    public async Task<bool> HasOpenPeriodsAsync(Guid fiscalYearId, string tenantId, CancellationToken ct) =>
        await db.FiscalPeriods.AnyAsync(x => x.FiscalYearId == fiscalYearId && x.TenantId == tenantId && x.Status == FiscalPeriodStatus.Open, ct);
    public async Task<(IReadOnlyCollection<FiscalYear> Items, long Total)> SearchYearsAsync(string tenantId, int? year, FiscalYearStatus? status, int page, int pageSize, CancellationToken ct)
    {
        var query = db.FiscalYears.AsNoTracking().Where(x => x.TenantId == tenantId);
        if (year.HasValue) query = query.Where(x => x.Year == year);
        if (status.HasValue) query = query.Where(x => x.Status == status);
        var total = await query.LongCountAsync(ct);
        return (await query.OrderByDescending(x => x.Year).Skip((page - 1) * pageSize).Take(pageSize).ToArrayAsync(ct), total);
    }
    public async Task<(IReadOnlyCollection<FiscalPeriod> Items, long Total)> SearchPeriodsAsync(string tenantId, Guid? fiscalYearId, FiscalPeriodStatus? status, int page, int pageSize, CancellationToken ct)
    {
        var query = db.FiscalPeriods.AsNoTracking().Where(x => x.TenantId == tenantId);
        if (fiscalYearId.HasValue) query = query.Where(x => x.FiscalYearId == fiscalYearId);
        if (status.HasValue) query = query.Where(x => x.Status == status);
        var total = await query.LongCountAsync(ct);
        return (await query.OrderBy(x => x.StartDate).ThenBy(x => x.PeriodNumber).Skip((page - 1) * pageSize).Take(pageSize).ToArrayAsync(ct), total);
    }
    public async Task<FiscalPeriod?> GetOpenPeriodByDateAsync(string tenantId, DateOnly date, CancellationToken ct) =>
        await db.FiscalPeriods.AsNoTracking().FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Status == FiscalPeriodStatus.Open && x.StartDate <= date && date <= x.EndDate, ct);
    public async Task SaveChangesAsync(CancellationToken ct) => await db.SaveChangesAsync(ct);
}

public sealed class EfJournalEntryRepository(FinancialDbContext db) : IJournalEntryRepository
{
    public async Task AddAsync(JournalEntry entry, CancellationToken ct) => await db.JournalEntries.AddAsync(entry, ct);
    public async Task AddLineAsync(JournalEntryLine line, CancellationToken ct) => await db.JournalEntryLines.AddAsync(line, ct);
    public async Task<JournalEntry?> GetByIdAsync(Guid id, string tenantId, CancellationToken ct) =>
        await db.JournalEntries.Include(x => x.Lines).FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId, ct);
    public async Task<JournalEntry?> GetByNumberAsync(string entryNumber, string tenantId, CancellationToken ct) =>
        await db.JournalEntries.Include(x => x.Lines).FirstOrDefaultAsync(x => x.EntryNumber == entryNumber && x.TenantId == tenantId, ct);
    public async Task<(IReadOnlyCollection<JournalEntry> Items, long Total)> SearchAsync(string tenantId, JournalEntryStatus? status, DateOnly? from, DateOnly? to, string? search, int page, int pageSize, CancellationToken ct)
    {
        var query = db.JournalEntries.Include(x => x.Lines).AsNoTracking().Where(x => x.TenantId == tenantId);
        if (status.HasValue) query = query.Where(x => x.Status == status);
        if (from.HasValue) query = query.Where(x => x.PostingDate >= from.Value);
        if (to.HasValue) query = query.Where(x => x.PostingDate <= to.Value);
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(x => (x.EntryNumber != null && x.EntryNumber.Contains(search)) || (x.Reference != null && x.Reference.Contains(search)) || x.Description.Contains(search));
        var total = await query.LongCountAsync(ct);
        return (await query.OrderByDescending(x => x.PostingDate).ThenByDescending(x => x.CreatedAtUtc).Skip((page - 1) * pageSize).Take(pageSize).ToArrayAsync(ct), total);
    }
    public async Task<string> GetNextEntryNumberAsync(string tenantId, int year, string prefix, int padding, CancellationToken ct)
    {
        prefix = string.IsNullOrWhiteSpace(prefix) ? "JE" : prefix.Trim().ToUpperInvariant();
        padding = Math.Clamp(padding, 1, 12);
        var key = $"{prefix}-{year}";

        var connection = db.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open) await connection.OpenAsync(ct);
        await using var transaction = await connection.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        await using var ensureCommand = connection.CreateCommand();
        ensureCommand.Transaction = transaction;
        ensureCommand.CommandText = """
IF NOT EXISTS (SELECT 1 FROM financial.accounting_sequences WITH (UPDLOCK, HOLDLOCK) WHERE TenantId = @tenantId AND SequenceKey = @sequenceKey)
BEGIN
    INSERT INTO financial.accounting_sequences (Id, TenantId, SequenceKey, NextValue, UpdatedAtUtc)
    VALUES (NEWID(), @tenantId, @sequenceKey, 1, SYSDATETIMEOFFSET());
END;
""";
        AddParameter(ensureCommand, "@tenantId", tenantId);
        AddParameter(ensureCommand, "@sequenceKey", key);
        await ensureCommand.ExecuteNonQueryAsync(ct);

        await using var takeCommand = connection.CreateCommand();
        takeCommand.Transaction = transaction;
        takeCommand.CommandText = """
DECLARE @value bigint;
SELECT @value = NextValue
FROM financial.accounting_sequences WITH (UPDLOCK, HOLDLOCK)
WHERE TenantId = @tenantId AND SequenceKey = @sequenceKey;

UPDATE financial.accounting_sequences
SET NextValue = NextValue + 1, UpdatedAtUtc = SYSDATETIMEOFFSET()
WHERE TenantId = @tenantId AND SequenceKey = @sequenceKey;

SELECT @value;
""";
        AddParameter(takeCommand, "@tenantId", tenantId);
        AddParameter(takeCommand, "@sequenceKey", key);
        var scalar = await takeCommand.ExecuteScalarAsync(ct);
        await transaction.CommitAsync(ct);
        var value = Convert.ToInt64(scalar, System.Globalization.CultureInfo.InvariantCulture);
        return $"{prefix}-{year}-{value.ToString().PadLeft(padding, '0')}";

        static void AddParameter(System.Data.Common.DbCommand command, string name, object value)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = value;
            command.Parameters.Add(parameter);
        }
    }
    public async Task<bool> HasPostedEntriesForAccountAsync(Guid accountId, string tenantId, CancellationToken ct) =>
        await db.JournalEntryLines.AnyAsync(x => x.AccountId == accountId && x.TenantId == tenantId && db.JournalEntries.Any(e => e.Id == x.JournalEntryId && e.Status == JournalEntryStatus.Posted), ct);
    public async Task<bool> HasDraftEntriesInPeriodAsync(Guid fiscalPeriodId, string tenantId, CancellationToken ct) =>
        await db.JournalEntries.AnyAsync(x => x.FiscalPeriodId == fiscalPeriodId && x.TenantId == tenantId && x.Status == JournalEntryStatus.Draft, ct);
    public async Task<bool> HasDraftEntriesInDateRangeAsync(string tenantId, DateOnly startDate, DateOnly endDate, CancellationToken ct) =>
        await db.JournalEntries.AnyAsync(x => x.TenantId == tenantId && x.Status == JournalEntryStatus.Draft && x.PostingDate >= startDate && x.PostingDate <= endDate, ct);
    public async Task<bool> HasPostedEntriesInPeriodAsync(Guid fiscalPeriodId, string tenantId, CancellationToken ct) =>
        await db.JournalEntries.AnyAsync(x => x.FiscalPeriodId == fiscalPeriodId && x.TenantId == tenantId && (x.Status == JournalEntryStatus.Posted || x.Status == JournalEntryStatus.Reversed), ct);
    public async Task<bool> HasPostedEntriesInFiscalYearAsync(Guid fiscalYearId, string tenantId, CancellationToken ct) =>
        await db.JournalEntries.AnyAsync(entry => entry.TenantId == tenantId && entry.Status != JournalEntryStatus.Draft && entry.Status != JournalEntryStatus.Voided &&
            db.FiscalPeriods.Any(period => period.Id == entry.FiscalPeriodId && period.FiscalYearId == fiscalYearId && period.TenantId == tenantId), ct);
    public async Task SaveChangesAsync(CancellationToken ct) => await db.SaveChangesAsync(ct);
}

public sealed class EfElectronicDocumentRepository(FinancialDbContext db) : IElectronicDocumentRepository
{
    public async Task AddAsync(ElectronicDocument document, CancellationToken ct) => await db.ElectronicDocuments.AddAsync(document, ct);
    public async Task AddLineAsync(ElectronicDocumentLine line, CancellationToken ct) => await db.ElectronicDocumentLines.AddAsync(line, ct);
    public async Task<ElectronicDocument?> GetByIdAsync(Guid id, string tenantId, CancellationToken ct) =>
        await db.ElectronicDocuments.Include(x => x.Lines).Include(x => x.Taxes).FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId, ct);
    public async Task<ElectronicDocument?> GetByAccessKeyAsync(string accessKey, string tenantId, CancellationToken ct) =>
        await db.ElectronicDocuments.Include(x => x.Lines).Include(x => x.Taxes).FirstOrDefaultAsync(x => x.AccessKey == accessKey && x.TenantId == tenantId, ct);
    public async Task<(IReadOnlyCollection<ElectronicDocument> Items, long Total)> SearchAsync(string tenantId, ElectronicDocumentStatus? status, string? accessKey, int page, int pageSize, CancellationToken ct)
    {
        var query = db.ElectronicDocuments.Include(x => x.Lines).AsNoTracking().Where(x => x.TenantId == tenantId);
        if (status.HasValue) query = query.Where(x => x.Status == status);
        if (!string.IsNullOrWhiteSpace(accessKey)) query = query.Where(x => x.AccessKey == accessKey);
        var total = await query.LongCountAsync(ct);
        return (await query.OrderByDescending(x => x.IssueDate).ThenByDescending(x => x.CreatedAtUtc).Skip((page - 1) * pageSize).Take(pageSize).ToArrayAsync(ct), total);
    }
    public async Task<string> GetNextSequentialAsync(string tenantId, ElectronicDocumentType documentType, SriEnvironment environment, string establishmentCode, string emissionPointCode, CancellationToken ct)
    {
        var connection = db.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open) await connection.OpenAsync(ct);
        await using var transaction = await connection.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        var documentTypeName = documentType.ToString();
        var environmentName = environment.ToString();

        await using var ensureCommand = connection.CreateCommand();
        ensureCommand.Transaction = transaction;
        ensureCommand.CommandText = """
IF NOT EXISTS (
    SELECT 1 FROM financial.sri_document_sequences WITH (UPDLOCK, HOLDLOCK)
    WHERE TenantId = @tenantId AND DocumentType = @documentType AND Environment = @environment AND EstablishmentCode = @establishmentCode AND EmissionPointCode = @emissionPointCode)
BEGIN
    INSERT INTO financial.sri_document_sequences (Id, TenantId, DocumentType, EstablishmentCode, EmissionPointCode, CurrentValue, Environment, IsActive, CreatedAtUtc, UpdatedAtUtc)
    VALUES (NEWID(), @tenantId, @documentType, @establishmentCode, @emissionPointCode, 0, @environment, 1, SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET());
END;
""";
        AddParameter(ensureCommand, "@tenantId", tenantId);
        AddParameter(ensureCommand, "@documentType", documentTypeName);
        AddParameter(ensureCommand, "@environment", environmentName);
        AddParameter(ensureCommand, "@establishmentCode", establishmentCode);
        AddParameter(ensureCommand, "@emissionPointCode", emissionPointCode);
        await ensureCommand.ExecuteNonQueryAsync(ct);

        await using var takeCommand = connection.CreateCommand();
        takeCommand.Transaction = transaction;
        takeCommand.CommandText = """
DECLARE @value bigint;
SELECT @value = CurrentValue + 1
FROM financial.sri_document_sequences WITH (UPDLOCK, HOLDLOCK)
WHERE TenantId = @tenantId AND DocumentType = @documentType AND Environment = @environment AND EstablishmentCode = @establishmentCode AND EmissionPointCode = @emissionPointCode;

UPDATE financial.sri_document_sequences
SET CurrentValue = @value, UpdatedAtUtc = SYSDATETIMEOFFSET()
WHERE TenantId = @tenantId AND DocumentType = @documentType AND Environment = @environment AND EstablishmentCode = @establishmentCode AND EmissionPointCode = @emissionPointCode;

SELECT @value;
""";
        AddParameter(takeCommand, "@tenantId", tenantId);
        AddParameter(takeCommand, "@documentType", documentTypeName);
        AddParameter(takeCommand, "@environment", environmentName);
        AddParameter(takeCommand, "@establishmentCode", establishmentCode);
        AddParameter(takeCommand, "@emissionPointCode", emissionPointCode);
        var scalar = await takeCommand.ExecuteScalarAsync(ct);
        await transaction.CommitAsync(ct);
        var value = Convert.ToInt64(scalar, System.Globalization.CultureInfo.InvariantCulture);
        return value.ToString("000000000", System.Globalization.CultureInfo.InvariantCulture);

        static void AddParameter(System.Data.Common.DbCommand command, string name, object value)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = value;
            command.Parameters.Add(parameter);
        }
    }
    public async Task<bool> SequenceDocumentExistsAsync(string tenantId, ElectronicDocumentType documentType, SriEnvironment environment, string establishmentCode, string emissionPointCode, string sequential, Guid? excludingId, CancellationToken ct) =>
        await db.ElectronicDocuments.AnyAsync(x => x.TenantId == tenantId && x.DocumentType == documentType && x.Environment == environment && x.EstablishmentCode == establishmentCode && x.EmissionPointCode == emissionPointCode && x.Sequential == sequential && (!excludingId.HasValue || x.Id != excludingId), ct);
    public async Task SaveChangesAsync(CancellationToken ct) => await db.SaveChangesAsync(ct);
}

public sealed class FinancialSqlHealthCheck(FinancialDbContext db) : IHealthCheck
{
    private static readonly string[] CoreTables =
    [
        "financial.accounts",
        "financial.fiscal_years",
        "financial.fiscal_periods",
        "financial.journal_entries",
        "financial.journal_entry_lines",
        "financial.accounting_sequences",
        "financial.electronic_documents",
        "financial.electronic_document_lines",
        "financial.electronic_document_taxes",
        "financial.sri_document_sequences",
        "financial.sri_catalog_items"
    ];

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken ct = default)
    {
        if (!await db.Database.CanConnectAsync(ct)) return HealthCheckResult.Unhealthy("Financial SQL is unavailable.");
        foreach (var table in CoreTables)
        {
            var exists = await db.Database.SqlQueryRaw<int>("SELECT CASE WHEN OBJECT_ID({0}, 'U') IS NULL THEN 0 ELSE 1 END AS Value", table).SingleAsync(ct);
            if (exists == 0) return HealthCheckResult.Unhealthy($"Financial SQL table is missing: {table}");
        }
        return HealthCheckResult.Healthy("Financial SQL core schema is ready.");
    }
}

public sealed class DevelopmentPortalAdapters(IOptions<PortalOptions> options, ILogger<DevelopmentPortalAdapters> logger, IConfiguration configuration) :
    IPortalAuditClient, IPortalNotificationClient, IPortalOutboxClient, IPortalConfigurationClient,
    IPortalSecurityClient, IPortalMenuRegistrationClient
{
    public string? LastCorrelationId { get; private set; }
    private void Trace(string capability, PortalCallContext context)
    {
        LastCorrelationId = context.CorrelationId;
        logger.LogInformation("Portal adapter {Capability} is in development mode; correlation {CorrelationId}; gateway {Gateway}", capability, context.CorrelationId, options.Value.GatewayBaseUrl);
    }
    public Task RecordAsync(AuditRecord record, PortalCallContext context, CancellationToken ct) { Trace("Audit", context); return Task.CompletedTask; }
    public Task RequestAsync(NotificationRequest request, PortalCallContext context, CancellationToken ct) { Trace("Notification", context); return Task.CompletedTask; }
    public Task EnqueueAsync(OutboxEnvelope message, PortalCallContext context, CancellationToken ct) { Trace("Outbox", context); return Task.CompletedTask; }
    public Task<string?> GetAsync(string key, PortalCallContext context, CancellationToken ct)
    {
        Trace("Configuration", context);
        return Task.FromResult(configuration[key] ?? configuration[key.Replace('.', ':')]);
    }
    public Task<bool> HasPermissionAsync(string permission, PortalCallContext context, CancellationToken ct) { Trace("Security", context); return Task.FromResult(false); }
    public Task RegisterModuleAsync(string moduleCode, PortalCallContext context, CancellationToken ct) { Trace("Menu", context); return Task.CompletedTask; }
}

public static class FinancialInfrastructureExtensions
{
    public static IServiceCollection AddFinancialInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<PortalOptions>(configuration.GetSection(PortalOptions.SectionName));
        services.Configure<FinancialPlatformOptions>(configuration.GetSection(FinancialPlatformOptions.SectionName));
        services.AddSingleton<DevelopmentPortalAdapters>();
        services.AddSingleton<IPortalAuditClient>(x => x.GetRequiredService<DevelopmentPortalAdapters>());
        services.AddSingleton<IPortalNotificationClient>(x => x.GetRequiredService<DevelopmentPortalAdapters>());
        services.AddSingleton<IPortalOutboxClient>(x => x.GetRequiredService<DevelopmentPortalAdapters>());
        services.AddSingleton<IPortalConfigurationClient>(x => x.GetRequiredService<DevelopmentPortalAdapters>());
        services.AddSingleton<IPortalSecurityClient>(x => x.GetRequiredService<DevelopmentPortalAdapters>());
        services.AddSingleton<IPortalMenuRegistrationClient>(x => x.GetRequiredService<DevelopmentPortalAdapters>());
        services.AddScoped<IFinancialConfigurationReader, FinancialConfigurationReader>();
        services.AddScoped<ChartOfAccountsService>();
        services.AddScoped<FiscalPeriodsService>();
        services.AddScoped<JournalEntriesService>();
        services.AddScoped<ElectronicDocumentsService>();
        services.AddScoped<IElectronicDocumentXmlGenerator, ElectronicInvoiceXmlGenerator>();
        services.AddScoped<IElectronicSignatureService, DevelopmentElectronicSignatureService>();
        services.AddScoped<ICertificateProvider, KeyVaultCertificateProviderPlaceholder>();
        services.AddScoped<ISriReceptionClient, DevelopmentSriReceptionClient>();
        services.AddScoped<ISriAuthorizationClient, DevelopmentSriAuthorizationClient>();
        services.AddScoped<IElectronicDocumentXmlValidator, ElectronicDocumentXmlValidator>();
        services.AddScoped<IXsdSchemaValidator, XsdSchemaValidatorPlaceholder>();
        services.AddScoped<IElectronicDocumentStorageClient, ConfiguredElectronicDocumentStorageClient>();
        services.AddScoped<IRidePdfGenerator, DevelopmentRidePdfGenerator>();

        var connectionString = configuration.GetConnectionString("FinancialDb");
        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            services.AddDbContext<FinancialDbContext>(x => x.UseSqlServer(connectionString));
            services.AddScoped<IAccountRepository, EfAccountRepository>();
            services.AddScoped<IFiscalRepository, EfFiscalRepository>();
            services.AddScoped<IJournalEntryRepository, EfJournalEntryRepository>();
            services.AddScoped<IElectronicDocumentRepository, EfElectronicDocumentRepository>();
            services.AddHealthChecks().AddCheck<FinancialSqlHealthCheck>("financial-sql", tags: ["ready"]);
            if (configuration.GetValue<bool>("Database:Initialize") || configuration.GetValue<bool>("Database:RunMigrations")) services.AddHostedService<FinancialDatabaseInitializer>();
        }
        else
        {
            services.AddScoped<IAccountRepository, UnconfiguredAccountRepository>();
            services.AddScoped<IFiscalRepository, UnconfiguredFiscalRepository>();
            services.AddScoped<IJournalEntryRepository, UnconfiguredJournalEntryRepository>();
            services.AddScoped<IElectronicDocumentRepository, UnconfiguredElectronicDocumentRepository>();
        }
        return services;
    }
}

internal sealed class UnconfiguredAccountRepository : IAccountRepository
{
    private static InvalidOperationException Error => new("FinancialDb connection string is not configured.");
    public Task AddAsync(Account account, CancellationToken ct) => Task.FromException(Error);
    public Task<Account?> GetByIdAsync(Guid id, string tenantId, CancellationToken ct) => Task.FromException<Account?>(Error);
    public Task<Account?> GetByCodeAsync(string code, string tenantId, CancellationToken ct) => Task.FromException<Account?>(Error);
    public Task<bool> ExistsByCodeAsync(string code, string tenantId, Guid? excludingId, CancellationToken ct) => Task.FromException<bool>(Error);
    public Task<bool> HasChildrenAsync(Guid id, string tenantId, CancellationToken ct) => Task.FromException<bool>(Error);
    public Task<(IReadOnlyCollection<Account> Items, long Total)> SearchAsync(string tenantId, string? search, AccountType? type, AccountStatus? status, int page, int pageSize, CancellationToken ct) =>
        Task.FromException<(IReadOnlyCollection<Account>, long)>(Error);
    public Task<IReadOnlyCollection<Account>> GetTreeAccountsAsync(string tenantId, CancellationToken ct) => Task.FromException<IReadOnlyCollection<Account>>(Error);
    public Task SaveChangesAsync(CancellationToken ct) => Task.FromException(Error);
}

internal sealed class UnconfiguredFiscalRepository : IFiscalRepository
{
    private static InvalidOperationException Error => new("FinancialDb connection string is not configured.");
    public Task AddYearAsync(FiscalYear year, CancellationToken ct) => Task.FromException(Error);
    public Task AddPeriodAsync(FiscalPeriod period, CancellationToken ct) => Task.FromException(Error);
    public Task<FiscalYear?> GetYearByIdAsync(Guid id, string tenantId, CancellationToken ct) => Task.FromException<FiscalYear?>(Error);
    public Task<FiscalPeriod?> GetPeriodByIdAsync(Guid id, string tenantId, CancellationToken ct) => Task.FromException<FiscalPeriod?>(Error);
    public Task<bool> YearExistsAsync(int year, string tenantId, Guid? excludingId, CancellationToken ct) => Task.FromException<bool>(Error);
    public Task<bool> PeriodNumberExistsAsync(Guid fiscalYearId, string tenantId, int periodNumber, Guid? excludingId, CancellationToken ct) => Task.FromException<bool>(Error);
    public Task<bool> PeriodOverlapsAsync(Guid fiscalYearId, string tenantId, DateOnly startDate, DateOnly endDate, Guid? excludingId, CancellationToken ct) => Task.FromException<bool>(Error);
    public Task<bool> HasOpenPeriodsAsync(Guid fiscalYearId, string tenantId, CancellationToken ct) => Task.FromException<bool>(Error);
    public Task<(IReadOnlyCollection<FiscalYear> Items, long Total)> SearchYearsAsync(string tenantId, int? year, FiscalYearStatus? status, int page, int pageSize, CancellationToken ct) => Task.FromException<(IReadOnlyCollection<FiscalYear>, long)>(Error);
    public Task<(IReadOnlyCollection<FiscalPeriod> Items, long Total)> SearchPeriodsAsync(string tenantId, Guid? fiscalYearId, FiscalPeriodStatus? status, int page, int pageSize, CancellationToken ct) => Task.FromException<(IReadOnlyCollection<FiscalPeriod>, long)>(Error);
    public Task<FiscalPeriod?> GetOpenPeriodByDateAsync(string tenantId, DateOnly date, CancellationToken ct) => Task.FromException<FiscalPeriod?>(Error);
    public Task SaveChangesAsync(CancellationToken ct) => Task.FromException(Error);
}

internal sealed class UnconfiguredJournalEntryRepository : IJournalEntryRepository
{
    private static InvalidOperationException Error => new("FinancialDb connection string is not configured.");
    public Task AddAsync(JournalEntry entry, CancellationToken ct) => Task.FromException(Error);
    public Task AddLineAsync(JournalEntryLine line, CancellationToken ct) => Task.FromException(Error);
    public Task<JournalEntry?> GetByIdAsync(Guid id, string tenantId, CancellationToken ct) => Task.FromException<JournalEntry?>(Error);
    public Task<JournalEntry?> GetByNumberAsync(string entryNumber, string tenantId, CancellationToken ct) => Task.FromException<JournalEntry?>(Error);
    public Task<(IReadOnlyCollection<JournalEntry> Items, long Total)> SearchAsync(string tenantId, JournalEntryStatus? status, DateOnly? from, DateOnly? to, string? search, int page, int pageSize, CancellationToken ct) => Task.FromException<(IReadOnlyCollection<JournalEntry>, long)>(Error);
    public Task<string> GetNextEntryNumberAsync(string tenantId, int year, string prefix, int padding, CancellationToken ct) => Task.FromException<string>(Error);
    public Task<bool> HasPostedEntriesForAccountAsync(Guid accountId, string tenantId, CancellationToken ct) => Task.FromException<bool>(Error);
    public Task<bool> HasDraftEntriesInPeriodAsync(Guid fiscalPeriodId, string tenantId, CancellationToken ct) => Task.FromException<bool>(Error);
    public Task<bool> HasDraftEntriesInDateRangeAsync(string tenantId, DateOnly startDate, DateOnly endDate, CancellationToken ct) => Task.FromException<bool>(Error);
    public Task<bool> HasPostedEntriesInPeriodAsync(Guid fiscalPeriodId, string tenantId, CancellationToken ct) => Task.FromException<bool>(Error);
    public Task<bool> HasPostedEntriesInFiscalYearAsync(Guid fiscalYearId, string tenantId, CancellationToken ct) => Task.FromException<bool>(Error);
    public Task SaveChangesAsync(CancellationToken ct) => Task.FromException(Error);
}

internal sealed class UnconfiguredElectronicDocumentRepository : IElectronicDocumentRepository
{
    private static InvalidOperationException Error => new("FinancialDb connection string is not configured.");
    public Task AddAsync(ElectronicDocument document, CancellationToken ct) => Task.FromException(Error);
    public Task AddLineAsync(ElectronicDocumentLine line, CancellationToken ct) => Task.FromException(Error);
    public Task<ElectronicDocument?> GetByIdAsync(Guid id, string tenantId, CancellationToken ct) => Task.FromException<ElectronicDocument?>(Error);
    public Task<ElectronicDocument?> GetByAccessKeyAsync(string accessKey, string tenantId, CancellationToken ct) => Task.FromException<ElectronicDocument?>(Error);
    public Task<(IReadOnlyCollection<ElectronicDocument> Items, long Total)> SearchAsync(string tenantId, ElectronicDocumentStatus? status, string? accessKey, int page, int pageSize, CancellationToken ct) => Task.FromException<(IReadOnlyCollection<ElectronicDocument>, long)>(Error);
    public Task<string> GetNextSequentialAsync(string tenantId, ElectronicDocumentType documentType, SriEnvironment environment, string establishmentCode, string emissionPointCode, CancellationToken ct) => Task.FromException<string>(Error);
    public Task<bool> SequenceDocumentExistsAsync(string tenantId, ElectronicDocumentType documentType, SriEnvironment environment, string establishmentCode, string emissionPointCode, string sequential, Guid? excludingId, CancellationToken ct) => Task.FromException<bool>(Error);
    public Task SaveChangesAsync(CancellationToken ct) => Task.FromException(Error);
}

internal sealed class FinancialDatabaseInitializer(IServiceProvider services) : IHostedService
{
    public async Task StartAsync(CancellationToken ct)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<FinancialDbContext>();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        if (configuration.GetValue<bool>("Database:Initialize"))
        {
            await db.Database.EnsureCreatedAsync(ct);
            await db.Database.ExecuteSqlRawAsync("""
IF SCHEMA_ID('financial') IS NULL EXEC('CREATE SCHEMA financial');
IF OBJECT_ID('financial.accounts', 'U') IS NULL
BEGIN
    CREATE TABLE financial.accounts (
        Id uniqueidentifier NOT NULL PRIMARY KEY,
        TenantId nvarchar(64) NOT NULL,
        Code nvarchar(64) NOT NULL,
        Name nvarchar(256) NOT NULL,
        Type nvarchar(32) NOT NULL,
        Level int NOT NULL,
        ParentAccountId uniqueidentifier NULL,
        IsMovementAccount bit NOT NULL,
        Status nvarchar(32) NOT NULL,
        CreatedAtUtc datetimeoffset NOT NULL,
        UpdatedAtUtc datetimeoffset NOT NULL
    );
    CREATE UNIQUE INDEX IX_accounts_TenantId_Code ON financial.accounts(TenantId, Code);
    CREATE INDEX IX_accounts_TenantId_ParentAccountId ON financial.accounts(TenantId, ParentAccountId);
END;
IF OBJECT_ID('financial.fiscal_years', 'U') IS NULL
BEGIN
    CREATE TABLE financial.fiscal_years (
        Id uniqueidentifier NOT NULL PRIMARY KEY,
        TenantId nvarchar(64) NOT NULL,
        Year int NOT NULL,
        StartDate date NOT NULL,
        EndDate date NOT NULL,
        Status nvarchar(32) NOT NULL,
        CreatedAtUtc datetimeoffset NOT NULL,
        UpdatedAtUtc datetimeoffset NOT NULL
    );
    CREATE UNIQUE INDEX IX_fiscal_years_TenantId_Year ON financial.fiscal_years(TenantId, Year);
END;
IF OBJECT_ID('financial.fiscal_periods', 'U') IS NULL
BEGIN
    CREATE TABLE financial.fiscal_periods (
        Id uniqueidentifier NOT NULL PRIMARY KEY,
        TenantId nvarchar(64) NOT NULL,
        FiscalYearId uniqueidentifier NOT NULL,
        PeriodNumber int NOT NULL,
        StartDate date NOT NULL,
        EndDate date NOT NULL,
        Status nvarchar(32) NOT NULL,
        CreatedAtUtc datetimeoffset NOT NULL,
        UpdatedAtUtc datetimeoffset NOT NULL
    );
    CREATE UNIQUE INDEX IX_fiscal_periods_TenantId_FiscalYearId_PeriodNumber ON financial.fiscal_periods(TenantId, FiscalYearId, PeriodNumber);
    CREATE INDEX IX_fiscal_periods_TenantId_FiscalYearId_StartDate_EndDate ON financial.fiscal_periods(TenantId, FiscalYearId, StartDate, EndDate);
END;
IF OBJECT_ID('financial.journal_entries', 'U') IS NULL
BEGIN
    CREATE TABLE financial.journal_entries (
        Id uniqueidentifier NOT NULL PRIMARY KEY,
        TenantId nvarchar(64) NOT NULL,
        FiscalPeriodId uniqueidentifier NULL,
        EntryNumber nvarchar(64) NULL,
        PostingDate date NOT NULL,
        Reference nvarchar(128) NULL,
        Description nvarchar(512) NOT NULL,
        Status nvarchar(32) NOT NULL,
        Source nvarchar(32) NOT NULL,
        ReversalOfJournalEntryId uniqueidentifier NULL,
        ReversedByJournalEntryId uniqueidentifier NULL,
        PostedAtUtc datetimeoffset NULL,
        ReversedAtUtc datetimeoffset NULL,
        VoidedAtUtc datetimeoffset NULL,
        Reason nvarchar(512) NULL,
        CreatedAtUtc datetimeoffset NOT NULL,
        UpdatedAtUtc datetimeoffset NOT NULL
    );
    CREATE UNIQUE INDEX IX_journal_entries_TenantId_EntryNumber ON financial.journal_entries(TenantId, EntryNumber) WHERE EntryNumber IS NOT NULL;
    CREATE INDEX IX_journal_entries_TenantId_PostingDate_Status ON financial.journal_entries(TenantId, PostingDate, Status);
END;
IF OBJECT_ID('financial.journal_entry_lines', 'U') IS NULL
BEGIN
    CREATE TABLE financial.journal_entry_lines (
        Id uniqueidentifier NOT NULL PRIMARY KEY,
        JournalEntryId uniqueidentifier NOT NULL,
        TenantId nvarchar(64) NOT NULL,
        AccountId uniqueidentifier NOT NULL,
        LineNumber int NOT NULL,
        Description nvarchar(512) NULL,
        Debit decimal(19,4) NOT NULL,
        Credit decimal(19,4) NOT NULL,
        CreatedAtUtc datetimeoffset NOT NULL,
        UpdatedAtUtc datetimeoffset NOT NULL,
        CONSTRAINT FK_journal_entry_lines_journal_entries FOREIGN KEY (JournalEntryId) REFERENCES financial.journal_entries(Id) ON DELETE CASCADE
    );
    CREATE UNIQUE INDEX IX_journal_entry_lines_TenantId_JournalEntryId_LineNumber ON financial.journal_entry_lines(TenantId, JournalEntryId, LineNumber);
    CREATE INDEX IX_journal_entry_lines_TenantId_AccountId ON financial.journal_entry_lines(TenantId, AccountId);
END;
IF OBJECT_ID('financial.accounting_sequences', 'U') IS NULL
BEGIN
    CREATE TABLE financial.accounting_sequences (
        Id uniqueidentifier NOT NULL PRIMARY KEY,
        TenantId nvarchar(64) NOT NULL,
        SequenceKey nvarchar(64) NOT NULL,
        NextValue bigint NOT NULL,
        UpdatedAtUtc datetimeoffset NOT NULL
    );
    CREATE UNIQUE INDEX IX_accounting_sequences_TenantId_SequenceKey ON financial.accounting_sequences(TenantId, SequenceKey);
END;
""", ct);
        }

        if (configuration.GetValue<bool>("Database:RunMigrations"))
        {
            await FinancialDatabaseMigrationRunner.RunAsync(db, AppContext.BaseDirectory, ct);
        }
    }
    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
}

internal static class FinancialDatabaseMigrationRunner
{
    public static async Task RunAsync(FinancialDbContext db, string baseDirectory, CancellationToken ct)
    {
        var migrationsDirectory = Path.Combine(baseDirectory, "database", "migrations", "financial");
        if (!Directory.Exists(migrationsDirectory)) return;

        await db.Database.ExecuteSqlRawAsync("""
IF SCHEMA_ID('financial') IS NULL EXEC('CREATE SCHEMA financial');
IF OBJECT_ID('financial.schema_versions', 'U') IS NULL
BEGIN
    CREATE TABLE financial.schema_versions (
        Version nvarchar(32) NOT NULL PRIMARY KEY,
        ScriptName nvarchar(256) NOT NULL,
        AppliedAtUtc datetimeoffset NOT NULL
    );
END;
""", ct);

        foreach (var scriptPath in Directory.GetFiles(migrationsDirectory, "*.sql").OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
        {
            var scriptName = Path.GetFileName(scriptPath);
            var version = scriptName.Split('_', 2)[0];
            var alreadyApplied = await db.Database.SqlQueryRaw<int>("SELECT COUNT(1) AS Value FROM financial.schema_versions WHERE Version = {0}", version).SingleAsync(ct);
            if (alreadyApplied > 0) continue;

            var sql = await File.ReadAllTextAsync(scriptPath, ct);
            await using var transaction = await db.Database.BeginTransactionAsync(ct);
            try
            {
                await db.Database.ExecuteSqlRawAsync(sql, ct);
                await db.Database.ExecuteSqlRawAsync("INSERT INTO financial.schema_versions (Version, ScriptName, AppliedAtUtc) VALUES ({0}, {1}, SYSDATETIMEOFFSET())", version, scriptName);
                await transaction.CommitAsync(ct);
            }
            catch (SqlException ex) when (ex.Number is 2601 or 2627)
            {
                await transaction.RollbackAsync(ct);
                throw new InvalidOperationException($"Financial migration '{scriptName}' hit a duplicate key constraint. Review idempotency and schema state.", ex);
            }
        }
    }
}
