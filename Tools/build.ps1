param([string]$Unity = 'C:\Program Files\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe')
$ErrorActionPreference = 'Stop'
$projectPath = Split-Path -Parent $PSScriptRoot
$logDirectory = Join-Path $projectPath 'Logs'
New-Item -ItemType Directory -Force -Path $logDirectory | Out-Null
$logPath = Join-Path $logDirectory 'Build.log'
$arguments = @('-batchmode', '-quit', '-accept-apiupdate', '-projectPath', ('"' + $projectPath + '"'), '-executeMethod', 'BuildGame.Build', '-logFile', ('"' + $logPath + '"'))
$process = Start-Process -FilePath $Unity -ArgumentList $arguments -WindowStyle Hidden -Wait -PassThru
if ($process.ExitCode -ne 0) { throw "Unity build failed; see $logPath" }
Write-Output (Join-Path $projectPath 'Builds\Windows\NO CAUSE FOR ALARM.exe')
