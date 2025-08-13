using AutoMapper;

namespace Matrix.Services
{
    /// <summary>
    /// NFT 服務實作
    /// </summary>
    public class NFTService : INFTService
    {
        private readonly INFTRepository _nftRepository;
        private readonly IPersonRepository _personRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<NFTService> _logger;

        public NFTService(
            INFTRepository nftRepository,
            IPersonRepository personRepository,
            IMapper mapper,
            ILogger<NFTService> logger)
        {
            _nftRepository = nftRepository;
            _personRepository = personRepository;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// 取得用戶的所有 NFT
        /// </summary>
        public async Task<IEnumerable<NFTDto>> GetUserNFTsAsync(Guid ownerId)
        {
            try
            {
                var nfts = await _nftRepository.GetByOwnerIdAsync(ownerId);
                return _mapper.Map<IEnumerable<NFTDto>>(nfts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting NFTs for owner {OwnerId}", ownerId);
                return Enumerable.Empty<NFTDto>();
            }
        }

        /// <summary>
        /// 取得用戶的所有 NFT，包含擁有者資訊
        /// </summary>
        public async Task<IEnumerable<NFTDto>> GetUserNFTsWithOwnerAsync(Guid ownerId)
        {
            try
            {
                var nfts = await _nftRepository.GetByOwnerIdWithOwnerAsync(ownerId);
                return _mapper.Map<IEnumerable<NFTDto>>(nfts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting NFTs with owner for owner {OwnerId}", ownerId);
                return Enumerable.Empty<NFTDto>();
            }
        }

        /// <summary>
        /// 根據 NFT ID 取得 NFT 詳情
        /// </summary>
        public async Task<NFTDto?> GetNFTByIdAsync(Guid nftId)
        {
            try
            {
                var nft = await _nftRepository.GetWithOwnerAsync(nftId);
                return nft != null ? _mapper.Map<NFTDto>(nft) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting NFT by ID {NftId}", nftId);
                return null;
            }
        }

        /// <summary>
        /// 創建新的 NFT
        /// </summary>
        public async Task<ReturnType<NFTDto>> CreateNFTAsync(CreateNFTDto createDto)
        {
            try
            {
                // 驗證資料
                createDto.Sanitize();
                if (!createDto.IsValid())
                {
                    return new ReturnType<NFTDto>
                    {
                        Success = false,
                        Message = "NFT 資料不完整或無效",
                        Data = default
                    };
                }

                // 檢查擁有者是否存在
                var owner = await _personRepository.GetByIdAsync(createDto.OwnerId);
                if (owner == null)
                {
                    return new ReturnType<NFTDto>
                    {
                        Success = false,
                        Message = "指定的擁有者不存在",
                        Data = default
                    };
                }

                // 創建 NFT
                var nft = _mapper.Map<NFT>(createDto);
                nft.NftId = Guid.NewGuid();

                var createdNft = await _nftRepository.AddAsync(nft);
                var nftDto = _mapper.Map<NFTDto>(createdNft);

                _logger.LogInformation("Created NFT {NftId} for owner {OwnerId}", nft.NftId, createDto.OwnerId);
                return new ReturnType<NFTDto>
                {
                    Success = true,
                    Message = "NFT 創建成功",
                    Data = nftDto
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating NFT for owner {OwnerId}", createDto.OwnerId);
                return new ReturnType<NFTDto>
                {
                    Success = false,
                    Message = "創建 NFT 時發生錯誤",
                    Data = default
                };
            }
        }

        /// <summary>
        /// 更新 NFT 資訊
        /// </summary>
        public async Task<ReturnType<bool>> UpdateNFTAsync(Guid nftId, UpdateNFTDto updateDto)
        {
            try
            {
                // 驗證資料
                updateDto.Sanitize();
                if (!updateDto.HasUpdates())
                {
                    return new ReturnType<bool>
                    {
                        Success = false,
                        Message = "沒有提供需要更新的資料",
                        Data = false
                    };
                }

                // 取得現有 NFT
                var existingNft = await _nftRepository.GetByIdAsync(nftId);
                if (existingNft == null)
                {
                    return new ReturnType<bool>
                    {
                        Success = false,
                        Message = "指定的 NFT 不存在",
                        Data = false
                    };
                }

                // 更新資料
                if (!string.IsNullOrEmpty(updateDto.FileName))
                {
                    existingNft.FileName = updateDto.FileName;
                }
                if (!string.IsNullOrEmpty(updateDto.FilePath))
                {
                    existingNft.FilePath = updateDto.FilePath;
                }
                if (updateDto.CollectTime.HasValue)
                {
                    existingNft.CollectTime = updateDto.CollectTime.Value;
                }
                if (!string.IsNullOrEmpty(updateDto.Currency))
                {
                    existingNft.Currency = updateDto.Currency;
                }
                if (updateDto.Price.HasValue)
                {
                    existingNft.Price = updateDto.Price.Value;
                }

                await _nftRepository.UpdateAsync(existingNft);

                _logger.LogInformation("Updated NFT {NftId}", nftId);
                return new ReturnType<bool>
                {
                    Success = true,
                    Message = "NFT 更新成功",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating NFT {NftId}", nftId);
                return new ReturnType<bool>
                {
                    Success = false,
                    Message = "更新 NFT 時發生錯誤",
                    Data = false
                };
            }
        }

        /// <summary>
        /// 刪除 NFT
        /// </summary>
        public async Task<ReturnType<bool>> DeleteNFTAsync(Guid nftId, Guid ownerId)
        {
            try
            {
                // 檢查 NFT 是否存在且屬於指定擁有者
                var nft = await _nftRepository.GetByIdAsync(nftId);
                if (nft == null)
                {
                    return new ReturnType<bool>
                    {
                        Success = false,
                        Message = "指定的 NFT 不存在",
                        Data = false
                    };
                }

                if (nft.OwnerId != ownerId)
                {
                    return new ReturnType<bool>
                    {
                        Success = false,
                        Message = "您沒有權限刪除此 NFT",
                        Data = false
                    };
                }

                await _nftRepository.DeleteAsync(nft);

                _logger.LogInformation("Deleted NFT {NftId} by owner {OwnerId}", nftId, ownerId);
                return new ReturnType<bool>
                {
                    Success = true,
                    Message = "NFT 刪除成功",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting NFT {NftId} by owner {OwnerId}", nftId, ownerId);
                return new ReturnType<bool>
                {
                    Success = false,
                    Message = "刪除 NFT 時發生錯誤",
                    Data = false
                };
            }
        }

        /// <summary>
        /// 根據幣別取得 NFT
        /// </summary>
        public async Task<IEnumerable<NFTDto>> GetNFTsByCurrencyAsync(Guid ownerId, string currency)
        {
            try
            {
                var nfts = await _nftRepository.GetByCurrencyAsync(ownerId, currency);
                return _mapper.Map<IEnumerable<NFTDto>>(nfts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting NFTs by currency {Currency} for owner {OwnerId}", currency, ownerId);
                return Enumerable.Empty<NFTDto>();
            }
        }

        /// <summary>
        /// 取得最近收藏的 NFT
        /// </summary>
        public async Task<IEnumerable<NFTDto>> GetRecentNFTsAsync(Guid ownerId, int count = 10)
        {
            try
            {
                var nfts = await _nftRepository.GetRecentNFTsAsync(ownerId, count);
                return _mapper.Map<IEnumerable<NFTDto>>(nfts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recent NFTs for owner {OwnerId}", ownerId);
                return Enumerable.Empty<NFTDto>();
            }
        }

        /// <summary>
        /// 根據價格範圍搜尋 NFT
        /// </summary>
        public async Task<IEnumerable<NFTDto>> GetNFTsByPriceRangeAsync(Guid ownerId, decimal minPrice, decimal maxPrice)
        {
            try
            {
                var nfts = await _nftRepository.GetByPriceRangeAsync(ownerId, minPrice, maxPrice);
                return _mapper.Map<IEnumerable<NFTDto>>(nfts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting NFTs by price range {MinPrice}-{MaxPrice} for owner {OwnerId}", minPrice, maxPrice, ownerId);
                return Enumerable.Empty<NFTDto>();
            }
        }

        /// <summary>
        /// 根據檔案名稱搜尋 NFT
        /// </summary>
        public async Task<IEnumerable<NFTDto>> SearchNFTsByFileNameAsync(Guid ownerId, string fileName)
        {
            try
            {
                var nfts = await _nftRepository.SearchByFileNameAsync(ownerId, fileName);
                return _mapper.Map<IEnumerable<NFTDto>>(nfts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching NFTs by filename {FileName} for owner {OwnerId}", fileName, ownerId);
                return Enumerable.Empty<NFTDto>();
            }
        }

        /// <summary>
        /// 根據日期範圍取得 NFT
        /// </summary>
        public async Task<IEnumerable<NFTDto>> GetNFTsByDateRangeAsync(Guid ownerId, DateTime startDate, DateTime endDate)
        {
            try
            {
                var nfts = await _nftRepository.GetByDateRangeAsync(ownerId, startDate, endDate);
                return _mapper.Map<IEnumerable<NFTDto>>(nfts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting NFTs by date range {StartDate}-{EndDate} for owner {OwnerId}", startDate, endDate, ownerId);
                return Enumerable.Empty<NFTDto>();
            }
        }

        /// <summary>
        /// 進階搜尋 NFT
        /// </summary>
        public async Task<IEnumerable<NFTDto>> SearchNFTsAsync(NFTSearchDto searchDto)
        {
            try
            {
                searchDto.Sanitize();

                var nfts = await _nftRepository.GetByOwnerIdAsync(searchDto.OwnerId);

                // 套用篩選條件
                var query = nfts.AsQueryable();

                if (!string.IsNullOrEmpty(searchDto.FileName))
                {
                    query = query.Where(n => n.FileName.Contains(searchDto.FileName, StringComparison.OrdinalIgnoreCase));
                }

                if (!string.IsNullOrEmpty(searchDto.Currency))
                {
                    query = query.Where(n => n.Currency.Equals(searchDto.Currency, StringComparison.OrdinalIgnoreCase));
                }

                if (searchDto.MinPrice.HasValue)
                {
                    query = query.Where(n => n.Price >= searchDto.MinPrice.Value);
                }

                if (searchDto.MaxPrice.HasValue)
                {
                    query = query.Where(n => n.Price <= searchDto.MaxPrice.Value);
                }

                if (searchDto.StartDate.HasValue)
                {
                    query = query.Where(n => n.CollectTime >= searchDto.StartDate.Value);
                }

                if (searchDto.EndDate.HasValue)
                {
                    query = query.Where(n => n.CollectTime <= searchDto.EndDate.Value);
                }

                // 排序
                query = searchDto.SortOrder switch
                {
                    NFTSortOrder.CollectTimeAsc => query.OrderBy(n => n.CollectTime),
                    NFTSortOrder.PriceDesc => query.OrderByDescending(n => n.Price),
                    NFTSortOrder.PriceAsc => query.OrderBy(n => n.Price),
                    NFTSortOrder.NameAsc => query.OrderBy(n => n.FileName),
                    NFTSortOrder.NameDesc => query.OrderByDescending(n => n.FileName),
                    _ => query.OrderByDescending(n => n.CollectTime)
                };

                // 分頁
                var pagedResults = query
                    .Skip((searchDto.Page - 1) * searchDto.PageSize)
                    .Take(searchDto.PageSize);

                return _mapper.Map<IEnumerable<NFTDto>>(pagedResults);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching NFTs for owner {OwnerId}", searchDto.OwnerId);
                return Enumerable.Empty<NFTDto>();
            }
        }

        /// <summary>
        /// 取得用戶 NFT 統計資料
        /// </summary>
        public async Task<NFTStatsDto> GetUserNFTStatsAsync(Guid ownerId)
        {
            try
            {
                var nfts = await _nftRepository.GetByOwnerIdAsync(ownerId);
                var nftList = nfts.ToList();

                var stats = new NFTStatsDto
                {
                    OwnerId = ownerId,
                    TotalCount = nftList.Count,
                    TotalValue = nftList.Sum(n => n.Price),
                    LastCollectTime = nftList.Any() ? nftList.Max(n => n.CollectTime) : null,
                    MostExpensive = nftList.Any() ? _mapper.Map<NFTDto>(nftList.OrderByDescending(n => n.Price).First()) : null
                };

                // 計算各幣別統計
                stats.CurrencyStats = nftList
                    .GroupBy(n => n.Currency)
                    .Select(g => new CurrencyStatsDto
                    {
                        Currency = g.Key,
                        Count = g.Count(),
                        TotalValue = g.Sum(n => n.Price)
                    })
                    .OrderByDescending(c => c.TotalValue)
                    .ToList();

                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting NFT stats for owner {OwnerId}", ownerId);
                return new NFTStatsDto { OwnerId = ownerId };
            }
        }

        /// <summary>
        /// 取得用戶 NFT 總數
        /// </summary>
        public async Task<int> GetUserNFTCountAsync(Guid ownerId)
        {
            try
            {
                return await _nftRepository.GetCountByOwnerIdAsync(ownerId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting NFT count for owner {OwnerId}", ownerId);
                return 0;
            }
        }

        /// <summary>
        /// 取得用戶 NFT 總價值
        /// </summary>
        public async Task<decimal> GetUserNFTTotalValueAsync(Guid ownerId, string? currency = null)
        {
            try
            {
                return await _nftRepository.GetTotalValueByOwnerIdAsync(ownerId, currency);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting NFT total value for owner {OwnerId}, currency {Currency}", ownerId, currency);
                return 0;
            }
        }

        /// <summary>
        /// 驗證用戶是否為 NFT 的擁有者
        /// </summary>
        public async Task<bool> IsOwnerAsync(Guid nftId, Guid ownerId)
        {
            try
            {
                var nft = await _nftRepository.GetByIdAsync(nftId);
                return nft?.OwnerId == ownerId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking ownership of NFT {NftId} by owner {OwnerId}", nftId, ownerId);
                return false;
            }
        }

        /// <summary>
        /// 批量導入 NFT
        /// </summary>
        public async Task<ReturnType<int>> BulkImportNFTsAsync(IEnumerable<CreateNFTDto> nfts)
        {
            try
            {
                var validNfts = new List<NFT>();
                var errors = new List<string>();

                foreach (var nftDto in nfts)
                {
                    nftDto.Sanitize();
                    if (!nftDto.IsValid())
                    {
                        errors.Add($"NFT {nftDto.FileName} 資料無效");
                        continue;
                    }

                    // 檢查擁有者是否存在
                    var owner = await _personRepository.GetByIdAsync(nftDto.OwnerId);
                    if (owner == null)
                    {
                        errors.Add($"NFT {nftDto.FileName} 的擁有者不存在");
                        continue;
                    }

                    var nft = _mapper.Map<NFT>(nftDto);
                    nft.NftId = Guid.NewGuid();
                    validNfts.Add(nft);
                }

                if (!validNfts.Any())
                {
                    return new ReturnType<int>
                    {
                        Success = false,
                        Message = "沒有有效的 NFT 可以導入",
                        Data = 0
                    };
                }

                // 批量新增
                foreach (var nft in validNfts)
                {
                    await _nftRepository.AddAsync(nft);
                }

                var message = $"成功導入 {validNfts.Count} 個 NFT";
                if (errors.Any())
                {
                    message += $"，{errors.Count} 個失敗";
                }

                _logger.LogInformation("Bulk imported {Count} NFTs", validNfts.Count);
                return new ReturnType<int>
                {
                    Success = true,
                    Message = message,
                    Data = validNfts.Count
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error bulk importing NFTs");
                return new ReturnType<int>
                {
                    Success = false,
                    Message = "批量導入 NFT 時發生錯誤",
                    Data = 0
                };
            }
        }

        /// <summary>
        /// 批量刪除 NFT
        /// </summary>
        public async Task<ReturnType<int>> BulkDeleteNFTsAsync(IEnumerable<Guid> nftIds, Guid ownerId)
        {
            try
            {
                var deletedCount = 0;
                var errors = new List<string>();

                foreach (var nftId in nftIds)
                {
                    var nft = await _nftRepository.GetByIdAsync(nftId);
                    if (nft == null)
                    {
                        errors.Add($"NFT {nftId} 不存在");
                        continue;
                    }

                    if (nft.OwnerId != ownerId)
                    {
                        errors.Add($"沒有權限刪除 NFT {nftId}");
                        continue;
                    }

                    await _nftRepository.DeleteAsync(nft);
                    deletedCount++;
                }

                var message = $"成功刪除 {deletedCount} 個 NFT";
                if (errors.Any())
                {
                    message += $"，{errors.Count} 個失敗";
                }

                _logger.LogInformation("Bulk deleted {Count} NFTs by owner {OwnerId}", deletedCount, ownerId);
                return new ReturnType<int>
                {
                    Success = true,
                    Message = message,
                    Data = deletedCount
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error bulk deleting NFTs by owner {OwnerId}", ownerId);
                return new ReturnType<int>
                {
                    Success = false,
                    Message = "批量刪除 NFT 時發生錯誤",
                    Data = 0
                };
            }
        }
    }
}