/**
 * API 工具函數
 * 提供統一的 API 請求方法和錯誤處理
 */
const useApi = () => {
    /**
     * 通用的 API 請求方法
     * @param {string} url - 請求的 URL
     * @param {Object} options - 請求選項
     * @returns {Promise<Object>} - 請求結果
     */
    const request = async (url, options = {}) => {
        const defaultOptions = {
            credentials: 'include',
            headers: {
                'Content-Type': 'application/json',
                ...options.headers
            }
        };

        const config = { ...defaultOptions, ...options };

        try {
            const response = await fetch(url, config);
            
            if (!response.ok) {
                throw new Error(`HTTP ${response.status}: ${response.statusText}`);
            }

            const data = await response.json();
            return { success: true, data };
        } catch (error) {
            console.error(`API request failed for ${url}:`, error);
            return { success: false, error: error.message };
        }
    };

    /**
     * GET 請求
     */
    const get = (url, options = {}) => {
        return request(url, { method: 'GET', ...options });
    };

    /**
     * POST 請求
     */
    const post = (url, data, options = {}) => {
        return request(url, {
            method: 'POST',
            body: JSON.stringify(data),
            ...options
        });
    };

    /**
     * 帶有防偽 token 的 POST 請求
     */
    const postWithToken = (url, data, token, options = {}) => {
        return request(url, {
            method: 'POST',
            headers: {
                'RequestVerificationToken': token,
                ...options.headers
            },
            body: JSON.stringify(data),
            ...options
        });
    };

    /**
     * 檢查認證狀態
     */
    const checkAuthStatus = () => {
        return get('/api/auth/status');
    };

    /**
     * 登出
     */
    const logout = () => {
        return request('/api/auth/logout', { method: 'POST' });
    };

    /**
     * 登入
     */
    const login = (loginData, token) => {
        return postWithToken('/api/login', loginData, token);
    };

    /**
     * 註冊
     */
    const register = (registerData, token) => {
        return postWithToken('/api/register', registerData, token);
    };

    return {
        request,
        get,
        post,
        postWithToken,
        checkAuthStatus,
        logout,
        login,
        register
    };
};

// 將 API 工具掛載到全域 - 同時支援命名空間和向後兼容
window.Matrix.utils.useApi = useApi;
window.useApi = useApi; // 向後兼容