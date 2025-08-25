/**
 * usePostActions Hook
 * 統一處理文章互動操作（按讚、收藏、評論、分享、檢舉等）
 */
import { authService } from '/js/services/AuthService.js'

export function usePostActions() {
    const getToken = () => localStorage.getItem('access_token') || ''

    // 以 authService 為準，避免抓不到 currentUser 而誤判
    const checkAuth = async () => {
        try {
            const r = await authService.getAuthStatus()
            const authed = !!(r && r.success && r.data && r.data.authenticated)
            if (!authed) {
                if (window.loginPopupManager?.open) window.loginPopupManager.open()
                else alert('請先登入才能進行此操作')
                return false
            }
            return true
        } catch (e) {
            console.error('auth check failed:', e)
            if (window.loginPopupManager?.open) window.loginPopupManager.open()
            else alert('請先登入才能進行此操作')
            return false
        }
    }

    /** Guid 判斷（最後的保險用在 userId 上，若後端其實塞的是 PersonId 也能吃到） */
    const looksLikeGuid = (v) => typeof v === 'string' && /^[0-9a-fA-F-]{36}$/.test(v)

    /** 專門把「作者的 PersonId」解出來；優先只用明確的 personId 欄位，最後才容忍 userId(看起來是 Guid 時) */
    const resolveAuthorPersonId = (row, provided) => {
        if (provided) return provided
        if (!row) return null
        return (
            row.authorPersonId ||
            row.personId ||
            row.authorId ||                 // 請確認這是 PersonId（建議在 formatArticles 直接對齊）
            row.profile?.personId ||
            row.author?.personId ||
            (looksLikeGuid(row.userPersonId) ? row.userPersonId : null) ||
            // ⚠️ 最後保險：若確定 userId 本來就存的是 PersonId，這行就能救回；否則不會吃到
            (looksLikeGuid(row.userId) ? row.userId : null) ||
            null
        )
    }

    /** 按讚 */
    const togglePraise = async (articleId, item = null) => {
        if (!(await checkAuth())) return false
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

            if (item) {
                item.isPraised = !!data.isPraised
                item.praiseCount = Number(data.praiseCount ?? item.praiseCount)
            }
            return { success: true, isPraised: !!data.isPraised, praiseCount: Number(data.praiseCount || 0) }
        } catch (error) {
            console.error('togglePraise failed:', error)
            alert(error.message === 'unauthorized' ? '請先登入' : '操作失敗')
            return { success: false, error: error.message }
        }
    }

    /** 收藏 */
    const toggleCollect = async (articleId, item = null) => {
        if (!(await checkAuth())) return false
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

            if (item) {
                item.isCollected = !!data.isCollected
                item.collectCount = Number(data.collectCount ?? item.collectCount)
            }
            return { success: true, isCollected: !!data.isCollected, collectCount: Number(data.collectCount || 0) }
        } catch (error) {
            console.error('toggleCollect failed:', error)
            alert(error.message === 'unauthorized' ? '請先登入' : '操作失敗')
            return { success: false, error: error.message }
        }
    }

    /** 回覆 */
    const openReply = async (articleId) => {
        if (!(await checkAuth())) return false
        try {
            if (window.globalApp?.openReplyWithAuth) {
                const currentUser = window.globalApp.currentUser || window.currentUser
                await window.globalApp.openReplyWithAuth(articleId, currentUser)
            } else if (window.globalApp?.replyModal) {
                await window.globalApp.openReply(articleId)
            } else {
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

    /** 分享 */
    const shareArticle = async (articleId, item = null) => {
        try {
            const url = `${window.location.origin}/article/${articleId}`
            if (navigator.share) {
                await navigator.share({
                    title: item?.title || '分享文章',
                    text: item?.content?.substring(0, 100) || '',
                    url
                })
            } else {
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

    /** 檢舉 */
    const openReport = async (articleId, authorPersonId, row = null) => {
        if (!(await checkAuth())) return false

        const resolvedAuthorId = resolveAuthorPersonId(row, authorPersonId)

        const typeInput = window.prompt('請選擇：0=檢舉使用者、1=檢舉文章', '1')
        if (typeInput === null) return false
        const type = Number(typeInput)
        if (![0, 1].includes(type)) { alert('檢舉類型不正確'); return false }

        if (type === 0 && !resolvedAuthorId) {
            console.debug('[report] row for debug =', row)
            alert('找不到文章作者 Id（PersonId），無法送出「檢舉使用者」。')
            return false
        }

        const payload = {
            type,
            targetId: type === 0 ? resolvedAuthorId : articleId,
            reportedUserId: type === 1 ? resolvedAuthorId : null,
            reason: (window.prompt('請輸入檢舉理由（1~500字）', '') || '').trim()
        }
        if (!payload.reason || payload.reason.length > 500) { alert('請輸入 1~500 字'); return false }

        try {
            const res = await fetch('/api/reports', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    ...(getToken() ? { Authorization: `Bearer ${getToken()}` } : {})
                },
                credentials: 'include',
                body: JSON.stringify(payload)
            })
            const text = await res.text()
            if (!res.ok) {
                try { const j = JSON.parse(text); alert(j.message || '檢舉失敗') } catch { alert('檢舉失敗') }
                return false
            }
            const data = JSON.parse(text || '{}')
            alert(data.message || '已收到你的檢舉，感謝回報！')
            return true
        } catch (e) {
            console.error('openReport failed:', e)
            alert('檢舉失敗')
            return false
        }
    }

    /** 統一的 stateFunc（自動把第二個參數當 payload 或 articleId；也會把 row（item）帶進去） */
    const stateFunc = async (action, articleIdOrPayload, item = null) => {
        // 兼容兩種呼叫：stateFunc('x', item) 或 stateFunc('x', {articleId, authorId}, item)
        let articleId = articleIdOrPayload
        let row = item
        if (typeof articleIdOrPayload === 'object') {
            articleId = articleIdOrPayload.articleId ?? item?.articleId
            row = row || articleIdOrPayload // 如果第三參數沒傳，就把第二參數當 row
        }

        if (!['share', 'report'].includes(action) && !articleId) return false
        if (row?._busy) return false
        if (row) row._busy = true

        try {
            switch (action) {
                case 'praise': return await togglePraise(articleId, row)
                case 'collect': return await toggleCollect(articleId, row)
                case 'comment': return await openReply(articleId)
                case 'share': return await shareArticle(articleId, row)
                case 'report': {
                    // 從 row (已處理過的 item) 或 articleIdOrPayload 中解構所需資料
                    let reportArticleId = articleId
                    let authorId = null
                    
                    // 如果 row 存在且包含必要資訊，優先使用 row
                    if (row && row.articleId) {
                        reportArticleId = row.articleId
                        authorId = row.authorId || row.authorName // 嘗試獲取 authorId 或 authorName
                    }
                    // 如果 articleIdOrPayload 是物件且包含 authorId，則使用它
                    else if (typeof articleIdOrPayload === 'object' && articleIdOrPayload.authorId) {
                        reportArticleId = articleIdOrPayload.articleId
                        authorId = articleIdOrPayload.authorId
                    }
                    
                    if (!reportArticleId) {
                        console.error('report action requires articleId')
                        return false
                    }
                    if (!authorId) {
                        console.error('report action requires authorId or authorName')
                        return false
                    }
                    
                    return await openReport(reportArticleId, authorId, row)
                }
                default: return false
            }
        } finally {
            if (row) row._busy = false
            window.lucide?.createIcons?.()
        }
    }

    return {
        stateFunc,
        openReport,
        togglePraise,
        toggleCollect,
        openReply,
        shareArticle,
        checkAuth
    }
}

export default usePostActions
