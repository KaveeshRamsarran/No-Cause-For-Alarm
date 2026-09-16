$ErrorActionPreference = 'Stop'
$projectPath = Split-Path -Parent $PSScriptRoot
$executable = Join-Path $projectPath 'Builds\Windows\NO CAUSE FOR ALARM.exe'
$logPath = Join-Path $projectPath 'Logs\StandaloneSmoke.log'
$arguments = @('-screen-fullscreen', '0', '-screen-width', '1280', '-screen-height', '720', '-ncfa-smoke', '-logFile', ('"' + $logPath + '"'))
$process = Start-Process -FilePath $executable -ArgumentList $arguments -WorkingDirectory $projectPath -WindowStyle Normal -Wait -PassThru
Get-Content (Join-Path $projectPath 'Artifacts\Smoke\results.txt')
if ($process.ExitCode -ne 0) { throw "Standalone validation failed; see $logPath" }
