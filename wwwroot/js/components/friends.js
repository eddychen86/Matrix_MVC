// Friends Service - 統一的好友列表服務 (ESM)
export class FriendsService {
    constructor() {
        this.baseUrl = '/api/friends';
    }

    // 取得好友列表
    // page: 第幾頁；pageSize: 每頁幾筆；username: 指定使用者，空的話用目前登入者
    
    async getFriends(page = 1, pageSize = 20, username = null, status = 'accepted') {
        return
        
        try {
            // 新版 API 不再需要分頁；保留參數以維持呼叫端相容，但不帶入
            const params = new URLSearchParams();
            if (status) params.set('status', status);

            const url = username
                ? `${this.baseUrl}/${encodeURIComponent(username)}${params.toString() ? `?${params.toString()}` : ''}`
                : `${this.baseUrl}${params.toString() ? `?${params.toString()}` : ''}`;

            const response = await fetch(url, {
                method: 'GET',
                headers: { 'Content-Type': 'application/json' },
                credentials: 'include'
            });

            if (response.status === 401) {
                // 未登入或未授權
                let data = null;
                try { data = await response.json(); } catch { /* ignore */ }
                return {
                    success: false,
                    unauthorized: true,
                    error: data?.message || '需要登入才能取得好友列表',
                    friends: [],
                    totalCount: 0
                };
            }

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const data = await response.json();
            const items = Array.isArray(data?.items) ? data.items : [];
            const friends = this.formatFriends(items);

            return {
                success: true,
                friends,
                totalCount: Number(data?.totalCount ?? items.length) || 0
            };
        } catch (error) {
            console.error('Error fetching friends:', error);
            return {
                success: false,
                friends: [],
                totalCount: 0,
                error: error?.message || 'Unknown error'
            };
        }
    }

    // 把好友資料整理成前端好用的樣子
    formatFriends(items) {
        return items.map(item => ({
            userId: String(item.userId || ''),
            userName: item.userName || '未知用戶',
            avatarPath: item.avatarPath || null
        }));
    }

    
}

// 導出單例
export const friendsService = new FriendsService();
export default friendsService;
