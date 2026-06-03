<#
.SYNOPSIS
    Génère la structure de dossiers Clean Architecture pour un module CNSS.
.PARAMETER ModuleName
    Nom du module (ex: CommunicationInterne, Notification, Archivage)
.PARAMETER OutputPath
    Chemin de destination (ex: D:\J_Projetsd\AppMetier\cnss-metier\src)
.EXAMPLE
    .\New-CleanArchModule.ps1 -ModuleName CommunicationInterne -OutputPath D:\J_Projetsd\AppMetier\cnss-metier\src
#>
param(
    [Parameter(Mandatory)][string]$ModuleName,
    [Parameter(Mandatory)][string]$OutputPath
)

$prefix = "Cnss.Metier.$ModuleName"
$root   = Join-Path $OutputPath $ModuleName

$folders = @(
    "$prefix.Domain\Aggregats"
    "$prefix.Domain\ValueObjects"
    "$prefix.Domain\Events"
    "$prefix.Domain\Services"
    "$prefix.Domain\Repositories"
    "$prefix.Domain\Enums"
    "$prefix.Application\Commands"
    "$prefix.Application\Queries"
    "$prefix.Application\Common"
    "$prefix.Application\Ports"
    "$prefix.Infrastructure\Persistence\Configurations"
    "$prefix.Infrastructure\Persistence\Migrations"
    "$prefix.Infrastructure\Persistence\Repositories"
    "$prefix.Infrastructure\Adapters"
)

foreach ($folder in $folders) {
    $path = Join-Path $root $folder
    New-Item -ItemType Directory -Path $path -Force | Out-Null
    Write-Host "  [OK] $path"
}

@"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
  </PropertyGroup>
</Project>
"@ | Set-Content (Join-Path $root "$prefix.Domain\$prefix.Domain.csproj")

@"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\$prefix.Domain\$prefix.Domain.csproj" />
  </ItemGroup>
</Project>
"@ | Set-Content (Join-Path $root "$prefix.Application\$prefix.Application.csproj")

@"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\$prefix.Application\$prefix.Application.csproj" />
    <ProjectReference Include="..\$prefix.Domain\$prefix.Domain.csproj" />
  </ItemGroup>
</Project>
"@ | Set-Content (Join-Path $root "$prefix.Infrastructure\$prefix.Infrastructure.csproj")

Write-Host ""
Write-Host "Structure '$ModuleName' créée dans : $root"
Write-Host "Projets générés : Domain, Application, Infrastructure"
