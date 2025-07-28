/**
 * 認證管理器
 * 負責處理用戶認證狀態檢查、登出等核心功能
 */
const useAuthManager = () => {
    const api = useApi();

    /**
     * 頁面載入時檢查認證狀態
     */
    const checkAuthOnLoad = async () => {
        try {
            const result = await api.checkAuthStatus();

            if (result.success) {
                const authData = result.data;
                
                if (authData.authenticated) {
                    console.log('User is authenticated:', authData.user);
                    handleAuthenticatedUser(authData.user);
                } else {
                    console.log('User is not authenticated');
                    handleUnauthenticatedUser();
                }
            } else {
                console.log('Auth status check failed:', result.error);
                handleUnauthenticatedUser();
            }
        } catch (error) {
            console.error('Error checking auth status:', error);
            handleUnauthenticatedUser();
        }
    };

    /**
     * 處理已認證用戶
     */
    const handleAuthenticatedUser = (user) => {
        // 更新 UI 狀態
        document.body.classList.remove('unauthenticated');
        document.body.classList.add('authenticated');
        
        // 觸發認證狀態變更事件
        window.dispatchEvent(new CustomEvent('authStatusChanged', {
            detail: { isAuthenticated: true, user: user }
        }));
    };

    /**
     * 處理未認證用戶
     */
    const handleUnauthenticatedUser = () => {
        document.body.classList.remove('authenticated');
        document.body.classList.add('unauthenticated');
        
        // 觸發認證狀態變更事件
        window.dispatchEvent(new CustomEvent('authStatusChanged', {
            detail: { isAuthenticated: false, user: null }
        }));
    };

    /**
     * 登出功能
     */
    const logout = async () => {
        try {
            const result = await api.logout();

            if (result.success) {
                console.log('Logout successful');
                // 重新導向到首頁
                window.location.href = '/';
            } else {
                console.error('Logout failed:', result.error);
            }
        } catch (error) {
            console.error('Error during logout:', error);
        }
    };

    /**
     * 獲取當前用戶資訊
     */
    const getCurrentUser = async () => {
        try {
            const result = await api.checkAuthStatus();
            if (result.success && result.data.authenticated) {
                return result.data.user;
            }
            return null;
        } catch (error) {
            console.error('Error getting current user:', error);
            return null;
        }
    };

    // 初始化時檢查認證狀態
    checkAuthOnLoad();

    return {
        checkAuthOnLoad,
        handleAuthenticatedUser,
        handleUnauthenticatedUser,
        logout,
        getCurrentUser
    };
};

// 將認證管理器掛載到全域 - 同時支援命名空間和向後兼容
window.Matrix.core.useAuthManager = useAuthManager;
window.useAuthManager = useAuthManager; // 向後兼容