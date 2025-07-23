using Microsoft.AspNetCore.Mvc;
using Matrix.ViewModels;
using Matrix.DTOs;
using Matrix.Services.Interfaces;
using Microsoft.Extensions.Localization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Matrix.Controllers
{
  public class AuthController(IUserService _userService, IConfiguration _configuration, IStringLocalizer<AuthController> _localizer, ILogger<AuthController> _logger) : Controller
  {
    private static readonly string[] InvalidCredentialsError = ["Invalid user name or password."];

    private Dictionary<string, string[]> GetModelStateErrors()
    {
      return ModelState
          .Where(kvp => kvp.Value != null && kvp.Value.Errors.Count > 0)
          .ToDictionary(
              kvp => kvp.Key,
              kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
          );
    }

    [HttpGet]
    [Route("/register")]
    public ActionResult Register()
    {
      return View("~/Views/Auth/Register.cshtml", new RegisterViewModel());
    }

    [HttpPost]
    [Route("/api/register")]
    public async Task<IActionResult> RegisterApi([FromBody] RegisterViewModel model)
    {
      _logger.LogInformation("Register attempt with UserName: {UserName}, Email: {Email}", model.UserName, model.Email);

      if (!ModelState.IsValid)
      {
        var errors = GetModelStateErrors();
        _logger.LogWarning("Model state is invalid: {Errors}", string.Join("; ", errors.Select(e => $"{e.Key}: {string.Join(", ", e.Value)}")));
        return Json(new { success = false, errors });
      }

      // 創建 CreateUserDto
      var createUserDto = new CreateUserDto
      {
        UserName = model.UserName,
        Email = model.Email ?? "example@mail.com", // 暫時處理，實際應該要求輸入 Email
        Password = model.Password,
        PasswordConfirm = model.PasswordConfirm ?? model.Password
      };

      // --- DEBUG: Temporarily disable database write ---
      _logger.LogInformation("Debug Mode - CreateUserDto: {@CreateUserDto}", createUserDto);
      var userId = await _userService.CreateUserAsync(createUserDto);
      if (userId == null)
      {
        var errors = new Dictionary<string, string[]> { { "UserName", [_localizer["UsernameOrEmailExists"].ToString()] } };
        _logger.LogWarning("User creation failed: Username or email already exists.");
        return Json(new { success = false, errors });
      }

      // --- DEBUG: Return success for debugging ---
      return Json(new
      {
        success = true,
        redirectUrl = Url.Action("Login", "Auth"),
        message = "Debug mode: Skipped database write."
      });
    }

    [HttpGet]
    [Route("/login")]
    public ActionResult Login()
    {
      return View("~/Views/Auth/Login.cshtml", new LoginViewModel());
    }

    [HttpPost]
    [Route("/api/login")]
    public async Task<IActionResult> LoginApi([FromBody] LoginViewModel model)
    {
      _logger.LogInformation("Login attempt - UserName: {UserName}, Password Length: {PasswordLength}, ModelState Valid: {IsValid}",
        model.UserName, model.Password?.Length ?? 0, ModelState.IsValid);

      // Debug: Log all ModelState entries
      foreach (var kvp in ModelState)
      {
        _logger.LogInformation("ModelState Key: {Key}, Valid: {Valid}, Errors: {Errors}",
          kvp.Key, kvp.Value?.ValidationState,
          string.Join(", ", kvp.Value?.Errors.Select(e => e.ErrorMessage) ?? []));
      }

      if (!ModelState.IsValid)
      {
        var errors = GetModelStateErrors();
        _logger.LogWarning("Model state is invalid: {Errors}", string.Join("; ", errors.Select(e => $"{e.Key}: {string.Join(", ", e.Value)}")));
        return Json(new { success = false, errors });
      }

      // --- DEBUG: Temporarily disable validation and JWT generation ---
      var isValid = await _userService.ValidateUserAsync(model.UserName, model.Password);
      if (!isValid)
      {
        var errors = new Dictionary<string, string[]> { { "", InvalidCredentialsError } };
        _logger.LogWarning("User validation failed for UserName: {UserName}", model.UserName);
        return Json(new { success = false, errors });
      }

      var userDto = await _userService.GetUserByEmailAsync(model.UserName) ?? await _userService.GetUserByUsernameAsync(model.UserName);
      if (userDto == null)
      {
        return Json(new { success = false, errors = new { Error = "User not found." } });
      }

      // Generate JWT
      var tokenHandler = new JwtSecurityTokenHandler();
      var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured."));
      var tokenDescriptor = new SecurityTokenDescriptor
      {
        Subject = new ClaimsIdentity([
                    new(ClaimTypes.NameIdentifier, userDto.UserId.ToString()),
                    new(ClaimTypes.Name, userDto.UserName),
                    new(ClaimTypes.Role, userDto.Role.ToString())
          ]),
        Expires = DateTime.UtcNow.AddDays(30),
        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
      };
      var token = tokenHandler.CreateToken(tokenDescriptor);
      var tokenString = tokenHandler.WriteToken(token);

      // 設定 Cookie
      var cookieOptions = new CookieOptions
      {
        HttpOnly = true,
        Secure = true,
        SameSite = SameSiteMode.Strict
      };

      if (model.RememberMe)
      {
        cookieOptions.Expires = DateTime.UtcNow.AddDays(30);
      }

      Response.Cookies.Append("AuthToken", tokenString, cookieOptions);

      // --- DEBUG: Return success for debugging ---
      return Json(new { success = true, message = "Debug mode: Skipped validation and JWT generation." });
    }

    [HttpPost]
    [Route("/login/forgot")]
    public ActionResult ForgotPwd(LoginViewModel model)
    {
      if (!ModelState.IsValid)
      {
        return View("~/Views/Auth/Login.cshtml", model);
      }

      /*
      SMTP 郵件發送邏輯可以在這裡實現
      */

      // 這裡可以添加忘記密碼的邏輯，例如發送重置密碼郵件等
      // 目前僅返回登入頁面
      ModelState.AddModelError("", "Forgot password functionality is not implemented yet.");
      return View("~/Views/Auth/Login.cshtml", model);
    }
  }
}
