using System;
using Microsoft.EntityFrameworkCore;
using Matrix.Data;
using Matrix.DTOs;
using Matrix.Models;

namespace Matrix.Services;

/// <summary>
/// 文章服務類別
/// 
/// 此服務負責處理與文章相關的業務邏輯，包括：
/// - 文章的 CRUD 操作
/// - 文章的讚數和收藏數管理
/// - 文章的搜尋和篩選
/// - 文章的狀態管理
/// - 文章的回覆管理
/// 
/// 注意事項：
/// - 所有方法都應該包含適當的錯誤處理
/// - 包含完整的資料驗證邏輯
/// - 支援非同步操作以提高效能
/// - 考慮使用者權限和隱私設定
/// </summary>
public class ArticleService
{
    private readonly ApplicationDbContext _context;
    
    /// <summary>
    /// 建構函式
    /// 用途：初始化服務並注入資料庫上下文
    /// </summary>
    /// <param name="context">資料庫上下文</param>
    public ArticleService(ApplicationDbContext context)
    {
        _context = context;
    }
    
    /// <summary>
    /// 根據 ID 獲取文章資料
    /// 用途：查詢特定文章的完整資料
    /// </summary>
    /// <param name="id">文章 ID</param>
    /// <returns>文章資料傳輸物件，如果不存在則返回 null</returns>
    public async Task<ArticleDto?> GetArticleAsync(Guid id)
    {
        try
        {
            var article = await _context.Articles
                .Include(a => a.Author)
                .Include(a => a.Replies)
                    .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(a => a.ArticleId == id);
            
            if (article == null) return null;
            
            return new ArticleDto
            {
                ArticleId = article.ArticleId,
                AuthorId = article.AuthorId,
                Content = article.Content,
                IsPublic = article.IsPublic,
                Status = article.Status,
                CreateTime = article.CreateTime,
                PraiseCount = article.PraiseCount,
                CollectCount = article.CollectCount,
                Author = article.Author != null ? new PersonDto
                {
                    PersonId = article.Author.PersonId,
                    UserId = article.Author.UserId,
                    DisplayName = article.Author.DisplayName,
                    Bio = article.Author.Bio,
                    AvatarPath = article.Author.AvatarPath,
                    BannerPath = article.Author.BannerPath,
                    ExternalUrl = article.Author.ExternalUrl,
                    IsPrivate = article.Author.IsPrivate,
                    WalletAddress = article.Author.WalletAddress,
                    ModifyTime = article.Author.ModifyTime
                } : null,
                Replies = article.Replies.Select(r => new ReplyDto
                {
                    ReplyId = r.ReplyId,
                    ArticleId = r.ArticleId,
                    AuthorId = r.UserId,
                    ParentReplyId = null, // Reply 模型沒有這個欄位
                    Content = r.Content,
                    Status = 0, // 預設為正常狀態
                    CreateTime = r.ReplyTime,
                    PraiseCount = 0, // Reply 模型沒有這個欄位，預設為 0
                    Author = r.User != null ? new PersonDto
                    {
                        PersonId = r.User.PersonId,
                        UserId = r.User.UserId,
                        DisplayName = r.User.DisplayName,
                        AvatarPath = r.User.AvatarPath,
                        IsPrivate = r.User.IsPrivate
                    } : null
                }).ToList()
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"獲取文章資料時發生錯誤: {ex.Message}");
            throw new Exception("獲取文章資料失敗", ex);
        }
    }
    
    /// <summary>
    /// 建立新文章
    /// 用途：建立新的文章
    /// </summary>
    /// <param name="dto">建立文章的資料傳輸物件</param>
    /// <param name="authorId">作者 ID</param>
    /// <returns>是否建立成功</returns>
    public async Task<bool> CreateArticleAsync(CreateArticleDto dto, Guid authorId)
    {
        try
        {
            // 驗證輸入資料
            if (!dto.IsValid)
            {
                throw new ArgumentException("輸入資料無效");
            }
            
            // 檢查作者是否存在
            var author = await _context.Persons.FirstOrDefaultAsync(p => p.UserId == authorId);
            if (author == null)
            {
                throw new InvalidOperationException("找不到指定的作者");
            }
            
            // 建立文章實體
            var article = new Article
            {
                ArticleId = Guid.NewGuid(),
                AuthorId = author.PersonId,
                Content = dto.Content,
                IsPublic = dto.IsPublic,
                Status = 0, // 預設為正常狀態
                CreateTime = DateTime.Now,
                PraiseCount = 0,
                CollectCount = 0,
                Author = author
            };
            
            _context.Articles.Add(article);
            await _context.SaveChangesAsync();
            
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"建立文章時發生錯誤: {ex.Message}");
            throw new Exception("建立文章失敗", ex);
        }
    }
    
    /// <summary>
    /// 更新文章內容
    /// 用途：更新現有文章的內容
    /// </summary>
    /// <param name="id">文章 ID</param>
    /// <param name="content">新的文章內容</param>
    /// <param name="isPublic">是否公開</param>
    /// <param name="authorId">作者 ID（用於權限檢查）</param>
    /// <returns>是否更新成功</returns>
    public async Task<bool> UpdateArticleAsync(Guid id, string content, int isPublic, Guid authorId)
    {
        try
        {
            var article = await _context.Articles
                .Include(a => a.Author)
                .FirstOrDefaultAsync(a => a.ArticleId == id);
            
            if (article == null)
            {
                throw new InvalidOperationException("找不到指定的文章");
            }
            
            // 檢查權限：只有作者可以更新文章
            if (article.Author.UserId != authorId)
            {
                throw new UnauthorizedAccessException("沒有權限更新此文章");
            }
            
            // 驗證內容
            if (string.IsNullOrWhiteSpace(content) || content.Length > 4000)
            {
                throw new ArgumentException("文章內容無效");
            }
            
            // 更新文章
            article.Content = content;
            article.IsPublic = isPublic;
            
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"更新文章時發生錯誤: {ex.Message}");
            throw new Exception("更新文章失敗", ex);
        }
    }
    
    /// <summary>
    /// 刪除文章
    /// 用途：軟刪除文章（設定狀態為已刪除）
    /// </summary>
    /// <param name="id">文章 ID</param>
    /// <param name="authorId">作者 ID（用於權限檢查）</param>
    /// <returns>是否刪除成功</returns>
    public async Task<bool> DeleteArticleAsync(Guid id, Guid authorId)
    {
        try
        {
            var article = await _context.Articles
                .Include(a => a.Author)
                .FirstOrDefaultAsync(a => a.ArticleId == id);
            
            if (article == null)
            {
                throw new InvalidOperationException("找不到指定的文章");
            }
            
            // 檢查權限：只有作者可以刪除文章
            if (article.Author.UserId != authorId)
            {
                throw new UnauthorizedAccessException("沒有權限刪除此文章");
            }
            
            // 軟刪除：設定狀態為已刪除
            article.Status = 2; // 2 = 已刪除狀態
            
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"刪除文章時發生錯誤: {ex.Message}");
            throw new Exception("刪除文章失敗", ex);
        }
    }
    
    /// <summary>
    /// 獲取文章列表
    /// 用途：分頁查詢文章列表
    /// </summary>
    /// <param name="page">頁碼</param>
    /// <param name="pageSize">每頁數量</param>
    /// <param name="searchKeyword">搜尋關鍵字</param>
    /// <param name="authorId">作者 ID（選填）</param>
    /// <returns>文章列表和總數</returns>
    public async Task<(List<ArticleDto> Articles, int TotalCount)> GetArticlesAsync(
        int page = 1, 
        int pageSize = 20, 
        string? searchKeyword = null, 
        Guid? authorId = null)
    {
        try
        {
            var query = _context.Articles
                .Include(a => a.Author)
                .AsQueryable();
            
            // 只查詢正常狀態和公開的文章
            query = query.Where(a => a.Status == 0 && a.IsPublic == 0);
            
            // 如果指定了作者，過濾作者
            if (authorId.HasValue)
            {
                query = query.Where(a => a.Author.UserId == authorId.Value);
            }
            
            // 如果有搜尋關鍵字，進行搜尋
            if (!string.IsNullOrEmpty(searchKeyword))
            {
                query = query.Where(a => a.Content.Contains(searchKeyword) ||
                                       (a.Author.DisplayName != null && a.Author.DisplayName.Contains(searchKeyword)));
            }
            
            var totalCount = await query.CountAsync();
            
            var articles = await query
                .OrderByDescending(a => a.CreateTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new ArticleDto
                {
                    ArticleId = a.ArticleId,
                    AuthorId = a.AuthorId,
                    Content = a.Content,
                    IsPublic = a.IsPublic,
                    Status = a.Status,
                    CreateTime = a.CreateTime,
                    PraiseCount = a.PraiseCount,
                    CollectCount = a.CollectCount,
                    Author = a.Author != null ? new PersonDto
                    {
                        PersonId = a.Author.PersonId,
                        UserId = a.Author.UserId,
                        DisplayName = a.Author.DisplayName,
                        AvatarPath = a.Author.AvatarPath,
                        IsPrivate = a.Author.IsPrivate
                    } : null
                })
                .ToListAsync();
            
            return (articles, totalCount);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"獲取文章列表時發生錯誤: {ex.Message}");
            throw new Exception("獲取文章列表失敗", ex);
        }
    }
    
    /// <summary>
    /// 增加文章讚數
    /// 用途：使用者對文章按讚
    /// </summary>
    /// <param name="articleId">文章 ID</param>
    /// <returns>是否操作成功</returns>
    public async Task<bool> IncreasePraiseCountAsync(Guid articleId)
    {
        try
        {
            var article = await _context.Articles.FirstOrDefaultAsync(a => a.ArticleId == articleId);
            
            if (article == null)
            {
                throw new InvalidOperationException("找不到指定的文章");
            }
            
            article.PraiseCount++;
            await _context.SaveChangesAsync();
            
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"增加文章讚數時發生錯誤: {ex.Message}");
            throw new Exception("增加文章讚數失敗", ex);
        }
    }
    
    /// <summary>
    /// 減少文章讚數
    /// 用途：使用者取消對文章的讚
    /// </summary>
    /// <param name="articleId">文章 ID</param>
    /// <returns>是否操作成功</returns>
    public async Task<bool> DecreasePraiseCountAsync(Guid articleId)
    {
        try
        {
            var article = await _context.Articles.FirstOrDefaultAsync(a => a.ArticleId == articleId);
            
            if (article == null)
            {
                throw new InvalidOperationException("找不到指定的文章");
            }
            
            if (article.PraiseCount > 0)
            {
                article.PraiseCount--;
                await _context.SaveChangesAsync();
            }
            
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"減少文章讚數時發生錯誤: {ex.Message}");
            throw new Exception("減少文章讚數失敗", ex);
        }
    }
    
    /// <summary>
    /// 增加文章收藏數
    /// 用途：使用者收藏文章
    /// </summary>
    /// <param name="articleId">文章 ID</param>
    /// <returns>是否操作成功</returns>
    public async Task<bool> IncreaseCollectCountAsync(Guid articleId)
    {
        try
        {
            var article = await _context.Articles.FirstOrDefaultAsync(a => a.ArticleId == articleId);
            
            if (article == null)
            {
                throw new InvalidOperationException("找不到指定的文章");
            }
            
            article.CollectCount++;
            await _context.SaveChangesAsync();
            
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"增加文章收藏數時發生錯誤: {ex.Message}");
            throw new Exception("增加文章收藏數失敗", ex);
        }
    }
    
    /// <summary>
    /// 減少文章收藏數
    /// 用途：使用者取消收藏文章
    /// </summary>
    /// <param name="articleId">文章 ID</param>
    /// <returns>是否操作成功</returns>
    public async Task<bool> DecreaseCollectCountAsync(Guid articleId)
    {
        try
        {
            var article = await _context.Articles.FirstOrDefaultAsync(a => a.ArticleId == articleId);
            
            if (article == null)
            {
                throw new InvalidOperationException("找不到指定的文章");
            }
            
            if (article.CollectCount > 0)
            {
                article.CollectCount--;
                await _context.SaveChangesAsync();
            }
            
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"減少文章收藏數時發生錯誤: {ex.Message}");
            throw new Exception("減少文章收藏數失敗", ex);
        }
    }
    
    /// <summary>
    /// 新增文章回覆
    /// 用途：對文章進行回覆
    /// </summary>
    /// <param name="dto">建立回覆的資料傳輸物件</param>
    /// <param name="authorId">回覆者 ID</param>
    /// <returns>是否建立成功</returns>
    public async Task<bool> CreateReplyAsync(CreateReplyDto dto, Guid authorId)
    {
        try
        {
            // 驗證輸入資料
            var (isValid, errors) = dto.ValidateForCreation();
            if (!isValid)
            {
                throw new ArgumentException($"輸入資料無效: {string.Join(", ", errors)}");
            }
            
            // 檢查文章是否存在
            var article = await _context.Articles.FirstOrDefaultAsync(a => a.ArticleId == dto.ArticleId);
            if (article == null)
            {
                throw new InvalidOperationException("找不到指定的文章");
            }
            
            // 檢查回覆者是否存在
            var author = await _context.Persons.FirstOrDefaultAsync(p => p.UserId == authorId);
            if (author == null)
            {
                throw new InvalidOperationException("找不到指定的回覆者");
            }
            
            // 建立回覆實體
            var reply = new Reply
            {
                ReplyId = Guid.NewGuid(),
                ArticleId = dto.ArticleId,
                UserId = authorId,
                Content = dto.Content,
                ReplyTime = DateTime.Now,
                User = author
            };
            
            _context.Replies.Add(reply);
            await _context.SaveChangesAsync();
            
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"建立回覆時發生錯誤: {ex.Message}");
            throw new Exception("建立回覆失敗", ex);
        }
    }
    
    /// <summary>
    /// 更新文章狀態
    /// 用途：更新文章的顯示狀態
    /// </summary>
    /// <param name="id">文章 ID</param>
    /// <param name="status">新狀態 (0=正常, 1=隱藏, 2=已刪除)</param>
    /// <returns>是否更新成功</returns>
    public async Task<bool> UpdateArticleStatusAsync(Guid id, int status)
    {
        try
        {
            var article = await _context.Articles.FirstOrDefaultAsync(a => a.ArticleId == id);
            
            if (article == null)
            {
                throw new InvalidOperationException("找不到指定的文章");
            }
            
            if (status < 0 || status > 2)
            {
                throw new ArgumentException("狀態值無效");
            }
            
            article.Status = status;
            await _context.SaveChangesAsync();
            
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"更新文章狀態時發生錯誤: {ex.Message}");
            throw new Exception("更新文章狀態失敗", ex);
        }
    }
    
    /// <summary>
    /// 獲取熱門文章
    /// 用途：根據讚數和收藏數獲取熱門文章
    /// </summary>
    /// <param name="limit">數量限制</param>
    /// <param name="days">天數範圍</param>
    /// <returns>熱門文章列表</returns>
    public async Task<List<ArticleDto>> GetPopularArticlesAsync(int limit = 10, int days = 7)
    {
        try
        {
            var sinceDate = DateTime.Now.AddDays(-days);
            
            var articles = await _context.Articles
                .Include(a => a.Author)
                .Where(a => a.Status == 0 && a.IsPublic == 0 && a.CreateTime >= sinceDate)
                .OrderByDescending(a => a.PraiseCount + a.CollectCount)
                .Take(limit)
                .Select(a => new ArticleDto
                {
                    ArticleId = a.ArticleId,
                    AuthorId = a.AuthorId,
                    Content = a.Content,
                    IsPublic = a.IsPublic,
                    Status = a.Status,
                    CreateTime = a.CreateTime,
                    PraiseCount = a.PraiseCount,
                    CollectCount = a.CollectCount,
                    Author = a.Author != null ? new PersonDto
                    {
                        PersonId = a.Author.PersonId,
                        UserId = a.Author.UserId,
                        DisplayName = a.Author.DisplayName,
                        AvatarPath = a.Author.AvatarPath,
                        IsPrivate = a.Author.IsPrivate
                    } : null
                })
                .ToListAsync();
            
            return articles;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"獲取熱門文章時發生錯誤: {ex.Message}");
            throw new Exception("獲取熱門文章失敗", ex);
        }
    }
}
