#!/bin/bash

# 智能監聽腳本 - 不依賴 Node.js
# 當 SCSS 編譯完成後重啟 Tailwind CSS watch

# 顏色定義
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# 全局變量
TAILWIND_PID=""
SASS_PID=""
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
WWWROOT_DIR="$(dirname "$SCRIPT_DIR")"
PROJECT_ROOT="$(dirname "$WWWROOT_DIR")"

# 啟動 Tailwind CSS watch
start_tailwind_watch() {
    if [ ! -z "$TAILWIND_PID" ] && kill -0 $TAILWIND_PID 2>/dev/null; then
        echo -e "${YELLOW}⏹️  Stopping Tailwind CSS watch...${NC}"
        kill $TAILWIND_PID 2>/dev/null
        wait $TAILWIND_PID 2>/dev/null
    fi
    
    echo -e "${BLUE}🎨 Starting Tailwind CSS watch...${NC}"
    cd "$PROJECT_ROOT"
    ./tailwindcss -i ./wwwroot/css/tailwind.css -o ./wwwroot/css/site.css --watch &
    TAILWIND_PID=$!
    cd "$WWWROOT_DIR"
}

# 啟動 SCSS watch
start_sass_watch() {
    echo -e "${GREEN}👀 Starting SCSS watch...${NC}"
    sass scss/main.scss css/components.css --watch --no-source-map &
    SASS_PID=$!
}

# 監聽文件變化
monitor_css_changes() {
    local css_file="css/components.css"
    local last_modified=""
    
    # 檢查系統是否有 fswatch (macOS) 或 inotifywait (Linux)
    if command -v fswatch >/dev/null 2>&1; then
        echo -e "${GREEN}📁 Using fswatch for file monitoring...${NC}"
        fswatch -o "$css_file" | while read; do
            echo -e "${YELLOW}📝 SCSS compiled, restarting Tailwind CSS...${NC}"
            start_tailwind_watch
            sleep 0.5
        done
    elif command -v inotifywait >/dev/null 2>&1; then
        echo -e "${GREEN}📁 Using inotifywait for file monitoring...${NC}"
        while inotifywait -e modify "$css_file" 2>/dev/null; do
            echo -e "${YELLOW}📝 SCSS compiled, restarting Tailwind CSS...${NC}"
            start_tailwind_watch
            sleep 0.5
        done
    else
        # 降級方案：polling
        echo -e "${YELLOW}📁 Using polling for file monitoring (install fswatch or inotify-tools for better performance)...${NC}"
        while true; do
            if [ -f "$css_file" ]; then
                current_modified=$(stat -c %Y "$css_file" 2>/dev/null || stat -f %m "$css_file" 2>/dev/null)
                if [ "$current_modified" != "$last_modified" ] && [ ! -z "$last_modified" ]; then
                    echo -e "${YELLOW}📝 SCSS compiled, restarting Tailwind CSS...${NC}"
                    start_tailwind_watch
                    sleep 0.5
                fi
                last_modified="$current_modified"
            fi
            sleep 1
        done
    fi
}

# 清理函數
cleanup() {
    echo -e "\n${RED}🛑 Shutting down...${NC}"
    if [ ! -z "$TAILWIND_PID" ]; then
        kill $TAILWIND_PID 2>/dev/null
    fi
    if [ ! -z "$SASS_PID" ]; then
        kill $SASS_PID 2>/dev/null
    fi
    # 清理所有相關進程
    pkill -f "tailwindcss.*--watch" 2>/dev/null
    pkill -f "sass.*--watch" 2>/dev/null
    exit 0
}

# 設置信號處理
trap cleanup SIGINT SIGTERM

# 主函數
main() {
    echo -e "${GREEN}🚀 Smart Watch Script Starting...${NC}"
    echo -e "${BLUE}📁 Working directory: $WWWROOT_DIR${NC}"
    
    # 檢查必要文件
    if [ ! -f "$PROJECT_ROOT/tailwindcss" ]; then
        echo -e "${RED}❌ Error: tailwindcss binary not found at $PROJECT_ROOT/tailwindcss${NC}"
        exit 1
    fi
    
    if [ ! -f "scss/main.scss" ]; then
        echo -e "${RED}❌ Error: scss/main.scss not found${NC}"
        exit 1
    fi
    
    # 確保 tailwindcss 可執行
    chmod +x "$PROJECT_ROOT/tailwindcss"
    
    # 切換到 wwwroot 目錄
    cd "$WWWROOT_DIR"
    
    # 啟動 SCSS watch
    start_sass_watch
    
    # 初始啟動 Tailwind CSS watch
    start_tailwind_watch
    
    # 開始監聽文件變化
    monitor_css_changes
}

# 執行主函數
main