/**
 * 首頁主要 JavaScript
 * 處理首頁核心功能，不包含發文彈窗相關功能
 */

/**
 * 初始化全域認證資料物件
 * 此物件會由伺服器端渲染時設置實際值
 */
window.matrixAuthData = window.matrixAuthData || {};

/**
 * 測試訪客登入彈窗功能
 * 用於開發階段測試訪客控制系統是否正常運作
 */
window.testLoginPopup = function () {
    // 檢查訪客控制物件是否已初始化
    if (window.guestAccessControl) {
        // 強制顯示訪客登入提示彈窗
        window.guestAccessControl.forceShowPopup();
    } else {
        // 如果尚未初始化則在控制台輸出提示訊息
        console.log('訪客控制尚未初始化');
    }
};

/**
 * 文章互動功能處理函數
 * 處理按讚、留言、收藏等操作
 * @param {string} action - 操作類型 ('praise', 'comment', 'collect')
 * @param {string} articleId - 文章 ID
 */
window.stateFunc = function(action, articleId) {
    // 檢查使用者認證狀態
    if (!window.matrixAuthData || !window.matrixAuthData.isAuthenticated) {
        alert('請先登入才能進行此操作');
        return;
    }
    
    // 根據操作類型執行相對應的功能
    switch(action) {
        case 'praise':
            console.log('按讚文章:', articleId);
            // TODO: 實作按讚 API 呼叫
            break;
        case 'comment':
            console.log('評論文章:', articleId);
            // TODO: 實作評論功能
            break;
        case 'collect':
            console.log('收藏文章:', articleId);
            // TODO: 實作收藏 API 呼叫
            break;
        default:
            console.warn('未知的操作類型:', action);
    }
};

/**
 * 首頁主要初始化功能
 * 當 DOM 載入完成後執行首頁相關的初始化工作
 */
document.addEventListener('DOMContentLoaded', function () {
    // console.log('首頁已載入完成');
    // console.log('認證資料:', window.matrixAuthData);
    
    // 設置熱門文章輪播
    setupHotNewsCarousel();
});

/**
 * 設置熱門文章輪播
 */
function setupHotNewsCarousel() {
    const prevBtn = document.getElementById('hotPrev');
    const nextBtn = document.getElementById('hotNext');
    const carousel = document.querySelector('.carousel-center');
    
    if (!carousel || !prevBtn || !nextBtn) {
        console.warn('Hot news carousel elements not found');
        return;
    }

    let currentIndex = 0;
    const items = carousel.querySelectorAll('.carousel-item');
    const totalItems = items.length;
    
    if (totalItems === 0) {
        // console.warn('No carousel items found');
        return;
    }

    function updateCarousel() {
        const itemWidth = items[0].offsetWidth + 16; // item width + gap
        carousel.scrollTo({
            left: currentIndex * itemWidth,
            behavior: 'smooth'
        });
    }

    prevBtn.addEventListener('click', () => {
        currentIndex = (currentIndex - 1 + totalItems) % totalItems;
        updateCarousel();
    });

    nextBtn.addEventListener('click', () => {
        currentIndex = (currentIndex + 1) % totalItems;
        updateCarousel();
    });

    // Auto-play carousel
    setInterval(() => {
        currentIndex = (currentIndex + 1) % totalItems;
        updateCarousel();
    }, 5000); // Change every 5 seconds
}