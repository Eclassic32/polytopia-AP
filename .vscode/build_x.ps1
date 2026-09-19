param(
    [Parameter(Mandatory = $true, Position = 0)]
    [string]$Folder,

    [string]$ZipName = "polytopia-AP.polymod"
)

echo "Building in: $Folder"

$ZipPath = Join-Path $Folder $ZipName
$TempZip = Join-Path ([System.IO.Path]::GetTempPath()) ([System.Guid]::NewGuid().ToString() + ".zip")

try {
    # Load ZIP support
    Add-Type -AssemblyName System.IO.Compression.FileSystem

    # Remove an existing ZIP with the same name, if present
    if (Test-Path $ZipPath) {
        Remove-Item $ZipPath -Force
    }

    # Create the ZIP outside the source folder
    [System.IO.Compression.ZipFile]::CreateFromDirectory(
        $Folder,
        $TempZip,
        [System.IO.Compression.CompressionLevel]::Optimal,
        $false
    )

    # Delete everything inside the folder
    Get-ChildItem -LiteralPath $Folder -Force |
        Remove-Item -Recurse -Force

    # Move the completed ZIP back into the folder
    Move-Item -LiteralPath $TempZip -Destination $ZipPath

    Write-Host "Created: $ZipPath"
}
catch {
    if (Test-Path $TempZip) {
        Remove-Item $TempZip -Force
    }

    Write-Error $_
}
