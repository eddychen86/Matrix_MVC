/**
 * 語言管理器
 * 負責處理多語言切換功能
 */
const useLanguageManager = () => {
    /**
     * 獲取當前語言
     */
    const getCurrentLanguage = () => {
        return document.documentElement.lang || 'zh-TW';
    };

    /**
     * 設定語言 Cookie
     * @param {string} lang - 語言代碼
     */
    const setLanguageCookie = (lang) => {
        // ASP.NET Core 預設的 culture cookie 名稱是 ".AspNetCore.Culture"
        const cultureCookie = `c=${lang}|uic=${lang}`;
        document.cookie = `.AspNetCore.Culture=${cultureCookie}; path=/; max-age=31536000; SameSite=Lax`;
        console.log(`Setting culture cookie: ${cultureCookie}`);
    };

    /**
     * 切換語言
     */
    const toggleLanguage = () => {
        const currentLang = getCurrentLanguage();
        
        // 切換語言
        const newLang = currentLang === 'zh-TW' ? 'en-US' : 'zh-TW';
        
        // 設定語言 Cookie
        setLanguageCookie(newLang);
        
        // 重新載入頁面以應用語言變更
        window.location.reload();
    };

    /**
     * 設定特定語言
     * @param {string} lang - 語言代碼
     */
    const setLanguage = (lang) => {
        if (lang !== getCurrentLanguage()) {
            setLanguageCookie(lang);
            window.location.reload();
        }
    };

    /**
     * 獲取可用語言列表
     */
    const getAvailableLanguages = () => {
        return [
            { code: 'zh-TW', name: '繁體中文', nativeName: '繁體中文' },
            { code: 'en-US', name: 'English', nativeName: 'English' }
        ];
    };

    /**
     * 獲取語言顯示名稱
     * @param {string} langCode - 語言代碼
     */
    const getLanguageName = (langCode) => {
        const languages = getAvailableLanguages();
        const language = languages.find(lang => lang.code === langCode);
        return language ? language.name : langCode;
    };

    return {
        getCurrentLanguage,
        setLanguageCookie,
        toggleLanguage,
        setLanguage,
        getAvailableLanguages,
        getLanguageName
    };
};

// 將語言管理器掛載到全域 - 同時支援命名空間和向後兼容
window.Matrix.core.useLanguageManager = useLanguageManager;
window.useLanguageManager = useLanguageManager; // 向後兼容