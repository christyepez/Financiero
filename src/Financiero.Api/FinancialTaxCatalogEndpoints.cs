using Financiero.Application;
using Financiero.Contracts;
using Microsoft.Extensions.Options;

namespace Financiero.Api;

public static class FinancialTaxCatalogEndpoints
{
    public static IEndpointRouteBuilder MapFinancialTaxCatalogs(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/financial/tax-catalogs").WithTags("Financial Tax Catalogs");
        group.MapGet("/", async (FinancialTaxCatalogService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
            Results.Ok(new ApiResponse<FinancialTaxCatalogSummary>(await service.GetAllAsync(Context(http, options.Value), ct), null, http.TraceIdentifier))).RequireAuthorization(FinancialPermissions.ElectronicDocumentsRead);
        group.MapGet("/purchase-document-types", async (FinancialTaxCatalogService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
            Results.Ok(new ApiResponse<FinancialTaxCatalogResponse>(await service.GetPurchaseDocumentTypesAsync(Context(http, options.Value), ct), null, http.TraceIdentifier))).RequireAuthorization(FinancialPermissions.ElectronicDocumentsRead);
        group.MapGet("/support-document-types", async (FinancialTaxCatalogService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
            Results.Ok(new ApiResponse<FinancialTaxCatalogResponse>(await service.GetSupportDocumentTypesAsync(Context(http, options.Value), ct), null, http.TraceIdentifier))).RequireAuthorization(FinancialPermissions.ElectronicDocumentsRead);
        group.MapGet("/voided-document-types", async (FinancialTaxCatalogService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
            Results.Ok(new ApiResponse<FinancialTaxCatalogResponse>(await service.GetVoidedDocumentTypesAsync(Context(http, options.Value), ct), null, http.TraceIdentifier))).RequireAuthorization(FinancialPermissions.ElectronicDocumentsRead);
        group.MapGet("/purchase-tax-codes", async (FinancialTaxCatalogService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
            Results.Ok(new ApiResponse<FinancialTaxCatalogResponse>(await service.GetPurchaseTaxCodesAsync(Context(http, options.Value), ct), null, http.TraceIdentifier))).RequireAuthorization(FinancialPermissions.ElectronicDocumentsRead);
        group.MapGet("/supplier-identification-types", async (FinancialTaxCatalogService service, HttpContext http, IOptions<FinancialPlatformOptions> options, CancellationToken ct) =>
            Results.Ok(new ApiResponse<FinancialTaxCatalogResponse>(await service.GetSupplierIdentificationTypesAsync(Context(http, options.Value), ct), null, http.TraceIdentifier))).RequireAuthorization(FinancialPermissions.ElectronicDocumentsRead);
        return app;
    }

    private static PortalCallContext Context(HttpContext http, FinancialPlatformOptions options)
    {
        var correlationId = http.Response.Headers.TryGetValue(options.CorrelationHeader, out var responseCorrelation) ? responseCorrelation.ToString() : http.TraceIdentifier;
        var bearer = http.Request.Headers.Authorization.ToString();
        return new PortalCallContext(options.TenantId, correlationId, bearer.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) ? bearer["Bearer ".Length..] : null);
    }
}
