// Friends Service - 統一的好友列表服務 (ESM)
export class FriendsService {
    constructor() {
        this.baseUrl = '/api/friends';
    }

    /**
     * 取得好友列表
     * @param {number} page - 頁碼 (從 1 開始，預設為 1)
     * @param {number} pageSize - 每頁數量 (預設為 20)
     * @param {string|null} username - 指定使用者名稱；不提供時取當前登入者
     * @returns {Promise<{success:boolean, friends:Array, totalCount:number, unauthorized?:boolean, error?:string}>}
     */
    async getFriends(page = 1, pageSize = 20, username = null, status = 'accepted') {
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

    /**
     * 格式化好友資料
     * @param {Array} items - 原始好友資料（FriendListViewModel[]）
     * @returns {Array<{userId:string, userName:string, avatarPath:string|null}>}
     */
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
