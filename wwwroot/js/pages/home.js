/**
 * 首頁處理
 * 處理首頁的特定功能和互動邏輯
 */
const useHomePage = () => {
    /**
     * 初始化首頁
     */
    const initHomePage = () => {
        const { ready } = useDom();

        // 等待 DOM 加載完成
        ready(() => {
            // 初始化文章載入更多功能
            initInfiniteScroll();

            // 初始化文章互動功能
            initArticleInteractions();

            // 初始化搜尋功能
            initSearchFeature();

            // 初始化 Lucide 圖標
            if (typeof lucide !== 'undefined') {
                lucide.createIcons();
            }
        });
    };

    /**
     * 初始化無限滾動
     */
    const initInfiniteScroll = () => {
        let isLoading = false;
        let currentPage = 1;
        let hasMoreContent = true;

        const loadMoreContent = async () => {
            if (isLoading || !hasMoreContent) return;

            isLoading = true;
            
            try {
                const api = useApi();
                const result = await api.get(`/api/articles?page=${currentPage + 1}`);

                if (result.success && result.data && result.data.length > 0) {
                    appendArticles(result.data);
                    currentPage++;
                } else {
                    hasMoreContent = false;
                }
            } catch (error) {
                console.error('Error loading more content:', error);
            } finally {
                isLoading = false;
            }
        };

        // 監聽滾動事件
        window.addEventListener('scroll', () => {
            const scrollTop = window.pageYOffset || document.documentElement.scrollTop;
            const windowHeight = window.innerHeight;
            const documentHeight = document.documentElement.scrollHeight;

            // 當滾動到底部附近時載入更多內容
            if (scrollTop + windowHeight >= documentHeight - 1000) {
                loadMoreContent();
            }
        });
    };

    /**
     * 添加文章到頁面
     * @param {Array} articles - 文章陣列
     */
    const appendArticles = (articles) => {
        const articlesContainer = document.getElementById('articles-container');
        if (!articlesContainer) return;

        const { createElement } = useDom();

        articles.forEach(article => {
            const articleElement = createElement('div', {
                className: 'article-item',
                'data-article-id': article.id,
                innerHTML: `
                    <div class="article-header">
                        <h3 class="article-title">${article.title}</h3>
                        <span class="article-date">${useFormatting().timeAgo({date: article.createdAt})}</span>
                    </div>
                    <div class="article-content">
                        <p>${article.excerpt}</p>
                    </div>
                    <div class="article-actions">
                        <button class="like-btn" data-article-id="${article.id}">
                            <i data-lucide="heart"></i>
                            <span>${article.likesCount || 0}</span>
                        </button>
                        <button class="comment-btn" data-article-id="${article.id}">
                            <i data-lucide="message-circle"></i>
                            <span>${article.commentsCount || 0}</span>
                        </button>
                        <button class="share-btn" data-article-id="${article.id}">
                            <i data-lucide="share"></i>
                        </button>
                    </div>
                `
            });

            articlesContainer.appendChild(articleElement);
        });

        // 重新創建圖標
        if (typeof lucide !== 'undefined') {
            lucide.createIcons();
        }
    };

    /**
     * 初始化文章互動功能
     */
    const initArticleInteractions = () => {
        const { on } = useDom();

        // 點讚功能
        on(document, 'click', async (e) => {
            if (e.target.closest('.like-btn')) {
                const btn = e.target.closest('.like-btn');
                const articleId = btn.dataset.articleId;
                
                try {
                    const api = useApi();
                    const result = await api.post(`/api/articles/${articleId}/like`);
                    
                    if (result.success) {
                        const countSpan = btn.querySelector('span');
                        countSpan.textContent = result.data.likesCount;
                        btn.classList.toggle('liked');
                    }
                } catch (error) {
                    console.error('Error liking article:', error);
                }
            }
        });

        // 評論功能
        on(document, 'click', (e) => {
            if (e.target.closest('.comment-btn')) {
                const btn = e.target.closest('.comment-btn');
                const articleId = btn.dataset.articleId;
                
                // 跳轉到文章詳情頁面的評論區
                window.location.href = `/articles/${articleId}#comments`;
            }
        });

        // 分享功能
        on(document, 'click', (e) => {
            if (e.target.closest('.share-btn')) {
                const btn = e.target.closest('.share-btn');
                const articleId = btn.dataset.articleId;
                
                shareArticle(articleId);
            }
        });
    };

    /**
     * 分享文章
     * @param {string} articleId - 文章 ID
     */
    const shareArticle = (articleId) => {
        const shareUrl = `${window.location.origin}/articles/${articleId}`;
        
        if (navigator.share) {
            // 使用 Web Share API（如果可用）
            navigator.share({
                title: 'Matrix - 分享文章',
                url: shareUrl
            });
        } else {
            // 複製到剪貼板
            navigator.clipboard.writeText(shareUrl).then(() => {
                // 顯示複製成功訊息
                showToast('連結已複製到剪貼板');
            });
        }
    };

    /**
     * 顯示 Toast 訊息
     * @param {string} message - 訊息內容
     */
    const showToast = (message) => {
        const { createElement } = useDom();

        const toast = createElement('div', {
            className: 'toast',
            innerHTML: message
        });

        document.body.appendChild(toast);

        // 顯示動畫
        setTimeout(() => toast.classList.add('show'), 100);

        // 自動隱藏
        setTimeout(() => {
            toast.classList.remove('show');
            setTimeout(() => toast.remove(), 300);
        }, 3000);
    };

    /**
     * 初始化搜尋功能
     */
    const initSearchFeature = () => {
        const searchInput = document.getElementById('search-input');
        if (!searchInput) return;

        let searchTimeout;

        searchInput.addEventListener('input', (e) => {
            clearTimeout(searchTimeout);
            
            searchTimeout = setTimeout(() => {
                const query = e.target.value.trim();
                if (query.length > 2) {
                    performSearch(query);
                }
            }, 500); // 防止過頻繁的搜尋
        });
    };

    /**
     * 執行搜尋
     * @param {string} query - 搜尋關鍵字
     */
    const performSearch = async (query) => {
        try {
            const api = useApi();
            const result = await api.get(`/api/search?q=${encodeURIComponent(query)}`);

            if (result.success) {
                displaySearchResults(result.data);
            }
        } catch (error) {
            console.error('Search error:', error);
        }
    };

    /**
     * 顯示搜尋結果
     * @param {Array} results - 搜尋結果
     */
    const displaySearchResults = (results) => {
        // 這裡可以實作搜尋結果的顯示邏輯
        console.log('Search results:', results);
    };

    return {
        initHomePage,
        initInfiniteScroll,
        initArticleInteractions,
        shareArticle,
        showToast,
        performSearch
    };
};

// 將首頁處理掛載到全域 - 同時支援命名空間和向後兼容
window.Matrix.pages.useHomePage = useHomePage;
window.useHomePage = useHomePage; // 向後兼容