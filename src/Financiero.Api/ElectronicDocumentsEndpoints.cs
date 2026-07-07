using Financiero.Application;
using Financiero.Contracts;
using Financiero.Domain;
using Microsoft.Extensions.Options;

namespace Financiero.Api;

public static class ElectronicDocumentsEndpoints
{
    public static IEndpointRouteBuilder MapElectronicDocuments(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/financial/electronic-documents").WithTags("Electronic Documents");

        group.MapPost("/invoices", async (CreateInvoiceRequest request, ElectronicDocumentsService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
            await ExecuteAsync(() => service.CreateInvoiceDraftAsync(request, Context(http, options.Value), ct), http)).RequireAuthorization(FinancialPermissions.ElectronicDocumentsCreate);
        group.MapPost("/{id:guid}/lines", async (Guid id, AddElectronicDocumentLineRequest request, ElectronicDocumentsService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
            await ExecuteAsync(() => service.AddInvoiceLineAsync(id, request, Context(http, options.Value), ct), http)).RequireAuthorization(FinancialPermissions.ElectronicDocumentsUpdate);
        group.MapPost("/{id:guid}/generate-xml", async (Guid id, ElectronicDocumentsService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
            await ExecuteAsync(() => service.GenerateInvoiceXmlAsync(id, Context(http, options.Value), ct), http)).RequireAuthorization(FinancialPermissions.ElectronicDocumentsGenerate);
        group.MapPost("/{id:guid}/sign", async (Guid id, ElectronicDocumentsService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
            await ExecuteAsync(() => service.SignElectronicDocumentAsync(id, Context(http, options.Value), ct), http)).RequireAuthorization(FinancialPermissions.ElectronicDocumentsSign);
        group.MapPost("/{id:guid}/send", async (Guid id, ElectronicDocumentsService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
            await ExecuteAsync(() => service.SendElectronicDocumentAsync(id, Context(http, options.Value), ct), http)).RequireAuthorization(FinancialPermissions.ElectronicDocumentsSend);
        group.MapPost("/{id:guid}/authorize", async (Guid id, ElectronicDocumentsService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
            await ExecuteAsync(() => service.AuthorizeElectronicDocumentAsync(id, Context(http, options.Value), ct), http)).RequireAuthorization(FinancialPermissions.ElectronicDocumentsAuthorize);
        group.MapGet("/", async ([AsParameters] SearchElectronicDocumentsRequest request, ElectronicDocumentsService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
            await ExecuteAsync(() => service.SearchAsync(request, Context(http, options.Value), ct), http)).RequireAuthorization(FinancialPermissions.ElectronicDocumentsRead);
        group.MapGet("/{id:guid}", async (Guid id, ElectronicDocumentsService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
            await ExecuteAsync(() => service.GetByIdAsync(id, Context(http, options.Value), ct), http)).RequireAuthorization(FinancialPermissions.ElectronicDocumentsRead);
        group.MapGet("/by-access-key/{accessKey}", async (string accessKey, ElectronicDocumentsService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
            await ExecuteAsync(() => service.GetByAccessKeyAsync(accessKey, Context(http, options.Value), ct), http)).RequireAuthorization(FinancialPermissions.ElectronicDocumentsRead);

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
