// Menu composable function
const useMenu = () => {
    const { ref, } = Vue

    //#region Sidebar State

    const isCollapsed = ref(false)

    const toggleSidebar = () => {
        isCollapsed.value = !isCollapsed.value
        lucide.createIcons()
    }

    //#endregion

    //#region Language

    const toggleLang = async () => {
        // current language
        const curLang = document.documentElement.lang

        // switch language
        const changeLang = curLang.match(/-TW/) ? 'en-US' : 'zh-TW'
        // console.log(changeLang)

        try {
            // 1. 取得新語言的翻譯
            const response = await fetch(`/api/translation/${changeLang}`)
            if (!response.ok) throw new Error('Failed to load translations')
            const translations = await response.json()

            // 2. 更新頁面文字
            updatePageText(translations)

            // 3. 設定 cookie 記住用戶偏好
            const cultureCookie = `c=${changeLang}|uic=${changeLang}`
            document.cookie = `.AspNetCore.Culture=${cultureCookie} path=/ max-age=31536000 SameSite=Lax`

            // 4. 更新 html lang 屬性
            document.documentElement.lang = changeLang

            // console.log(`Language switched to: ${changeLang}`)

        } catch (error) {
            console.error('Error switching language:', error)
            // 如果 API 失敗，回退到重新載入頁面
            // window.location.reload()
            console.log(error)
        }
    }

    // 更新頁面文字的函數
    const updatePageText = (translations) => {
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

        console.log('Page text updated with new translations')
    }

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

    return {
        isCollapsed,
        toggleSidebar,
        toggleLang,
        handleMenuClick,
        logout,
        login,
        loadDashboardPage,
        searchQuery,
    }
}