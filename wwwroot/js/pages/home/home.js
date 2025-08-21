import { useCreatePost } from '/js/components/create-post.js'

export const useHome = () => {
    const { ref, onMounted, onBeforeUnmount, nextTick } = Vue
    const openCreatePost = ref(false)
    const hotlist = ref([])
    const hotCarouselRef = ref(null)
    const canPrev = ref(false)
    const canNext = ref(false)

    const fetchHotList = async () => {
        try {
            const resp = await fetch('/api/post/hot?count=10', { credentials: 'same-origin' })
            if (!resp.ok) throw new Error('HTTP ' + resp.status)
            const data = await resp.json()
            hotlist.value = Array.isArray(data?.items) ? data.items : []
            await nextTick()
            window.lucide?.createIcons?.()
            updateEdge()
        } catch (e) {
            console.error('Failed to load hot list:', e)
            hotlist.value = []
            updateEdge()
        }
    }

    function getStep(el) {
        const list = el
        const item = list.querySelector('.carousel-item_cts')
        if (!item) return list.clientWidth
        const itemWidth = item.getBoundingClientRect().width
        const csList = getComputedStyle(list)
        const gap = parseFloat(csList.columnGap || csList.gap || '0')
        return itemWidth + gap
    }

    function snapToIndex(el, index) {
        const step = getStep(el)
        el.scrollTo({ left: index * step, behavior: 'smooth' })
    }

    function getIndex(el) {
        const step = getStep(el)
        return step ? Math.round(el.scrollLeft / step) : 0
    }

    function getMaxIndex(el) {
        const step = getStep(el)
        if (!step) return 0
        const maxLeft = Math.max(0, el.scrollWidth - el.clientWidth)
        return Math.max(0, Math.floor(maxLeft / step))
    }

    function updateEdge() {
        const el = hotCarouselRef.value
        if (!el) { canPrev.value = canNext.value = false; return }
        const idx = getIndex(el)
        const max = getMaxIndex(el)
        canPrev.value = idx > 0
        canNext.value = idx < max
    }

    function scrollOne(el, dir = 1) {
        const max = getMaxIndex(el)
        const idx = getIndex(el)
        const target = Math.max(0, Math.min(idx + dir, max))
        snapToIndex(el, target)
        canPrev.value = target > 0
        canNext.value = target < max
    }

    const hotPrev = () => {
        const el = hotCarouselRef.value
        if (!el) return
        scrollOne(el, -1)
    }

    const hotNext = () => {
        const el = hotCarouselRef.value
        if (!el) return
        scrollOne(el, 1)
    }

    const handleScroll = () => updateEdge()
    const handleResize = () => {
        const el = hotCarouselRef.value
        if (!el) return
        const idx = getIndex(el)
        snapToIndex(el, idx)
        updateEdge()
    }

    const toggleCreatePost = () => (openCreatePost.value = !openCreatePost.value)

    onMounted(() => {
        fetchHotList()
        const el = hotCarouselRef.value
        el?.addEventListener('scroll', handleScroll, { passive: true })
        window.addEventListener('resize', handleResize)
    })

    onBeforeUnmount(() => {
        const el = hotCarouselRef.value
        el?.removeEventListener('scroll', handleScroll)
        window.removeEventListener('resize', handleResize)
    })

    const getToken = () => localStorage.getItem('access_token') || ''

    async function stateFunc(action, item) {
        if (!item?.articleId || item._busy) return
        item._busy = true
        try {
            if (action === 'praise') {
                const res = await fetch('/api/PostState/praise', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json', ...(getToken() ? { Authorization: `Bearer ${getToken()}` } : {}) },
                    body: JSON.stringify({ articleId: item.articleId })
                })
                const body = await res.text()
                if (!res.ok) { if (res.status === 401) throw new Error('unauthorized'); throw new Error('praise failed: ' + body) }
                const data = JSON.parse(body || '{}')
                if (!data?.success) throw new Error(data?.message || 'praise failed')
                item.isPraised = !!data.isPraised
                item.praiseCount = Number(data.praiseCount ?? item.praiseCount)
            }

            if (action === 'collect') {
                const res = await fetch('/api/PostState/collect', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json', ...(getToken() ? { Authorization: `Bearer ${getToken()}` } : {}) },
                    body: JSON.stringify({ articleId: item.articleId })
                })
                const body = await res.text()
                if (!res.ok) { if (res.status === 401) throw new Error('unauthorized'); throw new Error('collect failed: ' + body) }
                const data = JSON.parse(body || '{}')
                if (!data?.success) throw new Error(data?.message || 'collect failed')
                item.isCollected = !!data.isCollected
                item.collectCount = Number(data.collectCount ?? item.collectCount)
            }
        } catch (e) {
            console.error(e)
            alert(e.message === 'unauthorized' ? '�Х��n�J' : '�ާ@����')
        } finally {
            item._busy = false
            window.lucide?.createIcons?.()
        }
    }

    const createPost = useCreatePost()

    return {
        ...createPost,
        toggleCreatePost, openCreatePost,
        hotlist, hotCarouselRef, hotPrev, hotNext,
        canPrev, canNext,
        stateFunc
    }
}

export default useHome
