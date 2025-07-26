/**
 * Matrix Core - 模組化版本主入口檔案
 * 統一載入和初始化所有模組
 */

// 初始化 MatrixCore 命名空間
window.MatrixCore = window.MatrixCore || {};

/**
 * Matrix 應用程式主控制器
 */
window.MatrixCore.Application = {
    /**
     * 檢查所有必要模組是否載入
     */
    checkDependencies() {
        const requiredModules = [
            'MatrixCore.Config.AppConfig',
            'MatrixCore.Config.EventConfig',
            'MatrixCore.Utils.PopupUtils',
            'MatrixCore.Utils.LanguageUtils',
            'MatrixCore.Utils.CommonUtils',
            'MatrixCore.Services.ApiService',
            'MatrixCore.Services.AuthService',
            'MatrixCore.AppState',
            'MatrixCore.PopupManager',
            'MatrixCore.SidebarManager',
            'MatrixCore.UseFormatting',
            'MatrixCore.AppInit'
        ];

        const missing = [];
        for (const module of requiredModules) {
            if (!this.getNestedProperty(window, module)) {
                missing.push(module);
            }
        }

        if (missing.length > 0) {
            console.error('Missing required modules:', missing);
            return false;
        }

        return true;
    },

    /**
     * 取得嵌套屬性
     */
    getNestedProperty(obj, path) {
        return path.split('.').reduce((current, key) => {
            return current && current[key] !== undefined ? current[key] : null;
        }, obj);
    },

    /**
     * 初始化應用程式
     */
    async initialize() {
        try {
            // 檢查相依性
            if (!this.checkDependencies()) {
                throw new Error('Missing required dependencies');
            }

            // 初始化配置
            window.MatrixCore.Config.EventConfig.initialize();

            // 檢查認證狀態
            await window.MatrixCore.Services.AuthService.checkAuthStatus();

            // 初始化 Vue 應用程式
            window.MatrixCore.AppInit.autoMount();

            console.log('Matrix Core initialized successfully');
            
            // 觸發應用程式初始化完成事件
            window.MatrixCore.Config.EventConfig.emit('appInitialized');

        } catch (error) {
            console.error('Matrix Core initialization failed:', error);
            
            // 觸發初始化錯誤事件
            window.MatrixCore.Config.EventConfig.emit('appInitializationError', {
                error: error.message
            });
        }
    },

    /**
     * 版本資訊
     */
    getVersion() {
        return window.MatrixCore.Config.AppConfig.get('app.version', '1.0.0');
    },

    /**
     * 取得應用程式狀態
     */
    getStatus() {
        return {
            version: this.getVersion(),
            isAuthenticated: window.MatrixCore.Services.AuthService.isUserAuthenticated(),
            currentUser: window.MatrixCore.Services.AuthService.getCurrentUser(),
            currentLang: window.MatrixCore.Utils.LanguageUtils.getCurrentLang()
        };
    }
};

// 向後兼容性：保留舊版 API
window.useFormatting = () => window.MatrixCore.UseFormatting();

// 自動初始化（只有在所有模組都載入後才會執行）
document.addEventListener('DOMContentLoaded', () => {
    // 延遲執行以確保所有模組都已載入
    setTimeout(() => {
        window.MatrixCore.Application.initialize();
    }, 100);
});