namespace Financiero.Application;

public static class ChartOfAccountsPortalMetadata
{
    public const string ModuleCode = "financiero";
    public const string MenuGroup = "Contabilidad";
    public const string MenuItem = "Plan de cuentas";
    public const string Route = "/financial/accounts";
    public const string ReadPermission = "financial.chartofaccounts.read";

    public static readonly string[] Permissions =
    [
        ReadPermission,
        "financial.chartofaccounts.create",
        "financial.chartofaccounts.update",
        "financial.chartofaccounts.activate",
        "financial.chartofaccounts.deactivate",
        "financial.chartofaccounts.archive",
        "financial.chartofaccounts.manage"
    ];

    public static readonly string[] ConfigurationKeys =
    [
        "financial.accountCode.format",
        "financial.accountCode.maxLength",
        "financial.accountCode.separator",
        "financial.accountLevels.maxDepth",
        "financial.chartOfAccounts.allowManualCodes",
        "financial.chartOfAccounts.requireParentForLevelGreaterThanOne"
    ];

    public static readonly string[] AuditEvents =
    [
        "AccountCreated",
        "AccountUpdated",
        "AccountActivated",
        "AccountDeactivated",
        "AccountArchived"
    ];

    public static readonly string[] OutboxEvents =
    [
        "FinancialAccountCreated",
        "FinancialAccountUpdated",
        "FinancialAccountStatusChanged"
    ];
}
