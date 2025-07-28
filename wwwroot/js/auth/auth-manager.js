/**
  * 頁面載入時檢查認證狀態
  */
const checkAuthOnLoad = async () => {
    try {
        const response = await fetch('/api/auth/status', {
            method: 'GET',
            credentials: 'include',
            headers: {
                'Content-Type': 'application/json'
            }
        });

        if (response.ok) {
            const authData = await response.json();
            
            if (authData.isAuthenticated) {
                console.log('User is authenticated:', authData.user);
                handleAuthenticatedUser(authData.user);
            } else {
                console.log('User is not authenticated');
                handleUnauthenticatedUser();
            }
        } else {
            console.log('Auth status check failed');
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
    // 可以在這裡更新 UI，顯示用戶資訊等
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
        const response = await fetch('/api/auth/logout', {
            method: 'POST',
            credentials: 'include',
            headers: {
                'Content-Type': 'application/json'
            }
        });

        if (response.ok) {
            console.log('Logout successful');
            // 重新導向到首頁
            window.location.href = '/';
        } else {
            console.error('Logout failed');
        }
    } catch (error) {
        console.error('Error during logout:', error);
    }
};

// 初始化
checkAuthOnLoad();

return {
    checkAuthOnLoad,
    handleAuthenticatedUser,
    handleUnauthenticatedUser,
    logout
};