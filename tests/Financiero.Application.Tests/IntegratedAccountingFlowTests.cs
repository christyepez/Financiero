using Financiero.Application;
using Financiero.Contracts;
using Financiero.Domain;
using Xunit;

namespace Financiero.Application.Tests;

public sealed class IntegratedAccountingFlowTests
{
    private static PortalCallContext Context => new("default", "corr-integrated-flow");

    [Fact]
    public async Task Runs_minimum_accounting_flow_with_audit_and_outbox()
    {
        var accountsRepo = new InMemoryAccountRepository();
        var fiscalRepo = new InMemoryFiscalRepository();
        var journalRepo = new InMemoryJournalEntryRepository();
        var audit = new RecordingAudit();
        var outbox = new RecordingOutbox();
        var configuration = new StaticFinancialConfigurationReader();
        var accounts = new ChartOfAccountsService(accountsRepo, journalRepo, audit, outbox);
        var fiscal = new FiscalPeriodsService(fiscalRepo, journalRepo, configuration, audit, outbox);
        var journals = new JournalEntriesService(journalRepo, accountsRepo, fiscalRepo, configuration, audit, outbox);

        var parent = await accounts.CreateAsync(new("1", "Activos", "Asset", 1, null, false), Context, default);
        await accounts.ActivateAsync(parent.Id, Context, default);
        var cash = await accounts.CreateAsync(new("1.1", "Caja", "Asset", 2, parent.Id), Context, default);
        await accounts.ActivateAsync(cash.Id, Context, default);
        var bank = await accounts.CreateAsync(new("1.2", "Bancos", "Asset", 2, parent.Id), Context, default);
        await accounts.ActivateAsync(bank.Id, Context, default);

        var year = await fiscal.CreateYearAsync(new(2026, new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31)), Context, default);
        await fiscal.OpenYearAsync(year.Id, Context, default);
        var period = await fiscal.CreatePeriodAsync(new(year.Id, 1, new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31)), Context, default);
        await fiscal.OpenPeriodAsync(period.Id, Context, default);

        var draft = await journals.CreateAsync(new(new DateOnly(2026, 1, 15), "Manual", "OPEN", "Opening entry"), Context, default);
        draft = await journals.AddLineAsync(draft.Id, new(cash.Id, "debit", 100, 0), Context, default);
        draft = await journals.AddLineAsync(draft.Id, new(bank.Id, "credit", 0, 100), Context, default);
        var posted = await journals.PostAsync(draft.Id, Context, default);
        var reversal = await journals.ReverseAsync(posted.Id, new("qa reversal"), Context, default);
        var original = await journals.GetByIdAsync(posted.Id, Context, default);

        Assert.Equal("JE-2026-000001", posted.EntryNumber);
        Assert.Equal(period.Id, posted.FiscalPeriodId);
        Assert.Equal(posted.TotalDebit, posted.TotalCredit);
        Assert.Equal("Posted", reversal.Status);
        Assert.Equal("Reversed", original.Status);
        Assert.Equal(reversal.Id, original.ReversedByJournalEntryId);
        Assert.Contains("JournalEntryPosted", audit.Actions);
        Assert.Contains("JournalEntryReversed", audit.Actions);
        Assert.Contains("JournalEntryPosted", outbox.EventTypes);
        Assert.All(outbox.CorrelationIds, x => Assert.Equal(Context.CorrelationId, x));
    }
}
