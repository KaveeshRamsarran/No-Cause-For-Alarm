$ErrorActionPreference = 'Stop'
$projectPath = Split-Path -Parent $PSScriptRoot
Add-Type @'
using System;
using System.Runtime.InteropServices;
public static class GraphicsTestWindow {
 [StructLayout(LayoutKind.Sequential)] public struct Point { public int X; public int Y; }
 [DllImport("user32.dll")] public static extern bool SetWindowPos(IntPtr window, IntPtr after, int x, int y, int w, int h, uint flags);
 [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr window);
 [DllImport("user32.dll")] public static extern bool ClientToScreen(IntPtr window, ref Point point);
 [DllImport("user32.dll")] public static extern bool SetCursorPos(int x, int y);
 [DllImport("user32.dll")] public static extern void mouse_event(uint flags,uint x,uint y,uint data,UIntPtr extra);
}
'@
$arguments = @('-screen-fullscreen','0','-screen-width','1280','-screen-height','720','-ncfa-art','-logFile','Logs/GraphicsReview.log')
$process = Start-Process (Join-Path $projectPath 'Builds/Windows/NO CAUSE FOR ALARM.exe') -ArgumentList $arguments -WorkingDirectory $projectPath -WindowStyle Normal -PassThru
# Exclusive fullscreen requires an active visible window. Activate the test player,
# then keep it above other windows only for this disposable automated run.
for($attempt=0;$attempt -lt 20;$attempt++) {
 Start-Sleep -Milliseconds 500
 $process.Refresh()
 if($process.MainWindowHandle -ne 0) { break }
}
if($process.MainWindowHandle -eq 0) { Stop-Process -Id $process.Id; throw 'Graphics player did not create a window.' }
[GraphicsTestWindow]::SetWindowPos($process.MainWindowHandle,[IntPtr](-1),0,0,0,0,0x43) | Out-Null
[GraphicsTestWindow]::SetForegroundWindow($process.MainWindowHandle) | Out-Null
$point=New-Object GraphicsTestWindow+Point
$point.X=610; $point.Y=-12
[GraphicsTestWindow]::ClientToScreen($process.MainWindowHandle,[ref]$point) | Out-Null
[GraphicsTestWindow]::SetCursorPos($point.X,$point.Y) | Out-Null
[GraphicsTestWindow]::mouse_event(2,0,0,0,[UIntPtr]::Zero)
Start-Sleep -Milliseconds 80
[GraphicsTestWindow]::mouse_event(4,0,0,0,[UIntPtr]::Zero)
$process.WaitForExit()
$process.Refresh()
Get-Content (Join-Path $projectPath 'Artifacts/overhaul/graphics-validation.txt')
if ($process.ExitCode -ne 0) { throw 'Graphics validation failed.' }
