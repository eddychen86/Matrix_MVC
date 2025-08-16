using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Matrix.Services;
using Matrix.DTOs;
using Matrix.Data;
using System;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Matrix.Areas.Dashboard.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class OverviewController(
        ILogger<OverviewController> logger,
        IUserService userService,
        ApplicationDbContext context
        // IArticleService postService,
        // IReportService reportService
    ) : ControllerBase
    {
        private readonly ILogger<OverviewController> _logger = logger;
        private readonly IUserService _userService = userService;
        private readonly ApplicationDbContext _context = context;
        // private readonly IArticleService _postService = postService;
        // private readonly IReportService _reportService = reportService;

        [HttpGet("GetUserState")]
        public async Task<IActionResult> GetTotalUsers()
        {
            var users = await _userService.GetUserBasicsAsync() ?? [];

            var result = new
            {
                totalUsers = users.Count,  // 使用 Count 屬性而不是 Count() 方法
                users
            };

            return Ok(result);
        }

        [HttpGet("GetReportsState")]
        public async Task<IActionResult> GetPendingReports()
        {
            try
            {
                // 計算待處理的報告數量 (Status == 0)
                var pendingReportsCount = await _context.Reports
                    .AsNoTracking()
                    .CountAsync(r => r.Status == 0);

                var result = new
                {
                    pendingReports = pendingReportsCount
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting reports state");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }
    }
}