/**
 * 彈窗管理組件
 * 負責各種彈窗的開啟、關閉和資料管理
 */

window.MatrixCore = window.MatrixCore || {};

window.MatrixCore.PopupManager = {
    /**
     * 建立彈窗相關方法
     */
    createMethods(state) {
        const { popupState, popupData } = state;

        return {
            /**
             * 開啟彈窗
             */
            async openPopup(type) {
                // 檢查是否可以開啟此類型的彈窗
                if (!window.MatrixCore.Utils.PopupUtils.canOpenPopup(type)) {
                    console.warn('Invalid popup type:', type);
                    return;
                }

                popupState.type = type;
                popupState.title = window.MatrixCore.Utils.PopupUtils.getPopupTitle(type);
                popupState.isVisible = true;

                // 觸發彈窗開啟事件
                window.MatrixCore.Config.EventConfig.emit(
                    window.MatrixCore.Config.EventConfig.events.POPUP_OPENED,
                    { type, title: popupState.title }
                );

                try {
                    const data = await window.MatrixCore.Services.ApiService.getPopupData(type);
                    window.MatrixCore.AppState.updatePopupData(popupData, type, data);
                    
                    // 觸發資料載入事件
                    window.MatrixCore.Config.EventConfig.emit(
                        window.MatrixCore.Config.EventConfig.events.DATA_LOADED,
                        { type, data }
                    );
                } catch (err) {
                    console.error('Popup data fetch error:', err);
                    
                    // 觸發錯誤事件
                    window.MatrixCore.Config.EventConfig.emit(
                        window.MatrixCore.Config.EventConfig.events.DATA_ERROR,
                        { type: 'popup_fetch_error', error: err.message, popupType: type }
                    );
                }
            },

            /**
             * 關閉彈窗
             */
            closePopup() {
                const currentType = popupState.type;
                
                popupState.isVisible = false;
                popupState.type = '';
                
                // 觸發彈窗關閉事件
                window.MatrixCore.Config.EventConfig.emit(
                    window.MatrixCore.Config.EventConfig.events.POPUP_CLOSED,
                    { type: currentType }
                );
            },

            /**
             * 切換彈窗狀態
             */
            togglePopup(type) {
                if (popupState.isVisible && popupState.type === type) {
                    this.closePopup();
                } else {
                    this.openPopup(type);
                }
            }
        };
    }
};