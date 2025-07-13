/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./Views/**/*.{cshtml,html}",
    "./wwwroot/css/tailwind.css"
  ],
  theme: {
    extend: {},
  },
  plugins: [
    require("./wwwroot/lib/daisyui/daisyui.js"),
    require("./wwwroot/lib/daisyui/daisyui-theme.js")
  ],
}