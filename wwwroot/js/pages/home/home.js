import { useCreatePost } from '/js/components/create-post.js'
import { usePostActions } from '/js/hooks/usePostActions.js'

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
        // 使用固定的 gap 值 (1.75rem = 28px)
        const gap = parseFloat(csList.gap || '28')
        return Math.round(itemWidth + gap)
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
        // 考慮 padding-right，確保最後一個 item 可以完全顯示
        const paddingRight = parseFloat(getComputedStyle(el).paddingRight || '0')
        const maxLeft = Math.max(0, el.scrollWidth - el.clientWidth + paddingRight)
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

        // 延遲更新按鈕狀態，等待滾動完成
        setTimeout(() => {
            const currentIdx = getIndex(el)
            const currentMax = getMaxIndex(el)
            canPrev.value = currentIdx > 0
            canNext.value = currentIdx < currentMax
        }, 300)
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

    // 使用統一的文章操作 hook
    const postActions = usePostActions()
    
    const stateFunc = async (action, item) => {
        if (!item?.articleId) return
        return await postActions.stateFunc(action, item.articleId, item)
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
