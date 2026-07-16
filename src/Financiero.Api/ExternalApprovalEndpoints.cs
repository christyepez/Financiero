using Financiero.Application;
using Financiero.Contracts;
using Microsoft.Extensions.Options;

namespace Financiero.Api;

public static class ExternalApprovalEndpoints
{
    public static IEndpointRouteBuilder MapExternalApprovals(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/financial/external-approvals").WithTags("External Approval Foundation");
        group.MapGet("/", async (IExternalApprovalReadinessService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
            await ExecuteAsync(() => service.GetAllAsync(Context(http, options.Value), ct), http)).RequireAuthorization(FinancialPermissions.ElectronicDocumentsRead);
        group.MapGet("/readiness", async (string scope, IExternalApprovalReadinessService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
            await ExecuteAsync(() => service.CheckAsync(scope, Context(http, options.Value), ct), http)).RequireAuthorization(FinancialPermissions.ElectronicDocumentsRead);
        group.MapGet("/{scope}", async (string scope, IExternalApprovalReadinessService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
            await ExecuteAsync(() => service.GetAsync(scope, Context(http, options.Value), ct), http)).RequireAuthorization(FinancialPermissions.ElectronicDocumentsRead);
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
        catch (FinancialApplicationException ex) { return Results.Text(System.Text.Json.JsonSerializer.Serialize(new ApiResponse<object>(null, new ErrorContract(ex.Code, ex.Message), http.TraceIdentifier)), "application/json", statusCode: StatusCodes.Status400BadRequest); }
    }
}
