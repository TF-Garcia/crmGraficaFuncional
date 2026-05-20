using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using PrintFlowApi.Data;
using PrintFlowApi.DTOs;
using PrintFlowApi.Model;
using PrintFlowApi.Services;

namespace PrintFlowApi.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(PrintFlowDbContext db, JwtTokenService jwtTokenService) : ControllerBase
{
    [HttpPost("register")]
    [EnableRateLimiting("auth")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (await db.Users.AnyAsync(user => user.Email == email, cancellationToken))
        {
            return Conflict(new { message = "Email ja cadastrado." });
        }

        var user = new User
        {
            Name = request.Name.Trim(),
            Email = email,
            Phone = request.Phone.Trim(),
            Document = request.Document?.Trim(),
            Address = request.Address?.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
        };

        db.Users.Add(user);
        await db.SaveChangesAsync(cancellationToken);
        return Ok(CreateAuthResponse(user));
    }

    [HttpPost("login")]
    [EnableRateLimiting("auth")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await db.Users.FirstOrDefaultAsync(item => item.Email == email && item.Active, cancellationToken);
        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return Unauthorized(new { message = "Email ou senha invalidos." });
        }

        return Ok(CreateAuthResponse(user));
    }

    private AuthResponse CreateAuthResponse(User user)
    {
        var token = jwtTokenService.CreateToken(user);
        return new AuthResponse(user.Id, user.Name, user.Email, user.Phone, user.Role.ToString(), token.Token, token.ExpiresAt);
    }
}
