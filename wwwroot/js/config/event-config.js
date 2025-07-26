/**
 * 事件配置和管理
 * 集中管理應用程式的事件定義和處理
 */

window.MatrixCore = window.MatrixCore || {};
window.MatrixCore.Config = window.MatrixCore.Config || {};

window.MatrixCore.Config.EventConfig = {
    /**
     * 事件名稱常數
     */
    events: {
        // 認證相關事件
        USER_LOGGED_IN: 'userLoggedIn',
        USER_LOGGED_OUT: 'userLoggedOut',
        AUTH_STATUS_CHANGED: 'authStatusChanged',
        
        // UI 相關事件
        POPUP_OPENED: 'popupOpened',
        POPUP_CLOSED: 'popupClosed',
        SIDEBAR_TOGGLED: 'sidebarToggled',
        LANGUAGE_CHANGED: 'languageChanged',
        
        // 資料相關事件
        DATA_LOADED: 'dataLoaded',
        DATA_ERROR: 'dataError',
        
        // 訪客模式事件
        GUEST_LIMIT_REACHED: 'guestLimitReached'
    },

    /**
     * 事件發射器
     */
    emit(eventName, detail = null) {
        const event = new CustomEvent(eventName, { 
            detail,
            bubbles: true,
            cancelable: true
        });
        
        window.dispatchEvent(event);
        
        // 偵錯日誌
        if (window.MatrixCore.Config.AppConfig.get('debug.enabled')) {
            console.log(`[Event] ${eventName}`, detail);
        }
    },

    /**
     * 事件監聽器
     */
    on(eventName, callback) {
        window.addEventListener(eventName, callback);
    },

    /**
     * 移除事件監聽器
     */
    off(eventName, callback) {
        window.removeEventListener(eventName, callback);
    },

    /**
     * 一次性事件監聽器
     */
    once(eventName, callback) {
        const wrappedCallback = (event) => {
            callback(event);
            this.off(eventName, wrappedCallback);
        };
        this.on(eventName, wrappedCallback);
    },

    /**
     * 初始化事件系統
     */
    initialize() {
        // 設定全域錯誤處理
        window.addEventListener('error', (event) => {
            this.emit(this.events.DATA_ERROR, {
                message: event.message,
                filename: event.filename,
                lineno: event.lineno,
                colno: event.colno,
                error: event.error
            });
        });

        // 設定 Promise 錯誤處理
        window.addEventListener('unhandledrejection', (event) => {
            this.emit(this.events.DATA_ERROR, {
                type: 'unhandledPromiseRejection',
                reason: event.reason
            });
        });
    }
};