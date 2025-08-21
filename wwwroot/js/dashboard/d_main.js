/**
 * Dashboard Main App - 重構版本
 * 整合各個模組化組件，簡化主文件
 * 支援 zh-TW 和 en-US 語系切換
 */

(function() {
    'use strict';

    // 檢查必要的依賴
    if (typeof Vue === 'undefined') {
        console.error('Vue is required for Dashboard')
        return
    }

    // 模組載入順序很重要
    const loadModules = async () => {
        try {
            // 1. 載入核心模組（按依賴順序）
            await loadScript('/js/dashboard/core/fetch-interceptor.js')
            await loadScript('/js/dashboard/core/state-manager.js') 
            await loadScript('/js/dashboard/core/language-manager.js')
            await loadScript('/js/dashboard/core/theme-manager.js')
            await loadScript('/js/dashboard/core/menu-app.js')

            // console.log('✅ Dashboard 核心模組載入完成')
            
            // 2. 初始化全域狀態
            if (typeof window.initGlobalState === 'function') {
                await window.initGlobalState()
                // console.log('✅ Dashboard 全域狀態初始化完成')
            }

            // 3. 初始化主題管理
            if (window.DashboardThemeManager) {
                window.DashboardThemeManager.initTheme()
                // console.log('✅ Dashboard 主題管理初始化完成')
            }

            // 4. 啟動選單應用
            if (typeof window.createDashboardMenuApp === 'function') {
                window.createDashboardMenuApp()
                // console.log('✅ Dashboard 選單應用啟動完成')
            }

        } catch (error) {
            console.error('❌ Dashboard 模組載入失敗:', error)
        }
    }

    // 動態載入 script 的輔助函數
    const loadScript = (src) => {
        return new Promise((resolve, reject) => {
            // 檢查是否已載入
            if (document.querySelector(`script[src="${src}"]`)) {
                resolve()
                return
            }

            const script = document.createElement('script')
            script.src = src
            script.async = true
            script.onload = () => resolve()
            script.onerror = () => reject(new Error(`Failed to load ${src}`))
            document.head.appendChild(script)
        })
    }

    // 提供向後相容的全域方法
    const setupBackwardCompatibility = () => {
        // 語言切換相容性
        if (!window.toggleLang && window.DashboardLanguageManager) {
            window.toggleLang = window.DashboardLanguageManager.toggleLanguage.bind(window.DashboardLanguageManager)
        }

        // 主題切換相容性
        if (!window.toggleTheme && window.DashboardThemeManager) {
            window.toggleTheme = window.DashboardThemeManager.toggleTheme.bind(window.DashboardThemeManager)
        }

        // 狀態管理相容性
        if (!window.dashboardGlobalState && window.DashboardStateManager) {
            window.dashboardGlobalState = {
                state: window.DashboardStateManager.getState(),
                actions: window.DashboardStateManager,
                useGlobalState: window.useGlobalState
            }
        }
    }

    // 錯誤處理和回退機制
    const handleInitializationError = (error) => {
        console.error('Dashboard 初始化失敗:', error)
        
        // 嘗試載入原始版本作為回退
        if (typeof window.__DASHBOARD_FALLBACK__ !== 'undefined') {
            console.warn('正在載入 Dashboard 回退版本...')
            loadScript('/js/dashboard/d_main-original.js')
                .catch(fallbackError => console.error('❌ Dashboard 回退版本載入失敗:', fallbackError))
        }
    }

    // 主要初始化流程
    const initialize = async () => {
        try {
            await loadModules()
            setupBackwardCompatibility()
            // console.log('🎉 Dashboard 初始化完成')
        } catch (error) {
            handleInitializationError(error)
        }
    }

    // 根據 DOM 狀態決定初始化時機
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initialize)
    } else {
        initialize()
    }

    // 暴露重構版本標識
    window.__DASHBOARD_REFACTORED__ = true
    window.__DASHBOARD_VERSION__ = '2.0.0'

})();