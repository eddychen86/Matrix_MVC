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
  const newMessage = ref('')
  
  // SignalR 連接
  let connection = null
  
  //#region 聊天 Popup 控制

  // 開啟聊天視窗
  const openChatPopup = async (receiver) => {
    if (!receiver || !receiver.userId) {
        console.error("Receiver information is missing.");
        return;
    }
    console.log(`Opening chat with:`, receiver);
    currentConversation.value = receiver;
    await loadConversation(receiver.userId);
    isChatPopupOpen.value = true;
  }

  // 關閉聊天視窗
  const closeChatPopup = () => {
    isChatPopupOpen.value = false;
    currentConversation.value = null;
    messages.value = []; // 清空訊息
  }

  // TODO: 實現切換聊天 Popup 邏輯
  const toggleChatPopup = () => {
  }

  //#endregion

  //#region 聊天方法

  // 發送訊息
  const sendMessage = async (receiverId, content) => {
    try {
      const response = await fetch('/api/chat/send', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json'
        },
        credentials: 'include',
        body: JSON.stringify({
          receiverId: receiverId,
          content: content
        })
      });
      
      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }
      
      const result = await response.json();
      
      // 將新消息添加到消息列表
      messages.value.push(result);
      
      console.log('Message sent successfully:', result);
      return result;
    } catch (error) {
      console.error('Failed to send message:', error);
      throw error;
    }
  }

  // 載入對話記錄
  const loadConversation = async (userId, page = 1, pageSize = 50) => {
    try {
        console.log(`Loading conversation with user ${userId}...`);
        const response = await fetch(`/api/chat/history/${userId}?page=${page}&pageSize=${pageSize}`, {
          credentials: 'include'
        });
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }
        const data = await response.json();
        
        // 確保 data是一個數組
        if (Array.isArray(data)) {
          messages.value = data.sort((a, b) => new Date(a.createTime) - new Date(b.createTime)); // 按時間排序
        } else {
          console.warn('Unexpected data format:', data);
          messages.value = [];
        }
        
        console.log('Conversation loaded:', messages.value);
        
        // 加載完成後自動滚動到底部
        Vue.nextTick(() => {
          scrollToBottom();
        });
    } catch (error) {
        console.error('Failed to load conversation:', error);
        messages.value = []; // 發生錯誤時清空
    }
  }

  //前端按鈕
    document.addEventListener('click', function (event) {
        const openBtn = event.target.closest('#openChatBtn');

        if (openBtn) {
            // Access the profile data from the global Vue app instance
            const profileData = window.globalApp.profile; // Assuming 'profile' is exposed from useProfile
            if (profileData && profileData.userId) {
                // Call the globally exposed openChatPopupGlobal function
                window.openChatPopupGlobal({
                    userId: profileData.userId,
                    displayName: profileData.displayName // Pass other relevant data if needed
                });

                // Initialize drag functionality for the chat popup
                Vue.nextTick(() => {
                    const chatPopup = document.getElementById('chat-popup');
                    if (chatPopup && !chatPopup.dataset.isDraggable) {
                        makeDraggable(chatPopup);
                        chatPopup.dataset.isDraggable = 'true';
                    }
                });
            } else {
                console.error("Profile data or userId not available for chat.");
            }
        }
    });

    function makeDraggable(element) {
        const header = element.querySelector('#chat-header');
        if (!header) return;

        let isDragging = false;
        let offsetX, offsetY;

        header.addEventListener('mousedown', (e) => {
            isDragging = true;

            const rect = element.getBoundingClientRect();
            offsetX = e.clientX - rect.left;
            offsetY = e.clientY - rect.top;

            // Ensure position is fixed for proper viewport-relative dragging
            element.style.position = 'fixed';

            document.addEventListener('mousemove', onMouseMove);
            document.addEventListener('mouseup', onMouseUp);
        });

        function onMouseMove(e) {
            if (!isDragging) return;

            // Set new position based on mouse position minus the initial offset
            element.style.left = (e.clientX - offsetX) + 'px';
            element.style.top = (e.clientY - offsetY) + 'px';
        }

        function onMouseUp() {
            isDragging = false;
            document.removeEventListener('mousemove', onMouseMove);
            document.removeEventListener('mouseup', onMouseUp);
        }
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

  //#region 輔助功能

  // 格式化消息時間
  const formatMessageTime = (dateTime) => {
    if (!dateTime) return ''
    
    const messageDate = new Date(dateTime)
    const now = new Date()
    const diffInSeconds = Math.floor((now - messageDate) / 1000)
    
    if (diffInSeconds < 60) {
      return '剛剛'
    } else if (diffInSeconds < 3600) {
      return `${Math.floor(diffInSeconds / 60)}分鐘前`
    } else if (diffInSeconds < 86400) {
      return `${Math.floor(diffInSeconds / 3600)}小時前`
    } else {
      return messageDate.toLocaleDateString('zh-TW', {
        month: 'short',
        day: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
      })
    }
  }

  // 滚動到聊天窗口底部
  const scrollToBottom = () => {
    Vue.nextTick(() => {
      const messagesContainer = document.getElementById('chat-messages')
      if (messagesContainer) {
        messagesContainer.scrollTop = messagesContainer.scrollHeight
      }
    })
  }

  // 處理發送消息
  const handleSendMessage = async () => {
    if (!newMessage.value.trim() || !currentConversation.value?.userId) {
      return
    }

    const messageContent = newMessage.value.trim()
    newMessage.value = '' // 清空輸入框

    try {
      await sendMessage(currentConversation.value.userId, messageContent)
      // 消息發送成功后，滚動到最新消息
      scrollToBottom()
    } catch (error) {
      console.error('Failed to send message:', error)
      // 可以在這裡顯示錯誤提示
      alert('發送消息失敗，請稍後再試')
      // 還原輸入內容
      newMessage.value = messageContent
    }
  }

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
    newMessage,
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
    handleSendMessage,
    formatMessageTime,
    
    // SignalR 連接
    startConnection,
    stopConnection
  }
}

export default useChat