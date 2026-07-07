namespace Financiero.Domain;

public enum JournalEntryStatus { Draft, Posted, Reversed, Voided }
public enum JournalEntrySource { Manual, OpeningBalance, Adjustment, Integration, System }

public sealed class JournalEntry
{
    private readonly List<JournalEntryLine> _lines = [];

    private JournalEntry() { }

    private JournalEntry(Guid id, string tenantId, DateOnly postingDate, JournalEntrySource source, string? reference, string description, DateTimeOffset now)
    {
        Id = id;
        TenantId = Required(tenantId, nameof(TenantId));
        PostingDate = postingDate;
        Source = source;
        Reference = Trim(reference);
        Description = Required(description, nameof(Description));
        Status = JournalEntryStatus.Draft;
        CreatedAtUtc = now;
        UpdatedAtUtc = now;
    }

    public Guid Id { get; private set; }
    public string TenantId { get; private set; } = FinancialTenant.Default;
    public Guid? FiscalPeriodId { get; private set; }
    public string? EntryNumber { get; private set; }
    public DateOnly PostingDate { get; private set; }
    public string? Reference { get; private set; }
    public string Description { get; private set; } = "";
    public JournalEntryStatus Status { get; private set; }
    public JournalEntrySource Source { get; private set; }
    public Guid? ReversalOfJournalEntryId { get; private set; }
    public Guid? ReversedByJournalEntryId { get; private set; }
    public DateTimeOffset? PostedAtUtc { get; private set; }
    public DateTimeOffset? ReversedAtUtc { get; private set; }
    public DateTimeOffset? VoidedAtUtc { get; private set; }
    public string? Reason { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }
    public IReadOnlyCollection<JournalEntryLine> Lines => _lines.OrderBy(x => x.LineNumber).ToArray();
    public decimal TotalDebit => FinancialPrecision.Normalize(_lines.Sum(x => x.Debit));
    public decimal TotalCredit => FinancialPrecision.Normalize(_lines.Sum(x => x.Credit));

    public static JournalEntry Create(string tenantId, DateOnly postingDate, JournalEntrySource source, string? reference, string description, DateTimeOffset now) =>
        new(Guid.NewGuid(), tenantId, postingDate, source, reference, description, now);

    public void Update(DateOnly postingDate, JournalEntrySource source, string? reference, string description, DateTimeOffset now)
    {
        EnsureDraft();
        PostingDate = postingDate;
        Source = source;
        Reference = Trim(reference);
        Description = Required(description, nameof(Description));
        UpdatedAtUtc = now;
    }

    public JournalEntryLine AddLine(Guid accountId, string? description, decimal debit, decimal credit, DateTimeOffset now)
    {
        EnsureDraft();
        var line = JournalEntryLine.Create(Guid.NewGuid(), Id, TenantId, accountId, NextLineNumber(), description, debit, credit, now);
        _lines.Add(line);
        UpdatedAtUtc = now;
        return line;
    }

    public JournalEntryLine UpdateLine(Guid lineId, Guid accountId, string? description, decimal debit, decimal credit, DateTimeOffset now)
    {
        EnsureDraft();
        var line = GetLine(lineId);
        line.Update(accountId, description, debit, credit, now);
        UpdatedAtUtc = now;
        return line;
    }

    public void RemoveLine(Guid lineId, DateTimeOffset now)
    {
        EnsureDraft();
        var line = GetLine(lineId);
        _lines.Remove(line);
        RenumberLines();
        UpdatedAtUtc = now;
    }

    public void Post(string entryNumber, Guid fiscalPeriodId, DateTimeOffset now)
    {
        EnsureDraft();
        ValidatePostable();
        EntryNumber = Required(entryNumber, nameof(EntryNumber));
        FiscalPeriodId = fiscalPeriodId;
        Status = JournalEntryStatus.Posted;
        PostedAtUtc = now;
        UpdatedAtUtc = now;
    }

    public void MarkReversed(Guid reversedByJournalEntryId, string reason, DateTimeOffset now)
    {
        if (Status != JournalEntryStatus.Posted) throw new FinancialDomainException("journal_entry.reverse.status", "Only posted journal entries can be reversed.");
        ReversedByJournalEntryId = reversedByJournalEntryId;
        ReversedAtUtc = now;
        Reason = Required(reason, nameof(Reason));
        Status = JournalEntryStatus.Reversed;
        UpdatedAtUtc = now;
    }

    public void Void(string reason, DateTimeOffset now)
    {
        EnsureDraft();
        Reason = Required(reason, nameof(Reason));
        Status = JournalEntryStatus.Voided;
        VoidedAtUtc = now;
        UpdatedAtUtc = now;
    }

    public JournalEntry CreateReversalDraft(string reason, DateTimeOffset now)
    {
        if (Status != JournalEntryStatus.Posted) throw new FinancialDomainException("journal_entry.reverse.status", "Only posted journal entries can be reversed.");
        if (ReversedByJournalEntryId.HasValue) throw new FinancialDomainException("journal_entry.reverse.duplicate", "Journal entry was already reversed.");
        var reversal = Create(TenantId, PostingDate, JournalEntrySource.System, EntryNumber is null ? null : $"REV-{EntryNumber}", $"Reversal: {Description}", now);
        reversal.ReversalOfJournalEntryId = Id;
        reversal.Reason = Required(reason, nameof(Reason));
        foreach (var line in Lines)
        {
            reversal.AddLine(line.AccountId, line.Description, line.Credit, line.Debit, now);
        }
        return reversal;
    }

    public void ValidatePostable()
    {
        if (_lines.Count < 2) throw new FinancialDomainException("journal_entry.lines.minimum", "A journal entry requires at least two lines to post.");
        if (TotalDebit <= 0 || TotalCredit <= 0) throw new FinancialDomainException("journal_entry.total.zero", "Journal entry totals must be greater than zero.");
        if (TotalDebit != TotalCredit) throw new FinancialDomainException("journal_entry.unbalanced", "Journal entry debit and credit totals must match.");
    }

    private JournalEntryLine GetLine(Guid lineId) =>
        _lines.FirstOrDefault(x => x.Id == lineId) ?? throw new FinancialDomainException("journal_entry.line.not_found", "Journal entry line was not found.");

    private int NextLineNumber() => _lines.Count == 0 ? 1 : _lines.Max(x => x.LineNumber) + 1;
    private void RenumberLines()
    {
        var number = 1;
        foreach (var line in _lines.OrderBy(x => x.LineNumber)) line.SetLineNumber(number++);
    }
    private void EnsureDraft()
    {
        if (Status != JournalEntryStatus.Draft) throw new FinancialDomainException("journal_entry.not_editable", "Only draft journal entries can be modified.");
    }
    private static string Required(string value, string name) => string.IsNullOrWhiteSpace(value) ? throw new FinancialDomainException($"{name.ToLowerInvariant()}.required", $"{name} is required.") : value.Trim();
    private static string? Trim(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

public sealed class JournalEntryLine
{
    private JournalEntryLine() { }
    private JournalEntryLine(Guid id, Guid journalEntryId, string tenantId, Guid accountId, int lineNumber, string? description, decimal debit, decimal credit, DateTimeOffset now)
    {
        Id = id;
        JournalEntryId = journalEntryId;
        TenantId = tenantId;
        AccountId = accountId == Guid.Empty ? throw new FinancialDomainException("journal_entry.line.account.required", "AccountId is required.") : accountId;
        LineNumber = lineNumber;
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        Debit = FinancialPrecision.Normalize(debit);
        Credit = FinancialPrecision.Normalize(credit);
        ValidateAmounts(Debit, Credit);
        CreatedAtUtc = now;
        UpdatedAtUtc = now;
    }

    public Guid Id { get; private set; }
    public Guid JournalEntryId { get; private set; }
    public string TenantId { get; private set; } = FinancialTenant.Default;
    public Guid AccountId { get; private set; }
    public int LineNumber { get; private set; }
    public string? Description { get; private set; }
    public decimal Debit { get; private set; }
    public decimal Credit { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public static JournalEntryLine Create(Guid id, Guid journalEntryId, string tenantId, Guid accountId, int lineNumber, string? description, decimal debit, decimal credit, DateTimeOffset now) =>
        new(id, journalEntryId, tenantId, accountId, lineNumber, description, debit, credit, now);

    public void Update(Guid accountId, string? description, decimal debit, decimal credit, DateTimeOffset now)
    {
        AccountId = accountId == Guid.Empty ? throw new FinancialDomainException("journal_entry.line.account.required", "AccountId is required.") : accountId;
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        Debit = FinancialPrecision.Normalize(debit);
        Credit = FinancialPrecision.Normalize(credit);
        ValidateAmounts(Debit, Credit);
        UpdatedAtUtc = now;
    }

    internal void SetLineNumber(int lineNumber) => LineNumber = lineNumber;

    private static void ValidateAmounts(decimal debit, decimal credit)
    {
        if (debit < 0 || credit < 0) throw new FinancialDomainException("journal_entry.line.amount.negative", "Debit and credit cannot be negative.");
        if (debit > 0 && credit > 0) throw new FinancialDomainException("journal_entry.line.amount.both", "A line cannot have both debit and credit.");
        if (debit == 0 && credit == 0) throw new FinancialDomainException("journal_entry.line.amount.empty", "A line requires debit or credit.");
    }
}
