@echo off
echo --- Running Pre-Build Orchestrator ---

set "SOLUTION_DIR=%~1"
set "SCRIPT_PATH=%SOLUTION_DIR%update_paths.ps1"

powershell.exe -ExecutionPolicy Bypass -File "%SCRIPT_PATH%" -SolutionDir "%SOLUTION_DIR%"

if %errorlevel% neq 0 (
    echo.
    echo ******************************************************
    echo *** PRE-BUILD SCRIPT FAILED. See output above. ***
    echo ******************************************************
    exit /b %errorlevel%
)

echo --- Pre-Build Orchestrator Finished Successfully ---
exit /b 0
