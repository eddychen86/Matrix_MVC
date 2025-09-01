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

    async function openReply(articleId, seedItem = null) {
        replyModal.visible = true
        replyModal.loading = true
        replyModal.submitting = false
        replyModal.error = ''
        replyModal.articleId = articleId
        replyModal.replies = []

        nextTick(() => {
            const el = document.querySelector('div[v-show="replyModal.visible"]')
            if (el) el.style.display = ''
        })

        // 先放入種子（同參照 → 不回灰）
        if (seedItem) {
            replyModal.article = seedItem
        } else {
            replyModal.article = replyModal.article || {}
            replyModal.article.articleId = articleId
        }

        try {
            const res = await fetch(`/api/post/${articleId}`, { credentials: 'include' })
            if (!res.ok) throw new Error(`HTTP ${res.status}`)
            const json = await res.json()
            if (!json?.success) throw new Error(json?.message || '讀取文章失敗')

            // 合併，不覆蓋（保住同一參照與既有旗標）
            Object.assign(replyModal.article, json.article)
            replyModal.article.isPraised =
                (json.article?.isPraised ?? replyModal.article.isPraised) ?? false
            replyModal.article.isCollected =
                (json.article?.isCollected ?? replyModal.article.isCollected) ?? false
            replyModal.article.praiseCount =
                Number((json.article?.praiseCount ?? replyModal.article.praiseCount) ?? 0)
            replyModal.article.collectCount =
                Number((json.article?.collectCount ?? replyModal.article.collectCount) ?? 0)

            const rawReplies = Array.isArray(json.article?.replies) ? json.article.replies : []
            replyModal.replies = rawReplies.map(normalizeReply).filter(Boolean)

            await nextTick()
            window.lucide?.createIcons?.()
        } catch (err) {
            console.error('[openReply] failed:', err)
            replyModal.error = '載入文章失敗'
            const el = document.querySelector('div[v-show="replyModal.visible"]')
            if (el) el.style.display = 'none'
            replyModal.visible = false
        } finally {
            replyModal.loading = false
        }
    }

    function closeReply() {
        const el = document.querySelector('div[v-show="replyModal.visible"]')
        if (el) el.style.display = 'none'
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
        if (!content) { replyModal.error = '留言不可空白'; return }

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
                window.loginPopupManager?.open?.() || alert('請先登入')
                return
            }

            let json
            try { json = await res.clone().json() }
            catch (e) { throw new Error('Invalid JSON: ' + (await res.text())) }

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

    // 自動解析 ref/物件/全域使用者；呼叫端可不傳 currentUser
    async function openReplyWithAuth(articleId, currentUser, seedItem = null) {
        const user =
            (currentUser && typeof currentUser === 'object' && 'value' in currentUser)
                ? currentUser.value
                : (currentUser ?? (window.currentUser && typeof window.currentUser === 'object' && 'value' in window.currentUser
                    ? window.currentUser.value
                    : window.currentUser) ?? null)

        //console.log('[openReplyWithAuth] articleId=', articleId, 'user=', user)
        if (!user?.isAuthenticated) {
            window.loginPopupManager?.open?.() || alert('請先登入')
            return
        }
        //console.log('[openReplyWithAuth] pass auth, seedItem=', seedItem)
        await openReply(articleId, seedItem)
    }

    return { replyModal, openReply, closeReply, submitReply, openReplyWithAuth }
}

export default useReply