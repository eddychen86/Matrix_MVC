const { createApp, ref, onMounted, nextTick } = Vue

createApp({
    setup() {
        // 響應式數據
        const showPassword = ref(false)
        const showConfirmPassword = ref(false)
        const isSubmitting = ref(false)
        const passwordIcon = ref(null)
        const confirmPasswordIcon = ref(null)
        const registerForm = ref({
            UserName: '',
            Email: '',
            Password: '',
            PasswordConfirm: ''
        })

        // 切換密碼顯示/隱藏
        const togglePasswordVisibility = () => {
            showPassword.value = !showPassword.value
            
            // 直接修改 data-lucide 屬性，避免 Vue 和 Lucide 衝突
            if (passwordIcon.value) {
                const iconName = showPassword.value ? 'eye' : 'eye-closed'
                passwordIcon.value.setAttribute('data-lucide', iconName)
                
                // 重新創建圖標
                nextTick(() => {
                    setTimeout(() => {
                        lucide.createIcons()
                    }, 0)
                })
            }
        }

        const toggleConfirmPasswordVisibility = () => {
            showConfirmPassword.value = !showConfirmPassword.value
            
            // 直接修改 data-lucide 屬性，避免 Vue 和 Lucide 衝突
            if (confirmPasswordIcon.value) {
                const iconName = showConfirmPassword.value ? 'eye' : 'eye-closed'
                confirmPasswordIcon.value.setAttribute('data-lucide', iconName)
                
                // 重新創建圖標
                nextTick(() => {
                    setTimeout(() => {
                        lucide.createIcons()
                    }, 0)
                })
            }
        }

        // 更新錯誤訊息
        const updateErrorMsg = (errors) => {
            // 清除之前的錯誤訊息
            document.querySelectorAll('.input-item p').forEach(p => p.textContent = '')

            Object.keys(errors).forEach(field => {
                const errMsg = errors[field]
                if (errMsg && errMsg.length > 0) {
                    const el = document.querySelector(`p[asp-validation-for="${field}"]`)
                    if (el && field) {
                        el.textContent = errMsg[0]
                    } else if (!field) {
                        console.log('General error:', errMsg[0])
                    } else {
                        console.log(`Could not find validation element for ${field}`)
                    }
                }
            })
        }

        // 表單提交
        const submitForm = async (event) => {
            event.preventDefault()

            // 設定提交狀態
            isSubmitting.value = true

            try {
                // 獲取表單數據
                const formData = new FormData(event.target)
                const token = formData.get('__RequestVerificationToken')

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
                })

                const result = await response.json()

                if (result.success && result.redirectUrl) {
                    // 檢查是否有郵件發送標記
                    if (result.emailSent) {
                        // 方案一：伺服器已發送確認信，但我們要在前端再次確認或重新發送
                        showCustomPopup(
                            result.message || '註冊成功！',
                            'success'
                        )

                        // 詢問用戶是否需要重新發送確認信
                        setTimeout(() => {
                            showEmailConfirmationOptions(result)
                        }, 2000)

                    } else if (result.needEmailConfirmation) {
                        // 方案二：需要前端發送確認信
                        await handleEmailConfirmation(result, token)

                    } else if (result.redirectUrl) {
                        // 一般成功情況，成功時保持 loading 狀態直到頁面跳轉
                        if (result.message) {
                            showCustomPopup(result.message, 'success')
                            setTimeout(() => {
                                window.location.href = result.redirectUrl
                            }, 3000)
                        } else {
                            window.location.href = result.redirectUrl
                        }
                    }
                } else {
                    // 失敗時重置提交狀態
                    isSubmitting.value = false
                    if (result.errors) {
                        updateErrorMsg(result.errors)
                    } else {
                        throw new Error(result.message || '註冊失敗')
                    }
                }
            } catch (error) {
                console.error('Register error:', error)
                // 發生錯誤時重置提交狀態
                isSubmitting.value = false
            }
        }

        // 顯示郵件確認選項
        const showEmailConfirmationOptions = (registerResult) => {
            const popup = document.createElement('div')
            popup.className = 'custom-popup custom-popup-info email-options-popup'

            popup.innerHTML = `
                <div class="popup-content">
                    <div class="popup-icon">
                        <i data-lucide="mail" style="color: #3B82F6"></i>
                    </div>
                    <div class="popup-message">
                        <h4>確認信已發送</h4>
                        <p>我們已經將確認信發送到您的郵箱。如果您沒有收到，可以：</p>
                    </div>
                </div>
                <div class="popup-actions">
                    <button class="popup-btn secondary" onclick="checkEmail()">
                        <i data-lucide="external-link"></i>
                        前往登入
                    </button>
                    <button class="popup-btn primary" onclick="resendEmail()">
                        <i data-lucide="refresh-cw"></i>
                        重新發送
                    </button>
                </div>
            `

            // 添加全域函數
            window.checkEmail = () => {
                popup.remove()
                window.location.href = '/login'
            }

            window.resendEmail = async () => {
                popup.remove()

                const token = document.querySelector('input[name="__RequestVerificationToken"]').value
                await handleEmailConfirmation({ needEmailConfirmation: true }, token)
            }

            document.body.appendChild(popup)
            setTimeout(() => lucide.createIcons(), 0)
        }

        // 處理確認信發送（改進版）
        const handleEmailConfirmation = async (registerResult, token) => {
            // 顯示loading popup
            const loadingPopup = showCustomPopup('正在發送確認信...', 'loading')

            try {
                const emailResponse = await fetch('/api/auth/SendConfirmationEmail', {
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
                })

                const emailResult = await emailResponse.json()

                // 關閉 loading popup
                if (loadingPopup && loadingPopup.parentElement) loadingPopup.remove()

                if (emailResult.success) {
                    showCustomPopup(
                        '確認信發送成功！請檢查您的郵箱並點擊確認連結。',
                        'success',
                        () => window.location.href = '/login'
                    )
                } else {
                    showEmailErrorPopup(emailResult.message)
                }

            } catch (emailError) {
                console.error('發送確認信失敗:', emailError)

                // 關閉 loading popup
                if (loadingPopup && loadingPopup.parentElement) loadingPopup.remove()

                showEmailErrorPopup('網路錯誤，請稍後再試。')
            }
        }

        // 顯示郵件發送錯誤 popup
        const showEmailErrorPopup = (errorMessage) => {
            const popup = document.createElement('div')
            popup.className = 'custom-popup custom-popup-warning email-error-popup'

            popup.innerHTML = `
                <div class="popup-content">
                    <div class="popup-icon">
                        <i data-lucide="alert-triangle" style="color: #D97706"></i>
                    </div>
                    <div class="popup-message">
                        <h4>確認信發送失敗</h4>
                        <p>${errorMessage || '發送確認信時發生錯誤，請稍後重試。'}</p>
                    </div>
                </div>
                <div class="popup-actions">
                    <button class="popup-btn secondary" onclick="skipToLogin()">
                        直接登入
                    </button>
                    <button class="popup-btn primary" onclick="retryEmail()">
                        <i data-lucide="refresh-cw"></i>
                        重試
                    </button>
                </div>
            `

            // 添加全域函數
            window.skipToLogin = () => {
                popup.remove()
                window.location.href = '/login'
            }

            window.retryEmail = async () => {
                popup.remove()
                const token = document.querySelector('input[name="__RequestVerificationToken"]').value
                await handleEmailConfirmation({ needEmailConfirmation: true }, token)
            }

            document.body.appendChild(popup)
            setTimeout(() => lucide.createIcons(), 0)
        }

        // 改進的客製化 popup 函數
        const showCustomPopup = (message, type = 'info', callback = null) => {
            const popup = document.createElement('div')
            popup.className = `custom-popup custom-popup-${type}`

            const icons = {
                success: 'check-circle',
                error: 'x-circle',
                warning: 'alert-triangle',
                loading: 'loader',
                info: 'info'
            }

            const colors = {
                success: '#059669',
                error: '#DC2626',
                warning: '#D97706',
                loading: '#3B82F6',
                info: '#6B7280'
            }

            popup.innerHTML = `
                <div class="popup-content">
                    <div class="popup-icon">
                        <i data-lucide="${icons[type]}" style="color: ${colors[type]}"></i>
                    </div>
                    <div class="popup-message">${message}</div>
                    ${type !== 'loading' ? `
                        <button class="popup-close"  onclick="this.parentElement.parentElement.remove()">
                            <i data-lucide="x"></i>
                        </button>
                    ` : ''}
                </div>
            `

            document.body.appendChild(popup)
            setTimeout(() => lucide.createIcons(), 0)

            if (callback) {
                setTimeout(() => {
                    if (popup.parentElement) popup.remove()
                    callback()
                }, 3000)
            } else if (type !== 'loading') {
                setTimeout(() => {
                    if (popup.parentElement)  popup.remove()
                }, 5000)
            }

            return popup // 返回 popup 元素以便後續操作
        }

        // 組件掛載後初始化
        onMounted(() => {
            // 綁定表單數據到 DOM 元素
            const userName = document.querySelector('input[name="UserName"]')
            const email = document.querySelector('input[name="Email"]')
            const pwd = document.querySelector('input[name="Password"]')
            const checkPwd = document.querySelector('input[name="PasswordConfirm"]')

            if (userName) userName.addEventListener('input', e => registerForm.value.UserName = e.target.value)

            if (email) email.addEventListener('input', e => registerForm.value.Email = e.target.value)

            if (pwd) pwd.addEventListener('input', e => registerForm.value.Password = e.target.value)

            if (checkPwd) checkPwd.addEventListener('input', e => registerForm.value.PasswordConfirm = e.target.value)

            // 初始化 Lucide 圖標
            lucide.createIcons()
        })

        return {
            showPassword,
            showConfirmPassword,
            isSubmitting,
            passwordIcon,
            confirmPasswordIcon,
            registerForm,
            togglePasswordVisibility,
            toggleConfirmPasswordVisibility,
            submitForm
        }
    }
}).mount('#auth-body')