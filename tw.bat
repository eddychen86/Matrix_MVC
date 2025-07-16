@echo off
setlocal

:: Tailwind CSS 工具腳本 - Windows 版本

set "arg=%~1"

if "%arg%"=="-b" goto build
if "%arg%"=="-d" goto dev  
if "%arg%"=="-w" goto watch
goto usage

:build
echo Building Tailwind CSS (minified)...
tailwindcss.exe -i "./wwwroot/css/tailwind.css" -o "./wwwroot/css/site.css" --minify
goto end

:dev
echo Starting Tailwind CSS development watch...
tailwindcss.exe -i "./wwwroot/css/tailwind.css" -o "./wwwroot/css/site.css" --watch --poll
goto end

:watch
echo Starting smart watch with SCSS support...
cd wwwroot
call js\watch-core.bat
goto end

:usage
echo 參數錯誤，請使用：
echo   -b  : build (產生最小化 CSS)
echo   -d  : dev watch (開發模式監聽)  
echo   -w  : smart watch (智能監聽，包含 SCSS 支援)
exit /b 1

:end