/**
 * DOM 工具函數
 * 提供常用的 DOM 操作和選擇器功能
 */
const useDom = () => {
    /**
     * 安全的查詢選擇器
     */
    const $ = (selector, context = document) => {
        return context.querySelector(selector);
    };

    /**
     * 查詢多個元素
     */
    const $$ = (selector, context = document) => {
        return context.querySelectorAll(selector);
    };

    /**
     * 創建元素
     */
    const createElement = (tag, attributes = {}, children = []) => {
        const element = document.createElement(tag);
        
        // 設置屬性
        Object.entries(attributes).forEach(([key, value]) => {
            if (key === 'className') {
                element.className = value;
            } else if (key === 'innerHTML') {
                element.innerHTML = value;
            } else {
                element.setAttribute(key, value);
            }
        });

        // 添加子元素
        children.forEach(child => {
            if (typeof child === 'string') {
                element.appendChild(document.createTextNode(child));
            } else {
                element.appendChild(child);
            }
        });

        return element;
    };

    /**
     * 添加事件監聽器
     */
    const on = (element, event, handler, options = {}) => {
        if (typeof element === 'string') {
            element = $(element);
        }
        if (element) {
            element.addEventListener(event, handler, options);
        }
    };

    /**
     * 移除事件監聽器
     */
    const off = (element, event, handler) => {
        if (typeof element === 'string') {
            element = $(element);
        }
        if (element) {
            element.removeEventListener(event, handler);
        }
    };

    /**
     * 切換類名
     */
    const toggleClass = (element, className) => {
        if (typeof element === 'string') {
            element = $(element);
        }
        if (element) {
            element.classList.toggle(className);
        }
    };

    /**
     * 添加類名
     */
    const addClass = (element, className) => {
        if (typeof element === 'string') {
            element = $(element);
        }
        if (element) {
            element.classList.add(className);
        }
    };

    /**
     * 移除類名
     */
    const removeClass = (element, className) => {
        if (typeof element === 'string') {
            element = $(element);
        }
        if (element) {
            element.classList.remove(className);
        }
    };

    /**
     * 檢查元素是否包含類名
     */
    const hasClass = (element, className) => {
        if (typeof element === 'string') {
            element = $(element);
        }
        return element ? element.classList.contains(className) : false;
    };

    /**
     * 等待 DOM 載入完成
     */
    const ready = (callback) => {
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', callback);
        } else {
            callback();
        }
    };

    /**
     * 獲取表單數據
     */
    const getFormData = (form) => {
        if (typeof form === 'string') {
            form = $(form);
        }
        if (!form) return null;

        const formData = new FormData(form);
        const data = {};
        
        for (let [key, value] of formData.entries()) {
            data[key] = value;
        }
        
        return data;
    };

    /**
     * 清除表單錯誤訊息
     */
    const clearFormErrors = (formSelector = '.input-item p') => {
        $$(formSelector).forEach(p => p.textContent = '');
    };

    return {
        $,
        $$,
        createElement,
        on,
        off,
        toggleClass,
        addClass,
        removeClass,
        hasClass,
        ready,
        getFormData,
        clearFormErrors
    };
};

// 將 DOM 工具掛載到全域 - 同時支援命名空間和向後兼容
window.Matrix.utils.useDom = useDom;
window.useDom = useDom; // 向後兼容