/**
 * 錯誤頁面處理
 * 處理錯誤頁面的互動邏輯
 */
const useErrorPage = () => {
    /**
     * 初始化錯誤頁面
     */
    const initErrorPage = () => {
        const { ready, on } = useDom();

        // 初始化 Lucide 圖標
        if (typeof lucide !== 'undefined') {
            lucide.createIcons();
        }

        // 等待 DOM 加載完成
        ready(() => {
            // 綁定 "try again" 按鈕事件
            const tryAgainBtn = document.getElementById('tryAgain');
            if (tryAgainBtn) {
                on(tryAgainBtn, 'click', () => {
                    window.location.reload();
                });
            }

            // 綁定 "go back" 按鈕事件
            const goBackBtn = document.getElementById('goBack');
            if (goBackBtn) {
                on(goBackBtn, 'click', () => {
                    if (window.history.length > 1) {
                        window.history.back();
                    } else {
                        window.location.href = '/';
                    }
                });
            }

            // 綁定 "go home" 按鈕事件
            const goHomeBtn = document.getElementById('goHome');
            if (goHomeBtn) {
                on(goHomeBtn, 'click', () => {
                    window.location.href = '/';
                });
            }
        });
    };

    /**
     * 顯示自定義錯誤訊息
     * @param {string} message - 錯誤訊息
     * @param {string} type - 錯誤類型
     */
    const showErrorMessage = (message, type = 'error') => {
        const { createElement } = useDom();

        const errorContainer = createElement('div', {
            className: `error-message error-${type}`,
            innerHTML: `
                <div class="error-icon">
                    <i data-lucide="alert-circle"></i>
                </div>
                <div class="error-content">
                    <p>${message}</p>
                </div>
            `
        });

        // 將錯誤訊息添加到頁面
        const errorBody = document.getElementById('error-body');
        if (errorBody) {
            errorBody.appendChild(errorContainer);
        }

        // 重新創建圖標
        if (typeof lucide !== 'undefined') {
            lucide.createIcons();
        }
    };

    /**
     * 自動重試機制
     * @param {Function} retryFunction - 重試函數
     * @param {number} maxRetries - 最大重試次數
     * @param {number} delay - 重試延遲（毫秒）
     */
    const autoRetry = async (retryFunction, maxRetries = 3, delay = 1000) => {
        let attempts = 0;

        const attempt = async () => {
            try {
                return await retryFunction();
            } catch (error) {
                attempts++;
                
                if (attempts < maxRetries) {
                    console.log(`Retry attempt ${attempts}/${maxRetries} after ${delay}ms`);
                    await new Promise(resolve => setTimeout(resolve, delay));
                    return attempt();
                } else {
                    throw error;
                }
            }
        };

        return attempt();
    };

    return {
        initErrorPage,
        showErrorMessage,
        autoRetry
    };
};

// 將錯誤頁面處理掛載到全域 - 同時支援命名空間和向後兼容
window.Matrix.pages.useErrorPage = useErrorPage;
window.useErrorPage = useErrorPage; // 向後兼容