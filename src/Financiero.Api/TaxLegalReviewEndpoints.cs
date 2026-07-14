using Financiero.Application;
using Financiero.Contracts;
using Financiero.Domain;
using Microsoft.Extensions.Options;

namespace Financiero.Api;

public static class TaxLegalReviewEndpoints
{
    public static IEndpointRouteBuilder MapTaxLegalReview(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/financial/tax-legal-review").WithTags("Tax Legal Review");

        group.MapGet("/ride-gaps", async (IRideLegalGapAnalysisService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
            await ExecuteAsync(() => service.AnalyzeAsync(Context(http, options.Value), ct), http)).RequireAuthorization(FinancialPermissions.ElectronicDocumentsRead);

        group.MapGet("/ats-gaps", async (IAtsOfficialGapAnalysisService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
        {
            var request = AtsOfficialDesignQueryFrom(http);
            if (!IsValidAtsPeriodRequest(request))
                return BadRequestJson("ats.period.invalid", "ATS gap analysis period must use yyyy-MM or provide from/to dates.", http);
            return await ExecuteAsync(() => service.AnalyzeAsync(request, Context(http, options.Value), ct), http);
        }).RequireAuthorization(FinancialPermissions.ElectronicDocumentsRead);

        group.MapGet("/approval-checklist", async (ITaxLegalApprovalChecklistService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
        {
            var scope = Value(http, "scope") ?? "all";
            if (scope.ToLowerInvariant() is not ("ride" or "ats" or "all"))
                return BadRequestJson("tax_legal.scope.invalid", "Approval checklist scope must be ride, ats or all.", http);
            return await ExecuteAsync(() => service.CheckAsync(scope, Context(http, options.Value), ct), http);
        }).RequireAuthorization(FinancialPermissions.ElectronicDocumentsRead);

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
        catch (FinancialApplicationException ex) { return Results.BadRequest(new ApiResponse<object>(null, new ErrorContract(ex.Code, ex.Message), http.TraceIdentifier)); }
    }

    private static IResult BadRequestJson(string code, string message, HttpContext http) =>
        Results.Text(System.Text.Json.JsonSerializer.Serialize(new ApiResponse<object>(null, new ErrorContract(code, message), http.TraceIdentifier)), "application/json", statusCode: StatusCodes.Status400BadRequest);

    private static bool IsValidAtsPeriodRequest(AtsOfficialDesignQuery request)
    {
        if (!string.IsNullOrWhiteSpace(request.Period) && DateOnly.TryParseExact($"{request.Period}-01", "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out _)) return true;
        return request.From.HasValue && request.To.HasValue && request.From.Value <= request.To.Value;
    }

    private static AtsOfficialDesignQuery AtsOfficialDesignQueryFrom(HttpContext http) => new(
        Period: Value(http, "period") ?? "",
        From: Date(http, "from"),
        To: Date(http, "to"),
        Environment: Value(http, "environment"));

    private static string? Value(HttpContext http, string key) =>
        http.Request.Query.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value.ToString()) ? value.ToString() : null;

    private static DateOnly? Date(HttpContext http, string key) =>
        DateOnly.TryParse(Value(http, key), System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var value) ? value : null;
}
