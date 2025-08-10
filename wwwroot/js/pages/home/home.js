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
      if (window.lucide && typeof window.lucide.createIcons === 'function') window.lucide.createIcons()
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

  const toggleCreatePost = () => openCreatePost.value = !openCreatePost.value

  onMounted(() => {
    fetchHotList()
  })

  return { toggleCreatePost, openCreatePost, hotlist, hotCarouselRef, hotPrev, hotNext }
}

export default useHome;

