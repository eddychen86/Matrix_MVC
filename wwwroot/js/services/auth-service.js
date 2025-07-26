/**
 * 認證服務層
 * 負責用戶認證相關的業務邏輯
 */

window.MatrixCore = window.MatrixCore || {};
window.MatrixCore.Services = window.MatrixCore.Services || {};

window.MatrixCore.Services.AuthService = {
    /**
     * 當前用戶狀態
     */
    currentUser: null,
    isAuthenticated: false,

    /**
     * 檢查認證狀態
     */
    async checkAuthStatus() {
        try {
            const response = await window.MatrixCore.Services.ApiService.getAuthStatus();
            
            if (response && response.isAuthenticated) {
                this.currentUser = response.user;
                this.isAuthenticated = true;
                return {
                    isAuthenticated: true,
                    user: response.user
                };
            } else {
                this.currentUser = null;
                this.isAuthenticated = false;
                return {
                    isAuthenticated: false,
                    user: null
                };
            }
        } catch (error) {
            console.error('Auth status check failed:', error);
            this.currentUser = null;
            this.isAuthenticated = false;
            return {
                isAuthenticated: false,
                user: null,
                error: error.message
            };
        }
    },

    /**
     * 登出
     */
    async logout() {
        try {
            await window.MatrixCore.Services.ApiService.logout();
            this.currentUser = null;
            this.isAuthenticated = false;
            
            // 觸發登出事件
            window.dispatchEvent(new CustomEvent('userLoggedOut'));
            
            // 重新導向到首頁
            window.location.href = '/';
            
            return { success: true };
        } catch (error) {
            console.error('Logout failed:', error);
            return { success: false, error: error.message };
        }
    },

    /**
     * 取得當前用戶
     */
    getCurrentUser() {
        return this.currentUser;
    },

    /**
     * 檢查是否已認證
     */
    isUserAuthenticated() {
        return this.isAuthenticated;
    },

    /**
     * 檢查用戶權限
     */
    hasPermission(permission) {
        if (!this.isAuthenticated || !this.currentUser) {
            return false;
        }
        
        // 基本權限檢查邏輯
        if (this.currentUser.role === 'Admin') {
            return true; // 管理員擁有所有權限
        }
        
        // 其他權限檢查邏輯可以在這裡擴展
        return true;
    },

    /**
     * 設定認證狀態監聽器
     */
    onAuthStateChanged(callback) {
        window.addEventListener('userLoggedIn', callback);
        window.addEventListener('userLoggedOut', callback);
    }
};