using Humanizer;
using Matrix.Data;
using Matrix.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Matrix.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProfileApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProfileApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        //修改個人資料
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] PersonDto dto)
        {
            var entity = await _context.Persons.FindAsync(id);
            if (entity == null)
                return NotFound();

            //更新的欄位
            entity.DisplayName = dto.DisplayName;
            entity.Bio = dto.Bio;
            entity.AvatarPath = dto.AvatarPath;
            entity.BannerPath = dto.BannerPath;
            entity.ExternalUrl = dto.ExternalUrl;
            entity.IsPrivate = dto.IsPrivate;
            entity.WalletAddress = dto.WalletAddress;
            entity.ModifyTime = dto.ModifyTime;

            await _context.SaveChangesAsync();
            return NoContent();
        }


        [HttpPost()]
        public async Task<ActionResult<PersonDto>> GetProfileById([FromBody] ProfileGetUserDto model) 
        {
            try
            {
                var uuid = UUID.FromGuid(model.id);
                var person = await _context.Persons.FirstOrDefaultAsync(x=>x.UserId == uuid);
                if (person == null)
                {
                    return NotFound();
                }

                var dto = new PersonDto
                {
                    PersonId = person.PersonId,
                    UserId = person.UserId,
                    DisplayName = person.DisplayName,
                    Bio = person.Bio,
                    AvatarPath = person.AvatarPath,
                    BannerPath = person.BannerPath,
                    ExternalUrl = person.ExternalUrl,
                    WalletAddress = person.WalletAddress,
                    IsPrivate = person.IsPrivate,
                    ModifyTime = person.ModifyTime
                };
                return Ok(dto);
            }
            catch (Exception ex)
            {

                return Ok(ex);
            }
        }
    }
}
