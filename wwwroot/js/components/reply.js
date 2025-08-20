export function useReply() {
    const { reactive, nextTick } = Vue

    const replyModal = reactive({
        visible: false,
        loading: false,
        submitting: false,
        error: '',
        articleId: null,
        article: null,
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
                '�����@��',
            authorAvatar:
                dto.authorAvatar ??
                dto.AuthorAvatar ??
                dto.author?.avatarPath ??
                dto.Author?.AvatarPath ??
                '/static/img/default_avatar.png',
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

        try {
            const res = await fetch(`/api/post/${articleId}`, { credentials: 'include' })
            if (!res.ok) throw new Error(`HTTP ${res.status}`)
            const json = await res.json()
            if (!json?.success) throw new Error(json?.message || 'Ū���峹����')

            replyModal.article = json.article

            const rawReplies = Array.isArray(json.article?.replies) ? json.article.replies : []
            replyModal.replies = rawReplies.map(normalizeReply).filter(Boolean)

            await nextTick()
            window.lucide?.createIcons?.()
        } catch (err) {
            console.error('[openReply] load article failed:', err)
            replyModal.error = '���J�峹����'
            replyModal.visible = false
        } finally {
            replyModal.loading = false
        }
    }

    function closeReply() {
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
            replyModal.error = '�d�����i�ť�'
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
                replyModal.error = '�Х��n�J�~��d��'
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
            replyModal.error = err.message || '�d������'
        } finally {
            replyModal.submitting = false
        }
    }

    async function openReplyWithAuth(articleId, currentUser) {
        if (!currentUser?.isAuthenticated) {
            window.loginPopupManager?.open?.()
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
