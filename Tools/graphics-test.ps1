$ErrorActionPreference = 'Stop'
$projectPath = Split-Path -Parent $PSScriptRoot
$arguments = @('-screen-fullscreen','0','-screen-width','1280','-screen-height','720','-ncfa-art','-logFile','Logs/GraphicsReview.log')
$process = Start-Process (Join-Path $projectPath 'Builds/Windows/NO CAUSE FOR ALARM.exe') -ArgumentList $arguments -WorkingDirectory $projectPath -WindowStyle Normal -Wait -PassThru
Get-Content (Join-Path $projectPath 'Artifacts/overhaul/graphics-validation.txt')
if ($process.ExitCode -ne 0) { throw 'Graphics validation failed.' }
