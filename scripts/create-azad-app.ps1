<#
.SYNOPSIS
  Creates/updates an Entra ID app with Graph application permissions needed for password reset.
  NOTE: You must be a tenant admin to consent application permissions.
#>
param(
  [string]$AppName = "BromcomReset-API"
)

Write-Host "This script is an outline. Use Azure Portal or MS Graph PowerShell to:" -ForegroundColor Yellow
Write-Host " 1) Create App Registration named $AppName" -ForegroundColor Yellow
Write-Host " 2) Add Graph Application permissions: User.ReadWrite.All (grant admin consent)" -ForegroundColor Yellow
Write-Host " 3) Create a client secret and copy TenantId/ClientId/Secret to appsettings" -ForegroundColor Yellow
Write-Host " 4) (Optional) Create a Helpdesk security group and set its Object ID in Reset:AllowedGroupObjectId" -ForegroundColor Yellow
