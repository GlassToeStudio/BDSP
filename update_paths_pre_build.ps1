# =================================================================================
# POWERSHELL SCRIPT TO MODIFY CSPROJ OUTPUT PATHS (FOR MSBUILD) - v4 (Toggle Logic)
# =================================================================================
# This script toggles the build output path based on the current user.
# - If the user is the specified admin, it REMOVES the <BaseOutputPath> tag.
# - For all other users, it SETS the <BaseOutputPath> tag to "\bin".
# =================================================================================

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

# --- Robustly read the .env file ---
$changeOutput = $null
$envPath = Join-Path -Path $SolutionDir -ChildPath ".env"

if (-not (Test-Path $envPath)) {
    Write-Host "FATAL ERROR: The file '$envPath' could not be found."
    exit 1
}

Get-Content $envPath | ForEach-Object {
    $line = $_.Trim()
    if ($line -like "MOVE_OUTPUT=*") {

        $isSet = [System.Convert]::ToBoolean($env:MOVE_OUTPUT)
        if ($isSet) { Write-Host "Feature is on" } else { "isSet = $isSet" }
        $changeOutput = $line.Split('=', 2)[1].Trim()
        Write-Host "Change Output Flag Detected: $changeOutput"
        write-Host "Line Content: $line"
        write-Host "Parsed Value: $($line.Split('=', 2)[1].Trim())"
        write-Host "$changeOutput == $true : $($changeOutput -eq $true)"
    }
}

if ([string]::IsNullOrEmpty($changeOutput)) {
    Write-Host "FATAL ERROR: .env file was found, but does not contain a valid 'TARGET_USER=username' entry."
    exit 1
}

# --- Main Logic Gate with Toggle ---
#$changeOutput = $false

# --- ACTION 1: REMOVE ---
# If the check passes, ensure the <BaseOutputPath> tag is REMOVED.
if ($changeOutput -eq $false) {
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
# --- ACTION 2: OTHER USERS ---
# If the check fails, ensure the <BaseOutputPath> tag is SET to "\bin".
else {
    Write-Host "Pre-Build Event: User check failed. Setting <BaseOutputPath> to '\bin'..."
    Write-Host "Current User: $currentUser (Expected: $expectedUser), Is Admin: $isAdmin"

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
                    $newElement.InnerText = "\bin"
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

Write-Host "Pre-build script finished."
exit 0
