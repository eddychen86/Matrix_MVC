/**
 * User Manager - 處理用戶認證相關功能
 * 從 main.js 中抽離出來的用戶管理功能
 */

export const useUserManager = () => {
    const { reactive } = Vue

    //#region 獲取用戶信息

    // 全局用戶狀態
    const currentUser = reactive({
        isAuthenticated: false,
        userId: null,
        username: '',
        email: '',
        role: 0,
        status: 0,
        isAdmin: false,
        isMember: false
    })

    const getCurrentUser = async () => {
        try {
            const { authService } = await import('/js/services/AuthService.js')
            if (authService) {
                const authStatus = await authService.getAuthStatus()

                if (authStatus.success && authStatus.data.authenticated) {
                    const user = authStatus.data.user

                    Object.assign(currentUser, {
                        isAuthenticated: true,
                        userId: user.id,
                        username: user.username,
                        email: user.email,
                        role: user.role || 0,
                        status: user.status || 0,
                        isAdmin: user.isAdmin || false,
                        isMember: user.isMember || true
                    })
                } else {
                    // 未認證狀態
                    Object.assign(currentUser, {
                        isAuthenticated: false,
                        userId: null
                    })
                }
            } else {
                console.warn('AuthService not available, using direct API call')
                // Fallback 直接 API 呼叫（理論上不會進入）
                const response = await fetch('/api/auth/status')
                const data = await response.json()

                if (data.success && data.data.authenticated) {
                    const user = data.data.user
                    Object.assign(currentUser, {
                        isAuthenticated: true,
                        userId: user.id,
                        username: user.username,
                        email: user.email,
                        role: user.role || 0,
                        status: user.status || 0,
                        isAdmin: user.isAdmin || false,
                        isMember: user.isMember || true
                    })
                } else {
                    // 未認證狀態
                    Object.assign(currentUser, {
                        isAuthenticated: false,
                        userId: null
                    })
                }
            }
        } catch (err) {
            console.error('獲取用戶信息失敗:', err)
            Object.assign(currentUser, {
                isAuthenticated: false,
                userId: null
            })
        }
    }

    // 登出功能
    const logout = async () => {
        try {
            const { authService } = await import('/js/services/AuthService.js')
            if (authService && typeof authService.logout === 'function') {
                await authService.logout()
            } else {
                // Fallback 直接 API 呼叫
                await fetch('/api/auth/logout', {
                    method: 'POST',
                    credentials: 'include'
                })
            }

            // 清空用戶狀態
            Object.assign(currentUser, {
                isAuthenticated: false,
                userId: null,
                username: '',
                email: '',
                role: 0,
                status: 0,
                isAdmin: false,
                isMember: false
            })

            // 刷新頁面或重定向
            window.location.reload()
        } catch (err) {
            console.error('登出失敗:', err)
        }
    }

    // 檢查用戶權限
    const hasPermission = (requiredRole = 0) => {
        return currentUser.isAuthenticated && currentUser.role >= requiredRole
    }

    // 檢查是否為管理員
    const isAdmin = () => {
        return currentUser.isAuthenticated && currentUser.isAdmin
    }

    // 檢查是否為會員
    const isMember = () => {
        return currentUser.isAuthenticated && currentUser.isMember
    }

    // 更新用戶資訊
    const updateUserInfo = (userInfo) => {
        Object.assign(currentUser, userInfo)
    }

    //#endregion

    return {
        // 狀態
        currentUser,

        // 方法
        getCurrentUser,
        logout,
        hasPermission,
        isAdmin,
        isMember,
        updateUserInfo
    }
}

// 單獨導出創建函數
export const createUserManager = useUserManager