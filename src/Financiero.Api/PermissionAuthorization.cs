using Microsoft.AspNetCore.Authorization;

namespace Financiero.Api;

public sealed record PermissionRequirement(string Permission) : IAuthorizationRequirement;

public sealed class PermissionAuthorizationHandler(IWebHostEnvironment environment) : AuthorizationHandler<PermissionRequirement>
{
    private const string DevPermissionsHeader = "X-Dev-Permissions";

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        if (HasClaimPermission(context.User, requirement.Permission) || HasDevelopmentHeaderPermission(context, requirement.Permission))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }

    private static bool HasClaimPermission(System.Security.Claims.ClaimsPrincipal user, string permission)
    {
        var claimValues = user.Claims
            .Where(x => x.Type is "permission" or "permissions" or "scope" or "roles" or System.Security.Claims.ClaimTypes.Role)
            .SelectMany(x => x.Value.Split([' ', ','], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
        return claimValues.Any(x => string.Equals(x, permission, StringComparison.OrdinalIgnoreCase) || string.Equals(x, "financial.*", StringComparison.OrdinalIgnoreCase));
    }

    private bool HasDevelopmentHeaderPermission(AuthorizationHandlerContext context, string permission)
    {
        if (!environment.IsDevelopment()) return false;
        if (context.Resource is not HttpContext http) return false;
        if (!http.Request.Headers.TryGetValue(DevPermissionsHeader, out var header)) return false;

        return header.ToString()
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Any(x => string.Equals(x, permission, StringComparison.OrdinalIgnoreCase) || string.Equals(x, "financial.*", StringComparison.OrdinalIgnoreCase));
    }
}

public static class FinancialPermissions
{
    public const string AccountsRead = "financial.chartofaccounts.read";
    public const string AccountsCreate = "financial.chartofaccounts.create";
    public const string AccountsUpdate = "financial.chartofaccounts.update";
    public const string AccountsActivate = "financial.chartofaccounts.activate";
    public const string AccountsDeactivate = "financial.chartofaccounts.deactivate";
    public const string AccountsArchive = "financial.chartofaccounts.archive";

    public const string FiscalYearsRead = "financial.fiscalyears.read";
    public const string FiscalYearsCreate = "financial.fiscalyears.create";
    public const string FiscalYearsUpdate = "financial.fiscalyears.update";
    public const string FiscalYearsOpen = "financial.fiscalyears.open";
    public const string FiscalYearsClose = "financial.fiscalyears.close";
    public const string FiscalYearsArchive = "financial.fiscalyears.archive";

    public const string FiscalPeriodsRead = "financial.fiscalperiods.read";
    public const string FiscalPeriodsCreate = "financial.fiscalperiods.create";
    public const string FiscalPeriodsUpdate = "financial.fiscalperiods.update";
    public const string FiscalPeriodsOpen = "financial.fiscalperiods.open";
    public const string FiscalPeriodsClose = "financial.fiscalperiods.close";
    public const string FiscalPeriodsLock = "financial.fiscalperiods.lock";
    public const string FiscalPeriodsReopen = "financial.fiscalperiods.reopen";
    public const string FiscalPeriodsArchive = "financial.fiscalperiods.archive";

    public const string JournalEntriesRead = "financial.journalentries.read";
    public const string JournalEntriesCreate = "financial.journalentries.create";
    public const string JournalEntriesUpdate = "financial.journalentries.update";
    public const string JournalEntriesPost = "financial.journalentries.post";
    public const string JournalEntriesReverse = "financial.journalentries.reverse";
    public const string JournalEntriesVoid = "financial.journalentries.void";

    public const string ElectronicDocumentsRead = "financial.electronicdocuments.read";
    public const string ElectronicDocumentsCreate = "financial.electronicdocuments.create";
    public const string ElectronicDocumentsUpdate = "financial.electronicdocuments.update";
    public const string ElectronicDocumentsGenerate = "financial.electronicdocuments.generate";
    public const string ElectronicDocumentsSign = "financial.electronicdocuments.sign";
    public const string ElectronicDocumentsSend = "financial.electronicdocuments.send";
    public const string ElectronicDocumentsAuthorize = "financial.electronicdocuments.authorize";
    public const string ElectronicDocumentsCancel = "financial.electronicdocuments.cancel";
    public const string ElectronicDocumentsManage = "financial.electronicdocuments.manage";

    public static IEnumerable<string> All =>
    [
        AccountsRead, AccountsCreate, AccountsUpdate, AccountsActivate, AccountsDeactivate, AccountsArchive,
        FiscalYearsRead, FiscalYearsCreate, FiscalYearsUpdate, FiscalYearsOpen, FiscalYearsClose, FiscalYearsArchive,
        FiscalPeriodsRead, FiscalPeriodsCreate, FiscalPeriodsUpdate, FiscalPeriodsOpen, FiscalPeriodsClose, FiscalPeriodsLock, FiscalPeriodsReopen, FiscalPeriodsArchive,
        JournalEntriesRead, JournalEntriesCreate, JournalEntriesUpdate, JournalEntriesPost, JournalEntriesReverse, JournalEntriesVoid,
        ElectronicDocumentsRead, ElectronicDocumentsCreate, ElectronicDocumentsUpdate, ElectronicDocumentsGenerate, ElectronicDocumentsSign,
        ElectronicDocumentsSend, ElectronicDocumentsAuthorize, ElectronicDocumentsCancel, ElectronicDocumentsManage
    ];
}

public static class FinancialAuthorizationExtensions
{
    public static IServiceCollection AddFinancialRuntimeAuthorization(this IServiceCollection services)
    {
        services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
        services.AddAuthorization(options =>
        {
            foreach (var permission in FinancialPermissions.All)
            {
                options.AddPolicy(permission, policy => policy.AddRequirements(new PermissionRequirement(permission)));
            }
        });
        return services;
    }
}
