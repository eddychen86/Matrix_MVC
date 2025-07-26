/**
 * 通用工具函數
 * 提供常用的輔助功能
 */

window.MatrixCore = window.MatrixCore || {};
window.MatrixCore.Utils = window.MatrixCore.Utils || {};

window.MatrixCore.Utils.CommonUtils = {
    /**
     * 防抖函數
     */
    debounce(func, wait) {
        let timeout;
        return function executedFunction(...args) {
            const later = () => {
                clearTimeout(timeout);
                func(...args);
            };
            clearTimeout(timeout);
            timeout = setTimeout(later, wait);
        };
    },

    /**
     * 節流函數
     */
    throttle(func, limit) {
        let inThrottle;
        return function executedFunction(...args) {
            if (!inThrottle) {
                func.apply(this, args);
                inThrottle = true;
                setTimeout(() => inThrottle = false, limit);
            }
        };
    },

    /**
     * 檢查是否為空值
     */
    isEmpty(value) {
        return value == null || value === '' || 
               (Array.isArray(value) && value.length === 0) ||
               (typeof value === 'object' && Object.keys(value).length === 0);
    },

    /**
     * 安全的 JSON 解析
     */
    safeJsonParse(jsonString, defaultValue = null) {
        try {
            return JSON.parse(jsonString);
        } catch (e) {
            console.warn('JSON parse error:', e);
            return defaultValue;
        }
    },

    /**
     * 產生隨機 ID
     */
    generateId(prefix = 'id') {
        return `${prefix}_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
    },

    /**
     * 等待指定時間
     */
    sleep(ms) {
        return new Promise(resolve => setTimeout(resolve, ms));
    },

    /**
     * 複製文字到剪貼板
     */
    async copyToClipboard(text) {
        try {
            await navigator.clipboard.writeText(text);
            return true;
        } catch (err) {
            console.error('Copy to clipboard failed:', err);
            return false;
        }
    }
};