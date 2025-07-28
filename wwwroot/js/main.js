/**
 * Matrix 統一模組載入器
 * 負責按正確順序載入所有模組化的 JavaScript 文件
 * 
 * 使用方式：
 * 在 .cshtml 中只需引用這一個文件：
 * <script src="/js/main.js"></script>
 * 
 * 架構：
 * - 自動處理模組依賴順序
 * - 統一錯誤處理和載入狀態
 * - 提供載入進度回饋
 * - 支援命名空間模式
 */

const MatrixLoader = (() => {
    // 模組載入清單 - 順序很重要！
    const modules = [
        // 1. 基礎工具模組（無依賴）
        { path: '/js/utils/dom.js', name: 'DOM 工具' },
        { path: '/js/utils/api.js', name: 'API 工具' },
        { path: '/js/utils/formatting.js', name: '格式化工具' },

        // 2. 核心管理器（依賴工具模組）
        { path: '/js/core/auth-manager.js', name: '認證管理器' },
        { path: '/js/core/language-manager.js', name: '語言管理器' },
        { path: '/js/core/popup-manager.js', name: '彈窗管理器' },

        // 3. Vue Hooks（依賴工具和核心模組）
        { path: '/js/hooks/usePasswordToggle.js', name: '密碼切換 Hook' },
        { path: '/js/hooks/useFormValidation.js', name: '表單驗證 Hook' },
        { path: '/js/hooks/useAuthForm.js', name: '認證表單 Hook' },

        // 4. Vue 組件（依賴 Hooks）
        { path: '/js/components/main-app.js', name: '主應用組件' },
        { path: '/js/components/auth-forms.js', name: '認證表單組件' },

        // 5. 頁面專用模組（依賴組件）
        { path: '/js/pages/home.js', name: '首頁邏輯' },
        { path: '/js/pages/error.js', name: '錯誤頁面邏輯' },

        // 6. 主應用入口（最後載入，依賴所有模組）
        { path: '/js/main-new.js', name: 'Matrix 主應用' }
    ];

    let loadedModules = 0;
    let isLoading = false;
    let loadStartTime = 0;

    /**
     * 動態載入單個 JavaScript 模組
     * @param {string} src - 模組路徑
     * @param {string} name - 模組名稱（用於日誌）
     * @returns {Promise} - 載入 Promise
     */
    const loadScript = (src, name) => {
        return new Promise((resolve, reject) => {
            // 檢查是否已經載入
            const existingScript = document.querySelector(`script[src="${src}"]`);
            if (existingScript) {
                console.log(`⚠️ ${name} 已載入，跳過`);
                resolve();
                return;
            }

            const script = document.createElement('script');
            script.src = src;
            script.async = false; // 確保順序載入
            
            script.onload = () => {
                loadedModules++;
                const progress = Math.round((loadedModules / modules.length) * 100);
                console.log(`✅ [${progress}%] ${name} 載入完成`);
                resolve();
            };
            
            script.onerror = (error) => {
                console.error(`❌ ${name} 載入失敗:`, error);
                reject(new Error(`Failed to load ${name} from ${src}`));
            };

            // 添加到 head 而非 body，確保早期載入
            document.head.appendChild(script);
        });
    };

    /**
     * 顯示載入進度
     */
    const showLoadingProgress = () => {
        if (typeof window !== 'undefined' && window.location.hostname === 'localhost') {
            console.log(`
🚀 Matrix 模組載入器啟動
📦 待載入模組: ${modules.length} 個
⏱️ 開始時間: ${new Date().toLocaleTimeString()}
            `);
        }
    };

    /**
     * 顯示載入完成信息
     */
    const showLoadingComplete = () => {
        const loadTime = performance.now() - loadStartTime;
        const formattedTime = loadTime.toFixed(2);
        
        console.log(`
✅ Matrix 所有模組載入完成！
📊 載入統計:
   - 模組數量: ${modules.length}
   - 載入時間: ${formattedTime}ms
   - 平均時間: ${(loadTime / modules.length).toFixed(2)}ms/模組

🎯 可用功能:
   - Matrix.utils.*     // 工具函數
   - Matrix.core.*      // 核心管理器  
   - Matrix.hooks.*     // Vue Hooks
   - Matrix.components.*// Vue 組件
   - Matrix.pages.*     // 頁面邏輯
   - Matrix.app.*       // 主應用實例

💡 開發提示:
   - 使用 Matrix.* 命名空間（推薦）
   - 向後兼容 window.use* 方式
        `);
    };

    /**
     * 處理載入錯誤
     * @param {Error} error - 錯誤對象  
     */
    const handleLoadingError = (error) => {
        console.error(`
❌ Matrix 模組載入失敗！
錯誤信息: ${error.message}

🔧 可能的解決方案:
1. 檢查文件路徑是否正確
2. 確認伺服器正在運行
3. 檢查瀏覽器控制台的網路錯誤
4. 確認所有模組文件都存在
        `);

        // 在頁面上顯示錯誤通知
        if (document.body) {
            const errorDiv = document.createElement('div');
            errorDiv.style.cssText = `
                position: fixed;
                top: 20px;
                right: 20px;
                background: #ff4444;
                color: white;
                padding: 15px 20px;
                border-radius: 8px;
                z-index: 9999;
                font-family: system-ui, sans-serif;
                box-shadow: 0 4px 12px rgba(0,0,0,0.15);
                max-width: 300px;
            `;
            errorDiv.innerHTML = `
                <strong>⚠️ 模組載入失敗</strong><br>
                <small>請檢查瀏覽器控制台以獲取詳細信息</small>
            `;
            document.body.appendChild(errorDiv);

            // 5秒後自動移除
            setTimeout(() => {
                if (errorDiv.parentNode) {
                    errorDiv.parentNode.removeChild(errorDiv);
                }
            }, 5000);
        }
    };

    /**
     * 檢查依賴項是否可用
     */
    const checkDependencies = () => {
        const requiredGlobals = [
            { name: 'Vue', check: () => typeof Vue !== 'undefined' },
            { name: 'Lucide', check: () => typeof lucide !== 'undefined' }
        ];

        const missingDeps = requiredGlobals
            .filter(dep => !dep.check())
            .map(dep => dep.name);

        if (missingDeps.length > 0) {
            console.warn(`⚠️ 缺少依賴項: ${missingDeps.join(', ')}`);
            console.warn('某些功能可能無法正常工作');
        }
    };

    /**
     * 主要的模組載入函數
     */
    const loadAllModules = async () => {
        if (isLoading) {
            console.warn('⚠️ 模組載入已在進行中');
            return;
        }

        isLoading = true;
        loadStartTime = performance.now();
        loadedModules = 0;

        try {
            showLoadingProgress();
            
            // 順序載入所有模組
            for (const module of modules) {
                await loadScript(module.path, module.name);
            }

            // 檢查依賴項
            checkDependencies();
            
            showLoadingComplete();
            
            // 觸發載入完成事件
            window.dispatchEvent(new CustomEvent('matrixModulesLoaded', {
                detail: {
                    loadTime: performance.now() - loadStartTime,
                    moduleCount: modules.length
                }
            }));

        } catch (error) {
            handleLoadingError(error);
        } finally {
            isLoading = false;
        }
    };

    /**
     * 獲取載入狀態
     */
    const getLoadingStatus = () => ({
        isLoading,
        loadedModules,
        totalModules: modules.length,
        progress: Math.round((loadedModules / modules.length) * 100)
    });

    /**
     * 重新載入所有模組（開發時使用）
     */
    const reloadModules = async () => {
        if (typeof window !== 'undefined' && window.location.hostname === 'localhost') {
            console.warn('🔄 重新載入所有模組（僅開發模式）');
            
            // 移除已載入的 script 標籤
            modules.forEach(module => {
                const script = document.querySelector(`script[src="${module.path}"]`);
                if (script) {
                    script.remove();
                }
            });

            // 重新載入
            await loadAllModules();
        } else {
            console.warn('⚠️ 重新載入僅在開發模式下可用');
        }
    };

    // 公開接口
    return {
        load: loadAllModules,
        getStatus: getLoadingStatus,
        reload: reloadModules
    };
})();

/**
 * 自動初始化（防重複執行）
 * 等待 DOM 準備就緒後開始載入模組
 */
const initMatrixLoader = () => {
    // 防止重複初始化
    if (window.MatrixLoaderInitialized) {
        console.warn('⚠️ Matrix 載入器已初始化，跳過重複執行');
        return;
    }
    
    window.MatrixLoaderInitialized = true;
    
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', () => {
            console.log('📄 DOM 載入完成，開始載入 Matrix 模組...');
            MatrixLoader.load();
        });
    } else {
        console.log('📄 DOM 已準備就緒，立即載入 Matrix 模組...');
        MatrixLoader.load();
    }
};

// 將載入器掛載到全域（開發時使用）
if (typeof window !== 'undefined') {
    window.MatrixLoader = MatrixLoader;
}

// 立即開始初始化
initMatrixLoader();

// 監聽載入完成事件的範例（可選）
window.addEventListener('matrixModulesLoaded', (event) => {
    const { loadTime, moduleCount } = event.detail;
    
    // 這裡可以添加載入完成後的自定義邏輯
    if (typeof window !== 'undefined' && window.location.hostname === 'localhost') {
        console.log(`🎉 Matrix 已準備就緒！載入了 ${moduleCount} 個模組，耗時 ${loadTime.toFixed(2)}ms`);
    }
});