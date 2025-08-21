export const useChat = (currentUser) => {
  const { ref, reactive, onMounted, onUnmounted } = Vue

  // 聊天狀態
  const messages = ref([])
  const conversations = ref([])
  const unreadCount = ref(0)
  const currentConversation = ref(null)
  const isConnected = ref(false)
  
  // 聊天 Popup 狀態
  const isChatPopupOpen = ref(false)
  
  // SignalR 連接
  let connection = null
  
  //#region 聊天 Popup 控制

  // 開啟聊天視窗
  const openChatPopup = () => {
  }

  // 關閉聊天視窗
  const closeChatPopup = () => {
  }

  // TODO: 實現切換聊天 Popup 邏輯
  const toggleChatPopup = () => {
  }

  //#endregion

  //#region 聊天方法

  // 發送訊息
  const sendMessage = async (receiverId, content) => {
  }

  // 載入對話記錄
  const loadConversation = async (userId, page = 1, pageSize = 20) => {
  }

  // 載入對話列表
  const loadConversations = async (limit = 10) => {
  }

  // 標記訊息已讀
  const markAsRead = async (messageId) => {
  }

  // 實現標記對話已讀
  const markConversationAsRead = async (senderId) => {
  }

  // 搜尋訊息
  const searchMessages = async (keyword, page = 1, pageSize = 20) => {
  }

  //#region TODO: SignalR 連接管理

  // SignalR 連接
  const startConnection = async () => {
  }

  // SignalR 斷開連接
  const stopConnection = async () => {
  }

  //#endregion

  //#region 生命週期
  
  // 進入網頁時需初始化的功能
  onMounted(() => {
  })

  // 卸載時的清理邏輯
  onUnmounted(() => {
  })

  //#endregion
  
  return {
    // 聊天狀態
    messages,
    conversations,
    unreadCount,
    currentConversation,
    isConnected,
    
    // 聊天 Popup 狀態
    isChatPopupOpen,
    openChatPopup,
    closeChatPopup,
    toggleChatPopup,
    
    // 聊天方法
    sendMessage,
    loadConversation,
    loadConversations,
    markAsRead,
    markConversationAsRead,
    searchMessages,
    
    // SignalR 連接
    startConnection,
    stopConnection
  }
}

export default useChat