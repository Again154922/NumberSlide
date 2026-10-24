param(
    [string]$OutputRoot = ''
)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

$repoRoot = Split-Path -Parent $PSScriptRoot
if ([string]::IsNullOrWhiteSpace($OutputRoot)) {
    $OutputRoot = Join-Path $repoRoot 'NumberSlideUnity\Assets\Images'
}

$boardPixels = 1024
$boardGap = 12
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

$palette = @{
    BoardTop = '#C7B5A4'
    BoardBottom = '#A9917E'
    BoardBorder = '#7D6A5B'
    BoardSlot = '#785F4F'
    BoardSlotBorder = '#D9C8B7'
    TileTop = '#F1E3C9'
    TileBottom = '#D8C09E'
    TileBorder = '#B69E83'
    TileHighlight = '#FFF8E9'
}

function Get-Color([string]$Hex) {
    return [System.Drawing.ColorTranslator]::FromHtml($Hex)
}

function New-RoundedPath(
    [float]$X,
    [float]$Y,
    [float]$Width,
    [float]$Height,
    [float]$Radius) {
    $path = [System.Drawing.Drawing2D.GraphicsPath]::new()
    $diameter = [Math]::Max(1.0, $Radius * 2.0)

    $path.AddArc($X, $Y, $diameter, $diameter, 180, 90)
    $path.AddArc($X + $Width - $diameter, $Y, $diameter, $diameter, 270, 90)
    $path.AddArc(
        $X + $Width - $diameter,
        $Y + $Height - $diameter,
        $diameter,
        $diameter,
        0,
        90)
    $path.AddArc($X, $Y + $Height - $diameter, $diameter, $diameter, 90, 90)
    $path.CloseFigure()
    return $path
}

function Draw-RoundedPanel(
    [System.Drawing.Graphics]$Graphics,
    [float]$X,
    [float]$Y,
    [float]$Width,
    [float]$Height,
    [float]$Radius,
    [string]$TopColor,
    [string]$BottomColor,
    [string]$BorderColor,
    [float]$BorderWidth) {
    $path = New-RoundedPath $X $Y $Width $Height $Radius
    try {
        $rect = [System.Drawing.RectangleF]::new($X, $Y, $Width, $Height)
        $brush = [System.Drawing.Drawing2D.LinearGradientBrush]::new(
            $rect,
            (Get-Color $TopColor),
            (Get-Color $BottomColor),
            90.0)
        try {
            $Graphics.FillPath($brush, $path)
        }
        finally {
            $brush.Dispose()
        }

        if ($BorderWidth -gt 0) {
            $pen = [System.Drawing.Pen]::new((Get-Color $BorderColor), $BorderWidth)
            try {
                $pen.Alignment = [System.Drawing.Drawing2D.PenAlignment]::Inset
                $Graphics.DrawPath($pen, $path)
            }
            finally {
                $pen.Dispose()
            }
        }
    }
    finally {
        $path.Dispose()
    }
}

function New-Canvas([int]$Width, [int]$Height) {
    return [System.Drawing.Bitmap]::new(
        $Width,
        $Height,
        [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
}

function New-Graphics([System.Drawing.Bitmap]$Bitmap) {
    $graphics = [System.Drawing.Graphics]::FromImage($Bitmap)
    $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    $graphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
    $graphics.CompositingQuality = [System.Drawing.Drawing2D.CompositingQuality]::HighQuality
    $graphics.Clear([System.Drawing.Color]::Transparent)
    return $graphics
}

function Save-Board([string]$Path, [int]$Rows, [int]$TileSize) {
    $bitmap = New-Canvas $boardPixels $boardPixels
    $graphics = New-Graphics $bitmap
    try {
        Draw-RoundedPanel $graphics 14 18 996 992 54 '#16000000' '#16000000' '#00000000' 0
        Draw-RoundedPanel $graphics 8 8 1008 1008 54 $palette.BoardTop $palette.BoardBottom $palette.BoardBorder 4

        $contentPixels = $Rows * $TileSize + ($Rows - 1) * $boardGap
        $offset = [Math]::Floor(($boardPixels - $contentPixels) / 2.0)
        $slotRadius = [Math]::Max(8.0, [Math]::Round($TileSize * 0.12))

        for ($row = 0; $row -lt $Rows; $row++) {
            for ($column = 0; $column -lt $Rows; $column++) {
                $x = $offset + $column * ($TileSize + $boardGap)
                $y = $offset + $row * ($TileSize + $boardGap)

                Draw-RoundedPanel `
                    $graphics `
                    $x `
                    $y `
                    $TileSize `
                    $TileSize `
                    $slotRadius `
                    $palette.BoardSlot `
                    $palette.BoardSlot `
                    $palette.BoardSlotBorder `
                    2
            }
        }

        $bitmap.Save($Path, [System.Drawing.Imaging.ImageFormat]::Png)
    }
    finally {
        $graphics.Dispose()
        $bitmap.Dispose()
    }
}

function Save-Tile([string]$Path, [int]$Size) {
    $bitmap = New-Canvas $Size $Size
    $graphics = New-Graphics $bitmap
    try {
        $margin = [Math]::Max(2.0, [Math]::Round($Size * 0.045))
        $radius = [Math]::Max(8.0, [Math]::Round($Size * 0.14))
        $panelSize = $Size - $margin * 2.0

        Draw-RoundedPanel `
            $graphics `
            ($margin + 1) `
            ($margin + 3) `
            $panelSize `
            $panelSize `
            $radius `
            '#22000000' `
            '#22000000' `
            '#00000000' `
            0

        Draw-RoundedPanel `
            $graphics `
            $margin `
            $margin `
            $panelSize `
            $panelSize `
            $radius `
            $palette.TileTop `
            $palette.TileBottom `
            $palette.TileBorder `
            ([Math]::Max(1.0, $Size * 0.012))

        $inset = [Math]::Max(2.0, [Math]::Round($Size * 0.075))
        $highlightPath = New-RoundedPath `
            ($margin + $inset) `
            ($margin + $inset) `
            ($panelSize - $inset * 2.0) `
            ($panelSize - $inset * 2.0) `
            ([Math]::Max(4.0, $radius * 0.7))
        try {
            $pen = [System.Drawing.Pen]::new(
                (Get-Color $palette.TileHighlight),
                [Math]::Max(1.0, $Size * 0.01))
            try {
                $pen.Alignment = [System.Drawing.Drawing2D.PenAlignment]::Inset
                $graphics.DrawPath($pen, $highlightPath)
            }
            finally {
                $pen.Dispose()
            }
        }
        finally {
            $highlightPath.Dispose()
        }

        $bitmap.Save($Path, [System.Drawing.Imaging.ImageFormat]::Png)
    }
    finally {
        $graphics.Dispose()
        $bitmap.Dispose()
    }
}

New-Item -ItemType Directory -Path $OutputRoot -Force | Out-Null

$manifestEntries = @()
foreach ($spec in $tileSpecs) {
    $rows = [int]$spec.Grid
    $tileSize = [int]$spec.TilePixels
    $folderName = '{0}x{0}' -f $rows
    $folder = Join-Path $OutputRoot $folderName
    New-Item -ItemType Directory -Path $folder -Force | Out-Null

    $obsoleteNames = @('Tile.png', 'TileCorrect.png', 'TileHint.png')
    $obsoleteNames += Get-ChildItem -LiteralPath $folder -File -Filter 'Tile_*.png' |
        ForEach-Object { $_.Name }
    foreach ($obsoleteName in $obsoleteNames | Select-Object -Unique) {
        $obsoletePath = Join-Path $folder $obsoleteName
        $obsoleteMeta = "$obsoletePath.meta"
        if ([System.IO.File]::Exists($obsoletePath)) {
            [System.IO.File]::Delete($obsoletePath)
        }
        if ([System.IO.File]::Exists($obsoleteMeta)) {
            [System.IO.File]::Delete($obsoleteMeta)
        }
    }

    Save-Board (Join-Path $folder 'Board.png') $rows $tileSize
    Save-Tile (Join-Path $folder 'Tile.png') $tileSize

    $contentPixels = $rows * $tileSize + ($rows - 1) * $boardGap
    $manifestEntries += [ordered]@{
        grid = $rows
        folder = $folderName
        board = 'Board.png'
        tile = 'Tile.png'
        boardPixels = $boardPixels
        tilePixels = $tileSize
        gapPixels = $boardGap
        contentPixels = $contentPixels
        offsetPixels = [int][Math]::Floor(($boardPixels - $contentPixels) / 2.0)
    }
}

$manifest = [ordered]@{
    version = 3
    palette = $palette
    sizes = $manifestEntries
}

$manifest |
    ConvertTo-Json -Depth 5 |
    Set-Content -LiteralPath (Join-Path $OutputRoot 'asset-manifest.json') -Encoding UTF8

Write-Output "Generated Unity images in: $OutputRoot"
