/**
 * 登入彈窗管理器
 * 負責訪客瀏覽限制和登入提示彈窗
 */
const useLoginPopupManager = () => {
    let maxVisibleArticles = 10; // 訪客最多能看的文章數量
    let articleCount = 0;
    let isGuest = true;
    let popupShown = false;

    const api = useApi();
    const { createElement } = useDom();

    /**
     * 檢查認證狀態
     */
    const checkAuthStatus = async () => {
        try {
            const result = await api.checkAuthStatus();
            
            if (result.success) {
                const authData = result.data;
                isGuest = !authData.authenticated;
            }
        } catch (error) {
            console.error('Error checking auth status:', error);
            isGuest = true;
        }
    };

    /**
     * 設定滾動監控
     */
    const setupScrollMonitoring = () => {
        if (!isGuest) return;

        // 監控文章元素
        const articles = document.querySelectorAll('[data-article-index]');
        
        if (articles.length === 0) return;

        // 使用 Intersection Observer 監控可見性
        const observer = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    const articleIndex = parseInt(entry.target.dataset.articleIndex);
                    
                    if (articleIndex >= maxVisibleArticles && !popupShown) {
                        showLoginPopup();
                        popupShown = true;
                    }
                }
            });
        }, {
            threshold: 0.5 // 當文章 50% 可見時觸發
        });

        // 觀察所有文章
        articles.forEach(article => {
            observer.observe(article);
        });
    };

    /**
     * 創建彈窗樣式
     */
    const createPopupStyles = () => {
        if (document.getElementById('login-popup-styles')) return;

        const styles = createElement('style', {
            id: 'login-popup-styles',
            innerHTML: `
                #login-popup-overlay {
                    animation: fadeIn 0.3s ease-out;
                }
                
                #login-popup-overlay > div {
                    animation: slideIn 0.3s ease-out;
                }
                
                @keyframes fadeIn {
                    from { opacity: 0; }
                    to { opacity: 1; }
                }
                
                @keyframes slideIn {
                    from { 
                        opacity: 0;
                        transform: translateY(-20px);
                    }
                    to { 
                        opacity: 1;
                        transform: translateY(0);
                    }
                }
            `
        });

        document.head.appendChild(styles);
    };

    /**
     * 顯示登入彈窗
     */
    const showLoginPopup = () => {
        // 創建彈窗覆蓋層
        const overlay = createElement('div', {
            id: 'login-popup-overlay',
            className: 'fixed inset-0 bg-black bg-opacity-50 z-50 flex items-center justify-center'
        });

        // 創建彈窗內容
        const popup = createElement('div', {
            className: 'bg-white rounded-lg p-6 max-w-md mx-4'
        });

        const content = createElement('div', {
            className: 'text-center',
            innerHTML: `
                <h3 class="text-lg font-semibold text-gray-900 mb-4">探索更多內容</h3>
                <p class="text-gray-600 mb-6">您已瀏覽了免費內容的限制。登入以繼續探索更多精彩內容。</p>
            `
        });

        // 創建按鈕容器
        const buttonContainer = createElement('div', {
            className: 'flex space-x-4'
        });

        // 登入按鈕
        const loginBtn = createElement('a', {
            href: '/login',
            className: 'flex-1 bg-orange-500 text-white py-2 px-4 rounded-full hover:bg-orange-600 transition-colors text-center',
            innerHTML: '登入'
        });

        // 關閉按鈕
        const closeBtn = createElement('button', {
            className: 'flex-1 bg-gray-300 text-gray-700 py-2 px-4 rounded-full hover:bg-gray-400 transition-colors',
            innerHTML: '關閉'
        });

        closeBtn.addEventListener('click', hideLoginPopup);

        buttonContainer.appendChild(loginBtn);
        buttonContainer.appendChild(closeBtn);
        content.appendChild(buttonContainer);
        popup.appendChild(content);
        overlay.appendChild(popup);

        // 添加樣式和彈窗到頁面
        createPopupStyles();
        document.body.appendChild(overlay);

        // 點擊覆蓋層關閉彈窗
        overlay.addEventListener('click', (e) => {
            if (e.target === overlay) {
                hideLoginPopup();
            }
        });
    };

    /**
     * 隱藏登入彈窗
     */
    const hideLoginPopup = () => {
        const popup = document.getElementById('login-popup-overlay');
        if (popup) {
            popup.remove();
        }
        popupShown = false;
    };

    /**
     * 設定最大可見文章數
     */
    const setMaxVisibleArticles = (count) => {
        maxVisibleArticles = count;
    };

    /**
     * 初始化
     */
    const init = () => {
        // 檢查用戶認證狀態
        checkAuthStatus();
        
        // 監聽認證狀態變更
        window.addEventListener('authStatusChanged', (event) => {
            isGuest = !event.detail.isAuthenticated;
            if (!isGuest) {
                hideLoginPopup();
            }
        });

        // 監聽滾動事件
        setupScrollMonitoring();
    };

    return {
        checkAuthStatus,
        setupScrollMonitoring,
        showLoginPopup,
        hideLoginPopup,
        setMaxVisibleArticles,
        init
    };
};

// 將登入彈窗管理器掛載到全域 - 同時支援命名空間和向後兼容
window.Matrix.core.useLoginPopupManager = useLoginPopupManager;
window.useLoginPopupManager = useLoginPopupManager; // 向後兼容