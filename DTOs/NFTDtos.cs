using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Matrix.DTOs
{
    /// <summary>
    /// NFT 基本資料傳輸物件
    /// </summary>
    public class NFTDto
    {
        /// <summary>
        /// NFT ID
        /// </summary>
        public Guid NftId { get; set; }

        /// <summary>
        /// 擁有者 ID
        /// </summary>
        public Guid OwnerId { get; set; }

        /// <summary>
        /// NFT 名稱
        /// </summary>
        public string FileName { get; set; } = "";

        /// <summary>
        /// 檔案儲存路徑
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
        /// 擁有者顯示名稱
        /// </summary>
        public string? OwnerDisplayName { get; set; }

        /// <summary>
        /// 擁有者頭像路徑
        /// </summary>
        public string? OwnerAvatarPath { get; set; }

        /// <summary>
        /// 格式化的價格顯示
        /// </summary>
        [JsonIgnore]
        public string FormattedPrice => $"{Price:F4} {Currency}";

        /// <summary>
        /// 格式化的收藏日期
        /// </summary>
        [JsonIgnore]
        public string FormattedDate => CollectTime.ToString("yyyy-MM-dd");

        /// <summary>
        /// 格式化的收藏時間
        /// </summary>
        [JsonIgnore]
        public string FormattedDateTime => CollectTime.ToString("yyyy-MM-dd HH:mm");
    }

    /// <summary>
    /// 建立 NFT 的資料傳輸物件
    /// </summary>
    public class CreateNFTDto
    {
        /// <summary>
        /// 擁有者 ID
        /// </summary>
        [Required(ErrorMessage = "NFT_OwnerIdRequired")]
        public Guid OwnerId { get; set; }

        /// <summary>
        /// NFT 名稱
        /// </summary>
        [Required(ErrorMessage = "NFT_FileNameRequired")]
        [StringLength(255, ErrorMessage = "NFT_FileNameMaxLength255")]
        public string FileName { get; set; } = "";

        /// <summary>
        /// 檔案儲存路徑
        /// </summary>
        [Required(ErrorMessage = "NFT_FilePathRequired")]
        [StringLength(2048, ErrorMessage = "NFT_FilePathMaxLength2048")]
        public string FilePath { get; set; } = "";

        /// <summary>
        /// 收藏時間
        /// </summary>
        [Required(ErrorMessage = "NFT_CollectTimeRequired")]
        public DateTime CollectTime { get; set; }

        /// <summary>
        /// 幣別
        /// </summary>
        [Required(ErrorMessage = "NFT_CurrencyRequired")]
        [StringLength(10, ErrorMessage = "NFT_CurrencyMaxLength10")]
        public string Currency { get; set; } = "";

        /// <summary>
        /// 價格
        /// </summary>
        [Range(0, double.MaxValue, ErrorMessage = "NFT_PriceMin0")]
        public decimal Price { get; set; }

        /// <summary>
        /// 驗證並清理資料
        /// </summary>
        public void Sanitize()
        {
            FileName = FileName?.Trim() ?? "";
            FilePath = FilePath?.Trim() ?? "";
            Currency = Currency?.Trim().ToUpperInvariant() ?? "";
        }

        /// <summary>
        /// 檢查是否為有效的資料
        /// </summary>
        /// <returns>是否有效</returns>
        public bool IsValid()
        {
            return OwnerId != Guid.Empty &&
                   !string.IsNullOrEmpty(FileName) &&
                   !string.IsNullOrEmpty(FilePath) &&
                   !string.IsNullOrEmpty(Currency) &&
                   Price >= 0;
        }
    }

    /// <summary>
    /// 更新 NFT 的資料傳輸物件
    /// </summary>
    public class UpdateNFTDto
    {
        /// <summary>
        /// NFT 名稱
        /// </summary>
        [StringLength(255, ErrorMessage = "NFT_FileNameMaxLength255")]
        public string? FileName { get; set; }

        /// <summary>
        /// 檔案儲存路徑
        /// </summary>
        [StringLength(2048, ErrorMessage = "NFT_FilePathMaxLength2048")]
        public string? FilePath { get; set; }

        /// <summary>
        /// 收藏時間
        /// </summary>
        public DateTime? CollectTime { get; set; }

        /// <summary>
        /// 幣別
        /// </summary>
        [StringLength(10, ErrorMessage = "NFT_CurrencyMaxLength10")]
        public string? Currency { get; set; }

        /// <summary>
        /// 價格
        /// </summary>
        [Range(0, double.MaxValue, ErrorMessage = "NFT_PriceMin0")]
        public decimal? Price { get; set; }

        /// <summary>
        /// 驗證並清理資料
        /// </summary>
        public void Sanitize()
        {
            FileName = FileName?.Trim();
            FilePath = FilePath?.Trim();
            Currency = Currency?.Trim().ToUpperInvariant();
            
            if (string.IsNullOrEmpty(FileName)) FileName = null;
            if (string.IsNullOrEmpty(FilePath)) FilePath = null;
            if (string.IsNullOrEmpty(Currency)) Currency = null;
        }

        /// <summary>
        /// 檢查是否有資料需要更新
        /// </summary>
        /// <returns>是否有更新資料</returns>
        public bool HasUpdates()
        {
            return !string.IsNullOrEmpty(FileName) ||
                   !string.IsNullOrEmpty(FilePath) ||
                   CollectTime.HasValue ||
                   !string.IsNullOrEmpty(Currency) ||
                   Price.HasValue;
        }
    }

    /// <summary>
    /// NFT 搜尋參數物件
    /// </summary>
    public class NFTSearchDto
    {
        /// <summary>
        /// 擁有者 ID
        /// </summary>
        public Guid OwnerId { get; set; }

        /// <summary>
        /// 檔案名稱關鍵字
        /// </summary>
        public string? FileName { get; set; }

        /// <summary>
        /// 幣別
        /// </summary>
        public string? Currency { get; set; }

        /// <summary>
        /// 最低價格
        /// </summary>
        [Range(0, double.MaxValue, ErrorMessage = "NFT_MinPriceMin0")]
        public decimal? MinPrice { get; set; }

        /// <summary>
        /// 最高價格
        /// </summary>
        [Range(0, double.MaxValue, ErrorMessage = "NFT_MaxPriceMin0")]
        public decimal? MaxPrice { get; set; }

        /// <summary>
        /// 開始日期
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// 結束日期
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// 頁數
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "NFT_PageMin1")]
        public int Page { get; set; } = 1;

        /// <summary>
        /// 每頁筆數
        /// </summary>
        [Range(1, 100, ErrorMessage = "NFT_PageSizeRange1To100")]
        public int PageSize { get; set; } = 20;

        /// <summary>
        /// 排序方式
        /// </summary>
        public NFTSortOrder SortOrder { get; set; } = NFTSortOrder.CollectTimeDesc;

        /// <summary>
        /// 驗證並清理資料
        /// </summary>
        public void Sanitize()
        {
            FileName = FileName?.Trim();
            Currency = Currency?.Trim().ToUpperInvariant();
            
            if (string.IsNullOrEmpty(FileName)) FileName = null;
            if (string.IsNullOrEmpty(Currency)) Currency = null;
            
            if (MinPrice.HasValue && MaxPrice.HasValue && MinPrice > MaxPrice)
            {
                (MinPrice, MaxPrice) = (MaxPrice, MinPrice);
            }
            
            if (StartDate.HasValue && EndDate.HasValue && StartDate > EndDate)
            {
                (StartDate, EndDate) = (EndDate, StartDate);
            }
        }
    }

    /// <summary>
    /// NFT 統計資料物件
    /// </summary>
    public class NFTStatsDto
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
        public List<CurrencyStatsDto> CurrencyStats { get; set; } = new();

        /// <summary>
        /// 最近收藏的 NFT
        /// </summary>
        public DateTime? LastCollectTime { get; set; }

        /// <summary>
        /// 最高價值的 NFT
        /// </summary>
        public NFTDto? MostExpensive { get; set; }
    }

    /// <summary>
    /// 幣別統計資料物件
    /// </summary>
    public class CurrencyStatsDto
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
    }

    /// <summary>
    /// NFT 排序方式
    /// </summary>
    public enum NFTSortOrder
    {
        /// <summary>
        /// 收藏時間降序（最新優先）
        /// </summary>
        CollectTimeDesc = 0,
        
        /// <summary>
        /// 收藏時間升序（最舊優先）
        /// </summary>
        CollectTimeAsc = 1,
        
        /// <summary>
        /// 價格降序（高價優先）
        /// </summary>
        PriceDesc = 2,
        
        /// <summary>
        /// 價格升序（低價優先）
        /// </summary>
        PriceAsc = 3,
        
        /// <summary>
        /// 名稱升序
        /// </summary>
        NameAsc = 4,
        
        /// <summary>
        /// 名稱降序
        /// </summary>
        NameDesc = 5
    }
}
