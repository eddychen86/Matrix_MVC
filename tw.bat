@echo off
setlocal enabledelayedexpansion

echo ========================================
echo    Matrix 專案 CSS 編譯器監控系統
echo ========================================
echo.

REM 設定變數
set "SCSS_SIGNAL_FILE=scss_change_signal.tmp"
set "SASS_WINDOW_TITLE=SASS Watcher"
set "TAILWIND_WINDOW_TITLE=Tailwind Watcher"
set "SCSS_MONITOR_TITLE=SCSS Monitor"

REM 清理函數 - 在開始時清理舊的信號檔案
if exist "%SCSS_SIGNAL_FILE%" (
    echo 清理舊的信號檔案...
    del "%SCSS_SIGNAL_FILE%" >nul 2>&1
)

echo 正在初始化 CSS 編譯環境...
echo.

REM 檢查並刪除舊的 CSS 檔案
:cleanup_old_files
echo [1/7] 清理舊的 CSS 檔案...

if exist "wwwroot\css\components.css" (
    echo   - 刪除舊的 components.css...
    del "wwwroot\css\components.css" >nul 2>&1
    if !errorlevel! equ 0 (
        echo   ✓ components.css 已刪除
    ) else (
        echo   ✗ 無法刪除 components.css
    )
) else (
    echo   - components.css 不存在，跳過
)

if exist "wwwroot\css\site.css" (
    echo   - 刪除舊的 site.css...
    del "wwwroot\css\site.css" >nul 2>&1
    if !errorlevel! equ 0 (
        echo   ✓ site.css 已刪除
    ) else (
        echo   ✗ 無法刪除 site.css
    )
) else (
    echo   - site.css 不存在，跳過
)

echo.

REM 啟動 SASS Watcher
:start_sass_watcher
echo [2/7] 啟動 SASS 編譯器...
start "%SASS_WINDOW_TITLE%" sass wwwroot\scss\main.scss wwwroot\css\components.css --watch --no-source-map

REM 等待 components.css 生成
:wait_for_components_css
echo [3/7] 等待 components.css 生成...
set /a wait_count=0
:wait_loop
if exist "wwwroot\css\components.css" (
    echo   ✓ components.css 已生成
    timeout /t 2 /nobreak >nul
    goto start_tailwind_watcher
) else (
    set /a wait_count+=1
    if !wait_count! gtr 12 (
        echo   ✗ 等待 components.css 生成超時
        echo   請檢查 SASS 編譯器是否正常運作
        goto cleanup_and_exit
    )
    echo   - 等待中... (!wait_count!/12)
    timeout /t 5 /nobreak >nul
    goto wait_loop
)

REM 啟動 Tailwind CSS Watcher
:start_tailwind_watcher
echo [4/7] 啟動 Tailwind CSS 編譯器...
start "%TAILWIND_WINDOW_TITLE%" .\tailwindcss -i wwwroot\css\tailwind.css -o wwwroot\css\site.css --watch
timeout /t 3 /nobreak >nul
echo   ✓ Tailwind CSS 編譯器已啟動
echo.

REM 啟動 components.css 檔案變更監控
:start_scss_monitor
echo [5/7] 啟動 components.css 檔案變更監控...

REM 建立 PowerShell 腳本來監控 components.css 檔案變更
echo $watcher = New-Object System.IO.FileSystemWatcher > scss_monitor.ps1
echo $watcher.Path = "wwwroot\css" >> scss_monitor.ps1
echo $watcher.Filter = "components.css" >> scss_monitor.ps1
echo $watcher.IncludeSubdirectories = $false >> scss_monitor.ps1
echo $watcher.EnableRaisingEvents = $true >> scss_monitor.ps1
echo. >> scss_monitor.ps1
echo $action = { >> scss_monitor.ps1
echo     $path = $Event.SourceEventArgs.FullPath >> scss_monitor.ps1
echo     $changeType = $Event.SourceEventArgs.ChangeType >> scss_monitor.ps1
echo     $timeStamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss" >> scss_monitor.ps1
echo     Write-Host "[$timeStamp] components.css 檔案變更: $changeType - $path" >> scss_monitor.ps1
echo     "SCSS_CHANGED" ^| Out-File -FilePath "scss_change_signal.tmp" -Encoding ASCII >> scss_monitor.ps1
echo } >> scss_monitor.ps1
echo. >> scss_monitor.ps1
echo Register-ObjectEvent -InputObject $watcher -EventName "Changed" -Action $action >> scss_monitor.ps1
echo. >> scss_monitor.ps1
echo Write-Host "components.css 檔案監控已啟動，監控路徑: wwwroot\css" >> scss_monitor.ps1
echo Write-Host "按 Ctrl+C 停止監控..." >> scss_monitor.ps1
echo. >> scss_monitor.ps1
echo try { >> scss_monitor.ps1
echo     while ($true) { >> scss_monitor.ps1
echo         Start-Sleep -Seconds 1 >> scss_monitor.ps1
echo     } >> scss_monitor.ps1
echo } finally { >> scss_monitor.ps1
echo     $watcher.EnableRaisingEvents = $false >> scss_monitor.ps1
echo     $watcher.Dispose() >> scss_monitor.ps1
echo     Write-Host "components.css 監控已停止" >> scss_monitor.ps1
echo } >> scss_monitor.ps1

start "%SCSS_MONITOR_TITLE%" powershell -ExecutionPolicy Bypass -File scss_monitor.ps1
timeout /t 2 /nobreak >nul
echo   ✓ components.css 檔案變更監控已啟動
echo.

REM 顯示啟動完成訊息
:startup_complete
echo [6/7] 系統啟動完成
echo ========================================
echo   ✓ SASS 編譯器運行中
echo   ✓ Tailwind CSS 編譯器運行中
echo   ✓ components.css 檔案變更監控運行中
echo ========================================
echo.
echo 監控的檔案：
echo   - wwwroot/css/components.css
echo.
echo 當 components.css 檔案變更時，系統會自動：
echo   - 停止 Tailwind CSS 編譯器
echo   - 等待 1 秒讓 SASS 完成編譯
echo   - 重新啟動 Tailwind CSS 編譯器
echo.

REM 主監控迴圈
:main_monitor_loop
echo [7/7] 進入主監控迴圈...
echo 按任意鍵退出並清理所有程序...
echo.

:monitor_loop
REM 非阻塞式檢查使用者輸入
echo|set /p="."
timeout /t 1 /nobreak >nul 2>&1

REM 檢查 SCSS 變更信號
if exist "%SCSS_SIGNAL_FILE%" (
    echo.
    echo ========================================
    echo 偵測到 components.css 檔案變更！
    echo ========================================
    
    REM 刪除信號檔案
    del "%SCSS_SIGNAL_FILE%" >nul 2>&1
    
    REM 停止 Tailwind CSS 編譯器
    echo 正在停止 Tailwind CSS 編譯器...
    taskkill /f /fi "windowtitle eq %TAILWIND_WINDOW_TITLE%*" >nul 2>&1
    if !errorlevel! equ 0 (
        echo   ✓ Tailwind CSS 編譯器已停止
    ) else (
        echo   - Tailwind CSS 編譯器未運行
    )
    
    REM 等待 SASS 完成編譯
    echo 等待 SASS 完成編譯...
    timeout /t 1 /nobreak >nul
    
    REM 重新啟動 Tailwind CSS 編譯器
    echo 重新啟動 Tailwind CSS 編譯器...
    start "%TAILWIND_WINDOW_TITLE%" .\tailwindcss -i wwwroot\css\tailwind.css -o wwwroot\css\site.css --watch
    timeout /t 2 /nobreak >nul
    echo   ✓ Tailwind CSS 編譯器已重新啟動
    echo ========================================
    echo.
    echo 繼續監控中...按任意鍵退出...
)

REM 檢查是否有按鍵輸入 (使用簡化的方法)
choice /c yn /d n /t 1 /m "" >nul 2>&1
if !errorlevel! equ 1 goto cleanup_and_exit

goto monitor_loop

REM 清理和退出
:cleanup_and_exit
echo.
echo ========================================
echo 正在清理並關閉所有程序...
echo ========================================

REM 停止 SASS Watcher
echo 停止 SASS 編譯器...
taskkill /f /fi "windowtitle eq %SASS_WINDOW_TITLE%*" >nul 2>&1
if !errorlevel! equ 0 (
    echo   ✓ SASS 編譯器已停止
) else (
    echo   - SASS 編譯器未運行或已停止
)

REM 停止 Tailwind Watcher
echo 停止 Tailwind CSS 編譯器...
taskkill /f /fi "windowtitle eq %TAILWIND_WINDOW_TITLE%*" >nul 2>&1
if !errorlevel! equ 0 (
    echo   ✓ Tailwind CSS 編譯器已停止
) else (
    echo   - Tailwind CSS 編譯器未運行或已停止
)

REM 停止 components.css Monitor
echo 停止 components.css 檔案監控...
taskkill /f /fi "windowtitle eq %SCSS_MONITOR_TITLE%*" >nul 2>&1
if !errorlevel! equ 0 (
    echo   ✓ components.css 檔案監控已停止
) else (
    echo   - components.css 檔案監控未運行或已停止
)

REM 清理臨時檔案
echo 清理臨時檔案...
if exist "%SCSS_SIGNAL_FILE%" (
    del "%SCSS_SIGNAL_FILE%" >nul 2>&1
    echo   ✓ 信號檔案已清理
)
if exist "scss_monitor.ps1" (
    del "scss_monitor.ps1" >nul 2>&1
    echo   ✓ PowerShell 腳本已清理
)

echo ========================================
echo 所有程序已停止，清理完成
echo ========================================
echo.
pause