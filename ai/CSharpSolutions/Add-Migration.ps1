param(
    [string]$MigrationName,
    [string]$StartupProjectPath = "./Recruiter/WebApi",
    [string]$ProjectPath = "./Recruiter/Infrastructure"
)

if (-not $MigrationName) {
    Write-Host "Migration name is required. Usage: .\Add-Migration.ps1 -MigrationName <MigrationName>"
    exit 1
}

# Check if dotnet-ef is installed
$toolInstalled = dotnet tool list -g | Select-String -Pattern "dotnet-ef"

if (-not $toolInstalled) {
    Write-Host "Installing dotnet-ef tool..."
    dotnet tool install --global dotnet-ef
} else {
    Write-Host "dotnet-ef is already installed."
}


# Add migration
dotnet ef migrations add $MigrationName --startup-project $StartupProjectPath --project $ProjectPath

Write-Host "Migration '$MigrationName' added and database updated successfully."