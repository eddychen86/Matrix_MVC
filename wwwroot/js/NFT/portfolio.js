// NFT/Crypto Portfolio helper (ESM)
// - 提供鏈別/地址驗證
// - 封裝呼叫 /api/portfolio 的方法

const CHAINS = ['eth', 'polygon', 'bnb', 'solana', 'bitcoin', 'tron']

export const validators = {
  isSupportedChain(chain) {
    return typeof chain === 'string' && CHAINS.includes(chain.toLowerCase())
  },
  isAddress(chain, address) {
    if (!address || !chain) return false
    const c = chain.toLowerCase()
    switch (c) {
      case 'eth':
      case 'polygon':
      case 'bnb':
        return /^0x[a-fA-F0-9]{40}$/.test(address)
      case 'solana':
        return /^[1-9A-HJ-NP-Za-km-z]{32,44}$/.test(address)
      case 'bitcoin':
        return /^(bc1[0-9a-z]{25,39}|[13][a-km-zA-HJ-NP-Z1-9]{25,34})$/.test(address)
      case 'tron':
        return /^T[1-9A-HJ-NP-Za-km-z]{33}$/.test(address) || /^41[0-9a-fA-F]{40}$/.test(address)
      default:
        return false
    }
  }
}

export const portfolioApi = {
  async validate(chain, address) {
    const url = `/api/portfolio/validate?chain=${encodeURIComponent(chain)}&address=${encodeURIComponent(address)}`
    const res = await fetch(url, { credentials: 'include' })
    const data = await res.json().catch(() => ({}))
    return { ok: res.ok, data }
  },

  async getNfts(chain, address) {
    const url = `/api/portfolio/nft?chain=${encodeURIComponent(chain)}&address=${encodeURIComponent(address)}`
    const res = await fetch(url, { credentials: 'include' })
    const data = await res.json().catch(() => ({}))
    return { ok: res.ok, data }
  },

  async getTokens(chain, address) {
    const url = `/api/portfolio/tokens?chain=${encodeURIComponent(chain)}&address=${encodeURIComponent(address)}`
    const res = await fetch(url, { credentials: 'include' })
    const data = await res.json().catch(() => ({}))
    return { ok: res.ok, data }
  }
}

// 簡易封裝：先在前端做驗證，不通過就不打 API
export async function fetchPortfolio(chain, address, { include = { nft: true, tokens: true } } = {}) {
  const result = { valid: false, errors: [], nft: null, tokens: null }

  if (!validators.isSupportedChain(chain)) {
    result.errors.push('不支援的鏈別')
    return result
  }
  if (!validators.isAddress(chain, address)) {
    result.errors.push('地址格式錯誤')
    return result
  }

  result.valid = true

  if (include.nft) {
    const n = await portfolioApi.getNfts(chain, address)
    result.nft = n.data || null
  }
  if (include.tokens) {
    const t = await portfolioApi.getTokens(chain, address)
    result.tokens = t.data || null
  }

  return result
}

export default { validators, portfolioApi, fetchPortfolio }

