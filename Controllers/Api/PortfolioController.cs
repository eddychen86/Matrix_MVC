using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;

namespace Matrix.Controllers.Api
{
    /// <summary>
    /// 提供錢包地址驗證 + 取得 NFT 與代幣餘額的 API（目前僅做參數驗證與回傳範本結構）
    /// 後續可接上實際供應商（如 Alchemy/Moralis/SimpleHash/Helius 等）
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class PortfolioController : ControllerBase
    {
        private static readonly HashSet<string> SupportedChains = new(StringComparer.OrdinalIgnoreCase)
        { "eth", "polygon", "bnb", "solana", "bitcoin", "tron" };

        [HttpGet("validate")]
        public IActionResult Validate([FromQuery] string chain, [FromQuery] string address)
        {
            var (ok, message) = ValidateChainAndAddress(chain, address);
            if (!ok) return BadRequest(new { success = false, message });

            return Ok(new { success = true, chain = chain.ToLowerInvariant(), address });
        }

        /// <summary>
        /// 取得 NFT 收藏（僅驗證與回傳範例結構）
        /// </summary>
        [HttpGet("nft")]
        public IActionResult GetNfts([FromQuery] string chain, [FromQuery] string address)
        {
            var (ok, message) = ValidateChainAndAddress(chain, address);
            if (!ok) return BadRequest(new { success = false, message });

            // TODO: 在此接上實際供應商查詢邏輯
            var example = new[]
            {
                new {
                    collectionName = "Example Collection",
                    contractAddress = chain.Equals("solana", StringComparison.OrdinalIgnoreCase) ? "So11111111111111111111111111111111111111112" : "0x0000000000000000000000000000000000000000",
                    tokenId = chain.Equals("solana", StringComparison.OrdinalIgnoreCase) ? "ExampleMintAddress" : "1",
                    imageUrl = "/static/images/default-nft.png"
                }
            };

            return Ok(new { success = true, chain = chain.ToLowerInvariant(), address, items = Array.Empty<object>() /* example */ });
        }

        /// <summary>
        /// 取得代幣/原生幣餘額（僅驗證與回傳範例結構）
        /// </summary>
        [HttpGet("tokens")]
        public IActionResult GetTokens([FromQuery] string chain, [FromQuery] string address)
        {
            var (ok, message) = ValidateChainAndAddress(chain, address);
            if (!ok) return BadRequest(new { success = false, message });

            // TODO: 在此接上實際供應商查詢邏輯
            var balances = new[]
            {
                new { symbol = chain.Equals("eth", StringComparison.OrdinalIgnoreCase) ? "ETH" : chain.Equals("polygon", StringComparison.OrdinalIgnoreCase) ? "MATIC" : "COIN",
                      contractAddress = (string?)null,
                      balance = 0m,
                      decimals = 18 }
            };

            return Ok(new { success = true, chain = chain.ToLowerInvariant(), address, items = Array.Empty<object>() /* balances */ });
        }

        private static (bool ok, string message) ValidateChainAndAddress(string? chain, string? address)
        {
            if (string.IsNullOrWhiteSpace(chain)) return (false, "chain 不能為空");
            if (!SupportedChains.Contains(chain)) return (false, $"不支援的 chain：{chain}");
            if (string.IsNullOrWhiteSpace(address)) return (false, "address 不能為空");

            // 依鏈別驗證基本格式
            bool valid = chain.ToLowerInvariant() switch
            {
                "eth" or "polygon" or "bnb" => Regex.IsMatch(address, "^0x[a-fA-F0-9]{40}$"),
                "solana" => Regex.IsMatch(address, "^[1-9A-HJ-NP-Za-km-z]{32,44}$"),
                "bitcoin" => Regex.IsMatch(address, "^(bc1[0-9a-z]{25,39}|[13][a-km-zA-HJ-NP-Z1-9]{25,34})$"),
                "tron" => Regex.IsMatch(address, "^T[1-9A-HJ-NP-Za-km-z]{33}$") || Regex.IsMatch(address, "^41[0-9a-fA-F]{40}$"),
                _ => false
            };

            if (!valid) return (false, $"address 格式與 {chain} 不符");
            return (true, "");
        }
    }
}

