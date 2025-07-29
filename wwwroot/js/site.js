const app = content => {
    if (typeof Vue === 'undefined') {
        console.log('Vue is not loaded.')
        return
    }
    window.popupApp = Vue.createApp(content).mount('#app')
}

lucide.createIcons()

app({
    setup() {
        const { ref, reactive, computed, } = Vue

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
            const curLang = document.documentElement.lang || 'zh-TW'

            // switch language
            const changeLang = curLang === 'zh-TW' ? 'en-US' : 'zh-TW'

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

                console.log(`Language switched to: ${changeLang}`)

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

        //#region Pop-Up Events

        // Popup State
        const popupState = reactive({
            isVisible: false,
            type: '',
            title: ''
        })

        // Popup Data Storage
        const popupData = reactive({
            Search: [],
            Notify: [],
            Follows: [],
            Collects: []
        })

        // popup helper
        const getPopupTitle = type => {
            const titles = {
                'Search': '搜尋',
                'Notify': '通知',
                'Follows': '追蹤',
                'Collects': '收藏'
            }

            return titles[type] || '視窗'
        }

        // Update popup data
        const updatePopupData = (type, data) => {
            if (popupData[type] !== undefined) popupData[type] = data
        }

        // Popup click
        const openPopup = async type => {
            popupState.type = type
            popupState.title = getPopupTitle(type)
            popupState.isVisible = true

            try {
                const res = await fetch('/api/' + type.toLowerCase())
                const data = await res.json()

                updatePopupData(type, data)
            } catch (err) {
                console.log('Fetch Error:', err)
            }
        }

        const closePopup = () => {
            popupState.isVisible = false
            popupState.type = ''
        }

        // Global Methods
        window.toggleFunc = (show, type) => show ? openPopup(type) : closePopup()

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

        //#region Search

        const searchQuery = ref('')

        //#endregion

        return {
            // language
            isCollapsed,
            toggleSidebar,
            toggleLang,

            // auth
            logout,
            login,

            // pop-up
            popupState,
            popupData,
            getPopupTitle,
            openPopup,
            closePopup,
            // 為新版 popup 提供向後兼容
            isOpen: computed(() => popupState.isVisible),
            closeCollectModal: closePopup,

            //
            searchQuery,
        }
    }
})