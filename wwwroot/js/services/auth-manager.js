/**
 * Matrix 認證管理器
 * 負責處理用戶認證狀態的自動檢查和管理
 */
class AuthManager {
    constructor() {
        this.authStatus = {
            authenticated: false,
            user: null,
            guest: false,
            loading: true
        };
        this.callbacks = [];
        this.initializeAuth();
    }

    /**
     * 初始化認證狀態
     * 在頁面載入時自動執行認證檢查
     */
    async initializeAuth() {
        try {
            const response = await fetch('/api/auth/status', {
                method: 'GET',
                credentials: 'include', // 確保包含 Cookie
                headers: {
                    'Content-Type': 'application/json'
                }
            });

            if (response.ok) {
                const data = await response.json();
                this.updateAuthStatus(data);
            } else {
                console.warn('認證狀態檢查失敗:', response.statusText);
                this.setGuestStatus();
            }
        } catch (error) {
            console.error('認證狀態檢查錯誤:', error);
            this.setGuestStatus();
        }
    }

    /**
     * 更新認證狀態
     * @param {Object} data - 從 API 回傳的認證資料
     */
    updateAuthStatus(data) {
        this.authStatus = {
            authenticated: data.authenticated,
            user: data.user || null,
            guest: data.guest || false,
            loading: false
        };
        
        // 觸發狀態變更回呼函數
        this.triggerCallbacks();
        
        // 記錄認證狀態
        if (data.authenticated) {
            console.log('用戶已認證:', data.user.username);
        } else {
            console.log('用戶未認證（訪客狀態）');
        }
    }

    /**
     * 設定為訪客狀態
     */
    setGuestStatus() {
        this.authStatus = {
            authenticated: false,
            user: null,
            guest: true,
            loading: false
        };
        this.triggerCallbacks();
    }

    /**
     * 用戶登出
     */
    async logout() {
        try {
            const response = await fetch('/api/auth/logout', {
                method: 'POST',
                credentials: 'include',
                headers: {
                    'Content-Type': 'application/json'
                }
            });

            if (response.ok) {
                this.setGuestStatus();
                console.log('用戶已登出');
                
                // 可選：重新導向到首頁或登入頁面
                window.location.href = '/';
            } else {
                console.error('登出失敗:', response.statusText);
            }
        } catch (error) {
            console.error('登出錯誤:', error);
        }
    }

    /**
     * 檢查用戶是否已認證
     * @returns {boolean}
     */
    isAuthenticated() {
        return this.authStatus.authenticated;
    }

    /**
     * 檢查用戶是否為訪客
     * @returns {boolean}
     */
    isGuest() {
        return this.authStatus.guest;
    }

    /**
     * 取得用戶資訊
     * @returns {Object|null}
     */
    getUser() {
        return this.authStatus.user;
    }

    /**
     * 檢查認證狀態是否正在載入中
     * @returns {boolean}
     */
    isLoading() {
        return this.authStatus.loading;
    }

    /**
     * 註冊認證狀態變更的回呼函數
     * @param {Function} callback - 當認證狀態變更時要執行的函數
     */
    onAuthChange(callback) {
        if (typeof callback === 'function') {
            this.callbacks.push(callback);
            
            // 如果認證狀態已載入完成，立即執行回呼
            if (!this.authStatus.loading) {
                callback(this.authStatus);
            }
        }
    }

    /**
     * 觸發所有註冊的回呼函數
     */
    triggerCallbacks() {
        this.callbacks.forEach(callback => {
            try {
                callback(this.authStatus);
            } catch (error) {
                console.error('認證狀態回呼函數執行錯誤:', error);
            }
        });
    }

    /**
     * 強制重新檢查認證狀態
     * 用於登入/登出後的状态同步
     */
    async refreshAuthStatus() {
        this.authStatus.loading = true;
        await this.initializeAuth();
    }
}

// 建立全域認證管理器實例
window.authManager = new AuthManager();

// DOM 載入完成後的初始化
document.addEventListener('DOMContentLoaded', function() {
    // 認證狀態變更時的處理
    window.authManager.onAuthChange(function(authStatus) {
        // 更新導覽列的登入/登出按鈕顯示
        updateNavigationUI(authStatus);
        
        // 可以在這裡加入其他需要根據認證狀態調整的 UI 元素
    });
});

/**
 * 更新導覽列 UI
 * @param {Object} authStatus - 認證狀態物件
 */
function updateNavigationUI(authStatus) {
    // 這個函數將在下一步實作，用於更新導覽列的顯示
    console.log('導覽列 UI 更新:', authStatus);
}