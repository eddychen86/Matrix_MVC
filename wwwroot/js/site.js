const app = content => {
    if (typeof Vue === 'undefined') {
        console.log('Vue is not loaded.');
        return;
    }
    window.popupApp = Vue.createApp(content).mount('#app');
};

lucide.createIcons();

app({
    setup() {
        const { ref, reactive, computed } = Vue;

        //#region Sidebar State

        const isCollapsed = ref(false);
        
        const toggleSidebar = () => {
            isCollapsed.value = !isCollapsed.value;
            lucide.createIcons()
        }

        //#endregion

        //#region Language

        const toggleLang = () => {
            // current language
            const curLang = document.documentElement.lang || 'zh-TW'

            // switch language
            const changeLang = curLang === 'zh-TW' ? 'en-US' : 'zh-TW'

            // set current language in cookie for 1 year
            // ASP.NET Core 預設的 culture cookie 名稱是 ".AspNetCore.Culture"
            const cultureCookie = `c=${changeLang}|uic=${changeLang}`
            document.cookie = `.AspNetCore.Culture=${cultureCookie}; path=/; max-age=31536000; SameSite=Lax`
            console.log(`Setting culture cookie: ${cultureCookie}`)

            // reload the website let the language change
            window.location.reload()
        }

        //#endregion

        //#region Pop-Up Events

        // Popup State
        const popupState = reactive({
            isVisible: false,
            type: '',
            title: ''
        });

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
            if (popupData[type] !== undefined)
                popupData[type] = data
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
                    headers: {
                        'Content-Type': 'application/json',
                    }
                });

                const result = await response.json();

                if (result.success) {
                    console.log('Logout successful');
                    // 跳轉到首頁或登入頁
                    window.location.href = '/';
                } else {
                    console.error('Logout failed');
                }
            } catch (error) {
                console.error('Error during logout:', error);
            }
        };

        // 登入跳轉功能
        const login = () => {
            window.location.href = '/login';
        };

        //#endregion

        //#region Search

        const searchQuery = ref('');

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
        };
    }
});