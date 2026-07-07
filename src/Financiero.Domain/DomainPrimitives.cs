namespace Financiero.Domain;

public abstract record DomainEvent(Guid EventId, DateTimeOffset OccurredAtUtc, string CorrelationId);

public static class FinancialPrecision
{
    public const int Precision = 19;
    public const int Scale = 4;
    public static decimal Normalize(decimal value) => decimal.Round(value, Scale, MidpointRounding.ToEven);
}

public static class FinancialTenant
{
    public const string Default = "default";
}
