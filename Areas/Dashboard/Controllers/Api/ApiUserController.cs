using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
using Org.BouncyCastle.Bcpg;
using System;

namespace Matrix.Areas.Dashboard.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApiUserController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ApiUserController(ApplicationDbContext context)
        {
            _context = context;
        }


        //取得使用者清單
        [HttpGet]
        public IActionResult GetUsers([FromQuery] string? search, [FromQuery] int? status)
        {
            var query = _context.Users.AsQueryable();

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

            var users = query
                .OrderByDescending(u => u.CreateTime)
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
                var deleteuser = await _context.Users.FindAsync(id);
                if (deleteuser == null)
                {
                    return NotFound();
                }

                _context.Users.Remove(deleteuser);
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
