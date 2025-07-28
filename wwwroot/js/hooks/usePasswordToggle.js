/**
 * 密碼顯示/隱藏 Hook
 * 提供密碼輸入框的可見性切換功能 - 純 Vue 3 setup() 實現
 */
const usePasswordToggle = () => {
    /**
     * 創建密碼切換功能
     * @param {Object} vueRefs - Vue 3 響應式引用 { ref, reactive, computed, ... }
     * @returns {Object} - 包含響應式狀態和切換方法
     */
    const createPasswordToggle = (vueRefs) => {
        const { ref } = vueRefs;
        
        const showPassword = ref(false);
        const showConfirmPassword = ref(false);

        /**
         * 切換主密碼顯示/隱藏
         */
        const togglePasswordVisibility = () => {
            showPassword.value = !showPassword.value;
            // 重新創建 Lucide 圖標
            requestAnimationFrame(() => {
                if (typeof lucide !== 'undefined') {
                    lucide.createIcons();
                }
            });
        };

        /**
         * 切換確認密碼顯示/隱藏
         */
        const toggleConfirmPasswordVisibility = () => {
            showConfirmPassword.value = !showConfirmPassword.value;
            // 重新創建 Lucide 圖標
            requestAnimationFrame(() => {
                if (typeof lucide !== 'undefined') {
                    lucide.createIcons();
                }
            });
        };

        /**
         * 通用的密碼切換方法
         * @param {string} type - 密碼類型 ('password' | 'confirmPassword')
         */
        const togglePasswordType = (type) => {
            if (type === 'confirmPassword') {
                toggleConfirmPasswordVisibility();
            } else {
                togglePasswordVisibility();
            }
        };

        /**
         * 獲取密碼輸入類型
         * @param {string} type - 密碼類型
         */
        const getPasswordType = (type) => {
            if (type === 'confirmPassword') {
                return showConfirmPassword.value ? 'text' : 'password';
            }
            return showPassword.value ? 'text' : 'password';
        };

        /**
         * 獲取眼睛圖標名稱
         * @param {string} type - 密碼類型
         */
        const getEyeIcon = (type) => {
            const isVisible = type === 'confirmPassword' ? showConfirmPassword.value : showPassword.value;
            return isVisible ? 'eye' : 'eye-off';
        };

        return {
            showPassword,
            showConfirmPassword,
            togglePasswordVisibility,
            toggleConfirmPasswordVisibility,
            togglePasswordType,
            getPasswordType,
            getEyeIcon
        };
    };

    return {
        createPasswordToggle
    };
};

// 將 Hook 掛載到全域 - 同時支援命名空間和向後兼容
window.Matrix.hooks.usePasswordToggle = usePasswordToggle;
window.usePasswordToggle = usePasswordToggle; // 向後兼容