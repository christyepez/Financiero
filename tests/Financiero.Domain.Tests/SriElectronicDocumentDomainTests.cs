using Financiero.Domain;
using Xunit;

namespace Financiero.Domain.Tests;

public sealed class SriElectronicDocumentDomainTests
{
    [Fact]
    public void Creates_valid_invoice_draft()
    {
        var invoice = NewInvoice();
        Assert.Equal(ElectronicDocumentStatus.Draft, invoice.Status);
        Assert.Equal(ElectronicDocumentType.Invoice, invoice.DocumentType);
    }

    [Fact]
    public void Rejects_invoice_without_customer()
    {
        Assert.Throws<FinancialDomainException>(() => ElectronicDocument.CreateInvoice("default", SriEnvironment.Test, SriEmissionType.Normal, "001", "001", new DateOnly(2026, 1, 1), "04", "0999999999001", "", "USD", DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Rejects_line_with_zero_quantity_or_negative_price()
    {
        var invoice = NewInvoice();
        Assert.Throws<FinancialDomainException>(() => invoice.AddLine("SKU", "Item", 0, 1, 0, DateTimeOffset.UtcNow));
        Assert.Throws<FinancialDomainException>(() => invoice.AddLine("SKU", "Item", 1, -1, 0, DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Does_not_allow_send_without_sign_or_sign_without_generated()
    {
        var invoice = NewInvoice();
        Assert.Throws<FinancialDomainException>(() => invoice.EnsureCanSend());
        Assert.Throws<FinancialDomainException>(() => invoice.EnsureCanSign());
    }

    [Fact]
    public void Does_not_allow_modifying_authorized_document()
    {
        var invoice = NewInvoice();
        invoice.AddLine("SKU", "Item", 1, 10, 0, DateTimeOffset.UtcNow);
        var key = SriAccessKeyGenerator.Generate(new(new DateOnly(2026, 1, 1), "01", "0999999999001", SriEnvironment.Test, "001", "001", "000000001", "12345678", SriEmissionType.Normal));
        invoice.Generate("000000001", key, "<factura />", DateTimeOffset.UtcNow);
        invoice.MarkSigned("<factura><firmaSimulada /></factura>", DateTimeOffset.UtcNow);
        invoice.MarkSent("DEV-RECEIVED", "ok", DateTimeOffset.UtcNow);
        invoice.MarkAuthorized(key.Value, DateTimeOffset.UtcNow, "DEV-AUTHORIZED", "ok", DateTimeOffset.UtcNow);

        Assert.Throws<FinancialDomainException>(() => invoice.AddLine("SKU2", "Item 2", 1, 10, 0, DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Access_key_has_49_digits_and_modulo_11_check_digit()
    {
        var key = SriAccessKeyGenerator.Generate(new(new DateOnly(2026, 1, 1), "01", "0999999999001", SriEnvironment.Test, "001", "001", "000000001", "12345678", SriEmissionType.Normal));
        Assert.Equal(49, key.Value.Length);
        Assert.True(key.Value.All(char.IsDigit));
        Assert.Equal(SriAccessKeyGenerator.CalculateModulo11CheckDigit(key.Value[..48]).ToString(), key.Value[^1].ToString());
    }

    [Theory]
    [InlineData("bad", "001", "000000001")]
    [InlineData("0999999999001", "01", "000000001")]
    [InlineData("0999999999001", "001", "1")]
    public void Access_key_rejects_invalid_inputs(string ruc, string establishment, string sequential)
    {
        Assert.Throws<FinancialDomainException>(() => SriAccessKeyGenerator.Generate(new(new DateOnly(2026, 1, 1), "01", ruc, SriEnvironment.Test, establishment, "001", sequential, "12345678", SriEmissionType.Normal)));
    }

    private static ElectronicDocument NewInvoice() =>
        ElectronicDocument.CreateInvoice("default", SriEnvironment.Test, SriEmissionType.Normal, "001", "001", new DateOnly(2026, 1, 1), "04", "0999999999001", "Cliente", "USD", DateTimeOffset.UtcNow);
}
