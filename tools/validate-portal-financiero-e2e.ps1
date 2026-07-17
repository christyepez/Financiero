param(
    [string]$FinancialBaseUrl = "http://localhost:8083",
    [string]$PortalBaseUrl = "http://localhost:8082",
    [string]$PortalShellBaseUrl = "",
    [string]$PortalHealthPath = "/health",
    [string]$SqlHost = "host.docker.internal",
    [int]$SqlPort = 21433,
    [switch]$SkipPortalChecks,
    [switch]$SkipApiHealthChecks,
    [switch]$OutputMarkdown,
    [switch]$VerboseDiagnostics,
    [switch]$SuggestFixes
)

$ErrorActionPreference = "Continue"

function Protect-Detail($Detail) {
    if ($null -eq $Detail) { return "" }
    return ($Detail.ToString() `
        -replace '(Password|pwd|token|secret)=([^;&\s]+)', '$1=***' `
        -replace '(access_token|id_token|refresh_token)=([^;&\s]+)', '$1=***')
}

function Write-Check($Name, $Status, $Detail, $Code = "", $Suggestion = "") {
    [pscustomobject]@{ check = $Name; status = $Status; code = $Code; detail = (Protect-Detail $Detail); suggestion = (Protect-Detail $Suggestion) }
}

function Test-Http($Name, $Url, $Headers = @{}) {
    try {
        $response = Invoke-WebRequest -UseBasicParsing -Uri $Url -Headers $Headers -TimeoutSec 5
        if ([int]$response.StatusCode -ge 200 -and [int]$response.StatusCode -lt 300) {
            Write-Check $Name "PASS" ("HTTP " + [int]$response.StatusCode) "PASS" ""
        } else {
            Write-Check $Name "BLOCKED_DEPENDENCY" ("HTTP " + [int]$response.StatusCode) "HTTP_STATUS_UNEXPECTED" "Validate service health path, route and dependency readiness."
        }
    } catch {
        Write-Check $Name "BLOCKED_DEPENDENCY" $_.Exception.Message "HTTP_ENDPOINT_UNREACHABLE" "Start the external service or override the URL/health path if this environment uses a different port."
    }
}

$results = @()
$results += Write-Check "PowerShell" "PASS" $PSVersionTable.PSVersion.ToString() "PASS" ""

try {
    $resolved = [System.Net.Dns]::GetHostAddresses($SqlHost)
    $results += Write-Check "Shared SQL host resolution" "PASS" ($SqlHost + " -> " + (($resolved | Select-Object -First 3) -join ",")) "PASS" ""
} catch {
    $results += Write-Check "Shared SQL host resolution" "BLOCKED_DEPENDENCY" $_.Exception.Message "HOST_NOT_RESOLVED" "Validate Docker Desktop networking, host override and SHARED_SQL_HOST/SqlHost value."
}

try {
    docker compose config *> $null
    $results += Write-Check "Docker compose config" "PASS" "Financiero compose is valid." "PASS" ""
} catch {
    $results += Write-Check "Docker compose config" "FAIL" $_.Exception.Message "COMPOSE_CONFIG_INVALID" "Fix docker compose syntax/configuration before E2E runtime validation."
}

try {
    $tcp = Test-NetConnection -ComputerName $SqlHost -Port $SqlPort -InformationLevel Quiet -WarningAction SilentlyContinue
    if ($tcp) {
        $results += Write-Check "Shared SQL TCP" "PASS" "${SqlHost}:$SqlPort" "PASS" ""
    } else {
        $results += Write-Check "Shared SQL TCP" "BLOCKED_DEPENDENCY" "${SqlHost}:$SqlPort" "HOST_RESOLVES_BUT_PORT_CLOSED" "Start shared SQL, validate port mapping $SqlPort, firewall and that SQL is exposed to the host."
    }
} catch {
    $results += Write-Check "Shared SQL TCP" "BLOCKED_DEPENDENCY" $_.Exception.Message "HOST_RESOLVES_BUT_PORT_CLOSED" "Start shared SQL, validate port mapping and firewall."
}

$composeText = Get-Content -Raw -Path "docker-compose.yml"
if ($composeText -match "mcr\.microsoft\.com/mssql|1433:1433|container_name:\s*.*sql") {
    $results += Write-Check "No Financiero SQL container" "FAIL" "Potential SQL Server service found in Financiero compose." "FINANCIERO_SQL_CONTAINER_FOUND" "Remove SQL Server from Financiero compose and use the shared Portal/infra SQL runtime."
} else {
    $results += Write-Check "No Financiero SQL container" "PASS" "Financiero does not define its own SQL Server container." "PASS" ""
}

$devHeaders = @{ "X-Dev-Permissions" = "financial.electronicdocuments.read"; "X-Correlation-ID" = "synthetic-e2e-correlation" }
if ($SkipApiHealthChecks) {
    $results += Write-Check "Financiero API health checks" "PASS" "Skipped by -SkipApiHealthChecks." "SERVICE_SKIPPED" "Run without -SkipApiHealthChecks for full E2E evidence."
} else {
    $results += Test-Http "Financiero health" "$FinancialBaseUrl/health"
    $results += Test-Http "Financiero live" "$FinancialBaseUrl/health/live"
    $results += Test-Http "Financiero ready" "$FinancialBaseUrl/health/ready"
    $results += Test-Http "Financiero SRI health" "$FinancialBaseUrl/health/sri"
    $results += Test-Http "Financiero Content/File health" "$FinancialBaseUrl/health/content-file"
    $results += Test-Http "Financiero Portal readiness" "$FinancialBaseUrl/api/financial/portal-integration/readiness" $devHeaders
}

if ($SkipPortalChecks) {
    $results += Write-Check "Portal runtime checks" "PASS" "Skipped by -SkipPortalChecks." "SERVICE_SKIPPED" "Run without -SkipPortalChecks for full Portal evidence."
} else {
    $portalHealthUrl = $PortalBaseUrl.TrimEnd("/") + $PortalHealthPath
    $results += Test-Http "Portal Gateway health" $portalHealthUrl
    if (-not [string]::IsNullOrWhiteSpace($PortalShellBaseUrl)) {
        $results += Test-Http "Portal Shell base" $PortalShellBaseUrl
    }
}

if ($OutputMarkdown) {
    if ($VerboseDiagnostics -or $SuggestFixes) {
        "| Check | Status | Code | Detail | Suggested action |"
        "|---|---|---|---|---|"
    } else {
        "| Check | Status | Detail |"
        "|---|---|---|"
    }
    foreach ($result in $results) {
        if ($VerboseDiagnostics -or $SuggestFixes) {
            "| $($result.check) | $($result.status) | $($result.code) | $($result.detail -replace '\|','/') | $($result.suggestion -replace '\|','/') |"
        } else {
            "| $($result.check) | $($result.status) | $($result.detail -replace '\|','/') |"
        }
    }
} else {
    if ($VerboseDiagnostics -or $SuggestFixes) {
        $results | Format-Table check,status,code,detail,suggestion -AutoSize
    } else {
        $results | Format-Table check,status,detail -AutoSize
    }
}

if ($results.status -contains "FAIL") {
    exit 1
}

if ($results.status -contains "BLOCKED_DEPENDENCY") {
    exit 2
}

exit 0
