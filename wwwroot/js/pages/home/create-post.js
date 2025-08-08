/**
 * 新增文章彈窗 JavaScript
 * 處理新增文章彈窗所有功能，包含檔案上傳、標籤選擇和文章提交
 */

document.addEventListener('DOMContentLoaded', function () {
    // ========== 資料儲存變數 ==========
    
    /** 儲存從後端載入的所有可選標籤 */
    let allHashtags = [];
    
    /** 使用者目前選中的標籤列表 */
    let selectedHashtags = [];
    
    /** 使用者選擇的圖片檔案列表 */
    let selectedImages = [];
    
    /** 使用者選擇的非圖片檔案列表 */
    let selectedFiles = [];

    // ========== DOM 元素獲取 ==========
    
    /** 開啟發文彈窗的按鈕 */
    const openBtn = document.getElementById('openPostBtn');
    
    /** 關閉發文彈窗的按鈕 */
    const closeBtn = document.getElementById('closePostBtn');
    
    /** 彈窗背景遮罩 */
    const overlay = document.getElementById('overlay');
    
    /** 發文彈窗主體容器 */
    const postModel = document.getElementById('postModel');
    
    /** 標籤選擇彈窗容器 */
    const hashtagModal = document.getElementById('hashtagModal');
    
    /** 標籤選擇清單容器 */
    const hashtagList = document.getElementById('hashtagList');
    
    /** 開啟標籤選擇彈窗的按鈕 */
    const openHashtagBtn = document.getElementById('openHashtagBtn');
    
    /** 關閉標籤選擇彈窗的按鈕 */
    const closeHashtagModalBtn = document.getElementById('closeHashtagModalBtn');
    
    /** 確認選擇標籤的按鈕 */
    const confirmHashtagBtn = document.getElementById('confirmHashtagBtn');
    
    /** 圖片預覽區域 */
    const imagePreview = document.getElementById('imagePreviewArea');
    
    /** 檔案預覽區域 */
    const filePreview = document.getElementById('filePreviewArea');
    
    /** 隱藏的檔案選擇輸入框 */
    const fileInput = document.getElementById('fileInput');
    
    /** 上傳檔案按鈕 */
    const uploadFileBtn = document.getElementById('uploadFileBtn');
    
    /** 上傳圖片按鈕 */
    const uploadImgBtn = document.getElementById('uploadImgBtn');

    // ========== 核心功能函數 ==========
    
    /**
     * 重置發文彈窗狀態
     * 清空所有輸入內容、檔案預覽、已選標籤等，回到初始狀態
     */
    function resetPostModal() {
        // 清空文章內容輸入框
        document.querySelector('#postModel textarea').value = '';
        
        // 清空圖片預覽區域
        imagePreview.innerHTML = '';
        
        // 清空檔案預覽區域
        filePreview.innerHTML = '';
        
        // 重置檔案選擇器
        fileInput.value = '';
        
        // 清空所有選中的檔案和標籤
        selectedImages = [];
        selectedFiles = [];
        selectedHashtags = [];
        
        // 重新渲染已選標籤顯示區（會顯示為空）
        renderSelectedHashtags();
    }

    // ========== 彈窗開關事件處理 ==========
    
    /**
     * 開啟發文彈窗事件處理
     * 檢查按鈕是否存在後才綁定事件，避免頁面中沒有此按鈕時出錯
     */
    if (openBtn) {
        openBtn.addEventListener('click', () => {
            // 移除隱藏樣式，顯示遮罩
            overlay.classList.remove('hidden');
            
            // 移除隱藏樣式，顯示發文彈窗
            postModel.classList.remove('hidden');
            
            // 重置彈窗內容到初始狀態
            resetPostModal();
        });
    }

    /**
     * 關閉發文彈窗事件處理
     * 點擊關閉按鈕時觸發
     */
    if (closeBtn) {
        closeBtn.addEventListener('click', () => {
            // 隱藏遮罩
            overlay.classList.add('hidden');
            
            // 隱藏發文彈窗
            postModel.classList.add('hidden');
            
            // 重置彈窗內容
            resetPostModal();
        });
    }

    /**
     * 點擊遮罩關閉彈窗事件處理
     * 提供更好的使用者體驗，點擊外部區域也能關閉彈窗
     */
    if (overlay) {
        overlay.addEventListener('click', () => {
            // 隱藏遮罩
            overlay.classList.add('hidden');
            
            // 隱藏發文彈窗
            postModel.classList.add('hidden');
            
            // 重置彈窗內容
            resetPostModal();
        });
    }

    // ========== 檔案上傳功能 ==========
    
    /**
     * 圖片上傳按鈕點擊事件處理
     * 限制只能選擇圖片格式檔案
     */
    if (uploadImgBtn && fileInput) {
        uploadImgBtn.addEventListener('click', () => {
            // 重置檔案選擇器，確保能重複選擇相同檔案
            fileInput.value = '';
            
            // 設定只接受圖片格式
            fileInput.accept = 'image/*';
            
            // 觸發檔案選擇對話框
            fileInput.click();
        });
    }

    /**
     * 一般檔案上傳按鈕點擊事件處理
     * 限制只能選擇指定格式的檔案（文件、壓縮檔等）
     */
    if (uploadFileBtn && fileInput) {
        uploadFileBtn.addEventListener('click', () => {
            // 重置檔案選擇器
            fileInput.value = '';
            
            // 設定接受的檔案格式（PDF、Word、Excel、PowerPoint、文字檔、壓縮檔）
            fileInput.accept = '.pdf,.docx,.doc,.ppt,.pptx,.xls,.xlsx,.txt,.zip,.rar';
            
            // 觸發檔案選擇對話框
            fileInput.click();
        });
    }

    /**
     * 檔案選擇變更事件處理
     * 根據當前的檔案類型設定，將選中的檔案分類到圖片或檔案陣列中
     */
    if (fileInput) {
        fileInput.addEventListener('change', () => {
        // 根據 accept 屬性判斷是圖片模式還是檔案模式
        if (fileInput.accept === 'image/*') {
            // 圖片模式：篩選出圖片類型的檔案
            const newImages = Array.from(fileInput.files).filter(f => f.type.startsWith('image/'));
            
            // 合併新選擇的圖片到現有陣列，並去除重複檔案
            // 判重條件：檔案名稱和檔案大小都相同
            selectedImages = selectedImages.concat(newImages)
                .filter((file, idx, arr) => arr.findIndex(f => f.name === file.name && f.size === file.size) === idx);
        } else {
            // 檔案模式：篩選出非圖片類型的檔案
            const newFiles = Array.from(fileInput.files).filter(f => !f.type.startsWith('image/'));
            
            // 合併新選擇的檔案到現有陣列，並去除重複檔案
            selectedFiles = selectedFiles.concat(newFiles)
                .filter((file, idx, arr) => arr.findIndex(f => f.name === file.name && f.size === file.size) === idx);
        }
        
        // 更新檔案預覽顯示
        renderPreviews();
        });
    }

    /**
     * 縮短檔案名稱顯示
     * 避免過長的檔案名稱影響介面美觀
     * @param {string} name - 原始檔案名稱
     * @returns {string} - 處理後的檔案名稱
     */
    function truncateFilename(name) {
        // 使用正規表達式檢查是否包含中文字元
        const hasChinese = /[^\x00-\x7F]/.test(name);
        
        if (hasChinese) {
            // 中文檔名：超過 5 個字元就截斷並加省略號
            return name.length > 5 ? name.slice(0, 5) + '…' : name;
        } else {
            // 英文檔名：超過 10 個字元就截斷並加省略號
            return name.length > 10 ? name.slice(0, 10) + '…' : name;
        }
    }

    // ========== 檔案預覽功能 ==========
    
    /**
     * 渲染圖片與檔案的預覽區域
     * 動態產生預覽元素，讓使用者可以視覺化確認已選擇的檔案
     */
    function renderPreviews() {
        // ========== 圖片預覽區域處理 ==========
        
        // 清空現有的圖片預覽內容
        if (imagePreview) {
            imagePreview.innerHTML = '';
            
            // 為每個選中的圖片建立預覽元素
            selectedImages.forEach(file => {
            // 建立圖片預覽容器
            const div = document.createElement('div');
            div.className = 'flex h-[100px] w-[100px] items-center justify-center overflow-hidden rounded-[30px] bg-cover bg-center bg-no-repeat';
            
            // 建立圖片元素
            const img = document.createElement('img');
            
            // 使用 URL.createObjectURL 建立本地圖片預覽連結
            img.src = URL.createObjectURL(file);
            img.className = 'h-full w-full object-cover';
            
            // 將圖片加入容器
            div.appendChild(img);
            
            // 將預覽容器加入預覽區域
            imagePreview.appendChild(div);
            });
        }

        // ========== 一般檔案預覽區域處理 ==========
        
        // 清空現有的檔案預覽內容
        if (filePreview) {
            filePreview.innerHTML = '';
        
        // 為每個選中的檔案建立預覽元素
        selectedFiles.forEach(file => {
            // 建立檔案預覽容器
            const container = document.createElement('div');
            container.className = "flex flex-col items-center gap-1 rounded-[20px] p-1";
            container.style.width = '60px';

            // 建立檔案圖示區域
            const iconDiv = document.createElement('div');
            iconDiv.className = "flex h-[40px] w-[60px] items-center justify-center rounded-[20px] bg-[#ffffff]";
            
            // 使用統一的檔案圖示 SVG
            iconDiv.innerHTML = `
                <svg xmlns="http://www.w3.org/2000/svg" class="h-6 w-6" style="fill: rgb(51, 51, 51);" viewBox="0 0 24 24">
                    <path fill="none" d="M0 0h24v24H0z"></path>
                    <path d="m20.41 8.41-4.83-4.83c-.37-.37-.88-.58-1.41-.58H5c-1.1 0-2 .9-2 2v14c0 1.1.9 2 2 2h14c1.1 0 2-.9 2-2V9.83c0-.53-.21-1.04-.59-1.42zM7 7h7v2H7V7zm10 10H7v-2h10v2zm0-4H7v-2h10v2z"></path>
                </svg>
            `;

            // 建立檔案名稱顯示元素
            const filenameSpan = document.createElement('span');
            
            // 使用縮短函數處理檔案名稱
            filenameSpan.textContent = truncateFilename(file.name);
            
            // 設定檔案名稱樣式
            filenameSpan.style.fontSize = '11px';
            filenameSpan.style.textAlign = 'center';
            filenameSpan.style.wordBreak = 'break-word';
            filenameSpan.style.maxWidth = '60px';

            // 將圖示和檔案名稱加入容器
            container.appendChild(iconDiv);
            container.appendChild(filenameSpan);
            
            // 將檔案預覽容器加入預覽區域
            filePreview.appendChild(container);
            });
        }
    }

    // ========== 標籤功能 ==========
    
    /**
     * 從後端 API 非同步獲取所有可用標籤
     * 使用快取機制，避免重複呼叫 API
     */
    async function fetchHashtags() {
        // 如果已經載入過標籤，直接返回，避免重複請求
        if (allHashtags.length > 0) return;
        
        try {
            // 呼叫後端 API 獲取所有標籤
            const res = await fetch('/Post/GetHashtags');
            
            // 將回應解析為 JSON 格式並儲存到全域變數
            allHashtags = await res.json();
        } catch (error) {
            // API 請求失敗時的錯誤處理
            console.error('載入標籤失敗:', error);
        }
    }

    /**
     * 渲染標籤選擇彈窗內的標籤清單
     * 顯示所有可用標籤，並標記已選中的標籤
     */
    function renderHashtagModal() {
        // 清空現有的標籤清單
        if (hashtagList) {
            hashtagList.innerHTML = '';
            
            // 為每個標籤建立選擇項目
            allHashtags.forEach(tag => {
            // 檢查該標籤是否已被使用者選中
            const isChecked = selectedHashtags.some(t => t && t.tagId === tag.tagId);
            
            // 建立標籤選擇項目的容器（label 元素）
            const label = document.createElement('label');
            label.className = 'flex items-center gap-2 px-2 py-2 rounded-xl bg-[#232323] hover:bg-gray-800 cursor-pointer min-h-[36px]';
            
            // 設定標籤選擇項目的內容（核取方塊 + 標籤文字）
            label.innerHTML = `
                <input type="checkbox" value="${tag.tagId}" ${isChecked ? 'checked' : ''} class="flex-shrink-0 accent-[#ffda78]">
                <span class="block max-w-[100px] truncate text-sm">${tag.content}</span>
            `;
            
            // 將標籤選擇項目加入清單
            hashtagList.appendChild(label);
            });
        }
    }

    /**
     * 渲染已選標籤的顯示區域
     * 在發文彈窗中顯示使用者已選中的標籤
     */
    function renderSelectedHashtags() {
        // 獲取已選標籤顯示區域的容器
        const container = document.getElementById('selected-hashtags');
        
        // 清空現有的已選標籤顯示
        if (container) {
            container.innerHTML = '';
        
        // 為每個已選標籤建立顯示元素
        selectedHashtags.forEach(tag => {
            // 跳過無效的標籤物件
            if (!tag) return;
            
            // 建立標籤顯示元素
            const tagSpan = document.createElement('span');
            tagSpan.className = 'min-h-[32px] min-w-[40px] text-center rounded-full bg-[#ffda78] text-[#333333] text-[14px] leading-[16px] font-medium font-["Roboto"] p-2';
            
            // 設定標籤顯示文字
            tagSpan.textContent = tag.content;
            
            // 設定標籤 ID 屬性，用於後續提交時識別
            tagSpan.setAttribute('data-tag-id', tag.tagId);
            
            // 將標籤顯示元素加入容器
            container.appendChild(tagSpan);
            });
        }
    }

    // ========== 標籤彈窗事件處理 ==========
    
    /**
     * 開啟標籤選擇彈窗事件處理
     * 點擊標籤按鈕時觸發，載入並顯示標籤選擇介面
     */
    if (openHashtagBtn && hashtagModal) {
        openHashtagBtn.addEventListener('click', async function () {
            // 先載入所有可用標籤（如果尚未載入）
            await fetchHashtags();
            
            // 渲染標籤選擇清單
            renderHashtagModal();
            
            // 顯示標籤選擇彈窗
            hashtagModal.classList.remove('hidden');
        });
    }

    /**
     * 關閉標籤選擇彈窗事件處理
     * 點擊取消按鈕時觸發，關閉標籤選擇彈窗但不儲存變更
     */
    if (closeHashtagModalBtn && hashtagModal) {
        closeHashtagModalBtn.addEventListener('click', () => {
            // 隱藏標籤選擇彈窗
            hashtagModal.classList.add('hidden');
        });
    }

    /**
     * 確認選擇標籤事件處理
     * 點擊確定按鈕時觸發，儲存使用者的標籤選擇
     */
    if (confirmHashtagBtn && hashtagList && hashtagModal) {
        confirmHashtagBtn.addEventListener('click', () => {
            // 獲取所有被選中的核取方塊
            const checks = hashtagList.querySelectorAll('input[type=checkbox]:checked');
            
            // 將選中的核取方塊轉換為標籤物件
            selectedHashtags = Array.from(checks).map(chk => {
                // 從核取方塊取得標籤 ID
                const tagId = chk.value;
                
                // 從所有標籤中找到對應的標籤物件
                return allHashtags.find(t => t.tagId === tagId);
            });
            
            // 重新渲染已選標籤顯示區域
            renderSelectedHashtags();
            
            // 隱藏標籤選擇彈窗
            hashtagModal.classList.add('hidden');
        });
    }

    // ========== 文章提交功能 ==========
    
    /**
     * 文章提交按鈕點擊事件處理
     * 收集所有輸入資料並送出到後端 API
     */
    const submitPostBtn = document.getElementById('submitPostBtn');
    if (submitPostBtn) {
        submitPostBtn.addEventListener('click', async () => {
        // ========== 資料收集 ==========
        
        // 取得文章內容並去除前後空白
        const content = document.getElementById('postContent').value.trim();
        
        // 取得上傳的檔案列表
        const attachments = document.getElementById('fileInput').files;
        
        // 從已選標籤顯示區域取得所有標籤 ID
        const selectedHashtags = Array.from(document.querySelectorAll('#selected-hashtags span'))
            .map(el => el.dataset.tagId);

        // ========== 資料驗證 ==========
        
        // 檢查文章內容是否為空
        if (!content) {
            alert('文章內容不能為空');
            return;
        }

        // ========== 資料封裝 ==========
        
        // 建立 FormData 物件，用於傳送檔案和文字資料
        const formData = new FormData();
        
        // 加入文章內容
        formData.append('Content', content);
        
        // 加入文章可見性設定（0=公開，1=私人）
        formData.append('IsPublic', '0');

        // 加入所有附件檔案
        for (const file of attachments) {
            formData.append('Attachments', file);
        }
        
        // 加入所有選中的標籤 ID
        selectedHashtags.forEach(tagId => {
            if (tagId) formData.append('SelectedHashtags', tagId);
        });

        // ========== API 請求處理 ==========
        
        try {
            // 向後端 API 送出新增文章請求
            const response = await fetch('/Post/Create', {
                method: 'POST',
                body: formData
            });

            if (response.ok) {
                // ========== 成功處理 ==========
                
                // 顯示成功訊息
                alert('文章送出成功！');
                
                // 重置表單內容
                document.getElementById('postContent').value = '';
                document.getElementById('fileInput').value = '';
                document.getElementById('selected-hashtags').innerHTML = '';
                
                // 關閉彈窗
                document.getElementById('postModel').classList.add('hidden');
                document.getElementById('overlay').classList.add('hidden');
                
                // 可在此處添加頁面重新整理或重新載入文章列表的邏輯
                // window.location.reload(); // 如需要重新整理頁面
                
            } else {
                // ========== 失敗處理 ==========
                
                // 取得錯誤訊息並顯示給使用者
                const errorText = await response.text();
                alert('送出失敗: ' + errorText);
            }
        } catch (err) {
            // ========== 例外處理 ==========
            
            // 網路錯誤或其他例外狀況的處理
            alert('發生錯誤: ' + err.message);
            console.error('文章提交錯誤:', err);
        }
        });
    }
    
    // ========== 初始化完成 ==========
    
    // console.log('新增文章彈窗功能已初始化完成');
});