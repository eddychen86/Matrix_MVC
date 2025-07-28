/**
 * Matrix 應用程式主入口 - 純 Vue 3 + const functional 實現
 * 負責初始化所有模塊並根據頁面類型載入相應的邏輯
 * 
 * 架構特點：
 * - 完全移除 jQuery 依賴
 * - 統一使用 Vue 3 setup() 語法
 * - const functional 模式保證一致性
 * - 原生 DOM API 替代 jQuery 操作
 * - 命名空間模式減少全域污染
 * 
 * 模塊結構：
 * - utils/: 工具函數 (formatting, api, dom)
 * - core/: 核心管理器 (auth-manager, popup-manager, language-manager)
 * - hooks/: 可重用邏輯 (usePasswordToggle, useFormValidation, useAuthForm)
 * - components/: Vue 組件 (main-app, auth-forms)
 * - pages/: 頁面專用邏輯 (home, error)
 */

// 創建 Matrix 命名空間以減少全域污染
window.Matrix = window.Matrix || {
    utils: {},
    core: {},
    hooks: {},
    components: {},
    pages: {}
};

/**
 * Matrix 應用程式函數式實現
 * 使用 const functional 模式而非 class
 */
const useMatrixApp = () => {
    // 應用狀態
    let isInitialized = false;
    let authManager = null;
    let loginPopupManager = null;
    let languageManager = null;

    /**
     * 初始化應用程式
     */
    const init = async () => {
        if (isInitialized) {
            console.warn('Matrix App already initialized');
            return;
        }

        try {
            console.log('Initializing Matrix App...');

            // 初始化 Lucide 圖標
            initLucideIcons();

            // 初始化核心管理器
            initCoreManagers();

            // 根據頁面類型初始化相應的應用
            initPageSpecificApps();

            // 設置全域方法
            setupGlobalMethods();

            // 添加全域事件監聽器
            setupGlobalEventListeners();

            isInitialized = true;
            console.log('✅ Matrix App initialized successfully');

        } catch (error) {
            console.error('❌ Error initializing Matrix App:', error);
            
            // 錯誤恢復機制
            handleInitializationError(error);
        }
    };

    /**
     * 初始化 Lucide 圖標
     */
    const initLucideIcons = () => {
        if (typeof lucide !== 'undefined') {
            lucide.createIcons();
            console.log('📦 Lucide icons initialized');
        } else {
            console.warn('⚠️ Lucide not found, icons may not display');
        }
    };

    /**
     * 初始化核心管理器
     */
    const initCoreManagers = () => {
        try {
            // 初始化認證管理器
            authManager = Matrix.core.useAuthManager();
            console.log('🔐 Auth Manager initialized');
            
            // 初始化登入彈窗管理器
            loginPopupManager = Matrix.core.useLoginPopupManager();
            loginPopupManager.init();
            console.log('🪟 Login Popup Manager initialized');

            // 初始化語言管理器
            languageManager = Matrix.core.useLanguageManager();
            console.log('🌍 Language Manager initialized');

        } catch (error) {
            console.error('Error initializing core managers:', error);
            throw error;
        }
    };

    /**
     * 根據頁面類型初始化相應的應用
     */
    const initPageSpecificApps = () => {
        const path = window.location.pathname.toLowerCase();
        console.log(`📄 Initializing page-specific apps for: ${path}`);
        
        try {
            // 主應用程式 (如果存在 #app 元素)
            if (document.getElementById('app')) {
                initMainApp();
                console.log('🏠 Main App initialized');
            }

            // 認證相關頁面
            if (document.getElementById('auth-body')) {
                initAuthPages(path);
                console.log(`🔑 Auth pages initialized for: ${path}`);
            }

            // 錯誤頁面
            if (document.getElementById('error-body')) {
                initErrorPage();
                console.log('❌ Error page initialized');
            }

            // 首頁特定功能
            if (path === '/' || path === '/home' || path === '/home/index') {
                initHomePage();
                console.log('🏡 Home page features initialized');
            }

        } catch (error) {
            console.error('Error initializing page-specific apps:', error);
            // 不拋出錯誤，允許部分功能載入失敗
        }
    };

    /**
     * 初始化主應用程式
     */
    const initMainApp = () => {
        try {
            const mainApp = Matrix.components.useMainApp();
            mainApp.initMainVueApp();
        } catch (error) {
            console.error('Error initializing main app:', error);
        }
    };

    /**
     * 初始化認證頁面
     * @param {string} path - 當前頁面路徑
     */
    const initAuthPages = (path) => {
        try {
            const authForms = Matrix.components.useAuthForms();

            // 初始化認證布局（所有認證頁面共用）
            authForms.initAuthLayout();
            
            // 添加輸入框焦點效果
            setTimeout(() => {
                authForms.addInputFocusEffects();
            }, 100);

            // 根據路徑初始化特定的認證表單
            if (path === '/login') {
                authForms.initLoginApp();
                console.log('📝 Login form initialized');
            } else if (path === '/register') {
                authForms.initRegisterApp();
                console.log('📝 Register form initialized');
            }

        } catch (error) {
            console.error('Error initializing auth pages:', error);
        }
    };

    /**
     * 初始化錯誤頁面
     */
    const initErrorPage = () => {
        try {
            const errorPage = Matrix.pages.useErrorPage();
            errorPage.initErrorPage();
        } catch (error) {
            console.error('Error initializing error page:', error);
        }
    };

    /**
     * 初始化首頁
     */
    const initHomePage = () => {
        try {
            const homePage = Matrix.pages.useHomePage();
            homePage.initHomePage();
        } catch (error) {
            console.error('Error initializing home page:', error);
        }
    };

    /**
     * 設置全域方法
     */
    const setupGlobalMethods = () => {
        try {
            // 將管理器掛載到全域供其他腳本使用
            window.authManager = authManager;
            window.loginPopupManager = loginPopupManager;
            window.languageManager = languageManager;
            
            // 全域登出函數
            window.logout = () => {
                if (authManager) {
                    authManager.logout();
                } else {
                    console.warn('Auth manager not available');
                }
            };

            // 全域語言切換函數
            window.toggleLanguage = () => {
                if (languageManager) {
                    languageManager.toggleLanguage();
                } else {
                    console.warn('Language manager not available');
                }
            };

            // 全域重新初始化函數
            window.reinitializeMatrixApp = reinitialize;

            console.log('🌐 Global methods set up');

        } catch (error) {
            console.error('Error setting up global methods:', error);
        }
    };

    /**
     * 設置全域事件監聽器
     */
    const setupGlobalEventListeners = () => {
        try {
            // 頁面卸載前的清理
            window.addEventListener('beforeunload', cleanup);

            // 視窗大小變化時重新初始化圖標
            let resizeTimeout;
            window.addEventListener('resize', () => {
                clearTimeout(resizeTimeout);
                resizeTimeout = setTimeout(() => {
                    initLucideIcons();
                }, 150);
            });

            // 監聽認證狀態變更
            window.addEventListener('authStatusChanged', (event) => {
                console.log('Auth status changed:', event.detail);
                
                // 根據認證狀態更新 UI
                const isAuthenticated = event.detail.isAuthenticated;
                document.body.classList.toggle('authenticated', isAuthenticated);
                document.body.classList.toggle('unauthenticated', !isAuthenticated);
            });

            // 監聽頁面可見性變化
            document.addEventListener('visibilitychange', () => {
                if (!document.hidden) {
                    // 頁面重獲焦點時重新初始化圖標
                    initLucideIcons();
                }
            });

            console.log('👂 Global event listeners set up');

        } catch (error) {
            console.error('Error setting up event listeners:', error);
        }
    };

    /**
     * 處理初始化錯誤
     */
    const handleInitializationError = (error) => {
        // 顯示用戶友好的錯誤訊息
        const errorMessage = '應用程式初始化失敗，請重新整理頁面';
        
        // 嘗試顯示錯誤訊息
        const showError = () => {
            const errorDiv = document.createElement('div');
            errorDiv.style.cssText = `
                position: fixed;
                top: 20px;
                right: 20px;
                background: #ff4444;
                color: white;
                padding: 12px 20px;
                border-radius: 8px;
                z-index: 9999;
                font-family: system-ui, sans-serif;
                box-shadow: 0 4px 12px rgba(0,0,0,0.15);
            `;
            errorDiv.textContent = errorMessage;
            document.body.appendChild(errorDiv);

            // 5秒後自動移除
            setTimeout(() => {
                if (errorDiv.parentNode) {
                    errorDiv.parentNode.removeChild(errorDiv);
                }
            }, 5000);
        };

        // 確保 DOM 已加載再顯示錯誤
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', showError);
        } else {
            showError();
        }
    };

    /**
     * 頁面卸載時的清理工作
     */
    const cleanup = () => {
        try {
            // 移除事件監聽器
            window.removeEventListener('beforeunload', cleanup);
            
            // 清理其他資源
            console.log('🧹 Matrix App cleaned up');
        } catch (error) {
            console.error('Error during cleanup:', error);
        }
    };

    /**
     * 重新初始化（用於動態內容更新後）
     */
    const reinitialize = () => {
        try {
            console.log('🔄 Reinitializing Matrix App...');
            
            // 重新初始化圖標
            initLucideIcons();
            
            // 重新初始化登入彈窗監控
            if (loginPopupManager) {
                loginPopupManager.setupScrollMonitoring();
            }

            console.log('✅ Matrix App reinitialized');
        } catch (error) {
            console.error('❌ Error reinitializing Matrix App:', error);
        }
    };

    /**
     * 獲取應用狀態
     */
    const getAppState = () => ({
        isInitialized,
        authManager: !!authManager,
        loginPopupManager: !!loginPopupManager,
        languageManager: !!languageManager,
        currentPath: window.location.pathname
    });

    return {
        init,
        cleanup,
        reinitialize,
        getAppState,
        
        // 管理器引用
        getAuthManager: () => authManager,
        getLoginPopupManager: () => loginPopupManager,
        getLanguageManager: () => languageManager
    };
};

// 創建全域應用實例
const matrixApp = useMatrixApp();

/**
 * DOM 載入完成後自動初始化
 */
const initializeWhenReady = () => {
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', () => {
            console.log('📖 DOM loaded, initializing Matrix App...');
            matrixApp.init();
        });
    } else {
        console.log('📖 DOM already loaded, initializing Matrix App...');
        matrixApp.init();
    }
};

/**
 * 頁面載入完成後的額外初始化
 */
window.addEventListener('load', () => {
    // 確保 Lucide 圖標正確載入
    if (typeof lucide !== 'undefined') {
        lucide.createIcons();
        console.log('🎨 Post-load icon refresh completed');
    }
});

// 立即檢查並初始化
initializeWhenReady();

// 將應用實例掛載到全域供外部使用 - 同時支援命名空間和向後兼容
window.Matrix.app = matrixApp;
window.matrixApp = matrixApp; // 向後兼容

// 開發環境下的額外調試信息
if (typeof window !== 'undefined' && window.location.hostname === 'localhost') {
    console.log(`
🚀 Matrix App Development Mode
📁 Module Structure:
   📂 utils/     - 工具函數 (formatting, api, dom)
   📂 core/      - 核心管理器 (auth, popup, language)
   📂 hooks/     - 可重用邏輯 (Vue 3 hooks)
   📂 components/- Vue 組件 (main-app, auth-forms)
   📂 pages/     - 頁面專用邏輯 (home, error)
   
🔧 Available Commands:
   - Matrix.app.reinitialize()          // 重新初始化 (推薦)
   - Matrix.app.getAppState()           // 獲取應用狀態 (推薦)
   - window.matrixApp.reinitialize()    // 重新初始化 (向後兼容)
   - window.matrixApp.getAppState()     // 獲取應用狀態 (向後兼容)
   - window.logout()                    // 全域登出
   - window.toggleLanguage()            // 切換語言
    `);
}

// 導出給其他模塊使用（如果支持模塊系統）
if (typeof module !== 'undefined' && module.exports) {
    module.exports = { useMatrixApp, matrixApp };
}