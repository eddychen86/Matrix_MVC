@echo off
REM 取得第一個參數
set arg=%1

if "%arg%"=="-b" (
    tailwindcss.exe -i ./wwwroot/css/tailwind.css -o ./wwwroot/css/site.css --minify
    exit /b
)
if "%arg%"=="-d" (
    tailwindcss.exe -i ./wwwroot/css/tailwind.css -o ./wwwroot/css/site.css --watch --poll
    exit /b
)

echo 參數錯誤，請使用 -b、-d
exit /b 1