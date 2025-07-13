#!/bin/bash

arg="$1"

case "$arg" in
  -b)
    ./tailwindcss -i ./wwwroot/css/tailwind.css -o ./wwwroot/css/site.css --minify
    ;;
  -d)
    ./tailwindcss -i ./wwwroot/css/tailwind.css -o ./wwwroot/css/site.css --watch --poll
    ;;
  *)
    echo "參數錯誤，請使用 -b、-d"
    exit 1
    ;;
esac
