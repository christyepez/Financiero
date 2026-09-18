using Xunit;

namespace Financiero.Api.Tests;

public sealed class JournalMigrationSafetyTests
{
    [Fact]
    public void JournalEntryLinesForeignKey_UsesNoAction_ForSqlServerCompatibility()
    {
        var path = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..", "..", "..", "..", "..",
            "database", "migrations", "financial", "004_journal_entries.sql"));

        var sql = File.ReadAllText(path);

        Assert.Contains("FK_journal_entry_lines_journal_entries", sql);
        Assert.Contains("REFERENCES financial.journal_entries(Id) ON DELETE NO ACTION", sql);
        Assert.DoesNotContain("REFERENCES financial.journal_entries(Id) ON DELETE CASCADE", sql);
    }
}
