// ========================================
// 1. 核心工具函數 (useFormatting.js)
// ========================================

function useFormatting() {
    const formatDate = (date, type = 'date', lang = 'zh-TW') => {
        if (!date) return '';
        
        const dateObj = new Date(date);
        if (isNaN(dateObj.getTime())) return '';
        
        // Simple date formatting without external dependencies
        const year = dateObj.getFullYear();
        const month = String(dateObj.getMonth() + 1).padStart(2, '0');
        const day = String(dateObj.getDate()).padStart(2, '0');
        const hours = String(dateObj.getHours()).padStart(2, '0');
        const minutes = String(dateObj.getMinutes()).padStart(2, '0');
        const ampm = dateObj.getHours() >= 12 ? 'PM' : 'AM';
        const engMonths = [{"01": "Jan"}, {"02": "Feb"}, {"03": "Mar"}, {"04": "Apr"}, {"05": "May"}, {"06": "Jun"}, {"07": "Jul"}, {"08": "Aug"}, {"09": "Sep"}, {"10": "Oct"}, {"11": "Nov"}, {"12": "Dec"}]
        const formattedDate = lang === 'en-US' ? `${engMonths[month]} ${day} ${year}` : `${year} 年 ${month} 月 ${day} 日`

        if (type === 'date') {
            return formattedDate
        } else {
            return `${formattedDate} ${hours}:${minutes} ${ampm}`
        }
    };

    const timeAgo = ({date, lang = "zh-TW"}) => {
        if (!date) return '';
        
        const now = new Date();
        const past = new Date(date);
        const diffInSeconds = Math.floor((now - past) / 1000);
        const days = Math.floor(diffInSeconds / 86400);

        if (diffInSeconds < 60) {
            return lang === "en-US" ? 'Just now' : '剛剛';
        } else if (diffInSeconds < 3600) {
            const minutes = Math.floor(diffInSeconds / 60);
            return lang === "en-US" ? `${minutes} minutes ago` : `${minutes} 分鐘前`;
        } else if (diffInSeconds < 86400) {
            const hours = Math.floor(diffInSeconds / 3600);
            return lang === "en-US" ? `${hours} hours ago` : `${hours} 小時前`;
        }
        
        if (days < 30) {
            return lang === "en-US" ? `${days} days ago` : `${days} 天前`;
        } else if (days < 365) {
            const months = Math.floor(days / 30);
            return lang === "en-US" ? `${months} months ago` : `${months} 個月前`;
        }

        const years = Math.floor(days / 365);
        return lang === "en-US" ? `${years} years ago` : `${years} 年前`;
    };

    return {
        formatDate,
        timeAgo
    };
}

// ========================================
// 2. 認證管理器 (auth-manager.js)
// ========================================

const createAuthManager = () => {
    /**
     * 頁面載入時檢查認證狀態
     */
    const checkAuthOnLoad = async () => {
        try {
            const response = await fetch('/api/auth/status', {
                method: 'GET',
                credentials: 'include',
                headers: {
                    'Content-Type': 'application/json'
                }
            });

            if (response.ok) {
                const authData = await response.json();
                
                if (authData.isAuthenticated) {
                    console.log('User is authenticated:', authData.user);
                    handleAuthenticatedUser(authData.user);
                } else {
                    console.log('User is not authenticated');
                    handleUnauthenticatedUser();
                }
            } else {
                console.log('Auth status check failed');
                handleUnauthenticatedUser();
            }
        } catch (error) {
            console.error('Error checking auth status:', error);
            handleUnauthenticatedUser();
        }
    };

    /**
     * 處理已認證用戶
     */
    const handleAuthenticatedUser = (user) => {
        // 可以在這裡更新 UI，顯示用戶資訊等
        document.body.classList.add('authenticated');
        
        // 觸發認證狀態變更事件
        window.dispatchEvent(new CustomEvent('authStatusChanged', {
            detail: { isAuthenticated: true, user: user }
        }));
    };

    /**
     * 處理未認證用戶
     */
    const handleUnauthenticatedUser = () => {
        document.body.classList.add('unauthenticated');
        
        // 觸發認證狀態變更事件
        window.dispatchEvent(new CustomEvent('authStatusChanged', {
            detail: { isAuthenticated: false, user: null }
        }));
    };

    /**
     * 登出功能
     */
    const logout = async () => {
        try {
            const response = await fetch('/api/auth/logout', {
                method: 'POST',
                credentials: 'include',
                headers: {
                    'Content-Type': 'application/json'
                }
            });

            if (response.ok) {
                console.log('Logout successful');
                // 重新導向到首頁
                window.location.href = '/';
            } else {
                console.error('Logout failed');
            }
        } catch (error) {
            console.error('Error during logout:', error);
        }
    };

    // 初始化
    checkAuthOnLoad();

    return {
        checkAuthOnLoad,
        handleAuthenticatedUser,
        handleUnauthenticatedUser,
        logout
    };
};

// ========================================
// 3. 登入彈窗管理器 (login-popup.js)
// ========================================

const createLoginPopupManager = () => {
    let maxVisibleArticles = 10; // 訪客最多能看的文章數量
    let articleCount = 0;
    let isGuest = true;
    let popupShown = false;

    /**
     * 檢查認證狀態
     */
    const checkAuthStatus = async () => {
        try {
            const response = await fetch('/api/auth/status', {
                credentials: 'include'
            });
            
            if (response.ok) {
                const authData = await response.json();
                isGuest = !authData.isAuthenticated;
            }
        } catch (error) {
            console.error('Error checking auth status:', error);
            isGuest = true;
        }
    };

    /**
     * 設定滾動監控
     */
    const setupScrollMonitoring = () => {
        if (!isGuest) return;

        // 監控文章元素
        const articles = document.querySelectorAll('[data-article-index]');
        
        if (articles.length === 0) return;

        // 使用 Intersection Observer 監控可見性
        const observer = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    const articleIndex = parseInt(entry.target.dataset.articleIndex);
                    
                    if (articleIndex >= maxVisibleArticles && !popupShown) {
                        showLoginPopup();
                        popupShown = true;
                    }
                }
            });
        }, {
            threshold: 0.5 // 當文章 50% 可見時觸發
        });

        // 觀察所有文章
        articles.forEach(article => {
            observer.observe(article);
        });
    };

    /**
     * 顯示登入彈窗
     */
    const showLoginPopup = () => {
        // 創建彈窗 HTML
        const popupHTML = `
            <div id="login-popup-overlay" class="fixed inset-0 bg-black bg-opacity-50 z-50 flex items-center justify-center">
                <div class="bg-white rounded-lg p-6 max-w-md mx-4">
                    <div class="text-center">
                        <h3 class="text-lg font-semibold text-gray-900 mb-4">探索更多內容</h3>
                        <p class="text-gray-600 mb-6">您已瀏覽了免費內容的限制。登入以繼續探索更多精彩內容。</p>
                        <div class="flex space-x-4">
                            <a href="/login" class="flex-1 bg-orange-500 text-white py-2 px-4 rounded-full hover:bg-orange-600 transition-colors">
                                登入
                            </a>
                            <button onclick="window.loginPopupManager.hideLoginPopup()" class="flex-1 bg-gray-300 text-gray-700 py-2 px-4 rounded-full hover:bg-gray-400 transition-colors">
                                關閉
                            </button>
                        </div>
                    </div>
                </div>
            </div>
        `;

        // 添加到頁面
        document.body.insertAdjacentHTML('beforeend', popupHTML);

        // 添加樣式
        addPopupStyles();
    };

    /**
     * 隱藏登入彈窗
     */
    const hideLoginPopup = () => {
        const popup = document.getElementById('login-popup-overlay');
        if (popup) {
            popup.remove();
        }
        popupShown = false;
    };

    /**
     * 添加彈窗樣式
     */
    const addPopupStyles = () => {
        if (document.getElementById('login-popup-styles')) return;

        const styles = `
            <style id="login-popup-styles">
                #login-popup-overlay {
                    animation: fadeIn 0.3s ease-out;
                }
                
                #login-popup-overlay > div {
                    animation: slideIn 0.3s ease-out;
                }
                
                @keyframes fadeIn {
                    from { opacity: 0; }
                    to { opacity: 1; }
                }
                
                @keyframes slideIn {
                    from { 
                        opacity: 0;
                        transform: translateY(-20px);
                    }
                    to { 
                        opacity: 1;
                        transform: translateY(0);
                    }
                }
            </style>
        `;

        document.head.insertAdjacentHTML('beforeend', styles);
    };

    /**
     * 初始化
     */
    const init = () => {
        // 檢查用戶認證狀態
        checkAuthStatus();
        
        // 監聽認證狀態變更
        window.addEventListener('authStatusChanged', (event) => {
            isGuest = !event.detail.isAuthenticated;
            if (!isGuest) {
                hideLoginPopup();
            }
        });

        // 監聽滾動事件
        setupScrollMonitoring();
    };

    // 執行初始化
    init();

    return {
        checkAuthStatus,
        setupScrollMonitoring,
        showLoginPopup,
        hideLoginPopup,
        addPopupStyles,
        init
    };
};

// ========================================
// 4. 主要 Vue.js 應用程式 (site.js)
// ========================================

const createMainApp = (content) => {
    if (typeof Vue === 'undefined') {
        console.log('Vue is not loaded.');
        return;
    }
    window.popupApp = Vue.createApp(content).mount('#app');
};

const initMainVueApp = () => {
    createMainApp({
        setup() {
            const { ref, reactive, computed } = Vue;

            //#region Sidebar State

            const isCollapsed = ref(false);
            
            const toggleSidebar = () => {
                isCollapsed.value = !isCollapsed.value;
                lucide.createIcons()
            }

            //#endregion

            //#region Language

            const toggleLang = () => {
                // current language
                const curLang = document.documentElement.lang || 'zh-TW'

                // switch language
                const changeLang = curLang === 'zh-TW' ? 'en-US' : 'zh-TW'

                // set current language in cookie for 1 year
                // ASP.NET Core 預設的 culture cookie 名稱是 ".AspNetCore.Culture"
                const cultureCookie = `c=${changeLang}|uic=${changeLang}`
                document.cookie = `.AspNetCore.Culture=${cultureCookie}; path=/; max-age=31536000; SameSite=Lax`
                console.log(`Setting culture cookie: ${cultureCookie}`)

                // reload the website let the language change
                window.location.reload()
            }

            //#endregion

            //#region Pop-Up Events

            // Popup State
            const popupState = reactive({
                isVisible: false,
                type: '',
                title: ''
            });

            // Popup Data Storage
            const popupData = reactive({
                Search: [],
                Notify: [],
                Follows: [],
                Collects: []
            })

            // popup helper
            const getPopupTitle = type => {
                const titles = {
                    'Search': '搜尋',
                    'Notify': '通知',
                    'Follows': '追蹤',
                    'Collects': '收藏'
                }

                return titles[type] || '視窗'
            }

            // Update popup data
            const updatePopupData = (type, data) => {
                if (popupData[type] !== undefined)
                    popupData[type] = data
            }

            // Popup click
            const openPopup = async type => {
                popupState.type = type
                popupState.title = getPopupTitle(type)
                popupState.isVisible = true

                try {
                    const res = await fetch('/api/' + type.toLowerCase())
                    const data = await res.json()

                    updatePopupData(type, data)
                } catch (err) {
                    console.log('Fetch Error:', err)
                }
            }

            const closePopup = () => {
                popupState.isVisible = false
                popupState.type = ''
            }

            // Global Methods
            window.toggleFunc = (show, type) => show ? openPopup(type) : closePopup()

            //#endregion

            //#region Search

            const searchQuery = ref('');

            //#endregion

            return {
                // language
                isCollapsed,
                toggleSidebar,
                toggleLang,

                // pop-up
                popupState,
                popupData,
                getPopupTitle,
                openPopup,
                closePopup,
                // 為新版 popup 提供向後兼容
                isOpen: computed(() => popupState.isVisible),
                closeCollectModal: closePopup,

                // search
                searchQuery,
            };
        }
    });
};

// ========================================
// 5. 登入頁面 Vue.js 應用程式
// ========================================

const initLoginApp = () => {
    if (typeof Vue === 'undefined' || !document.getElementById('auth-body')) {
        return;
    }

    const { createApp, ref, onMounted } = Vue;

    createApp({
        setup() {
            // 響應式數據
            const isForgot = ref(false);
            const showPassword = ref(false);
            const loginForm = ref({
                UserName: '',
                Password: '',
                RememberMe: false
            });

            // 切換忘記密碼彈窗
            const toggleOpen = () => isForgot.value = true;

            const toggleClose = (event) => {
                if (event.target === event.currentTarget) {
                    isForgot.value = false;
                }
            };

            // 切換密碼顯示/隱藏
            const togglePasswordVisibility = () => {
                showPassword.value = !showPassword.value;
                // 重新創建 Lucide 圖標
                setTimeout(() => lucide.createIcons(), 0);
            };

            // 更新錯誤訊息
            const updateErrorMsg = (errors) => {
                // 清除之前的錯誤訊息
                document.querySelectorAll('.input-item p').forEach(p => p.textContent = '');

                Object.keys(errors).forEach(field => {
                    const errMsg = errors[field];
                    if (errMsg && errMsg.length > 0) {
                        const el = document.querySelector(`p[asp-validation-for="${field}"]`);
                        if (el && field) {
                            el.textContent = errMsg[0];
                        } else if (!field) {
                            console.log('General error:', errMsg[0]);
                        } else {
                            console.log(`Could not find validation element for ${field}`);
                        }
                    }
                });
            };

            // 表單提交
            const submitForm = async (event) => {
                event.preventDefault();

                try {
                    // 獲取表單數據
                    const formData = new FormData(event.target);
                    const token = formData.get('__RequestVerificationToken');

                    const response = await fetch('/api/login', {
                        method: 'POST',
                        headers: {
                            'Content-Type': 'application/json',
                            'RequestVerificationToken': token
                        },
                        body: JSON.stringify({
                            UserName: loginForm.value.UserName,
                            Password: loginForm.value.Password,
                            RememberMe: loginForm.value.RememberMe
                        })
                    });

                    const result = await response.json();

                    if (result.success && result.redirectUrl) {
                        window.location.href = result.redirectUrl;
                    } else if (result.errors) {
                        updateErrorMsg(result.errors);
                    }
                } catch (error) {
                    console.error('Login error:', error);
                }
            };

            // 組件掛載後初始化
            onMounted(() => {
                // 綁定表單數據到 DOM 元素
                const userNameInput = document.querySelector('input[name="UserName"]');
                const passwordInput = document.querySelector('input[name="Password"]');
                const rememberMeInput = document.querySelector('input[name="RememberMe"]');

                if (userNameInput) {
                    userNameInput.addEventListener('input', (e) => {
                        loginForm.value.UserName = e.target.value;
                    });
                }

                if (passwordInput) {
                    passwordInput.addEventListener('input', (e) => {
                        loginForm.value.Password = e.target.value;
                    });
                }

                if (rememberMeInput) {
                    rememberMeInput.addEventListener('change', (e) => {
                        loginForm.value.RememberMe = e.target.checked;
                    });
                }

                // 初始化 Lucide 圖標
                lucide.createIcons();
            });

            return {
                isForgot,
                showPassword,
                loginForm,
                toggleOpen,
                toggleClose,
                togglePasswordVisibility,
                submitForm
            };
        }
    }).mount('#auth-body');
};

// ========================================
// 6. 註冊頁面 Vue.js 應用程式
// ========================================

const initRegisterApp = () => {
    if (typeof Vue === 'undefined' || !document.getElementById('auth-body')) {
        return;
    }

    const { createApp, ref, onMounted } = Vue;

    createApp({
        setup() {
            // 響應式數據
            const showPassword = ref(false);
            const showConfirmPassword = ref(false);
            const registerForm = ref({
                UserName: '',
                Email: '',
                Password: '',
                PasswordConfirm: ''
            });

            // 切換密碼顯示/隱藏
            const togglePasswordVisibility = () => {
                showPassword.value = !showPassword.value;
                setTimeout(() => lucide.createIcons(), 0);
            };

            const toggleConfirmPasswordVisibility = () => {
                showConfirmPassword.value = !showConfirmPassword.value;
                setTimeout(() => lucide.createIcons(), 0);
            };

            // 更新錯誤訊息
            const updateErrorMsg = (errors) => {
                // 清除之前的錯誤訊息
                document.querySelectorAll('.input-item p').forEach(p => p.textContent = '');

                Object.keys(errors).forEach(field => {
                    const errMsg = errors[field];
                    if (errMsg && errMsg.length > 0) {
                        const el = document.querySelector(`p[asp-validation-for="${field}"]`);
                        if (el && field) {
                            el.textContent = errMsg[0];
                        } else if (!field) {
                            console.log('General error:', errMsg[0]);
                        } else {
                            console.log(`Could not find validation element for ${field}`);
                        }
                    }
                });
            };

            // 表單提交
            const submitForm = async (event) => {
                event.preventDefault();

                try {
                    // 獲取表單數據
                    const formData = new FormData(event.target);
                    const token = formData.get('__RequestVerificationToken');

                    const response = await fetch('/api/register', {
                        method: 'POST',
                        headers: {
                            'Content-Type': 'application/json',
                            'RequestVerificationToken': token
                        },
                        body: JSON.stringify({
                            UserName: registerForm.value.UserName,
                            Email: registerForm.value.Email,
                            Password: registerForm.value.Password,
                            PasswordConfirm: registerForm.value.PasswordConfirm
                        })
                    });

                    const result = await response.json();

                    if (result.success && result.redirectUrl) {
                        window.location.href = result.redirectUrl;
                    } else if (result.errors) {
                        updateErrorMsg(result.errors);
                    }
                } catch (error) {
                    console.error('Register error:', error);
                }
            };

            // 組件掛載後初始化
            onMounted(() => {
                // 綁定表單數據到 DOM 元素
                const userNameInput = document.querySelector('input[name="UserName"]');
                const emailInput = document.querySelector('input[name="Email"]');
                const passwordInput = document.querySelector('input[name="Password"]');
                const confirmPasswordInput = document.querySelector('input[name="PasswordConfirm"]');

                if (userNameInput) {
                    userNameInput.addEventListener('input', (e) => {
                        registerForm.value.UserName = e.target.value;
                    });
                }

                if (emailInput) {
                    emailInput.addEventListener('input', (e) => {
                        registerForm.value.Email = e.target.value;
                    });
                }

                if (passwordInput) {
                    passwordInput.addEventListener('input', (e) => {
                        registerForm.value.Password = e.target.value;
                    });
                }

                if (confirmPasswordInput) {
                    confirmPasswordInput.addEventListener('input', (e) => {
                        registerForm.value.PasswordConfirm = e.target.value;
                    });
                }

                // 初始化 Lucide 圖標
                lucide.createIcons();
            });

            return {
                showPassword,
                showConfirmPassword,
                registerForm,
                togglePasswordVisibility,
                toggleConfirmPasswordVisibility,
                submitForm
            };
        }
    }).mount('#auth-body');
};

// ========================================
// 7. 錯誤頁面處理
// ========================================

const initErrorPage = () => {
    // 初始化 Lucide 圖標
    lucide.createIcons();

    // 等待 DOM 加載完成
    document.addEventListener('DOMContentLoaded', function() {
        // 綁定 "try again" 按鈕事件
        const tryAgainBtn = document.getElementById('tryAgain');
        if (tryAgainBtn) {
            tryAgainBtn.addEventListener('click', function() {
                window.location.reload();
            });
        }
    });
};

// ========================================
// 8. 認證布局處理
// ========================================

const initAuthLayout = () => {
    // 初始化 Lucide 圖標
    lucide.createIcons();

    // 等待 DOM 加載完成
    document.addEventListener('DOMContentLoaded', function() {
        // 檢查是否為登入頁面，並添加對應的 CSS 類別
        const isLoginPage = window.location.pathname.toLowerCase() === '/login';
        const authBody = document.getElementById('auth-body');
        
        if (authBody) {
            authBody.classList.add(isLoginPage ? 'auth-layout_login' : 'auth-layout_register');
        }
    });
};

// ========================================
// 9. 全域初始化
// ========================================

// 創建全域實例
let matrixApp = {
    authManager: null,
    loginPopupManager: null,
    
    init() {
        // 初始化 Lucide 圖標
        if (typeof lucide !== 'undefined') {
            lucide.createIcons();
        }

        // 初始化認證管理器
        this.authManager = createAuthManager();
        
        // 初始化登入彈窗管理器
        this.loginPopupManager = createLoginPopupManager();

        // 根據頁面類型初始化相應的應用
        this.initPageSpecificApps();

        // 設置全域方法
        this.setupGlobalMethods();
    },

    initPageSpecificApps() {
        const path = window.location.pathname.toLowerCase();
        
        // 主應用程式 (如果存在 #app 元素)
        if (document.getElementById('app')) {
            initMainVueApp();
        }

        // 登入頁面
        if (path === '/login' && document.getElementById('auth-body')) {
            initLoginApp();
        }

        // 註冊頁面
        if (path === '/register' && document.getElementById('auth-body')) {
            initRegisterApp();
        }

        // 錯誤頁面
        if (document.getElementById('error-body')) {
            initErrorPage();
        }

        // 認證布局
        if (document.getElementById('auth-body')) {
            initAuthLayout();
        }
    },

    setupGlobalMethods() {
        // 全域方法設置
        window.authManager = this.authManager;
        window.loginPopupManager = this.loginPopupManager;
        
        // 全域登出函數
        window.logout = () => {
            this.authManager.logout();
        };

        // 格式化工具
        window.useFormatting = useFormatting;
    }
};

// DOM 載入完成後自動初始化
document.addEventListener('DOMContentLoaded', function() {
    matrixApp.init();
});

// 為了向後兼容，也提供 window 載入事件
window.addEventListener('load', function() {
    // 確保 Lucide 圖標正確載入
    if (typeof lucide !== 'undefined') {
        lucide.createIcons();
    }
});

// 導出主要對象供其他腳本使用
window.matrixApp = matrixApp;