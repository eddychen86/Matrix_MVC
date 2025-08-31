import { useFormatting } from '/js/hooks/useFormatting.js'

// PostList Service - 統一的文章列表服務 (ESM)
export class PostListService {
    constructor() {
        this.baseUrl = '/api/Post';
    }

    async getPosts(page = 1, pageSize = 10, uid = null, isProfilePage = false) {
        try {
            const requestData = { page: page - 1, pageSize };
            let url = this.baseUrl;
            if (uid !== null && uid !== undefined) {
                const params = new URLSearchParams({ uid: uid });
                url = `${this.baseUrl}?${params}`;
            }

            const response = await fetch(url, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                credentials: 'include',
                body: JSON.stringify(requestData),
            });

            if (response.status === 403) {
                let data = null;
                try { data = await response.json(); } catch { }
                return {
                    success: false,
                    requireLogin: true,
                    message: data?.message || '請登入以繼續瀏覽更多內容',
                    articles: [],
                    totalCount: 0
                };
            }

            if (!response.ok) throw new Error(`HTTP error! status: ${response.status}`);

            const data = await response.json();

            return {
                success: true,
                articles: data.articles || [],
                totalCount: data.totalCount || 0
            };
        } catch (error) {
            console.error('Error fetching posts:', error);
            return { success: false, articles: [], totalCount: 0, error: error.message };
        }
    }

    // 這裡補上：作者 PersonId、修正 avatar 拼字、保留相容欄位
    formatArticles(articles) {
        const { formatDate } = useFormatting()
        const lang = (typeof document !== 'undefined' && document.documentElement.lang) ? document.documentElement.lang : 'zh-TW'
        return (articles || []).map(a => {

            // 從後端原始欄位推斷「作者的 PersonId」
            const authorPersonId =
                a.authorPersonId || a.personId || a.author?.personId || a.authorId || null;

            const data = {
                articleId: a.articleId,
                content: a.content,
                createTimeRaw: a.createTime,
                createTime: formatDate(a.createTime, 'datetime', lang),

                praiseCount: a.praiseCount || 0,
                collectCount: a.collectCount || 0,

                authorName: a.authorName || a.author?.name || 'Unknown',

                // 修正拼字，前端用的是 authorAvatar
                authorAvatar: a.authorAvatar || a.author?.avatarPath || null,

                // 最關鍵：把 PersonId 映射到前端物件
                authorPersonId,          // 給新程式用（report 會用這個）
                authorId: authorPersonId, // 相容舊模板（你 CSHTML 用到 item.authorId）

                attachments: (a.attachments || []).map(att => ({
                    fileId: att.fileId,
                    fileName: att.fileName,
                    filePath: att.filePath,
                    type: (att.type || '').toLowerCase(),
                })),

                // 提取主圖片給 Vue 模板使用
                image: (a.attachments || []).find(att => 
                    (att.type || '').toLowerCase() === 'image'
                ),

                isPraised: !!a.isPraised,
                isCollected: !!a.isCollected,
                hashtags: Array.isArray(a.hashtags)
                    ? a.hashtags.map(t => ({
                        tagId: t.tagId || t.TagId,
                        name: t.name || t.Name
                    }))
                    : [],
            }

            return data
        });
    }
}

export const postListService = new PostListService();
export default postListService;
