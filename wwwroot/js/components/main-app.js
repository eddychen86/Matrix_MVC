/**
 * 主要應用程式組件
 * 處理側邊欄、彈窗和搜尋功能
 */
const useMainApp = () => {
    /**
     * 創建主要 Vue 應用程式
     */
    const initMainVueApp = () => {
        if (typeof Vue === 'undefined' || !document.getElementById('app')) {
            return;
        }

        const { createApp, ref, reactive, computed } = Vue;

        createApp({
            setup() {
                const languageManager = useLanguageManager();

                //#region Sidebar State
                const isCollapsed = ref(false);
                
                const toggleSidebar = () => {
                    isCollapsed.value = !isCollapsed.value;
                    if (typeof lucide !== 'undefined') {
                        lucide.createIcons();
                    }
                };
                //#endregion

                //#region Language
                const toggleLang = () => {
                    languageManager.toggleLanguage();
                };
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
                });

                // Popup helper
                const getPopupTitle = type => {
                    const titles = {
                        'Search': '搜尋',
                        'Notify': '通知',
                        'Follows': '追蹤',
                        'Collects': '收藏'
                    };

                    return titles[type] || '視窗';
                };

                // Update popup data
                const updatePopupData = (type, data) => {
                    if (popupData[type] !== undefined) {
                        popupData[type] = data;
                    }
                };

                // Popup operations
                const openPopup = async type => {
                    popupState.type = type;
                    popupState.title = getPopupTitle(type);
                    popupState.isVisible = true;

                    try {
                        const api = useApi();
                        const result = await api.get('/api/' + type.toLowerCase());
                        
                        if (result.success) {
                            updatePopupData(type, result.data);
                        }
                    } catch (err) {
                        console.log('Fetch Error:', err);
                    }
                };

                const closePopup = () => {
                    popupState.isVisible = false;
                    popupState.type = '';
                };

                // Global Methods for backward compatibility
                window.toggleFunc = (show, type) => show ? openPopup(type) : closePopup();
                //#endregion

                //#region Search
                const searchQuery = ref('');
                //#endregion

                return {
                    // Sidebar
                    isCollapsed,
                    toggleSidebar,

                    // Language
                    toggleLang,

                    // Pop-up
                    popupState,
                    popupData,
                    getPopupTitle,
                    openPopup,
                    closePopup,
                    // 為新版 popup 提供向後兼容
                    isOpen: computed(() => popupState.isVisible),
                    closeCollectModal: closePopup,

                    // Search
                    searchQuery,
                };
            }
        }).mount('#app');
    };

    return {
        initMainVueApp
    };
};

// 將主要應用程式組件掛載到全域 - 同時支援命名空間和向後兼容
window.Matrix.components.useMainApp = useMainApp;
window.useMainApp = useMainApp; // 向後兼容