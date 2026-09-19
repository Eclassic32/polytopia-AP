$required = @(
    "POLYTOPIA_RESOURCES",
    "POLYTOPIA_MOD_DIR",
    "POLYTOPIA_DEBUG_DLL",
    "POLYTOPIA_X_DLL"
)

$envFile = Join-Path (Get-Location) ".env"

if (-not (Test-Path $envFile)) {
    throw "Environment file not found: $envFile"
}

# Read key=value pairs from ../.env
$values = @{}

foreach ($line in Get-Content $envFile) {
    $line = $line.Trim()

    # Ignore blank lines and comments
    if ([string]::IsNullOrWhiteSpace($line) -or $line.StartsWith("#")) {
        continue
    }

    if ($line -match '^\s*([^=]+?)\s*=\s*(.*)\s*$') {
        $name = $matches[1].Trim()
        $value = $matches[2].Trim()

        # Remove matching single or double quotes
        if ($value.Length -ge 2 -and
            (($value.StartsWith('"') -and $value.EndsWith('"')) -or
             ($value.StartsWith("'") -and $value.EndsWith("'")))) {
            $value = $value.Substring(1, $value.Length - 2)
        }

        $values[$name] = $value
    }
}

# Set the required variables in the current PowerShell process
foreach ($name in $required) {
    if (-not $values.ContainsKey($name)) {
        throw "Missing required variable in $envFile`: $name"
    }

    Set-Item -Path "Env:$name" -Value $values[$name]
}

Write-Host "Loaded environment variables from $envFile"
