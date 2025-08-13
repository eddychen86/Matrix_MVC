namespace Matrix.ViewModels
{
    /// <summary>
    /// NFT 彈窗顯示的 ViewModel
    /// </summary>
    public class NFTPopupViewModel
    {
        /// <summary>
        /// 擁有者的 Person ID
        /// </summary>
        public Guid PersonId { get; set; }

        /// <summary>
        /// 擁有者的 User ID
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// 擁有者顯示名稱
        /// </summary>
        public string DisplayName { get; set; } = "";

        /// <summary>
        /// 擁有者頭像路徑
        /// </summary>
        public string? AvatarPath { get; set; }

        /// <summary>
        /// 錢包地址
        /// </summary>
        public string? WalletAddress { get; set; }

        /// <summary>
        /// NFT 清單
        /// </summary>
        public List<NFTItemViewModel> NFTs { get; set; } = new();

        /// <summary>
        /// NFT 總數
        /// </summary>
        public int TotalNFTs => NFTs.Count;

        /// <summary>
        /// 是否有 NFT
        /// </summary>
        public bool HasNFTs => NFTs.Any();

        /// <summary>
        /// NFT 總價值
        /// </summary>
        public decimal TotalValue => NFTs.Sum(n => n.Price);

        /// <summary>
        /// 各幣別統計
        /// </summary>
        public List<CurrencyStatsViewModel> CurrencyStats { get; set; } = new();

        /// <summary>
        /// 最近收藏的 NFT
        /// </summary>
        public NFTItemViewModel? LatestNFT => NFTs.OrderByDescending(n => n.CollectTime).FirstOrDefault();

        /// <summary>
        /// 最高價值的 NFT
        /// </summary>
        public NFTItemViewModel? MostExpensiveNFT => NFTs.OrderByDescending(n => n.Price).FirstOrDefault();
    }

    /// <summary>
    /// NFT 項目的 ViewModel
    /// </summary>
    public class NFTItemViewModel
    {
        /// <summary>
        /// NFT ID
        /// </summary>
        public Guid NftId { get; set; }

        /// <summary>
        /// NFT 名稱
        /// </summary>
        public string FileName { get; set; } = "";

        /// <summary>
        /// 檔案路徑
        /// </summary>
        public string FilePath { get; set; } = "";

        /// <summary>
        /// 收藏時間
        /// </summary>
        public DateTime CollectTime { get; set; }

        /// <summary>
        /// 幣別
        /// </summary>
        public string Currency { get; set; } = "";

        /// <summary>
        /// 價格
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// 格式化的價格顯示
        /// </summary>
        public string FormattedPrice => $"{Price:F4} {Currency}";

        /// <summary>
        /// 格式化的收藏日期
        /// </summary>
        public string FormattedDate => CollectTime.ToString("yyyy-MM-dd");

        /// <summary>
        /// 格式化的收藏時間
        /// </summary>
        public string FormattedDateTime => CollectTime.ToString("yyyy-MM-dd HH:mm");

        /// <summary>
        /// 相對時間顯示（例如：2 天前）
        /// </summary>
        public string RelativeTime
        {
            get
            {
                var timeSpan = DateTime.Now - CollectTime;
                
                if (timeSpan.TotalDays >= 365)
                {
                    int years = (int)(timeSpan.TotalDays / 365);
                    return $"{years} 年前";
                }
                else if (timeSpan.TotalDays >= 30)
                {
                    int months = (int)(timeSpan.TotalDays / 30);
                    return $"{months} 個月前";
                }
                else if (timeSpan.TotalDays >= 1)
                {
                    int days = (int)timeSpan.TotalDays;
                    return $"{days} 天前";
                }
                else if (timeSpan.TotalHours >= 1)
                {
                    int hours = (int)timeSpan.TotalHours;
                    return $"{hours} 小時前";
                }
                else if (timeSpan.TotalMinutes >= 1)
                {
                    int minutes = (int)timeSpan.TotalMinutes;
                    return $"{minutes} 分鐘前";
                }
                else
                {
                    return "剛剛";
                }
            }
        }

        /// <summary>
        /// 圖片 URL（用於顯示 NFT 圖片）
        /// </summary>
        public string ImageUrl => string.IsNullOrEmpty(FilePath) ? "/static/images/default-nft.png" : FilePath;

        /// <summary>
        /// 是否為最近收藏（7天內）
        /// </summary>
        public bool IsRecent => (DateTime.Now - CollectTime).TotalDays <= 7;

        /// <summary>
        /// 是否為高價值 NFT（價格大於平均值）
        /// </summary>
        public bool IsHighValue { get; set; } = false;

        /// <summary>
        /// CSS 類別（根據幣別不同顯示不同顏色）
        /// </summary>
        public string CurrencyBadgeClass => Currency.ToLowerInvariant() switch
        {
            "eth" => "badge-primary",
            "btc" => "badge-warning",
            "matic" => "badge-secondary",
            "bnb" => "badge-accent",
            "sol" => "badge-info",
            "usdt" or "usdc" => "badge-success",
            _ => "badge-neutral"
        };
    }

    /// <summary>
    /// 幣別統計的 ViewModel
    /// </summary>
    public class CurrencyStatsViewModel
    {
        /// <summary>
        /// 幣別
        /// </summary>
        public string Currency { get; set; } = "";

        /// <summary>
        /// 該幣別的 NFT 數量
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// 該幣別的總價值
        /// </summary>
        public decimal TotalValue { get; set; }

        /// <summary>
        /// 平均價格
        /// </summary>
        public decimal AveragePrice => Count > 0 ? TotalValue / Count : 0;

        /// <summary>
        /// 格式化的總價值
        /// </summary>
        public string FormattedTotalValue => $"{TotalValue:F4} {Currency}";

        /// <summary>
        /// 格式化的平均價格
        /// </summary>
        public string FormattedAveragePrice => $"{AveragePrice:F4} {Currency}";

        /// <summary>
        /// 百分比（在所有幣別中的佔比）
        /// </summary>
        public decimal Percentage { get; set; }

        /// <summary>
        /// 格式化的百分比
        /// </summary>
        public string FormattedPercentage => $"{Percentage:F1}%";

        /// <summary>
        /// CSS 類別（根據幣別不同顯示不同顏色）
        /// </summary>
        public string BadgeClass => Currency.ToLowerInvariant() switch
        {
            "eth" => "badge-primary",
            "btc" => "badge-warning",
            "matic" => "badge-secondary",
            "bnb" => "badge-accent",
            "sol" => "badge-info",
            "usdt" or "usdc" => "badge-success",
            _ => "badge-neutral"
        };

        /// <summary>
        /// 幣別圖示
        /// </summary>
        public string Icon => Currency.ToLowerInvariant() switch
        {
            "eth" => "🔷",
            "btc" => "₿",
            "matic" => "🔷",
            "bnb" => "🟡",
            "sol" => "🟣",
            "usdt" or "usdc" => "💵",
            _ => "🪙"
        };
    }

    /// <summary>
    /// NFT 統計資訊的 ViewModel
    /// </summary>
    public class NFTStatsViewModel
    {
        /// <summary>
        /// 擁有者 ID
        /// </summary>
        public Guid OwnerId { get; set; }

        /// <summary>
        /// NFT 總數
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// 總價值
        /// </summary>
        public decimal TotalValue { get; set; }

        /// <summary>
        /// 各幣別統計
        /// </summary>
        public List<CurrencyStatsViewModel> CurrencyStats { get; set; } = new();

        /// <summary>
        /// 最近收藏的 NFT 時間
        /// </summary>
        public DateTime? LastCollectTime { get; set; }

        /// <summary>
        /// 最高價值的 NFT
        /// </summary>
        public NFTItemViewModel? MostExpensive { get; set; }

        /// <summary>
        /// 格式化的總價值
        /// </summary>
        public string FormattedTotalValue => $"{TotalValue:F4}";

        /// <summary>
        /// 平均 NFT 價值
        /// </summary>
        public decimal AverageValue => TotalCount > 0 ? TotalValue / TotalCount : 0;

        /// <summary>
        /// 格式化的平均價值
        /// </summary>
        public string FormattedAverageValue => $"{AverageValue:F4}";

        /// <summary>
        /// 最近收藏時間的相對顯示
        /// </summary>
        public string? LastCollectRelativeTime
        {
            get
            {
                if (!LastCollectTime.HasValue) return null;
                
                var timeSpan = DateTime.Now - LastCollectTime.Value;
                
                if (timeSpan.TotalDays >= 365)
                {
                    int years = (int)(timeSpan.TotalDays / 365);
                    return $"{years} 年前";
                }
                else if (timeSpan.TotalDays >= 30)
                {
                    int months = (int)(timeSpan.TotalDays / 30);
                    return $"{months} 個月前";
                }
                else if (timeSpan.TotalDays >= 1)
                {
                    int days = (int)timeSpan.TotalDays;
                    return $"{days} 天前";
                }
                else if (timeSpan.TotalHours >= 1)
                {
                    int hours = (int)timeSpan.TotalHours;
                    return $"{hours} 小時前";
                }
                else if (timeSpan.TotalMinutes >= 1)
                {
                    int minutes = (int)timeSpan.TotalMinutes;
                    return $"{minutes} 分鐘前";
                }
                else
                {
                    return "剛剛";
                }
            }
        }

        /// <summary>
        /// 是否為活躍收藏者（近30天內有收藏）
        /// </summary>
        public bool IsActiveCollector => LastCollectTime.HasValue && 
                                        (DateTime.Now - LastCollectTime.Value).TotalDays <= 30;

        /// <summary>
        /// 收藏等級（根據NFT數量）
        /// </summary>
        public string CollectorLevel => TotalCount switch
        {
            >= 100 => "鑽石收藏家",
            >= 50 => "白金收藏家",
            >= 20 => "黃金收藏家",
            >= 10 => "白銀收藏家",
            >= 5 => "青銅收藏家",
            >= 1 => "新手收藏家",
            _ => "訪客"
        };

        /// <summary>
        /// 收藏等級對應的圖示
        /// </summary>
        public string CollectorLevelIcon => TotalCount switch
        {
            >= 100 => "💎",
            >= 50 => "🏆",
            >= 20 => "🥇",
            >= 10 => "🥈",
            >= 5 => "🥉",
            >= 1 => "🌟",
            _ => "👀"
        };

        /// <summary>
        /// 收藏等級對應的 CSS 類別
        /// </summary>
        public string CollectorLevelClass => TotalCount switch
        {
            >= 100 => "text-primary font-bold",
            >= 50 => "text-secondary font-bold",
            >= 20 => "text-warning font-semibold",
            >= 10 => "text-info font-semibold",
            >= 5 => "text-success",
            >= 1 => "text-base-content",
            _ => "text-base-content opacity-60"
        };
    }

    /// <summary>
    /// NFT 搜尋結果的 ViewModel
    /// </summary>
    public class NFTSearchResultViewModel
    {
        /// <summary>
        /// NFT 清單
        /// </summary>
        public List<NFTItemViewModel> NFTs { get; set; } = new();

        /// <summary>
        /// 搜尋參數
        /// </summary>
        public NFTSearchDto SearchParams { get; set; } = new();

        /// <summary>
        /// 總筆數
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// 總頁數
        /// </summary>
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / SearchParams.PageSize);

        /// <summary>
        /// 是否有上一頁
        /// </summary>
        public bool HasPreviousPage => SearchParams.Page > 1;

        /// <summary>
        /// 是否有下一頁
        /// </summary>
        public bool HasNextPage => SearchParams.Page < TotalPages;

        /// <summary>
        /// 上一頁頁數
        /// </summary>
        public int PreviousPage => Math.Max(1, SearchParams.Page - 1);

        /// <summary>
        /// 下一頁頁數
        /// </summary>
        public int NextPage => Math.Min(TotalPages, SearchParams.Page + 1);

        /// <summary>
        /// 分頁資訊文字
        /// </summary>
        public string PaginationInfo => $"第 {SearchParams.Page} 頁，共 {TotalPages} 頁，總計 {TotalCount} 筆";
    }
}