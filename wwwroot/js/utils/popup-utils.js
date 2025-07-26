/**
 * 彈窗工具函數
 * 提供彈窗相關的輔助功能
 */

window.MatrixCore = window.MatrixCore || {};
window.MatrixCore.Utils = window.MatrixCore.Utils || {};

window.MatrixCore.Utils.PopupUtils = {
    /**
     * 取得彈窗標題
     */
    getPopupTitle(type) {
        const titles = {
            'Search': '搜尋',
            'Notify': '通知',
            'Follows': '追蹤',
            'Collects': '收藏'
        };
        return titles[type] || '未知';
    },

    /**
     * 檢查彈窗是否可以開啟
     */
    canOpenPopup(type) {
        const allowedTypes = ['Search', 'Notify', 'Follows', 'Collects'];
        return allowedTypes.includes(type);
    },

    /**
     * 產生彈窗 API 路徑
     */
    getApiPath(type) {
        return '/api/' + type.toLowerCase();
    }
};

// 向後兼容性：在全域命名空間設定簡化方法
window.MatrixCore.Utils.getPopupTitle = window.MatrixCore.Utils.PopupUtils.getPopupTitle;