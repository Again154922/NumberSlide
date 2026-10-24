param(
    [string]$Configuration = 'Release'
)

$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot
$project = Join-Path $repoRoot 'NumberSlideConsole\NumberSlide.Core\NumberSlide.Core.csproj'
$output = Join-Path $repoRoot "NumberSlideConsole\NumberSlide.Core\bin\$Configuration\netstandard2.1"
$destination = Join-Path $repoRoot 'NumberSlideUnity\Assets\Plugins\NumberSlide.Core'

dotnet build $project -c $Configuration -f netstandard2.1
if ($LASTEXITCODE -ne 0) {
    throw "NumberSlide.Core 编译失败。"
}

$files = @(
    'NumberSlide.Core.dll',
    'NumberSlide.Core.pdb',
    'System.Text.Json.dll',
    'System.Text.Encodings.Web.dll',
    'Microsoft.Bcl.AsyncInterfaces.dll',
    'System.Runtime.CompilerServices.Unsafe.dll'
)

New-Item -ItemType Directory -Path $destination -Force | Out-Null

foreach ($file in $files) {
    $sourcePath = Join-Path $output $file
    if (-not (Test-Path -LiteralPath $sourcePath)) {
        throw "缺少构建产物: $sourcePath"
    }

    Copy-Item `
        -LiteralPath $sourcePath `
        -Destination (Join-Path $destination $file) `
        -Force
}

Write-Output "Unity Core updated: $destination"
