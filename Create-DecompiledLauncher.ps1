<#
    Sadly this one doesn't work because we need XAML decompilation too which is not part of ilspycmd
#>

function Get-LauncherPath() {  
    [System.Reflection.Assembly]::LoadWithPartialName("System.windows.forms") | Out-Null

    $OpenFileDialog = New-Object System.Windows.Forms.OpenFileDialog
    $OpenFileDialog.filter = "Dartkide Launcher (Launcher.exe) | Launcher.exe"
    $OpenFileDialog.ShowDialog() | Out-Null
    
    return $OpenFileDialog.FileName
}

$programFilesPath = [environment]::getfolderpath("ProgramFilesX86")
$steamLauncherPath = Join-Path -Path $programFilesPath -ChildPath "Steam\steamapps\common\Warhammer 40,000 DARKTIDE\launcher\Launcher.exe"
$xboxlauncherPath = "C:\XboxGames\Warhammer 40,000- Darktide\Content\launcher\Launcher.exe"

if (Test-Path -Path $steamLauncherPath) {
    $launcherPath = $steamLauncherPath
}
elseif (Test-Path -Path $xboxlauncherPath) {
    $launcherPath = $steamLauncherPath
}
else {
    $launcherPath = Get-LauncherPath
}

dotnet tool run ilspycmd --nested-directories -p -o .\DartkideLauncher $launcherPath
