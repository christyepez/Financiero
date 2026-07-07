param(
    [string]$BaseUrl = "http://localhost:8083",
    [string]$Permissions = "financial.*"
)

$ErrorActionPreference = "Stop"
$headers = @{
    "X-Dev-Permissions" = $Permissions
    "X-Correlation-ID" = "financial-smoke-$([guid]::NewGuid().ToString('N'))"
}

function Invoke-FinancialJson {
    param([string]$Method, [string]$Path, [object]$Body = $null)
    $params = @{
        Method = $Method
        Uri = "$BaseUrl$Path"
        Headers = $headers
        ContentType = "application/json"
        UseBasicParsing = $true
    }
    if ($null -ne $Body) { $params.Body = ($Body | ConvertTo-Json -Depth 10) }
    $response = Invoke-WebRequest @params
    if ($response.Content) { return ($response.Content | ConvertFrom-Json) }
    return $null
}

Invoke-WebRequest -UseBasicParsing "$BaseUrl/health/live" | Out-Null
Invoke-WebRequest -UseBasicParsing "$BaseUrl/health/ready" | Out-Null
Invoke-WebRequest -UseBasicParsing "$BaseUrl/" | Out-Null

$suffix = (Get-Date).ToString("HHmmss")
$year = 2200 + (Get-Random -Minimum 1 -Maximum 200)
$parent = Invoke-FinancialJson POST "/api/financial/accounts/" @{ code = "SMK-$suffix"; name = "Smoke Activos"; type = "Asset"; level = 1; parentAccountId = $null; isMovementAccount = $false }
Invoke-FinancialJson POST "/api/financial/accounts/$($parent.data.id)/activate" | Out-Null
$cash = Invoke-FinancialJson POST "/api/financial/accounts/" @{ code = "SMK-$suffix-1"; name = "Smoke Caja"; type = "Asset"; level = 2; parentAccountId = $parent.data.id; isMovementAccount = $true }
Invoke-FinancialJson POST "/api/financial/accounts/$($cash.data.id)/activate" | Out-Null
$bank = Invoke-FinancialJson POST "/api/financial/accounts/" @{ code = "SMK-$suffix-2"; name = "Smoke Bancos"; type = "Asset"; level = 2; parentAccountId = $parent.data.id; isMovementAccount = $true }
Invoke-FinancialJson POST "/api/financial/accounts/$($bank.data.id)/activate" | Out-Null

$fy = Invoke-FinancialJson POST "/api/financial/fiscal-years/" @{ year = $year; startDate = "$year-01-01"; endDate = "$year-12-31" }
Invoke-FinancialJson POST "/api/financial/fiscal-years/$($fy.data.id)/open" | Out-Null
$period = Invoke-FinancialJson POST "/api/financial/fiscal-periods/" @{ fiscalYearId = $fy.data.id; periodNumber = 1; startDate = "$year-01-01"; endDate = "$year-01-31" }
Invoke-FinancialJson POST "/api/financial/fiscal-periods/$($period.data.id)/open" | Out-Null

$entry = Invoke-FinancialJson POST "/api/financial/journal-entries/" @{ postingDate = "$year-01-15"; source = "Manual"; reference = "SMOKE"; description = "Smoke entry" }
Invoke-FinancialJson POST "/api/financial/journal-entries/$($entry.data.id)/lines" @{ accountId = $cash.data.id; description = "Debit"; debit = 10; credit = 0 } | Out-Null
Invoke-FinancialJson POST "/api/financial/journal-entries/$($entry.data.id)/lines" @{ accountId = $bank.data.id; description = "Credit"; debit = 0; credit = 10 } | Out-Null
$posted = Invoke-FinancialJson POST "/api/financial/journal-entries/$($entry.data.id)/post"

if (-not $posted.data.entryNumber) { throw "Smoke failed: missing entry number." }
Write-Output "Financial smoke OK. EntryNumber=$($posted.data.entryNumber), FiscalPeriodId=$($posted.data.fiscalPeriodId)"
