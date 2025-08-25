/**
 * Matrix SignalR 客戶端管理
 * 處理所有即時更新功能：通知、功能開關、按讚、追蹤、新貼文等
 */

class MatrixSignalR {
  constructor() {
    this.connection = null;
    this.isConnected = false;
    this.reconnectAttempts = 0;
    this.maxReconnectAttempts = 5;
    this.eventHandlers = new Map();
  }

  /**
   * 初始化 SignalR 連接
   */
  async initialize() {
    if (this.connection) {
      //console.log('SignalR 連接已存在');
      return;
    }

    try {
      // 建立連接
      this.connection = new signalR.HubConnectionBuilder()
        .withUrl('/matrixHub')
        .withAutomaticReconnect([0, 2000, 10000, 30000])
        .configureLogging(signalR.LogLevel.Information)
        .build();

      // 設置事件監聽器
      this.setupEventHandlers();

      // 開始連接
      await this.connection.start();
      this.isConnected = true;
      this.reconnectAttempts = 0;

      console.log('✅ MatrixSignalR 連接成功');

      // 觸發連接成功事件
      this.emit('connected');

    } catch (error) {
      console.error('❌ MatrixSignalR 連接失敗:', error);
      this.isConnected = false;
    }
  }

  /**
   * 設置 SignalR 事件監聽器
   */
  setupEventHandlers() {
    if (!this.connection) return;

    // 連接狀態變化
    this.connection.onreconnecting(() => {
      console.log('🔄 SignalR 重新連接中...');
      this.isConnected = false;
      this.emit('reconnecting');
    });

    this.connection.onreconnected(() => {
      console.log('✅ SignalR 重新連接成功');
      this.isConnected = true;
      this.reconnectAttempts = 0;
      this.emit('reconnected');
    });

    this.connection.onclose((error) => {
      console.log('❌ SignalR 連接關閉', error);
      this.isConnected = false;
      this.emit('disconnected', error);
    });

    // 心跳回應
    this.connection.on('Pong', (timestamp) => {
      console.log('💓 SignalR Pong received:', timestamp);
    });

    // 功能開關變更
    this.connection.on('FeatureToggle', (data) => {
      console.log('🔧 功能開關變更:', data);
      this.emit('featureToggle', data);
    });

    // 系統公告
    this.connection.on('SystemAnnouncement', (announcement) => {
      console.log('📢 系統公告:', announcement);
      this.emit('systemAnnouncement', announcement);
    });

    // 互動更新（按讚/收藏）
    this.connection.on('InteractionUpdate', (update) => {
      console.log('👍 互動更新:', update);
      this.emit('interactionUpdate', update);
    });

    // 追蹤更新
    this.connection.on('FollowUpdate', (update) => {
      console.log('👥 追蹤更新:', update);
      this.emit('followUpdate', update);
    });

    // 個人通知
    this.connection.on('PersonalNotification', (notification) => {
      console.log('🔔 個人通知:', notification);
      this.emit('newNotification', notification);
    });

    // 統計更新
    this.connection.on('StatsUpdate', (stats) => {
      console.log('📊 統計更新:', stats);
      this.emit('statsUpdate', stats);
    });

    // 新貼文通知
    this.connection.on('NewPostReceived', (postData) => {
      console.log('📝 收到新貼文通知:', postData);

      // 觸發自定義事件，讓貼文列表組件監聽
      window.dispatchEvent(new CustomEvent('post:listRefresh', {
        detail: {
          action: 'prepend',
          newArticle: postData.formattedArticle,
          rawArticle: postData,
          source: 'signalr' // 標記為 SignalR 事件
        }
      }));

      this.emit('newPost', postData);
    });
  }

  /**
   * 發送心跳
   */
  async ping() {
    if (this.isConnected && this.connection) {
      try {
        await this.connection.invoke('Ping');
      } catch (error) {
        console.error('SignalR ping 失敗:', error);
      }
    }
  }

  /**
   * 通知新貼文（供發文者調用）
   */
  async notifyNewPost(postData) {
    if (this.isConnected && this.connection) {
      try {
        await this.connection.invoke('NotifyNewPost', postData);
        return true;
      } catch (error) {
        console.error('SignalR 新貼文通知失敗:', error);
        return false;
      }
    }
    return false;
  }

  /**
   * 加入特定群組
   */
  async joinGroup(groupName) {
    if (this.isConnected && this.connection) {
      try {
        await this.connection.invoke('JoinGroup', groupName);
        console.log(`✅ 已加入群組: ${groupName}`);
      } catch (error) {
        console.error(`❌ 加入群組失敗: ${groupName}`, error);
      }
    }
  }

  /**
   * 離開特定群組
   */
  async leaveGroup(groupName) {
    if (this.isConnected && this.connection) {
      try {
        await this.connection.invoke('LeaveGroup', groupName);
        console.log(`✅ 已離開群組: ${groupName}`);
      } catch (error) {
        console.error(`❌ 離開群組失敗: ${groupName}`, error);
      }
    }
  }

  /**
   * 註冊事件監聽器
   */
  on(eventName, handler) {
    if (!this.eventHandlers.has(eventName)) {
      this.eventHandlers.set(eventName, []);
    }
    this.eventHandlers.get(eventName).push(handler);
  }

  /**
   * 移除事件監聽器
   */
  off(eventName, handler) {
    if (this.eventHandlers.has(eventName)) {
      const handlers = this.eventHandlers.get(eventName);
      const index = handlers.indexOf(handler);
      if (index > -1) {
        handlers.splice(index, 1);
      }
    }
  }

  /**
   * 觸發事件
   */
  emit(eventName, data) {
    if (this.eventHandlers.has(eventName)) {
      this.eventHandlers.get(eventName).forEach(handler => {
        try {
          handler(data);
        } catch (error) {
          console.error(`事件處理器錯誤 (${eventName}):`, error);
        }
      });
    }
  }

  /**
   * 斷開連接
   */
  async disconnect() {
    if (this.connection) {
      try {
        await this.connection.stop();
        this.connection = null;
        this.isConnected = false;
        console.log('SignalR 連接已斷開');
      } catch (error) {
        console.error('SignalR 斷開連接失敗:', error);
      }
    }
  }

  /**
   * 獲取連接狀態
   */
  getConnectionState() {
    return {
      isConnected: this.isConnected,
      state: this.connection?.state || 'Disconnected'
    };
  }
}

// 創建全局實例
window.matrixSignalR = new MatrixSignalR();

// 自動初始化（僅限已登入用戶）
document.addEventListener('DOMContentLoaded', async () => {
  // 使用 controller 中已取得的用戶資訊來判斷是否要初始化 SignalR
  const authData = window.matrixAuthData || { isAuthenticated: false };

  if (authData.isAuthenticated) {
    try {
      await window.matrixSignalR.initialize();

      // 將連接實例也掛載到 window.signalRConnection 供其他組件使用
      window.signalRConnection = window.matrixSignalR.connection;

      // console.log(`SignalR 已為用戶 ${authData.userName} 初始化`);
    } catch (error) {
      console.warn('SignalR 自動初始化失敗:', error);
    }
  } else {
    console.log('用戶未登入，跳過 SignalR 初始化');
  }
});

// 導出供 ES6 模組使用
if (typeof module !== 'undefined' && module.exports) {
  module.exports = MatrixSignalR;
}