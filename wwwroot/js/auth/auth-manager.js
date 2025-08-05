/**
 * Authentication Manager
 * 處理用戶認證狀態檢查和自動登入
 */

class AuthManager {
    constructor() {
        this.checkAuthOnLoad();
    }

    /**
     * 頁面載入時檢查認證狀態
     */
    async checkAuthOnLoad() {
        try {
            if (window.authService) {
                const authStatus = await window.authService.getAuthStatus();
                
                if (authStatus.success && authStatus.data.authenticated) {
                    console.log('User is authenticated:', authStatus.data.user);
                    this.handleAuthenticatedUser(authStatus.data.user);
                } else {
                    // console.log('User is not authenticated');
                    this.handleUnauthenticatedUser();
                }
            } else {
                console.warn('AuthService not available, skipping auth check');
                this.handleUnauthenticatedUser();
            }
        } catch (error) {
            console.error('Error checking auth status:', error);
            this.handleUnauthenticatedUser();
        }
    }

    /**
     * 處理已認證用戶
     */
    handleAuthenticatedUser(user) {
        // 可以在這裡更新 UI，顯示用戶資訊等
        document.body.classList.add('authenticated');
        
        // 觸發認證狀態變更事件
        window.dispatchEvent(new CustomEvent('authStatusChanged', {
            detail: { isAuthenticated: true, user: user }
        }));
    }

    /**
     * 處理未認證用戶
     */
    handleUnauthenticatedUser() {
        document.body.classList.add('unauthenticated');
        
        // 觸發認證狀態變更事件
        window.dispatchEvent(new CustomEvent('authStatusChanged', {
            detail: { isAuthenticated: false, user: null }
        }));
    }

    /**
     * 登出功能
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
                console.log('Logout successful');
                
                // 清除 AuthService 緩存
                if (window.authService) {
                    window.authService.clearAuthStatus();
                }
                
                // 重新導向到首頁
                window.location.href = '/';
            } else {
                console.error('Logout failed');
            }
        } catch (error) {
            console.error('Error during logout:', error);
        }
    }
}

// 全域認證管理器實例
window.authManager = new AuthManager();

// 全域登出函數
window.logout = () => {
    window.authManager.logout();
};