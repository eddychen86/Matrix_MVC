// PostList Service - 統一的文章列表服務 (ESM)
export class PostListService {
    constructor() {
        this.baseUrl = '/api/Post';
    }

    /**
     * 獲取文章列表
     * @param {number} page - 頁碼 (從 1 開始，預設為 1)
     * @param {number} pageSize - 每頁文章數量 (預設為 10)
     * @param {string|null} uid - 用戶ID (可選，用於個人檔案頁面)
     * @param {boolean} isProfilePage - 是否為個人檔案頁面
     * @returns {Promise} 返回文章數據
     */
    async getPosts(page = 1, pageSize = 10, uid = null, isProfilePage = false) {
        try {
            // 準備 POST body 數據
            const requestData = {
                page: page - 1, // 轉換為 0-based 給後端
                pageSize: pageSize
            };

            // 建構 URL，如果有 uid 則添加查詢參數
            let url = this.baseUrl;
            if (uid !== null && uid !== undefined) {
                const params = new URLSearchParams({ uid: uid });
                url = `${this.baseUrl}?${params}`;
            }

            const response = await fetch(url, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                credentials: 'include',
                body: JSON.stringify(requestData)
            });

            // 特別處理訪客第二次請求的 403，避免在前端噴錯
            if (response.status === 403) {
                let data = null;
                try { data = await response.json(); } catch { /* ignore */ }
                return {
                    success: false,
                    requireLogin: true,
                    message: data?.message || '請登入以繼續瀏覽更多內容',
                    articles: [],
                    totalCount: 0
                };
            }

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const data = await response.json();
            return {
                success: true,
                articles: data.articles || [],
                totalCount: data.totalCount || 0
            };
        } catch (error) {
            // 其它非 403 錯誤再記錄
            console.error('Error fetching posts:', error);
            return {
                success: false,
                articles: [],
                totalCount: 0,
                error: error.message
            };
        }
    }

    /**
     * 格式化文章數據以符合前端需求
     * @param {Array} articles - 原始文章數據
     * @returns {Array} 格式化後的文章數據
     */
    formatArticles(articles) {
        return articles.map(article => ({
            articleId: article.articleId,
            content: article.content,
            createTime: this.formatDate(article.createTime),
            praiseCount: article.praiseCount || 0,
            collectCount: article.collectCount || 0,
            authorName: article.authorName || 'Unknown',
            authorAvator: article.authorAvatar || null,
            attachments: (article.attachments || []).map(att => ({
                fileId: att.fileId,
                fileName: att.fileName,
                filePath: att.filePath,
                type: att.type.toLowerCase()
            })),
            isPraised: !!article.isPraised,
            isCollected: !!article.isCollected
        }));
    }

    /**
     * 格式化日期
     * @param {string} dateString - 日期字符串
     * @returns {string} 格式化後的日期
     */
    formatDate(dateString) {
        if (!dateString) return '';
        const date = new Date(dateString);
        return date.toLocaleString('zh-TW', {
            year: 'numeric',
            month: '2-digit',
            day: '2-digit',
            hour: '2-digit',
            minute: '2-digit'
        });
    }
}
// 導出單例
export const postListService = new PostListService();
export default postListService;
