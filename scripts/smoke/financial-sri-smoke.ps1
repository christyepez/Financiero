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
Invoke-RestMethod -Method Get -Uri "$BaseUrl/health/sri" | Out-Null
Invoke-RestMethod -Method Get -Uri "$BaseUrl/health/content-file" | Out-Null

$suffix = Get-Date -Format "HHmmss"
$invoice = PostJson "$BaseUrl/api/financial/electronic-documents/invoices" @{
    issueDate = "2026-01-15"
    customerIdentificationType = "04"
    customerIdentification = "1790012345001"
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
$validation = PostJson "$BaseUrl/api/financial/electronic-documents/$id/validate-xml" @{}
if (-not $validation.data.isValid) {
    throw "SRI XML validation failed."
}
PostJson "$BaseUrl/api/financial/electronic-documents/$id/sign" @{} | Out-Null
PostJson "$BaseUrl/api/financial/electronic-documents/$id/send" @{} | Out-Null
$authorized = PostJson "$BaseUrl/api/financial/electronic-documents/$id/authorize" @{}
$ride = PostJson "$BaseUrl/api/financial/electronic-documents/$id/generate-ride" @{}
if (-not $ride.data.ridePdfStorageId) {
    throw "RIDE storage metadata was not registered."
}
$storedRide = PostJson "$BaseUrl/api/financial/electronic-documents/$id/store-ride" @{}
if (-not $storedRide.data.ridePdfStorageId -or $storedRide.data.ridePdfStorageId -notmatch "^dev://|^portal-content-file://") {
    throw "RIDE Content/File storage readiness metadata was not registered."
}

$accessKey = $authorized.data.accessKey
if (-not $accessKey -or $accessKey.Length -ne 49) {
    throw "Invalid access key returned by SRI smoke."
}

$lookup = Invoke-RestMethod -Method Get -Uri "$BaseUrl/api/financial/electronic-documents/by-access-key/$accessKey" -Headers $headers
if ($lookup.data.id -ne $id) {
    throw "Lookup by access key did not return the created document."
}
$status = Invoke-RestMethod -Method Get -Uri "$BaseUrl/api/financial/electronic-documents/$id/status" -Headers $headers
if ($status.data.status -ne "Authorized") {
    throw "Unexpected document status: $($status.data.status)"
}
$storage = Invoke-RestMethod -Method Get -Uri "$BaseUrl/api/financial/electronic-documents/$id/storage-metadata" -Headers $headers
if (-not $storage.data.unsignedXmlStorageId -or -not $storage.data.signedXmlStorageId -or -not $storage.data.authorizationXmlStorageId -or -not $storage.data.ridePdfStorageId) {
    throw "Storage metadata placeholder was not registered."
}
$rideMetadata = Invoke-RestMethod -Method Get -Uri "$BaseUrl/api/financial/electronic-documents/$id/ride-metadata" -Headers $headers
if (-not $rideMetadata.data.ridePdfHash) {
    throw "RIDE metadata endpoint failed."
}
$ridePreview = Invoke-RestMethod -Method Get -Uri "$BaseUrl/api/financial/electronic-documents/$id/ride-preview" -Headers $headers
if (($ridePreview | ConvertTo-Json -Depth 10) -match "<factura|claveAcceso>|PRIVATE KEY|BEGIN CERTIFICATE|1790012345001") {
    throw "RIDE preview exposed XML, certificate material, or full customer identification."
}
$rideLegalReadiness = Invoke-RestMethod -Method Get -Uri "$BaseUrl/api/financial/electronic-documents/$id/ride-legal-readiness" -Headers $headers
if (-not $rideLegalReadiness.data.disclaimer -or $rideLegalReadiness.data.disclaimer -notmatch "not a legal final RIDE") { throw "RIDE legal readiness disclaimer is missing." }
if (($rideLegalReadiness | ConvertTo-Json -Depth 20) -match "<factura|PRIVATE KEY|BEGIN CERTIFICATE|$accessKey") { throw "RIDE legal readiness exposed sensitive payload." }

$readiness = Invoke-RestMethod -Method Get -Uri "$BaseUrl/api/financial/electronic-documents/sri/readiness" -Headers $headers
if ($readiness.data.status -ne "Healthy") {
    throw "Unexpected SRI readiness status: $($readiness.data.status)"
}
$contentFileReadiness = Invoke-RestMethod -Method Get -Uri "$BaseUrl/api/financial/electronic-documents/content-file/readiness" -Headers $headers
if ($contentFileReadiness.data.status -notin @("Healthy", "Degraded")) {
    throw "Unexpected Content/File readiness status: $($contentFileReadiness.data.status)"
}
if (($contentFileReadiness | ConvertTo-Json -Depth 10) -match "<factura|claveAcceso>|PRIVATE KEY|BEGIN CERTIFICATE|1790012345001") {
    throw "Content/File readiness exposed sensitive payload."
}
$probe = Invoke-RestMethod -Method Get -Uri "$BaseUrl/api/financial/electronic-documents/sri/connectivity-probe" -Headers $headers
if (-not $probe.data.status) {
    throw "SRI connectivity probe endpoint failed."
}
if (($probe | ConvertTo-Json -Depth 10) -match "<factura|claveAcceso>|PRIVATE KEY|BEGIN CERTIFICATE") {
    throw "SRI connectivity probe exposed sensitive payload."
}

$creditNote = PostJson "$BaseUrl/api/financial/electronic-documents/credit-notes" @{
    issueDate = "2026-01-16"
    customerIdentificationType = "04"
    customerIdentification = "1790012345001"
    customerName = "Cliente Smoke NC $suffix"
    relatedDocumentTypeCode = "01"
    relatedDocumentNumber = "001-001-000000001"
    relatedDocumentIssueDate = "2026-01-15"
    reason = "Devolución smoke"
    currency = "USD"
    establishmentCode = "001"
    emissionPointCode = "001"
}
$creditNoteId = $creditNote.data.id
PostJson "$BaseUrl/api/financial/electronic-documents/$creditNoteId/credit-note-lines" @{
    productCode = "NC-$suffix"
    description = "Nota de crédito smoke"
    quantity = 1
    unitPrice = 1
    discount = 0
} | Out-Null
$creditGenerated = PostJson "$BaseUrl/api/financial/electronic-documents/$creditNoteId/generate-credit-note-xml" @{}
if ($creditGenerated.data.accessKey.Substring(8, 2) -ne "04") { throw "Credit note access key codDoc is invalid." }
PostJson "$BaseUrl/api/financial/electronic-documents/$creditNoteId/generate-ride" @{} | Out-Null
$creditPreview = Invoke-RestMethod -Method Get -Uri "$BaseUrl/api/financial/electronic-documents/$creditNoteId/ride-preview" -Headers $headers
if (($creditPreview | ConvertTo-Json -Depth 10) -match "<notaCredito|claveAcceso>|PRIVATE KEY|BEGIN CERTIFICATE") { throw "Credit note RIDE preview exposed sensitive payload." }
$creditReadiness = Invoke-RestMethod -Method Get -Uri "$BaseUrl/api/financial/electronic-documents/$creditNoteId/ride-legal-readiness" -Headers $headers
if (-not $creditReadiness.data.disclaimer) { throw "Credit note RIDE legal readiness failed." }

$debitNote = PostJson "$BaseUrl/api/financial/electronic-documents/debit-notes" @{
    issueDate = "2026-01-16"
    customerIdentificationType = "04"
    customerIdentification = "1790012345001"
    customerName = "Cliente Smoke ND $suffix"
    relatedDocumentTypeCode = "01"
    relatedDocumentNumber = "001-001-000000001"
    relatedDocumentIssueDate = "2026-01-15"
    currency = "USD"
    establishmentCode = "001"
    emissionPointCode = "001"
}
$debitNoteId = $debitNote.data.id
PostJson "$BaseUrl/api/financial/electronic-documents/$debitNoteId/debit-note-reasons" @{ reason = "Interés smoke"; amount = 1.25 } | Out-Null
$debitGenerated = PostJson "$BaseUrl/api/financial/electronic-documents/$debitNoteId/generate-debit-note-xml" @{}
if ($debitGenerated.data.accessKey.Substring(8, 2) -ne "05") { throw "Debit note access key codDoc is invalid." }
PostJson "$BaseUrl/api/financial/electronic-documents/$debitNoteId/generate-ride" @{} | Out-Null
$debitPreview = Invoke-RestMethod -Method Get -Uri "$BaseUrl/api/financial/electronic-documents/$debitNoteId/ride-preview" -Headers $headers
if (($debitPreview | ConvertTo-Json -Depth 10) -match "<notaDebito|claveAcceso>|PRIVATE KEY|BEGIN CERTIFICATE") { throw "Debit note RIDE preview exposed sensitive payload." }
$debitReadiness = Invoke-RestMethod -Method Get -Uri "$BaseUrl/api/financial/electronic-documents/$debitNoteId/ride-legal-readiness" -Headers $headers
if (-not $debitReadiness.data.disclaimer) { throw "Debit note RIDE legal readiness failed." }

$withholding = PostJson "$BaseUrl/api/financial/electronic-documents/withholdings" @{
    issueDate = "2026-01-16"
    subjectIdentificationType = "04"
    subjectIdentification = "1790012345001"
    subjectName = "Proveedor Smoke $suffix"
    fiscalPeriod = "01/2026"
    supportDocumentTypeCode = "01"
    supportDocumentNumber = "001-001-000000001"
    supportDocumentIssueDate = "2026-01-15"
    currency = "USD"
    establishmentCode = "001"
    emissionPointCode = "001"
}
$withholdingId = $withholding.data.id
PostJson "$BaseUrl/api/financial/electronic-documents/$withholdingId/withholding-taxes" @{
    taxCode = "1"
    withholdingCode = "312"
    taxBase = 100
    withholdingPercentage = 1.75
    withheldAmount = 1.75
    supportDocumentNumber = "001-001-000000001"
    supportDocumentIssueDate = "2026-01-15"
    fiscalPeriod = "01/2026"
} | Out-Null
$withholdingGenerated = PostJson "$BaseUrl/api/financial/electronic-documents/$withholdingId/generate-withholding-xml" @{}
if ($withholdingGenerated.data.accessKey.Substring(8, 2) -ne "07") { throw "Withholding access key codDoc is invalid." }
PostJson "$BaseUrl/api/financial/electronic-documents/$withholdingId/generate-ride" @{} | Out-Null
$withholdingPreview = Invoke-RestMethod -Method Get -Uri "$BaseUrl/api/financial/electronic-documents/$withholdingId/ride-preview" -Headers $headers
if (($withholdingPreview | ConvertTo-Json -Depth 10) -match "<comprobanteRetencion|claveAcceso>|PRIVATE KEY|BEGIN CERTIFICATE") { throw "Withholding RIDE preview exposed sensitive payload." }
$withholdingReadiness = Invoke-RestMethod -Method Get -Uri "$BaseUrl/api/financial/electronic-documents/$withholdingId/ride-legal-readiness" -Headers $headers
if (-not $withholdingReadiness.data.disclaimer) { throw "Withholding RIDE legal readiness failed." }

$reportSummary = Invoke-RestMethod -Method Get -Uri "$BaseUrl/api/financial/tax-reporting/summary?startDate=2026-01-01&endDate=2026-01-31" -Headers $headers
if (-not $reportSummary.data.totals -or $reportSummary.data.totals.count -lt 4) {
    throw "Tax reporting summary did not include smoke documents."
}
$actionQueue = Invoke-RestMethod -Method Get -Uri "$BaseUrl/api/financial/tax-reporting/action-queue?startDate=2026-01-01&endDate=2026-01-31" -Headers $headers
if (-not $actionQueue.data) { throw "Tax reporting action queue endpoint failed." }
$monthlySummary = Invoke-RestMethod -Method Get -Uri "$BaseUrl/api/financial/tax-reporting/monthly-summary?startDate=2026-01-01&endDate=2026-01-31" -Headers $headers
if (-not $monthlySummary.data) { throw "Tax reporting monthly summary endpoint failed." }
$exportJson = Invoke-RestMethod -Method Get -Uri "$BaseUrl/api/financial/tax-reporting/export?from=2026-01-01&to=2026-01-31&format=Json" -Headers $headers
$exportCsvResponse = Invoke-WebRequest -UseBasicParsing -Method Get -Uri "$BaseUrl/api/financial/tax-reporting/export?from=2026-01-01&to=2026-01-31&format=Csv" -Headers $headers
$storedExportJson = Invoke-RestMethod -Method Post -Uri "$BaseUrl/api/financial/tax-reporting/export/store?from=2026-01-01&to=2026-01-31&format=Json" -Headers $headers
$storedExportCsv = Invoke-RestMethod -Method Post -Uri "$BaseUrl/api/financial/tax-reporting/export/store?from=2026-01-01&to=2026-01-31&format=Csv" -Headers $headers
if ($storedExportJson.data.storedFile.storageId -notmatch "^dev://tax-export|^portal-content-file://") { throw "Stored JSON export did not return storage metadata." }
if ($storedExportCsv.data.storedFile.storageId -notmatch "^dev://tax-export|^portal-content-file://") { throw "Stored CSV export did not return storage metadata." }
$atsReadiness = Invoke-RestMethod -Method Get -Uri "$BaseUrl/api/financial/tax-reporting/ats-readiness?period=2026-01" -Headers $headers
if (-not $atsReadiness.data.disclaimer -or $atsReadiness.data.disclaimer -notmatch "not an official ATS") { throw "ATS readiness disclaimer is missing." }
$atsOfficialDesign = Invoke-RestMethod -Method Get -Uri "$BaseUrl/api/financial/tax-reporting/ats-official-design?period=2026-01" -Headers $headers
if (-not $atsOfficialDesign.data.disclaimer -or $atsOfficialDesign.data.disclaimer -notmatch "not an official ATS") { throw "ATS official design disclaimer is missing." }
if (-not ($atsOfficialDesign.data.sections | Where-Object { $_.code -eq "purchases" })) { throw "ATS official design purchases section is missing." }
$reportPayload = $reportSummary | ConvertTo-Json -Depth 20
if (($reportPayload + ($exportJson | ConvertTo-Json -Depth 20) + $exportCsvResponse.Content + ($storedExportJson | ConvertTo-Json -Depth 20) + ($storedExportCsv | ConvertTo-Json -Depth 20) + ($atsReadiness | ConvertTo-Json -Depth 20) + ($atsOfficialDesign | ConvertTo-Json -Depth 20)) -match "<factura|<notaCredito|<notaDebito|<comprobanteRetencion|PRIVATE KEY|BEGIN CERTIFICATE|$accessKey") {
    throw "Tax reporting exposed XML or certificate material."
}

$integrationStatus = Invoke-RestMethod -Method Get -Uri "$BaseUrl/api/financial/electronic-documents/$id/integration-status" -Headers $headers
if ($integrationStatus.data.accessKey -eq $accessKey) {
    throw "Integration status exposed full access key."
}
if ($integrationStatus.data.customerIdentification -eq $invoice.customerIdentification) {
    throw "Integration status exposed full customer identification."
}

Write-Host "Financial SRI smoke OK. Invoice=$id NC=$creditNoteId ND=$debitNoteId RET=$withholdingId AccessKey=$accessKey Status=$($authorized.data.status)"
