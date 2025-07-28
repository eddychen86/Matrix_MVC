/**
 * 認證表單組件
 * 提供登入和註冊表單的 Vue 組件 - 純 Vue 3 setup() 實現
 */
const useAuthForms = () => {
    /**
     * 創建登入表單應用程式
     */
    const initLoginApp = () => {
        if (typeof Vue === 'undefined' || !document.getElementById('auth-body')) {
            return;
        }

        const { createApp, ref } = Vue;

        createApp({
            setup() {
                // 獲取 Vue 響應式引用
                const vueRefs = { ref, reactive: Vue.reactive, onMounted: Vue.onMounted };
                
                // 忘記密碼彈窗狀態
                const isForgot = ref(false);

                // 使用認證表單 Hook
                const { createAuthForm } = useAuthForm();
                const authForm = createAuthForm(vueRefs, 'login');

                /**
                 * 切換忘記密碼彈窗
                 */
                const toggleForgotPassword = () => {
                    isForgot.value = true;
                };

                /**
                 * 關閉忘記密碼彈窗
                 */
                const closeForgotPassword = (event) => {
                    // 只有點擊背景時才關閉
                    if (event && event.target === event.currentTarget) {
                        isForgot.value = false;
                    }
                };

                /**
                 * 處理忘記密碼表單提交
                 */
                const handleForgotPassword = async (event) => {
                    event.preventDefault();
                    
                    const formData = new FormData(event.target);
                    const email = formData.get('email');
                    
                    if (!email) {
                        alert('請輸入電子郵件地址');
                        return;
                    }

                    try {
                        // 這裡可以調用忘記密碼 API
                        console.log('Forgot password request for:', email);
                        alert('密碼重設連結已發送到您的信箱');
                        isForgot.value = false;
                    } catch (error) {
                        console.error('Forgot password error:', error);
                        alert('發送失敗，請稍後再試');
                    }
                };

                return {
                    // 忘記密碼相關
                    isForgot,
                    toggleForgotPassword,
                    closeForgotPassword,
                    handleForgotPassword,
                    
                    // 認證表單相關
                    ...authForm
                };
            }
        }).mount('#auth-body');
    };

    /**
     * 創建註冊表單應用程式
     */
    const initRegisterApp = () => {
        if (typeof Vue === 'undefined' || !document.getElementById('auth-body')) {
            return;
        }

        const { createApp } = Vue;

        createApp({
            setup() {
                // 獲取 Vue 響應式引用
                const vueRefs = { 
                    ref: Vue.ref, 
                    reactive: Vue.reactive, 
                    onMounted: Vue.onMounted,
                    computed: Vue.computed,
                    watch: Vue.watch
                };

                // 使用認證表單 Hook
                const { createAuthForm } = useAuthForm();
                const authForm = createAuthForm(vueRefs, 'register');

                /**
                 * 密碼強度檢查
                 */
                const passwordStrength = Vue.computed(() => {
                    const password = authForm.formData.Password;
                    if (!password) return { level: 0, text: '' };

                    let score = 0;
                    let feedback = [];

                    // 長度檢查
                    if (password.length >= 8) score += 1;
                    else feedback.push('至少8個字符');

                    // 大小寫檢查
                    if (/[a-z]/.test(password) && /[A-Z]/.test(password)) score += 1;
                    else feedback.push('包含大小寫字母');

                    // 數字檢查
                    if (/\d/.test(password)) score += 1;
                    else feedback.push('包含數字');

                    // 特殊字符檢查
                    if (/[!@#$%^&*(),.?":{}|<>]/.test(password)) score += 1;
                    else feedback.push('包含特殊字符');

                    const levels = ['弱', '一般', '良好', '強'];
                    const colors = ['text-red-500', 'text-yellow-500', 'text-blue-500', 'text-green-500'];

                    return {
                        level: score,
                        text: levels[score] || '弱',
                        color: colors[score] || 'text-red-500',
                        feedback: feedback.join('、')
                    };
                });

                /**
                 * 檢查密碼是否匹配
                 */
                const passwordsMatch = Vue.computed(() => {
                    const { Password, PasswordConfirm } = authForm.formData;
                    if (!Password || !PasswordConfirm) return null;
                    return Password === PasswordConfirm;
                });

                /**
                 * 處理條款同意
                 */
                const termsAccepted = Vue.ref(false);
                const toggleTerms = () => {
                    termsAccepted.value = !termsAccepted.value;
                };

                /**
                 * 增強的表單提交驗證
                 */
                const enhancedSubmitForm = async (event) => {
                    if (!termsAccepted.value) {
                        alert('請先同意服務條款和隱私政策');
                        return;
                    }

                    if (passwordStrength.value.level < 2) {
                        alert('密碼強度不足，請設置更強的密碼');
                        return;
                    }

                    if (!passwordsMatch.value) {
                        alert('密碼確認不匹配');
                        return;
                    }

                    // 調用原始提交方法
                    await authForm.submitForm(event);
                };

                return {
                    // 認證表單相關
                    ...authForm,
                    
                    // 註冊專用功能
                    passwordStrength,
                    passwordsMatch,
                    termsAccepted,
                    toggleTerms,
                    submitForm: enhancedSubmitForm
                };
            }
        }).mount('#auth-body');
    };

    /**
     * 初始化認證布局 - 使用純 DOM API 而非 jQuery
     */
    const initAuthLayout = () => {
        // 初始化 Lucide 圖標
        if (typeof lucide !== 'undefined') {
            lucide.createIcons();
        }

        // DOM 就緒後執行
        const initializeLayout = () => {
            // 檢查是否為登入頁面，並添加對應的 CSS 類別
            const isLoginPage = window.location.pathname.toLowerCase() === '/login';
            const authBody = document.getElementById('auth-body');
            
            if (authBody) {
                // 移除可能存在的舊類名
                authBody.classList.remove('auth-layout_login', 'auth-layout_register');
                
                // 添加對應的類名
                const layoutClass = isLoginPage ? 'auth-layout_login' : 'auth-layout_register';
                authBody.classList.add(layoutClass);
                
                // 添加載入動畫效果
                authBody.style.opacity = '0';
                authBody.style.transform = 'translateY(20px)';
                
                // 延遲顯示以創造載入效果
                requestAnimationFrame(() => {
                    authBody.style.transition = 'opacity 0.3s ease, transform 0.3s ease';
                    authBody.style.opacity = '1';
                    authBody.style.transform = 'translateY(0)';
                });
            }

            // 處理響應式設計
            handleResponsiveLayout();
        };

        /**
         * 處理響應式布局
         */
        const handleResponsiveLayout = () => {
            const updateLayout = () => {
                const authBody = document.getElementById('auth-body');
                if (!authBody) return;

                const isMobile = window.innerWidth < 768;
                
                if (isMobile) {
                    authBody.classList.add('auth-mobile');
                } else {
                    authBody.classList.remove('auth-mobile');
                }
            };

            // 初始檢查
            updateLayout();

            // 監聽視窗大小變化
            window.addEventListener('resize', updateLayout);
        };

        // 如果 DOM 已就緒，直接執行；否則等待
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', initializeLayout);
        } else {
            initializeLayout();
        }
    };

    /**
     * 通用的輸入框焦點效果
     */
    const addInputFocusEffects = () => {
        const inputs = document.querySelectorAll('.input-item input');
        
        inputs.forEach(input => {
            const inputItem = input.closest('.input-item');
            
            input.addEventListener('focus', () => {
                inputItem?.classList.add('focused');
            });
            
            input.addEventListener('blur', () => {
                inputItem?.classList.remove('focused');
                if (input.value.trim()) {
                    inputItem?.classList.add('has-value');
                } else {
                    inputItem?.classList.remove('has-value');
                }
            });

            // 初始狀態檢查
            if (input.value.trim()) {
                inputItem?.classList.add('has-value');
            }
        });
    };

    return {
        initLoginApp,
        initRegisterApp,
        initAuthLayout,
        addInputFocusEffects
    };
};

// 將認證表單組件掛載到全域 - 同時支援命名空間和向後兼容
window.Matrix.components.useAuthForms = useAuthForms;
window.useAuthForms = useAuthForms; // 向後兼容