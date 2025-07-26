/**
 * 登入提示 Popup 組件
 * 當訪客瀏覽到限制內容時顯示登入提示
 */
class LoginPopup {
    constructor() {
        this.isShown = false;
        this.popup = null;
        this.createPopup();
        this.bindEvents();
    }

    /**
     * 建立 Popup DOM 結構
     */
    createPopup() {
        const popupHTML = `
            <div id="login-popup-overlay" class="login-popup-overlay hide">
                <div class="login-popup-container">
                    <div class="login-popup-header">
                        <h2>
                            <i data-lucide="lighthouse" class="w-6 h-6 text-orange"></i>
                            需要登入才能繼續瀏覽
                        </h2>
                        <button class="close-btn" id="login-popup-close">
                            <i data-lucide="x" class="w-5 h-5"></i>
                        </button>
                    </div>
                    <div class="login-popup-content">
                        <p class="text-white font-medium">您已經瀏覽了所有可用的公開內容。</p>
                        <p>登入後即可享受完整的 Matrix 體驗：</p>
                        <ul class="feature-list">
                            <li>瀏覽所有精彩文章</li>
                            <li>參與深度討論和留言</li>
                            <li>關注其他創作者</li>
                            <li>發表您的獨特見解</li>
                            <li>收藏喜愛的內容</li>
                        </ul>
                    </div>
                    <div class="login-popup-actions">
                        <button class="btn-secondary" id="login-popup-cancel">
                            稍後再說
                        </button>
                        <button class="btn-primary" id="login-popup-login">
                            <i data-lucide="log-in" class="w-4 h-4 mr-2"></i>
                            立即登入
                        </button>
                    </div>
                </div>
            </div>
        `;

        // 將 Popup 加入到 body
        document.body.insertAdjacentHTML('beforeend', popupHTML);
        this.popup = document.getElementById('login-popup-overlay');
    }

    /**
     * 綁定事件監聽器
     */
    bindEvents() {
        const closeBtn = document.getElementById('login-popup-close');
        const cancelBtn = document.getElementById('login-popup-cancel');
        const loginBtn = document.getElementById('login-popup-login');

        // 關閉按鈕
        closeBtn?.addEventListener('click', () => this.hide());
        cancelBtn?.addEventListener('click', () => this.hide());

        // 登入按鈕
        loginBtn?.addEventListener('click', () => {
            window.location.href = '/login';
        });

        // 點擊背景關閉
        this.popup?.addEventListener('click', (e) => {
            if (e.target === this.popup) {
                this.hide();
            }
        });

        // ESC 鍵關閉
        document.addEventListener('keydown', (e) => {
            if (e.key === 'Escape' && this.isShown) {
                this.hide();
            }
        });
    }

    /**
     * 顯示 Popup
     */
    show() {
        if (this.isShown || !this.popup) return;

        this.isShown = true;
        this.popup.classList.remove('hide');
        this.popup.classList.add('show');
        
        // 防止背景滾動
        document.body.style.overflow = 'hidden';
        
        // 重新建立圖示
        setTimeout(() => {
            if (typeof lucide !== 'undefined') {
                lucide.createIcons();
            }
        }, 100);

        console.log('登入提示 Popup 已顯示');
    }

    /**
     * 隱藏 Popup
     */
    hide() {
        if (!this.isShown || !this.popup) return;

        this.isShown = false;
        this.popup.classList.remove('show');
        this.popup.classList.add('hide');
        
        // 恢復背景滾動
        document.body.style.overflow = '';

        console.log('登入提示 Popup 已隱藏');
    }

    /**
     * 檢查是否已顯示
     */
    isVisible() {
        return this.isShown;
    }
}

/**
 * 訪客權限控制管理器
 * 監控訪客的瀏覽行為並在適當時候顯示登入提示
 */
class GuestAccessControl {
    constructor() {
        this.loginPopup = new LoginPopup();
        this.articleLimit = 10; // 預設限制
        this.hasShownPopup = false;
        this.isAuthenticated = false;
        this.initializeFromViewBag();
        this.setupScrollMonitoring();
    }

    /**
     * 從 ViewBag 讀取認證狀態和限制資訊
     */
    initializeFromViewBag() {
        // 這些值將在 HTML 中由 Razor 設定
        this.isAuthenticated = window.matrixAuthData?.isAuthenticated || false;
        this.articleLimit = window.matrixAuthData?.articleLimit || 10;
        
        console.log('訪客控制初始化:', {
            isAuthenticated: this.isAuthenticated,
            articleLimit: this.articleLimit
        });
    }

    /**
     * 設定滾動監控
     */
    setupScrollMonitoring() {
        if (this.isAuthenticated) {
            console.log('已認證用戶，跳過訪客限制');
            return;
        }

        // 監控滾動到文章列表底部
        this.observeArticleList();
    }

    /**
     * 使用 Intersection Observer 監控文章列表
     */
    observeArticleList() {
        // 尋找文章容器（根據您的 HTML 結構調整選擇器）
        const articles = document.querySelectorAll('[data-article-index]');
        
        if (articles.length === 0) {
            console.warn('未找到文章元素，請檢查 HTML 結構');
            return;
        }

        // 監控第10篇文章（索引為9）
        const tenthArticle = document.querySelector('[data-article-index="9"]');
        if (!tenthArticle) {
            console.log('文章數量少於10篇，無需顯示限制提示');
            return;
        }

        const observer = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting && !this.hasShownPopup) {
                    this.showLoginPrompt();
                }
            });
        }, {
            threshold: 0.5 // 當50%的元素進入視窗時觸發
        });

        observer.observe(tenthArticle);
        console.log('已開始監控第10篇文章的滾動狀態');
    }

    /**
     * 顯示登入提示
     */
    showLoginPrompt() {
        if (this.hasShownPopup || this.isAuthenticated) return;

        this.hasShownPopup = true;
        this.loginPopup.show();
        
        console.log('訪客已瀏覽到限制內容，顯示登入提示');
    }

    /**
     * 強制顯示登入提示（供測試使用）
     */
    forceShowPopup() {
        this.loginPopup.show();
    }
}

// 建立全域實例
window.guestAccessControl = null;

// DOM 載入完成後初始化
document.addEventListener('DOMContentLoaded', function() {
    // 等待認證管理器完成初始化
    if (window.authManager) {
        window.authManager.onAuthChange(function(authStatus) {
            // 只有在訪客狀態下才需要訪客控制
            if (!authStatus.authenticated && authStatus.guest) {
                if (!window.guestAccessControl) {
                    window.guestAccessControl = new GuestAccessControl();
                }
            }
        });
    } else {
        // 如果認證管理器尚未載入，延遲初始化
        setTimeout(() => {
            if (!window.guestAccessControl) {
                window.guestAccessControl = new GuestAccessControl();
            }
        }, 1000);
    }
});