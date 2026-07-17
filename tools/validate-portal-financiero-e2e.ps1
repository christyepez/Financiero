param(
    [string]$FinancialBaseUrl = "http://localhost:8083",
    [string]$PortalBaseUrl = "http://localhost:8082",
    [string]$SqlHost = "host.docker.internal",
    [int]$SqlPort = 21433
)

$ErrorActionPreference = "Continue"

function Write-Check($Name, $Status, $Detail) {
    [pscustomobject]@{ check = $Name; status = $Status; detail = $Detail }
}

function Test-Http($Name, $Url, $Headers = @{}) {
    try {
        $response = Invoke-WebRequest -UseBasicParsing -Uri $Url -Headers $Headers -TimeoutSec 5
        Write-Check $Name "PASS" ("HTTP " + [int]$response.StatusCode)
    } catch {
        Write-Check $Name "BLOCKED" ($_.Exception.Message -replace '(Password|pwd|token|secret)=([^;&\s]+)', '$1=***')
    }
}

$results = @()
$results += Write-Check "PowerShell" "PASS" $PSVersionTable.PSVersion.ToString()

try {
    docker compose config *> $null
    $results += Write-Check "Docker compose config" "PASS" "Financiero compose is valid."
} catch {
    $results += Write-Check "Docker compose config" "BLOCKED" $_.Exception.Message
}

try {
    $tcp = Test-NetConnection -ComputerName $SqlHost -Port $SqlPort -InformationLevel Quiet -WarningAction SilentlyContinue
    $results += Write-Check "Shared SQL TCP" ($(if ($tcp) { "PASS" } else { "BLOCKED" })) "${SqlHost}:$SqlPort"
} catch {
    $results += Write-Check "Shared SQL TCP" "BLOCKED" $_.Exception.Message
}

$composeText = Get-Content -Raw -Path "docker-compose.yml"
if ($composeText -match "mcr\.microsoft\.com/mssql|1433:1433|container_name:\s*.*sql") {
    $results += Write-Check "No Financiero SQL container" "FAIL" "Potential SQL Server service found in Financiero compose."
} else {
    $results += Write-Check "No Financiero SQL container" "PASS" "Financiero does not define its own SQL Server container."
}

$devHeaders = @{ "X-Dev-Permissions" = "financial.electronicdocuments.read"; "X-Correlation-ID" = "synthetic-e2e-correlation" }
$results += Test-Http "Financiero health" "$FinancialBaseUrl/health"
$results += Test-Http "Financiero live" "$FinancialBaseUrl/health/live"
$results += Test-Http "Financiero ready" "$FinancialBaseUrl/health/ready"
$results += Test-Http "Financiero SRI health" "$FinancialBaseUrl/health/sri"
$results += Test-Http "Financiero Content/File health" "$FinancialBaseUrl/health/content-file"
$results += Test-Http "Financiero Portal readiness" "$FinancialBaseUrl/api/financial/portal-integration/readiness" $devHeaders
$results += Test-Http "Portal Gateway/Shell" "$PortalBaseUrl/health"

$results | Format-Table -AutoSize

if ($results.status -contains "FAIL") {
    exit 1
}

exit 0
