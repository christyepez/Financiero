using Financiero.Domain;
using Xunit;

namespace Financiero.Domain.Tests;

public sealed class JournalEntryDomainTests
{
    private static readonly Guid DebitAccount = Guid.NewGuid();
    private static readonly Guid CreditAccount = Guid.NewGuid();

    [Fact]
    public void Creates_valid_draft_with_lines()
    {
        var entry = Draft();
        entry.AddLine(DebitAccount, "Debe", 10, 0, DateTimeOffset.UtcNow);
        entry.AddLine(CreditAccount, "Haber", 0, 10, DateTimeOffset.UtcNow);
        Assert.Equal(JournalEntryStatus.Draft, entry.Status);
        Assert.Equal(10, entry.TotalDebit);
        Assert.Equal(10, entry.TotalCredit);
    }

    [Fact] public void Rejects_line_with_debit_and_credit() => Assert.Throws<FinancialDomainException>(() => Draft().AddLine(DebitAccount, null, 1, 1, DateTimeOffset.UtcNow));
    [Fact] public void Rejects_line_without_amount() => Assert.Throws<FinancialDomainException>(() => Draft().AddLine(DebitAccount, null, 0, 0, DateTimeOffset.UtcNow));
    [Fact] public void Rejects_negative_amount() => Assert.Throws<FinancialDomainException>(() => Draft().AddLine(DebitAccount, null, -1, 0, DateTimeOffset.UtcNow));

    [Fact]
    public void Rejects_post_with_less_than_two_lines()
    {
        var entry = Draft();
        entry.AddLine(DebitAccount, null, 10, 0, DateTimeOffset.UtcNow);
        Assert.Throws<FinancialDomainException>(() => entry.Post("JE-2026-000001", Guid.NewGuid(), DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Rejects_unbalanced_post()
    {
        var entry = Draft();
        entry.AddLine(DebitAccount, null, 10, 0, DateTimeOffset.UtcNow);
        entry.AddLine(CreditAccount, null, 0, 9, DateTimeOffset.UtcNow);
        Assert.Throws<FinancialDomainException>(() => entry.Post("JE-2026-000001", Guid.NewGuid(), DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Posts_balanced_entry_and_blocks_edit()
    {
        var entry = BalancedDraft();
        entry.Post("JE-2026-000001", Guid.NewGuid(), DateTimeOffset.UtcNow);
        Assert.Equal(JournalEntryStatus.Posted, entry.Status);
        Assert.Throws<FinancialDomainException>(() => entry.Update(new DateOnly(2026, 1, 2), JournalEntrySource.Manual, null, "Edit", DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Reverse_creates_inverse_lines_and_marks_original()
    {
        var entry = BalancedDraft();
        entry.Post("JE-2026-000001", Guid.NewGuid(), DateTimeOffset.UtcNow);
        var reversal = entry.CreateReversalDraft("error", DateTimeOffset.UtcNow);
        reversal.Post("JE-2026-000002", Guid.NewGuid(), DateTimeOffset.UtcNow);
        entry.MarkReversed(reversal.Id, "error", DateTimeOffset.UtcNow);

        Assert.Equal(entry.Id, reversal.ReversalOfJournalEntryId);
        Assert.Equal(reversal.Id, entry.ReversedByJournalEntryId);
        Assert.Equal(JournalEntryStatus.Reversed, entry.Status);
        Assert.Equal(10, reversal.TotalDebit);
        Assert.Equal(10, reversal.TotalCredit);
    }

    [Fact]
    public void Void_only_applies_to_draft()
    {
        var draft = BalancedDraft();
        draft.Void("duplicate", DateTimeOffset.UtcNow);
        Assert.Equal(JournalEntryStatus.Voided, draft.Status);

        var posted = BalancedDraft();
        posted.Post("JE-2026-000001", Guid.NewGuid(), DateTimeOffset.UtcNow);
        Assert.Throws<FinancialDomainException>(() => posted.Void("no", DateTimeOffset.UtcNow));
    }

    private static JournalEntry Draft() => JournalEntry.Create("default", new DateOnly(2026, 1, 15), JournalEntrySource.Manual, "REF", "Entry", DateTimeOffset.UtcNow);
    private static JournalEntry BalancedDraft()
    {
        var entry = Draft();
        entry.AddLine(DebitAccount, null, 10, 0, DateTimeOffset.UtcNow);
        entry.AddLine(CreditAccount, null, 0, 10, DateTimeOffset.UtcNow);
        return entry;
    }
}
