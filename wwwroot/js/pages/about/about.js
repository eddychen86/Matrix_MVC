export const useAbout = () => {
  const { ref, onMounted } = Vue
  
  const expandedCard = ref(null)

  const toggleCard = (cardElement) => {
    // 如果點擊的是已展開的卡片，則收合
    if (expandedCard.value === cardElement) {
      cardElement.classList.remove('expanded')
      expandedCard.value = null
    } else {
      // 收合之前展開的卡片
      if (expandedCard.value) {
        expandedCard.value.classList.remove('expanded')
      }
      
      // 展開新的卡片
      cardElement.classList.add('expanded')
      expandedCard.value = cardElement
    }
  }

  const initializeTeamCards = () => {
    const teamCards = document.querySelectorAll('.team-card')
    
    teamCards.forEach((card) => {
      card.addEventListener('click', (e) => {
        // 檢查點擊的是否為連結或連結內的元素
        const clickedElement = e.target
        const isLink = clickedElement.tagName === 'A' || clickedElement.closest('a')
        
        // 如果點擊的是連結，不觸發 toggle
        if (isLink) {
          return
        }
        
        e.preventDefault()
        toggleCard(card)
      })
    })
  }

  onMounted(() => {
    initializeTeamCards()
    
    // 檢測預設展開的卡片
    const defaultExpandedCard = document.querySelector('.team-card.expanded')
    if (defaultExpandedCard) {
      expandedCard.value = defaultExpandedCard
    }
    
    // 初始化 Lucide icons
    if (typeof lucide !== 'undefined' && lucide.createIcons) {
      lucide.createIcons()
    }
  })

  return {
    toggleCard,
    expandedCard
  }
}