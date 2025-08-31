// Fetch Interceptor - 處理 API 監控與可取消請求（從 d_main.js 抽出）

(function() {
    'use strict';

    // Navigation-scoped AbortController for page GET APIs
    let navAbortController = (typeof window !== 'undefined' && window.__navAbortController) || new AbortController()
    if (typeof window !== 'undefined') window.__navAbortController = navAbortController

    // 提供外部呼叫以中止當前頁面的 DB GET 請求
    if (typeof window !== 'undefined') {
        window.abortAllDbGets = function() {
            try { 
                navAbortController.abort() 
            } finally {
                navAbortController = new AbortController()
                window.__navAbortController = navAbortController
            }
        }
    }

    // Patch fetch to track /api/Db_* requests and attach cancellable signals (GET only)
    if (typeof window !== 'undefined' && typeof window.fetch === 'function' && !window.__dbFetchPatched) {
        window.__dbFetchPatched = true
        const origFetch = window.fetch
        let dbApiInFlight = 0
        
        const isDbApi = (input) => {
            try {
                let url
                if (typeof input === 'string') url = input
                else if (input && typeof input.url === 'string') url = input.url
                else return false
                const u = new URL(url, window.location.origin)
                return /^\/api\/Db_/i.test(u.pathname)
            } catch { 
                return false 
            }
        }

        window.__getDbInFlight = () => dbApiInFlight

        window.fetch = function(...args) {
            const tracking = isDbApi(args[0])
            const method = (args[1] && args[1].method) ? String(args[1].method).toUpperCase() : 'GET'

            // 建立可被導航中止的 signal（僅 GET）
            let init = args[1]
            let combinedController = null
            let detachHandlers = () => {}
            
            if (tracking && method === 'GET') {
                combinedController = new AbortController()
                const pageAbort = navAbortController

                const onPageAbort = () => { 
                    try { 
                        combinedController.abort() 
                    } catch(_){} 
                }
                pageAbort.signal.addEventListener('abort', onPageAbort)

                let onOrigAbort = null
                if (init && init.signal) {
                    if (init.signal.aborted) {
                        combinedController.abort()
                    } else {
                        onOrigAbort = () => { 
                            try { 
                                combinedController.abort() 
                            } catch(_){} 
                        }
                        init.signal.addEventListener('abort', onOrigAbort)
                    }
                }

                init = { ...(init || {}), signal: combinedController.signal }
                detachHandlers = () => {
                    try { 
                        pageAbort.signal.removeEventListener('abort', onPageAbort) 
                    } catch(_){}
                    try { 
                        if (init && init.signal && onOrigAbort) 
                            init.signal.removeEventListener('abort', onOrigAbort) 
                    } catch(_){}
                }
            }

            if (tracking) { 
                dbApiInFlight++ 
            }
            
            const finalize = () => { 
                if (tracking) { 
                    dbApiInFlight = Math.max(0, dbApiInFlight - 1) 
                } 
                detachHandlers() 
            }

            try {
                const p = origFetch.call(this, args[0], init)
                if (p && typeof p.finally === 'function') 
                    return p.finally(finalize)
                // Fallback
                return p.then((x) => { 
                    finalize(); 
                    return x 
                }, (e) => { 
                    finalize(); 
                    throw e 
                })
            } catch (e) {
                finalize(); 
                throw e
            }
        }
    }

    // 暴露工具函數到全域
    window.DashboardFetchInterceptor = {
        abortAllDbGets: window.abortAllDbGets,
        getDbInFlight: window.__getDbInFlight || (() => 0),
        isPatched: () => !!window.__dbFetchPatched
    }

})();
