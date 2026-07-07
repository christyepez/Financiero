param(
    [string]$BaseUrl = "http://localhost:8083",
    [string]$Permissions = "financial.*"
)

$ErrorActionPreference = "Stop"
$headers = @{ "X-Dev-Permissions" = $Permissions; "Content-Type" = "application/json" }

function PostJson($url, $body) {
    Invoke-RestMethod -Method Post -Uri $url -Headers $headers -Body ($body | ConvertTo-Json -Depth 8)
}

Invoke-RestMethod -Method Get -Uri "$BaseUrl/health/ready" | Out-Null

$suffix = Get-Date -Format "HHmmss"
$invoice = PostJson "$BaseUrl/api/financial/electronic-documents/invoices" @{
    issueDate = "2026-01-15"
    customerIdentificationType = "04"
    customerIdentification = "0999999999001"
    customerName = "Cliente Smoke SRI $suffix"
    currency = "USD"
    establishmentCode = "001"
    emissionPointCode = "001"
}

$id = $invoice.data.id
PostJson "$BaseUrl/api/financial/electronic-documents/$id/lines" @{
    productCode = "SMK-$suffix"
    description = "Servicio smoke SRI"
    quantity = 1
    unitPrice = 10
    discount = 0
} | Out-Null

$generated = PostJson "$BaseUrl/api/financial/electronic-documents/$id/generate-xml" @{}
PostJson "$BaseUrl/api/financial/electronic-documents/$id/sign" @{} | Out-Null
PostJson "$BaseUrl/api/financial/electronic-documents/$id/send" @{} | Out-Null
$authorized = PostJson "$BaseUrl/api/financial/electronic-documents/$id/authorize" @{}

$accessKey = $authorized.data.accessKey
if (-not $accessKey -or $accessKey.Length -ne 49) {
    throw "Invalid access key returned by SRI smoke."
}

$lookup = Invoke-RestMethod -Method Get -Uri "$BaseUrl/api/financial/electronic-documents/by-access-key/$accessKey" -Headers $headers
if ($lookup.data.id -ne $id) {
    throw "Lookup by access key did not return the created document."
}

Write-Host "Financial SRI smoke OK. Id=$id AccessKey=$accessKey Status=$($authorized.data.status)"
