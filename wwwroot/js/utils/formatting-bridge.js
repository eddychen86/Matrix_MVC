// Bridge: expose hooks/useFormatting to classic scripts
import { useFormatting } from '/js/hooks/useFormatting.js'
const fmt = useFormatting()
const detectLang = () => (document?.documentElement?.lang || 'zh-TW')

window.formatDateGlobal = (date, type = 'datetime', lang = detectLang()) => fmt.formatDate(date, type, lang)
window.timeAgoGlobal = (date, lang = detectLang()) => fmt.timeAgo(date, lang)

