using Financiero.Application;
using Financiero.Contracts;
using Financiero.Domain;
using Microsoft.Extensions.Options;

namespace Financiero.Api;

public static class JournalEntriesEndpoints
{
    public static IEndpointRouteBuilder MapJournalEntries(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/financial/journal-entries").WithTags("Journal Entries");

        group.MapPost("/", async (CreateJournalEntryRequest request, JournalEntriesService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
            await ExecuteAsync(() => service.CreateAsync(request, Context(http, options.Value), ct), http));
        group.MapPut("/{id:guid}", async (Guid id, UpdateJournalEntryRequest request, JournalEntriesService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
            await ExecuteAsync(() => service.UpdateAsync(id, request, Context(http, options.Value), ct), http));
        group.MapGet("/", async ([AsParameters] SearchJournalEntriesRequest request, JournalEntriesService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
            await ExecuteAsync(() => service.SearchAsync(request, Context(http, options.Value), ct), http));
        group.MapGet("/{id:guid}", async (Guid id, JournalEntriesService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
            await ExecuteAsync(() => service.GetByIdAsync(id, Context(http, options.Value), ct), http));
        group.MapGet("/by-number/{entryNumber}", async (string entryNumber, JournalEntriesService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
            await ExecuteAsync(() => service.GetByNumberAsync(entryNumber, Context(http, options.Value), ct), http));
        group.MapPost("/{id:guid}/lines", async (Guid id, CreateJournalEntryLineRequest request, JournalEntriesService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
            await ExecuteAsync(() => service.AddLineAsync(id, request, Context(http, options.Value), ct), http));
        group.MapPut("/{id:guid}/lines/{lineId:guid}", async (Guid id, Guid lineId, UpdateJournalEntryLineRequest request, JournalEntriesService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
            await ExecuteAsync(() => service.UpdateLineAsync(id, lineId, request, Context(http, options.Value), ct), http));
        group.MapDelete("/{id:guid}/lines/{lineId:guid}", async (Guid id, Guid lineId, JournalEntriesService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
            await ExecuteAsync(() => service.RemoveLineAsync(id, lineId, Context(http, options.Value), ct), http));
        group.MapPost("/{id:guid}/post", async (Guid id, JournalEntriesService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
            await ExecuteAsync(() => service.PostAsync(id, Context(http, options.Value), ct), http));
        group.MapPost("/{id:guid}/reverse", async (Guid id, ReverseJournalEntryRequest request, JournalEntriesService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
            await ExecuteAsync(() => service.ReverseAsync(id, request, Context(http, options.Value), ct), http));
        group.MapPost("/{id:guid}/void", async (Guid id, VoidJournalEntryRequest request, JournalEntriesService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
            await ExecuteAsync(() => service.VoidAsync(id, request, Context(http, options.Value), ct), http));

        return app;
    }

    private static PortalCallContext Context(HttpContext http, FinancialPlatformOptions options)
    {
        var correlationId = http.Response.Headers.TryGetValue(options.CorrelationHeader, out var responseCorrelation) ? responseCorrelation.ToString() : http.TraceIdentifier;
        var bearer = http.Request.Headers.Authorization.ToString();
        return new PortalCallContext(options.TenantId, correlationId, bearer.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) ? bearer["Bearer ".Length..] : null);
    }

    private static async Task<IResult> ExecuteAsync<T>(Func<Task<T>> action, HttpContext http)
    {
        try { return Results.Ok(new ApiResponse<T>(await action(), null, http.TraceIdentifier)); }
        catch (FinancialDomainException ex) { return Results.BadRequest(new ApiResponse<object>(null, new ErrorContract(ex.Code, ex.Message), http.TraceIdentifier)); }
        catch (FinancialApplicationException ex) when (ex.Code.EndsWith("not_found", StringComparison.OrdinalIgnoreCase)) { return Results.NotFound(new ApiResponse<object>(null, new ErrorContract(ex.Code, ex.Message), http.TraceIdentifier)); }
        catch (FinancialApplicationException ex) { return Results.BadRequest(new ApiResponse<object>(null, new ErrorContract(ex.Code, ex.Message), http.TraceIdentifier)); }
    }
}
