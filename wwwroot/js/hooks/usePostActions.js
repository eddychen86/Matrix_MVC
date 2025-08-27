/**
 * usePostActions Hook
 * 統一處理文章互動操作（按讚、收藏、評論、分享、檢舉等）
 */
import { authService } from '/js/services/AuthService.js'

export function usePostActions() {
    const getToken = () => localStorage.getItem('access_token') || ''

    // 以 authService 為準，避免抓不到 currentUser 而誤判
    const checkAuth = async () => {
        try {
            const r = await authService.getAuthStatus()
            const authed = !!(r && r.success && r.data && r.data.authenticated)
            if (!authed) {
                if (window.loginPopupManager?.open) window.loginPopupManager.open()
                else alert('請先登入才能進行此操作')
                return false
            }
            return true
        } catch (e) {
            console.error('auth check failed:', e)
            if (window.loginPopupManager?.open) window.loginPopupManager.open()
            else alert('請先登入才能進行此操作')
            return false
        }
    }

    /** Guid 判斷（最後的保險用在 userId 上，若後端其實塞的是 PersonId 也能吃到） */
    const looksLikeGuid = (v) => typeof v === 'string' && /^[0-9a-fA-F-]{36}$/.test(v)

    /** 專門把「作者的 PersonId」解出來；優先只用明確的 personId 欄位，最後才容忍 userId(看起來是 Guid 時) */
    const resolveAuthorPersonId = (row, provided) => {
        if (provided) return provided
        if (!row) return null
        return (
            row.authorPersonId ||
            row.personId ||
            row.authorId ||                 // 請確認這是 PersonId（建議在 formatArticles 直接對齊）
            row.profile?.personId ||
            row.author?.personId ||
            (looksLikeGuid(row.userPersonId) ? row.userPersonId : null) ||
            // ⚠️ 最後保險：若確定 userId 本來就存的是 PersonId，這行就能救回；否則不會吃到
            (looksLikeGuid(row.userId) ? row.userId : null) ||
            null
        )
    }

    /** 按讚 */
    const togglePraise = async (articleId, item = null) => {
        if (!(await checkAuth())) return false
        try {
            const res = await fetch('/api/PostState/praise', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    ...(getToken() ? { Authorization: `Bearer ${getToken()}` } : {})
                },
                credentials: 'include',
                body: JSON.stringify({ articleId })
            })
            const body = await res.text()
            if (!res.ok) {
                if (res.status === 401) throw new Error('unauthorized')
                throw new Error('praise failed: ' + body)
            }
            const data = JSON.parse(body || '{}')
            if (!data?.success) throw new Error(data?.message || 'praise failed')

            if (item) {
                item.isPraised = !!data.isPraised
                item.praiseCount = Number(data.praiseCount ?? item.praiseCount)
            }
            return { success: true, isPraised: !!data.isPraised, praiseCount: Number(data.praiseCount || 0) }
        } catch (error) {
            console.error('togglePraise failed:', error)
            alert(error.message === 'unauthorized' ? '請先登入' : '操作失敗')
            return { success: false, error: error.message }
        }
    }

    /** 收藏 */
    const toggleCollect = async (articleId, item = null) => {
        if (!(await checkAuth())) return false
        try {
            const res = await fetch('/api/PostState/collect', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    ...(getToken() ? { Authorization: `Bearer ${getToken()}` } : {})
                },
                credentials: 'include',
                body: JSON.stringify({ articleId })
            })
            const body = await res.text()
            if (!res.ok) {
                if (res.status === 401) throw new Error('unauthorized')
                throw new Error('collect failed: ' + body)
            }
            const data = JSON.parse(body || '{}')
            if (!data?.success) throw new Error(data?.message || 'collect failed')

            if (item) {
                item.isCollected = !!data.isCollected
                item.collectCount = Number(data.collectCount ?? item.collectCount)
            }
            return { success: true, isCollected: !!data.isCollected, collectCount: Number(data.collectCount || 0) }
        } catch (error) {
            console.error('toggleCollect failed:', error)
            alert(error.message === 'unauthorized' ? '請先登入' : '操作失敗')
            return { success: false, error: error.message }
        }
    }

    /** 回覆 */
    const openReply = async (articleId) => {
        if (!(await checkAuth())) return false
        try {
            if (window.globalApp?.openReplyWithAuth) {
                const currentUser = window.globalApp.currentUser || window.currentUser
                await window.globalApp.openReplyWithAuth(articleId, currentUser)
            } else if (window.globalApp?.replyModal) {
                await window.globalApp.openReply(articleId)
            } else {
                console.log('Opening reply for article:', articleId)
                alert('回覆功能開發中')
            }
            return true
        } catch (error) {
            console.error('openReply failed:', error)
            alert('開啟回覆失敗')
            return false
        }
    }

    /** 分享 */
    const shareArticle = async (articleId, item = null) => {
        try {
            const url = `${window.location.origin}/article/${articleId}`
            if (navigator.share) {
                await navigator.share({
                    title: item?.title || '分享文章',
                    text: item?.content?.substring(0, 100) || '',
                    url
                })
            } else {
                await navigator.clipboard.writeText(url)
                alert('文章連結已複製到剪貼板')
            }
            return true
        } catch (error) {
            console.error('shareArticle failed:', error)
            alert('分享失敗')
            return false
        }
    }

    // =========================================================================================
    // ⭐ 新增：簡單的「原生 <dialog>」檢舉 UI（避免 prompt，改成彈窗）
    // =========================================================================================
    function createReportDialogIfNeed() {
        let dlg = document.getElementById('report-dialog')
        if (dlg) return dlg._controller

        const wrapper = document.createElement('div')
                    wrapper.innerHTML = `
            <dialog id="report-dialog">
              <form method="dialog" id="report-form"
                    style="width:min(800px,96vw)"  /* 直寫 style 保證寬度，不靠 Tailwind 編譯 */
                    class="flex flex-col gap-3 rounded-2xl p-5
                           bg-neutral-900/95 text-white shadow-2xl ring-1 ring-white/10">
                <h3 class="font-bold text-lg">檢舉</h3>
            
                <fieldset class="flex items-center gap-4">
                  <label class="inline-flex items-center gap-2">
                    <input type="radio" name="type" value="1" checked>
                    <span>檢舉文章</span>
                  </label>
                  <label class="inline-flex items-center gap-2 opacity-95" id="report-radio-user">
                    <input type="radio" name="type" value="0">
                    <span>檢舉使用者</span>
                  </label>
                </fieldset>
            
                <label class="flex flex-col gap-2">
                  <span>檢舉理由</span>
                  <textarea name="reason" rows="5" maxlength="500"
                    class="rounded-xl p-3 bg-neutral-800 text-gray-100 border border-neutral-700 outline-none
                           focus:border-blue-500 focus:ring-2 focus:ring-blue-500/30"
                    placeholder="請輸入 1–500 字"></textarea>
                  <small class="self-end text-gray-400"><span id="report-count">0</span>/500</small>
                </label>
            
                <menu class="flex justify-end gap-3">
                  <!-- 兩顆都 submit，用 e.submitter 分辨 -->
                  <button type="submit" value="cancel" class="px-4 py-2 rounded-xl bg-neutral-700 hover:bg-neutral-600">取消</button>
                  <button type="submit" value="submit" class="px-4 py-2 rounded-xl bg-rose-600 hover:bg-rose-500">送出</button>
                </menu>
            
                <input type="hidden" name="articleId">
                <input type="hidden" name="authorPersonId">
              </form>
            </dialog>`.trim()

        document.body.appendChild(wrapper.firstElementChild)

        // 元素
        dlg = document.getElementById('report-dialog')
        const form = document.getElementById('report-form')
        const radioUser = document.getElementById('report-radio-user')
        const count = document.getElementById('report-count')

        // 置中（讓 <dialog> 只當容器，外觀放在 form）
        dlg.style.position = 'fixed'
        dlg.style.top = '50%'; dlg.style.left = '50%'
        dlg.style.transform = 'translate(-50%, -50%)'
        dlg.style.padding = '0'
        dlg.style.border = 'none'
        dlg.style.background = 'transparent'
        dlg.style.maxWidth = '96vw'
        dlg.style.zIndex = '1000'

        // ::backdrop（用 <style> 注入控制）
        const style = document.createElement('style')
        style.textContent = `
    #report-dialog::backdrop{
      backdrop-filter: blur(3px);
      background: rgba(0,0,0,.60);
    }`
        document.head.appendChild(style)

        // 字數計數（加防呆）
        form.elements['reason'].addEventListener('input', e => {
            if (count) count.textContent = String(e.target.value.length)
        })

        // 關閉時重置
        dlg.addEventListener('close', () => form.reset())

        const controller = {
            open({ articleId, authorPersonId }) {
                form.elements['articleId'].value = articleId || ''
                form.elements['authorPersonId'].value = authorPersonId || ''
                // 沒作者 id 就禁用「檢舉使用者」
                const ru = radioUser.querySelector('input')
                ru.disabled = !authorPersonId
                radioUser.style.opacity = authorPersonId ? '1' : '.4'
                form.elements['type'].value = 1
                form.elements['reason'].value = ''
                if (count) count.textContent = '0'
                dlg.showModal()
            },
            submit(postJson) {
                return new Promise((resolve, reject) => {
                    form.onsubmit = async (e) => {
                        e.preventDefault()

                        // ⬅ 取消：一定要 resolve(false)，避免外層 await 卡住
                        if (e.submitter && e.submitter.value === 'cancel') {
                            dlg.close()
                            return resolve(false)
                        }

                        const type = Number(form.elements['type'].value)
                        const reason = (form.elements['reason'].value || '').trim()
                        const articleId = form.elements['articleId'].value
                        const authorPersonId = form.elements['authorPersonId'].value

                        if (!(reason.length >= 1 && reason.length <= 500)) return alert('請輸入 1–500 字')
                        if (type === 0 && !authorPersonId) return alert('找不到作者 Id，無法檢舉使用者')
                        if (type === 1 && !articleId) return alert('找不到文章 Id')

                        const payload = {
                            type,
                            targetId: type === 0 ? authorPersonId : articleId,
                            reportedUserId: type === 1 ? authorPersonId : null,
                            reason
                        }

                        try {
                            const res = await postJson('/api/reports', payload)
                            if (!res.ok) {
                                try { const j = await res.json(); alert(j.message || '檢舉失敗') }
                                catch { alert('檢舉失敗') }
                                return reject(new Error('report failed'))
                            }
                            const data = await res.json()
                            alert(data.message || '已收到你的檢舉，感謝回報！')
                            dlg.close()
                            resolve(true)
                        } catch (err) {
                            console.error('report submit failed', err)
                            alert('檢舉失敗')
                            reject(err)
                        }
                    }
                })
            }
        }

        dlg._controller = controller
        return controller
    }
    // =========================================================================================

    /** 檢舉 */
    /** 檢舉（只呼叫上面的 dialog，不再內嵌 template） */
    const openReport = async (articleId, authorPersonId, row = null) => {
        if (!(await checkAuth())) return false

        const resolvedAuthorId = resolveAuthorPersonId(row, authorPersonId)
        const ui = createReportDialogIfNeed()

        const postJson = (url, body) => fetch(url, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                ...(getToken() ? { Authorization: `Bearer ${getToken()}` } : {})
            },
            credentials: 'include',
            body: JSON.stringify(body)
        })

        ui.open({ articleId, authorPersonId: resolvedAuthorId })
        try { await ui.submit(postJson); return true } catch { return false }
    }

    /** 統一的 stateFunc（自動把第二個參數當 payload 或 articleId；也會把 row（item）帶進去） */
    const stateFunc = async (action, articleIdOrPayload, item = null) => {
        // 兼容兩種呼叫：stateFunc('x', item) 或 stateFunc('x', {articleId, authorId}, item)
        let articleId = articleIdOrPayload
        let row = item
        if (typeof articleIdOrPayload === 'object') {
            articleId = articleIdOrPayload.articleId ?? item?.articleId
            row = row || articleIdOrPayload // 如果第三參數沒傳，就把第二參數當 row
        }

        if (!['share', 'report'].includes(action) && !articleId) return false
        if (row?._busy) return false
        if (row) row._busy = true

        try {
            switch (action) {
                case 'praise': return await togglePraise(articleId, row)
                case 'collect': return await toggleCollect(articleId, row)
                case 'comment': return await openReply(articleId)
                case 'share': return await shareArticle(articleId, row)
                case 'report': {
                    // 從 row (已處理過的 item) 或 articleIdOrPayload 中解構所需資料
                    let reportArticleId = articleId
                    let authorId = null
                    
                    // 如果 row 存在且包含必要資訊，優先使用 row
                    if (row && row.articleId) {
                        reportArticleId = row.articleId
                        authorId = row.authorId || row.authorName // 嘗試獲取 authorId 或 authorName
                    }
                    // 如果 articleIdOrPayload 是物件且包含 authorId，則使用它
                    else if (typeof articleIdOrPayload === 'object' && articleIdOrPayload.authorId) {
                        reportArticleId = articleIdOrPayload.articleId
                        authorId = articleIdOrPayload.authorId
                    }
                    
                    if (!reportArticleId) {
                        console.error('report action requires articleId')
                        return false
                    }
                    if (!authorId) {
                        console.error('report action requires authorId or authorName')
                        return false
                    }
                    
                    return await openReport(reportArticleId, authorId, row)
                }
                default: return false
            }
        } finally {
            if (row) row._busy = false
            window.lucide?.createIcons?.()
        }
    }

    return {
        stateFunc,
        openReport,
        togglePraise,
        toggleCollect,
        openReply,
        shareArticle,
        checkAuth
    }
}

export default usePostActions
