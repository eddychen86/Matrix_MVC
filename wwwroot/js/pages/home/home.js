export const useHome = () => {
    const { ref, onMounted, nextTick } = Vue
    const openCreatePost = ref(false)
    const hotlist = ref([])
    const hotCarouselRef = ref(null)

    const fetchHotList = async () => {
        try {
            const resp = await fetch('/api/post/hot?count=10', { credentials: 'same-origin' })
            if (!resp.ok) throw new Error('HTTP ' + resp.status)
            const data = await resp.json()
            hotlist.value = Array.isArray(data?.items) ? data.items : []
            await nextTick()
            window.lucide?.createIcons?.()
        } catch (e) {
            console.error('Failed to load hot list:', e)
            hotlist.value = []
        }
    }

    const hotPrev = () => {
        const el = hotCarouselRef.value
        if (!el) return
        el.scrollBy({ left: -(el.clientWidth * 0.8), behavior: 'smooth' })
    }

    const hotNext = () => {
        const el = hotCarouselRef.value
        if (!el) return
        el.scrollBy({ left: el.clientWidth * 0.8, behavior: 'smooth' })
    }

    const toggleCreatePost = () => (openCreatePost.value = !openCreatePost.value)

    onMounted(() => { fetchHotList() })

    const getToken = () => localStorage.getItem('access_token') || ''

    async function stateFunc(action, item) {
        if (!item || !item.articleId) return
        if (item._busy) return
        item._busy = true

        try {
            if (action === 'praise') {
                const res = await fetch('/api/PostState/praise', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        ...(getToken() ? { Authorization: `Bearer ${getToken()}` } : {})
                    },
                    body: JSON.stringify({ articleId: item.articleId })
                })
                const body = await res.text()
                if (!res.ok) {
                    console.error('API /praise error', res.status, body)
                    if (res.status === 401) throw new Error('unauthorized')
                    throw new Error('praise failed')
                }
                const data = JSON.parse(body || '{}')
                if (!data?.success) throw new Error(data?.message || 'praise failed')

                //以後端數字為準
                item.isPraised = !!data.isPraised
                item.praiseCount = Number(data.praiseCount ?? item.praiseCount)
            }

            if (action === 'collect') {
                const res = await fetch('/api/PostState/collect', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        ...(getToken() ? { Authorization: `Bearer ${getToken()}` } : {})
                    },
                    body: JSON.stringify({ articleId: item.articleId })
                })
                const body = await res.text()
                if (!res.ok) {
                    console.error('API /collect error', res.status, body)
                    if (res.status === 401) throw new Error('unauthorized')
                    throw new Error('collect failed')
                }
                const data = JSON.parse(body || '{}')
                if (!data?.success) throw new Error(data?.message || 'collect failed')

                //以後端數字為準
                item.isCollected = !!data.isCollected
                item.collectCount = Number(data.collectCount ?? item.collectCount)
            }

            if (action === 'comment') {
                const content = window.prompt('留下你的留言：')
                if (!content || !content.trim()) return
            }
        } catch (e) {
            console.error(e)
            if (e.message === 'unauthorized') {
                alert('請先登入')
            } else {
                alert('操作失敗')
            }
        } finally {
            item._busy = false
            window.lucide?.createIcons?.()
        }
    }


    return {
        toggleCreatePost, openCreatePost,
        hotlist, hotCarouselRef, hotPrev, hotNext,
        stateFunc
    }
}

export default useHome
