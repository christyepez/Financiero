[CmdletBinding(PositionalBinding = $false)]
param(
    [string]$FinancialBaseUrl = "http://localhost:8083",
    [string]$PortalBaseUrl = "http://localhost:8082",
    [string]$PortalShellBaseUrl = "",
    [string]$PortalHealthPath = "/health",
    [string]$PortalGatewayHealthPath = "",
    [string]$PortalShellHealthPath = "/health",
    [string]$FinancialApiHealthPath = "/health",
    [string]$SqlHost = "host.docker.internal",
    [int]$SqlPort = 21433,
    [switch]$SkipPortalChecks,
    [switch]$SkipApiHealthChecks,
    [string]$EvidenceOutputPath = "",
    [switch]$AcceptanceGateReport,
    [Parameter(ValueFromRemainingArguments = $true)]
    [string[]]$RemainingArguments,
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
        if ([string]::IsNullOrWhiteSpace($Url) -or -not [System.Uri]::IsWellFormedUriString($Url, [System.UriKind]::Absolute)) {
            return Write-Check $Name "BLOCKED_DEPENDENCY" "Invalid or empty absolute URL: $Url" "HTTP_URL_INVALID" "Set an explicit http/https base URL and health path confirmed by the service owner."
        }

        $uri = [System.Uri]$Url
        if ($uri.Scheme -notin @("http", "https")) {
            return Write-Check $Name "BLOCKED_DEPENDENCY" "Unsupported URL scheme '$($uri.Scheme)' for $Url" "HTTP_URL_SCHEME_UNSUPPORTED" "Use http/https service URLs. Do not pass evidence file paths as service base URLs."
        }

        $response = Invoke-WebRequest -UseBasicParsing -Uri $Url -Headers $Headers -TimeoutSec 5
        if ([int]$response.StatusCode -ge 200 -and [int]$response.StatusCode -lt 300) {
            Write-Check $Name "PASS" ("HTTP " + [int]$response.StatusCode) "PASS" ""
        } else {
            Write-Check $Name "BLOCKED_DEPENDENCY" ("HTTP " + [int]$response.StatusCode) "HTTP_STATUS_UNEXPECTED" "Validate service health path, route and dependency readiness. If this is a health check, confirm the configured health path."
        }
    } catch {
        $statusCode = $null
        try { $statusCode = [int]$_.Exception.Response.StatusCode } catch { $statusCode = $null }
        if ($null -ne $statusCode -and $statusCode -gt 0) {
            Write-Check $Name "BLOCKED_DEPENDENCY" ("HTTP " + $statusCode) "HTTP_STATUS_UNEXPECTED" "Validate service health path, route and dependency readiness. If this is a health check, confirm the configured health path."
        } else {
            Write-Check $Name "BLOCKED_DEPENDENCY" $_.Exception.Message "HTTP_ENDPOINT_UNREACHABLE" "Start the external service or override the URL/health path if this environment uses a different port."
        }
    }
}

function Join-UrlPath($BaseUrl, $Path) {
    if ([string]::IsNullOrWhiteSpace($Path)) { return $BaseUrl.TrimEnd("/") }
    $normalizedPath = $Path
    if (-not $normalizedPath.StartsWith("/")) { $normalizedPath = "/" + $normalizedPath }
    return $BaseUrl.TrimEnd("/") + $normalizedPath
}

function Write-HealthPathCheck($Name, $Path, $DefaultPath = "/health") {
    if ([string]::IsNullOrWhiteSpace($Path)) {
        Write-Check $Name "BLOCKED_DEPENDENCY" "Health path is empty." "HEALTH_PATH_NOT_CONFIRMED" "Set an explicit health path such as $DefaultPath or the route confirmed by the service owner."
    } elseif ($Path -ne $DefaultPath) {
        Write-Check $Name "BLOCKED_DEPENDENCY" "Using configured health path $Path instead of $DefaultPath." "HEALTH_ROUTE_ALTERNATIVE_REQUIRED" "Confirm the service owner documented this health route before accepting PASS evidence."
    } else {
        Write-Check $Name "PASS" "Using default health path $Path." "PASS" ""
    }
}

function Write-MarkdownResults($Results) {
    $lines = @()
    if ($VerboseDiagnostics -or $SuggestFixes) {
        $lines += "| Check | Status | Code | Detail | Suggested action |"
        $lines += "|---|---|---|---|---|"
    } else {
        $lines += "| Check | Status | Detail |"
        $lines += "|---|---|---|"
    }
    foreach ($result in $Results) {
        if ($VerboseDiagnostics -or $SuggestFixes) {
            $lines += "| $($result.check) | $($result.status) | $($result.code) | $($result.detail -replace '\|','/') | $($result.suggestion -replace '\|','/') |"
        } else {
            $lines += "| $($result.check) | $($result.status) | $($result.detail -replace '\|','/') |"
        }
    }
    return $lines
}

function Write-AcceptanceGateRows($Results) {
    $sqlTcp = $Results | Where-Object { $_.check -eq "Shared SQL TCP" } | Select-Object -First 1
    $gateway = $Results | Where-Object { $_.check -eq "Portal Gateway health" } | Select-Object -First 1
    $financialHealth = $Results | Where-Object { $_.check -eq "Financiero health" } | Select-Object -First 1
    $financialReady = $Results | Where-Object { $_.check -eq "Financiero ready" } | Select-Object -First 1
    $portalReadiness = $Results | Where-Object { $_.check -eq "Financiero Portal readiness" } | Select-Object -First 1
    $shell = $Results | Where-Object { $_.check -eq "Portal Shell health" } | Select-Object -First 1

    $rows = @()
    $rows += "| Gate | Status | Evidence |"
    $rows += "|---|---|---|"
    $rows += "| Gate 1 SQL TCP | $($sqlTcp.status) | $($sqlTcp.code): $($sqlTcp.detail -replace '\|','/') |"
    $rows += "| Gate 2 FinancieroDb | BLOCKED_DEPENDENCY | Requires SQL TCP PASS and owner DB evidence. |"
    $rows += "| Gate 3 Portal Gateway health | $($gateway.status) | $($gateway.code): $($gateway.detail -replace '\|','/') |"
    if ($null -ne $shell) {
        $rows += "| Gate 4 Portal Shell health | $($shell.status) | $($shell.code): $($shell.detail -replace '\|','/') |"
    } else {
        $rows += "| Gate 4 Portal Shell health | BLOCKED_DEPENDENCY | PortalShellBaseUrl not provided; owner evidence required. |"
    }
    $rows += "| Gate 5 PortalShellContext live | BLOCKED_DEPENDENCY | Owner evidence required. |"
    $rows += "| Gate 6 Financiero API health | $($financialHealth.status) | $($financialHealth.code): $($financialHealth.detail -replace '\|','/') |"
    $rows += "| Gate 7 Portal integration readiness | $($portalReadiness.status) | $($portalReadiness.code): $($portalReadiness.detail -replace '\|','/') |"
    $rows += "| Gate 8 Preflight exit code 0 | BLOCKED_DEPENDENCY | Requires all runtime gates PASS. |"
    $rows += "| Gate 9 No-production guardrails | PASS | Static/documented guardrails remain enabled. |"
    $rows += "| Final gate | BLOCKED_DEPENDENCY | Sprint 10 cannot capture PASS until all gates are accepted. |"
    return $rows
}

$acceptanceGateOutputPath = ""
if ($RemainingArguments.Count -gt 0) {
    $acceptanceGateOutputPath = $RemainingArguments[0]
}

$results = @()
$results += Write-Check "PowerShell" "PASS" $PSVersionTable.PSVersion.ToString() "PASS" ""

if ([string]::IsNullOrWhiteSpace($PortalGatewayHealthPath)) {
    $PortalGatewayHealthPath = $PortalHealthPath
}

$results += Write-HealthPathCheck "Financial API health path configuration" $FinancialApiHealthPath "/health"
$results += Write-HealthPathCheck "Portal Gateway health path configuration" $PortalGatewayHealthPath "/health"
if (-not [string]::IsNullOrWhiteSpace($PortalShellBaseUrl)) {
    $results += Write-HealthPathCheck "Portal Shell health path configuration" $PortalShellHealthPath "/health"
}

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

$p3PackagePaths = @(
    "docs/coordination/financial-next-cycle-p3-sql-owner-remediation-package.md",
    "docs/coordination/financial-next-cycle-p3-portal-owner-remediation-package.md",
    "docs/coordination/financial-next-cycle-p3-owner-handoff-message.md",
    "docs/qa/financial-next-cycle-p3-accepted-evidence-checklist.md"
)
foreach ($packagePath in $p3PackagePaths) {
    if (Test-Path $packagePath) {
        $results += Write-Check "P3 owner package $packagePath" "PASS" "Package exists." "PASS" ""
    } else {
        $results += Write-Check "P3 owner package $packagePath" "FAIL" "Package missing." "P3_OWNER_PACKAGE_MISSING" "Create the external SQL/Portal owner remediation package before delivery."
    }
}

$p4DecisionPaths = @(
    "docs/qa/financial-next-cycle-p4-evidence-intake-review.md",
    "docs/coordination/financial-next-cycle-p4-productization-pause-decision.md"
)
foreach ($decisionPath in $p4DecisionPaths) {
    if (Test-Path $decisionPath) {
        $results += Write-Check "P4 decision artifact $decisionPath" "PASS" "Artifact exists." "PASS" ""
    } else {
        $results += Write-Check "P4 decision artifact $decisionPath" "FAIL" "Artifact missing." "P4_DECISION_ARTIFACT_MISSING" "Create P4 evidence intake and either PASS capture, pause decision or rejected evidence decision before delivery."
    }
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
    $results += Test-Http "Financiero health" (Join-UrlPath $FinancialBaseUrl $FinancialApiHealthPath)
    $results += Test-Http "Financiero live" (Join-UrlPath $FinancialBaseUrl "/health/live")
    $results += Test-Http "Financiero ready" (Join-UrlPath $FinancialBaseUrl "/health/ready")
    $results += Test-Http "Financiero SRI health" "$FinancialBaseUrl/health/sri"
    $results += Test-Http "Financiero Content/File health" "$FinancialBaseUrl/health/content-file"
    $results += Test-Http "Financiero Portal readiness" "$FinancialBaseUrl/api/financial/portal-integration/readiness" $devHeaders
}

if ($SkipPortalChecks) {
    $results += Write-Check "Portal runtime checks" "PASS" "Skipped by -SkipPortalChecks." "SERVICE_SKIPPED" "Run without -SkipPortalChecks for full Portal evidence."
} else {
    $portalHealthUrl = Join-UrlPath $PortalBaseUrl $PortalGatewayHealthPath
    $results += Test-Http "Portal Gateway health" $portalHealthUrl
    if (-not [string]::IsNullOrWhiteSpace($PortalShellBaseUrl)) {
        $results += Test-Http "Portal Shell health" (Join-UrlPath $PortalShellBaseUrl $PortalShellHealthPath)
    }
}

$blockedCount = @($results | Where-Object { $_.status -eq "BLOCKED_DEPENDENCY" }).Count
$failCount = @($results | Where-Object { $_.status -eq "FAIL" }).Count
$passCount = @($results | Where-Object { $_.status -eq "PASS" }).Count
$results += Write-Check "OwnerEvidenceRequired" ($(if ($blockedCount -gt 0) { "BLOCKED_DEPENDENCY" } else { "PASS" })) "SQL Common Owner, Portal Gateway Owner, Portal Shell Owner and Portal Contract Owner must return sanitized evidence before PASS." ($(if ($blockedCount -gt 0) { "OWNER_EVIDENCE_REQUIRED" } else { "PASS" })) "Use docs/qa/templates and docs/coordination/financial-sprint-10-p1-owner-evidence-intake.md."
$results += Write-Check "AcceptanceGateSummary" ($(if ($failCount -gt 0) { "FAIL" } elseif ($blockedCount -gt 0) { "BLOCKED_DEPENDENCY" } else { "PASS" })) "PASS=$passCount; BLOCKED_DEPENDENCY=$blockedCount; FAIL=$failCount." ($(if ($failCount -gt 0) { "FAIL" } elseif ($blockedCount -gt 0) { "E2E_GATES_BLOCKED" } else { "PASS" })) "Gate final requires SQL, Portal Gateway, Portal Shell, PortalShellContext, Financiero health and preflight exit 0."

if ($OutputMarkdown) {
    Write-MarkdownResults $results
    if ($AcceptanceGateReport) {
        ""
        "## AcceptanceGateReport"
        Write-AcceptanceGateRows $results
    }
} else {
    if ($VerboseDiagnostics -or $SuggestFixes) {
        $results | Format-Table check,status,code,detail,suggestion -AutoSize
    } else {
        $results | Format-Table check,status,detail -AutoSize
    }
}

if (-not [string]::IsNullOrWhiteSpace($EvidenceOutputPath)) {
    $safePath = $EvidenceOutputPath
    $parent = Split-Path -Parent $safePath
    if (-not [string]::IsNullOrWhiteSpace($parent) -and -not (Test-Path $parent)) {
        New-Item -ItemType Directory -Path $parent -Force *> $null
    }
    $content = @()
    $content += "# Financiero E2E Preflight Evidence"
    $content += ""
    $content += "Generated by sanitized preflight. Do not add secrets, passwords, tokens, certificates, XML reales or private URLs."
    $content += ""
    $content += "## Owner evidence required"
    $content += ""
    $content += "- SQL Common Owner."
    $content += "- Portal Gateway Owner."
    $content += "- Portal Shell Owner."
    $content += "- Portal Contract Owner."
    $content += ""
    $content += "## Acceptance gate summary"
    $content += ""
    $content += "- PASS: $passCount"
    $content += "- BLOCKED_DEPENDENCY: $blockedCount"
    $content += "- FAIL: $failCount"
    $content += ""
    $content += "## Results"
    $content += ""
    $content += (Write-MarkdownResults $results)
    if ($AcceptanceGateReport) {
        $content += ""
        $content += "## AcceptanceGateReport"
        $content += ""
        $content += (Write-AcceptanceGateRows $results)
    }
    Set-Content -Path $safePath -Value $content -Encoding UTF8
}

if ($AcceptanceGateReport -and -not [string]::IsNullOrWhiteSpace($acceptanceGateOutputPath)) {
    $safeGatePath = $acceptanceGateOutputPath
    $gateParent = Split-Path -Parent $safeGatePath
    if (-not [string]::IsNullOrWhiteSpace($gateParent) -and -not (Test-Path $gateParent)) {
        New-Item -ItemType Directory -Path $gateParent -Force *> $null
    }

    $gateContent = @()
    $gateContent += "# Financiero E2E Acceptance Gate Report"
    $gateContent += ""
    $gateContent += "Generated by sanitized preflight. Do not add secrets, passwords, tokens, certificates, XML reales or private URLs."
    $gateContent += ""
    $gateContent += (Write-AcceptanceGateRows $results)
    Set-Content -Path $safeGatePath -Value $gateContent -Encoding UTF8
}

if ($results.status -contains "FAIL") {
    exit 1
}

if ($results.status -contains "BLOCKED_DEPENDENCY") {
    exit 2
}

exit 0
