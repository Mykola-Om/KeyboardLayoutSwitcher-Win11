Add-Type -AssemblyName System.Drawing
$src = [System.Drawing.Image]::FromFile("icon.png")

$bmp = New-Object System.Drawing.Bitmap 256, 256
$g = [System.Drawing.Graphics]::FromImage($bmp)
$g.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
$g.DrawImage($src, 0, 0, 256, 256)

$ms = New-Object System.IO.MemoryStream
$bmp.Save($ms, [System.Drawing.Imaging.ImageFormat]::Png)
$pngBytes = $ms.ToArray()

$fs = [System.IO.File]::Create("icon.ico")
$bw = New-Object System.IO.BinaryWriter $fs

$bw.Write([ushort]0)
$bw.Write([ushort]1)
$bw.Write([ushort]1)

$bw.Write([byte]0)
$bw.Write([byte]0)
$bw.Write([byte]0)
$bw.Write([byte]0)
$bw.Write([ushort]1)
$bw.Write([ushort]32)
$bw.Write([uint]$pngBytes.Length)
$bw.Write([uint]22)

$bw.Write($pngBytes)

$bw.Close()
$fs.Close()
$ms.Close()
$g.Dispose()
$bmp.Dispose()
$src.Dispose()
