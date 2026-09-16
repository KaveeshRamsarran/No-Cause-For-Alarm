# Native keyboard/mouse check of the built game's ordinary UI. No debug teleporting.
# This opens the interactive player and creates an ordinary local save file.
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing
Add-Type @'
using System;
using System.Runtime.InteropServices;
public static class GameInput {
 [StructLayout(LayoutKind.Sequential)] public struct Point { public int X; public int Y; }
 [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr window);
 [DllImport("user32.dll")] public static extern bool ClientToScreen(IntPtr window, ref Point point);
 [DllImport("user32.dll")] public static extern void keybd_event(byte key, byte scan, uint flags, UIntPtr extra);
 [DllImport("user32.dll")] public static extern void mouse_event(uint flags,uint x,uint y,uint data,UIntPtr extra);
}
'@
$projectPath = Split-Path -Parent $PSScriptRoot
$process = Start-Process -FilePath (Join-Path $projectPath 'Builds\Windows\NO CAUSE FOR ALARM.exe') -ArgumentList '-screen-fullscreen 0 -screen-width 1280 -screen-height 720 -logFile Logs/InputCheck.log' -WorkingDirectory $projectPath -PassThru
Start-Sleep -Seconds 5
$process.Refresh()
[GameInput]::SetForegroundWindow($process.MainWindowHandle) | Out-Null
$origin = New-Object GameInput+Point
[GameInput]::ClientToScreen($process.MainWindowHandle,[ref]$origin) | Out-Null
function Click-Game([int]$x,[int]$y) {
 [System.Windows.Forms.Cursor]::Position = New-Object System.Drawing.Point(($origin.X+$x),($origin.Y+$y))
 [GameInput]::mouse_event(2,0,0,0,[UIntPtr]::Zero)
 Start-Sleep -Milliseconds 80
 [GameInput]::mouse_event(4,0,0,0,[UIntPtr]::Zero)
 Start-Sleep -Milliseconds 250
}
function Press-Game([byte]$key,[int]$milliseconds=120) {
 [GameInput]::keybd_event($key,0,0,[UIntPtr]::Zero)
 Start-Sleep -Milliseconds $milliseconds
 [GameInput]::keybd_event($key,0,2,[UIntPtr]::Zero)
 Start-Sleep -Milliseconds 200
}
function Capture-Game([string]$name) {
 $bitmap=New-Object System.Drawing.Bitmap(1280,720)
 $graphics=[System.Drawing.Graphics]::FromImage($bitmap)
 $graphics.CopyFromScreen($origin.X,$origin.Y,0,0,$bitmap.Size)
 $bitmap.Save((Join-Path $projectPath ('Artifacts\'+$name+'.png')))
 $graphics.Dispose();$bitmap.Dispose()
}
Click-Game 280 423
for($step=0;$step -lt 7;$step++) { Click-Game 1080 673 }
Start-Sleep -Seconds 1
Press-Game 27
Start-Sleep -Milliseconds 400
$savePath=Join-Path $env:USERPROFILE 'AppData\LocalLow\Bellwether Games\NO CAUSE FOR ALARM\cohort-save.json'
$before=Get-Content $savePath -Raw | ConvertFrom-Json
Capture-Game 'input-pause'
Press-Game 27
Press-Game 68 370
Press-Game 87 1400
Press-Game 70
Start-Sleep -Seconds 2
Press-Game 27
$after=Get-Content $savePath -Raw | ConvertFrom-Json
$distance=[Math]::Sqrt([Math]::Pow($after.px-$before.px,2)+[Math]::Pow($after.pz-$before.pz,2))
$result="Native input: moved $distance metres; fuel before $($before.fuel), after $($after.fuel)."
$result | Set-Content (Join-Path $projectPath 'Artifacts\input-check.txt')
Write-Output $result
Capture-Game 'input-after-movement'
Press-Game 27
Press-Game 9
Capture-Game 'input-notebook'
Press-Game 27
Press-Game 77
Capture-Game 'input-map'
Press-Game 27
Press-Game 27
Click-Game 285 453
if($distance -lt .5 -or $after.fuel -ge $before.fuel) { throw 'Movement or lighter input failed; inspect screenshots.' }
