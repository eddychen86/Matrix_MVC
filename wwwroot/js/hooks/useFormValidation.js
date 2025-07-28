/**
 * 表單驗證 Hook
 * 提供統一的表單錯誤處理和驗證功能 - 純 Vue 3 setup() 實現
 */
const useFormValidation = () => {
    /**
     * 更新表單錯誤訊息 - 不依賴 jQuery
     * @param {Object} errors - 錯誤對象
     */
    const updateErrorMessages = (errors) => {
        // 清除之前的錯誤訊息 - 使用原生 DOM API
        const errorElements = document.querySelectorAll('.input-item p[asp-validation-for]');
        errorElements.forEach(el => el.textContent = '');

        // 更新新的錯誤訊息
        Object.entries(errors).forEach(([field, messages]) => {
            if (messages && messages.length > 0) {
                const errorElement = document.querySelector(`p[asp-validation-for="${field}"]`);
                if (errorElement) {
                    errorElement.textContent = messages[0];
                    errorElement.classList.add('error-visible');
                } else if (!field) {
                    // 處理通用錯誤訊息
                    console.warn('General form error:', messages[0]);
                    showGeneralError(messages[0]);
                } else {
                    console.warn(`Could not find validation element for field: ${field}`);
                }
            }
        });
    };

    /**
     * 顯示通用錯誤訊息
     * @param {string} message - 錯誤訊息
     */
    const showGeneralError = (message) => {
        // 尋找通用錯誤顯示區域
        let generalErrorEl = document.querySelector('.general-error-message');
        
        if (!generalErrorEl) {
            // 創建通用錯誤顯示元素
            generalErrorEl = document.createElement('div');
            generalErrorEl.className = 'general-error-message text-red-500 text-center mb-4';
            
            // 插入到表單頂部
            const form = document.querySelector('form');
            if (form) {
                form.insertBefore(generalErrorEl, form.firstChild);
            }
        }
        
        generalErrorEl.textContent = message;
        generalErrorEl.style.display = 'block';
    };

    /**
     * 清除所有錯誤訊息
     */
    const clearErrors = () => {
        // 清除字段錯誤
        const errorElements = document.querySelectorAll('.input-item p[asp-validation-for]');
        errorElements.forEach(el => {
            el.textContent = '';
            el.classList.remove('error-visible');
        });

        // 清除通用錯誤
        const generalErrorEl = document.querySelector('.general-error-message');
        if (generalErrorEl) {
            generalErrorEl.style.display = 'none';
            generalErrorEl.textContent = '';
        }
    };

    /**
     * 驗證單個字段
     * @param {string} field - 字段名
     * @param {*} value - 字段值
     * @param {Object} rules - 驗證規則
     * @returns {string|null} - 錯誤訊息或 null
     */
    const validateField = (field, value, rules) => {
        if (!rules) return null;

        const { required, minLength, maxLength, pattern, match } = rules;

        // 必填驗證
        if (required && (!value || String(value).trim() === '')) {
            return `${field} 為必填項目`;
        }

        // 如果值為空且非必填，跳過其他驗證
        if (!value || String(value).trim() === '') {
            return null;
        }

        const stringValue = String(value);

        // 最小長度驗證
        if (minLength && stringValue.length < minLength) {
            return `${field} 至少需要 ${minLength} 個字元`;
        }

        // 最大長度驗證
        if (maxLength && stringValue.length > maxLength) {
            return `${field} 不能超過 ${maxLength} 個字元`;
        }

        // 正則表達式驗證
        if (pattern && !pattern.test(stringValue)) {
            return `${field} 格式不正確`;
        }

        // 匹配其他字段驗證（如確認密碼）
        if (match && stringValue !== String(match)) {
            return `${field} 不匹配`;
        }

        return null;
    };

    /**
     * 驗證整個表單
     * @param {Object} formData - 表單數據
     * @param {Object} validationRules - 驗證規則
     * @returns {Object} - { isValid: boolean, errors: Object }
     */
    const validateForm = (formData, validationRules) => {
        const errors = {};
        let isValid = true;

        Object.entries(validationRules).forEach(([field, rules]) => {
            const error = validateField(field, formData[field], rules);
            if (error) {
                errors[field] = [error];
                isValid = false;
            }
        });

        return { isValid, errors };
    };

    /**
     * 創建表單驗證實例（用於 Vue setup()）
     * @param {Object} vueRefs - Vue 3 響應式引用
     * @returns {Object} - 驗證方法集合
     */
    const createFormValidation = (vueRefs) => {
        const { reactive } = vueRefs;
        
        const validationState = reactive({
            errors: {},
            isValidating: false,
            hasErrors: false
        });

        /**
         * 響應式的錯誤更新
         */
        const updateErrors = (newErrors) => {
            validationState.errors = newErrors;
            validationState.hasErrors = Object.keys(newErrors).length > 0;
            updateErrorMessages(newErrors);
        };

        /**
         * 響應式的錯誤清除
         */
        const clearValidationErrors = () => {
            validationState.errors = {};
            validationState.hasErrors = false;
            clearErrors();
        };

        return {
            validationState,
            updateErrors,
            clearValidationErrors,
            validateField,
            validateForm
        };
    };

    return {
        updateErrorMessages,
        showGeneralError,
        clearErrors,
        validateField,
        validateForm,
        createFormValidation
    };
};

// 將 Hook 掛載到全域 - 同時支援命名空間和向後兼容
window.Matrix.hooks.useFormValidation = useFormValidation;
window.useFormValidation = useFormValidation; // 向後兼容