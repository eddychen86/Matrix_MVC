/**
 * usePostActions Hook
 * 統一處理文章互動操作（按讚、收藏、評論、分享等）
 */

export function usePostActions() {

    const getToken = () => localStorage.getItem('access_token') || ''

    /**
     * 檢查用戶是否已登入
     */
    const checkAuth = () => {
        const authData = window.matrixAuthData || window.currentUser
        if (!authData?.isAuthenticated) {
            if (window.loginPopupManager?.open) {
                window.loginPopupManager.open()
            } else {
                alert('請先登入才能進行此操作')
            }
            return false
        }
        return true
    }

    /**
     * 按讚/取消按讚
     */
    const togglePraise = async (articleId, item = null) => {
        if (!checkAuth()) return false

        try {
            const res = await fetch('/api/PostState/praise', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    ...(getToken() ? { Authorization: `Bearer ${getToken()}` } : {})
                },
                credentials: 'include',
                body: JSON.stringify({ articleId })
            })

            const body = await res.text()
            if (!res.ok) {
                if (res.status === 401) throw new Error('unauthorized')
                throw new Error('praise failed: ' + body)
            }

            const data = JSON.parse(body || '{}')
            if (!data?.success) throw new Error(data?.message || 'praise failed')

            // 更新 item 狀態（如果有提供）
            if (item) {
                item.isPraised = !!data.isPraised
                item.praiseCount = Number(data.praiseCount ?? item.praiseCount)
            }

            return {
                success: true,
                isPraised: !!data.isPraised,
                praiseCount: Number(data.praiseCount || 0)
            }
        } catch (error) {
            console.error('togglePraise failed:', error)
            alert(error.message === 'unauthorized' ? '請先登入' : '操作失敗')
            return { success: false, error: error.message }
        }
    }

    /**
     * 收藏/取消收藏
     */
    const toggleCollect = async (articleId, item = null) => {
        if (!checkAuth()) return false

        try {
            const res = await fetch('/api/PostState/collect', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    ...(getToken() ? { Authorization: `Bearer ${getToken()}` } : {})
                },
                credentials: 'include',
                body: JSON.stringify({ articleId })
            })

            const body = await res.text()
            if (!res.ok) {
                if (res.status === 401) throw new Error('unauthorized')
                throw new Error('collect failed: ' + body)
            }

            const data = JSON.parse(body || '{}')
            if (!data?.success) throw new Error(data?.message || 'collect failed')

            // 更新 item 狀態（如果有提供）
            if (item) {
                item.isCollected = !!data.isCollected
                item.collectCount = Number(data.collectCount ?? item.collectCount)
            }

            return {
                success: true,
                isCollected: !!data.isCollected,
                collectCount: Number(data.collectCount || 0)
            }
        } catch (error) {
            console.error('toggleCollect failed:', error)
            alert(error.message === 'unauthorized' ? '請先登入' : '操作失敗')
            return { success: false, error: error.message }
        }
    }

    /**
     * 開啟回覆彈窗
     */
    const openReply = async (articleId) => {
        if (!checkAuth()) return false

        try {
            // 使用全域的 reply modal（如果存在）
            if (window.globalApp?.openReplyWithAuth) {
                const currentUser = window.globalApp.currentUser || window.currentUser
                await window.globalApp.openReplyWithAuth(articleId, currentUser)
            } else if (window.globalApp?.replyModal) {
                await window.globalApp.openReply(articleId)
            } else {
                // 備用方案：導向回覆頁面
                console.log('Opening reply for article:', articleId)
                alert('回覆功能開發中')
            }
            return true
        } catch (error) {
            console.error('openReply failed:', error)
            alert('開啟回覆失敗')
            return false
        }
    }

    /**
     * 分享文章
     */
    const shareArticle = async (articleId, item = null) => {
        try {
            const url = `${window.location.origin}/article/${articleId}`

            if (navigator.share) {
                // 使用 Web Share API（移動裝置）
                await navigator.share({
                    title: item?.title || '分享文章',
                    text: item?.content?.substring(0, 100) || '',
                    url: url
                })
            } else {
                // 備用方案：複製到剪貼板
                await navigator.clipboard.writeText(url)
                alert('文章連結已複製到剪貼板')
            }
            return true
        } catch (error) {
            console.error('shareArticle failed:', error)
            alert('分享失敗')
            return false
        }
    }

    /**
     * 統一的 stateFunc 處理器
     * @param {string} action - 操作類型 ('praise', 'collect', 'comment', 'share')
     * @param {string} articleId - 文章 ID
     * @param {Object} item - 文章項目（用於更新狀態）
     */
    const stateFunc = async (action, articleId, item = null) => {
        console.log(articleId)

        if (!articleId) return false

        // 防重複請求
        if (item?._busy) return false
        if (item) item._busy = true

        try {
            let result = false

            switch (action) {
                case 'praise':
                    result = await togglePraise(articleId, item)
                    break
                case 'collect':
                    result = await toggleCollect(articleId, item)
                    break
                case 'comment':
                    if (window.globalApp?.openReplyWithAuth) {
                        const currentUser = window.globalApp.currentUser || window.currentUser
                        // 傳 seed item，讓彈窗沿用同一參照
                        result = await window.globalApp.openReplyWithAuth(articleId, currentUser, item)
                    } else if (window.globalApp?.openReply) {
                        result = await window.globalApp.openReply(articleId, item)
                    } else {
                        // 備援：直接用 hook 的 openReply
                        result = await openReply(articleId, item)
                    }
                    break
                case 'share':
                    result = await shareArticle(articleId, item)
                    break
                default:
                    console.warn('Unknown action:', action)
                    return false
            }

            return result
        } catch (error) {
            console.error('stateFunc failed:', error)
            return false
        } finally {
            if (item) item._busy = false
            // 重新渲染圖標
            window.lucide?.createIcons?.()
        }
    }

    return {
        stateFunc,
        togglePraise,
        toggleCollect,
        openReply,
        shareArticle,
        checkAuth
    }
}

export default usePostActions