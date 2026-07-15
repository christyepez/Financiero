using Financiero.Application;
using Financiero.Contracts;
using Financiero.Domain;
using Microsoft.Extensions.Options;

namespace Financiero.Api;

public static class PurchaseTaxDocumentsEndpoints
{
    public static IEndpointRouteBuilder MapPurchaseTaxDocuments(this IEndpointRouteBuilder app)
    {
        var purchases = app.MapGroup("/api/financial/purchases").WithTags("Purchase Tax Documents");
        purchases.MapPost("/", async (CreatePurchaseTaxDocumentRequest request, PurchaseTaxDocumentService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
            await ExecuteAsync(() => service.CreateAsync(request, Context(http, options.Value), ct), http)).RequireAuthorization(FinancialPermissions.ElectronicDocumentsManage);
        purchases.MapPost("/{id:guid}/lines", async (Guid id, AddPurchaseLineRequest request, PurchaseTaxDocumentService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
            await ExecuteAsync(() => service.AddLineAsync(id, request, Context(http, options.Value), ct), http)).RequireAuthorization(FinancialPermissions.ElectronicDocumentsManage);
        purchases.MapPost("/{id:guid}/taxes", async (Guid id, AddPurchaseTaxRequest request, PurchaseTaxDocumentService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
            await ExecuteAsync(() => service.AddTaxAsync(id, request, Context(http, options.Value), ct), http)).RequireAuthorization(FinancialPermissions.ElectronicDocumentsManage);
        purchases.MapPost("/{id:guid}/validate", async (Guid id, PurchaseTaxDocumentService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
            await ExecuteAsync(() => service.ValidateAsync(id, Context(http, options.Value), ct), http)).RequireAuthorization(FinancialPermissions.ElectronicDocumentsManage);
        purchases.MapGet("/", async (string period, PurchaseTaxDocumentService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
            !PurchaseTaxDocumentValidator.IsFiscalPeriod(period) ? BadRequestJson("purchase.period.invalid", "Purchase period must use YYYY-MM.", http) : await ExecuteAsync(() => service.GetByPeriodAsync(period, Context(http, options.Value), ct), http)).RequireAuthorization(FinancialPermissions.ElectronicDocumentsRead);
        purchases.MapGet("/{id:guid}", async (Guid id, PurchaseTaxDocumentService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
            await ExecuteAsync(() => service.GetByIdAsync(id, Context(http, options.Value), ct), http)).RequireAuthorization(FinancialPermissions.ElectronicDocumentsRead);

        var voided = app.MapGroup("/api/financial/voided-documents").WithTags("Voided Tax Documents");
        voided.MapPost("/", async (RegisterVoidedTaxDocumentRequest request, VoidedTaxDocumentService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
            await ExecuteAsync(() => service.RegisterAsync(request, Context(http, options.Value), ct), http)).RequireAuthorization(FinancialPermissions.ElectronicDocumentsManage);
        voided.MapGet("/", async (string period, VoidedTaxDocumentService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
            !PurchaseTaxDocumentValidator.IsFiscalPeriod(period) ? BadRequestJson("voided.period.invalid", "Voided period must use YYYY-MM.", http) : await ExecuteAsync(() => service.GetByPeriodAsync(period, Context(http, options.Value), ct), http)).RequireAuthorization(FinancialPermissions.ElectronicDocumentsRead);
        voided.MapGet("/{id:guid}", async (Guid id, VoidedTaxDocumentService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
            await ExecuteAsync(() => service.GetByIdAsync(id, Context(http, options.Value), ct), http)).RequireAuthorization(FinancialPermissions.ElectronicDocumentsRead);
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
        catch (FinancialDomainException ex) { return BadRequestJson(ex.Code, ex.Message, http); }
        catch (FinancialApplicationException ex) when (ex.Code.EndsWith("not_found", StringComparison.OrdinalIgnoreCase)) { return Results.NotFound(new ApiResponse<object>(null, new ErrorContract(ex.Code, ex.Message), http.TraceIdentifier)); }
        catch (FinancialApplicationException ex) { return BadRequestJson(ex.Code, ex.Message, http); }
    }
    private static IResult BadRequestJson(string code, string message, HttpContext http) =>
        Results.Text(System.Text.Json.JsonSerializer.Serialize(new ApiResponse<object>(null, new ErrorContract(code, message), http.TraceIdentifier)), "application/json", statusCode: StatusCodes.Status400BadRequest);
}
