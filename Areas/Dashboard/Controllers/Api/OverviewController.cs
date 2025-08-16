using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Matrix.Services;
using Matrix.DTOs;
using Matrix.Data;
using System;

namespace Matrix.Areas.Dashboard.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class OverviewController(
        IUserService userService,
        IArticleService postService,
        IReportService reportService
    ) : ControllerBase
    {
        private readonly IUserService _userService = userService;
        private readonly IArticleService _postService = postService;
        private readonly IReportService _reportService = reportService;

        // [HttpGet("total-users")]
        // public async Task<IActionResult> GetTotalUsers()
        // {
        //     try
        //     {
        //         var users = await _userService.GetUsersAsync();
        //         return Ok(users.TotalCount);
        //     }
        //     catch (Exception ex)
        //     {

        //     }
        // }
    }
}