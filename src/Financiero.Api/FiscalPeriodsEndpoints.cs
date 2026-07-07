using Financiero.Application;
using Financiero.Contracts;
using Financiero.Domain;
using Microsoft.Extensions.Options;

namespace Financiero.Api;

public static class FiscalPeriodsEndpoints
{
    public static IEndpointRouteBuilder MapFiscalPeriods(this IEndpointRouteBuilder app)
    {
        var years = app.MapGroup("/api/financial/fiscal-years").WithTags("Fiscal Years");
        years.MapPost("/", async (CreateFiscalYearRequest request, FiscalPeriodsService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
            await ExecuteAsync(() => service.CreateYearAsync(request, Context(http, options.Value), ct), http));
        years.MapPut("/{id:guid}", async (Guid id, UpdateFiscalYearRequest request, FiscalPeriodsService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
            await ExecuteAsync(() => service.UpdateYearAsync(id, request, Context(http, options.Value), ct), http));
        years.MapGet("/", async ([AsParameters] SearchFiscalYearsRequest request, FiscalPeriodsService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
            await ExecuteAsync(() => service.SearchYearsAsync(request, Context(http, options.Value), ct), http));
        years.MapGet("/{id:guid}", async (Guid id, FiscalPeriodsService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
            await ExecuteAsync(() => service.GetYearByIdAsync(id, Context(http, options.Value), ct), http));
        years.MapPost("/{id:guid}/open", async (Guid id, FiscalPeriodsService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
            await ExecuteAsync(() => service.OpenYearAsync(id, Context(http, options.Value), ct), http));
        years.MapPost("/{id:guid}/close", async (Guid id, FiscalPeriodsService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
            await ExecuteAsync(() => service.CloseYearAsync(id, Context(http, options.Value), ct), http));
        years.MapPost("/{id:guid}/archive", async (Guid id, FiscalPeriodsService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
            await ExecuteAsync(() => service.ArchiveYearAsync(id, Context(http, options.Value), ct), http));

        var periods = app.MapGroup("/api/financial/fiscal-periods").WithTags("Fiscal Periods");
        periods.MapPost("/", async (CreateFiscalPeriodRequest request, FiscalPeriodsService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
            await ExecuteAsync(() => service.CreatePeriodAsync(request, Context(http, options.Value), ct), http));
        periods.MapPut("/{id:guid}", async (Guid id, UpdateFiscalPeriodRequest request, FiscalPeriodsService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
            await ExecuteAsync(() => service.UpdatePeriodAsync(id, request, Context(http, options.Value), ct), http));
        periods.MapGet("/", async ([AsParameters] SearchFiscalPeriodsRequest request, FiscalPeriodsService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
            await ExecuteAsync(() => service.SearchPeriodsAsync(request, Context(http, options.Value), ct), http));
        periods.MapGet("/{id:guid}", async (Guid id, FiscalPeriodsService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
            await ExecuteAsync(() => service.GetPeriodByIdAsync(id, Context(http, options.Value), ct), http));
        periods.MapGet("/open-by-date", async (DateOnly date, FiscalPeriodsService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
            await ExecuteAsync(() => service.GetOpenPeriodByDateAsync(date, Context(http, options.Value), ct), http));
        periods.MapPost("/{id:guid}/open", async (Guid id, FiscalPeriodsService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
            await ExecuteAsync(() => service.OpenPeriodAsync(id, Context(http, options.Value), ct), http));
        periods.MapPost("/{id:guid}/close", async (Guid id, FiscalPeriodsService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
            await ExecuteAsync(() => service.ClosePeriodAsync(id, Context(http, options.Value), ct), http));
        periods.MapPost("/{id:guid}/lock", async (Guid id, FiscalPeriodsService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
            await ExecuteAsync(() => service.LockPeriodAsync(id, Context(http, options.Value), ct), http));
        periods.MapPost("/{id:guid}/reopen", async (Guid id, FiscalPeriodsService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
            await ExecuteAsync(() => service.ReopenPeriodAsync(id, Context(http, options.Value), ct), http));
        periods.MapPost("/{id:guid}/archive", async (Guid id, FiscalPeriodsService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
            await ExecuteAsync(() => service.ArchivePeriodAsync(id, Context(http, options.Value), ct), http));

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
