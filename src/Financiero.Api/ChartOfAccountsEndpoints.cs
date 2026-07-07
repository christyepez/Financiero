using Financiero.Application;
using Financiero.Contracts;
using Financiero.Domain;
using Microsoft.Extensions.Options;

namespace Financiero.Api;

public static class ChartOfAccountsEndpoints
{
    public static RouteGroupBuilder MapChartOfAccounts(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/financial/accounts").WithTags("Chart of Accounts");

        group.MapPost("/", async (CreateAccountRequest request, ChartOfAccountsService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
            await ExecuteAsync(() => service.CreateAsync(request, Context(http, options.Value), ct), http))
            .RequireAuthorization(FinancialPermissions.AccountsCreate);

        group.MapPut("/{id:guid}", async (Guid id, UpdateAccountRequest request, ChartOfAccountsService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
            await ExecuteAsync(() => service.UpdateAsync(id, request, Context(http, options.Value), ct), http))
            .RequireAuthorization(FinancialPermissions.AccountsUpdate);

        group.MapGet("/", async ([AsParameters] SearchAccountsRequest request, ChartOfAccountsService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
            await ExecuteAsync(() => service.SearchAsync(request, Context(http, options.Value), ct), http))
            .RequireAuthorization(FinancialPermissions.AccountsRead);

        group.MapGet("/{id:guid}", async (Guid id, ChartOfAccountsService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
            await ExecuteAsync(() => service.GetByIdAsync(id, Context(http, options.Value), ct), http))
            .RequireAuthorization(FinancialPermissions.AccountsRead);

        group.MapGet("/by-code/{code}", async (string code, ChartOfAccountsService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
            await ExecuteAsync(() => service.GetByCodeAsync(code, Context(http, options.Value), ct), http))
            .RequireAuthorization(FinancialPermissions.AccountsRead);

        group.MapGet("/tree", async (ChartOfAccountsService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
            await ExecuteAsync(() => service.GetTreeAsync(Context(http, options.Value), ct), http))
            .RequireAuthorization(FinancialPermissions.AccountsRead);

        group.MapPost("/{id:guid}/activate", async (Guid id, ChartOfAccountsService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
            await ExecuteAsync(() => service.ActivateAsync(id, Context(http, options.Value), ct), http))
            .RequireAuthorization(FinancialPermissions.AccountsActivate);

        group.MapPost("/{id:guid}/deactivate", async (Guid id, ChartOfAccountsService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
            await ExecuteAsync(() => service.DeactivateAsync(id, Context(http, options.Value), ct), http))
            .RequireAuthorization(FinancialPermissions.AccountsDeactivate);

        group.MapPost("/{id:guid}/archive", async (Guid id, ChartOfAccountsService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
            await ExecuteAsync(() => service.ArchiveAsync(id, Context(http, options.Value), ct), http))
            .RequireAuthorization(FinancialPermissions.AccountsArchive);

        return group;
    }

    private static PortalCallContext Context(HttpContext http, FinancialPlatformOptions options)
    {
        var correlationId = http.Response.Headers.TryGetValue(options.CorrelationHeader, out var responseCorrelation)
            ? responseCorrelation.ToString()
            : http.TraceIdentifier;
        var bearer = http.Request.Headers.Authorization.ToString();
        return new PortalCallContext(options.TenantId, correlationId, bearer.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) ? bearer["Bearer ".Length..] : null);
    }

    private static async Task<IResult> ExecuteAsync<T>(Func<Task<T>> action, HttpContext http)
    {
        try
        {
            return Results.Ok(new ApiResponse<T>(await action(), null, http.TraceIdentifier));
        }
        catch (FinancialDomainException ex)
        {
            return Results.BadRequest(new ApiResponse<object>(null, new ErrorContract(ex.Code, ex.Message), http.TraceIdentifier));
        }
        catch (FinancialApplicationException ex) when (ex.Code.EndsWith("not_found", StringComparison.OrdinalIgnoreCase))
        {
            return Results.NotFound(new ApiResponse<object>(null, new ErrorContract(ex.Code, ex.Message), http.TraceIdentifier));
        }
        catch (FinancialApplicationException ex)
        {
            return Results.BadRequest(new ApiResponse<object>(null, new ErrorContract(ex.Code, ex.Message), http.TraceIdentifier));
        }
    }
}
