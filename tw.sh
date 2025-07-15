#!/bin/bash

chmod +x ./tailwindcss
arg="$1"

case "$arg" in
  -b)
    ./tailwindcss -i ./wwwroot/css/tailwind.css -o ./wwwroot/css/site.css --minify
    ;;
  -d)
    ./tailwindcss -i ./wwwroot/css/tailwind.css -o ./wwwroot/css/site.css --watch --poll
    ;;
  -w)
    cd wwwroot && ./js/watch-core.sh
    ;;
  *)
    echo "參數錯誤，請使用 -b (build)、-d (dev watch)、-w (smart watch with sass)"
    exit 1
    ;;
esac
