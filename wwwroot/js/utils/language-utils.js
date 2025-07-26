/**
 * 語言工具函數
 * 提供多語言切換和本地化功能
 */

window.MatrixCore = window.MatrixCore || {};
window.MatrixCore.Utils = window.MatrixCore.Utils || {};

window.MatrixCore.Utils.LanguageUtils = {
    /**
     * 當前語言設定
     */
    currentLang: localStorage.getItem('matrix-lang') || 'zh-TW',

    /**
     * 切換語言
     */
    toggleLang() {
        this.currentLang = this.currentLang === 'zh-TW' ? 'en-US' : 'zh-TW';
        localStorage.setItem('matrix-lang', this.currentLang);
        
        // 觸發語言變更事件
        window.dispatchEvent(new CustomEvent('languageChanged', {
            detail: { lang: this.currentLang }
        }));
        
        return this.currentLang;
    },

    /**
     * 取得當前語言
     */
    getCurrentLang() {
        return this.currentLang;
    },

    /**
     * 設定語言
     */
    setLang(lang) {
        if (['zh-TW', 'en-US'].includes(lang)) {
            this.currentLang = lang;
            localStorage.setItem('matrix-lang', this.currentLang);
            return true;
        }
        return false;
    },

    /**
     * 取得本地化文字
     */
    getText(key, lang = null) {
        const targetLang = lang || this.currentLang;
        const texts = {
            'zh-TW': {
                'search': '搜尋',
                'notify': '通知',
                'follows': '追蹤',
                'collects': '收藏',
                'login': '登入',
                'register': '註冊',
                'logout': '登出'
            },
            'en-US': {
                'search': 'Search',
                'notify': 'Notifications',
                'follows': 'Following',
                'collects': 'Collections',
                'login': 'Login',
                'register': 'Register',
                'logout': 'Logout'
            }
        };
        
        return texts[targetLang]?.[key] || key;
    }
};