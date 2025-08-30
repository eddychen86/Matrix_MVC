// import { useCreatePost } from '/js/components/create-post.js' // 現在全域載入
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
        const item = list.querySelector('li')  // 直接查找第一個 li 元素
        if (!item) return list.clientWidth
        const itemWidth = item.getBoundingClientRect().width
        const csList = getComputedStyle(list)
        // 使用固定的 gap 值，與 Tailwind 的 gap-x-4 對應 (1rem = 16px)
        const gap = 16  // gap-x-4 = 1rem = 16px
        return Math.round(itemWidth + gap)
    }

    function snapToIndex(el, index) {
        const step = getStep(el)
        const maxIndex = getMaxIndex(el)
        
        // 如果是最後一個索引，計算讓最後一個項目居中的滾動位置
        if (index === maxIndex && maxIndex > 0) {
            const items = el.querySelectorAll('li')
            const itemCount = items.length
            
            if (itemCount > 0) {
                const containerWidth = el.clientWidth
                const lastItem = items[itemCount - 1]
                const lastItemRect = lastItem.getBoundingClientRect()
                const containerRect = el.getBoundingClientRect()
                
                // 計算最後一個項目當前相對於容器左側的位置
                const lastItemRelativeLeft = lastItemRect.left - containerRect.left + el.scrollLeft
                const lastItemWidth = lastItemRect.width
                
                // 計算讓最後項目居中的滾動位置
                const centerPosition = lastItemRelativeLeft - (containerWidth - lastItemWidth) / 2
                const maxScrollLeft = Math.max(0, el.scrollWidth - el.clientWidth)
                const targetScrollLeft = Math.min(centerPosition, maxScrollLeft)
                
                el.scrollTo({ left: targetScrollLeft, behavior: 'smooth' })
                return
            }
        }
        
        // 正常情況下的滾動
        el.scrollTo({ left: index * step, behavior: 'smooth' })
    }

    function getIndex(el) {
        const step = getStep(el)
        return step ? Math.round(el.scrollLeft / step) : 0
    }

    function getMaxIndex(el) {
        const step = getStep(el)
        if (!step) return 0
        
        // 使用原始的滾動範圍計算方法
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

    // createPost 現在全域載入，移除本地載入避免重複
    // const createPost = useCreatePost()

    return {
        // ...createPost, // 移除，現在全域可用
        toggleCreatePost, openCreatePost,
        hotlist, hotCarouselRef, hotPrev, hotNext,
        canPrev, canNext,
        stateFunc
    }
}

export default useHome
