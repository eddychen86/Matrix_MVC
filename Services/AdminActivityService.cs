using AutoMapper;
using Microsoft.Extensions.Logging;
using Matrix.DTOs;
using Matrix.Models;
using Matrix.Repository.Interfaces;
using Matrix.Services.Interfaces;
using Matrix.Helpers;

namespace Matrix.Services
{
    /// <summary>
    /// 管理員活動記錄服務實作
    /// </summary>
    public class AdminActivityService : IAdminActivityService
    {
        private readonly ILoginRecordRepository _activityRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<AdminActivityService> _logger;

        public AdminActivityService(
            ILoginRecordRepository activityRepository,
            IMapper mapper,
            ILogger<AdminActivityService> logger)
        {
            _activityRepository = activityRepository;
            _mapper = mapper;
            _logger = logger;
        }

        #region 活動記錄操作

        /// <summary>
        /// 記錄管理員活動
        /// </summary>
        public async Task<Guid> LogActivityAsync(CreateActivityLogDto activityDto)
        {
            try
            {
                var activity = _mapper.Map<AdminActivityLog>(activityDto);
                activity.LoginId = Guid.NewGuid();
                activity.ActionTime = TimeZoneHelper.GetTaipeiTime();

                await _activityRepository.LogActivityAsync(activity);
                
                _logger.LogInformation("管理員活動記錄成功: UserId={UserId}, ActionType={ActionType}, ActionDescription={ActionDescription}", 
                    activity.UserId, activity.ActionType, activity.ActionDescription);

                return activity.LoginId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "記錄管理員活動時發生錯誤: UserId={UserId}", activityDto.UserId);
                throw;
            }
        }

        /// <summary>
        /// 記錄管理員登入
        /// </summary>
        public async Task<Guid> LogLoginAsync(Guid userId, string adminName, int role, string ipAddress, string userAgent)
        {
            var activityDto = new CreateActivityLogDto
            {
                UserId = userId,
                AdminName = adminName,
                Role = role,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                LoginTime = TimeZoneHelper.GetTaipeiTime(),
                ActionType = "LOGIN",
                ActionDescription = $"管理員 {adminName} 成功登入系統",
                IsSuccessful = true
            };

            return await LogActivityAsync(activityDto);
        }

        /// <summary>
        /// 記錄管理員登出
        /// </summary>
        public async Task<bool> LogLogoutAsync(Guid userId, string ipAddress)
        {
            try
            {
                // 查找最近的登入記錄並更新登出時間
                var recentLogin = await _activityRepository.GetByUserIdAsync(userId, 1, 1);
                var loginRecord = recentLogin.FirstOrDefault(r => r.ActionType == "LOGIN" && r.IpAddress == ipAddress);

                if (loginRecord != null)
                {
                    var taipeiNow = TimeZoneHelper.GetTaipeiTime();
                    loginRecord.LogoutTime = taipeiNow;
                    loginRecord.Duration = (int)(taipeiNow - (loginRecord.LoginTime ?? taipeiNow)).TotalSeconds;
                    
                    await _activityRepository.UpdateAsync(loginRecord);
                }

                // 記錄登出活動
                var logoutDto = new CreateActivityLogDto
                {
                    UserId = userId,
                    AdminName = loginRecord?.AdminName ?? "Unknown",
                    Role = loginRecord?.Role ?? 1,
                    IpAddress = ipAddress,
                    UserAgent = loginRecord?.UserAgent ?? "",
                    LogoutTime = TimeZoneHelper.GetTaipeiTime(),
                    ActionType = "LOGOUT",
                    ActionDescription = $"管理員成功登出系統",
                    IsSuccessful = true
                };

                await LogActivityAsync(logoutDto);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "記錄管理員登出時發生錯誤: UserId={UserId}", userId);
                return false;
            }
        }

        /// <summary>
        /// 記錄頁面訪問
        /// </summary>
        public async Task<Guid> LogPageVisitAsync(Guid userId, string adminName, int role, string pagePath, 
            string ipAddress, string userAgent, int duration = 0)
        {
            var activityDto = new CreateActivityLogDto
            {
                UserId = userId,
                AdminName = adminName,
                Role = role,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                PagePath = pagePath,
                Duration = duration,
                ActionType = "VIEW",
                ActionDescription = $"管理員 {adminName} 訪問頁面: {pagePath}",
                IsSuccessful = true
            };

            return await LogActivityAsync(activityDto);
        }

        /// <summary>
        /// 記錄管理員操作
        /// </summary>
        public async Task<Guid> LogActionAsync(Guid userId, string adminName, int role, string actionType, 
            string actionDescription, string pagePath, string ipAddress, string userAgent, 
            bool isSuccessful = true, string? errorMessage = null)
        {
            var activityDto = new CreateActivityLogDto
            {
                UserId = userId,
                AdminName = adminName,
                Role = role,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                PagePath = pagePath,
                ActionType = actionType,
                ActionDescription = actionDescription,
                IsSuccessful = isSuccessful,
                ErrorMessage = errorMessage
            };

            return await LogActivityAsync(activityDto);
        }

        /// <summary>
        /// 記錄錯誤活動
        /// </summary>
        public async Task<Guid> LogErrorAsync(Guid userId, string adminName, int role, string errorMessage, 
            string pagePath, string ipAddress, string userAgent)
        {
            var activityDto = new CreateActivityLogDto
            {
                UserId = userId,
                AdminName = adminName,
                Role = role,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                PagePath = pagePath,
                ActionType = "ERROR",
                ActionDescription = $"系統錯誤: {errorMessage}",
                IsSuccessful = false,
                ErrorMessage = errorMessage
            };

            return await LogActivityAsync(activityDto);
        }

        #endregion

        #region 查詢操作

        /// <summary>
        /// 取得管理員活動記錄（分頁）
        /// </summary>
        public async Task<PagedActivityLogDto> GetActivitiesAsync(ActivityLogFilterDto filter)
        {
            try
            {
                IEnumerable<AdminActivityLog> activities;

                // 根據篩選條件查詢
                if (filter.UserId.HasValue)
                {
                    activities = await _activityRepository.GetByUserIdAsync(filter.UserId.Value, filter.Page, filter.PageSize);
                }
                else if (!string.IsNullOrEmpty(filter.ActionType))
                {
                    activities = await _activityRepository.GetByActionTypeAsync(filter.ActionType, filter.Page, filter.PageSize);
                }
                else if (!string.IsNullOrEmpty(filter.PagePath))
                {
                    activities = await _activityRepository.GetByPagePathAsync(filter.PagePath, filter.Page, filter.PageSize);
                }
                else if (!string.IsNullOrEmpty(filter.IpAddress))
                {
                    activities = await _activityRepository.GetByIpAddressAsync(filter.IpAddress, filter.Page, filter.PageSize);
                }
                else if (filter.StartDate.HasValue && filter.EndDate.HasValue)
                {
                    activities = await _activityRepository.GetByDateRangeAsync(filter.StartDate.Value, filter.EndDate.Value, filter.Page, filter.PageSize);
                }
                else if (filter.IsSuccessful.HasValue)
                {
                    activities = filter.IsSuccessful.Value 
                        ? await _activityRepository.GetSuccessfulLoginsAsync(Guid.Empty, filter.Page, filter.PageSize)
                        : await _activityRepository.GetFailedLoginsAsync(Guid.Empty, filter.Page, filter.PageSize);
                }
                else
                {
                    // 預設查詢所有記錄 (這裡需要擴展 Repository 來支援無條件分頁查詢)
                    activities = await _activityRepository.GetByDateRangeAsync(DateTime.MinValue, DateTime.MaxValue, filter.Page, filter.PageSize);
                }

                var activityDtos = _mapper.Map<List<AdminActivityLogDto>>(activities);
                
                // 計算總數（簡化實作，實際應該有專門的計數方法）
                var totalCount = activityDtos.Count;
                var totalPages = (int)Math.Ceiling((double)totalCount / filter.PageSize);

                return new PagedActivityLogDto
                {
                    Items = activityDtos,
                    TotalCount = totalCount,
                    TotalPages = totalPages,
                    CurrentPage = filter.Page,
                    PageSize = filter.PageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "查詢管理員活動記錄時發生錯誤");
                throw;
            }
        }

        /// <summary>
        /// 取得指定用戶的活動記錄
        /// </summary>
        public async Task<PagedActivityLogDto> GetUserActivitiesAsync(Guid userId, int page = 1, int pageSize = 20)
        {
            var filter = new ActivityLogFilterDto
            {
                UserId = userId,
                Page = page,
                PageSize = pageSize
            };

            return await GetActivitiesAsync(filter);
        }

        /// <summary>
        /// 取得成功的操作記錄
        /// </summary>
        public async Task<PagedActivityLogDto> GetSuccessfulActivitiesAsync(Guid? userId = null, int page = 1, int pageSize = 20)
        {
            var filter = new ActivityLogFilterDto
            {
                UserId = userId,
                IsSuccessful = true,
                Page = page,
                PageSize = pageSize
            };

            return await GetActivitiesAsync(filter);
        }

        /// <summary>
        /// 取得失敗的操作記錄
        /// </summary>
        public async Task<PagedActivityLogDto> GetFailedActivitiesAsync(Guid? userId = null, int page = 1, int pageSize = 20)
        {
            var filter = new ActivityLogFilterDto
            {
                UserId = userId,
                IsSuccessful = false,
                Page = page,
                PageSize = pageSize
            };

            return await GetActivitiesAsync(filter);
        }

        /// <summary>
        /// 取得特定操作類型的活動記錄
        /// </summary>
        public async Task<PagedActivityLogDto> GetActivitiesByTypeAsync(string actionType, int page = 1, int pageSize = 20)
        {
            var filter = new ActivityLogFilterDto
            {
                ActionType = actionType,
                Page = page,
                PageSize = pageSize
            };

            return await GetActivitiesAsync(filter);
        }

        /// <summary>
        /// 取得特定頁面的活動記錄
        /// </summary>
        public async Task<PagedActivityLogDto> GetActivitiesByPageAsync(string pagePath, int page = 1, int pageSize = 20)
        {
            var filter = new ActivityLogFilterDto
            {
                PagePath = pagePath,
                Page = page,
                PageSize = pageSize
            };

            return await GetActivitiesAsync(filter);
        }

        /// <summary>
        /// 取得特定IP地址的活動記錄
        /// </summary>
        public async Task<PagedActivityLogDto> GetActivitiesByIpAsync(string ipAddress, int page = 1, int pageSize = 20)
        {
            var filter = new ActivityLogFilterDto
            {
                IpAddress = ipAddress,
                Page = page,
                PageSize = pageSize
            };

            return await GetActivitiesAsync(filter);
        }

        /// <summary>
        /// 取得時間範圍內的活動記錄
        /// </summary>
        public async Task<PagedActivityLogDto> GetActivitiesByDateRangeAsync(DateTime startDate, DateTime endDate, 
            int page = 1, int pageSize = 20)
        {
            var filter = new ActivityLogFilterDto
            {
                StartDate = startDate,
                EndDate = endDate,
                Page = page,
                PageSize = pageSize
            };

            return await GetActivitiesAsync(filter);
        }

        #endregion

        #region 統計分析

        /// <summary>
        /// 取得活動統計資料
        /// </summary>
        public async Task<ActivityLogStatsDto> GetActivityStatsAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var stats = await _activityRepository.GetLoginStatsAsync(startDate, endDate);

                return new ActivityLogStatsDto
                {
                    TotalActivities = stats.GetValueOrDefault("TotalActivities", 0),
                    SuccessfulActivities = stats.GetValueOrDefault("SuccessfulActivities", 0),
                    FailedActivities = stats.GetValueOrDefault("FailedActivities", 0),
                    UniqueUsers = stats.GetValueOrDefault("UniqueUsers", 0)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得活動統計資料時發生錯誤");
                throw;
            }
        }

        /// <summary>
        /// 取得用戶最後成功登入記錄
        /// </summary>
        public async Task<AdminActivityLogDto?> GetLastSuccessfulLoginAsync(Guid userId)
        {
            try
            {
                var lastLogin = await _activityRepository.GetLastSuccessfulLoginAsync(userId);
                return lastLogin != null ? _mapper.Map<AdminActivityLogDto>(lastLogin) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得用戶最後成功登入記錄時發生錯誤: UserId={UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// 計算用戶失敗操作次數
        /// </summary>
        public async Task<int> CountFailedActivitiesAsync(Guid userId, DateTime since)
        {
            try
            {
                return await _activityRepository.CountFailedLoginsAsync(userId, since);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "計算用戶失敗操作次數時發生錯誤: UserId={UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// 計算IP地址失敗操作次數
        /// </summary>
        public async Task<int> CountFailedActivitiesByIpAsync(string ipAddress, DateTime since)
        {
            try
            {
                return await _activityRepository.CountFailedLoginsByIpAsync(ipAddress, since);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "計算IP地址失敗操作次數時發生錯誤: IpAddress={IpAddress}", ipAddress);
                throw;
            }
        }

        /// <summary>
        /// 取得異常活動記錄
        /// </summary>
        public async Task<PagedActivityLogDto> GetAnomalousActivitiesAsync(Guid userId, int page = 1, int pageSize = 20)
        {
            try
            {
                var anomalousActivities = await _activityRepository.GetAnomalousLoginsAsync(userId, page, pageSize);
                var activityDtos = _mapper.Map<List<AdminActivityLogDto>>(anomalousActivities);
                
                var totalCount = activityDtos.Count;
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                return new PagedActivityLogDto
                {
                    Items = activityDtos,
                    TotalCount = totalCount,
                    TotalPages = totalPages,
                    CurrentPage = page,
                    PageSize = pageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得異常活動記錄時發生錯誤: UserId={UserId}", userId);
                throw;
            }
        }

        #endregion

        #region 維護操作

        /// <summary>
        /// 清理過期的活動記錄
        /// </summary>
        public async Task<int> CleanupExpiredRecordsAsync(DateTime expiredBefore)
        {
            try
            {
                var activitiesBeforeCleanup = await _activityRepository.GetByDateRangeAsync(DateTime.MinValue, expiredBefore, 1, int.MaxValue);
                var recordCount = activitiesBeforeCleanup.Count();

                await _activityRepository.CleanupExpiredRecordsAsync(expiredBefore);
                
                _logger.LogInformation("清理過期活動記錄完成，清理了 {RecordCount} 筆記錄", recordCount);
                
                return recordCount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "清理過期活動記錄時發生錯誤");
                throw;
            }
        }

        #endregion
    }
}