// Menu composable function (ESM)
export const useMenu = () => {
    const { ref, } = Vue

    //#region Sidebar State

    const isCollapsed = ref(false)

    const toggleSidebar = () => {
        isCollapsed.value = !isCollapsed.value
        lucide.createIcons()
    }

    //#endregion

    //#region Language

    // 翻譯快取系統
    const translationCache = new Map()
    const requestCache = new Map() // 防止重複請求
    const preconnectedDomains = new Set()

    // DNS 預解析和連接預熱
    const preconnectToAPI = () => {
        if (!preconnectedDomains.has(location.origin)) {
            // 創建預連接
            const link = document.createElement('link')
            link.rel = 'preconnect'
            link.href = location.origin
            document.head.appendChild(link)
            preconnectedDomains.add(location.origin)
        }
    }

    // 高速 fetch 函數 - 集成多種優化技術
    const fastFetch = async (url, options = {}) => {
        // 1. 檢查是否有相同的請求正在進行（防止重複請求）
        const requestKey = `${url}:${JSON.stringify(options)}`
        if (requestCache.has(requestKey)) {
            return requestCache.get(requestKey)
        }

        // 2. 創建優化的請求 Promise
        const requestPromise = (async () => {
            try {
                // 3. 優化的 fetch 選項
                const optimizedOptions = {
                    method: 'GET',
                    credentials: 'same-origin',
                    cache: 'force-cache', // 使用瀏覽器快取
                    priority: 'high', // Chrome 支援的優先級
                    ...options,
                    headers: {
                        'Accept': 'application/json',
                        'Accept-Encoding': 'gzip, deflate, br',
                        'Cache-Control': 'max-age=3600',
                        ...options.headers
                    }
                }

                // 4. 使用 AbortController 設置合理超時
                const controller = new AbortController()
                const timeoutId = setTimeout(() => controller.abort("Request timeout"), 10000) // 10秒超時

                const response = await fetch(url, {
                    ...optimizedOptions,
                    signal: controller.signal
                })

                clearTimeout(timeoutId)

                if (!response.ok) {
                    throw new Error(`HTTP ${response.status}: ${response.statusText}`)
                }

                // 解析 JSON 並返回數據，而不是 Response 對象
                const data = await response.json()
                return data
            } catch (error) {
                // Handle AbortError specifically
                if (error.name === 'AbortError') {
                    console.warn('Request was aborted (likely due to timeout):', url)
                } else {
                    console.error('Fetch error:', error)
                }
                throw error
            } finally {
                // 5. 清理請求快取
                setTimeout(() => requestCache.delete(requestKey), 100)
            }
        })()

        // 6. 快取請求 Promise 以防止重複
        requestCache.set(requestKey, requestPromise)

        return requestPromise
    }

    // 預載翻譯狀態追蹤
    let preloadInProgress = false

    // 預載翻譯
    const preloadTranslations = async () => {
        if (preloadInProgress) {
            console.log('Translation preload already in progress, skipping...')
            return
        }

        preloadInProgress = true
        try {
            const languages = ['zh-TW', 'en-US']
            const currentLang = document.documentElement.lang

            // 標準化當前語言，確定要預載的目標語言
            let normalizedCurrentLang = currentLang
            if (currentLang === 'en-TW' || currentLang === 'zh-tw' || currentLang === 'zh') {
                normalizedCurrentLang = 'zh-TW'
            } else if (currentLang === 'en' || currentLang === 'en-tw') {
                normalizedCurrentLang = 'en-US'
            }

            // 預載另一種語言
            const targetLang = (normalizedCurrentLang === 'zh-TW' || normalizedCurrentLang.includes('zh')) ? 'en-US' : 'zh-TW'

            if (!translationCache.has(targetLang)) {
                try {
                    // console.log(`Preloading translations for ${targetLang}`)
                    const translations = await fastFetch(`/api/translation/${targetLang}`)
                    translationCache.set(targetLang, translations)
                    // console.log(`Successfully preloaded ${Object.keys(translations).length} translations for ${targetLang}`)
                } catch (error) {
                    // 預載失敗不應影響應用正常運行，靜默處理
                    if (error.name === 'AbortError') {
                        console.debug('Translation preload timed out for:', targetLang, '- will load on demand')
                    } else {
                        console.debug('Translation preload failed for:', targetLang, '- will load on demand')
                    }
                }
            }
        } finally {
            preloadInProgress = false
        }
    }

    const toggleLang = async () => {
        // current language
        const curLang = document.documentElement.lang

        // 標準化當前語言代碼，處理可能的錯誤格式
        let normalizedCurLang = curLang
        if (curLang === 'en-TW' || curLang === 'zh-tw' || curLang === 'zh') {
            normalizedCurLang = 'zh-TW'
        } else if (curLang === 'en' || curLang === 'en-tw') {
            normalizedCurLang = 'en-US'
        }

        // switch language - 確保切換到正確的目標語言
        const changeLang = (normalizedCurLang === 'zh-TW' || normalizedCurLang.includes('zh')) ? 'en-US' : 'zh-TW'
        // console.log(`Switching from ${curLang} (normalized: ${normalizedCurLang}) to ${changeLang}`)

        try {
            let translations

            // 1. 檢查快取
            if (translationCache.has(changeLang)) {
                // console.log('Using cached translations')
                translations = translationCache.get(changeLang)
            } else {
                // 2. 從 API 取得新語言的翻譯（使用優化的 fastFetch）
                translations = await fastFetch(`/api/translation/${changeLang}`)
                translationCache.set(changeLang, translations)
            }

            // 3. 更新頁面文字
            updatePageText(translations)

            // 4. 設定 cookie 記住用戶偏好（符合 ASP.NET Core 的格式）
            const cultureCookie = `c=${changeLang}|uic=${changeLang}`
            document.cookie = `.AspNetCore.Culture=${encodeURIComponent(cultureCookie)}; path=/; max-age=31536000; SameSite=Lax`
            // console.log('Cookie set:', cultureCookie)

            // 5. 更新 html lang 屬性
            document.documentElement.lang = changeLang

            // 6. 預載下一次可能切換的語言
            setTimeout(() => preloadTranslations(), 100)

            // console.log(`Language switched to: ${changeLang}`)

        } catch (error) {
            console.error('Error switching language:', error)

            // 如果 API 失敗，嘗試從快取獲取
            const cachedTranslations = translationCache.get(changeLang)
            if (cachedTranslations) {
                console.log('Using fallback cached translations')
                updatePageText(cachedTranslations)
                const cultureCookie = `c=${changeLang}|uic=${changeLang}`
                document.cookie = `.AspNetCore.Culture=${encodeURIComponent(cultureCookie)}; path=/; max-age=31536000; SameSite=Lax`
                document.documentElement.lang = changeLang
            } else {
                console.error('No cached translations available for fallback')
                alert('Language switching failed. Please try again.')
            }
        }
    }

    // 更新頁面文字的函數
    const updatePageText = (translations) => {
        if (!translations || typeof translations !== 'object') {
            console.error('Invalid translations object:', translations)
            return
        }

        // console.log('Updating page text with', Object.keys(translations).length, 'translations')

        // 找到所有帶有 data-i18n 屬性的元素
        document.querySelectorAll('[data-i18n]').forEach(element => {
            const key = element.getAttribute('data-i18n')

            if (translations[key]) {
                // 檢查元素類型來決定更新方式
                if (element.tagName === 'INPUT' && (element.type === 'submit' || element.type === 'button')) {
                    // 按鈕 input 更新 value
                    element.value = translations[key]
                } else if (element.placeholder !== undefined) {
                    // 有 placeholder 的元素更新 placeholder
                    element.placeholder = translations[key]
                } else {
                    // 一般元素更新 textContent
                    element.textContent = translations[key]
                }
            }
        })

        // 處理 data-i18n-placeholder 屬性的元素
        document.querySelectorAll('[data-i18n-placeholder]').forEach(element => {
            const key = element.getAttribute('data-i18n-placeholder')
            if (translations[key]) element.placeholder = translations[key]
        })

        // 更新頁面標題（如果有 title 翻譯）
        if (translations['Title']) document.title = translations['Title']

        // 呼叫 Profile 頁面的翻譯回調函數（如果存在）
        if (window.profileTranslationCallbacks) {
            window.profileTranslationCallbacks.forEach(callback => {
                if (typeof callback === 'function') {
                    try {
                        callback(translations)
                    } catch (error) {
                        console.error('Profile translation callback error:', error)
                    }
                }
            })
        }

        // console.log('Page text updated with new translations')
    }

    // 將翻譯相關函數暴露到全域
    window.updatePageText = updatePageText
    window.translationCache = translationCache

    //#endregion

    //#region Auth Functions

    // 登出功能
    const logout = async () => {
        try {
            const response = await fetch('/api/auth/logout', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json', }
            })

            const result = await response.json()

            if (result.success) {
                console.log('Logout successful')
                // 跳轉到首頁或登入頁
                window.location.href = '/'
            } else console.error('Logout failed')
        } catch (error) {
            console.error('Error during logout:', error)
        }
    }

    // 登入跳轉功能
    const login = () => window.location.href = '/login'

    //#endregion

    //#region 側邊欄 pop-up 視窗功能

    const searchQuery = ref('')

    //#endregion

    //#region Menu Click Handler

    // 統一處理選單點擊事件
    const handleMenuClick = (action) => {
        switch (action) {
            case 'toggleLang':
                toggleLang()
                break
            case 'toggleSidebar':
                toggleSidebar()
                break
            case 'logout':
                logout()
                break
            case 'login':
                login()
                break
            default:
                console.warn(`Unknown action: ${action}`)
        }
    }

    //#endregion

    //#region Dashboard Navigation

    // 載入 Dashboard 頁面內容
    const loadDashboardPage = async (page) => {
        try {
            // console.log(`Loading dashboard page: ${page}`)

            // 設定載入狀態
            const contentArea = document.querySelector('#dashboard-content')
            if (contentArea) {
                contentArea.innerHTML = '<div class="flex justify-center items-center h-64"><div class="loading loading-spinner loading-lg"></div></div>'
            }

            // 將第一個字母大寫以匹配路由
            const capitalizedPage = page.charAt(0).toUpperCase() + page.slice(1)
            const fetchUrl = `/Dashboard/${capitalizedPage}/Partial`
            // console.log(`Fetching URL: ${fetchUrl}`)

            // AJAX 請求載入 Partial View
            const response = await fetch(fetchUrl, {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json',
                    'X-Requested-With': 'XMLHttpRequest'
                }
            })

            // console.log(`Response status: ${response.status}`)

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`)
            }

            const html = await response.text()
            // console.log(`Response received, content length: ${html.length}`)

            // 更新內容區域
            if (contentArea) {
                contentArea.innerHTML = html

                // 重新初始化 Lucide 圖示
                lucide.createIcons()

                // 更新瀏覽器 URL（不刷新頁面）
                window.history.pushState(null, '', `/Dashboard/${capitalizedPage}`)

                // console.log(`Dashboard page loaded: ${capitalizedPage}`)
            }

        } catch (error) {
            console.error('Error loading dashboard page:', error)
            console.error('Error details:', {
                message: error.message,
                stack: error.stack
            })

            // 錯誤處理
            const contentArea = document.querySelector('#dashboard-content')
            if (contentArea) {
                contentArea.innerHTML = `
                        <div class="alert alert-error">
                            <span>載入頁面時發生錯誤：${error.message}</span>
                        </div>
                    `
            }
        }
    }

    //#endregion

    // 初始化優化 - 延遲預載以避免與應用啟動衝突
    preconnectToAPI()

    // 延遲預載翻譯，讓應用先完全啟動
    const initPreloading = () => {
        if (document.readyState === 'complete') {
            setTimeout(() => {
                preloadTranslations()
            }, 1000) // 1秒後開始預載
        } else {
            window.addEventListener('load', () => {
                setTimeout(() => {
                    preloadTranslations()
                }, 1000) // 頁面完全載入後1秒開始預載
            })
        }
    }

    initPreloading()

    return {
        isCollapsed,
        toggleSidebar,
        toggleLang,
        handleMenuClick,
        logout,
        login,
        loadDashboardPage,
        searchQuery,
        preloadTranslations,
    }
}

export default useMenu;
