// Language Manager - 處理多語系切換和翻譯功能（從 d_main.js 抽出）

(function() {
    'use strict';

    // 翻譯快取系統
    const translationCache = new Map()

    // 語言管理器
    const LanguageManager = {
        // 可用語言清單
        availableLanguages: [
            { code: 'zh-TW', name: '繁體中文', flag: 'TW' },
            { code: 'en-US', name: 'English', flag: 'US' }
        ],

        // 獲取當前語言
        getCurrentLanguage() {
            return document.documentElement.lang || 'zh-TW'
        },

        // 標準化語言代碼
        normalizeLanguage(lang) {
            if (lang === 'en-TW' || lang === 'zh-tw' || lang === 'zh') {
                return 'zh-TW'
            } else if (lang === 'en' || lang === 'en-tw') {
                return 'en-US'
            }
            return lang
        },

        // 語系切換
        async toggleLanguage() {
            const curLang = this.getCurrentLanguage()
            const normalizedCurLang = this.normalizeLanguage(curLang)
            
            // 切換到另一種語言
            const changeLang = (normalizedCurLang === 'zh-TW' || normalizedCurLang.includes('zh')) 
                ? 'en-US' 
                : 'zh-TW'

            try {
                let translations

                // 檢查快取
                if (translationCache.has(changeLang)) {
                    translations = translationCache.get(changeLang)
                } else {
                    // 從 API 取得翻譯
                    const response = await fetch(`/api/translation/${changeLang}`)
                    if (!response.ok) throw new Error(`HTTP ${response.status}`)
                    translations = await response.json()
                    translationCache.set(changeLang, translations)
                }

                // 更新頁面文字
                this.updatePageText(translations)

                // 設定 cookie
                const cultureCookie = `c=${changeLang}|uic=${changeLang}`
                document.cookie = `.AspNetCore.Culture=${encodeURIComponent(cultureCookie)}; path=/; max-age=31536000; SameSite=Lax`

                // 更新 html lang 屬性
                document.documentElement.lang = changeLang

                // 觸發自定義事件
                window.dispatchEvent(new CustomEvent('language-changed', {
                    detail: { 
                        language: changeLang, 
                        langData: this.availableLanguages.find(l => l.code === changeLang)
                    }
                }))

                // console.log(`語系已切換至: ${changeLang}`)
                return changeLang

            } catch (error) {
                console.error('Error switching language:', error)
                
                // 錯誤處理邏輯
                const cachedTranslations = translationCache.get(changeLang)
                if (cachedTranslations) {
                    this.updatePageText(cachedTranslations)
                    const cultureCookie = `c=${changeLang}|uic=${changeLang}`
                    document.cookie = `.AspNetCore.Culture=${encodeURIComponent(cultureCookie)}; path=/; max-age=31536000; SameSite=Lax`
                    document.documentElement.lang = changeLang
                    return changeLang
                } else {
                    alert('Language switching failed. Please try again.')
                    throw error
                }
            }
        },

        // 更新頁面文字
        updatePageText(translations) {
            if (!translations || typeof translations !== 'object') {
                console.error('Invalid translations object:', translations)
                return
            }

            // 找到所有帶有 data-i18n 屬性的元素
            document.querySelectorAll('[data-i18n]').forEach(element => {
                const key = element.getAttribute('data-i18n')

                if (translations[key]) {
                    if (element.tagName === 'INPUT' && (element.type === 'submit' || element.type === 'button')) {
                        element.value = translations[key]
                    } else if (element.placeholder !== undefined) {
                        element.placeholder = translations[key]
                    } else {
                        element.textContent = translations[key]
                    }
                }
            })

            // 處理 data-i18n-placeholder 屬性的元素
            document.querySelectorAll('[data-i18n-placeholder]').forEach(element => {
                const key = element.getAttribute('data-i18n-placeholder')
                if (translations[key]) element.placeholder = translations[key]
            })

            // 更新頁面標題
            if (translations['Title']) document.title = translations['Title']

            // 呼叫翻譯回調函數
            if (window.profileTranslationCallbacks) {
                window.profileTranslationCallbacks.forEach(callback => {
                    if (typeof callback === 'function') {
                        try {
                            callback(translations)
                        } catch (error) {
                            console.error('Profile translation callback error:', error)
                        }
                    }
                })
            }
        },

        // 獲取語言信息
        getLanguageInfo(langCode) {
            return this.availableLanguages.find(lang => lang.code === langCode)
        },

        // 預載入翻譯
        async preloadTranslation(langCode) {
            if (translationCache.has(langCode)) {
                return translationCache.get(langCode)
            }

            try {
                const response = await fetch(`/api/translation/${langCode}`)
                if (!response.ok) throw new Error(`HTTP ${response.status}`)
                const translations = await response.json()
                translationCache.set(langCode, translations)
                return translations
            } catch (error) {
                console.error(`Failed to preload translation for ${langCode}:`, error)
                return null
            }
        },

        // 清理翻譯快取
        clearCache() {
            translationCache.clear()
        },

        // 獲取快取大小
        getCacheSize() {
            return translationCache.size
        }
    }

    // 暴露到全域
    window.DashboardLanguageManager = LanguageManager
    
    // 向後相容性
    window.updatePageText = LanguageManager.updatePageText.bind(LanguageManager)
    window.translationCache = translationCache

    // 全域動態國際化處理
    const initGlobalDynamicTranslation = async () => {
        try {
            const currentLang = LanguageManager.getCurrentLanguage()
            
            // 獲取或載入當前語言翻譯
            let translations = translationCache.get(currentLang)
            if (!translations) {
                translations = await LanguageManager.preloadTranslation(currentLang)
            }
            
            // 處理所有 data-i18n 元素（包括動態生成的）
            if (translations) {
                LanguageManager.updatePageText(translations)
                // console.log(`✅ 全域動態國際化已應用: ${currentLang}`)
            }
        } catch (error) {
            console.error('全域動態國際化失敗:', error)
        }
    }

    // 確保頁面完全載入後再處理國際化
    const handlePageLoad = () => {
        // 使用 setTimeout 確保所有同步和異步資源都完成載入
        setTimeout(async () => {
            await initGlobalDynamicTranslation()
        }, 100)
    }

    // 監聽頁面載入完成事件
    if (document.readyState === 'loading') {
        // 如果文檔還在載入中，監聽 DOMContentLoaded
        document.addEventListener('DOMContentLoaded', () => {
            // DOMContentLoaded 後再等待所有資源載入完成
            if (document.readyState === 'complete') {
                handlePageLoad()
            } else {
                window.addEventListener('load', handlePageLoad, { once: true })
            }
        }, { once: true })
    } else if (document.readyState === 'interactive') {
        // 如果 DOM 已載入但資源可能還在載入
        if (document.readyState === 'complete') {
            handlePageLoad()
        } else {
            window.addEventListener('load', handlePageLoad, { once: true })
        }
    } else {
        // 如果頁面已完全載入
        handlePageLoad()
    }

    // 監聽動態內容變化（用於處理後續動態載入的內容）
    const observeDynamicContent = () => {
        const observer = new MutationObserver((mutations) => {
            let hasNewI18nElements = false
            
            mutations.forEach((mutation) => {
                if (mutation.type === 'childList' && mutation.addedNodes.length > 0) {
                    mutation.addedNodes.forEach((node) => {
                        if (node.nodeType === 1) { // Element node
                            if (node.hasAttribute && node.hasAttribute('data-i18n')) {
                                hasNewI18nElements = true
                            } else if (node.querySelector && node.querySelector('[data-i18n]')) {
                                hasNewI18nElements = true
                            }
                        }
                    })
                }
            })
            
            // 如果發現新的 data-i18n 元素，延遲處理翻譯
            if (hasNewI18nElements) {
                clearTimeout(window._dynamicTranslationTimeout)
                window._dynamicTranslationTimeout = setTimeout(async () => {
                    await initGlobalDynamicTranslation()
                    lucide.createIcons()
                }, 200)
            }
        })
        
        observer.observe(document.body, {
            childList: true,
            subtree: true
        })
    }

    // 當頁面完全載入後啟動動態內容監聽
    window.addEventListener('load', () => {
        setTimeout(observeDynamicContent, 500)
    }, { once: true })

})();
