// 統一認證服務 - 單例模式 (ESM)
// 負責管理用戶認證狀態，避免重複 API 呼叫
export class AuthService {
    constructor() {
        this.authData = null;
        this.authPromise = null;
        this.cacheTime = null;
        this.CACHE_DURATION = 5 * 60 * 1000; // 5分鐘緩存
        this.subscribers = new Set(); // 狀態變更訂閱者
    }

    // 取得認證狀態（有快取）
    async getAuthStatus() {
        // 如果有有效緩存，直接返回
        if (this.authData && this.cacheTime &&
            Date.now() - this.cacheTime < this.CACHE_DURATION) {
            return this.authData;
        }

        // 如果正在請求中，返回相同的Promise避免重複調用
        if (this.authPromise) {
            return this.authPromise;
        }

        // 發起新的認證請求
        this.authPromise = this._fetchAuthStatus();

        try {
            const result = await this.authPromise;
            return result;
        } finally {
            // 清除Promise引用，允許下次請求
            this.authPromise = null;
        }
    }

    // 私有方法：真的去打 API
    async _fetchAuthStatus() {
        try {
            const response = await fetch('/api/auth/status', {
                method: 'GET',
                credentials: 'include',
                headers: {
                    'Content-Type': 'application/json'
                }
            });

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const data = await response.json();

            // 更新緩存
            this.authData = data;
            this.cacheTime = Date.now();

            // 通知訂閱者
            this._notifySubscribers(data);

            return data;

        } catch (error) {
            console.error('認證狀態檢查失敗:', error);

            // 清除無效緩存
            this.authData = null;
            this.cacheTime = null;

            // 返回未認證狀態
            const fallbackData = {
                success: false,
                data: { authenticated: false, user: null }
            };

            this._notifySubscribers(fallbackData);
            return fallbackData;
        }
    }

    // 取得當前用戶 ID（如果已認證）
    async getCurrentUserId() {
        const authStatus = await this.getAuthStatus();

        if (authStatus.success &&
            authStatus.data.authenticated &&
            authStatus.data.user?.id) {
            return authStatus.data.user.id;
        }

        return null;
    }

    // 取得當前用戶資訊
    async getCurrentUser() {
        const authStatus = await this.getAuthStatus();

        if (authStatus.success && authStatus.data.authenticated) {
            return authStatus.data.user;
        }

        return null;
    }

    // 檢查是否已認證
    async isAuthenticated() {
        const authStatus = await this.getAuthStatus();
        return authStatus.success && authStatus.data.authenticated;
    }

    // 強制刷新認證狀態（清空快取）
    async refreshAuthStatus() {
        this.authData = null;
        this.cacheTime = null;
        this.authPromise = null;

        return await this.getAuthStatus();
    }

    // 登出時清除認證狀態
    clearAuthStatus() {
        this.authData = null;
        this.cacheTime = null;
        this.authPromise = null;

        const logoutData = {
            success: false,
            data: { authenticated: false, user: null }
        };

        this._notifySubscribers(logoutData);
    }

    // 訂閱認證狀態變更
    subscribe(callback) {
        this.subscribers.add(callback);

        // 如果有現有數據，立即通知
        if (this.authData) {
            callback(this.authData);
        }

        // 返回取消訂閱函數
        return () => {
            this.subscribers.delete(callback);
        };
    }

    // 通知所有訂閱者
    _notifySubscribers(data) {
        this.subscribers.forEach(callback => {
            try {
                callback(data);
            } catch (error) {
                console.error('認證狀態訂閱者回調錯誤:', error);
            }
        });
    }

    // 取得快取狀態（除錯用）
    getCacheInfo() {
        return {
            hasCache: !!this.authData,
            cacheTime: this.cacheTime,
            isExpired: this.cacheTime ?
                Date.now() - this.cacheTime > this.CACHE_DURATION : true,
            subscriberCount: this.subscribers.size
        };
    }
}

// 導出單例
export const authService = new AuthService();
export default authService;
