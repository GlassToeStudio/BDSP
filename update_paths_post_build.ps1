param (
    [string]$SolutionDir
)
exit 0
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