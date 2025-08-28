using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Matrix.DTOs;
using Matrix.Attributes;
using Matrix.Data;
using System;

namespace Matrix.Areas.Dashboard.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [AdminAuthorization] // 跟頁面一樣，只有管理員可用
    public class DB_UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DB_UsersController(ApplicationDbContext context)

        {
            _context = context;
        }


        //取得使用者清單
        [HttpGet]
        public IActionResult GetUsers([FromQuery] string? search, [FromQuery] int? status ,[FromQuery] DateTime? createDate)
        {
            var query = _context.Users.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(u =>
                    u.UserName.Contains(search) ||
                    u.Email.Contains(search));
            }

            if (status.HasValue)
            {
                query = query.Where(u => u.Status == status.Value);
            }
            //月曆篩選
            if (createDate.HasValue) 
            {
                var start = createDate.Value.Date;
                var end = start.AddDays(1);
                query = query.Where(u => u.CreateTime >= start && u.CreateTime < end);
            }


            var users = query
                .OrderByDescending(u => u.CreateTime)
                .Where(w => w.Role == 0 && w.IsDelete == 0)    // role 為 1、2 的使用者在 ConfigController 中管理，且只顯示未刪除的用戶
                .Select(u => new UserDto
                {
                    UserId = u.UserId,
                    Role = u.Role,
                    UserName = u.UserName,
                    Email = u.Email,
                    CreateTime = u.CreateTime,
                    LastLoginTime = u.LastLoginTime,
                    Status = u.Status,
                })
                .ToList();

            return Ok(users);
        }

        //Delete
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            try
            {
                var deleteuser = await _context.Users
                    .FirstOrDefaultAsync(u => u.UserId == id && u.IsDelete == 0);
                
                if (deleteuser == null)
                {
                    return NotFound(new { message = "找不到指定的用戶或該用戶已被刪除" });
                }

                // 軟刪除：將 IsDelete 欄位設為 1，Status 設為 2（被封禁）
                deleteuser.IsDelete = 1;
                deleteuser.Status = 2;
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"刪除失敗：{ex.Message}");
            }
        }

        //編輯
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserStatusDto dto) 
        {
            if (dto == null) 
            {
                return BadRequest(new
                {
                    Message = "查無資料"
                });
            }
            if (dto.Status < 0 || dto.Status > 2)
            {
                return BadRequest(new
                {
                    Message = "只允許0,1,2"
                });
            }
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == id);
            if (user == null) 
            {
                return NotFound();
            }
            user.Status = dto.Status;

            try
            {
                await _context.SaveChangesAsync();
                return Ok(new { message = "更新成功" });
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new { message = "更新失敗", detail = ex.InnerException?.Message ?? ex.Message });
            }

        }

    }
}
