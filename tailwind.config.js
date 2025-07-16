/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./Views/**/*.{cshtml,html}",
    "./Areas/**/*.cshtml",
    "./wwwroot/css/tailwind.css",
    ".wwwroot/scss/**/*.scss",
    "./**/*.html",
    "./**/*.js",
  ],
  theme: {
    extend: {
      colors: {
        primary: '#1e40af',
        secondary: '#1f2937',
      },
      spacing: {
        '18': '4.5rem',
      }
    },
  },
  plugins: [
    require("./wwwroot/lib/daisyui/daisyui.js"),
    require("./wwwroot/lib/daisyui/daisyui-theme.js")
  ],
}