using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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

    }
}
