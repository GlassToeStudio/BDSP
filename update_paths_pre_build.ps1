param (
    [string]$SolutionDir
)

# --- Clean the incoming path from MSBuild ---
$SolutionDir = $SolutionDir.TrimEnd('"')

# --- Change to the Solution Directory ---
try {
    Set-Location -Path $SolutionDir -ErrorAction Stop
}
catch {
    Write-Host "FATAL ERROR: Could not change directory to '$SolutionDir'. Halting script."
    exit 1
}

# --- Read the .env file ---
$envPath = Join-Path -Path $SolutionDir -ChildPath ".env"
if (-not (Test-Path $envPath)) {
    Write-Host "FATAL ERROR: The file '$envPath' could not be found."
    exit 0
}

$moveOutput = $false
$stringValue = ""
Get-Content $envPath | ForEach-Object {
    $line = $_.Trim()
    if ($line -like "MOVE_OUTPUT=*") {
        $stringValue = $line.Split('=', 2)[1].Trim()
        $moveOutput = ($stringValue -eq 'true')
    }
}

if ([string]::IsNullOrEmpty($moveOutput)) {
    Write-Host "FATAL ERROR: .env file was found, but does not contain a valid 'MOVE_OUTPUT=username' entry."
    exit 1
}

# --- ACTION 1: CHANGE OUTPUT PATH ---
# If the check passes, ensure the <BaseOutputPath> tag is SET to "\bin".
if ($moveOutput -eq $true) {
    Write-Host "Changing <BaseOutputPath> to bin\."

    Get-ChildItem -Path . -Filter "*.csproj" -Recurse | ForEach-Object {
        $csprojPath = $_.FullName
        try {
            $xml = [xml](Get-Content $csprojPath)
            $propertyGroup = $xml.Project.PropertyGroup | Select-Object -First 1
            
            if ($null -ne $propertyGroup) {
                # If the tag doesn't exist, create it.
                if ($null -eq $propertyGroup.BaseOutputPath) {
                    Write-Host " -> Adding tag to: $csprojPath"
                    $newElement = $xml.CreateElement("BaseOutputPath")
                    $newElement.InnerText = "\bin\"
                    $propertyGroup.AppendChild($newElement) | Out-Null
                } 
                # If the tag exists but has the wrong value, update it.
                elseif ($propertyGroup.BaseOutputPath -ne "\bin") {
                    Write-Host " -> Updating tag in: $csprojPath"
                    $propertyGroup.BaseOutputPath = "\bin"
                }
                $xml.Save($csprojPath)
            }
        }
        catch {
            Write-Host " -> ERROR processing file: $_.Exception.Message"
        }
    }
}
# --- ACTION 2: OTHER USERS ---
# If the check fails, ensure the <BaseOutputPath> tag is REMOVED.
else {
    Write-Host "Removing <BaseOutputPath> to restore default build paths."

    Get-ChildItem -Path . -Filter "*.csproj" -Recurse | ForEach-Object {
        $csprojPath = $_.FullName
        try {
            $xml = [xml](Get-Content $csprojPath)
            $propertyGroup = $xml.Project.PropertyGroup | Select-Object -First 1

            # --- THE CORRECTED REMOVAL LOGIC ---
            # 1. Find the actual XML node for BaseOutputPath.
            $nodeToRemove = $propertyGroup.SelectSingleNode("BaseOutputPath")

            # 2. If that node exists, remove it.
            if ($null -ne $nodeToRemove) {
                Write-Host " -> Removing tag from: $csprojPath"
                $propertyGroup.RemoveChild($nodeToRemove) | Out-Null
                $xml.Save($csprojPath)
            }
        }
        catch {
            Write-Host " -> ERROR processing file: $_.Exception.Message"
        }
    }
}

Write-Host "Pre-build script finished."
exit 0
