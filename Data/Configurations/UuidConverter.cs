using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Matrix.Models;

namespace Matrix.Data.Configurations
{
    /// <summary>
    /// 提供 UUID 與 Guid 之間的轉換功能，讓 EF Core 能夠正確處理自訂的 UUID 型別。
    /// </summary>
    public class UuidConverter : ValueConverter<UUID, Guid>
    {
        /// <summary>
        /// 初始化 UuidConverter，定義 UUID 到 Guid 以及 Guid 到 UUID 的轉換邏輯。
        /// </summary>
        public UuidConverter() : base(
            // 定義如何將 UUID 物件轉換為 Guid 以便存入資料庫
            uuid => uuid.ToGuid(),
            // 定義如何將從資料庫讀取的 Guid 轉換回 UUID 物件
            guid => UUID.FromGuid(guid))
        {
        }
    }
}
