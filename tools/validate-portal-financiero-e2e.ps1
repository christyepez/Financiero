param(
    [string]$FinancialBaseUrl = "http://localhost:8083",
    [string]$PortalBaseUrl = "http://localhost:8082",
    [string]$PortalShellBaseUrl = "",
    [string]$PortalHealthPath = "/health",
    [string]$SqlHost = "host.docker.internal",
    [int]$SqlPort = 21433,
    [switch]$SkipPortalChecks,
    [switch]$SkipApiHealthChecks,
    [switch]$OutputMarkdown
)

$ErrorActionPreference = "Continue"

function Protect-Detail($Detail) {
    if ($null -eq $Detail) { return "" }
    return ($Detail.ToString() `
        -replace '(Password|pwd|token|secret)=([^;&\s]+)', '$1=***' `
        -replace '(access_token|id_token|refresh_token)=([^;&\s]+)', '$1=***')
}

function Write-Check($Name, $Status, $Detail) {
    [pscustomobject]@{ check = $Name; status = $Status; detail = (Protect-Detail $Detail) }
}

function Test-Http($Name, $Url, $Headers = @{}) {
    try {
        $response = Invoke-WebRequest -UseBasicParsing -Uri $Url -Headers $Headers -TimeoutSec 5
        Write-Check $Name "PASS" ("HTTP " + [int]$response.StatusCode)
    } catch {
        Write-Check $Name "BLOCKED_DEPENDENCY" $_.Exception.Message
    }
}

$results = @()
$results += Write-Check "PowerShell" "PASS" $PSVersionTable.PSVersion.ToString()

try {
    $resolved = [System.Net.Dns]::GetHostAddresses($SqlHost)
    $results += Write-Check "Shared SQL host resolution" "PASS" ($SqlHost + " -> " + (($resolved | Select-Object -First 3) -join ","))
} catch {
    $results += Write-Check "Shared SQL host resolution" "BLOCKED_DEPENDENCY" $_.Exception.Message
}

try {
    docker compose config *> $null
    $results += Write-Check "Docker compose config" "PASS" "Financiero compose is valid."
} catch {
    $results += Write-Check "Docker compose config" "FAIL" $_.Exception.Message
}

try {
    $tcp = Test-NetConnection -ComputerName $SqlHost -Port $SqlPort -InformationLevel Quiet -WarningAction SilentlyContinue
    $results += Write-Check "Shared SQL TCP" ($(if ($tcp) { "PASS" } else { "BLOCKED_DEPENDENCY" })) "${SqlHost}:$SqlPort"
} catch {
    $results += Write-Check "Shared SQL TCP" "BLOCKED_DEPENDENCY" $_.Exception.Message
}

$composeText = Get-Content -Raw -Path "docker-compose.yml"
if ($composeText -match "mcr\.microsoft\.com/mssql|1433:1433|container_name:\s*.*sql") {
    $results += Write-Check "No Financiero SQL container" "FAIL" "Potential SQL Server service found in Financiero compose."
} else {
    $results += Write-Check "No Financiero SQL container" "PASS" "Financiero does not define its own SQL Server container."
}

$devHeaders = @{ "X-Dev-Permissions" = "financial.electronicdocuments.read"; "X-Correlation-ID" = "synthetic-e2e-correlation" }
if ($SkipApiHealthChecks) {
    $results += Write-Check "Financiero API health checks" "PASS" "Skipped by -SkipApiHealthChecks."
} else {
    $results += Test-Http "Financiero health" "$FinancialBaseUrl/health"
    $results += Test-Http "Financiero live" "$FinancialBaseUrl/health/live"
    $results += Test-Http "Financiero ready" "$FinancialBaseUrl/health/ready"
    $results += Test-Http "Financiero SRI health" "$FinancialBaseUrl/health/sri"
    $results += Test-Http "Financiero Content/File health" "$FinancialBaseUrl/health/content-file"
    $results += Test-Http "Financiero Portal readiness" "$FinancialBaseUrl/api/financial/portal-integration/readiness" $devHeaders
}

if ($SkipPortalChecks) {
    $results += Write-Check "Portal runtime checks" "PASS" "Skipped by -SkipPortalChecks."
} else {
    $portalHealthUrl = $PortalBaseUrl.TrimEnd("/") + $PortalHealthPath
    $results += Test-Http "Portal Gateway health" $portalHealthUrl
    if (-not [string]::IsNullOrWhiteSpace($PortalShellBaseUrl)) {
        $results += Test-Http "Portal Shell base" $PortalShellBaseUrl
    }
}

if ($OutputMarkdown) {
    "| Check | Status | Detail |"
    "|---|---|---|"
    foreach ($result in $results) {
        "| $($result.check) | $($result.status) | $($result.detail -replace '\|','/') |"
    }
} else {
    $results | Format-Table -AutoSize
}

if ($results.status -contains "FAIL") {
    exit 1
}

if ($results.status -contains "BLOCKED_DEPENDENCY") {
    exit 2
}

exit 0
