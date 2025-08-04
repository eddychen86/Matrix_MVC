
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Matrix.Services.Interfaces
{
    public interface IFileService
    {
        /// <summary>
        /// 儲存一個新檔案，並回傳其相對路徑。
        /// </summary>
        /// <param name="file">要儲存的 IFormFile 物件。</param>
        /// <param name="subfolder">儲存的目標子資料夾 (例如 "profile/imgs")。</param>
        /// <returns>成功後的相對路徑，若失敗則回傳 null。</returns>
        Task<string?> CreateFileAsync(IFormFile file, string subfolder);

        /// <summary>
        /// 刪除一個指定相對路徑的檔案。
        /// </summary>
        /// <param name="relativePath">要刪除檔案的相對路徑。</param>
        /// <returns>操作是否成功。</returns>
        Task<bool> DeleteFileAsync(string? relativePath);

        /// <summary>
        /// 更新一個檔案。此方法會先刪除舊檔案，再儲存新檔案。
        /// </summary>
        /// <param name="newFile">要儲存的新 IFormFile 物件。</param>
        /// <param name="oldRelativePath">舊檔案的相對路徑，用於刪除。</param>
        /// <param name="subfolder">儲存的目標子資料夾。</param>
        /// <returns>新檔案的相對路徑，若無新檔案可存則回傳 null。</returns>
        Task<string?> UpdateFileAsync(IFormFile newFile, string? oldRelativePath, string subfolder);
    }
}
