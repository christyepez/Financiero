using Financiero.Application;
using Financiero.Contracts;
using Microsoft.Extensions.Options;

namespace Financiero.Api;

public static class PortalIntegrationEndpoints
{
    public static IEndpointRouteBuilder MapPortalIntegration(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/financial/portal-integration").WithTags("Portal Integration Readiness");
        group.MapGet("/readiness", async (PortalIntegrationReadinessService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
            Results.Ok(new ApiResponse<PortalIntegrationReadinessResult>(await service.CheckAsync(Context(http, options.Value), ct), null, http.TraceIdentifier)))
            .RequireAuthorization(FinancialPermissions.ElectronicDocumentsRead);
        return app;
    }

    private static PortalCallContext Context(HttpContext http, FinancialPlatformOptions options)
    {
        var correlationId = http.Response.Headers.TryGetValue(options.CorrelationHeader, out var responseCorrelation) ? responseCorrelation.ToString() : http.TraceIdentifier;
        var bearer = http.Request.Headers.Authorization.ToString();
        return new PortalCallContext(options.TenantId, correlationId, bearer.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) ? bearer["Bearer ".Length..] : null);
    }
}
