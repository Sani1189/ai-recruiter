param(
    [string]$MigrationName = $null,
    [string]$StartupProjectPath = "./Recruiter/WebApi",
    [string]$ProjectPath = "./Recruiter/Infrastructure"
)


# Check if dotnet-ef is installed
$toolInstalled = dotnet tool list -g | Select-String -Pattern "dotnet-ef"

if (-not $toolInstalled) {
    Write-Host "Installing dotnet-ef tool..."
    dotnet tool install --global dotnet-ef
} else {
    Write-Host "dotnet-ef is already installed."
}


if ($MigrationName -eq $null) {
    dotnet ef database update --startup-project $StartupProjectPath --project $ProjectPath
}
else {
    dotnet ef database update $MigrationName --startup-project $StartupProjectPath --project $ProjectPath
}

Write-Host "Successfully run the Update."