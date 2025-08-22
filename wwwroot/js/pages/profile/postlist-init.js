/**
 * Profile PostList 初始化
 * 專門處理個人檔案頁面的文章列表
 */

document.addEventListener('DOMContentLoaded', function () {
    // 檢查是否在個人檔案頁面
    if (!document.getElementById('profilePostsList')) {
        return;
    }

    console.log('Initializing Profile PostList...');

    initializeProfilePostList();
});

/**
 * 初始化個人檔案文章列表
 */
const initializeProfilePostList = () => {
    const container = document.getElementById('profilePostsList');
    if (!container) {
        console.error('Profile posts list container not found');
        return;
    }

    // 獲取認證資料
    const authData = window.matrixAuthData || {
        isAuthenticated: false,
        user: { userId: null }
    };

    // 從 URL 或其他方式獲取目標用戶 ID
    // 如果是自己的檔案頁面，使用當前登入用戶的 ID
    const profileUserId = getProfileUserIdFromUrl() || authData.user?.userId;

    // 創建 Vue 應用
    const { createApp } = Vue;

    createApp({
        data() {
            return {
                posts: [],
                isLoading: true,
                hasMorePosts: true,
                currentPage: 0,
                pageSize: 10,
                profileUserId: profileUserId
            };
        },
        async mounted() {
            await this.loadPosts();
            this.setupPostRefreshListener();
        },
        methods: {
            async loadPosts() {
                if (!window.postListService) {
                    console.error('PostListService not available');
                    this.isLoading = false;
                    return;
                }

                this.isLoading = true;

                try {
                    const result = await window.postListService.getPosts(
                        this.currentPage,
                        this.pageSize,
                        this.profileUserId // 傳遞用戶 ID 來篩選該用戶的文章
                    );

                    if (result.success) {
                        const formattedArticles = window.postListService.formatArticles(result.articles);
                        this.posts = formattedArticles;
                        this.hasMorePosts = result.articles.length === this.pageSize;
                    } else {
                        console.error('Failed to load posts:', result.error);
                    }
                } catch (error) {
                    console.error('Error loading posts:', error);
                } finally {
                    this.isLoading = false;
                }
            },
            async loadMorePosts() {
                if (!this.hasMorePosts || this.isLoading) return;

                this.currentPage++;
                this.isLoading = true;

                try {
                    const result = await window.postListService.getPosts(
                        this.currentPage,
                        this.pageSize,
                        this.profileUserId
                    );

                    if (result.success) {
                        const formattedArticles = window.postListService.formatArticles(result.articles);
                        this.posts.push(...formattedArticles);
                        this.hasMorePosts = result.articles.length === this.pageSize;
                    }
                } catch (error) {
                    console.error('Error loading more posts:', error);
                } finally {
                    this.isLoading = false;
                }
            },
            stateFunc(action, articleId) {
                // Handle post actions (praise, comment, collect)
                console.log(`Action: ${action.name}, Article ID: ${articleId}`);

                if (!window.matrixAuthData.isAuthenticated) {
                    alert('請先登入才能進行此操作');
                    return;
                }

                // Call appropriate action
                action(articleId);
            },
            setupPostRefreshListener() {
                window.addEventListener('post:listRefresh', (event) => {
                    const { action, newArticle } = event.detail;

                    if (action === 'prepend' && newArticle) {
                        // 檢查是否是當前用戶的貼文（如果在個人檔案頁面）
                        const currentUserId = window.matrixAuthData?.user?.userId;
                        const isOwnProfile = this.profileUserId === currentUserId;

                        // 只有在自己的檔案頁面才添加新貼文
                        if (isOwnProfile) {
                            this.posts.unshift(newArticle);

                            // 添加視覺提示
                            this.$nextTick(() => {
                                const firstPost = document.querySelector('.post-item:first-child, .article-item:first-child');
                                if (firstPost) {
                                    firstPost.classList.add('new-post-highlight');
                                    setTimeout(() => {
                                        firstPost.classList.remove('new-post-highlight');
                                    }, 3000);
                                }
                            });

                            console.log('新貼文已添加到個人檔案列表頂部');
                        }
                    }
                });
            }
        }
    }).mount(container);

    // Define action functions
    window.praize = (articleId) => {
        console.log('Praise action for article:', articleId);
        // TODO: Implement praise API call
    };

    window.comment = (articleId) => {
        console.log('Comment action for article:', articleId);
        // TODO: Implement comment functionality
    };

    window.collect = (articleId) => {
        console.log('Collect action for article:', articleId);
        // TODO: Implement collect API call
    };
}

/**
 * 從 URL 獲取用戶 ID（如果適用）
 * @returns {string|null} 用戶 ID
 */
const getProfileUserIdFromUrl = () => {
    // 這裡可以實作從 URL 路徑或查詢參數中提取用戶 ID 的邏輯
    // 例如：/profile/123 或 /profile?userId=123
    const pathParts = window.location.pathname.split('/');
    if (pathParts[1] === 'profile' && pathParts[2]) {
        return pathParts[2];
    }

    const urlParams = new URLSearchParams(window.location.search);
    return urlParams.get('userId');
}
