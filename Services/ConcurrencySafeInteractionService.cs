using Matrix.Models;
using Matrix.Repository.Interfaces;
using Matrix.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Transactions;

namespace Matrix.Services
{
    /// <summary>
    /// 併發安全的互動服務，處理按讚、收藏、追蹤等操作
    /// 防止多用戶同時操作造成的資料競爭問題
    /// </summary>
    public class ConcurrencySafeInteractionService
    {
        private readonly IArticleRepository _articleRepository;
        private readonly IPraiseCollectRepository _praiseCollectRepository;
        private readonly IFollowRepository _followRepository;
        private readonly ISignalRService _signalRService;
        private readonly ILogger<ConcurrencySafeInteractionService> _logger;

        public ConcurrencySafeInteractionService(
            IArticleRepository articleRepository,
            IPraiseCollectRepository praiseCollectRepository,
            IFollowRepository followRepository,
            ISignalRService signalRService,
            ILogger<ConcurrencySafeInteractionService> logger)
        {
            _articleRepository = articleRepository;
            _praiseCollectRepository = praiseCollectRepository;
            _followRepository = followRepository;
            _signalRService = signalRService;
            _logger = logger;
        }

        #region 按讚功能（併發安全）

        /// <summary>
        /// 切換按讚狀態（併發安全）
        /// </summary>
        public async Task<(bool Success, bool IsLiked, int NewCount)> TogglePraiseAsync(Guid userId, Guid articleId)
        {
            const int maxRetries = 3;
            
            for (int attempt = 0; attempt < maxRetries; attempt++)
            {
                try
                {
                    using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
                    
                    // 檢查是否已按讚
                    var existingPraise = await _praiseCollectRepository.GetAsync(
                        pc => pc.UserId == userId && pc.ArticleId == articleId && pc.Type == 0);

                    bool isLiked;
                    bool updateResult;

                    if (existingPraise == null)
                    {
                        // 新增按讚
                        var newPraise = new PraiseCollect
                        {
                            UserId = userId,
                            ArticleId = articleId,
                            Type = 0, // 按讚類型
                            CreateTime = DateTime.UtcNow
                        };
                        
                        await _praiseCollectRepository.AddAsync(newPraise);
                        updateResult = await _articleRepository.IncreasePraiseCountAtomicAsync(articleId);
                        isLiked = true;
                    }
                    else
                    {
                        // 取消按讚
                        await _praiseCollectRepository.DeleteAsync(existingPraise);
                        updateResult = await _articleRepository.DecreasePraiseCountAtomicAsync(articleId);
                        isLiked = false;
                    }

                    if (!updateResult)
                    {
                        throw new InvalidOperationException("更新文章按讚數失敗");
                    }

                    transaction.Complete();

                    // 獲取最新計數
                    var article = await _articleRepository.GetByIdAsync(articleId);
                    var newCount = article?.PraiseCount ?? 0;

                    // 發送 SignalR 即時更新
                    await _signalRService.BroadcastInteractionUpdateAsync(
                        articleId, 
                        "praise", 
                        newCount, 
                        article?.AuthorId // 通知文章作者
                    );

                    _logger.LogInformation("用戶 {UserId} {Action} 文章 {ArticleId}，新計數：{Count}", 
                        userId, isLiked ? "按讚" : "取消按讚", articleId, newCount);

                    return (true, isLiked, newCount);
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    _logger.LogWarning("按讚操作併發衝突，嘗試 {Attempt}/{MaxRetries}：{Error}", 
                        attempt + 1, maxRetries, ex.Message);
                    
                    if (attempt == maxRetries - 1)
                        throw new InvalidOperationException("按讚操作失敗，請稍後重試", ex);
                    
                    await Task.Delay(100 * (attempt + 1)); // 指數延遲
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "按讚操作發生錯誤：UserId={UserId}, ArticleId={ArticleId}", userId, articleId);
                    throw;
                }
            }

            return (false, false, 0);
        }

        #endregion

        #region 收藏功能（併發安全）

        /// <summary>
        /// 切換收藏狀態（併發安全）
        /// </summary>
        public async Task<(bool Success, bool IsCollected, int NewCount)> ToggleCollectAsync(Guid userId, Guid articleId)
        {
            const int maxRetries = 3;
            
            for (int attempt = 0; attempt < maxRetries; attempt++)
            {
                try
                {
                    using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
                    
                    // 檢查是否已收藏
                    var existingCollect = await _praiseCollectRepository.GetAsync(
                        pc => pc.UserId == userId && pc.ArticleId == articleId && pc.Type == 1);

                    bool isCollected;
                    bool updateResult;

                    if (existingCollect == null)
                    {
                        // 新增收藏
                        var newCollect = new PraiseCollect
                        {
                            UserId = userId,
                            ArticleId = articleId,
                            Type = 1, // 收藏類型
                            CreateTime = DateTime.UtcNow
                        };
                        
                        await _praiseCollectRepository.AddAsync(newCollect);
                        updateResult = await _articleRepository.IncreaseCollectCountAtomicAsync(articleId);
                        isCollected = true;
                    }
                    else
                    {
                        // 取消收藏
                        await _praiseCollectRepository.DeleteAsync(existingCollect);
                        updateResult = await _articleRepository.DecreaseCollectCountAtomicAsync(articleId);
                        isCollected = false;
                    }

                    if (!updateResult)
                    {
                        throw new InvalidOperationException("更新文章收藏數失敗");
                    }

                    transaction.Complete();

                    // 獲取最新計數
                    var article = await _articleRepository.GetByIdAsync(articleId);
                    var newCount = article?.CollectCount ?? 0;

                    // 發送 SignalR 即時更新
                    await _signalRService.BroadcastInteractionUpdateAsync(
                        articleId, 
                        "collect", 
                        newCount, 
                        article?.AuthorId // 通知文章作者
                    );

                    _logger.LogInformation("用戶 {UserId} {Action} 文章 {ArticleId}，新計數：{Count}", 
                        userId, isCollected ? "收藏" : "取消收藏", articleId, newCount);

                    return (true, isCollected, newCount);
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    _logger.LogWarning("收藏操作併發衝突，嘗試 {Attempt}/{MaxRetries}：{Error}", 
                        attempt + 1, maxRetries, ex.Message);
                    
                    if (attempt == maxRetries - 1)
                        throw new InvalidOperationException("收藏操作失敗，請稍後重試", ex);
                    
                    await Task.Delay(100 * (attempt + 1)); // 指數延遲
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "收藏操作發生錯誤：UserId={UserId}, ArticleId={ArticleId}", userId, articleId);
                    throw;
                }
            }

            return (false, false, 0);
        }

        #endregion

        #region 追蹤功能（併發安全）

        /// <summary>
        /// 切換追蹤狀態（併發安全）
        /// </summary>
        public async Task<(bool Success, bool IsFollowing)> ToggleFollowAsync(Guid followerId, Guid followedId)
        {
            if (followerId == followedId)
            {
                throw new InvalidOperationException("無法追蹤自己");
            }

            const int maxRetries = 3;
            
            for (int attempt = 0; attempt < maxRetries; attempt++)
            {
                try
                {
                    using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
                    
                    // 檢查是否已追蹤
                    var existingFollow = await _followRepository.GetFollowRelationAsync(followerId, followedId);

                    bool isFollowing;

                    if (existingFollow == null)
                    {
                        // 新增追蹤
                        var newFollow = new Follow
                        {
                            UserId = followerId,
                            FollowedId = followedId,
                            Type = 1, // 追蹤用戶類型
                            FollowTime = DateTime.UtcNow,
                            User = null! // 會由 EF Core 自動設定
                        };
                        
                        await _followRepository.AddAsync(newFollow);
                        isFollowing = true;
                    }
                    else
                    {
                        // 取消追蹤
                        await _followRepository.DeleteAsync(existingFollow);
                        isFollowing = false;
                    }

                    transaction.Complete();

                    // 發送 SignalR 即時更新
                    await _signalRService.BroadcastFollowUpdateAsync(followerId, followedId, isFollowing);

                    _logger.LogInformation("用戶 {FollowerId} {Action} 用戶 {FollowedId}", 
                        followerId, isFollowing ? "追蹤" : "取消追蹤", followedId);

                    return (true, isFollowing);
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    _logger.LogWarning("追蹤操作併發衝突，嘗試 {Attempt}/{MaxRetries}：{Error}", 
                        attempt + 1, maxRetries, ex.Message);
                    
                    if (attempt == maxRetries - 1)
                        throw new InvalidOperationException("追蹤操作失敗，請稍後重試", ex);
                    
                    await Task.Delay(100 * (attempt + 1)); // 指數延遲
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "追蹤操作發生錯誤：FollowerId={FollowerId}, FollowedId={FollowedId}", 
                        followerId, followedId);
                    throw;
                }
            }

            return (false, false);
        }

        #endregion

        #region 批量操作（高效能）

        /// <summary>
        /// 批量處理多個互動操作（提高效能）
        /// </summary>
        public async Task<Dictionary<Guid, (bool Success, string Action, int NewCount)>> BatchProcessInteractionsAsync(
            List<(Guid UserId, Guid ArticleId, string Action)> interactions)
        {
            var results = new Dictionary<Guid, (bool Success, string Action, int NewCount)>();
            
            // 按文章分組以減少併發衝突
            var groupedByArticle = interactions.GroupBy(i => i.ArticleId);
            
            var tasks = groupedByArticle.Select(async group =>
            {
                var articleId = group.Key;
                foreach (var interaction in group)
                {
                    try
                    {
                        bool success = false;
                        int newCount = 0;
                        
                        if (interaction.Action.ToLower() == "praise")
                        {
                            var result = await TogglePraiseAsync(interaction.UserId, interaction.ArticleId);
                            success = result.Success;
                            newCount = result.NewCount;
                        }
                        else if (interaction.Action.ToLower() == "collect")
                        {
                            var result = await ToggleCollectAsync(interaction.UserId, interaction.ArticleId);
                            success = result.Success;
                            newCount = result.NewCount;
                        }
                        else
                        {
                            throw new ArgumentException($"未知的操作類型：{interaction.Action}");
                        }
                        
                        results[articleId] = (success, interaction.Action, newCount);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "批量操作失敗：{Interaction}", interaction);
                        results[articleId] = (false, interaction.Action, 0);
                    }
                }
            });
            
            await Task.WhenAll(tasks);
            return results;
        }

        #endregion
    }
}