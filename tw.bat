@echo off
setlocal enabledelayedexpansion

:: --- 設定路徑 ---
set "SCSS_MAIN=wwwroot/scss/main.scss"
set "COMPONENTS_CSS=wwwroot/css/components.css"
set "TAILWIND_INPUT=wwwroot/css/tailwind.css"
set "SITE_CSS=wwwroot/css/site.css"

:: --- 主程序 ---
cls
echo =================================
echo  Matrix 前端監聽工具 (進階連動版)
echo =================================
echo.

:: 檢查 Node.js 是否存在
node -v >nul 2>&1
if %errorlevel% neq 0 (
    echo [ERROR] 找不到 Node.js。請先安裝 Node.js 後再執行此腳本。
    pause
    exit /b
)

echo [INFO] 安裝 SASS
start npm i -g sass

echo [INFO] 正在背景啟動 SASS 監聽...
:: 【修正處】使用 node 來執行 sass.dart.js
start "SASS_Watcher" /b sass "%SCSS_MAIN%" "%COMPONENTS_CSS%" --watch --no-source-map
echo [SUCCESS] SASS 監聽已啟動，並已生成 components.css。

:: --- 主監控迴圈 ---
:main_loop
    echo.
    echo [INFO] 正在啟動 Tailwind CSS 監聽...
    :: 使用 start /b 啟動 Tailwind，並給予一個唯一的視窗標題 "Tailwind_Watcher"
    :: 這個標題是後續用來精準終止此程序的關鍵
    start "Tailwind_Watcher" /b tailwindcss -i "%TAILWIND_INPUT%" -o "%SITE_CSS%" --watch

    :: 獲取 components.css 目前的修改時間
    for %%F in ("%COMPONENTS_CSS%") do set "LAST_MODIFIED=%%~tF"
    echo [WATCHING] 開始監控 %COMPONENTS_CSS% 的變更... (目前時間戳: !LAST_MODIFIED!)

    :: 內部迴圈，專門用來檢查檔案變更
    :watch_for_changes
        :: 每秒檢查一次
        timeout /t 1 >nul

        :: 檢查 SASS 的輸出檔 components.css 是否被更新
        for %%F in ("%COMPONENTS_CSS%") do set "CURRENT_MODIFIED=%%~tF"

        :: 如果時間戳不同，代表 SASS 重新編譯了
        if not "!LAST_MODIFIED!"=="!CURRENT_MODIFIED!" (
            echo.
            echo [CHANGE DETECTED] SASS 已更新 components.css！
            echo [ACTION] 準備重新啟動 Tailwind 監聽...

            :: 根據我們啟動時設定的視窗標題，精準地終止舊的 Tailwind 程序
            taskkill /fi "WINDOWTITLE eq Tailwind_Watcher" /f >nul 2>&1
            
            :: 跳轉回主迴圈，以重新啟動 Tailwind
            goto main_loop
        )
        
        :: 如果沒有變更，繼續監控
        goto watch_for_changes

echo.
echo ======================================================
echo  若要停止所有監聽，請直接關閉此視窗。
echo ======================================================
echo.
