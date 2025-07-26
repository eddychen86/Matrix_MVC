/**
 * 應用程式狀態管理核心
 * 負責 Vue 應用程式的狀態定義和基本操作
 */

window.MatrixCore = window.MatrixCore || {};

window.MatrixCore.AppState = {
    /**
     * 建立應用程式狀態
     */
    createAppState() {
        const { ref, reactive, computed } = Vue;

        return {
            // 彈窗狀態
            popupState: reactive({
                isVisible: false,
                type: '',
                title: ''
            }),

            // UI 狀態
            isCollapsed: ref(false),
            searchQuery: ref(''),

            // 資料儲存
            popupData: reactive({
                Search: [],
                Notify: [],
                Follows: [],
                Collects: []
            }),

            // 計算屬性
            isOpen: computed(() => this.popupState?.isVisible || false)
        };
    },

    /**
     * 更新彈窗資料
     */
    updatePopupData(popupData, type, data) {
        if (popupData[type] !== undefined) {
            popupData[type] = data;
        }
    }
};