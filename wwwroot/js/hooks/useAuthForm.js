/**
 * 認證表單 Hook
 * 提供登入和註冊表單的共同邏輯 - 純 Vue 3 setup() 實現
 */
const useAuthForm = () => {
    /**
     * 創建認證表單邏輯
     * @param {Object} vueRefs - Vue 3 響應式引用 { ref, reactive, onMounted, ... }
     * @param {string} formType - 表單類型 ('login' | 'register')
     * @returns {Object} - 表單邏輯對象
     */
    const createAuthForm = (vueRefs, formType = 'login') => {
        const { ref, reactive, onMounted } = vueRefs;
        
        // 獲取工具函數
        const api = useApi();
        const { updateErrorMessages, createFormValidation } = useFormValidation();
        const { createPasswordToggle } = usePasswordToggle();

        // 創建驗證實例
        const validation = createFormValidation(vueRefs);

        // 基礎表單數據
        const baseFormData = {
            UserName: '',
            Password: ''
        };

        // 根據表單類型添加額外字段
        const formData = reactive(formType === 'register' ? {
            ...baseFormData,
            Email: '',
            PasswordConfirm: ''
        } : {
            ...baseFormData,
            RememberMe: false
        });

        // 表單狀態
        const formState = reactive({
            isSubmitting: false,
            isValid: true
        });

        // 密碼顯示切換
        const passwordToggle = createPasswordToggle(vueRefs);

        /**
         * 表單驗證規則
         */
        const getValidationRules = () => {
            const baseRules = {
                UserName: {
                    required: true,
                    minLength: 3,
                    maxLength: 20
                },
                Password: {
                    required: true,
                    minLength: 8,
                    maxLength: 50
                }
            };

            if (formType === 'register') {
                return {
                    ...baseRules,
                    Email: {
                        required: true,
                        pattern: /^[^\s@]+@[^\s@]+\.[^\s@]+$/,
                        maxLength: 50
                    },
                    PasswordConfirm: {
                        required: true,
                        match: formData.Password
                    }
                };
            }

            return baseRules;
        };

        /**
         * 綁定表單輸入事件 - 使用原生 DOM API 而非 jQuery
         */
        const bindFormInputs = () => {
            const inputMappings = [
                { selector: 'input[name="UserName"]', field: 'UserName' },
                { selector: 'input[name="Password"]', field: 'Password' }
            ];

            if (formType === 'register') {
                inputMappings.push(
                    { selector: 'input[name="Email"]', field: 'Email' },
                    { selector: 'input[name="PasswordConfirm"]', field: 'PasswordConfirm' }
                );
            } else {
                inputMappings.push(
                    { selector: 'input[name="RememberMe"]', field: 'RememberMe', type: 'checkbox' }
                );
            }

            // 使用原生 DOM API 綁定事件
            inputMappings.forEach(({ selector, field, type }) => {
                const input = document.querySelector(selector);
                if (input) {
                    const eventType = type === 'checkbox' ? 'change' : 'input';
                    
                    input.addEventListener(eventType, (e) => {
                        const value = type === 'checkbox' ? e.target.checked : e.target.value;
                        formData[field] = value;
                        
                        // 即時驗證
                        if (value) {
                            const rules = getValidationRules();
                            const error = validation.validateField(field, value, rules[field]);
                            if (error) {
                                validation.updateErrors({ [field]: [error] });
                            } else {
                                // 清除該字段的錯誤
                                const currentErrors = { ...validation.validationState.errors };
                                delete currentErrors[field];
                                validation.updateErrors(currentErrors);
                            }
                        }
                    });

                    // 設置初始值
                    if (type === 'checkbox') {
                        input.checked = formData[field];
                    } else {
                        input.value = formData[field];
                    }
                }
            });
        };

        /**
         * 提交表單 - 使用純 fetch API
         */
        const submitForm = async (event) => {
            event.preventDefault();
            
            if (formState.isSubmitting) return;

            formState.isSubmitting = true;
            validation.clearValidationErrors();

            try {
                // 前端驗證
                const validationRules = getValidationRules();
                const validationResult = validation.validateForm(formData, validationRules);
                
                if (!validationResult.isValid) {
                    validation.updateErrors(validationResult.errors);
                    return;
                }

                // 獲取防偽 token - 使用原生 DOM API
                const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
                const token = tokenInput?.value;

                if (!token) {
                    throw new Error('Missing CSRF token');
                }

                // 根據表單類型調用相應的 API
                const apiMethod = formType === 'register' ? 'register' : 'login';
                const result = await api[apiMethod](formData, token);

                if (result.success) {
                    const data = result.data;
                    if (data.success) {
                        // 處理成功情況
                        if (formType === 'register' && data.needEmailConfirmation) {
                            // 處理註冊成功但需要郵件確認
                            await handleEmailConfirmation(data, token);
                        } else if (data.redirectUrl) {
                            // 直接重定向
                            window.location.href = data.redirectUrl;
                        }
                    } else if (data.errors) {
                        validation.updateErrors(data.errors);
                    } else {
                        throw new Error(data.message || `${formType} failed`);
                    }
                } else {
                    throw new Error(result.error);
                }
            } catch (error) {
                console.error(`${formType} error:`, error);
                validation.updateErrors({ '': [error.message || '提交失敗，請稍後再試'] });
            } finally {
                formState.isSubmitting = false;
            }
        };

        /**
         * 處理郵件確認（僅用於註冊）
         */
        const handleEmailConfirmation = async (registerResult, token) => {
            // 這裡可以實現郵件確認相關邏輯
            console.log('Registration successful, email confirmation needed:', registerResult);
            
            // 暫時重定向到登入頁面
            setTimeout(() => {
                window.location.href = '/login';
            }, 2000);
        };

        /**
         * 重置表單
         */
        const resetForm = () => {
            Object.keys(formData).forEach(key => {
                if (typeof formData[key] === 'boolean') {
                    formData[key] = false;
                } else {
                    formData[key] = '';
                }
            });
            
            validation.clearValidationErrors();
            formState.isSubmitting = false;
        };

        /**
         * 初始化表單
         */
        const initForm = () => {
            bindFormInputs();

            // 初始化 Lucide 圖標
            if (typeof lucide !== 'undefined') {
                lucide.createIcons();
            }
        };

        // 組件掛載後初始化
        onMounted(initForm);

        return {
            formData,
            formState,
            validation,
            submitForm,
            resetForm,
            ...passwordToggle
        };
    };

    return {
        createAuthForm
    };
};

// 將 Hook 掛載到全域 - 同時支援命名空間和向後兼容
window.Matrix.hooks.useAuthForm = useAuthForm;
window.useAuthForm = useAuthForm; // 向後兼容