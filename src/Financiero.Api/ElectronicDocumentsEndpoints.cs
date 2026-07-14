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
        group.MapPost("/credit-notes", async (CreateCreditNoteRequest request, ElectronicDocumentsService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
            await ExecuteAsync(() => service.CreateCreditNoteDraftAsync(request, Context(http, options.Value), ct), http)).RequireAuthorization(FinancialPermissions.ElectronicDocumentsCreate);
        group.MapPost("/debit-notes", async (CreateDebitNoteRequest request, ElectronicDocumentsService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
            await ExecuteAsync(() => service.CreateDebitNoteDraftAsync(request, Context(http, options.Value), ct), http)).RequireAuthorization(FinancialPermissions.ElectronicDocumentsCreate);
        group.MapPost("/withholdings", async (CreateWithholdingRequest request, ElectronicDocumentsService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
            await ExecuteAsync(() => service.CreateWithholdingDraftAsync(request, Context(http, options.Value), ct), http)).RequireAuthorization(FinancialPermissions.ElectronicDocumentsCreate);
        group.MapPost("/{id:guid}/lines", async (Guid id, AddElectronicDocumentLineRequest request, ElectronicDocumentsService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
            await ExecuteAsync(() => service.AddInvoiceLineAsync(id, request, Context(http, options.Value), ct), http)).RequireAuthorization(FinancialPermissions.ElectronicDocumentsUpdate);
        group.MapPost("/{id:guid}/credit-note-lines", async (Guid id, AddElectronicDocumentLineRequest request, ElectronicDocumentsService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
            await ExecuteAsync(() => service.AddInvoiceLineAsync(id, request, Context(http, options.Value), ct), http)).RequireAuthorization(FinancialPermissions.ElectronicDocumentsUpdate);
        group.MapPost("/{id:guid}/debit-note-reasons", async (Guid id, AddDebitNoteReasonRequest request, ElectronicDocumentsService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
            await ExecuteAsync(() => service.AddDebitNoteReasonAsync(id, request, Context(http, options.Value), ct), http)).RequireAuthorization(FinancialPermissions.ElectronicDocumentsUpdate);
        group.MapPost("/{id:guid}/withholding-taxes", async (Guid id, AddWithholdingTaxRequest request, ElectronicDocumentsService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
            await ExecuteAsync(() => service.AddWithholdingTaxAsync(id, request, Context(http, options.Value), ct), http)).RequireAuthorization(FinancialPermissions.ElectronicDocumentsUpdate);
        group.MapPost("/{id:guid}/generate-xml", async (Guid id, ElectronicDocumentsService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
            await ExecuteAsync(() => service.GenerateInvoiceXmlAsync(id, Context(http, options.Value), ct), http)).RequireAuthorization(FinancialPermissions.ElectronicDocumentsGenerate);
        group.MapPost("/{id:guid}/generate-credit-note-xml", async (Guid id, ElectronicDocumentsService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
            await ExecuteAsync(() => service.GenerateCreditNoteXmlAsync(id, Context(http, options.Value), ct), http)).RequireAuthorization(FinancialPermissions.ElectronicDocumentsGenerate);
        group.MapPost("/{id:guid}/generate-debit-note-xml", async (Guid id, ElectronicDocumentsService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
            await ExecuteAsync(() => service.GenerateDebitNoteXmlAsync(id, Context(http, options.Value), ct), http)).RequireAuthorization(FinancialPermissions.ElectronicDocumentsGenerate);
        group.MapPost("/{id:guid}/generate-withholding-xml", async (Guid id, ElectronicDocumentsService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
            await ExecuteAsync(() => service.GenerateWithholdingXmlAsync(id, Context(http, options.Value), ct), http)).RequireAuthorization(FinancialPermissions.ElectronicDocumentsGenerate);
        group.MapPost("/{id:guid}/sign", async (Guid id, ElectronicDocumentsService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
            await ExecuteAsync(() => service.SignElectronicDocumentAsync(id, Context(http, options.Value), ct), http)).RequireAuthorization(FinancialPermissions.ElectronicDocumentsSign);
        group.MapPost("/{id:guid}/send", async (Guid id, ElectronicDocumentsService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
            await ExecuteAsync(() => service.SendElectronicDocumentAsync(id, Context(http, options.Value), ct), http)).RequireAuthorization(FinancialPermissions.ElectronicDocumentsSend);
        group.MapPost("/{id:guid}/authorize", async (Guid id, ElectronicDocumentsService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
            await ExecuteAsync(() => service.AuthorizeElectronicDocumentAsync(id, Context(http, options.Value), ct), http)).RequireAuthorization(FinancialPermissions.ElectronicDocumentsAuthorize);
        group.MapPost("/{id:guid}/validate-xml", async (Guid id, ElectronicDocumentsService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
            await ExecuteAsync(() => service.ValidateInvoiceXmlAsync(id, Context(http, options.Value), ct), http)).RequireAuthorization(FinancialPermissions.ElectronicDocumentsGenerate);
        group.MapPost("/{id:guid}/generate-ride", async (Guid id, ElectronicDocumentsService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
            await ExecuteAsync(() => service.GenerateRidePdfAsync(id, Context(http, options.Value), ct), http)).RequireAuthorization(FinancialPermissions.ElectronicDocumentsGenerate);
        group.MapPost("/{id:guid}/store-ride", async (Guid id, ElectronicDocumentsService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
            await ExecuteAsync(() => service.StoreRideAsync(id, Context(http, options.Value), ct), http)).RequireAuthorization(FinancialPermissions.ElectronicDocumentsGenerate);
        group.MapGet("/", async ([AsParameters] SearchElectronicDocumentsRequest request, ElectronicDocumentsService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
            await ExecuteAsync(() => service.SearchAsync(request, Context(http, options.Value), ct), http)).RequireAuthorization(FinancialPermissions.ElectronicDocumentsRead);
        group.MapGet("/{id:guid}", async (Guid id, ElectronicDocumentsService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
            await ExecuteAsync(() => service.GetByIdAsync(id, Context(http, options.Value), ct), http)).RequireAuthorization(FinancialPermissions.ElectronicDocumentsRead);
        group.MapGet("/{id:guid}/status", async (Guid id, ElectronicDocumentsService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
            await ExecuteAsync(() => service.GetStatusAsync(id, Context(http, options.Value), ct), http)).RequireAuthorization(FinancialPermissions.ElectronicDocumentsRead);
        group.MapGet("/{id:guid}/storage-metadata", async (Guid id, ElectronicDocumentsService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
            await ExecuteAsync(() => service.GetStorageMetadataAsync(id, Context(http, options.Value), ct), http)).RequireAuthorization(FinancialPermissions.ElectronicDocumentsRead);
        group.MapGet("/{id:guid}/ride-metadata", async (Guid id, ElectronicDocumentsService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
            await ExecuteAsync(() => service.GetRideMetadataAsync(id, Context(http, options.Value), ct), http)).RequireAuthorization(FinancialPermissions.ElectronicDocumentsRead);
        group.MapGet("/{id:guid}/ride-preview", async (Guid id, ElectronicDocumentsService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
            await ExecuteAsync(() => service.GetRidePreviewAsync(id, Context(http, options.Value), ct), http)).RequireAuthorization(FinancialPermissions.ElectronicDocumentsRead);
        group.MapGet("/{id:guid}/integration-status", async (Guid id, ElectronicDocumentsService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
            await ExecuteAsync(() => service.GetIntegrationStatusAsync(id, Context(http, options.Value), ct), http)).RequireAuthorization(FinancialPermissions.ElectronicDocumentsRead);
        group.MapGet("/by-access-key/{accessKey}", async (string accessKey, ElectronicDocumentsService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
            await ExecuteAsync(() => service.GetByAccessKeyAsync(accessKey, Context(http, options.Value), ct), http)).RequireAuthorization(FinancialPermissions.ElectronicDocumentsRead);
        group.MapGet("/sri/readiness", async (SriIntegrationReadinessService readiness, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
            await ExecuteAsync(() => readiness.CheckAsync(Context(http, options.Value), ct), http)).RequireAuthorization(FinancialPermissions.ElectronicDocumentsManage);
        group.MapGet("/sri/connectivity-probe", async (SriManualTestConnectivityService connectivity, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
            await ExecuteAsync(() => connectivity.CheckAsync(Context(http, options.Value), ct), http)).RequireAuthorization(FinancialPermissions.ElectronicDocumentsManage);
        group.MapGet("/content-file/readiness", async (ContentFileReadinessService readiness, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
            await ExecuteAsync(() => readiness.CheckAsync(Context(http, options.Value), ct), http)).RequireAuthorization(FinancialPermissions.ElectronicDocumentsManage);

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
