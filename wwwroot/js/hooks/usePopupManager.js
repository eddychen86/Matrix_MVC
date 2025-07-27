let maxVisibleArticles = 10; // 訪客最多能看的文章數量
let articleCount = 0;
let isGuest = true;
let popupShown = false;

/**
 * 檢查認證狀態
 */
const checkAuthStatus = async () => {
    try {
        const response = await fetch('/api/auth/status', {
            credentials: 'include'
        });
        
        if (response.ok) {
            const authData = await response.json();
            isGuest = !authData.isAuthenticated;
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
 * 顯示登入彈窗
 */
const showLoginPopup = () => {
    // 創建彈窗 HTML
    const popupHTML = `
        <div id="login-popup-overlay" class="fixed inset-0 bg-black bg-opacity-50 z-50 flex items-center justify-center">
            <div class="bg-white rounded-lg p-6 max-w-md mx-4">
                <div class="text-center">
                    <h3 class="text-lg font-semibold text-gray-900 mb-4">探索更多內容</h3>
                    <p class="text-gray-600 mb-6">您已瀏覽了免費內容的限制。登入以繼續探索更多精彩內容。</p>
                    <div class="flex space-x-4">
                        <a href="/login" class="flex-1 bg-orange-500 text-white py-2 px-4 rounded-full hover:bg-orange-600 transition-colors">
                            登入
                        </a>
                        <button onclick="window.loginPopupManager.hideLoginPopup()" class="flex-1 bg-gray-300 text-gray-700 py-2 px-4 rounded-full hover:bg-gray-400 transition-colors">
                            關閉
                        </button>
                    </div>
                </div>
            </div>
        </div>
    `;

    // 添加到頁面
    document.body.insertAdjacentHTML('beforeend', popupHTML);

    // 添加樣式
    addPopupStyles();
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
 * 添加彈窗樣式
 */
const addPopupStyles = () => {
    if (document.getElementById('login-popup-styles')) return;

    const styles = `
        <style id="login-popup-styles">
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
        </style>
    `;

    document.head.insertAdjacentHTML('beforeend', styles);
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

// 執行初始化
init();

return {
    checkAuthStatus,
    setupScrollMonitoring,
    showLoginPopup,
    hideLoginPopup,
    addPopupStyles,
    init
};