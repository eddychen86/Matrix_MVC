using Humanizer;
using Matrix.Data;
using Matrix.DTOs;
using Matrix.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Matrix.Controllers.Api
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
        public async Task<ReturnType> Update(Guid id, [FromBody] PersonDto dto)
        {

            if (id != dto.UserId)
            {
                return new ReturnType
                {
                    Ok = false,
                    Message = "3更新失敗!"
                };
            }
            Person? pe = await _context.Persons.FirstOrDefaultAsync(p=>p.UserId==id);
            if (pe == null)
            {
                return new ReturnType
                {
                    Ok = false,
                    Message = "2更新失敗!"
                };
            }

            User? pas = await _context.Users.FirstOrDefaultAsync(p => p.UserId == id);
            if (pas == null)
            {
                return new ReturnType
                {
                    Ok = false,
                    Message = "2更新失敗!"
                };
            }


            //map DTO to pe
            //更新的欄位
            pe.DisplayName = dto.DisplayName;
            pe.Bio = dto.Bio;
            pas.Password = dto.Password;
            if (dto.Email != null) pas.Email = dto.Email;
            pe.Website1 = dto.Website1;
            pe.Website2 = dto.Website2;
            pe.Website3 = dto.Website3;
            //pe.ModifyTime = dto.ModifyTime;
            //pe.AvatarPath = dto.AvatarPath;
            //pe.BannerPath = dto.BannerPath;
            //pe.ExternalUrl = dto.ExternalUrl;
            //pe.IsPrivate = dto.IsPrivate;
            //pe.WalletAddress = dto.WalletAddress;

            _context.Entry(pe).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (PersonExists(id))
                {
                    return new ReturnType
                    {
                        Ok = false,
                        Message = "1更新失敗!"
                    };
                }
                else
                {
                    throw;
                }

            }
            return new ReturnType
            {
                Ok = true,
                Message = "更新成功!"
            };

        }
        private bool PersonExists(Guid id) { 
            return _context.Persons.Any(p=>p.PersonId == id);
        }

        
        [HttpPost()]
        public async Task<ActionResult<PersonDto>> GetProfileById([FromBody] ProfileGetUserDto model) 
        {
            try
            {
                var person = await _context.Persons
                    .Include(x => x.User)
                    .Include(x => x.Articles)
                    .FirstOrDefaultAsync(x=>x.UserId == model.id);

                if (person == null)
                {
                    return NotFound();
                }



                var dto = new PersonDto
                {
                    Content = person.Articles?.Select(a => a.Content).ToList(),
                    Articles = person.Articles?.Select(a => new ArticleDto {
                        Content = a.Content,
                        CreateTime = a.CreateTime
                    }).ToList() ?? new List<ArticleDto>(),
                    Password = person.User?.Password ?? string.Empty,
                    Email = person.User?.Email,
                    PersonId = person.PersonId,
                    UserId = person.UserId,
                    DisplayName = person.DisplayName,
                    Bio = person.Bio,
                    AvatarPath = person.AvatarPath,
                    BannerPath = person.BannerPath,
                    ExternalUrl = person.ExternalUrl,
                    WalletAddress = person.WalletAddress,
                    IsPrivate = person.IsPrivate,
                    ModifyTime = person.ModifyTime,
                    Website1 = person.Website1,
                    Website2 = person.Website2,
                    Website3 = person.Website3
                };
                return Ok(dto);
            }
            catch (Exception ex)
            {

                return Ok(ex);
            }
        }
        //GET:api/ArticleApi/Getpicture/1
        //上傳圖片
        [HttpGet("GetPicture/{id}")]

        public async Task<FileResult> GetPicture(Guid id) 
        {
            string Filename = Path.Combine("wwwroot", "static", "img", "noimages.jpg");
            Article? e = await _context.Articles.FindAsync(id);
            byte[] ImageContent = e?.ArticleCover != null?
                e.ArticleCover:
                System.IO.File.ReadAllBytes(Filename);
            //e有值取  ArticleCover  沒有值就取null

            return File(ImageContent, "image/jpeg");
        }
        private bool ArticleExists(Guid id) 
        {
            return _context.Articles.Any(e => e.ArticleId == id);
        }
        


    }
}
