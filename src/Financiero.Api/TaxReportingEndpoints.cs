using Financiero.Application;
using Financiero.Contracts;
using Financiero.Domain;
using Microsoft.Extensions.Options;

namespace Financiero.Api;

public static class TaxReportingEndpoints
{
    public static IEndpointRouteBuilder MapTaxReporting(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/financial/tax-reporting").WithTags("Tax Reporting");

        group.MapGet("/summary", async ([AsParameters] TaxReportQuery request, ITaxReportingService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
            await ExecuteAsync(() => service.GetSummaryAsync(request, Context(http, options.Value), ct), http)).RequireAuthorization(FinancialPermissions.ElectronicDocumentsRead);
        group.MapGet("/documents", async ([AsParameters] TaxReportQuery request, ITaxReportingService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
            await ExecuteAsync(() => service.GetDocumentsAsync(request, Context(http, options.Value), ct), http)).RequireAuthorization(FinancialPermissions.ElectronicDocumentsRead);
        group.MapGet("/tax-totals", async ([AsParameters] TaxReportQuery request, ITaxReportingService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
            await ExecuteAsync(() => service.GetTaxTotalsAsync(request, Context(http, options.Value), ct), http)).RequireAuthorization(FinancialPermissions.ElectronicDocumentsRead);
        group.MapGet("/withholding-totals", async ([AsParameters] TaxReportQuery request, ITaxReportingService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
            await ExecuteAsync(() => service.GetWithholdingTotalsAsync(request, Context(http, options.Value), ct), http)).RequireAuthorization(FinancialPermissions.ElectronicDocumentsRead);
        group.MapGet("/export", async (ITaxExportService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
        {
            var request = TaxExportQueryFrom(http);
            if (!Enum.TryParse<TaxExportFormat>(request.Format, true, out _))
                return BadRequestJson("tax_export.format.invalid", "Tax export format must be Json or Csv.", http);
            return await ExecuteFileAsync(() => service.ExportAsync(request, Context(http, options.Value), ct), http);
        }).RequireAuthorization(FinancialPermissions.ElectronicDocumentsRead);
        group.MapPost("/export/store", async (ITaxExportService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
        {
            var request = TaxExportQueryFrom(http) with { Store = true };
            if (!Enum.TryParse<TaxExportFormat>(request.Format, true, out _))
                return BadRequestJson("tax_export.format.invalid", "Tax export format must be Json or Csv.", http);
            return await ExecuteAsync(async () =>
            {
                var result = await service.ExportAsync(request, Context(http, options.Value), ct);
                return new { result.Metadata, result.StoredFile };
            }, http);
        }).RequireAuthorization(FinancialPermissions.ElectronicDocumentsManage);
        group.MapGet("/ats-readiness", async (ITaxExportService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
        {
            var request = AtsReadinessQueryFrom(http);
            if (!IsValidAtsPeriodRequest(request))
                return BadRequestJson("ats.period.invalid", "ATS readiness period must use yyyy-MM or provide from/to dates.", http);
            return await ExecuteAsync(() => service.EvaluateAtsReadinessAsync(request, Context(http, options.Value), ct), http);
        }).RequireAuthorization(FinancialPermissions.ElectronicDocumentsRead);
        group.MapGet("/ats-official-design", async (ITaxExportService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
        {
            var request = AtsOfficialDesignQueryFrom(http);
            if (!IsValidAtsOfficialPeriodRequest(request))
                return BadRequestJson("ats.period.invalid", "ATS official design period must use yyyy-MM or provide from/to dates.", http);
            return await ExecuteAsync(() => service.EvaluateAtsOfficialDesignAsync(request, Context(http, options.Value), ct), http);
        }).RequireAuthorization(FinancialPermissions.ElectronicDocumentsRead);
        group.MapGet("/ats-section-readiness", async (AtsSupportMappingService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
        {
            var period = http.Request.Query["period"].ToString();
            if (!PurchaseTaxDocumentValidator.IsFiscalPeriod(period))
                return BadRequestJson("ats.period.invalid", "ATS section readiness period must use yyyy-MM.", http);
            return await ExecuteAsync(() => service.GetSectionReadinessAsync(period, Context(http, options.Value), ct), http);
        }).RequireAuthorization(FinancialPermissions.ElectronicDocumentsRead);
        group.MapGet("/support-document-mappings", async (AtsSupportMappingService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
            await ExecuteAsync(() => service.GetMappingsAsync(Context(http, options.Value), ct), http)).RequireAuthorization(FinancialPermissions.ElectronicDocumentsRead);
        group.MapGet("/action-queue", async ([AsParameters] TaxReportQuery request, ITaxExportService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
            await ExecuteAsync(() => service.GetActionQueueAsync(request, Context(http, options.Value), ct), http)).RequireAuthorization(FinancialPermissions.ElectronicDocumentsRead);
        group.MapGet("/monthly-summary", async ([AsParameters] TaxReportQuery request, ITaxExportService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
            await ExecuteAsync(() => service.GetMonthlySummaryAsync(request, Context(http, options.Value), ct), http)).RequireAuthorization(FinancialPermissions.ElectronicDocumentsRead);

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

    private static async Task<IResult> ExecuteFileAsync(Func<Task<TaxExportResult>> action, HttpContext http)
    {
        try
        {
            var result = await action();
            http.Response.Headers["X-Tax-Export-Hash"] = result.Metadata.Hash;
            http.Response.Headers["X-Tax-Export-Row-Count"] = result.Metadata.RowCount.ToString(System.Globalization.CultureInfo.InvariantCulture);
            return Results.File(result.Content, result.Metadata.ContentType, result.Metadata.FileName);
        }
        catch (FinancialDomainException ex) { return BadRequestJson(ex.Code, ex.Message, http); }
        catch (FinancialApplicationException ex) { return BadRequestJson(ex.Code, ex.Message, http); }
    }

    private static IResult BadRequestJson(string code, string message, HttpContext http) =>
        Results.Text(System.Text.Json.JsonSerializer.Serialize(new ApiResponse<object>(null, new ErrorContract(code, message), http.TraceIdentifier)), "application/json", statusCode: StatusCodes.Status400BadRequest);

    private static bool IsValidAtsPeriodRequest(AtsReadinessQuery request)
    {
        if (!string.IsNullOrWhiteSpace(request.Period) && DateOnly.TryParseExact($"{request.Period}-01", "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out _)) return true;
        return request.From.HasValue && request.To.HasValue && request.From.Value <= request.To.Value;
    }
    private static bool IsValidAtsOfficialPeriodRequest(AtsOfficialDesignQuery request)
    {
        if (!string.IsNullOrWhiteSpace(request.Period) && DateOnly.TryParseExact($"{request.Period}-01", "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out _)) return true;
        return request.From.HasValue && request.To.HasValue && request.From.Value <= request.To.Value;
    }

    private static TaxExportQuery TaxExportQueryFrom(HttpContext http) => new(
        From: Date(http, "from"),
        To: Date(http, "to"),
        DocumentType: Value(http, "documentType"),
        Status: Value(http, "status"),
        Environment: Value(http, "environment"),
        Kind: Value(http, "kind") ?? "DocumentSummary",
        Format: Value(http, "format") ?? "Json",
        IncludeSensitive: bool.TryParse(Value(http, "includeSensitive"), out var includeSensitive) && includeSensitive,
        Store: bool.TryParse(Value(http, "store"), out var store) && store);

    private static AtsReadinessQuery AtsReadinessQueryFrom(HttpContext http) => new(
        Period: Value(http, "period") ?? "",
        From: Date(http, "from"),
        To: Date(http, "to"),
        Environment: Value(http, "environment"));
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
