@echo off
setlocal enabledelayedexpansion

:: --- Set Paths ---
set "SCSS_MAIN=wwwroot/scss/main.scss"
set "COMPONENTS_CSS=wwwroot/css/components.css"
set "TAILWIND_INPUT=wwwroot/css/tailwind.css"
set "SITE_CSS=wwwroot/css/site.css"
set "SASS_CMD=%USERPROFILE%\.npm-global\sass.cmd"
set "TAILWIND_CMD=tailwindcss.exe"

:: --- Main Program ---
cls
echo =================================
echo  Matrix Frontend Watcher Tool
echo =================================
echo.

:: Check if Node.js exists
node -v >nul 2>&1
if %errorlevel% neq 0 (
    echo [ERROR] Node.js not found. Please install Node.js before running this script.
    pause
    exit /b
)

echo [INFO] Checking if SASS is installed...
echo [DEBUG] SASS_CMD: %SASS_CMD%
call "%SASS_CMD%" --version > sass_check.tmp 2>&1
echo [DEBUG] SASS check exit code: %errorlevel%
if %errorlevel% neq 0 (
    echo [INFO] SASS not installed, installing...
    npm i -g sass
    if !errorlevel! neq 0 (
        echo [ERROR] SASS installation failed. Please check network connection or permissions.
        pause
        exit /b
    )
    echo [INFO] SASS installation complete, verifying...
    call "%SASS_CMD%" --version > sass_install_verify.tmp 2>&1
    if !errorlevel! neq 0 (
        echo [ERROR] SASS cannot be used after installation. Please manually check installation status.
        pause
        exit /b
    )
    echo [SUCCESS] SASS successfully installed and working.
) else (
    echo [SUCCESS] SASS already installed, verifying...
    call "%SASS_CMD%" --version > sass_verify.tmp 2>&1
    if !errorlevel! neq 0 (
        echo [ERROR] SASS is installed but not working. Please reinstall.
        pause
        exit /b
    )
)

echo [INFO] Starting SASS watcher in background...
:: [FIX] Use node to execute sass.dart.js
start "SASS_Watcher" /b "%SASS_CMD%" "%SCSS_MAIN%" "%COMPONENTS_CSS%" --watch --no-source-map
echo [SUCCESS] SASS watcher started and components.css generated.

:: --- Main monitoring loop ---
:main_loop
    echo.
    echo [INFO] Starting Tailwind CSS watcher...
    :: Use start /b to launch Tailwind with unique window title "Tailwind_Watcher"
    :: This title is the key to precisely terminate this process later
    start "Tailwind_Watcher" /b "%TAILWIND_CMD%" -i "%TAILWIND_INPUT%" -o "%SITE_CSS%" --watch

    :: Get current modification time of components.css
    for %%F in ("%COMPONENTS_CSS%") do set "LAST_MODIFIED=%%~tF"
    echo [WATCHING] Starting to monitor %COMPONENTS_CSS% changes... (current timestamp: !LAST_MODIFIED!)

    :: Internal loop for checking file changes
    :watch_for_changes
        :: Check once per second
        timeout /t 1 >nul

        :: Check if SASS output file components.css has been updated
        for %%F in ("%COMPONENTS_CSS%") do set "CURRENT_MODIFIED=%%~tF"

        :: If timestamp is different, SASS has recompiled
        if not "!LAST_MODIFIED!"=="!CURRENT_MODIFIED!" (
            echo.
            echo [CHANGE DETECTED] SASS has updated components.css!
            echo [ACTION] Preparing to restart Tailwind watcher...

            :: Precisely terminate old Tailwind process based on window title we set at startup
            taskkill /fi "WINDOWTITLE eq Tailwind_Watcher" /f >nul 2>&1
            
            :: Jump back to main loop to restart Tailwind
            goto main_loop
        )
        
        :: If no changes, continue monitoring
        goto watch_for_changes

echo.
echo ======================================================
echo  To stop all watchers, please close this window directly.
echo ======================================================
echo.
