namespace Financiero.Contracts;

public sealed record ErrorContract(string Code, string Message);
public sealed record ApiResponse<T>(T? Data, ErrorContract? Error, string CorrelationId);
public sealed record PageRequest(int Page = 1, int PageSize = 20);
public sealed record PageResponse<T>(IReadOnlyCollection<T> Items, int Page, int PageSize, long Total);
public sealed record CorrelationContext(string CorrelationId, string TenantId);
