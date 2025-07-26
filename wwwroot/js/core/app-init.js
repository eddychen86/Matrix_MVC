/**
 * 應用程式初始化器
 * 負責 Vue 應用程式的創建和掛載
 */

window.MatrixCore = window.MatrixCore || {};

window.MatrixCore.AppInit = {
    /**
     * 初始化 Matrix 應用程式
     */
    initialize() {
        const { createApp } = Vue;
        
        return createApp({
            setup() {
                // 獲取狀態管理
                const state = window.MatrixCore.AppState.createAppState();
                
                // 獲取組件方法
                const popupMethods = window.MatrixCore.PopupManager.createMethods(state);
                const sidebarMethods = window.MatrixCore.SidebarManager.createMethods(state);
                
                // 獲取工具方法
                const formatMethods = window.MatrixCore.UseFormatting();
                const languageMethods = {
                    toggleLang: window.MatrixCore.Utils.LanguageUtils.toggleLang.bind(window.MatrixCore.Utils.LanguageUtils),
                    getCurrentLang: window.MatrixCore.Utils.LanguageUtils.getCurrentLang.bind(window.MatrixCore.Utils.LanguageUtils)
                };
                
                // 設定全域方法
                this.setupGlobalMethods({
                    ...popupMethods,
                    ...sidebarMethods,
                    ...formatMethods,
                    ...languageMethods
                });

                return {
                    ...state,
                    ...popupMethods,
                    ...sidebarMethods,
                    ...formatMethods,
                    ...languageMethods
                };
            }
        });
    },

    /**
     * 設定全域方法
     */
    setupGlobalMethods(methods) {
        // 舊版兼容性
        window.toggleFunc = (show, type) => {
            if (show) {
                methods.openPopup(type);
            } else {
                methods.closePopup();
            }
        };

        // 新版全域 API
        window.matrixApp = {
            openPopup: methods.openPopup,
            closePopup: methods.closePopup,
            toggleSidebar: methods.toggleSidebar,
            toggleLanguage: methods.toggleLang,
            formatDate: methods.formatDate,
            timeAgo: methods.timeAgo
        };
    },

    /**
     * 自動掛載應用程式
     */
    autoMount() {
        document.addEventListener('DOMContentLoaded', () => {
            if (typeof Vue !== 'undefined') {
                const app = this.initialize();
                window.popupApp = app.mount('#app');
                console.log('Matrix App initialized successfully');
            } else {
                console.error('Vue.js not loaded');
            }
        });
    }
};