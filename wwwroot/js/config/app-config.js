/**
 * 應用程式配置
 * 集中管理應用程式的設定和常數
 */

window.MatrixCore = window.MatrixCore || {};
window.MatrixCore.Config = window.MatrixCore.Config || {};

window.MatrixCore.Config.AppConfig = {
    /**
     * 應用程式資訊
     */
    app: {
        name: 'Matrix',
        version: '1.0.0',
        description: 'Web3 社群平台'
    },

    /**
     * API 設定
     */
    api: {
        baseUrl: '',
        timeout: 10000,
        retryAttempts: 3,
        endpoints: {
            auth: '/api/auth',
            search: '/api/search',
            notify: '/api/notify',
            follows: '/api/follows',
            collects: '/api/collects'
        }
    },

    /**
     * UI 設定
     */
    ui: {
        defaultLang: 'zh-TW',
        supportedLangs: ['zh-TW', 'en-US'],
        popup: {
            animation: 'fade',
            duration: 300
        },
        sidebar: {
            breakpoint: 768,
            defaultCollapsed: false
        },
        guest: {
            maxVisibleArticles: 10,
            showLoginPromptAfter: 10
        }
    },

    /**
     * 儲存設定
     */
    storage: {
        keys: {
            language: 'matrix-lang',
            theme: 'matrix-theme',
            sidebarState: 'matrix-sidebar-collapsed'
        }
    },

    /**
     * 功能開關
     */
    features: {
        darkMode: true,
        multiLanguage: true,
        notifications: true,
        guestMode: true
    },

    /**
     * 偵錯設定
     */
    debug: {
        enabled: false,
        logLevel: 'info' // 'debug', 'info', 'warn', 'error'
    },

    /**
     * 取得設定值
     */
    get(path, defaultValue = null) {
        const keys = path.split('.');
        let current = this;
        
        for (const key of keys) {
            if (current && typeof current === 'object' && key in current) {
                current = current[key];
            } else {
                return defaultValue;
            }
        }
        
        return current;
    }
};