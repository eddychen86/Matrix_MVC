/**
 * Login Popup Manager
 * 管理登入彈窗的顯示和訪客權限控制
 */

class LoginPopupManager {
    constructor() {
        this.maxVisibleArticles = 10; // 訪客最多能看的文章數量
        this.articleCount = 0;
        this.isGuest = true;
        this.popupShown = false;
        
        this.init();
    }

    /**
     * 初始化
     */
    init() {
        // 檢查用戶認證狀態
        this.checkAuthStatus();
        
        // 監聽認證狀態變更
        window.addEventListener('authStatusChanged', (event) => {
            this.isGuest = !event.detail.isAuthenticated;
            if (!this.isGuest) {
                this.hideLoginPopup();
            }
        });

        // 監聽滾動事件
        this.setupScrollMonitoring();
    }

    /**
     * 檢查認證狀態
     */
    async checkAuthStatus() {
        try {
            const response = await fetch('/api/auth/status', {
                credentials: 'include'
            });
            
            if (response.ok) {
                const authData = await response.json();
                this.isGuest = !authData.isAuthenticated;
            }
        } catch (error) {
            console.error('Error checking auth status:', error);
            this.isGuest = true;
        }
    }

    /**
     * 設定滾動監控
     */
    setupScrollMonitoring() {
        if (!this.isGuest) return;

        // 監控文章元素
        const articles = document.querySelectorAll('[data-article-index]');
        
        if (articles.length === 0) return;

        // 使用 Intersection Observer 監控可見性
        const observer = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    const articleIndex = parseInt(entry.target.dataset.articleIndex);
                    
                    if (articleIndex >= this.maxVisibleArticles && !this.popupShown) {
                        this.showLoginPopup();
                        this.popupShown = true;
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
    }

    /**
     * 顯示登入彈窗
     */
    showLoginPopup() {
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
        this.addPopupStyles();
    }

    /**
     * 隱藏登入彈窗
     */
    hideLoginPopup() {
        const popup = document.getElementById('login-popup-overlay');
        if (popup) {
            popup.remove();
        }
        this.popupShown = false;
    }

    /**
     * 添加彈窗樣式
     */
    addPopupStyles() {
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
    }
}

// 全域登入彈窗管理器實例
window.loginPopupManager = new LoginPopupManager();

// DOM 載入完成後初始化
document.addEventListener('DOMContentLoaded', function() {
    if (!window.loginPopupManager) {
        window.loginPopupManager = new LoginPopupManager();
    }
});