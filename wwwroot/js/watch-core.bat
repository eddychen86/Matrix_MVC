@echo off
setlocal enabledelayedexpansion

:: 智能監聽腳本 - Windows 版本
:: 當 SCSS 編譯完成後重啟 Tailwind CSS watch

title Smart Watch Script

:: 顏色定義 (Windows 10/11 支援 ANSI 色彩)
set "RED=[91m"
set "GREEN=[92m"
set "YELLOW=[93m"
set "BLUE=[94m"
set "NC=[0m"

:: 全局變量
set "TAILWIND_PID="
set "SASS_PID="
set "SCRIPT_DIR=%~dp0"
set "WWWROOT_DIR=%SCRIPT_DIR%.."
set "PROJECT_ROOT=%WWWROOT_DIR%\.."

:: 啟用 ANSI 色彩支援
for /f "tokens=2 delims=[]" %%x in ('ver') do set "version=%%x"
for /f "tokens=2,3,4 delims=. " %%x in ("!version!") do set "major=%%x" & set "minor=%%y"
if !major! GEQ 10 (
    for /f %%a in ('echo prompt $E ^| cmd') do set "ESC=%%a"
) else (
    set "ESC="
)

:: 函數：啟動 Tailwind CSS watch
:start_tailwind_watch
if defined TAILWIND_PID (
    echo %ESC%%YELLOW%⏹️  Stopping Tailwind CSS watch...%ESC%%NC%
    taskkill /PID %TAILWIND_PID% /F >nul 2>&1
    set "TAILWIND_PID="
)

echo %ESC%%BLUE%🎨 Starting Tailwind CSS watch...%ESC%%NC%
cd /d "%PROJECT_ROOT%"
start /b "TailwindWatch" tailwindcss.exe -i "./wwwroot/css/tailwind.css" -o "./wwwroot/css/site.css" --watch

:: 取得新進程 PID
for /f "tokens=2" %%i in ('tasklist /fi "windowtitle eq TailwindWatch*" /fo table /nh 2^>nul') do set "TAILWIND_PID=%%i"

cd /d "%WWWROOT_DIR%"
goto :eof

:: 函數：啟動 SCSS watch  
:start_sass_watch
echo %ESC%%GREEN%👀 Starting SCSS watch...%ESC%%NC%
start /b "SassWatch" sass scss/main.scss css/components.css --watch --no-source-map

:: 取得 Sass 進程 PID
for /f "tokens=2" %%i in ('tasklist /fi "imagename eq sass.exe" /fo table /nh 2^>nul') do set "SASS_PID=%%i"
goto :eof

:: 函數：監聽文件變化
:monitor_css_changes
set "css_file=css\components.css"
set "last_modified="

:: 檢查是否有 PowerShell（用於更精確的文件監聽）
where powershell >nul 2>&1
if %errorlevel% equ 0 (
    echo %ESC%%GREEN%📁 Using PowerShell FileSystemWatcher for monitoring...%ESC%%NC%
    goto :powershell_watch
) else (
    echo %ESC%%YELLOW%📁 Using polling for file monitoring...%ESC%%NC%
    goto :polling_watch
)

:powershell_watch
:: 使用 PowerShell 的 FileSystemWatcher 進行監聽
powershell -Command "& {
    $watcher = New-Object System.IO.FileSystemWatcher
    $watcher.Path = '%CD%\css'
    $watcher.Filter = 'components.css'
    $watcher.NotifyFilter = [System.IO.NotifyFilters]::LastWrite
    $watcher.EnableRaisingEvents = $true
    
    while ($true) {
        $result = $watcher.WaitForChanged([System.IO.WatcherChangeTypes]::Changed, 1000)
        if ($result.TimedOut -eq $false) {
            Write-Host '📝 SCSS compiled, restarting Tailwind CSS...'
            Start-Sleep -Milliseconds 500
            exit 1
        }
    }
}"
if %errorlevel% equ 1 (
    call :start_tailwind_watch
    goto :powershell_watch
)
goto :eof

:polling_watch
:: 降級方案：輪詢檢查文件修改時間
:polling_loop
if exist "%css_file%" (
    for %%f in ("%css_file%") do set "current_modified=%%~tf"
    
    if defined last_modified (
        if not "!current_modified!"=="!last_modified!" (
            echo %ESC%%YELLOW%📝 SCSS compiled, restarting Tailwind CSS...%ESC%%NC%
            call :start_tailwind_watch
            timeout /t 1 /nobreak >nul
        )
    )
    set "last_modified=!current_modified!"
)
timeout /t 1 /nobreak >nul
goto :polling_loop

:: 函數：清理進程
:cleanup
echo.
echo %ESC%%RED%🛑 Shutting down...%ESC%%NC%
if defined TAILWIND_PID (
    taskkill /PID %TAILWIND_PID% /F >nul 2>&1
)
if defined SASS_PID (
    taskkill /PID %SASS_PID% /F >nul 2>&1
)
:: 清理所有相關進程
taskkill /FI "WINDOWTITLE eq TailwindWatch*" /F >nul 2>&1
taskkill /FI "WINDOWTITLE eq SassWatch*" /F >nul 2>&1
taskkill /FI "IMAGENAME eq sass.exe" /F >nul 2>&1
taskkill /FI "IMAGENAME eq tailwindcss.exe" /F >nul 2>&1
exit /b 0

:: 主程序
:main
echo %ESC%%GREEN%🚀 Smart Watch Script Starting...%ESC%%NC%
echo %ESC%%BLUE%📁 Working directory: %WWWROOT_DIR%%ESC%%NC%

:: 檢查必要文件
if not exist "%PROJECT_ROOT%\tailwindcss.exe" (
    echo %ESC%%RED%❌ Error: tailwindcss.exe not found at %PROJECT_ROOT%\tailwindcss.exe%ESC%%NC%
    pause
    exit /b 1
)

if not exist "scss\main.scss" (
    echo %ESC%%RED%❌ Error: scss\main.scss not found%ESC%%NC%
    pause
    exit /b 1
)

:: 設置 Ctrl+C 處理
if "%~1"=="cleanup" goto :cleanup
start /min "%~f0" cleanup
set "cleanup_pid=%ERRORLEVEL%"

:: 切換到 wwwroot 目錄
cd /d "%WWWROOT_DIR%"

:: 啟動 SCSS watch
call :start_sass_watch

:: 初始啟動 Tailwind CSS watch
call :start_tailwind_watch

:: 開始監聽文件變化
call :monitor_css_changes

:: 程序結束時清理
call :cleanup