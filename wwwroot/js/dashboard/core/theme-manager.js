/**
 * Theme Manager - 處理主題切換功能
 * 從 d_main.js 中抽離出來的主題管理功能
 */

(function() {
    'use strict';

    // 主題管理器
    const ThemeManager = {
        // 主題配置
        themes: {
            light: {
                isDark: false,
                primaryColor: '#3b82f6',
                backgroundColor: '#ffffff',
                textColor: '#1f2937'
            },
            dark: {
                isDark: true,
                primaryColor: '#60a5fa',
                backgroundColor: '#111827',
                textColor: '#f9fafb'
            }
        },

        // 獲取儲存的主題
        getSavedTheme() {
            return localStorage.getItem('dashboard-theme') || 'light'
        },

        // 設定主題
        setTheme(isDarkOrThemeName) {
            let themeName
            let isDark

            // 支持傳入 boolean 或 string
            if (typeof isDarkOrThemeName === 'boolean') {
                isDark = isDarkOrThemeName
                themeName = isDark ? 'dark' : 'light'
            } else {
                themeName = isDarkOrThemeName || 'light'
                isDark = themeName === 'dark'
            }

            // 更新 localStorage
            localStorage.setItem('dashboard-theme', themeName)

            // 獲取主題配置
            const themeConfig = this.themes[themeName] || this.themes.light

            // 更新全域狀態（如果存在）
            if (window.DashboardStateManager) {
                window.DashboardStateManager.updateTheme(themeConfig)
            }

            // 更新 CSS 變數
            this.updateCSSVariables(themeConfig)

            // 更新 body class
            this.updateBodyClass(isDark)

            // 觸發主題變更事件
            window.dispatchEvent(new CustomEvent('theme-changed', {
                detail: { 
                    themeName, 
                    isDark, 
                    config: themeConfig 
                }
            }))

            // console.log(`主題已切換至: ${themeName}`)
            return themeConfig
        },

        // 切換主題
        toggleTheme() {
            const currentTheme = this.getSavedTheme()
            const newTheme = currentTheme === 'dark' ? 'light' : 'dark'
            return this.setTheme(newTheme)
        },

        // 更新 CSS 變數
        updateCSSVariables(themeConfig) {
            const root = document.documentElement
            
            root.style.setProperty('--primary-color', themeConfig.primaryColor)
            root.style.setProperty('--background-color', themeConfig.backgroundColor)
            root.style.setProperty('--text-color', themeConfig.textColor)

            // 根據主題設置其他 CSS 變數
            if (themeConfig.isDark) {
                root.style.setProperty('--border-color', '#374151')
                root.style.setProperty('--card-background', '#1f2937')
                root.style.setProperty('--hover-background', '#374151')
            } else {
                root.style.setProperty('--border-color', '#e5e7eb')
                root.style.setProperty('--card-background', '#ffffff')
                root.style.setProperty('--hover-background', '#f3f4f6')
            }
        },

        // 更新 body class
        updateBodyClass(isDark) {
            const body = document.body
            
            if (isDark) {
                body.classList.add('dark-theme')
                body.classList.remove('light-theme')
            } else {
                body.classList.add('light-theme')
                body.classList.remove('dark-theme')
            }
        },

        // 初始化主題
        initTheme() {
            const savedTheme = this.getSavedTheme()
            this.setTheme(savedTheme)
        },

        // 獲取當前主題配置
        getCurrentTheme() {
            const savedTheme = this.getSavedTheme()
            return this.themes[savedTheme] || this.themes.light
        },

        // 檢查是否為深色主題
        isDarkTheme() {
            return this.getSavedTheme() === 'dark'
        },

        // 註冊主題變更監聽器
        onThemeChange(callback) {
            if (typeof callback !== 'function') {
                console.error('Theme change callback must be a function')
                return
            }

            window.addEventListener('theme-changed', callback)
            
            // 返回取消監聽的函數
            return () => {
                window.removeEventListener('theme-changed', callback)
            }
        },

        // 重置主題到默認值
        resetTheme() {
            localStorage.removeItem('dashboard-theme')
            this.setTheme('light')
        },

        // 根據系統偏好設定主題
        setThemeFromSystem() {
            if (window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches) {
                this.setTheme('dark')
            } else {
                this.setTheme('light')
            }
        },

        // 監聽系統主題變更
        watchSystemTheme() {
            if (window.matchMedia) {
                const mediaQuery = window.matchMedia('(prefers-color-scheme: dark)')
                
                const handleChange = (e) => {
                    this.setTheme(e.matches ? 'dark' : 'light')
                }
                
                mediaQuery.addEventListener('change', handleChange)
                
                // 返回取消監聽的函數
                return () => {
                    mediaQuery.removeEventListener('change', handleChange)
                }
            }
            
            return () => {} // 空函數，用於不支持的瀏覽器
        }
    }

    // 暴露到全域
    window.DashboardThemeManager = ThemeManager

})();