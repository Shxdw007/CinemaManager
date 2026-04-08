using BCrypt.Net;
using CinemaManager.API.Contracts.Users;
using CinemaManager.API.Data;
using CinemaManager.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CinemaManager.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public sealed class UsersController : ControllerBase
{
    private readonly CinemaDbContext _db;
    private readonly IConfiguration _configuration;

    public UsersController(CinemaDbContext db, IConfiguration configuration)
    {
        _db = db;
        _configuration = configuration;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest("Email и пароль обязательны.");

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var user = await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail);

        if (user is null)
            return Unauthorized();

        var ok = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        if (!ok)
            return Unauthorized();

        var token = CreateJwt(user);

        return Ok(new LoginResponse
        {
            Token = token,
            User = new UserDto { Id = user.Id, Email = user.Email, Role = user.Role }
        });
    }

    // Dev-удобство: быстро создать тестового администратора.
    // Создаёт админа только если в БД ещё нет пользователей.
    [AllowAnonymous]
    [HttpPost("seed-admin")]
    public async Task<ActionResult<UserDto>> SeedAdmin()
    {
        var anyUsers = await _db.Users.AnyAsync();
        if (anyUsers)
            return Conflict("Пользователи уже существуют. Seed-admin отключён.");

        var email = _configuration["SeedAdmin:Email"] ?? "admin@cinemamanager.local";
        var password = _configuration["SeedAdmin:Password"] ?? "Admin123!";

        var user = new User
        {
            Email = email.Trim(),
            Role = "Admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password)
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return Ok(new UserDto { Id = user.Id, Email = user.Email, Role = user.Role });
    }

    // Swagger-friendly: создать админа с заданным email/password.
    // Анонимно доступно только пока Users пустая. После первичной инициализации — только для роли Admin.
    [AllowAnonymous]
    [HttpPost("create-admin")]
    public async Task<ActionResult<UserDto>> CreateAdmin([FromBody] CreateAdminRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest("Email и пароль обязательны.");

        var anyUsers = await _db.Users.AnyAsync();
        if (anyUsers)
        {
            // если уже есть пользователи — требуем токен Admin
            if (!(User?.Identity?.IsAuthenticated ?? false) || !User.IsInRole("Admin"))
                return Forbid();
        }

        var email = request.Email.Trim();
        var normalizedEmail = email.ToLowerInvariant();

        var exists = await _db.Users.AnyAsync(u => u.Email.ToLower() == normalizedEmail);
        if (exists) return Conflict("Пользователь с таким email уже существует.");

        var user = new User
        {
            Email = email,
            Role = "Admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return Ok(new UserDto { Id = user.Id, Email = user.Email, Role = user.Role });
    }

    private string CreateJwt(User user)
    {
        var issuer = _configuration["Jwt:Issuer"]!;
        var audience = _configuration["Jwt:Audience"]!;
        var key = _configuration["Jwt:Key"]!;

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role)
        };

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var jwt = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(jwt);
    }
}

