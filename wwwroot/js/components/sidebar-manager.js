/**
 * 側邊欄管理組件
 * 負責側邊欄的展開收合和相關 UI 控制
 */

window.MatrixCore = window.MatrixCore || {};

window.MatrixCore.SidebarManager = {
    /**
     * 建立側邊欄相關方法
     */
    createMethods(state) {
        const { isCollapsed } = state;

        return {
            /**
             * 切換側邊欄狀態
             */
            toggleSidebar() {
                isCollapsed.value = !isCollapsed.value;
                
                // 觸發側邊欄切換事件
                window.dispatchEvent(new CustomEvent('sidebarToggled', {
                    detail: { isCollapsed: isCollapsed.value }
                }));
                
                // 延遲重新初始化圖標，確保 DOM 更新完成
                setTimeout(() => {
                    if (typeof lucide !== 'undefined') {
                        lucide.createIcons();
                    }
                }, 100);
            },

            /**
             * 展開側邊欄
             */
            expandSidebar() {
                if (isCollapsed.value) {
                    this.toggleSidebar();
                }
            },

            /**
             * 收合側邊欄
             */
            collapseSidebar() {
                if (!isCollapsed.value) {
                    this.toggleSidebar();
                }
            },

            /**
             * 取得側邊欄狀態
             */
            getSidebarState() {
                return {
                    isCollapsed: isCollapsed.value
                };
            }
        };
    }
};