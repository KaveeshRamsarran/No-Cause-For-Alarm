param(
    [string]$Unity = 'C:\Program Files\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe',
    [string]$Blender = 'C:\Program Files\Blender Foundation\Blender 5.2\blender.exe'
)
$ErrorActionPreference = 'Stop'
$projectPath = Split-Path -Parent $PSScriptRoot
Push-Location $projectPath
try {
    python Tools/import_cached_assets.py
    if ($LASTEXITCODE -ne 0) { throw 'Free Unity Store packs are missing from the local cache.' }
    if (-not (Test-Path -LiteralPath 'SourceAssets/Overhaul/lighter/vintage_lighter.blend')) {
        python Tools/download_lighter.py
        if ($LASTEXITCODE -ne 0) { throw 'Could not download the CC0 Poly Haven lighter.' }
    }
    $conversion = Start-Process $Blender -ArgumentList '-b --python Tools/prepare_lighter.py' -WindowStyle Hidden -Wait -PassThru
    if ($conversion.ExitCode -ne 0) { throw 'Blender asset preparation failed.' }
    $arguments = @('-batchmode','-quit','-accept-apiupdate','-projectPath',('"'+$projectPath+'"'),'-executeMethod','OverhaulAssets.Prepare','-logFile','Logs/OverhaulPrepare.log')
    $prepare = Start-Process $Unity -ArgumentList $arguments -WindowStyle Hidden -Wait -PassThru
    if ($prepare.ExitCode -ne 0) { throw 'Unity asset preparation failed; see Logs/OverhaulPrepare.log.' }
} finally { Pop-Location }
