/**
 * Friends Manager - 處理好友相關功能
 * 從 main.js 中抽離出來的好友管理功能
 */

export const useFriendsManager = (currentUser) => {
    const { ref } = Vue

    //#region Friends Data
    const friends = ref([])
    const friendsLoading = ref(false)
    const friendsTotal = ref(0)
    const friendsStatus = ref('accepted')

    const getUsernameFromPath = () => {
        const parts = window.location.pathname.split('/').filter(Boolean)
        if (parts[0]?.toLowerCase() === 'profile' && parts[1] && parts[1].toLowerCase() !== 'index') {
            return parts[1]
        }
        return null
    }

    const loadFriends = async (page = 1, pageSize = 20, username = null, status = friendsStatus.value) => {
        try {
            friendsLoading.value = true
            const { friendsService } = await import('/js/components/friends.js')

            const targetUsername = username || getUsernameFromPath() || currentUser.username || null
            const result = await friendsService.getFriends(page, pageSize, targetUsername, status)

            if (result.success) {
                friends.value = result.friends
                friendsTotal.value = result.totalCount
            } else if (result.unauthorized) {
                friends.value = []
                friendsTotal.value = 0
            } else {
                console.error('Failed to load friends:', result.error)
            }
        } catch (err) {
            console.error('Error loading friends:', err)
        } finally {
            friendsLoading.value = false
        }
    }

    const changeFriendsStatus = (status) => {
        friendsStatus.value = status
        // 預設重新載入列表
        loadFriends(1, 20, null, friendsStatus.value)
    }

    // 小工具：把搜尋使用者轉成 Follows 清單的資料形狀
    const mapSearchUserToFollowItem = (u) => ({
        personId: u.personId,
        senderName: u.displayName,
        senderAvatarUrl: u.avatarUrl || '/static/img/cute.png',
        followTime: new Date().toISOString()
    })

    const toggleFollow = async (targetPersonId, currentStatus, popupData, fetchFollows) => {
        if (!currentUser.isAuthenticated) {
            alert('請先登入才能追蹤')
            return
        }

        // ✅ 防呆：targetPersonId 必須存在
        if (!targetPersonId) {
            console.warn('toggleFollow: targetPersonId 為空，取消請求')
            return
        }
        try {
            const method = currentStatus ? 'DELETE' : 'POST'
            const res = await fetch(`/api/follows/${targetPersonId}`, {
                method,
                credentials: 'include'
            })

            // ✅ 500/HTML 錯誤頁防呆
            const ct = res.headers.get('content-type') || ''
            let result
            if (ct.includes('application/json')) {
                result = await res.json()
            } else {
                const text = await res.text()
                console.error('Follow API raw:', text)
                alert('伺服器錯誤：' + text.slice(0, 140))
                return
            }

            if (!res.ok || !result?.success) {
                alert(result?.message || '操作失敗，請稍後再試')
                return
            }

            // ✅ 同步更新「搜尋結果」按鈕狀態
            const u = popupData.Search.Users.find(u => u.personId === targetPersonId)
            if (u) u.isFollowed = !currentStatus

            if (currentStatus === true) {
                // ✅ 取消追蹤：從清單移除
                popupData.Follows = popupData.Follows.filter(f => f.personId !== targetPersonId)
            } else {
                // ✅ 追蹤：若清單沒有，樂觀加入一筆
                const exists = popupData.Follows.some(f => f.personId === targetPersonId)
                if (!exists) {
                    const item = u
                        ? {
                            personId: u.personId,
                            senderName: u.displayName,
                            senderAvatarUrl: u.avatarUrl || '/static/img/cute.png',
                            followTime: new Date().toISOString()
                        }
                        : {
                            personId: targetPersonId,
                            senderName: '已追蹤使用者',
                            senderAvatarUrl: '/static/img/cute.png',
                            followTime: new Date().toISOString()
                        }
                    popupData.Follows.unshift(item)
                }
            }

            // ✅ 若此時關鍵字已清空、且身在 Follows 視窗 → 重抓一次清單（確保與後端一致）
            // 這個邏輯需要在使用時傳入相關的參數
        } catch (err) {
            console.error('追蹤操作錯誤：', err)
        }
    }
    //#endregion

    return {
        // 狀態
        friends,
        friendsLoading,
        friendsTotal,
        friendsStatus,

        // 方法
        getUsernameFromPath,
        loadFriends,
        changeFriendsStatus,
        mapSearchUserToFollowItem,
        toggleFollow
    }
}

// 單獨導出創建函數
export const createFriendsManager = useFriendsManager