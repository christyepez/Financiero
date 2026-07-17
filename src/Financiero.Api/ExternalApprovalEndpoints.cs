using Financiero.Application;
using Financiero.Contracts;
using Financiero.Domain;
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

        var requests = app.MapGroup("/api/financial/external-approval-requests").WithTags("External Approval Requests Foundation");
        requests.MapGet("/", async (string? scope, string? status, ExternalApprovalWorkflowQueryService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
            await ExecuteAsync(() => service.ListRequestsAsync(scope, status, Context(http, options.Value), ct), http)).RequireAuthorization(FinancialPermissions.ElectronicDocumentsRead);
        requests.MapGet("/readiness", async (string? scope, ExternalApprovalWorkflowQueryService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
            await ExecuteAsync(() => service.GetReadinessWithPersistedRequestsAsync(scope ?? "all", Context(http, options.Value), ct), http)).RequireAuthorization(FinancialPermissions.ElectronicDocumentsRead);
        requests.MapGet("/integration-readiness", async (ExternalApprovalWorkflowQueryService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
            await ExecuteAsync(() => service.GetIntegrationReadinessAsync(Context(http, options.Value), ct), http)).RequireAuthorization(FinancialPermissions.ElectronicDocumentsRead);
        requests.MapGet("/{id:guid}", async (Guid id, ExternalApprovalWorkflowQueryService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
            await ExecuteAsync(() => service.GetRequestAsync(id, Context(http, options.Value), ct), http)).RequireAuthorization(FinancialPermissions.ElectronicDocumentsRead);
        requests.MapPost("/", async (CreateExternalApprovalRequest request, ExternalApprovalWorkflowCommandService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
            await ExecuteAsync(() => service.CreateRequestAsync(request, Context(http, options.Value), ct), http)).RequireAuthorization(FinancialPermissions.ElectronicDocumentsManage);
        requests.MapPost("/{id:guid}/submit", async (Guid id, ExternalApprovalWorkflowCommandService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
            await ExecuteAsync(() => service.SubmitRequestAsync(id, Context(http, options.Value), ct), http)).RequireAuthorization(FinancialPermissions.ElectronicDocumentsManage);
        requests.MapPost("/{id:guid}/start-review", async (Guid id, ExternalApprovalWorkflowCommandService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
            await ExecuteAsync(() => service.StartReviewAsync(id, Context(http, options.Value), ct), http)).RequireAuthorization(FinancialPermissions.ElectronicDocumentsManage);
        requests.MapPost("/{id:guid}/evidence-references", async (Guid id, AddExternalApprovalEvidenceReferenceRequest request, ExternalApprovalWorkflowCommandService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
            await ExecuteAsync(() => service.AddEvidenceReferenceAsync(id, request, Context(http, options.Value), ct), http)).RequireAuthorization(FinancialPermissions.ElectronicDocumentsManage);
        requests.MapPost("/{id:guid}/decision", async (Guid id, RecordExternalApprovalDecisionRequest request, ExternalApprovalWorkflowCommandService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
            await ExecuteAsync(() => service.RecordDecisionAsync(id, request, Context(http, options.Value), ct), http)).RequireAuthorization(FinancialPermissions.ElectronicDocumentsManage);
        requests.MapPost("/{id:guid}/cancel", async (Guid id, CancelExternalApprovalRequest request, ExternalApprovalWorkflowCommandService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
            await ExecuteAsync(() => service.CancelRequestAsync(id, request, Context(http, options.Value), ct), http)).RequireAuthorization(FinancialPermissions.ElectronicDocumentsManage);
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
        catch (FinancialDomainException ex) { return Results.Text(System.Text.Json.JsonSerializer.Serialize(new ApiResponse<object>(null, new ErrorContract(ex.Code, ex.Message), http.TraceIdentifier)), "application/json", statusCode: StatusCodes.Status400BadRequest); }
    }
}
