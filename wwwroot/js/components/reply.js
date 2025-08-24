export function useReply() {
    const { reactive, nextTick } = Vue

    const replyModal = reactive({
        visible: false,
        loading: false,
        submitting: false,
        error: '',
        articleId: null,
        article: {
            authorAvatar: '',
            authorName: '',
            content: '',
            attachments: []
        },
        replies: [],
        newComment: ''
    })

    function normalizeReply(dto) {
        if (!dto || typeof dto !== 'object') return null
        return {
            replyId: dto.replyId ?? dto.ReplyId ?? dto.id ?? dto.Id,
            content: dto.content ?? dto.Content ?? '',
            authorName:
                dto.authorName ??
                dto.AuthorName ??
                dto.author?.effectiveDisplayName ??
                dto.Author?.EffectiveDisplayName ??
                '未知作者',
            authorAvatar:
                dto.authorAvatar ??
                dto.AuthorAvatar ??
                dto.author?.avatarPath ??
                dto.Author?.AvatarPath ??
                '',
            createTime:
                dto.createTime ?? dto.CreateTime ?? dto.replyTime ?? dto.ReplyTime ?? null,
            createTimeFormatted:
                dto.createTimeFormatted ?? dto.CreateTimeFormatted ?? null
        }
    }


    async function openReply(articleId) {
        replyModal.visible = true
        replyModal.loading = true
        replyModal.submitting = false
        replyModal.error = ''
        replyModal.articleId = articleId
        replyModal.article = null
        replyModal.replies = []

        // 顯示彈窗元件
        nextTick(() => {
            const replyEl = document.querySelector('div[v-show="replyModal.visible"]')
            if (replyEl) replyEl.style.display = ''
        })

        try {
            const res = await fetch(`/api/post/${articleId}`, { credentials: 'include' })
            if (!res.ok) throw new Error(`HTTP ${res.status}`)
            const json = await res.json()
            // console.log('[DEBUG] article:', json)
            if (!json?.success) throw new Error(json?.message || '讀取文章失敗')

            replyModal.article = json.article

            const rawReplies = Array.isArray(json.article?.replies) ? json.article.replies : []
            replyModal.replies = rawReplies.map(normalizeReply).filter(Boolean)

            await nextTick()
            window.lucide?.createIcons?.()
        } catch (err) {
            console.error('[openReply] load article failed:', err)
            replyModal.error = '載入文章失敗'
            
            // 隱藏彈窗元件
            const replyEl = document.querySelector('div[v-show="replyModal.visible"]')
            if (replyEl) replyEl.style.display = 'none'
            
            replyModal.visible = false
        } finally {
            replyModal.loading = false
        }
    }

    function closeReply() {
        // 隱藏彈窗元件
        const replyEl = document.querySelector('div[v-show="replyModal.visible"]')
        if (replyEl) replyEl.style.display = 'none'
        
        replyModal.visible = false
        replyModal.articleId = null
        replyModal.article = null
        replyModal.replies = []
        replyModal.newComment = ''
        replyModal.error = ''
        replyModal.submitting = false
    }

    async function submitReply() {
        if (replyModal.submitting) return

        const content = (replyModal.newComment || '').trim()
        if (!content) {
            replyModal.error = '留言不可空白'
            return
        }

        replyModal.error = ''
        replyModal.submitting = true
        try {
            const res = await fetch(`/api/post/${replyModal.articleId}/reply`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                credentials: 'include',
                body: JSON.stringify({ content })
            })

            if (res.status === 401) {
                replyModal.error = '請先登入才能留言'
                window.loginPopupManager?.open?.()
                return
            }

            let json
            try {
                json = await res.clone().json()
            } catch (e) {
                const text = await res.text()
                throw new Error('Invalid JSON: ' + text)
            }

            if (!res.ok || !json?.success) {
                throw new Error(json?.message || res.statusText)
            }

            const normalized = normalizeReply(json.data)
            if (normalized) replyModal.replies.unshift(normalized)
            replyModal.newComment = ''

            await nextTick()
            window.lucide?.createIcons?.()
        } catch (err) {
            console.error('[submitReply] failed:', err)
            replyModal.error = err.message || '留言失敗'
        } finally {
            replyModal.submitting = false
        }
    }

    async function openReplyWithAuth(articleId, currentUser) {
        if (!currentUser?.isAuthenticated) {
            window.loginPopupManager?.open?.() || alert('請先登入')
            return
        }
        await openReply(articleId)
    }

    return {
        replyModal,
        openReply,
        closeReply,
        submitReply,
        openReplyWithAuth
    }
}