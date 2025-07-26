/**
 * API 服務層
 * 負責所有 HTTP 請求的處理
 */

window.MatrixCore = window.MatrixCore || {};
window.MatrixCore.Services = window.MatrixCore.Services || {};

window.MatrixCore.Services.ApiService = {
    /**
     * 基礎 fetch 包裝器
     */
    async request(url, options = {}) {
        const defaultOptions = {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json',
                'X-Requested-With': 'XMLHttpRequest'
            },
            credentials: 'same-origin'
        };

        const finalOptions = { ...defaultOptions, ...options };

        try {
            const response = await fetch(url, finalOptions);
            
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            // 檢查回應是否為 JSON
            const contentType = response.headers.get('content-type');
            if (contentType && contentType.includes('application/json')) {
                return await response.json();
            } else {
                return await response.text();
            }
        } catch (error) {
            console.error('API Request Error:', error);
            throw error;
        }
    },

    /**
     * GET 請求
     */
    async get(url) {
        return this.request(url, { method: 'GET' });
    },

    /**
     * POST 請求
     */
    async post(url, data) {
        return this.request(url, {
            method: 'POST',
            body: JSON.stringify(data)
        });
    },

    /**
     * PUT 請求
     */
    async put(url, data) {
        return this.request(url, {
            method: 'PUT',
            body: JSON.stringify(data)
        });
    },

    /**
     * DELETE 請求
     */
    async delete(url) {
        return this.request(url, { method: 'DELETE' });
    },

    /**
     * 取得認證狀態
     */
    async getAuthStatus() {
        return this.get('/api/auth/status');
    },

    /**
     * 登出
     */
    async logout() {
        return this.post('/api/auth/logout');
    },

    /**
     * 取得彈窗資料
     */
    async getPopupData(type) {
        return this.get(`/api/${type.toLowerCase()}`);
    }
};