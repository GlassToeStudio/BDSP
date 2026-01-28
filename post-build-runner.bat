@echo off
echo --- Running Post-Build Orchestrator ---

set "SOLUTION_DIR=%~1"
set "SCRIPT_PATH=%SOLUTION_DIR%update_paths_post_build.ps1"

powershell.exe -ExecutionPolicy Bypass -File "%SCRIPT_PATH%" -SolutionDir "%SOLUTION_DIR%"

if %errorlevel% neq 0 (
    echo.
    echo ******************************************************
    echo *** PRE-BUILD SCRIPT FAILED. See output above. ***
    echo ******************************************************
    exit /b %errorlevel%
)

echo --- Post-Build Orchestrator Finished Successfully ---
exit /b 0
