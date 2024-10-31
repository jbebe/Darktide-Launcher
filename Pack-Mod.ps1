$version = Read-Host "Mod version"
$zipFile = "launcher-improvements-$version.zip"
Remove-Item $zipFile -ErrorAction Ignore

Add-Type -AssemblyName System.IO.Compression, System.IO.Compression.FileSystem

$zip = [System.IO.Compression.ZipFile]::Open(
    (Join-Path -Path $(Resolve-Path -Path ".") -ChildPath $zipFile),
  [System.IO.Compression.ZipArchiveMode]::Create
)

function CreateZipItem ([string]$filePath, [string] $prefix) {
  $newPath = $(Resolve-Path -Path $filePath -Relative) -replace '\.\\', ''
  $newPath = Join-Path $prefix $newPath
  Write-Output "$newPath"
  $zipEntry = $zip.CreateEntry($newPath)
  $zipEntryWriter = New-Object -TypeName System.IO.BinaryWriter $zipEntry.Open()
  Write-Host "abs: $filePath"
  $zipEntryWriter.Write([System.IO.File]::ReadAllBytes($filePath))
  $zipEntryWriter.Flush()
  $zipEntryWriter.Close()
}

try {
  # Create zip item(s)
  Push-Location "Launcher"
  dotnet build --configuration Release
  Push-Location "bin\Release\net472"
  $filePath = Join-Path -Path "$(Get-Location)" -ChildPath "Launcher.exe"
  CreateZipItem $filePath "launcher" 
}
finally {
  # Clean up
  Pop-Location
  Pop-Location
  $zip.Dispose()
}
