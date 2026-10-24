param(
    [string]$OutputRoot = ''
)

$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot
if ([string]::IsNullOrWhiteSpace($OutputRoot)) {
    $OutputRoot = Join-Path $repoRoot 'NumberSlideUnity\Assets\Resources\BoardLayouts'
}

$boardPixels = 1024
$pixelsPerUnit = 100
$boardWorldUnits = $boardPixels / $pixelsPerUnit
$boardGapPixels = 12
$boardGapWorldUnits = $boardGapPixels / $pixelsPerUnit

$tileSpecs = @(
    [pscustomobject]@{ Grid = 3; TilePixels = 312 },
    [pscustomobject]@{ Grid = 4; TilePixels = 231 },
    [pscustomobject]@{ Grid = 5; TilePixels = 182 },
    [pscustomobject]@{ Grid = 6; TilePixels = 150 },
    [pscustomobject]@{ Grid = 7; TilePixels = 126 },
    [pscustomobject]@{ Grid = 8; TilePixels = 109 },
    [pscustomobject]@{ Grid = 9; TilePixels = 96 },
    [pscustomobject]@{ Grid = 10; TilePixels = 85 }
)

function Round-Position([double]$Value) {
    return [Math]::Round($Value, 4, [MidpointRounding]::AwayFromZero)
}

New-Item -ItemType Directory -Path $OutputRoot -Force | Out-Null

foreach ($spec in $tileSpecs) {
    $grid = [int]$spec.Grid
    $tilePixels = [int]$spec.TilePixels
    $tileWorldUnits = $tilePixels / $pixelsPerUnit
    $contentPixels = $grid * $tilePixels + ($grid - 1) * $boardGapPixels
    $offsetWorldUnits = (($boardPixels - $contentPixels) / 2.0) / $pixelsPerUnit
    $stepWorldUnits = $tileWorldUnits + $boardGapWorldUnits
    $worldCenter = $boardWorldUnits / 2.0

    $matrix = @()
    for ($row = 0; $row -lt $grid; $row++) {
        $rowCells = @()
        for ($column = 0; $column -lt $grid; $column++) {
            $x = $worldCenter - $offsetWorldUnits - $tileWorldUnits / 2.0 - $column * $stepWorldUnits
            $y = $worldCenter - $offsetWorldUnits - $tileWorldUnits / 2.0 - $row * $stepWorldUnits

            $rowCells += ,@(
                (Round-Position $x),
                (Round-Position $y)
            )
        }

        $matrix += ,$rowCells
    }

    $path = Join-Path $OutputRoot ('{0}x{0}.json' -f $grid)
    $json = ConvertTo-Json -InputObject $matrix -Depth 4
    Set-Content -LiteralPath $path -Value $json -Encoding UTF8
}

Write-Output "Generated board layouts in: $OutputRoot"
