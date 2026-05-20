using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using PrintFlowApi.Data;
using PrintFlowApi.DTOs;
using PrintFlowApi.Services;

namespace PrintFlowApi.Controllers;

[ApiController]
[Route("api/perfil")]
[Authorize]
public class ProfileController(PrintFlowDbContext db) : ControllerBase
{
    [HttpGet]
    [EnableRateLimiting("public-read")]
    public async Task<ActionResult<ProfileResponse>> Get(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(item => item.Id == userId, cancellationToken);
        return user is null
            ? NotFound(new { message = "Usuario nao encontrado." })
            : Ok(new ProfileResponse(user.Id, user.Name, user.Email, user.Phone, user.Document, user.Address, user.ContactPreference, user.Active, user.CreatedAt));
    }

    [HttpPut]
    [EnableRateLimiting("write")]
    public async Task<ActionResult<ProfileResponse>> Update(UpdateProfileRequest request, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var user = await db.Users.FirstOrDefaultAsync(item => item.Id == userId, cancellationToken);
        if (user is null)
        {
            return NotFound(new { message = "Usuario nao encontrado." });
        }

        user.Name = request.Name.Trim();
        user.Phone = request.Phone.Trim();
        user.Document = request.Document?.Trim();
        user.Address = request.Address?.Trim();
        user.ContactPreference = request.ContactPreference?.Trim();
        user.UpdatedAt = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        }

        await db.SaveChangesAsync(cancellationToken);
        return Ok(new ProfileResponse(user.Id, user.Name, user.Email, user.Phone, user.Document, user.Address, user.ContactPreference, user.Active, user.CreatedAt));
    }
}
